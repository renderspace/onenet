using System;
using System.Web;

namespace One.Net.BLL
{
    public enum FileBelongsTo { Person = 1, Photogallery }

	[Serializable]
    public class BOFile : ICategorizable
    {
        public const string FOLDER_CATEGORIZATION_TYPE = "tree_file_folder";

        #region Properties

        public BOFile() { }

        public BOInternalContent Content { get; set; }

	    public bool IsImage
	    {
            get { return MimeType.Contains("image"); }
	    }

        public string RelativeUrl
        {
            get 
            {
                return "/_files/" + Id.ToString() + "/" + Name;
            }
        }

        public string Alt
        {
            get
            {
                return Content != null ? Content.Title : Name;

            }
        }

        public string EncodedAlt
        {
            get
            {
                return HttpUtility.HtmlEncode(Alt);
            }
        }



        public int? Id { get; set; }

        public int Idx { get; set; }

        public int? ContentId { get { return (Content != null ? Content.ContentId : null); } }

        /// <summary>
        /// File name, excluding extension
        /// </summary>
        public string Name { get; set; }

        public string Extension { get; set; }

        public byte[] File { get; set; }

        /// <summary>
        /// Size of file byte[]
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Folder representation
        /// </summary>
        public BOCategory Folder { get; set; }

        public string MimeType { get; set; }

        /// <summary>
        /// Determines whether allocated Size property equals the actual size of the byte[] File.
        /// </summary>
        public bool BinaryDataMissing { get { return (File == null || Size != File.Length || Size == 0); } }

        public string Title
        {
            get { return (Content != null ? Content.Title : ""); }
        }

        public override string ToString()
        {
            return string.Format("File {0}{1} data {2} ", (Id.HasValue ? "[" + Id.Value + "]" : "[never saved]"), (BinaryDataMissing ? " without" : " with"), (Name ?? ""));
        }

		public DateTime Created { get; set; }
		public DateTime? Modified { get; set; }

        #endregion Properties
    }
}
