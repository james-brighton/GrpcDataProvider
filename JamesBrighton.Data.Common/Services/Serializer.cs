using System.Reflection;
using System.Text;
using System.Text.Json;
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
	static Dictionary<Type, MemberDelegate?>? DeserializeMethods;

	/// <summary>
	/// A dictionary that contains serialize methods for different types.
	/// </summary>
	[ThreadStatic]
	static Dictionary<Type, MemberDelegate?>? SerializeMethods;

	/// <summary>
	/// Serializes the specified object to a stream using ProtoBuf serializer.
	/// </summary>
	/// <param name="destination">The stream to write the serialized object to.</param>
	/// <param name="content">The object to be serialized.</param>
	/// <returns>True on success and false otherwise.</returns>
	/// <remarks>
	/// This method uses the ProtoBuf.Serializer to perform the serialization.
	/// </remarks>
	public static bool Serialize(Stream destination, object content)
	{
		var type = content.GetType();

        SerializeMethods ??= new Dictionary<Type, MemberDelegate?>();
		if (SerializeMethods.TryGetValue(type, out var f))
		{
			if (f == null)
				return false;
			f(destination, content);
			return true;
		}

		var serializeT = typeof(Serializer).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
			x is { Name: nameof(Serializer.Serialize), IsGenericMethodDefinition: true } &&
			x.GetParameters().Length == 2 &&
			x.GetParameters()[0].ParameterType == typeof(Stream));
		if (serializeT == null) return false;
		var serialize = serializeT.MakeGenericMethod(type);

		var createAction = typeof(Serializer2)
			.GetMethod(nameof(CreateActionDelegateP2), BindingFlags.Static | BindingFlags.NonPublic)
			?.MakeGenericMethod(typeof(Stream), type);

		if (createAction == null)
			return false;

		var result = createAction.Invoke(null, new object[] { serialize });
		if (result == null)
			return false;
		var del = (MemberDelegate)result;
		try
		{
			del(destination, content);
			SerializeMethods.Add(type, del);
			return true;
		}
		catch (InvalidOperationException)
		{
			if (FallbackSerialize(destination, content))
				return true;
			SerializeMethods.Add(type, null);
			return false;
		}
	}

	/// <summary>
	/// Serializes the specified object to a stream using JSON serializer.
	/// </summary>
	/// <param name="destination">The stream to write the serialized object to.</param>
	/// <param name="content">The object to be serialized.</param>
	/// <returns>True on success and false otherwise.</returns>
    static bool FallbackSerialize(Stream destination, object content)
    {
		var pos1 = destination.Position;
		try
		{
			JsonSerializer.Serialize(destination, content, content.GetType());
		}
		catch (NotSupportedException)
		{
			return false;
		}
		var pos2 = destination.Position;
		var result = pos2 > pos1;
		destination.Position = pos1;
		return result;
    }

    /// <summary>
    /// Tries to deserialize an object from the specified stream using ProtoBuf serializer.
    /// </summary>
    /// <param name="type">The type of the object to be deserialized.</param>
    /// <param name="source">The stream containing the serialized object.</param>
    /// <param name="value">[out] When this method returns, contains the deserialized value, if it could be deserialized; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>True of deserialized successfully and false otherwise.</returns>
    /// <remarks>
    /// This method uses the ProtoBuf.Serializer to perform the deserialization.
    /// </remarks>
    public static bool TryDeserialize(Type type, Stream source, out object? value)
	{
        DeserializeMethods ??= new Dictionary<Type, MemberDelegate?>();
		if (DeserializeMethods.TryGetValue(type, out var f))
		{
			if (f == null)
			{
				value = null;
				return false;
			}
			value = f(source);
			return true;
		}
		var deserializeT = typeof(Serializer).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
			x is { Name: nameof(Serializer.Deserialize), IsGenericMethodDefinition: true } &&
			x.GetParameters().Length == 1 &&
			x.GetParameters()[0].ParameterType == typeof(Stream));
		if (deserializeT == null)
		{
			value = null;
			return false;
		}
		var deserialize = deserializeT.MakeGenericMethod(type);

		var createFunc = typeof(Serializer2)
			.GetMethod(nameof(CreateFuncDelegateP1), BindingFlags.Static | BindingFlags.NonPublic)
			?.MakeGenericMethod(typeof(Stream), type);

		if (createFunc == null)
		{
			value = null;
			return false;
		}

		var func = createFunc.Invoke(null, new object[] { deserialize });
		if (func == null)
		{
			value = null;
			return false;
		}
		var del = (MemberDelegate)func;

		try
		{
			var result = del(source);
			DeserializeMethods.Add(type, del);
			value = result;
			return true;
		}
		catch (InvalidOperationException)
		{
			source.Position = 0;
			if (TryFallbackDeserialize(type, source, out value))
				return true;
			DeserializeMethods.Add(type, null);
			value = null;
			return false;
		}
	}

    /// <summary>
    /// Tries to deserialize an object from the specified stream using JSON serializer.
    /// </summary>
    /// <param name="type">The type of the object to be deserialized.</param>
    /// <param name="source">The stream containing the serialized object.</param>
    /// <param name="value">[out] When this method returns, contains the deserialized value, if it could be deserialized; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>True of deserialized successfully and false otherwise.</returns>
    static bool TryFallbackDeserialize(Type type, Stream source, out object? value)
    {
		var pos1 = source.Position;
		try
		{
        	value = JsonSerializer.Deserialize(source, type);
		}
		catch (NotSupportedException)
		{
			value = null;
			return false;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
		var pos2 = source.Position;
		var result = pos2 > pos1;
		return result;
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