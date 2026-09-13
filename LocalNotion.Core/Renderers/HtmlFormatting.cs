// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

#if CleanHTML
using System.IO;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace LocalNotion.Core;

internal static class HtmlFormatting {
	public static string Format(string html) {
		var parser = new HtmlParser();
		var document = parser.ParseDocument(html);
		var formatter = new CleanHtmlFormatter();
		var stringBuilder = new StringBuilder();
		using var writer = new StringWriter(stringBuilder);
		document.ToHtml(writer, formatter);
		return stringBuilder.ToString();
	}
}
#endif
