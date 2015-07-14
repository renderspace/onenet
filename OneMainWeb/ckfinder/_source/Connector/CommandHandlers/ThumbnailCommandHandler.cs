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
using System.Drawing;
using One.Net.BLL;
using One.Net.BLL.Utility;

namespace CKFinder.Connector.CommandHandlers
{
	internal class ThumbnailCommandHandler : CommandHandlerBase
	{
		public ThumbnailCommandHandler() : base() { }

        public override void SendResponse(System.Web.HttpResponse response, HttpContext context)
		{
			this.CheckConnector();

			try
			{
				this.CheckRequest();
			}
			catch ( ConnectorException connectorException )
			{
				response.AddHeader( "X-CKFinder-Error", ( connectorException.Number ).ToString() );
				response.StatusCode = 403;
				response.End();
				return;
			}
			catch
			{
				response.AddHeader( "X-CKFinder-Error", ( (int)Errors.Unknown ).ToString() );
				response.StatusCode = 403;
				response.End();
				return;
			}

			if ( !Config.Current.Thumbnails.Enabled )
			{
				response.AddHeader( "X-CKFinder-Error", ((int)Errors.ThumbnailsDisabled).ToString() );
				response.StatusCode = 403;
				response.End();
				return;
			}

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileView ) )
			{
				response.AddHeader( "X-CKFinder-Error", ( (int)Errors.Unauthorized ).ToString() );
				response.StatusCode = 403;
				response.End();
				return;
			}

			bool is304 = false;

			string fileName = HttpContext.Current.Request[ "FileName" ];

			if ( !Connector.CheckFileName( fileName ) )
			{
				response.AddHeader( "X-CKFinder-Error", ( (int)Errors.InvalidRequest ).ToString() );
				response.StatusCode = 403;
				response.End();
				return;
			}

			if ( Config.Current.CheckIsHiddenFile( fileName ) )
			{
				response.AddHeader( "X-CKFinder-Error", ( (int)Errors.FileNotFound ).ToString() + " - Hidden folder" );
				response.StatusCode = 404;
				response.End();
				return;
			}

			var file = DbFileSystem.GetFile(fileName);
			
			string eTag = (file.Modified.HasValue ? file.Modified.Value : file.Created).Ticks.ToString( "X" ) + "-" + file.Size.ToString( "X" );

			string chachedETag = Request.ServerVariables[ "HTTP_IF_NONE_MATCH" ];
			if ( chachedETag != null && chachedETag.Length > 0 && eTag == chachedETag )
			{
				is304 = true ;
			}

			if ( !is304 )
			{
				string cachedTimeStr = Request.ServerVariables[ "HTTP_IF_MODIFIED_SINCE" ];
				if ( cachedTimeStr != null && cachedTimeStr.Length > 0 )
				{
					try
					{
						DateTime cachedTime = DateTime.Parse( cachedTimeStr );

						if ( cachedTime >= (file.Modified.HasValue ? file.Modified.Value : file.Created) )
							is304 = true;
					}
					catch
					{
						is304 = false;
					}
				}
			}

			if ( is304 )
			{
				response.StatusCode = 304;
				response.End();
				return;
			}

			if ( fileName.EndsWith(".jpg"))
				response.ContentType = "image/jpeg";
			else
				response.ContentType = "image/" + System.IO.Path.GetExtension(fileName);

			response.Cache.SetETag( eTag );
			response.Cache.SetLastModified( file.Modified.HasValue ? file.Modified.Value : file.Created );
			response.Cache.SetCacheability( HttpCacheability.Private );

            var urlBuilder = new UrlBuilder(Request.Url);
            urlBuilder.Path = fileName;
            urlBuilder.QueryString["w"] = Config.Current.Thumbnails.MaxWidth.ToString();
            urlBuilder.QueryString["h"] = Config.Current.Thumbnails.MaxHeight.ToString();
            urlBuilder.QueryString["q"] = Config.Current.Thumbnails.Quality.ToString();

			DbFileSystem.ProcessImage(fileName, Config.Current.Thumbnails.MaxWidth, Config.Current.Thumbnails.MaxHeight, Config.Current.Thumbnails.Quality);
		}
	}
}