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
	internal class GetFilesCommandHandler : XmlCommandHandlerBase
	{
		public GetFilesCommandHandler() : base()
		{
		}

		protected override void BuildXml()
		{
			if ( !this.CurrentFolder.CheckAcl( AccessControlRules.FileView ) )
			{
				ConnectorException.Throw( Errors.Unauthorized );
			}

			var selectedFolder = DbFileSystem.GetFolder(this.CurrentFolder.ClientPath);
			var showThumbs = Request.QueryString["showThumbs"] != null && Request.QueryString["showThumbs"].ToString().Equals("1");

			// Create the "Files" node.
			XmlNode oFilesNode = XmlUtil.AppendElement(this.ConnectorNode, "Files");

			if (selectedFolder == null) return;

			var fileList = DbFileSystem.ListFiles(selectedFolder.Id.Value);
			foreach (var file in fileList)
			{
				string sExtension = file.Extension;
				if (!this.CurrentFolder.ResourceTypeInfo.CheckExtension(sExtension)) continue;

				var iFileSize = Math.Round( (Decimal)file.Size / 1024 ) ;
				if (iFileSize < 1 && file.Size != 0) iFileSize = 1 ;

				// Create the "File" node.
				var fileNode = XmlUtil.AppendElement(oFilesNode, "File");
				XmlUtil.SetAttribute(fileNode, "name", "/_files/" + file.Id + "/" + file.Name);
				XmlUtil.SetAttribute(fileNode, "date", (file.Modified.HasValue ? file.Modified.Value : file.Created).ToString("yyyyMMddHHmm"));
				if (Config.Current.Thumbnails.Enabled && (Config.Current.Thumbnails.DirectAccess || showThumbs) && ImageTools.IsImageExtension(sExtension.TrimStart( '.' )))
				{
					XmlUtil.SetAttribute(fileNode, "thumb", file.Name);
					XmlUtil.SetAttribute(fileNode, "thumb", "?" + file.Name);
				}
				XmlUtil.SetAttribute(fileNode, "size", iFileSize.ToString(CultureInfo.InvariantCulture ));
			}
		}
	}
}