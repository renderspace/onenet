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
using System.Text.RegularExpressions;

namespace CKFinder.Connector.CommandHandlers
{
	public abstract class CommandHandlerBase
	{
		private Connector _Connector;
		private FolderHandler _CurrentFolder;
		private HttpRequest _Request;

		public CommandHandlerBase()
		{
			_CurrentFolder = FolderHandler.GetCurrent();
			_Request = HttpContext.Current.Request;
		}

		protected HttpRequest Request
		{
			get { return _Request; }
		}

		protected Connector Connector
		{
			get
			{
				if ( _Connector == null )
					_Connector = (Connector)HttpContext.Current.Handler;
				return _Connector;
			}
		}

		public FolderHandler CurrentFolder
		{
			get
			{
				if ( _CurrentFolder == null )
					_CurrentFolder = FolderHandler.GetCurrent();
				return _CurrentFolder;
			}
		}

		protected void CheckConnector()
		{
			if ( !Config.Current.CheckAuthentication() )
				ConnectorException.Throw( Errors.ConnectorDisabled );
		}

		protected void CheckRequest()
		{
			// Check if the current folder is a valid path.
			if ( Regex.IsMatch( this.CurrentFolder.ClientPath, @"(/\.)|(\.\.)|(//)|([\\:\*\?""\<\>\|])" ) )
				ConnectorException.Throw( Errors.InvalidName );

			if ( this.CurrentFolder.ResourceTypeInfo == null )
				ConnectorException.Throw( Errors.InvalidType );
/*
			if ( !this.CurrentFolder.FolderInfo.Exists )
			{
				if ( this.CurrentFolder.ClientPath == "/" )
				{
					try
					{
						this.CurrentFolder.FolderInfo.Create();
					}
					catch ( System.UnauthorizedAccessException )
					{
						ConnectorException.Throw( Errors.AccessDenied );
					}
					catch ( System.Security.SecurityException )
					{
						ConnectorException.Throw( Errors.AccessDenied );
					}
					catch
					{
						ConnectorException.Throw( Errors.Unknown );
					}
				}
				else
					ConnectorException.Throw( Errors.FolderNotFound );
			}
*/
		}

		public abstract void SendResponse( HttpResponse response, HttpContext context );
	}
}
