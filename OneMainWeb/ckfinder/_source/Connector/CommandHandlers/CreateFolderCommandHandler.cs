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
	internal class CreateFolderCommandHandler : XmlCommandHandlerBase
	{
		public CreateFolderCommandHandler()
			: base()
		{
		}

		protected override void BuildXml()
		{
			if ( Request.Form["CKFinderCommand"] != "true" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FolderCreate ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			string sNewFolderName = HttpContext.Current.Request.QueryString[ "newFolderName" ];

			if ( !Connector.CheckFileName( sNewFolderName ) || Config.Current.CheckIsHiddenFolder( sNewFolderName ) )
				ConnectorException.Throw( Errors.InvalidName );
			else
			{
				bool bCreated = false;

				if ( DbFileSystem.FolderExists(this.CurrentFolder.ClientPath, sNewFolderName) )
					ConnectorException.Throw( Errors.AlreadyExist );

				try
				{
					DbFileSystem.CreateFolder(this.CurrentFolder.ClientPath, sNewFolderName);
					bCreated = true;
				}
				catch ( ArgumentException )
				{
					ConnectorException.Throw( Errors.InvalidName );
				}
				catch ( System.IO.PathTooLongException )
				{
					ConnectorException.Throw( Errors.InvalidName );
				}
				catch ( System.Security.SecurityException )
				{
					ConnectorException.Throw( Errors.AccessDenied );
				}
				catch ( ConnectorException connectorException )
				{
					throw connectorException;
				}
				catch ( Exception )
				{
#if DEBUG
					throw;
#else
					ConnectorException.Throw( Errors.Unknown );
#endif
				}

				if ( bCreated )
				{
					XmlNode oNewFolderNode = XmlUtil.AppendElement( this.ConnectorNode, "NewFolder" );
					XmlUtil.SetAttribute( oNewFolderNode, "name", sNewFolderName );
				}
			}
		}
	}
}
