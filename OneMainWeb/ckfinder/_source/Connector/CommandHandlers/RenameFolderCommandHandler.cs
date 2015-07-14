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
using System.Text.RegularExpressions;

namespace CKFinder.Connector.CommandHandlers
{
	internal class RenameFolderCommandHandler : XmlCommandHandlerBase
	{
		public RenameFolderCommandHandler()
			: base()
		{
		}

		protected override void BuildXml()
		{
			if ( Request.Form["CKFinderCommand"] != "true" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FolderRename ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			// The root folder cannot be deleted.
			if ( this.CurrentFolder.ClientPath == "/" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
				return;
			}

			string newFolderName = Request[ "NewFolderName" ];

			if ( !Connector.CheckFileName( newFolderName ) || Config.Current.CheckIsHiddenFolder( newFolderName ) )
			{
				ConnectorException.Throw( Errors.InvalidName );
				return;
			}

			try
			{
				DbFileSystem.RenameFolder(this.CurrentFolder.ClientPath, newFolderName);
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
			catch ( System.NotSupportedException )
			{
				ConnectorException.Throw( Errors.InvalidName );
			}
			catch ( ConnectorException connectorException )
			{
				throw connectorException;
			}
			catch
			{
#if DEBUG
				throw;
#else
				ConnectorException.Throw( Errors.Unknown );
#endif
			}

			string newFolderPath = Regex.Replace( this.CurrentFolder.ClientPath, "[^/]+/?$", newFolderName ) + "/";
			string newFolderUrl = this.CurrentFolder.ResourceTypeInfo.Url + newFolderPath.TrimStart( '/' );

			XmlNode oRenamedNode = XmlUtil.AppendElement( this.ConnectorNode, "RenamedFolder" );
			XmlUtil.SetAttribute( oRenamedNode, "newName", newFolderName );
			XmlUtil.SetAttribute( oRenamedNode, "newPath", newFolderPath );
			XmlUtil.SetAttribute( oRenamedNode, "newUrl", newFolderUrl );
		}
	}
}
