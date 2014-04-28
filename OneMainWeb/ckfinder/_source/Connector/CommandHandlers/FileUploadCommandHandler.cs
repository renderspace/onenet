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
	internal class FileUploadCommandHandler : CommandHandlerBase
	{
		public FileUploadCommandHandler()
			: base()
		{
		}

        public override void SendResponse(System.Web.HttpResponse response, HttpContext context)
		{
			int iErrorNumber = 0;
			string sFileName = "";
			string sFilePath = "";
			string sUnsafeFileName = "";

			try
			{
				this.CheckConnector();
				this.CheckRequest();

				if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileUpload ) )
				{
					ConnectorException.Throw( Errors.Unauthorized );
				}

				HttpPostedFile oFile = HttpContext.Current.Request.Files[HttpContext.Current.Request.Files.AllKeys[0]];

				if ( oFile != null )
				{
					sUnsafeFileName = System.IO.Path.GetFileName(oFile.FileName);
					sFileName = Regex.Replace( sUnsafeFileName, @"[\:\*\?\|\/]", "_", RegexOptions.None );
					if ( sFileName != sUnsafeFileName )
						iErrorNumber = Errors.UploadedInvalidNameRenamed ;

					if ( Connector.CheckFileName( sFileName ) && !Config.Current.CheckIsHiddenFile( sFileName ) )
					{
						// Replace dots in the name with underscores (only one dot can be there... security issue).
						if ( Config.Current.ForceSingleExtension )
							sFileName = Regex.Replace( sFileName, @"\.(?![^.]*$)", "_", RegexOptions.None );

						if ( !Config.Current.CheckSizeAfterScaling && this.CurrentFolder.ResourceTypeInfo.MaxSize > 0 && oFile.ContentLength > this.CurrentFolder.ResourceTypeInfo.MaxSize )
							ConnectorException.Throw( Errors.UploadedTooBig );

						string sExtension = System.IO.Path.GetExtension( oFile.FileName );
						sExtension = sExtension.TrimStart( '.' );

						if ( !this.CurrentFolder.ResourceTypeInfo.CheckExtension( sExtension ) )
							ConnectorException.Throw( Errors.InvalidExtension );

						if ( Config.Current.CheckIsNonHtmlExtension( sExtension ) && !this.CheckNonHtmlFile( oFile ) )
							ConnectorException.Throw( Errors.UploadedWrongHtmlFile );
						DbFileSystem.AddFile(this.CurrentFolder.ClientPath, sFileName, oFile);
					}
					else
						ConnectorException.Throw( Errors.InvalidName );
				}
				else
					ConnectorException.Throw( Errors.UploadedCorrupt );
			}
			catch ( ConnectorException connectorException )
			{
				iErrorNumber = connectorException.Number;
			}
			catch ( System.Security.SecurityException )
			{
#if DEBUG
				throw;
#else
				iErrorNumber = Errors.AccessDenied;
#endif
			}
			catch ( System.UnauthorizedAccessException )
			{
#if DEBUG
				throw;
#else
				iErrorNumber = Errors.AccessDenied;
#endif
			}
			catch
			{
#if DEBUG
				throw;
#else
				iErrorNumber = Errors.Unknown;
#endif
			}

#if DEBUG
			if ( iErrorNumber == Errors.None || iErrorNumber == Errors.UploadedFileRenamed || iErrorNumber == Errors.UploadedInvalidNameRenamed )
				response.Clear();
#else
			response.Clear();
#endif
			response.Write( "<script type=\"text/javascript\">" );
			response.Write( this.GetJavaScriptCode( iErrorNumber, sFileName, this.CurrentFolder.Url + sFileName ) );
			response.Write( "</script>" );

			response.End();

			Connector.CKFinderEvent.ActivateEvent( CKFinderEvent.Hooks.AfterFileUpload, this.CurrentFolder, sFilePath );
		}

		protected virtual string GetJavaScriptCode( int errorNumber, string fileName, string fileUrl )
		{
			System.Web.HttpRequest _Request = System.Web.HttpContext.Current.Request;
			string _funcNum = _Request.QueryString["CKFinderFuncNum"];
			string _errorMsg = "";

			if ( errorNumber > 0 )
			{
				_errorMsg = Lang.getErrorMessage( errorNumber ).Replace( "%1", fileName );
				if ( errorNumber != Errors.UploadedFileRenamed && errorNumber != Errors.UploadedInvalidNameRenamed )
					fileName = "";
			}
			if ( _funcNum != null )
			{
				_funcNum = Regex.Replace( _funcNum, @"[^0-9]", "", RegexOptions.None );
				if ( errorNumber > 0 )
				{
					_errorMsg = Lang.getErrorMessage( errorNumber ).Replace( "%1", fileName );
					if ( errorNumber != Errors.UploadedFileRenamed )
						fileUrl = "";
				}
				return "window.parent.CKFinder.tools.callFunction(" + _funcNum + ",'" + fileUrl.Replace( "'", "\\'" ) + "','" + _errorMsg.Replace( "'", "\\'" ) + "') ;";
			}
			else
			{
				return "window.parent.OnUploadCompleted('" + fileName.Replace( "'", "\\'" ) + "','" + _errorMsg.Replace( "'", "\\'" ) + "') ;";
			}
		}

		private bool CheckNonHtmlFile( HttpPostedFile file )
		{
			byte[] buffer = new byte[ 1024 ];
			file.InputStream.Read( buffer, 0, 1024 );

			string firstKB = System.Text.ASCIIEncoding.ASCII.GetString( buffer );

			if ( Regex.IsMatch( firstKB, @"<!DOCTYPE\W*X?HTML", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			if ( Regex.IsMatch( firstKB, @"<(?:body|head|html|img|pre|script|table|title)", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//type = javascript
			if ( Regex.IsMatch( firstKB, @"type\s*=\s*[\'""]?\s*(?:\w*/)?(?:ecma|java)", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//href = javascript
			//src = javascript
			//data = javascript
			if ( Regex.IsMatch( firstKB, @"(?:href|src|data)\s*=\s*[\'""]?\s*(?:ecma|java)script:", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			//url(javascript
			if ( Regex.IsMatch( firstKB, @"url\s*\(\s*[\'""]?\s*(?:ecma|java)script:", RegexOptions.IgnoreCase | RegexOptions.Singleline ) )
				return false;

			return true;
		}
	}
}
