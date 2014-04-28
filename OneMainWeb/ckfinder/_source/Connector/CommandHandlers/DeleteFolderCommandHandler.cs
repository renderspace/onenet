/*
 * CKFinder
 * ========
 * http://ckfinder.com
 * Copyright (C) 2007-2010, CKSource - Frederico Knabben. All rights reserved.
 *
 * The software, this file and its contents are subject to the CKFinder
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of CKFinder.
 */

using System;
using System.Web;
using System.Xml;
using System.Globalization;

namespace CKFinder.Connector.CommandHandlers
{
	internal class DeleteFolderCommandHandler : XmlCommandHandlerBase
	{
		public DeleteFolderCommandHandler()
			: base()
		{
		}

		protected override void BuildXml()
		{
			if ( Request.Form["CKFinderCommand"] != "true" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FolderDelete ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			// The root folder cannot be deleted.
			if ( this.CurrentFolder.ClientPath == "/" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
				return;
			}

			try
			{
				DbFileSystem.DeleteFolder(this.CurrentFolder.ClientPath);
			}
			catch ( System.UnauthorizedAccessException )
			{
				ConnectorException.Throw( Errors.AccessDenied );
			}
			catch ( System.Security.SecurityException )
			{
				ConnectorException.Throw( Errors.AccessDenied );
			}
			catch ( System.ArgumentException )
			{
				ConnectorException.Throw( Errors.InvalidName );
			}
			catch ( System.IO.PathTooLongException )
			{
				ConnectorException.Throw( Errors.InvalidName );
			}
			catch ( Exception )
			{
#if DEBUG
				throw;
#else
				ConnectorException.Throw( Errors.Unknown );
#endif
			}
		}
	}
}
