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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sphere10.VisualRenderer;
using Notion.Client;
using Sphere10.Framework;
using R = Sphere10.VisualRenderer;

namespace LocalNotion.Core;

public sealed partial class NotionRenderModelBuilder {

	private R.Icon MapThumbnail(LocalNotionEditableResource owner, LocalNotionThumbnail thumbnail, string outputDirectory) => thumbnail?.Type switch {
		ThumbnailType.Emoji => new R.Icon { Emoji = thumbnail.Data, AltText = owner.Title ?? string.Empty },
		ThumbnailType.Image => new R.Icon { Url = ResolveUrl(owner, thumbnail.Data, outputDirectory), AltText = owner.Title ?? string.Empty },
		_ => null
	};

	private R.Icon MapIcon(ProjectionContext context, IPageIcon icon) {
		if (icon is null)
			return null;
		if (icon is EmojiPageIcon emoji)
			return new R.Icon { Emoji = emoji.Emoji };
		if (context.Purpose == ProjectionPurpose.Text)
			return null;
		var rawUrl = icon switch {
			FilePageIcon file => file.File?.Url,
			ExternalPageIcon external => external.External?.Url,
			CustomEmojiPageIcon custom => custom.CustomEmoji?.Url,
			IconPageIcon builtIn => builtIn.GetIconUrl(),
			FileObject file => file switch { ExternalFile external => external.External?.Url, UploadedFile uploaded => uploaded.File?.Url, _ => null },
			_ => null
		};
		return string.IsNullOrWhiteSpace(rawUrl) ? null : new R.Icon { Url = ResolveUrl(context.Resource, rawUrl, context.OutputDirectory) };
	}

	private R.MediaBlock MapMedia(ProjectionContext context, FileObject file, R.MediaType type) {
		var rawUrl = FileUrl(file);
		var urlValue = context.Purpose == ProjectionPurpose.Text ? rawUrl : ResolveMediaUrl(context.Resource, file, context.OutputDirectory);
		var displayUrl = urlValue ?? string.Empty;
		var pathValue = Uri.TryCreate(displayUrl, UriKind.Absolute, out var uriValue) && uriValue.Scheme is "http" or "https" ? uriValue.AbsolutePath : displayUrl.Split('?', '#')[0];
		return new R.MediaBlock {
			Type = type,
			Url = urlValue ?? string.Empty,
			FileName = Uri.UnescapeDataString(Path.GetFileName(pathValue) ?? string.Empty),
			Caption = MapRichText(context, file?.Caption),
			AltText = Plain(file?.Caption)
		};
	}

	private VisualNode MapVideo(ProjectionContext context, VideoBlock block) {
		var rawUrl = FileUrl(block.Video);
		if (string.IsNullOrWhiteSpace(rawUrl))
			return Unsupported(block) with { PlainTextOverride = string.Empty };
		R.MediaBlock media = MapMedia(context, block.Video, R.MediaType.Video);
		if (block.Video is ExternalFile && Tools.Url.IsVideoSharingUrl(rawUrl, out var platform, out var videoID)
			&& Enum.TryParse<EmbedProvider>(platform.ToString(), out var provider))
			return media with {
				Provider = provider,
				ProviderId = videoID,
				PlainTextOverride = $"[{platform}]({videoID}) {block.Video.Caption} " + Environment.NewLine
			};
		return media with { PlainTextOverride = $"[Video]({rawUrl}) {block.Video.Caption} " + Environment.NewLine };
	}

