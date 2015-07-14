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
	internal class RenameFileCommandHandler : XmlCommandHandlerBase
	{
		public RenameFileCommandHandler()
			: base()
		{
		}

		protected override void BuildXml()
		{
			if ( Request.Form["CKFinderCommand"] != "true" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileRename ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			string fileName = Request["fileName"];
			string newFileName = Request["newFileName"];

			XmlNode oRenamedFileNode = XmlUtil.AppendElement( this.ConnectorNode, "RenamedFile" );
			XmlUtil.SetAttribute( oRenamedFileNode, "name", fileName );

			if ( !Connector.CheckFileName( fileName ) || Config.Current.CheckIsHiddenFile( fileName ) )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
				return;
			}

			if ( !this.CurrentFolder.ResourceTypeInfo.CheckExtension( System.IO.Path.GetExtension( fileName ) ) )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
				return;
			}

			if ( !Connector.CheckFileName( newFileName ) || Config.Current.CheckIsHiddenFile( newFileName ) )
			{
				ConnectorException.Throw( Errors.InvalidName );
				return;
			}

			if ( !this.CurrentFolder.ResourceTypeInfo.CheckExtension( System.IO.Path.GetExtension( newFileName ) ) )
				ConnectorException.Throw( Errors.InvalidExtension );

			if ( !DbFileSystem.FileExists( fileName ) )
				ConnectorException.Throw( Errors.FileNotFound );

			try
			{
				DbFileSystem.RenameFile(fileName, newFileName);
				XmlUtil.SetAttribute( oRenamedFileNode, "newName", newFileName );
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
			catch
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
