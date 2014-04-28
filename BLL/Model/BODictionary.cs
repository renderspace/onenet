using System;

namespace One.Net.BLL
{
    [Serializable]
    public class BODictionaryEntry : BOInternalContent
    {
        private string keyWord = string.Empty;

        public string KeyWord { get { return keyWord; } set { keyWord = value; } }
    }
}
