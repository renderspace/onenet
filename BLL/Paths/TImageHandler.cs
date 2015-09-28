using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Configuration;
using NLog;
using One.Net.BLL.Caching;

namespace One.Net.BLL
{

    public class TImageHandler : IHttpHandler
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();
        protected static ICacheProvider cache = CacheFactory.ResolveCacheFromConfig();

        private const int DEFAULT_QUALITY = 80;

        private const string CACHE_ID = "TIH";
        private int OverlayFileId = 0;
        private int OverlayMode = 0;
        private int Width = 0;
        private int Height = 0;
        private int Quality = DEFAULT_QUALITY;
        private int Max = 0;
        private bool ByPassCache = false;
        private bool AspectRatioSize = false;
        private bool Croppable = false;
        private bool DoNotUpscale = false;
        private int ResizePercentage = 100;
        private bool IsDownload = false;

        private Color BackgroundColor = Color.FromName("White");

        private const int chunkSize = 65536;

        private static readonly object _lock = new object();

        enum AnchorPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }

        // under heavy load mulitple requests will start the reading process, then the Cache
        // inserts will invalidate cache. Better to check for existence first.
        private static readonly object cacheLockingEncodedImage = new Object();

        protected bool ResizeEnabled
        {
            get { return (Width > 0 || Height > 0 || Max > 0 || ResizePercentage != 100); }
        }

        protected bool OverlayEnabled
        {
            get { return (OverlayFileId > 0); }
        }

        protected bool EnableProcessing
        {
            get { return OverlayEnabled || ResizeEnabled || Quality != DEFAULT_QUALITY; }
        }

        private bool? enableDiskCache;

        protected bool EnableDiskCache
        {
            get
            {
                if (enableDiskCache != null)
                    return enableDiskCache.Value;
                bool temp = false;
                bool.TryParse(ConfigurationManager.AppSettings["EnableDiskCache"], out temp);
                enableDiskCache = temp;
                return temp;
            }
        }

        public bool PublishFlag
        {
            get
            {
                var publishFlag = false;
                bool.TryParse(ConfigurationManager.AppSettings["PublishFlag"], out publishFlag);
                return publishFlag;
            }
        }
        
        public void ProcessRequest(HttpContext context)
        {
            var virtualPath = context.Request.Path;

            if (context.Request.Params["h"] != null)
                int.TryParse(context.Request.Params["h"], out Height);

            if (context.Request.Params["w"] != null)
                int.TryParse(context.Request.Params["w"], out Width);
            else if (context.Request.Params["width"] != null)
                int.TryParse(context.Request.Params["width"], out Width);

            if (context.Request.Params["q"] != null)
                int.TryParse(context.Request.Params["q"], out Quality);
            if (context.Request.Params["m"] != null)
                int.TryParse(context.Request.Params["m"], out Max);
            if (context.Request.Params["o"] != null)
                int.TryParse(context.Request.Params["o"], out OverlayFileId);
            if (context.Request.Params["om"] != null)
                int.TryParse(context.Request.Params["om"], out OverlayMode);
            if (context.Request.Params["b"] != null)
                ByPassCache = FormatTool.GetBoolean(context.Request.Params["b"]);
            if (context.Request.Params["crop"] != null)
                Croppable = FormatTool.GetBoolean(context.Request.Params["crop"]);
            if (context.Request.Params["dnu"] != null)
                DoNotUpscale = FormatTool.GetBoolean(context.Request.Params["dnu"]);
            if (context.Request.Params["d"] != null)
                IsDownload = FormatTool.GetBoolean(context.Request.Params["d"]);

            if (context.Request.Params["p"] != null)
            {
                int.TryParse(context.Request.Params["o"], out ResizePercentage);
                ResizePercentage = Math.Max(Math.Min(1000, ResizePercentage), 0);
            }

            if (context.Request.Params["ars"] != null)
                AspectRatioSize = FormatTool.GetBoolean(context.Request.Params["ars"]);

            if (context.Request.Params["c"] != null)
            {
                try 
                {
                    BackgroundColor = StringTool.HexStringToColor(context.Request.Params["c"]);
                }
                catch {}
            }

            if (PublishFlag)
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(DateTime.Now.Add(new TimeSpan(28, 0, 0, 0)));
            }
            else
            {
                context.Response.Cache.SetExpires(DateTime.Now.Add(new TimeSpan(1, 0, 0)));
                context.Response.Cache.SetCacheability(HttpCacheability.Private);
                context.Response.Cache.AppendCacheExtension("post-check=900,pre-check=3600");
                context.Response.Cache.SetValidUntilExpires(false);
            }
            
            try 
            {
                if (EnableProcessing)
                    Process(context);
                else
                {   
                    // the underlying stream should (is) be cached.
                    using (Stream iStream = VirtualPathProvider.OpenFile(virtualPath))
                    {
                        WriteStream(iStream, context, virtualPath, IsDownload);
                    }
                }
            }
            catch (DirectoryNotFoundException ioex)
            {
                log.Error(ioex, "Directory not found");
                log.Debug("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 404;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">404:  Directory Not Found</h1><h2>TImageHandler</h2></body></html>");
            }
            catch (FileNotFoundException ioex)
            {
                log.Error(ioex, "File not found");
                log.Debug("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 404;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">404:  File Not Found</h1><h2>TImageHandler</h2></body></html>");
            }
            catch (Exception ex)
            {
                log.Fatal(ex, "Uknown error");
                log.Fatal("Url: " + context.Request.Url.ToString());
                log.Fatal("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 500;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">500:  Unkown internal server error.</h1><h2>TImageHandler</h2></body></html>");
            }
            context.Response.End();
        }


        // used in ckfinder connector !
        public static void ProcessRequest(string virtualPath, int width, int height, int quality)
        {
            var context = HttpContext.Current;

            //Bitmap originalBitmap = null;
            byte[] encodedImage = null;

            string cacheKey = CACHE_ID + virtualPath + "W" + width + "H" + height + "Q" + quality;

            encodedImage = cache.Get<byte[]>(cacheKey);

            if (encodedImage == null)
            {
                Bitmap processedBitmap = null;
                using (Stream iStream = VirtualPathProvider.OpenFile(virtualPath))
                {
                    if (iStream != null)
                    {
                        processedBitmap = getResizedImage(iStream, width, height, 500, Color.White, true, false, 100, false);
                        if (processedBitmap == null)
                            processedBitmap = new Bitmap(iStream);
                    }
                    // PART 2

                    if (processedBitmap != null)
                    {
                        log.Debug("resized: " + context.Request.Url.ToString());
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                        encodedImage = GetByteArray(processedBitmap, GetImageFormat(virtualPath), encParams);
                        lock (cacheLockingEncodedImage)
                        {
                            byte[] tempEncodedImage = cache.Get<byte[]>(cacheKey);
                            if (null == tempEncodedImage)
                                cache.Put(cacheKey, encodedImage);
                        }
                    }
                    else
                    {
                        log.Error("processedBitmap is NULL");
                        context.Response.StatusCode = 404;
                    }
                }
            }

            if (encodedImage != null)
            {
                using (MemoryStream iStream = new MemoryStream(encodedImage))
                {
                    WriteStream(iStream, context, virtualPath);
                    // TODO: take care of big files someday
                }
            }
            else
            {
                log.Error("encodedImage (byte[]) is NULL");
                context.Response.StatusCode = 404;
            }
        }

        //public static void ProcessRequest(string virtualPath, int width, int height, int max, int quality, Color bgColor, bool resizeEnabled, bool bypassCache, int overlayFileId, bool overlayEnabled)
        //{
        //    
        //}

        public static void WriteStream(Stream iStream, HttpContext context, string virtualPath, bool forceDownload = false)
        {
            if (iStream != null)
            {
                byte[] buffer = new Byte[chunkSize];
                if (forceDownload)
                {
                    var file = new FileInfo(virtualPath);
                    context.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", file.Name));
                }
                else
                {
                    context.Response.AddHeader("Content-Type", GetContentType(virtualPath));
                }

                //context.Response.WriteFile(file.FullName, false);
                
                iStream.Seek(0, SeekOrigin.Begin);
                long dataToRead = iStream.Length;
                while (dataToRead > 0)
                {
                    if (context.Response.IsClientConnected)
                    {
                        int length = iStream.Read(buffer, 0, chunkSize);
                        context.Response.OutputStream.Write(buffer, 0, length);
                        context.Response.Flush();
                        buffer = new Byte[chunkSize];
                        dataToRead = dataToRead - length;
                    }
                    else
                    {
                        //prevent infinite loop if user disconnects
                        dataToRead = -1;
                    }
                }
            }
            else
            {
                log.Error("iStream is NULL");
                context.Response.StatusCode = 404;
            }
        }

        public void Process(HttpContext context)
		{
            byte[] encodedImage = null;


            string cacheKey = CACHE_ID + "W" + Width + "H" + Height + "M" + Max + "BG" + BackgroundColor + "Q" + Quality + "OFD" + OverlayFileId + "DNU" + DoNotUpscale + "OM" + OverlayMode + (Croppable ? "C" : "") + context.Request.Path;

            var cachedFileInfo = new FileInfo(Path.Combine(context.Request.PhysicalApplicationPath, "_cache/" + cacheKey));

            if (!ByPassCache)
            {
                if (EnableDiskCache)
                {
                    if (cachedFileInfo.Exists)
                    {
                        if (IsDownload)
                        {
                            context.Response.AddHeader("content-disposition", string.Format("attachment; filename=\"{0}\"", cachedFileInfo.Name));
                        }
                        else
                        {
                            context.Response.ContentType = MimeMapping.GetMimeMapping(cachedFileInfo.Name);
                        }
                        context.Response.TransmitFile(cachedFileInfo.FullName);
                        return;
                    }
                }
                else
                {
                    encodedImage = cache.Get<byte[]>(cacheKey);
                }
            }
                
            
            if (encodedImage == null)
            {
                Bitmap processedBitmap = null;
                using (Stream iStream = VirtualPathProvider.OpenFile(context.Request.Path))
                {
                    if (iStream != null)
                    {
                        if (ResizeEnabled)
                            processedBitmap = getResizedImage(iStream, Width, Height, Max, BackgroundColor, AspectRatioSize, Croppable, ResizePercentage, DoNotUpscale);

                        if (processedBitmap == null)
                            processedBitmap = new Bitmap(iStream);

                        if (OverlayEnabled)
                            processedBitmap = HandleOverlayFile(OverlayFileId, processedBitmap, OverlayMode);
                    }
                    // PART 2
                    
                    if (processedBitmap != null)
                    {
                        log.Debug("resized: " + context.Request.Url.ToString());
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, Quality);
                        encodedImage = GetByteArray(processedBitmap, GetImageFormat(context.Request.Path), encParams);
                        if (!ByPassCache)
                        {
                            lock (cacheLockingEncodedImage)
                            {
                                if (EnableDiskCache)
                                {
                                    if (!cachedFileInfo.Directory.Exists)
                                        cachedFileInfo.Directory.Create();

                                    using (var memoryStream = new MemoryStream(encodedImage))
                                    {
                                        var fileLength = WriteFileStream(memoryStream, cachedFileInfo.FullName);
                                    }
                                }
                                else
                                {
                                    byte[] tempEncodedImage = cache.Get<byte[]>(cacheKey);
                                    if (null == tempEncodedImage)
                                        cache.Put(cacheKey, encodedImage);
                                }
                            }
                        }
                    }
                    else
                    {
                        log.Error("processedBitmap is NULL");
                        context.Response.StatusCode = 404;
                    }
                }
            }

            if (encodedImage != null)
            {
                using (MemoryStream iStream = new MemoryStream(encodedImage))
                {
                    WriteStream(iStream, context, context.Request.Path, IsDownload);
                    // TODO: take care of big files someday
                }
            }
            else
            {
                log.Error("encodedImage (byte[]) is NULL");
                context.Response.StatusCode = 404;
            }
		}


        internal static byte[] GetByteArray(Image originalBitmap, ImageFormat oFormat, EncoderParameters oEncoderParams)
        {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream())
            {
                originalBitmap.Save(stream, GetEncoderInfo(oFormat.Guid), oEncoderParams);
                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, (int) stream.Length);
            }

            return data;
        }

        internal static int WriteFileStream(Stream stream, string physicalPath)
        {
            var fileLength = (int)stream.Length;
            var lChunkSize = Math.Min(chunkSize, fileLength);
            var buffer = new Byte[lChunkSize];
            stream.Seek(0, SeekOrigin.Begin);
            var dataToRead = stream.Length;

            using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                while (dataToRead > 0)
                {
                    var length = stream.Read(buffer, 0, lChunkSize);
                    fileStream.Write(buffer, 0, length);
                    buffer = new Byte[lChunkSize];
                    dataToRead = dataToRead - length;
                }
            }
            return fileLength;
        }

        static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }


        private static Bitmap HandleOverlayFile(int fileId, Image originalBitmap, int overlayMode)
        {
            BFileSystem fileB = new BFileSystem();
            BOFile overlayFile = fileB.Get(fileId);
            if ( overlayFile != null)
            {
                overlayFile.File = BFileSystem.GetCachedFileBytes(fileId);
                if (overlayFile.File != null && !overlayFile.BinaryDataMissing)
                {
                    Image overlayImage = Image.FromStream(new MemoryStream(overlayFile.File), true, false);

                    // the original image
                    Bitmap baseBmp = new Bitmap((int)originalBitmap.Width, (int)originalBitmap.Height, originalBitmap.PixelFormat);
                    Graphics drawGraphic = Graphics.FromImage(baseBmp);
                    drawGraphic.DrawImage(originalBitmap, 0, 0);

                    // rotate and draw corners based on assumed orientation being Top Left
                    Point topLeftPoint = new Point(0, 0);
                    Point topRightPoint = new Point(originalBitmap.Width - overlayImage.Width, 0);
                    Point bottomLeftPoint = new Point(0, originalBitmap.Height - overlayImage.Height);
                    Point bottomRightPoint = new Point(originalBitmap.Width - overlayImage.Width, originalBitmap.Height - overlayImage.Height);

                    byte om = (byte) ~((byte)overlayMode);
                    if (IsBitSet(om, 3))
                    {
                        Bitmap topLeft = new Bitmap(overlayImage);
                        drawGraphic.DrawImage(topLeft, (int)topLeftPoint.X, (int) topLeftPoint.Y, overlayImage.Width, overlayImage.Height);
                        topLeft.Dispose();
                    }
                    if (IsBitSet(om, 2))
                    {
                        Bitmap topRight = new Bitmap(overlayImage);
                        topRight.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        drawGraphic.DrawImage(topRight, (int)topRightPoint.X, (int)topRightPoint.Y, overlayImage.Height, overlayImage.Width);
                        topRight.Dispose();
                    }

                    if (IsBitSet(om, 1))
                    {
                        Bitmap bottomLeft = new Bitmap(overlayImage);
                        bottomLeft.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        drawGraphic.DrawImage(bottomLeft, bottomLeftPoint.X, bottomLeftPoint.Y, (float)overlayImage.Height, (float)overlayImage.Width);
                        bottomLeft.Dispose();
                    }

                    if (IsBitSet(om, 0))
                    {
                        Bitmap bottomRight = new Bitmap(overlayImage);
                        bottomRight.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        drawGraphic.DrawImage(bottomRight, bottomRightPoint.X, bottomRightPoint.Y, (float)overlayImage.Width, (float)overlayImage.Height);
                        bottomRight.Dispose();
                    }
                    return baseBmp;
                }
            }

            return null;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        internal static Bitmap getResizedImage(Stream iStream, int width, int height, int max, Color BgColor, bool aspectRatioSize, bool croppable, int resizePercentage, bool doNotUpscale)
        {
            Bitmap imgIn = new Bitmap(iStream);
            double y = imgIn.Height;
            double x = imgIn.Width;
            double factor = 1;

            // If both width and height are not specified and max is specified
            // then determine which dimension of original image is bigger ( x or y )
            // and if height is bigger then assigned max to height, else
            // assigned max to width.
            if (width <= 0 && height <= 0 && max > 0)
            {
                if (x <= y)
                {
                    height = max;
                }
                else
                {
                    width = max;
                }
            }

            bool isFixedSize = false;

            if (width > 0 && height > 0)
            {
                var factorW = width / x;
                var factorH = height / y;
                factor = factorW > factorH ? factorH : factorW;
                isFixedSize = true;
            }
            else if (width > 0)
            {
                factor = width / x;
            }
            else if (height > 0)
            {
                factor = height / y;
            }
            else if (resizePercentage != 100)
            {
                factor = ((double)resizePercentage) / ((double)100);
            }

            if (aspectRatioSize)
                isFixedSize = false;

            int finalWidth = isFixedSize ? width : (int) (x*factor);
            int finalHeight = isFixedSize ? height : (int)(y * factor);
            
            var upscaleIssue = doNotUpscale && (finalHeight > y || finalWidth > x);
            if (upscaleIssue)
            {
                finalHeight = (int)y;
                finalWidth = (int)x;
                factor = 1;
            }

            Bitmap imgOut = null;
            if (croppable && !upscaleIssue)
            {
                imgOut = new Bitmap(Crop(imgIn, width, height, AnchorPosition.Center));
            }
            else
                imgOut = new Bitmap(finalWidth, finalHeight);


            Graphics g = Graphics.FromImage(imgOut);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (isFixedSize && !aspectRatioSize)
            {
                g.FillRectangle(new SolidBrush(BgColor), 0, 0, finalWidth, finalHeight);
            }

            int startY = isFixedSize ? ((height - (int)(y*factor)) / 2) : 0;
            int startX = isFixedSize ? ((width - (int) (x*factor))/2) : 0;

            g.DrawImage(imgIn, new Rectangle(startX, startY, (int)(x * factor), (int)(y * factor)), new Rectangle(0, 0, (int)x, (int)y), GraphicsUnit.Pixel);  
            return imgOut;
        }

        static Image Crop(Image imgPhoto, int width, int height, AnchorPosition anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)width / (float)sourceWidth);
            nPercentH = ((float)height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;
                    case AnchorPosition.Bottom:
                        destY = (int)(height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (anchor)
                {
                    case AnchorPosition.Left:
                        destX = 0;
                        break;
                    case AnchorPosition.Right:
                        destX = (int)(width - (sourceWidth * nPercent));
                        break;
                    default:
                        destX = (int)((width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static ImageCodecInfo GetEncoderInfo(Guid fType)
        {
            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatID == fType)
                {
                    return info;
                }
            }
            // GIF
            return ImageCodecInfo.GetImageEncoders()[2];
        }

        private static string GetContentType(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return "Image/bmp";
                case ".gif": return "Image/gif";
                case ".jpg": return "Image/jpeg";
                case ".png": return "Image/png";
                default: break;
            }
            return "";
        }

        internal static ImageFormat GetImageFormat(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg": return ImageFormat.Jpeg;
                case ".png": return ImageFormat.Png;
                default: break;
            }
            return ImageFormat.Jpeg;
        }
    }
}
