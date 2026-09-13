// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[Platform("Win")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Children)]
public class NativeLauncherTests {

	private static string _buildRoot;

	private string _fixtureRoot;

	private string _launcher;

	private string _probe;

	[OneTimeSetUp]
	public static async Task CompileExecutables() {
		_buildRoot = Path.Combine(Path.GetTempPath(), "localnotion-launcher-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_buildRoot);
		var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		var compiler = Path.Combine(windows, "Microsoft.NET", "Framework64", "v4.0.30319", "csc.exe");
		if (!File.Exists(compiler))
			compiler = Path.Combine(windows, "Microsoft.NET", "Framework", "v4.0.30319", "csc.exe");
		Assert.That(File.Exists(compiler), Is.True, "Windows launcher tests use the built-in .NET Framework compiler.");
		await Compile(Path.Combine(AppContext.BaseDirectory, "Launcher", "LocalNotionDockerLauncher.cs"), "localnotion.exe");
		var probeSource = Path.Combine(_buildRoot, "Probe.cs");
		File.WriteAllText(probeSource, """
			using System;
			using System.IO;
			using System.Runtime.Serialization.Json;
			using System.Text;
			internal static class Probe {
				public static int Main(string[] args) {
					if (args.Length > 0 && args[0] == "--probe-exit")
						return int.Parse(args[1]);
					if (args.Length > 0 && args[0] == "--probe-streams") {
						var bytes = new UTF8Encoding(false).GetBytes("Snow 雪 / 😀 / café\r\n");
						using (var output = Console.OpenStandardOutput()) output.Write(bytes, 0, bytes.Length);
						using (var error = Console.OpenStandardError()) error.Write(bytes, 0, bytes.Length);
						return 0;
					}
					var values = new string[args.Length + 2];
					values[0] = Environment.CurrentDirectory;
					values[1] = Environment.GetEnvironmentVariable("NOTION_API_KEY_FILE");
					Array.Copy(args, 0, values, 2, args.Length);
					using (var output = Console.OpenStandardOutput())
						new DataContractJsonSerializer(typeof(string[])).WriteObject(output, values);
					return 0;
				}
			}
			""", new UTF8Encoding(false));
		await Compile(probeSource, "probe.exe");

		async Task Compile(string source, string output) {
			var result = await Run(compiler, ["/nologo", "/codepage:65001", "/target:exe", "/r:System.Runtime.Serialization.dll", "/out:" + Path.Combine(_buildRoot, output), source], _buildRoot);
			Assert.That(result.ExitCode, Is.Zero, Encoding.UTF8.GetString(result.Output) + Encoding.UTF8.GetString(result.Error));
		}
	}

	[OneTimeTearDown]
	public static void RemoveBuildFiles() {
		if (Directory.Exists(_buildRoot)) {
			Assert.That(Path.GetDirectoryName(Path.GetFullPath(_buildRoot)), Is.EqualTo(Path.TrimEndingDirectorySeparator(Path.GetTempPath())));
			Assert.That(Path.GetFileName(_buildRoot), Does.StartWith("localnotion-launcher-"));
			Directory.Delete(_buildRoot, recursive: true);
		}
	}

	[SetUp]
	public void SetUp() {
		_fixtureRoot = Path.Combine(_buildRoot, Guid.NewGuid().ToString("N") + " space 雪");
		Directory.CreateDirectory(_fixtureRoot);
		_launcher = Path.Combine(_fixtureRoot, "localnotion.exe");
		_probe = Path.Combine(_fixtureRoot, "native probe.exe");
		File.Copy(Path.Combine(_buildRoot, "localnotion.exe"), _launcher);
		File.Copy(Path.Combine(_buildRoot, "probe.exe"), _probe);
		Configure(_probe);
	}

	[TestCase("")]
	[TestCase("two words")]
	[TestCase("embedded\"quote")]
	[TestCase("C:\\folder with spaces\\")]
	[TestCase("\\\\before\"after\\\\")]
	[TestCase("雪 😀 café")]
	[TestCase("&|<>^%!$();`{contents}")]
	[TestCase("line\r\nnext\tcolumn")]
	public async Task NativeBackendPreservesArgumentsAndWorkingDirectory(string argument) {
		var arguments = new[] { "--path", argument, "", "final" };
		var result = await Run(_launcher, arguments, _fixtureRoot);
		Assert.That(result.ExitCode, Is.Zero, Encoding.UTF8.GetString(result.Error));
		var values = JsonSerializer.Deserialize<string[]>(result.Output);
		Assert.That(values[0], Is.EqualTo(_fixtureRoot));
		Assert.That(values.Skip(2), Is.EqualTo(arguments));
	}

	[TestCase(0)]
	[TestCase(37)]
	[TestCase(-2)]
	public async Task NativeBackendReturnsExactExitCode(int exitCode) {
		var result = await Run(_launcher, ["--probe-exit", exitCode.ToString()], _fixtureRoot);
		Assert.That(result.ExitCode, Is.EqualTo(exitCode));
	}

	[Test]
	public async Task NativeBackendPreservesUtf8StandardStreams() {
		var result = await Run(_launcher, ["--probe-streams"], _fixtureRoot);
		var expected = new UTF8Encoding(false).GetBytes("Snow 雪 / 😀 / café\r\n");
		Assert.That(result.ExitCode, Is.Zero);
		Assert.That(result.Output, Is.EqualTo(expected));
		Assert.That(result.Error, Is.EqualTo(expected));
	}

	[TestCase("relative")]
	[TestCase("drive-relative")]
	[TestCase("root-relative")]
	[TestCase("missing")]
	[TestCase("directory")]
	[TestCase("self")]
	public async Task NativeBackendRejectsInvalidTarget(string type) {
		var target = type switch {
			"relative" => Path.GetFileName(_probe),
			"drive-relative" => Path.GetPathRoot(_probe).TrimEnd('\\') + Path.GetFileName(_probe),
			"root-relative" => Path.DirectorySeparatorChar + Path.GetRelativePath(Path.GetPathRoot(_probe), _probe),
			"missing" => Path.Combine(_fixtureRoot, "missing.exe"),
			"directory" => _fixtureRoot,
			"self" => _launcher,
			_ => throw new ArgumentOutOfRangeException(nameof(type))
		};
		Configure(target);
		var result = await Run(_launcher, [], _fixtureRoot);
		Assert.That(result.ExitCode, Is.EqualTo(1));
		Assert.That(Encoding.UTF8.GetString(result.Error), Does.Contain("native executable").IgnoreCase.Or.Contain("nativeExecutable"));
	}

	[TestCase("docker", true)]
	[TestCase(null, false)]
	public async Task DockerBackendRemainsAvailable(string backend, bool configuredNative) {
		Configure(configuredNative ? _probe : null);
		File.WriteAllText(Path.Combine(_fixtureRoot, "localnotion-docker.ps1"), "[Console]::Write('docker-mode'); exit 43", new UTF8Encoding(false));
		var result = await Run(_launcher, [], _fixtureRoot, new() { ["LOCALNOTION_BACKEND"] = backend });
		Assert.That(result.ExitCode, Is.EqualTo(43));
		Assert.That(Encoding.UTF8.GetString(result.Output), Is.EqualTo("docker-mode"));
	}

	[TestCase("invalid")]
	[TestCase("native")]
	public async Task InvalidBackendOrMissingNativeConfigurationFailsClearly(string backend) {
		Configure(null);
		var result = await Run(_launcher, [], _fixtureRoot, new() { ["LOCALNOTION_BACKEND"] = backend });
		Assert.That(result.ExitCode, Is.EqualTo(1));
		Assert.That(result.Error, Is.Not.Empty);
	}

	[TestCase(null, "relative token file")]
	[TestCase("explicit token file", "relative token file")]
	public async Task NativeBackendPreservesTokenFilePrecedence(string nativeTokenFile, string legacyTokenFile) {
		var result = await Run(_launcher, [], _fixtureRoot, new() { ["NOTION_API_KEY_FILE"] = nativeTokenFile, ["LOCALNOTION_TOKEN_FILE"] = legacyTokenFile });
		Assert.That(result.ExitCode, Is.Zero);
		var values = JsonSerializer.Deserialize<string[]>(result.Output);
		Assert.That(values[1], Is.EqualTo(nativeTokenFile ?? legacyTokenFile));
	}

	[Test]
	public async Task NativeInstallerWorksWithoutDockerAndPreservesConfiguredBackend() {
		var installerDirectory = Path.Combine(_fixtureRoot, "checkout", "docker");
		Directory.CreateDirectory(installerDirectory);
		foreach (var file in new[] { "install-cli.ps1", "LocalNotionDockerLauncher.cs", "localnotion-docker.ps1" })
			File.Copy(Path.Combine(AppContext.BaseDirectory, "Launcher", file), Path.Combine(installerDirectory, file));
		var installed = Path.Combine(_fixtureRoot, "installed");
		var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
		var installer = Path.Combine(installerDirectory, "install-cli.ps1");
		var environment = new Dictionary<string, string> { ["PATH"] = string.Empty };
		var result = await Run(powershell, ["-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", installer, "-InstallDirectory", installed, "-NativeExecutable", _probe, "-NoPath"], _fixtureRoot, environment);
		Assert.That(result.ExitCode, Is.Zero, Encoding.UTF8.GetString(result.Error));
		var configurationPath = Path.Combine(installed, "localnotion-docker.json");
		using (var configuration = JsonDocument.Parse(File.ReadAllText(configurationPath))) {
			Assert.That(configuration.RootElement.GetProperty("nativeExecutable").GetString(), Is.EqualTo(_probe));
			Assert.That(configuration.RootElement.GetProperty("image").GetString(), Is.EqualTo("local-notion:latest"));
		}
		result = await Run(powershell, ["-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", installer, "-InstallDirectory", installed, "-NoPath"], _fixtureRoot, environment);
		Assert.That(result.ExitCode, Is.Zero, Encoding.UTF8.GetString(result.Error));
		result = await Run(Path.Combine(installed, "localnotion.exe"), ["--probe-exit", "37"], _fixtureRoot);
		Assert.That(result.ExitCode, Is.EqualTo(37));
	}

	private void Configure(string nativeExecutable) => File.WriteAllText(Path.Combine(_fixtureRoot, "localnotion-docker.json"), JsonSerializer.Serialize(new { nativeExecutable }), new UTF8Encoding(false));

	private static async Task<(int ExitCode, byte[] Output, byte[] Error)> Run(string executable, string[] arguments, string directory, Dictionary<string, string> environment = null) {
		var start = new ProcessStartInfo(executable) {
			WorkingDirectory = directory,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};
		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);
		foreach (var key in new[] { "LOCALNOTION_BACKEND", "LOCALNOTION_TOKEN_FILE", "NOTION_API_KEY_FILE", "LOCALNOTION_DOCKER_ARGS" })
			start.Environment.Remove(key);
		if (environment is not null)
			foreach (var pair in environment)
				start.Environment[pair.Key] = pair.Value;
		using var process = Process.Start(start);
		using var output = new MemoryStream();
		using var error = new MemoryStream();
		var outputTask = process.StandardOutput.BaseStream.CopyToAsync(output);
		var errorTask = process.StandardError.BaseStream.CopyToAsync(error);
		await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask).WaitAsync(TimeSpan.FromSeconds(30));
		return (process.ExitCode, output.ToArray(), error.ToArray());
	}
}
