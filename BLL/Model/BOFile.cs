using System;

namespace One.Net.BLL
{
    public enum FileBelongsTo { Person = 1, Photogallery }

	[Serializable]
    public class BOFile : ICategorizable
    {
        #region Constants

        public const string FOLDER_CATEGORIZATION_TYPE = "tree_file_folder";

        #endregion Constants

        #region Variables

        private BOInternalContent content = null;
        private BOCategory folder = null;
        private int? id = null;
        private int idx;
        private int size = 0;
        private string name = string.Empty;
        private string extension = string.Empty;
        private string mimeType = string.Empty;
        private byte[] file = null;

        #endregion Variables

        #region Properties

        public BOFile() { }

        public BOInternalContent Content
        {
            get { return content; }
            set { content = value; }
        }

	    public bool IsImage
	    {
            get { return MimeType.Contains("image"); }
	    }


        /// <summary>
        /// Internal file id
        /// </summary>
        public int? Id { get { return id; } set { id = value; } }

        public int Idx { get { return idx; } set { idx = value; } }

        public int? ContentId { get { return (content != null ? content.ContentId : null); } }

        /// <summary>
        /// File name, excluding extension
        /// </summary>
        public string Name { get { return name; } set { name = value; } }

        public string Extension { get { return extension; } set { extension = value; } }

        public byte[] File { get { return file; } set { file = value; } }

        /// <summary>
        /// Size of file byte[]
        /// </summary>
        public int Size { get { return size; } set { size = value; } }
        
        /// <summary>
        /// Folder representation
        /// </summary>
        public BOCategory Folder { get { return folder; } set { folder = value; } }

        public string MimeType { get { return mimeType; } set { mimeType = value; } }

        /// <summary>
        /// Determines whether allocated Size property equals the actual size of the byte[] File.
        /// </summary>
        public bool BinaryDataMissing { get { return (file == null || size != file.Length || size == 0); } }

        public string Title
        {
            get { return (content != null ? content.Title : string.Empty); }
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
