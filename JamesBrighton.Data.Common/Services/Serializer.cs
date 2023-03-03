using System.Reflection;
using ProtoBuf;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Provides protocol-buffer serialization capability for concrete, attributed types.
/// </summary>
public static class Serializer2
{
	/// <summary>
	/// A dictionary that contains deserialize methods for different types.
	/// </summary>
	[ThreadStatic]
	static Dictionary<Type, MemberDelegate>? DeserializeMethods;

	/// <summary>
	/// A dictionary that contains serialize methods for different types.
	/// </summary>
	[ThreadStatic]
	static Dictionary<Type, MemberDelegate>? SerializeMethods;

	/// <summary>
	/// Serializes the specified object to a stream using ProtoBuf serializer.
	/// </summary>
	/// <param name="destination">The stream to write the serialized object to.</param>
	/// <param name="content">The object to be serialized.</param>
	/// <remarks>
	/// This method uses the ProtoBuf.Serializer to perform the serialization.
	/// </remarks>
	public static void Serialize(Stream destination, object content)
	{
		var type = content.GetType();

        if (SerializeMethods == null)
            SerializeMethods = new Dictionary<Type, MemberDelegate>();
		if (SerializeMethods.TryGetValue(type, out var f))
		{
			f(destination, content);
			return;
		}

		var serializeT = typeof(Serializer).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
			x is { Name: nameof(Serializer.Serialize), IsGenericMethodDefinition: true } &&
			x.GetParameters().Length == 2 &&
			x.GetParameters()[0].ParameterType == typeof(Stream));
		if (serializeT == null) return;
		var serialize = serializeT.MakeGenericMethod(type);

		var createAction = typeof(Serializer2)
			.GetMethod(nameof(CreateActionDelegateP2), BindingFlags.Static | BindingFlags.NonPublic)
			?.MakeGenericMethod(typeof(Stream), type);

		if (createAction == null)
			return;

		var result = createAction.Invoke(null, new object[] { serialize });
		if (result == null)
			return;
		var del = (MemberDelegate)result;
		SerializeMethods.Add(type, del);
		del(destination, content);
	}

	/// <summary>
	/// Deserializes an object from the specified stream using ProtoBuf serializer.
	/// </summary>
	/// <param name="type">The type of the object to be deserialized.</param>
	/// <param name="source">The stream containing the serialized object.</param>
	/// <returns>The deserialized object.</returns>
	/// <remarks>
	/// This method uses the ProtoBuf.Serializer to perform the deserialization.
	/// </remarks>
	public static object? Deserialize(Type type, Stream source)
	{
        if (DeserializeMethods == null)
            DeserializeMethods = new Dictionary<Type, MemberDelegate>();
		if (DeserializeMethods.TryGetValue(type, out var f))
			return f(source);
		var deserializeT = typeof(Serializer).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
			x is { Name: nameof(Serializer.Deserialize), IsGenericMethodDefinition: true } &&
			x.GetParameters().Length == 1 &&
			x.GetParameters()[0].ParameterType == typeof(Stream));
		if (deserializeT == null) return null;
		var deserialize = deserializeT.MakeGenericMethod(type);

		var createFunc = typeof(Serializer2)
			.GetMethod(nameof(CreateFuncDelegateP1), BindingFlags.Static | BindingFlags.NonPublic)
			?.MakeGenericMethod(typeof(Stream), type);

		if (createFunc == null)
			return null;

		var result = createFunc.Invoke(null, new object[] { deserialize });
		if (result == null)
			return null;
		var del = (MemberDelegate)result;
		DeserializeMethods.Add(type, del);
		return del(source);
	}

	/// <summary>
	/// Creates a delegate for a function with one parameter and a result.
	/// </summary>
	/// <typeparam name="TP1">The type of the first parameter of the function.</typeparam>
	/// <typeparam name="TResult">The type of the result of the function.</typeparam>
	/// <param name="m">The MethodInfo representing the function.</param>
	/// <returns>A delegate for the specified function.</returns>
	static MemberDelegate CreateFuncDelegateP1<TP1, TResult>(MethodInfo m)
	{
#nullable disable
		return p => ((Func<TP1, TResult>)Delegate.CreateDelegate(typeof(Func<TP1, TResult>), m))((TP1)p[0]);
#nullable enable
	}

	/// <summary>
	/// Creates a delegate for an action with two parameters.
	/// </summary>
	/// <typeparam name="TP1">The type of the first parameter of the action.</typeparam>
	/// <typeparam name="TP2">The type of the second parameter of the action.</typeparam>
	/// <param name="m">The MethodInfo representing the action.</param>
	/// <returns>A delegate for the specified action.</returns>
	static MemberDelegate CreateActionDelegateP2<TP1, TP2>(MethodInfo m)
	{
#nullable disable
		return p =>
		{
			((Action<TP1, TP2>)Delegate.CreateDelegate(typeof(Action<TP1, TP2>), m))((TP1)p![0], (TP2)p[1]);
			return null;
		};
#nullable enable
	}

	/// <summary>
	/// This delegate represents a member delegate.
	/// </summary>
	/// <param name="args">The arguments of the delegate.</param>
	/// <returns>The method result.</returns>
	delegate object? MemberDelegate(params object?[]? args);
}