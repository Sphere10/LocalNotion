// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace LocalNotion.Core;

/// <summary>
/// Identifies a newly created database whose preset setup or verification could not complete.
/// </summary>
public sealed class CMSDatabaseCreationException : Exception {
	public CMSDatabaseCreationException(Guid databaseID, Guid setupPageID, bool setupPageArchived, Exception error, Exception cleanupError = null)
		: base("The CMS database was created, but its setup or verification did not complete.", error ?? cleanupError) {
		DatabaseID = databaseID;
		SetupPageID = setupPageID;
		SetupPageArchived = setupPageArchived;
		CleanupError = cleanupError;
	}

	public Guid DatabaseID { get; }
	public Guid SetupPageID { get; }
	public bool SetupPageArchived { get; }
	public Exception CleanupError { get; }
	public bool CleanupFailed => CleanupError != null;
}
