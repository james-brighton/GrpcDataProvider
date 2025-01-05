using System.Net;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace JamesBrighton.Data.GrpcClient.Common;

/// <summary>
/// Manages the creation and disposal of gRPC channels.
/// </summary>
public class ChannelManager : IDisposable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelManager"/> class with the specified address.
	/// </summary>
	/// <param name="address">The address of the gRPC channel.</param>
	public ChannelManager(string address)
	{
		Channel = GetChannel(address);
	}

	/// <summary>
	/// Disposes the <see cref="ChannelManager"/> instance and its associated gRPC channel.
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Gets the gRPC channel managed by this instance.
	/// </summary>
	public GrpcChannel Channel { get; private set; }

	/// <summary>
	/// Gets or creates a gRPC channel for the specified address.
	/// </summary>
	/// <param name="address">The address of the gRPC channel.</param>
	/// <returns>The gRPC channel associated with the specified address.</returns>
	static GrpcChannel GetChannel(string address)
	{
		lock (lockObject)
		{
			if (dictionary.TryGetValue(address, out var v))
			{
				dictionary[address] = (v.Item1, v.Item2 + 1);
				return v.Item1;
			}

			var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
			var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
			{
				HttpClient = httpClient,
				HttpVersion = HttpVersion.Version11
			});
			dictionary[address] = (channel, 1);
			return channel;
		}
	}

	/// <summary>
	/// Disposes the specified gRPC channel and removes it from the internal dictionary if it exists.
	/// </summary>
	/// <param name="channel">The gRPC channel to dispose.</param>
	static void Dispose(GrpcChannel channel)
	{
		lock (lockObject)
		{
			foreach (var item in dictionary)
			{
				if (item.Value.Item1 != channel)
					continue;
				var count = item.Value.Item2 - 1;
				if (count > 0)
				{
					dictionary[item.Key] = (channel, count);
				}
				else
				{
					channel.Dispose();
					dictionary.Remove(item.Key);
				}
				return;
			}
		}
	}

	/// <summary>
	/// Releases the unmanaged resources used by the ChannelManager and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue) return;
		if (disposing)
		{
			Dispose(Channel);
		}

		disposedValue = true;
	}

	/// <summary>
	/// The disposed value.
	/// </summary>
	bool disposedValue;
	/// <summary>
	/// The dictionary with the channels and references.
	/// </summary>
	static readonly Dictionary<string, (GrpcChannel, int)> dictionary = [];
	/// <summary>
	/// The lock object.
	/// </summary>
	static readonly object lockObject = new();
}
