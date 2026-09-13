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
using Microsoft.Extensions.FileProviders;

namespace LocalNotion.Core;

internal static class RepositoryThemeDeployment {
	private static readonly ManifestEmbeddedFileProvider _embeddedThemes = new(typeof(Sphere10.VisualRenderer.HtmlRenderer).Assembly, "Themes");

	internal static Task DeployMissing(string themesDirectory) => DeployDirectory("", themesDirectory);

	private static async Task DeployDirectory(string sourceDirectory, string destinationDirectory) {
		Directory.CreateDirectory(destinationDirectory);
		foreach (var entry in _embeddedThemes.GetDirectoryContents(string.IsNullOrEmpty(sourceDirectory) ? "/" : sourceDirectory)) {
			var destination = Path.Combine(destinationDirectory, entry.Name);
			if (entry.IsDirectory) {
				var source = string.IsNullOrEmpty(sourceDirectory) ? entry.Name : sourceDirectory + "/" + entry.Name;
				await DeployDirectory(source, destination);
				continue;
			}
			if (File.Exists(destination))
				continue;

			// Publish complete files atomically and never replace a user override, including an empty file.
			var temporaryFile = Path.Combine(destinationDirectory, ".localnotion-theme-" + Guid.NewGuid().ToString("N") + ".tmp");
			try {
				using (var source = entry.CreateReadStream()) {
					await using var output = new FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
					await source.CopyToAsync(output);
				}
				try {
					File.Move(temporaryFile, destination);
				} catch (IOException) when (File.Exists(destination)) {
					// Another repository opener or the user supplied this file while it was being copied.
				}
			} finally {
				File.Delete(temporaryFile);
			}
		}
	}
}
