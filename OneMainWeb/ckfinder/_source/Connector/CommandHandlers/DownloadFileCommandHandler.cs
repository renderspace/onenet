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
using System.Drawing;
using System.IO;

namespace CKFinder.Connector.CommandHandlers
{
	internal class DownloadFileCommandHandler : CommandHandlerBase
	{
		public DownloadFileCommandHandler()
			: base()
		{
		}

        public override void SendResponse(HttpResponse response, HttpContext context)
		{
			this.CheckConnector();

			response.ClearHeaders();
			response.Clear();

			response.ContentType = "application/octet-stream";

			try
			{
				this.CheckRequest();
			}
			catch ( ConnectorException connectorException )
			{
				response.AddHeader( "X-CKFinder-Error", ( connectorException.Number ).ToString() );

				if ( connectorException.Number == Errors.FolderNotFound )
					response.StatusCode = 404;
				else
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

			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileView ) )
			{
				response.StatusCode = 403;
				response.End();
				return;
			}

			var virtualPath = HttpContext.Current.Request["FileName"];
			var arrFileName = virtualPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			var fileName = virtualPath;
			if (arrFileName.Length == 3)
			{
				fileName = arrFileName[arrFileName.Length - 1];
			}
			var extension = System.IO.Path.GetExtension( virtualPath );
			if ( !this.CurrentFolder.ResourceTypeInfo.CheckExtension( extension) )
			{
				response.StatusCode = 403;
				response.End();
				return;
			}

			if ( Request["format"] == "text" )
			{
				response.AddHeader( "Content-Type", "text/plain; charset=utf-8" );
			}
			else
			{
				response.AddHeader( "Content-Disposition", "attachment; filename=\"" + fileName + "\"" );
			}

			if (ImageTools.IsImageExtension(extension))
			{
			//	DbFileSystem.ProcessImage(virtualPath, 0, 0, Config.Current.Images.Quality);
			}
			else
			{
				using (var memoryStream = new MemoryStream(DbFileSystem.GetFileBytes(virtualPath)))
				{
					var aBuffer = new byte[10000];
					var iDataToRead = memoryStream.Length;
					while ( iDataToRead > 0 )
					{
						if ( response.IsClientConnected )
						{
							var iLength = memoryStream.Read( aBuffer, 0, 10000 );
							response.OutputStream.Write( aBuffer, 0, iLength );
							response.Flush();
							aBuffer = new Byte[10000];
							iDataToRead = iDataToRead - iLength;
						}
						else
						{
							iDataToRead = -1;
						}
					}
				}
			}
			response.End();
		}
	}
}
