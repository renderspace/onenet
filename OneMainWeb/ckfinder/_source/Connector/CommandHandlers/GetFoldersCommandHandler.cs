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
using System.Xml;
using System.Globalization;

namespace CKFinder.Connector.CommandHandlers
{
	internal class GetFoldersCommandHandler : XmlCommandHandlerBase
	{
		public GetFoldersCommandHandler() : base() { }

		protected override void BuildXml()
		{
			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FolderView ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			// Create the "Folders" node.
			XmlNode oFoldersNode = XmlUtil.AppendElement( this.ConnectorNode, "Folders" );

			var currFolder = this.CurrentFolder;
			var folderId = DbFileSystem.GetFolder(currFolder.ClientPath).Id.Value;
			var folderList = DbFileSystem.ListChildFolders(folderId);
			for ( int i = 0 ; i < folderList.Count ; i++ )
			{
				var folder = folderList[i];
				string sSubDirName = folder.Title;
				int aclMask = Config.Current.AccessControl.GetComputedMask( this.CurrentFolder.ResourceTypeName, folder.Id + "/" );
				if ( ( aclMask & (int)AccessControlRules.FolderView ) != (int)AccessControlRules.FolderView )
					continue;

				// Create the "Folders" node.
				XmlNode oFolderNode = XmlUtil.AppendElement( oFoldersNode, "Folder" );
				XmlUtil.SetAttribute( oFolderNode, "name", sSubDirName );
				try
				{
					XmlUtil.SetAttribute( oFolderNode, "hasChildren", folder.ChildCount > 0 ? "true" : "false" );
				}
				catch
				{
					// It was not possible to verify if it has children. Assume "yes".
					XmlUtil.SetAttribute( oFolderNode, "hasChildren", "false" );
				}
				XmlUtil.SetAttribute( oFolderNode, "acl", aclMask.ToString() );
			}
		}
	}
}