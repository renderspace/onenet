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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CKFinder.Connector
{
	public class FolderHandler
	{
		private Settings.ResourceType _ResourceTypeInfo;

		private string _ResourceTypeName;
		private string _ClientPath;
		private string _Url;
		private string _ServerPath;
		private string _ThumbsServerPath;
		private int _AclMask;

		public FolderHandler( string resourceTypeName, string clientFolderPath )
		{
			_ResourceTypeName = resourceTypeName;

			// ## ClientPath
			_ClientPath = clientFolderPath;

			// Check the current folder syntax (must begin and start with a slash).
			if ( !_ClientPath.EndsWith( "/" ) )
				_ClientPath += "/";
			if ( !_ClientPath.StartsWith( "/" ) )
				_ClientPath = "/" + _ClientPath;

			_AclMask = -1;
		}

		public static FolderHandler GetCurrent()
		{
			System.Web.HttpRequest _Request = System.Web.HttpContext.Current.Request;

			string _ResourceType = _Request.QueryString[ "type" ];
			if ( _ResourceType == null )
				_ResourceType = "";

			string _CurrentFolder = _Request.QueryString[ "currentFolder" ];
			if ( _CurrentFolder == null )
			{
				_CurrentFolder = "/";
			}
			return new FolderHandler( _ResourceType, _CurrentFolder );
		}

		public Settings.ResourceType ResourceTypeInfo
		{
			get
			{
				if ( _ResourceTypeInfo == null )
				{
					_ResourceTypeInfo = Config.Current.GetResourceTypeConfig( this.ResourceTypeName );
					if ( _ResourceTypeInfo == null )
						ConnectorException.Throw( Errors.InvalidType );
				}
				return _ResourceTypeInfo;
			}
		}

		public string ResourceTypeName
		{
			get { return _ResourceTypeName; }
		}

		public string ClientPath
		{
			get { return _ClientPath; }
		}

		public string Url
		{
			get { return string.Empty; }
		}

		public string ServerPath
		{
			get
			{
				return "";
				var request = HttpContext.Current.Request;
				if (string.IsNullOrEmpty(_ServerPath))
				{
					_ServerPath = !request.Url.AbsoluteUri.EndsWith("/") ? request.Url.AbsoluteUri.Replace(request.Url.PathAndQuery, "") : request.Url.AbsoluteUri;
					if (_ServerPath.EndsWith("/"))
						_ServerPath = _ServerPath.Remove(_ServerPath.LastIndexOf("/"));
				}
				return _ServerPath;
			}
		}

		public string ThumbsServerPath
		{
			get { return string.Empty; }
		}

		public int AclMask
		{
			get
			{
				if ( _AclMask == -1 )
					_AclMask = Config.Current.AccessControl.GetComputedMask( this.ResourceTypeName, this.ClientPath );
				return _AclMask;
			}
		}

		public bool CheckAcl( int aclToCheck )
		{
			return ( ( this.AclMask & aclToCheck ) == aclToCheck );
		}

		public bool CheckAcl( AccessControlRules aclToCheck )
		{
			return this.CheckAcl( (int)aclToCheck );
		}
	}
}
