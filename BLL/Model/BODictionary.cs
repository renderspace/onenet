using System;

namespace One.Net.BLL
{
    [Serializable]
    public class BODictionaryEntry : BOInternalContent
    {
        private string keyWord = "";

        public string KeyWord { get { return keyWord; } set { keyWord = value; } }
    }
}
