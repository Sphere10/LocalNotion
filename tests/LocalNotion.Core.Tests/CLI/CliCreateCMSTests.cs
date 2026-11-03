// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using LocalNotion.CLI;
using Notion.Client;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Unit")]
[NonParallelizable]
public class CliCreateCMSTests {
	private const string ParentPageID = "33a865d7-7bed-48e8-880c-57aa191c55bf";
	private string _fixtureRoot;
	private string _originalSecretFile;
	private RecordingNotionTransport _transport;
	private string _receivedKey;
	private int _factoryCalls;

	[SetUp]
	public void SetUp() {
		_originalSecretFile = Environment.GetEnvironmentVariable("NOTION_API_KEY_FILE");
		Environment.SetEnvironmentVariable("NOTION_API_KEY_FILE", null);
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-create-cms-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
		_transport = new RecordingNotionTransport();
		_receivedKey = null;
		_factoryCalls = 0;
	}

	[TearDown]
	public void TearDown() {
		Environment.SetEnvironmentVariable("NOTION_API_KEY_FILE", _originalSecretFile);
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_fixtureRoot)), Is.EqualTo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()))));
		Assert.That(Path.GetFileName(_fixtureRoot), Does.StartWith("localnotion-create-cms-"));
		Assert.That(File.GetAttributes(_fixtureRoot).HasFlag(FileAttributes.ReparsePoint), Is.False);
		Directory.Delete(_fixtureRoot, recursive: true);
	}

	[Test]
	public async Task RegisteredCommandCreatesWithoutExistingRepository() {
		var result = await Parse("create-cms", "--parent", ParentPageID, "--key", "synthetic-explicit-secret");
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(_receivedKey, Is.EqualTo("synthetic-explicit-secret"));
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
		Assert.That((string)_transport.Body["title"][0]["text"]["content"], Is.EqualTo("My Website"));
		Assert.That(result.Output, Does.Contain("Database ID:").And.Contain("https://www.notion.so/").And.Contain("localnotion init --cms"));
		Assert.That(result.Output, Does.Contain("zero active rows").And.Contain("temporary Hidden setup row").And.Contain("trash"));
		Assert.That(result.Output, Does.Not.Contain("synthetic-explicit-secret"));
		Assert.That(Directory.EnumerateFileSystemEntries(_fixtureRoot), Is.Empty);
	}

	[TestCase("--help")]
	[TestCase("create-cms", "--help")]
	public async Task HelpIncludesNewCommandAndOptions(params string[] arguments) {
		var result = await Parse(arguments);
		if (arguments.Length > 1)
			Assert.That(result.Output, Does.Contain("--parent").And.Contain("--title").And.Contain("--path").And.Contain("--key"));
		else
			Assert.That(result.Output, Does.Contain("create-cms"));
		Assert.That(_factoryCalls, Is.Zero);
	}

	[Test]
	public async Task ParentIsRequiredByActualCommandParser() {
		var result = await Parse("create-cms", "--key", "synthetic-secret");
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("parent"));
		Assert.That(_factoryCalls, Is.Zero);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("not-a-page-id")]
	[TestCase("00000000-0000-0000-0000-000000000000")]
	public async Task InvalidParentDoesNotContactNotion(string parent) {
		var arguments = Arguments();
		arguments.Parent = parent;
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("valid, non-empty Notion page UUID"));
		Assert.That(_factoryCalls, Is.Zero);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase(" ")]
	public async Task BlankTitleDoesNotContactNotion(string title) {
		var arguments = Arguments();
		arguments.Title = title;
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(_factoryCalls, Is.Zero);
	}

	[Test]
	public async Task OversizedTitleDoesNotContactNotion() {
		var arguments = Arguments();
		arguments.Title = new string('x', 2001);
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(_factoryCalls, Is.Zero);
	}

	[Test]
	public async Task MissingKeyReportsSupportedSourcesWithoutContactingNotion() {
		var arguments = Arguments();
		arguments.APIKey = null;
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("NOTION_API_KEY_FILE").And.Contain("--key").And.Contain("--path"));
		Assert.That(_factoryCalls, Is.Zero);
	}

	[Test]
	public async Task SecretFileIsUsedWithoutEchoingIt() {
		var keyFile = Path.Combine(_fixtureRoot, "api-key.txt");
		await File.WriteAllTextAsync(keyFile, " synthetic-file-secret\r\n");
		Environment.SetEnvironmentVariable("NOTION_API_KEY_FILE", keyFile);
		var result = await Parse("create-cms", "--parent", ParentPageID, "--title", "Documentation website");
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(_receivedKey, Is.EqualTo("synthetic-file-secret"));
		Assert.That(result.Output, Does.Not.Contain("synthetic-file-secret"));
		Assert.That((string)_transport.Body["title"][0]["text"]["content"], Is.EqualTo("Documentation website"));
	}

	[Test]
	public async Task ExplicitKeyTakesPrecedenceOverSecretFileAndRepository() {
		var keyFile = Path.Combine(_fixtureRoot, "api-key.txt");
		await File.WriteAllTextAsync(keyFile, "synthetic-file-secret");
		Environment.SetEnvironmentVariable("NOTION_API_KEY_FILE", keyFile);
		await CreateRepository();
		var arguments = Arguments();
		arguments.Path = _fixtureRoot;
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(_receivedKey, Is.EqualTo("synthetic-explicit-secret"));
	}

	[Test]
	public async Task SecretFileTakesPrecedenceOverRepository() {
		var keyFile = Path.Combine(_fixtureRoot, "api-key.txt");
		await File.WriteAllTextAsync(keyFile, "synthetic-file-secret");
		Environment.SetEnvironmentVariable("NOTION_API_KEY_FILE", keyFile);
		await CreateRepository();
		var arguments = Arguments();
		arguments.APIKey = null;
		arguments.Path = _fixtureRoot;
		var result = await Execute(arguments);
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(_receivedKey, Is.EqualTo("synthetic-file-secret"));
	}

	[Test]
	public async Task RepositoryCredentialFallbackReadsWithoutOpeningOrChangingRepository() {
		var registryFile = await CreateRepository();
		var original = await File.ReadAllTextAsync(registryFile);
		var result = await Parse("create-cms", "--parent", ParentPageID, "--path", _fixtureRoot);
		Assert.That(result.ExitCode, Is.Zero, result.Output);
		Assert.That(_receivedKey, Is.EqualTo("synthetic-repository-secret"));
		Assert.That(await File.ReadAllTextAsync(registryFile), Is.EqualTo(original));
		Assert.That(Directory.EnumerateFiles(_fixtureRoot, "*", SearchOption.AllDirectories).ToArray(), Is.EqualTo(new[] { registryFile }));
		Assert.That(result.Output, Does.Not.Contain("synthetic-repository-secret"));
	}

	[Test]
	public async Task InvalidRepositoryFailsWithoutCreateOrLeakingRegistry() {
		var registryFile = await CreateRepository();
		await File.WriteAllTextAsync(registryFile, "{\"notion_api_key\":\"synthetic-repository-secret\", broken");
		var result = await Parse("create-cms", "--parent", ParentPageID, "--path", _fixtureRoot);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(_factoryCalls, Is.Zero);
		Assert.That(result.Output, Does.Not.Contain("synthetic-repository-secret"));
	}

	[Test]
	public async Task MissingRepositoryRegistryFailsWithoutCreate() {
		var result = await Parse("create-cms", "--parent", ParentPageID, "--path", _fixtureRoot);
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(_factoryCalls, Is.Zero);
		Assert.That(result.Output, Does.Contain("registry.json"));
	}

	[TestCase(HttpStatusCode.Unauthorized, "API key")]
	[TestCase(HttpStatusCode.Forbidden, "insert-content")]
	[TestCase(HttpStatusCode.NotFound, "share that page")]
	[TestCase(HttpStatusCode.TooManyRequests, "rate limit")]
	public async Task APIErrorIsSanitizedAndDoesNotRetryOrReportSuccess(HttpStatusCode statusCode, string guidance) {
		_transport.Error = new NotionApiException(statusCode, null, "secret response synthetic-explicit-secret");
		var result = await Execute(Arguments());
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain($"HTTP {(int)statusCode}").And.Contain(guidance).And.Contain("No retry"));
		Assert.That(result.Output, Does.Not.Contain("synthetic-explicit-secret").And.Not.Contain("secret response").And.Not.Contain("Database ID:"));
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task AmbiguousTransportFailureIsSanitizedAndDoesNotRetry(bool ioFailure) {
		_transport.Error = ioFailure
			? new IOException("secret response synthetic-explicit-secret")
			: new HttpRequestException("secret response synthetic-explicit-secret");
		var result = await Execute(Arguments());
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("may have been created").And.Contain("No retry"));
		Assert.That(result.Output, Does.Not.Contain("synthetic-explicit-secret").And.Not.Contain("Database ID:"));
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
	}

	[Test]
	public async Task MissingDatabaseIDReportsAmbiguousFailure() {
		_transport.Response = new Database();
		var result = await Execute(Arguments());
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("may have been created").And.Not.Contain("Database ID:"));
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
	}

	[Test]
	public async Task CancelledCommandReturnsNonzeroWithoutCreatingClient() {
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var result = await Capture(() => Program.ExecuteCreateCMSCommandAsync(Arguments(), cancellation.Token, CreateClient));
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(_factoryCalls, Is.Zero);
		Assert.That(result.Output, Does.Contain("cancelled"));
	}

	[Test]
	public async Task PostCreateFailureReportsKnownRecoveryURLsAndSanitizesCause() {
		_transport.PageUpdateError = new IOException("synthetic-explicit-secret");
		var result = await Execute(Arguments());
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("setup or verification is incomplete").And.Contain("Database ID:")
			.And.Contain("Temporary setup row:").And.Contain("moved to the trash").And.Contain("another database"));
		Assert.That(result.Output, Does.Not.Contain("synthetic-explicit-secret").And.Not.Contain("localnotion init --cms"));
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
		Assert.That(_transport.PageCreateCount, Is.EqualTo(1));
	}

	[Test]
	public async Task CleanupFailureClearlyIdentifiesTheHiddenRowRequiringRecovery() {
		_transport.ArchiveError = new IOException("synthetic-explicit-secret");
		var result = await Execute(Arguments());
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("could not be confirmed in the trash").And.Contain("Temporary setup row:")
			.And.Contain("update-content").And.Not.Contain("synthetic-explicit-secret"));
		Assert.That(_transport.ArchiveCount, Is.EqualTo(1));
	}

	[Test]
	public async Task InterruptedSetupReportsDatabaseAndArchivesTheKnownRow() {
		var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var pending = new TaskCompletionSource<Page>(TaskCreationOptions.RunContinuationsAsynchronously);
		_transport.PendingPageUpdate = token => {
			entered.SetResult();
			return pending.Task.WaitAsync(token);
		};
		using var cancellation = new CancellationTokenSource();
		var resultTask = Capture(() => Program.ExecuteCreateCMSCommandAsync(Arguments(), cancellation.Token, CreateClient));
		await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
		cancellation.Cancel();
		var result = await resultTask;
		Assert.That(result.ExitCode, Is.Not.Zero);
		Assert.That(result.Output, Does.Contain("Database ID:").And.Contain("Temporary setup row:").And.Contain("moved to the trash"));
		Assert.That(_transport.SetupPageArchived, Is.True);
		Assert.That(_transport.CreateCount, Is.EqualTo(1));
	}

	private INotionClient CreateClient(string key) {
		_factoryCalls++;
		_receivedKey = key;
		return _transport.CreateClient();
	}

	private static Program.CreateCMSCommandArguments Arguments()
		=> new Program.CreateCMSCommandArguments { Parent = ParentPageID, APIKey = "synthetic-explicit-secret" };

	private Task<(int ExitCode, string Output)> Execute(Program.CreateCMSCommandArguments arguments)
		=> Capture(() => Program.ExecuteCreateCMSCommandAsync(arguments, CancellationToken.None, CreateClient));

	private Task<(int ExitCode, string Output)> Parse(params string[] arguments) {
		return Capture(async () => {
			using var parser = new Parser(settings => settings.HelpWriter = Console.Error);
			return await Program.ProcessCommandLineAsync(arguments, parser, CreateClient);
		});
	}

	private async Task<string> CreateRepository() {
		var registryDirectory = Path.Combine(_fixtureRoot, ".localnotion");
		Directory.CreateDirectory(registryDirectory);
		var registryFile = Path.Combine(registryDirectory, "registry.json");
		await File.WriteAllTextAsync(registryFile, "{\"notion_api_key\":\"synthetic-repository-secret\",\"git\":{\"enabled\":true,\"push\":true},\"resources\":[]}");
		return registryFile;
	}

	private static async Task<(int ExitCode, string Output)> Capture(Func<Task<int>> operation) {
		using var output = new StringWriter();
		var originalOutput = Console.Out;
		var originalError = Console.Error;
		using var restore = new ActionScope(() => {
			Console.SetOut(originalOutput);
			Console.SetError(originalError);
		});
		Console.SetOut(output);
		Console.SetError(output);
		var exitCode = await operation();
		return (exitCode, output.ToString());
	}
}
