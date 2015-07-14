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

namespace CKFinder.Settings
{
	public class ResourceType
	{
		internal string Name;

		private string _Url;
		public string Dir;
		public int MaxSize;
		public string[] AllowedExtensions;
		public string[] DeniedExtensions;

		internal ResourceType( string name )
		{
			Name = name;

			Url = "";
			Dir = "";
			MaxSize = 0;
			AllowedExtensions = new string[ 0 ];
			DeniedExtensions = new string[ 0 ];
		}

		public string Url
		{
			get
			{
				return _Url;
			}
			set
			{
				_Url = value;

				if ( _Url.StartsWith( "~" ) )
					_Url = ( (System.Web.UI.Page)System.Web.HttpContext.Current.Handler ).ResolveUrl( _Url );

				if ( !_Url.EndsWith( "/" ) )
					_Url += "/";
			}
		}

		public bool CheckExtension( string extension )
		{
			if ( extension.Length == 0 )
				return true;

			extension = extension.TrimStart( '.' ).ToLower();

			if ( DeniedExtensions.Length > 0 )
			{
				if ( Array.IndexOf( this.DeniedExtensions, extension ) >= 0 )
					return false;
			}

			if ( AllowedExtensions.Length > 0 )
				return ( Array.IndexOf( this.AllowedExtensions, extension ) >= 0 ) ;
			else
				return true;
		}

		public string GetTargetDirectory()
		{
			if ( Dir.Length == 0 )
				return System.Web.HttpContext.Current.Server.MapPath( Url );
			else
				return Dir;
		}
	}
}
