// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LocalNotion.CLI;
using NUnit.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.Children)]
public class GitSentryRetryTests {
	private static readonly string ObjectWriteFailure = $"error: unable to write file .git/objects/2b/{new string('a', 38)}: Permission denied";

	[Test]
	public async Task SuccessfulAddDoesNotRetry() {
		var sentry = new ScriptedGitSentry((0, "added"));

		Assert.That(await sentry.AddAll(), Is.True);
		Assert.That(sentry.Commands, Has.Count.EqualTo(1));
		Assert.That(sentry.RetryDelays, Is.Empty);
		Assert.That(sentry.Output, Is.EqualTo("added"));
	}

	[Test]
	[Platform("Win")]
	public async Task AddRetriesObjectFinalizationErrorAndClearsEarlierOutput() {
		var sentry = new ScriptedGitSentry((1, ObjectWriteFailure), (0, string.Empty));

		Assert.That(await sentry.AddAll(), Is.True);
		Assert.That(sentry.Commands, Has.Count.EqualTo(2));
		Assert.That(sentry.Commands.All(command => command.SequenceEqual(new[] { "add", "--all" })), Is.True);
		Assert.That(sentry.RetryDelays, Is.EqualTo(new[] { TimeSpan.FromMilliseconds(250) }));
		Assert.That(sentry.Output, Is.Empty, "A successful retry must not retain the earlier error.");
	}

	[Test]
	[Platform("Win")]
	public async Task PersistentObjectFinalizationErrorStopsAfterThreeAttemptsAndRetainsFinalOutput() {
		var finalOutput = ObjectWriteFailure + Environment.NewLine + "third attempt";
		var sentry = new ScriptedGitSentry((1, ObjectWriteFailure), (1, ObjectWriteFailure), (1, finalOutput), (0, "must not run"));

		Assert.That(await sentry.AddAll(), Is.False);
		Assert.That(sentry.Commands, Has.Count.EqualTo(3));
		Assert.That(sentry.RetryDelays, Is.EqualTo(new[] { TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(500) }));
		Assert.That(sentry.Output, Is.EqualTo(finalOutput));
	}

	[TestCaseSource(nameof(NonRetryableFailures))]
	public async Task OtherAddFailuresAreNotRetried(string failure) {
		var sentry = new ScriptedGitSentry((1, failure), (0, "must not run"));

		Assert.That(await sentry.AddAll(), Is.False);
		Assert.That(sentry.Commands, Has.Count.EqualTo(1));
		Assert.That(sentry.RetryDelays, Is.Empty);
		Assert.That(sentry.Output, Is.EqualTo(failure));
	}

	[TestCase("commit")]
	[TestCase("push")]
	public async Task OtherGitOperationsNeverRetryObjectFinalizationError(string operation) {
		var sentry = new ScriptedGitSentry((1, ObjectWriteFailure), (0, "must not run"));
		var succeeded = operation == "commit" ? await sentry.Commit("message") : await sentry.Push();

		Assert.That(succeeded, Is.False);
		Assert.That(sentry.Commands, Has.Count.EqualTo(1));
		Assert.That(sentry.Commands[0][0], Is.EqualTo(operation));
		Assert.That(sentry.RetryDelays, Is.Empty);
		Assert.That(sentry.Output, Is.EqualTo(ObjectWriteFailure));
	}

	[Test]
	[Platform(Exclude = "Win")]
	public async Task NonWindowsAddDoesNotRetryObjectFinalizationError() {
		var sentry = new ScriptedGitSentry((1, ObjectWriteFailure), (0, "must not run"));

		Assert.That(await sentry.AddAll(), Is.False);
		Assert.That(sentry.Commands, Has.Count.EqualTo(1));
		Assert.That(sentry.RetryDelays, Is.Empty);
	}

	[Test]
	[Platform("Win")]
	public async Task CancellationDuringRetryWaitPreventsAnotherAttempt() {
		using var cancellation = new CancellationTokenSource();
		var sentry = new ScriptedGitSentry((1, ObjectWriteFailure), (0, "must not run")) { PauseRetry = true };
		var operation = sentry.AddAll(cancellation.Token);
		await sentry.RetryWaitStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

		cancellation.Cancel();

		Assert.That(async () => await operation, Throws.InstanceOf<OperationCanceledException>());
		Assert.That(sentry.Commands, Has.Count.EqualTo(1));
		Assert.That(sentry.Output, Is.EqualTo(ObjectWriteFailure));
	}

	private static IEnumerable<string> NonRetryableFailures() {
		yield return "fatal: Unable to create '.git/index.lock': File exists.";
		yield return "error: insufficient permission for adding an object to repository database .git/objects";
		yield return ObjectWriteFailure.Replace("Permission denied", "No space left on device", StringComparison.Ordinal);
		yield return ObjectWriteFailure.Replace("error:", "fatal:", StringComparison.Ordinal);
		yield return ObjectWriteFailure.Replace("/objects/", "/working-files/", StringComparison.Ordinal);
		yield return ObjectWriteFailure + " (additional text)";
	}

	private sealed class ScriptedGitSentry : GitSentry {
		private readonly Queue<(int ExitCode, string Output)> _results;

		public ScriptedGitSentry(params (int ExitCode, string Output)[] results)
			: base(Path.GetTempPath()) {
			_results = new Queue<(int ExitCode, string Output)>(results);
		}

		public List<string[]> Commands { get; } = [];

		public List<TimeSpan> RetryDelays { get; } = [];

		public TaskCompletionSource<bool> RetryWaitStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool PauseRetry { get; init; }

		protected override Task<int> RunGitAsync(CancellationToken cancellationToken, params string[] arguments) {
			cancellationToken.ThrowIfCancellationRequested();
			Commands.Add(arguments);
			var result = _results.Dequeue();
			OutputWriter.Write(result.Output);
			return Task.FromResult(result.ExitCode);
		}

		protected override Task WaitForAddRetryAsync(TimeSpan delay, CancellationToken cancellationToken) {
			RetryDelays.Add(delay);
			RetryWaitStarted.TrySetResult(true);
			return PauseRetry ? Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken) : Task.CompletedTask;
		}
	}
}
