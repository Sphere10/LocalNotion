// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.IO;
using System.Threading.Tasks;
using Sphere10.Framework;

namespace LocalNotion.Core;

internal static class RepositoryThemeDeployment {
	internal static async Task DeployMissing(string themesDirectory) {
		using var archive = Sphere10.Framework.Web.VisualRenderer.EmbeddedThemes.OpenArchive();
		foreach (var entry in archive.Entries) {
			if (entry.Name.Length == 0)
				continue;
			var destination = Path.Combine(themesDirectory, entry.FullName);
			await DeployFile(entry.Open, destination);
		}
	}

	private static async Task DeployFile(Func<Stream> openSource, string destination) {
		if (File.Exists(destination))
			return;
		var directory = Path.GetDirectoryName(destination);
		Directory.CreateDirectory(directory);
		// Publish complete files atomically and preserve user overrides, including empty files.
		var temporaryFile = Path.Combine(directory, ".localnotion-theme-" + Guid.NewGuid().ToString("N") + ".tmp");
		using var cleanup = new ActionScope(() => File.Delete(temporaryFile));
		using (var source = openSource()) {
			await using var output = new FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
			await source.CopyToAsync(output);
		}
		try {
			File.Move(temporaryFile, destination);
		} catch (IOException) when (File.Exists(destination)) {
			// Another opener or the user supplied the destination during extraction.
		}
	}
}
