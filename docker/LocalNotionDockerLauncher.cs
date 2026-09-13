// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

internal static class LocalNotionDockerLauncher {

	public static int Main(string[] args) {
		try {
			var configuration = ReadConfiguration();
			var backend = Environment.GetEnvironmentVariable("LOCALNOTION_BACKEND");
			if (!string.IsNullOrWhiteSpace(backend) && !backend.Equals("native", StringComparison.OrdinalIgnoreCase) && !backend.Equals("docker", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("LOCALNOTION_BACKEND must be native or docker.");
			var useNative = string.Equals(backend, "native", StringComparison.OrdinalIgnoreCase)
				|| string.IsNullOrWhiteSpace(backend) && !string.IsNullOrWhiteSpace(configuration.NativeExecutable);
			var start = useNative ? StartNative(configuration.NativeExecutable, args) : StartDocker(args);
			// Ctrl+C reaches the child too. Let the application or Docker script finish its cleanup.
			Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs) { eventArgs.Cancel = true; };
			using (var process = Process.Start(start)) {
				process.WaitForExit();
				return process.ExitCode;
			}
		} catch (Exception error) {
			Console.Error.WriteLine("Local Notion launcher: " + error.Message);
			return 1;
		}
	}

	private static LauncherConfiguration ReadConfiguration() {
		var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localnotion-docker.json");
		if (!File.Exists(path))
			return new LauncherConfiguration();
		using (var stream = File.OpenRead(path))
			return (LauncherConfiguration)new DataContractJsonSerializer(typeof(LauncherConfiguration)).ReadObject(stream) ?? new LauncherConfiguration();
	}

	private static ProcessStartInfo StartNative(string executable, string[] args) {
		if (string.IsNullOrWhiteSpace(executable))
			throw new InvalidOperationException("Configure nativeExecutable before selecting the native backend.");
		var fullPath = Path.GetFullPath(executable);
		if (!Path.IsPathRooted(executable) || !string.Equals(Path.GetPathRoot(executable), Path.GetPathRoot(fullPath), StringComparison.OrdinalIgnoreCase))
			throw new InvalidOperationException("nativeExecutable must be an absolute executable path.");
		using (var current = Process.GetCurrentProcess())
			if (string.Equals(fullPath, Path.GetFullPath(current.MainModule.FileName), StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("nativeExecutable cannot refer to this command launcher.");
		if (!File.Exists(fullPath))
			throw new FileNotFoundException("The configured native executable does not exist.", fullPath);
		var start = new ProcessStartInfo {
			FileName = fullPath,
			Arguments = string.Join(" ", Array.ConvertAll(args, Quote)),
			WorkingDirectory = Environment.CurrentDirectory,
			UseShellExecute = false
		};
		var tokenFile = Environment.GetEnvironmentVariable("LOCALNOTION_TOKEN_FILE");
		if (!string.IsNullOrWhiteSpace(tokenFile) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("NOTION_API_KEY_FILE")))
			start.EnvironmentVariables["NOTION_API_KEY_FILE"] = tokenFile;
		return start;
	}

	private static ProcessStartInfo StartDocker(string[] args) {
		var script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localnotion-docker.ps1");
		if (!File.Exists(script))
			throw new FileNotFoundException("Local Notion Docker launcher is incomplete. Run docker/install-cli.ps1 again.", script);
		var start = new ProcessStartInfo {
			FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"System32\WindowsPowerShell\v1.0\powershell.exe"),
			Arguments = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File " + Quote(script),
			WorkingDirectory = Environment.CurrentDirectory,
			UseShellExecute = false
		};
		using (var stream = new MemoryStream()) {
			new DataContractJsonSerializer(typeof(string[])).WriteObject(stream, args);
			start.EnvironmentVariables["LOCALNOTION_DOCKER_ARGS"] = Convert.ToBase64String(stream.ToArray());
		}
		return start;
	}

	private static string Quote(string value) {
		var output = new StringBuilder("\"");
		var slashes = 0;
		foreach (var character in value) {
			if (character == '\\') {
				slashes++;
				continue;
			}
			if (character == '"') {
				output.Append('\\', slashes * 2 + 1);
				output.Append(character);
			} else {
				output.Append('\\', slashes);
				output.Append(character);
			}
			slashes = 0;
		}
		output.Append('\\', slashes * 2);
		output.Append('"');
		return output.ToString();
	}

	[DataContract]
	private sealed class LauncherConfiguration {

		[DataMember(Name = "nativeExecutable")]
		public string NativeExecutable { get; set; }
	}
}