	private VisualNode MapEmbed(ProjectionContext context, EmbedBlock block) {
		var rawUrl = block.Embed?.Url;
		if (string.IsNullOrWhiteSpace(rawUrl))
			return Unsupported(block) with { PlainTextOverride = string.Empty };
		if (rawUrl.Contains("twitter", StringComparison.OrdinalIgnoreCase) || rawUrl.Contains("x.com", StringComparison.OrdinalIgnoreCase))
			return new R.MediaBlock {
				Type = R.MediaType.Embed,
				Provider = EmbedProvider.Twitter,
				Caption = MapRichText(context, block.Embed.Caption),
				Url = context.Purpose == ProjectionPurpose.Text ? rawUrl : ResolveUrl(context.Resource, rawUrl.Replace("https://x.com", "https://twitter.com"), context.OutputDirectory),
				PlainTextOverride = rawUrl + Environment.NewLine
			};
		if (Tools.Url.IsVideoSharingUrl(rawUrl, out var platform, out var videoID)
			&& Enum.TryParse<EmbedProvider>(platform.ToString(), out var provider))
			return new R.MediaBlock {
				Type = R.MediaType.Embed,
				Provider = provider,
				ProviderId = videoID,
				Url = context.Purpose == ProjectionPurpose.Text ? rawUrl : ResolveUrl(context.Resource, rawUrl, context.OutputDirectory),
				Caption = MapRichText(context, block.Embed.Caption),
				PlainTextOverride = Environment.NewLine
			};
		return Unsupported(block);
	}

	private static string FileUrl(FileObject file) => file switch {
		ExternalFile external => external.External?.Url ?? string.Empty,
		UploadedFile uploaded => uploaded.File?.Url ?? string.Empty,
		_ => string.Empty
	};

	private R.BreadcrumbBlock MapBreadcrumb(ProjectionContext context) {
		if (context.Purpose == ProjectionPurpose.Text)
			return new R.BreadcrumbBlock { PlainTextOverride = string.Empty };
		var ancestry = GetAncestry(context.Resource);
		var last = ancestry.LastOrDefault();
		// The legacy generator walks repository ancestry without a cycle guard.
		if (ancestry.Length >= MaximumDepth || last?.ParentResourceID is not null && ancestry.Any(item => NormalizeID(item.ID) == NormalizeID(last.ParentResourceID)))
			return Fallback();
		try {
			var breadcrumb = new BreadCrumbGenerator(_repository, Links).CalculateBreadcrumb(context.Resource);
			return new R.BreadcrumbBlock {
				Items = breadcrumb.Trail.Select(item => {
					var thumbnailOwner = ancestry.OfType<LocalNotionEditableResource>().FirstOrDefault(ancestor => ancestor.Thumbnail?.Data == item.Data) ?? context.Resource;
					R.Icon icon = item.Traits.HasFlag(BreadCrumbItemTraits.HasEmojiIcon)
						? new R.Icon { Emoji = item.Data }
						: item.Traits.HasFlag(BreadCrumbItemTraits.HasImageIcon) ? new R.Icon { Url = ResolveUrl(thumbnailOwner, item.Data, context.OutputDirectory) } : null;
					return new R.BreadcrumbItem {
						IsCurrent = item.Traits.HasFlag(BreadCrumbItemTraits.IsCurrentPage),
						Reference = new R.Reference {
							Label = item.Text ?? string.Empty,
							Url = _repository.Paths.Mode == LocalNotionMode.Offline ? RebaseRelativeUrl(context.Resource, item.Url, context.OutputDirectory, true) : ResolveUrl(context.Resource, item.Url, context.OutputDirectory),
							Icon = icon,
							IsAvailable = item.Traits.HasFlag(BreadCrumbItemTraits.HasUrl)
						}
					};
				}).ToArray(),
				PlainTextOverride = string.Empty
			};
		} catch (Exception error) when (error is not OperationCanceledException) {
			_logger?.Warning($"Unable to project complete breadcrumbs for '{context.Resource.Title}': {error.Message}");
			return Fallback();
		}
		R.BreadcrumbBlock Fallback() => new() {
			Items = ancestry.Reverse().Select(item => new R.BreadcrumbItem {
				IsCurrent = item.ID == context.Resource.ID,
				Reference = ResolveReference(context.Resource, item.ID, context.OutputDirectory, omitIndicator: true)
			}).ToArray(),
			PlainTextOverride = string.Empty
		};
	}
}
