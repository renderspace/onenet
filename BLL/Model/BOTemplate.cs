using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class BOTemplate
    {
        [NonSerialized]
        private string extension = "";

        public BOTemplate()
        { }

        public int? Id { get; set; }

        public string Name { get; set; }

        public byte[] Source
        {
            get { return Encoding.ASCII.GetBytes(TemplateContent); }
        }

        public string Type { get; set; }

        public string TemplateContent { get; set; }

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

        public string PhysicalApplicationPath { get; set; }

        public string Extension { get; set; }

        public string TemplateSourceDirectory { get; set; }
    }
}
