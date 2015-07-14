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
using System.Collections;
using System.Text.RegularExpressions;

namespace CKFinder.Connector.CommandHandlers
{
	internal class CopyFilesCommandHandler : XmlCommandHandlerBase
	{
		private XmlNode ErrorsNode;

		public CopyFilesCommandHandler()
			: base()
		{
		}

		private void appendErrorNode( int errorCode, string name, string type, string path )
		{
			if ( this.ErrorsNode == null )
				this.ErrorsNode = XmlUtil.AppendElement( this.ConnectorNode, "Errors" );

			XmlNode Error = XmlUtil.AppendElement( this.ErrorsNode, "Error" );
			XmlUtil.SetAttribute( Error, "code", errorCode.ToString() );
			XmlUtil.SetAttribute( Error, "name", name );
			XmlUtil.SetAttribute( Error, "type", type );
			XmlUtil.SetAttribute( Error, "folder", path );
		}

		protected override void BuildXml()
		{
			if ( Request.Form["CKFinderCommand"] != "true" )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileRename | AccessControlRules.FileUpload | AccessControlRules.FileDelete ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			if ( Request.Form["files[0][type]"] == null )
			{
				ConnectorException.Throw( Errors.InvalidRequest );
			}

			int copied = 0;
			int copiedAll = 0;
			if ( Request.Form["copied"] != null )
			{
				copiedAll = Int32.Parse(Request.Form["copied"]);
			}
			Settings.ResourceType resourceType;
			Hashtable resourceTypeConfig = new Hashtable();
			Hashtable checkedPaths = new Hashtable();

			int iFileNum = 0;
			while ( Request.Form["files[" + iFileNum.ToString() + "][type]"] != null && Request.Form["files[" + iFileNum.ToString() + "][type]"].Length > 0 )
			{
				string name = Request.Form["files[" + iFileNum.ToString() + "][name]"];
				string type = Request.Form["files[" + iFileNum.ToString() + "][type]"];
				string path = Request.Form["files[" + iFileNum.ToString() + "][folder]"];
				string options = "";

				if ( name == null || name.Length < 1 || type == null || type.Length < 1 || path == null || path.Length < 1 )
				{
					ConnectorException.Throw( Errors.InvalidRequest );
					return;
				}

				if ( Request.Form["files[" + iFileNum.ToString() + "][options]"] != null )
				{
					options = Request.Form["files[" + iFileNum.ToString() + "][options]"];
				}
				iFileNum++;

				// check #1 (path)
				if ( !Connector.CheckFileName( name ) || Regex.IsMatch( path, @"(/\.)|(\.\.)|(//)|([\\:\*\?""\<\>\|])" ) )
				{
					ConnectorException.Throw( Errors.InvalidRequest );
					return;
				}

				// get resource type config for current file
				if ( !resourceTypeConfig.ContainsKey( type ) )
				{
					resourceTypeConfig[type] = Config.Current.GetResourceTypeConfig( type );
				}

				// check #2 (resource type)
				if ( resourceTypeConfig[type] == null )
				{
					ConnectorException.Throw( Errors.InvalidRequest );
					return;
				}

				resourceType = (Settings.ResourceType)resourceTypeConfig[type];
				FolderHandler folder = new FolderHandler( type, path );

				// check #3 (extension)
				if ( !resourceType.CheckExtension( System.IO.Path.GetExtension( name ) ) )
				{
					this.appendErrorNode( Errors.InvalidExtension, name, type, path );
					continue;
				}

				// check #4 (extension) - when moving to another resource type, double check extension
				if ( this.CurrentFolder.ResourceTypeName != type )
				{
					if ( !this.CurrentFolder.ResourceTypeInfo.CheckExtension( System.IO.Path.GetExtension( name ) ) )
					{
						this.appendErrorNode( Errors.InvalidExtension, name, type, path );
						continue;
					}
				}

				// check #5 (hidden folders)
				if ( !checkedPaths.ContainsKey( path ) )
				{
					checkedPaths[path] = true;
					if ( Config.Current.CheckIsHidenPath( path ) )
					{
						ConnectorException.Throw( Errors.InvalidRequest );
						return;
					}
				}

				// check #6 (hidden file name)
				if ( Config.Current.CheckIsHiddenFile( name ) )
				{
					ConnectorException.Throw( Errors.InvalidRequest );
					return;
				}

				// check #7 (Access Control, need file view permission to source files)
				if ( !folder.CheckAcl( AccessControlRules.FileView ) )
				{
					ConnectorException.Throw( Errors.Unauthorized );
					return;
				}

				// check #8 (invalid file name)
				if ( !DbFileSystem.FileExists(name) || !DbFileSystem.FolderExists(path) )
				{
					this.appendErrorNode( Errors.FileNotFound, name, type, path );
					continue;
				}

				// check #9 (max size)
				if ( this.CurrentFolder.ResourceTypeName != type )
				{
					var file = DbFileSystem.GetFile(name);
					if ( this.CurrentFolder.ResourceTypeInfo.MaxSize > 0 && file.Size > this.CurrentFolder.ResourceTypeInfo.MaxSize )
					{
						this.appendErrorNode( Errors.UploadedTooBig, name, type, path );
						continue;
					}
				}

				// check if file exists if we don't force overwriting
				if (!options.Contains( "overwrite" ) )
				{
					if ( options.Contains( "autorename" ) ) 
					{
						try
						{
							DbFileSystem.CopyFile(CurrentFolder.ClientPath, name, false);
							copied++;
						}
						catch ( ArgumentException )
						{
							this.appendErrorNode( Errors.InvalidName, name, type, path );
							continue;
						}
						catch ( Exception )
						{
#if DEBUG
							throw;
#else
							this.appendErrorNode( Errors.AccessDenied, name, type, path );
							continue;
#endif
						}
					}
					else
					{
						this.appendErrorNode( Errors.AlreadyExist, name, type, path );
						continue;
					}
				}
				else {
					try
					{
						// overwrite without warning
						DbFileSystem.CopyFile(CurrentFolder.ClientPath, name, true);
						copied++;
					}
					catch ( ArgumentException )
					{
						this.appendErrorNode( Errors.InvalidName, name, type, path );
						continue;
					}
					catch ( System.IO.PathTooLongException )
					{
						this.appendErrorNode( Errors.InvalidName, name, type, path );
						continue;
					}
					catch ( Exception )
					{
#if DEBUG
							throw;
#else
						this.appendErrorNode( Errors.AccessDenied, name, type, path );
						continue;
#endif
					}
				}
			}

			XmlNode copyFilesNode = XmlUtil.AppendElement( this.ConnectorNode, "CopyFiles" );
			XmlUtil.SetAttribute( copyFilesNode, "copied", copied.ToString() );
			XmlUtil.SetAttribute( copyFilesNode, "copiedTotal", (copiedAll + copied).ToString() );

			if ( this.ErrorsNode != null )
			{
				ConnectorException.Throw( Errors.CopyFailed );
				return;
			}
		}
	}
}
