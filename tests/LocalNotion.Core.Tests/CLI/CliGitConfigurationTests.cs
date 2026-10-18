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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using LocalNotion.CLI;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class CliGitConfigurationTests {
	private string _suiteRoot;
	private string _themesPath;
	private string _fixtureRoot;
	private string _registryPath;

	[OneTimeSetUp]
	public void OneTimeSetUp() {
		_suiteRoot = Path.Combine(Path.GetTempPath(), "localnotion-git-config-" + Guid.NewGuid().ToString("N"));
		_themesPath = Path.Combine(_suiteRoot, "themes");
		Directory.CreateDirectory(_suiteRoot);
	}

	[SetUp]
	public void SetUp() {
		_fixtureRoot = Path.Combine(_suiteRoot, Guid.NewGuid().ToString("N"));
		var directory = Path.Combine(_fixtureRoot, ".localnotion");
		Directory.CreateDirectory(directory);
		_registryPath = Path.Combine(directory, "registry.json");
		WriteRegistry();
	}

	[TearDown]
	public void TearDown() {
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_fixtureRoot)), Is.EqualTo(_suiteRoot));
		Assert.That(File.GetAttributes(_fixtureRoot).HasFlag(FileAttributes.ReparsePoint), Is.False);
		foreach (var file in Directory.EnumerateFiles(_fixtureRoot, "*", SearchOption.AllDirectories))
			File.SetAttributes(file, FileAttributes.Normal);
		Directory.Delete(_fixtureRoot, recursive: true);
	}

	[OneTimeTearDown]
	public void OneTimeTearDown() {
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_suiteRoot)), Is.EqualTo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()))));
		Assert.That(Path.GetFileName(_suiteRoot), Does.StartWith("localnotion-git-config-"));
		Assert.That(File.GetAttributes(_suiteRoot).HasFlag(FileAttributes.ReparsePoint), Is.False);
		Directory.Delete(_suiteRoot, recursive: true);
	}

	[Test]
	public async Task DisplayUsesNormalRepositoryLoadingWithoutRewritingRegistryOrPrintingPrivateSettings() {
		var registry = ReadRegistry();
		registry["git"] = JObject.Parse("{\"enabled\":true,\"push\":false,\"future\":\"synthetic-private-git-setting\"}");
		registry["future"] = "synthetic-private-repository-setting";
		File.WriteAllText(_registryPath, registry.ToString());
		var original = File.ReadAllBytes(_registryPath);

		var result = await Parse("git", "--path", _fixtureRoot);

		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":true,\"push\":false}}"));
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
		Assert.That(Directory.Exists(Path.Combine(_themesPath, "default")), Is.True, "The command follows the same theme deployment and repository loading path as other commands.");
		Assert.That(Directory.Exists(Path.Combine(_fixtureRoot, ".git")), Is.False);
	}

	[Test]
	public async Task ChangedSettingsPersistThroughNormalSaveWhileRetainingRepositoryConfigurationAndUnknownFields() {
		var registry = ReadRegistry();
		registry["git"] = JObject.Parse("{\"enabled\":true,\"push\":true,\"future\":{\"push\":true,\"secret\":\"synthetic-private-git-setting\"}}");
		registry["future"] = JObject.Parse("{\"nested\":[1,2,3],\"text\":\"synthetic-private-repository-setting\"}");
		File.WriteAllText(_registryPath, registry.ToString());

		var result = await Parse("git", "--path", _fixtureRoot, "--enabled", "false", "--push", "false");

		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":false,\"push\":false}}"));
		var saved = ReadRegistry();
		Assert.That((bool)saved["git"]["enabled"], Is.False);
		Assert.That((bool)saved["git"]["push"], Is.False);
		Assert.That(JToken.DeepEquals(saved["git"]["future"], registry["git"]["future"]), Is.True);
		Assert.That(JToken.DeepEquals(saved["future"], registry["future"]), Is.True);
		foreach (var property in new[] { "notion_api_key", "paths", "default_themes", "nginx", "apache", "log_level", "resources", "cms_items" })
			Assert.That(JToken.DeepEquals(saved[property], registry[property]), Is.True, $"Changing Git settings must preserve {property}.");
		Assert.That(File.Exists(_registryPath + ".persist"), Is.False);
		Assert.That(File.Exists(_registryPath + ".commit"), Is.False);
		Assert.That(Directory.Exists(Path.Combine(_fixtureRoot, ".git")), Is.False);
	}

	[Test]
	public async Task OneSettingPreservesTheOtherAndPushCanBePreparedWhileDisabled() {
		var push = await Parse("git", "--path", _fixtureRoot, "--push", "true");
		Assert.That(push.ExitCode, Is.Zero, push.Output);
		Assert.That(push.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":false,\"push\":true}}"));
		var enabled = await Parse("git", "--path", _fixtureRoot, "--enabled", "true");
		Assert.That(enabled.ExitCode, Is.Zero, enabled.Output);
		Assert.That(enabled.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":true,\"push\":true}}"));
		var disabled = await Parse("git", "--path", _fixtureRoot, "--enabled", "false");
		Assert.That(disabled.ExitCode, Is.Zero, disabled.Output);
		Assert.That(disabled.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":false,\"push\":true}}"));
		Assert.That(Directory.Exists(Path.Combine(_fixtureRoot, ".git")), Is.False);
	}

	[TestCase("absent")]
	[TestCase("null")]
	[TestCase("empty")]
	public async Task MissingGitSettingsUseRepositoryDefaultsAndCanBeSaved(string shape) {
		var registry = ReadRegistry();
		if (shape == "absent")
			registry.Remove("git");
		else
			registry["git"] = shape == "null" ? JValue.CreateNull() : new JObject();
		File.WriteAllText(_registryPath, registry.ToString());
		var original = File.ReadAllBytes(_registryPath);

		var display = await Parse("git", "--path", _fixtureRoot);
		Assert.That(display.ExitCode, Is.Zero, display.Output);
		Assert.That(display.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":false,\"push\":false}}"));
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
		var update = await Parse("git", "--path", _fixtureRoot, "--enabled", "true", "--push", "true");
		Assert.That(update.ExitCode, Is.Zero, update.Output);
		Assert.That(update.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":true,\"push\":true}}"));
		Assert.That((bool)ReadRegistry()["git"]["enabled"], Is.True);
		Assert.That((bool)ReadRegistry()["git"]["push"], Is.True);
	}

	[TestCase("")]
	[TestCase("null")]
	[TestCase("[]")]
	[TestCase("not json synthetic-private-key")]
	[TestCase("{\"git\":\"synthetic-private-key\"}")]
	[TestCase("{\"git\":{\"enabled\":{\"secret\":\"synthetic-private-key\"}}}")]
	public async Task InvalidRegistryFailsWithoutWritingOrLeakingRawData(string original) {
		File.WriteAllText(_registryPath, original);
		var result = await Parse("git", "--path", _fixtureRoot, "--enabled", "false");
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Not.Contain("synthetic-private-key").And.Not.Contain("NullReferenceException"));
		Assert.That(File.ReadAllText(_registryPath), Is.EqualTo(original));
	}

	[TestCase("--enabled", "maybe")]
	[TestCase("--push", "yes")]
	[TestCase("--enabled", null)]
	public async Task InvalidBooleanIsRejectedByActualParser(string option, string value) {
		var original = File.ReadAllBytes(_registryPath);
		var arguments = new[] { "git", "--path", _fixtureRoot, option };
		var result = await Parse(value == null ? arguments : arguments.Append(value).ToArray());
		Assert.That(result.ExitCode, Is.Not.Zero, result.Output);
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
	}

	[Test]
	public async Task UnchangedAssignmentDoesNotRequireRegistryWriteAccess() {
		var original = File.ReadAllBytes(_registryPath);
		File.SetAttributes(_registryPath, FileAttributes.ReadOnly);
		var result = await Parse("git", "--path", _fixtureRoot, "--enabled", "false");
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
	}

	[Test]
	public async Task FailedSaveIsRecoveredWhenReadOnlyRegistryCanBeWrittenAgain() {
		var original = File.ReadAllBytes(_registryPath);
		File.SetAttributes(_registryPath, FileAttributes.ReadOnly);
		var result = await Parse("git", "--path", _fixtureRoot, "--enabled", "true");
		Assert.That(result.ExitCode, Is.Not.Zero, result.Output);
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
		Assert.That(result.Output, Does.Not.Contain("synthetic-private-key"));
		Assert.That(File.Exists(_registryPath + ".commit"), Is.True, "The normal save retains the completed commit phase when overwriting the primary registry fails.");
		File.SetAttributes(_registryPath, FileAttributes.Normal);
		var recovered = await Parse("git", "--path", _fixtureRoot);
		Assert.That(recovered.ExitCode, Is.Zero, recovered.Output);
		Assert.That(recovered.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":true,\"push\":false}}"));
		Assert.That(File.Exists(_registryPath + ".persist"), Is.False);
		Assert.That(File.Exists(_registryPath + ".commit"), Is.False);
	}

	[Test]
	public async Task MissingRepositoryIsNotCreated() {
		var missing = Path.Combine(_fixtureRoot, "missing");
		var result = await Parse("git", "--path", missing, "--enabled", "true");
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(Directory.Exists(missing), Is.False);
	}

	[Test]
	public async Task OmittedPathSelectsCurrentDirectory() {
		var originalDirectory = Environment.CurrentDirectory;
		using var restore = Tools.Scope.ExecuteOnDispose(() => Environment.CurrentDirectory = originalDirectory);
		Environment.CurrentDirectory = _fixtureRoot;
		var result = await Parse("git");
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":false,\"push\":false}}"));
	}

	[Test]
	public async Task ExistingRegistryWriterPreventsOpeningOrChangingSettings() {
		var original = File.ReadAllBytes(_registryPath);
		using (var writer = new FileStream(_registryPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
			var result = await Parse("git", "--path", _fixtureRoot, "--enabled", "true");
			Assert.That(result.ExitCode, Is.Not.Zero, result.Output);
			Assert.That(File.Exists(_registryPath + ".persist"), Is.False);
			Assert.That(File.Exists(_registryPath + ".commit"), Is.False);
		}
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
	}

	[Test]
	public async Task CancellationBeforeOpeningLeavesRegistryAndRepositoryFoldersUnchanged() {
		var original = File.ReadAllBytes(_registryPath);
		var files = Directory.GetFileSystemEntries(_fixtureRoot, "*", SearchOption.AllDirectories);
		var result = await Capture(() => Program.ExecuteGitCommandAsync(new Program.GitRepositoryCommandArguments { Path = _fixtureRoot, Enabled = new[] { "true" } }, new CancellationToken(canceled: true)));
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(File.ReadAllBytes(_registryPath), Is.EqualTo(original));
		Assert.That(Directory.GetFileSystemEntries(_fixtureRoot, "*", SearchOption.AllDirectories), Is.EquivalentTo(files));
	}

	[TestCase(".persist", false)]
	[TestCase(".persist", true)]
	[TestCase(".commit", false)]
	[TestCase(".commit", true)]
	public async Task NormalRepositoryRecoveryPrecedesDisplayOrSettingsChanges(string suffix, bool edit) {
		var committed = ReadRegistry();
		committed["git"] = JObject.Parse("{\"enabled\":true,\"push\":true}");
		File.WriteAllText(_registryPath + suffix, suffix == ".commit" ? committed.ToString() : "unfinished registry write");
		var arguments = new[] { "git", "--path", _fixtureRoot };
		var result = await Parse(edit ? arguments.Concat(new[] { "--push", "false" }).ToArray() : arguments);
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		var enabled = suffix == ".commit";
		var push = enabled && !edit;
		Assert.That(result.Output.Trim(), Is.EqualTo($"{{\"git\":{{\"enabled\":{enabled.ToString().ToLowerInvariant()},\"push\":{push.ToString().ToLowerInvariant()}}}}}"));
		Assert.That(File.Exists(_registryPath + ".persist"), Is.False);
		Assert.That(File.Exists(_registryPath + ".commit"), Is.False);
	}

	[Test]
	public async Task ChangingEnabledRepositorySettingsDoesNotStageCommitOrPush() {
		try {
			await Git("init");
		} catch (System.ComponentModel.Win32Exception) {
			Assert.Ignore("Git is required for the repository change-control integration test.");
		}
		await Git("config", "user.name", "LocalNotion Git settings fixture");
		await Git("config", "user.email", "fixture@example.invalid");
		await Git("config", "commit.gpgsign", "false");
		await Git("config", "core.hooksPath", Path.Combine(_fixtureRoot, ".git", "hooks"));
		await Git("config", "core.autocrlf", "false");
		await Git("config", "remote.origin.url", new Uri(Path.Combine(_suiteRoot, "missing-remote")).AbsoluteUri);
		await Git("config", "remote.pushDefault", "origin");
		await Git("config", "push.default", "current");
		WriteRegistry(enabled: true);
		await Git("add", ".localnotion/registry.json");
		await Git("commit", "-m", "Create isolated Git settings fixture");
		var head = await Git("rev-parse", "HEAD");
		var indexPath = Path.Combine(_fixtureRoot, ".git", "index");
		var index = File.ReadAllBytes(indexPath);

		var result = await Parse("git", "--path", _fixtureRoot, "--push", "true");

		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.Output.Trim(), Is.EqualTo("{\"git\":{\"enabled\":true,\"push\":true}}"));
		Assert.That(await Git("rev-parse", "HEAD"), Is.EqualTo(head));
		Assert.That(File.ReadAllBytes(indexPath), Is.EqualTo(index));
		Assert.That((await Git("status", "--porcelain", "--untracked-files=no")).TrimEnd(), Is.EqualTo(" M .localnotion/registry.json"));
		Assert.That(await Git("diff", "--cached", "--name-only"), Is.Empty);
	}

	[TestCase("--help")]
	[TestCase("git", "--help")]
	public async Task HelpListsCommandAndItsSupportedOptions(params string[] arguments) {
		var result = await Parse(arguments);
		if (arguments.Length > 1)
			Assert.That(result.Output, Does.Contain("--enabled").And.Contain("--push").And.Contain("--path").And.Contain("true").And.Contain("false"));
		else
			Assert.That(result.Output, Does.Contain("git"));
	}

	private void WriteRegistry(bool enabled = false, bool push = false) {
		Tools.Json.WriteToFile(_registryPath, new LocalNotionRegistry {
			NotionApiKey = "synthetic-private-key",
			Paths = new LocalNotionPathProfile {
				ThemesPathR = _themesPath,
				BaseUrl = "https://example.invalid/",
				Mode = LocalNotionMode.Online
			},
			GitSettings = new GitSettings { Enabled = enabled, Push = push },
			NGinxSettings = new NGinxSettings { Enabled = true, ReloadCommand = "synthetic-command-must-not-run" },
			ApacheSettings = new ApacheSettings { Enabled = true },
			LogLevel = LogLevel.Error,
			DefaultThemes = ["default", "synthetic-user-theme"]
		});
	}

	private JObject ReadRegistry() => JObject.Parse(File.ReadAllText(_registryPath));

	private Task<(int ExitCode, string Output)> Parse(params string[] arguments)
		=> Capture(async () => {
			using var parser = new Parser(settings => settings.HelpWriter = Console.Error);
			return await Program.ProcessCommandLineAsync(arguments, parser, _ => throw new AssertionException("Git configuration must not create a Notion client."));
		});

	private static async Task<(int ExitCode, string Output)> Capture(Func<Task<int>> operation) {
		using var output = new StringWriter();
		var originalOutput = Console.Out;
		var originalError = Console.Error;
		using var restore = Tools.Scope.ExecuteOnDispose(() => {
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
		});
		Console.SetOut(output);
		Console.SetError(output);
		var exitCode = await operation();
		return (exitCode, output.ToString());
	}

	private async Task<string> Git(params string[] arguments) {
		var startInfo = new ProcessStartInfo("git") {
			WorkingDirectory = _fixtureRoot,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		startInfo.ArgumentList.Add("-c");
		startInfo.ArgumentList.Add("safe.directory=" + _fixtureRoot.Replace('\\', '/'));
		foreach (var argument in arguments)
			startInfo.ArgumentList.Add(argument);
		using var process = Process.Start(startInfo);
		var output = process.StandardOutput.ReadToEndAsync();
		var error = process.StandardError.ReadToEndAsync();
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		try {
			await process.WaitForExitAsync(timeout.Token);
		} catch (OperationCanceledException) {
			process.Kill(entireProcessTree: true);
			await process.WaitForExitAsync();
			throw;
		}
		Assert.That(process.ExitCode, Is.Zero, await output + await error);
		return await output;
	}
}
