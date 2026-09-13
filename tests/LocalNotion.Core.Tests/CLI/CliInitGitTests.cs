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
using System.Threading;
using System.Threading.Tasks;
using LocalNotion.CLI;
using LocalNotion.Core;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class CliInitGitTests {
	private string _fixtureRoot;
	private GitSettings _originalGitSettings;

	[SetUp]
	public async Task SetUp() {
		_originalGitSettings = GitSettings.Default;
		GitSettings.Default = new GitSettings();
		try {
			_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-init-git-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_fixtureRoot);
			if (!await new GitSentry(_fixtureRoot).TestGitInstalled())
				Assert.Ignore("Git is required for CLI change-control integration tests.");
			await Git("init");
			await Git("config", "user.name", "LocalNotion regression fixture");
			await Git("config", "user.email", "fixture@example.invalid");
			await Git("config", "commit.gpgsign", "false");
			await Git("config", "core.hooksPath", Path.Combine(_fixtureRoot, ".git", "hooks"));
		} catch {
			GitSettings.Default = _originalGitSettings;
			throw;
		}
	}

	[TearDown]
	public void TearDown() {
		try {
			if (!Directory.Exists(_fixtureRoot))
				return;
			Assert.That(Path.GetDirectoryName(Path.GetFullPath(_fixtureRoot)), Is.EqualTo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()))));
			Assert.That(Path.GetFileName(_fixtureRoot), Does.StartWith("localnotion-init-git-"));
			Assert.That(File.GetAttributes(_fixtureRoot).HasFlag(FileAttributes.ReparsePoint), Is.False);
			foreach (var file in Directory.EnumerateFiles(_fixtureRoot, "*", SearchOption.AllDirectories))
				File.SetAttributes(file, FileAttributes.Normal);
			Directory.Delete(_fixtureRoot, recursive: true);
		} finally {
			GitSettings.Default = _originalGitSettings;
		}
	}

	[TestCase("add")]
	[TestCase("commit")]
	[TestCase("push")]
	public async Task GitFailureReturnsFailureWithoutReportingInitSuccess(string failingOperation) {
		if (failingOperation == "add")
			await File.WriteAllTextAsync(Path.Combine(_fixtureRoot, ".git", "index.lock"), "fixture lock");
		if (failingOperation == "commit") {
			var hook = Path.Combine(_fixtureRoot, ".git", "hooks", "pre-commit");
			await File.WriteAllTextAsync(hook, "#!/bin/sh\nexit 1\n");
			if (!OperatingSystem.IsWindows())
				File.SetUnixFileMode(hook, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
		}
		if (failingOperation == "push") {
			await Git("config", "remote.origin.url", new Uri(Path.Combine(_fixtureRoot, "missing-remote")).AbsoluteUri);
			await Git("config", "remote.pushDefault", "origin");
			await Git("config", "push.default", "current");
		}

		var result = await Init(enableGit: true, pushRemote: failingOperation == "push");

		Assert.That(result.ExitCode, Is.Not.Zero, result.Output);
		Assert.That(result.Output, Does.Contain("git failed with error:"));
		Assert.That(result.Output, Does.Not.Contain("repository has been created"));
		Assert.That(File.Exists(Path.Combine(_fixtureRoot, ".localnotion", "registry.json")), Is.True, "A Git failure is reported without deleting the initialized repository.");
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task SuccessfulInitReportsSuccess(bool enableGit) {
		var result = await Init(enableGit);

		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(result.Output, Does.Contain("repository has been created"));
		if (enableGit)
			await Git("rev-parse", "--verify", "HEAD");
	}

	private async Task<(int ExitCode, string Output)> Init(bool enableGit, bool pushRemote = false) {
		using var output = new StringWriter();
		var originalOutput = Console.Out;
		var originalError = Console.Error;
		try {
			Console.SetOut(output);
			Console.SetError(output);
			var arguments = new Program.InitRepositoryCommandArguments {
				Path = _fixtureRoot,
				APIKey = "synthetic-init-regression-key",
				LogLevel = LogLevel.Info,
				EnableGit = enableGit,
				PushRemote = pushRemote
			};
			var exitCode = await Program.ExecuteInitCommandAsync(arguments, CancellationToken.None);
			return (exitCode, output.ToString());
		} finally {
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
		}
	}

	private async Task Git(params string[] arguments) {
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
	}
}