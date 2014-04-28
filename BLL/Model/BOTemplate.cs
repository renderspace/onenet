using System;
using System.IO;
using System.Text;

namespace One.Net.BLL
{
    [Serializable]
    public class BOTemplate
    {
        private int? id;// = -1;
        private string name = string.Empty;
        private string templateType = string.Empty;
        private string templateContent = string.Empty;

        [NonSerialized]
        private string physicalApplicationPath = string.Empty;

        [NonSerialized]
        private string extension = string.Empty;

        [NonSerialized]
        private string templateSourceDirectory = string.Empty;

        public BOTemplate()
        { }

        public int? Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /*
        private byte[] source;
        
        public byte[] Source
        {
            get { return source; }
            set { source = value; }
        }*/

        public byte[] Source
        {
            get { return Encoding.ASCII.GetBytes(TemplateContent); }
        }

        public string Type
        {
            get { return templateType; }
            set { templateType = value; }
        }

        public string TemplateContent
        {
            get { return templateContent; }
            set { templateContent = value; }
        }

        public string BuildPath(string absolutePath)
        {
            if (!absolutePath.EndsWith("/"))
                absolutePath += "/";
            return absolutePath + Name + "." + Extension;
        }

        public string FilePath
        {
            get
            {
                return PhysicalApplicationPath.TrimEnd(Path.DirectorySeparatorChar) +
              Path.DirectorySeparatorChar + TemplateSourceDirectory.TrimEnd(Path.DirectorySeparatorChar) +
              Path.DirectorySeparatorChar + Name + "." + extension.TrimStart('.');
            }
        }

        public string PhysicalApplicationPath
        {
            get { return physicalApplicationPath; }
            set { physicalApplicationPath = value; }
        }

        public string Extension
        {
            get { return extension; }
            set { extension = value; }
        }

        public string TemplateSourceDirectory
        {
            get { return templateSourceDirectory; }
            set { templateSourceDirectory = value; }
        }
    }
}
