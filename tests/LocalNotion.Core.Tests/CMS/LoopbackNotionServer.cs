// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Notion.Client;

namespace LocalNotion.Core.Tests;

/// <summary>
/// Captures actual SDK HTTP requests on loopback using synthetic credentials only.
/// </summary>
public sealed class LoopbackNotionServer : IAsyncDisposable {
	private readonly TcpListener _listener;
	private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
	private readonly List<Task> _connections = new List<Task>();
	private readonly Task _acceptLoop;
	private readonly RecordingNotionTransport _state;

	public LoopbackNotionServer(RecordingNotionTransport state) {
		_state = state;
		_listener = new TcpListener(IPAddress.Loopback, 0);
		_listener.Start();
		BaseUrl = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/";
		_acceptLoop = Task.Run(AcceptAsync);
	}

	public string BaseUrl { get; }
	public string NotionVersion { get; private set; }

	public async ValueTask DisposeAsync() {
		_shutdown.Cancel();
		_listener.Stop();
		await _acceptLoop;
		await Task.WhenAll(_connections);
		_shutdown.Dispose();
	}

	private async Task AcceptAsync() {
		try {
			while (!_shutdown.IsCancellationRequested) {
				var connection = await _listener.AcceptTcpClientAsync(_shutdown.Token);
				_connections.Add(HandleAsync(connection));
			}
		} catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) {
		} catch (SocketException) when (_shutdown.IsCancellationRequested) {
		}
	}

	private async Task HandleAsync(TcpClient connection) {
		using var client = connection;
		try {
			var stream = client.GetStream();
			var bytes = new List<byte>();
			var single = new byte[1];
			while (bytes.Count < 65536) {
				await stream.ReadExactlyAsync(single, _shutdown.Token);
				bytes.Add(single[0]);
				if (bytes.Count >= 4 && bytes.TakeLast(4).SequenceEqual(new byte[] { 13, 10, 13, 10 }))
					break;
			}
			var lines = Encoding.ASCII.GetString(bytes.ToArray()).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
			var request = lines[0].Split(' ');
			var headers = lines.Skip(1).Select(x => x.Split(':', 2)).ToDictionary(x => x[0], x => x[1].Trim(), StringComparer.OrdinalIgnoreCase);
			NotionVersion = headers["Notion-Version"];
			var length = headers.TryGetValue("Content-Length", out var size) ? int.Parse(size, CultureInfo.InvariantCulture) : 0;
			var bodyBytes = new byte[length];
			await stream.ReadExactlyAsync(bodyBytes, _shutdown.Token);
			var body = length == 0 ? null : JObject.Parse(Encoding.UTF8.GetString(bodyBytes));
			var status = 200;
			object response;
			try {
				response = await _state.HandleAsync(request[0], request[1], body, _shutdown.Token);
			} catch (NotionApiException error) {
				status = (int)error.StatusCode;
				response = new { @object = "error", status, code = "validation_error", message = error.Message };
			}
			var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings {
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
			});
			var responseBytes = Encoding.UTF8.GetBytes(json);
			var responseHeaders = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Test\r\nContent-Type: application/json\r\nContent-Length: {responseBytes.Length}\r\nConnection: close\r\n\r\n");
			await stream.WriteAsync(responseHeaders, _shutdown.Token);
			await stream.WriteAsync(responseBytes, _shutdown.Token);
		} catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) {
		} catch (IOException) when (_shutdown.IsCancellationRequested) {
		}
	}
}
