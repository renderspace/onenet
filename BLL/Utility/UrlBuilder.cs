using System;
using System.Web;
using System.Collections.Specialized;

namespace One.Net.BLL.Utility
{
    public class UrlBuilder : UriBuilder
    {
        #region Fields
        QueryStringDictionary _queryString = null;
        IQueryStringEncoder _encoder = new DefaultEncoder();
        #endregion

        #region Properties
        public QueryStringDictionary QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    _queryString = new QueryStringDictionary();
                }

                return _queryString;
            }
        }

        /// <summary>
        /// Gets or sets the name of the page.
        /// </summary>
        public string PageName
        {
            get
            {
                string path = base.Path;
                return path.Substring(path.LastIndexOf("/") + 1);
            }
            set
            {
                string path = base.Path;
                path = path.Substring(0, path.LastIndexOf("/"));
                base.Path = string.Concat(path, "/", value);
            }
        }
        #endregion

        #region Constructor overloads
        public UrlBuilder(System.Web.UI.Page page)
            : base(page.Request.Url)
        {
            Initialise();
        }

        public UrlBuilder(System.Web.UI.Page page, IQueryStringEncoder encoder)
            : base(page.Request.Url)
        {
            _encoder = encoder;
            Initialise();
        }

        public UrlBuilder(string uri)
            : base(uri)
        {
            Initialise();
        }

        public UrlBuilder(string uri, IQueryStringEncoder encoder)
            : base(uri)
        {
            _encoder = encoder;
            Initialise();
        }

        public UrlBuilder(Uri uri)
            : base(uri)
        {
            Initialise();
        }

        public UrlBuilder(Uri uri, IQueryStringEncoder encoder)
            : base(uri)
        {
            _encoder = encoder;
            Initialise();
        }

        public UrlBuilder(string scheme, string host)
            : base(scheme, host)
        {
        }

        public UrlBuilder(string scheme, string host, int port)
            : base(scheme, host, port)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string path)
            : base(scheme, host, port, path)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string path, string extra)
            : base(scheme, host, port, path, extra)
        {
        }

        public UrlBuilder()
            : base()
        {
        }

        // Exchange the above constructor for this one if you wish to use the current page Uri by default.
        // Effectively, this is the same as calling: new UrlBuilder(this);
        //public UrlBuilder() : base(((System.Web.UI.Page)HttpContext.Current.Handler).Request.Url) {
        //}
        #endregion

        #region Public methods
        public void Navigate()
        {
            _Navigate(true);
        }

        public void Navigate(bool endResponse)
        {
            _Navigate(endResponse);
        }

        private void _Navigate(bool endResponse)
        {
            string uri = this.ToString();
            HttpContext.Current.Response.Redirect(uri, endResponse);
        }

        /// <summary>
        /// Format options:
        ///	"e" is the default and returns the string encoded using the specified encoder, or the default encoder if none specified.
        ///	"p" always returns the string as plaintext.
        /// </summary>
        /// <returns>A string representation of the Uri</returns>
        public new string ToString()
        {
            return _ToString("e");
        }

        public string ToString(string format)
        {
            return _ToString(format);
        }

        private string _ToString(string format)
        {
            base.Query = _queryString.ToString(format);

            return base.Uri.AbsoluteUri;
        }

        public string ToString(bool makeRelative)
        {
            if (makeRelative)
                return this.Path + "?" + this.QueryString.ToString();
            else
                return this.ToString();
        }

        #endregion

        #region Private methods
        private void Initialise()
        {
            if (_queryString == null)
            {
                _queryString = new QueryStringDictionary(base.Query, _encoder);
            }
        }
        #endregion

        #region QueryStringDictionary class
        public class QueryStringDictionary : NameValueCollection
        {
            IQueryStringEncoder _encoder;

            public QueryStringDictionary()
            {
            }

            public QueryStringDictionary(string query, IQueryStringEncoder encoder)
            {
                _encoder = encoder;
                Initialise(query);
            }

            /// <summary>
            /// Gets all the values in the QueryStringDictionary.
            /// </summary>
            public string[] Values
            {
                get
                {
                    string[] values = new string[this.Count];
                    this.CopyTo(values, 0);

                    return values;
                }
            }

            /// <summary>
            /// Determines if the QueryStringDictionary contains a specific key. This method performs a linear search in a case-insensitive manner.
            /// </summary>
            /// <param name="key">The key to locate in the QueryStringDictionary.</param>
            /// <returns>true if the QueryStringDictionary contains an entry with the specified key; otherwise, false.</returns>
            public bool ContainsKey(string key)
            {
                foreach (string k in this.AllKeys)
                {
                    if (k.ToLower().Equals(key.ToLower()))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Determines if the QueryStringDictionary contains a specific value. This method performs a linear search in a case-insensitive manner.
            /// </summary>
            /// <param name="value">The value to locate in the QueryStringDictionary. The value can be a null reference (Nothing in Visual Basic).</param>
            /// <returns>true if the QueryStringDictionary contains an entry with the specified value; otherwise, false.</returns>
            public bool ContainsValue(string value)
            {
                string[] values = new string[this.Count];
                this.CopyTo(values, 0);

                foreach (string v in values)
                {
                    if (v.ToLower().Equals(value.ToLower()))
                    {
                        return true;
                    }
                }

                return false;
            }

            private void Initialise(string query)
            {
                if (query == "" || query == null)
                {
                    return;
                }

                this.Clear(); //clear the collection
                query = query.Substring(1); //remove the leading '?'

                query = HttpUtility.UrlDecode(query); //not actually necessary if using a  "~" prefix

                if (_encoder.Prefix != "" && _encoder.Prefix != null && query.StartsWith(_encoder.Prefix))
                {
                    query = query.Substring(_encoder.Prefix.Length);
                    query = _encoder.Decode(query);
                }

                string[] pairs = query.Split(new char[] { '&' });
                foreach (string s in pairs)
                {
                    string[] pair = s.Split(new char[] { '=' });
                    this[pair[0]] = (pair.Length > 1) ? pair[1] : "";
                }
            }

            public new string ToString()
            {
                return _ToString("e");
            }

            public string ToString(string format)
            {
                return _ToString(format);
            }

            private string _ToString(string format)
            {
                if (!this.HasKeys())
                {
                    return "";
                }

                int count = this.Count;

                string[] keys = this.AllKeys;
                string[] values = new string[count];
                this.CopyTo(values, 0);

                string[] pairs = new string[count];
                for (int i = 0; i < count; i++)
                {
                    pairs[i] = string.Concat(keys[i], "=", values[i]);
                }

                string qs = string.Join("&", pairs);

                switch (format)
                {
                    case "e": //encoded
                        return string.Concat(_encoder.Prefix, _encoder.Encode(qs));
                    case "p": //plaintext
                        return qs;
                    default:
                        throw new FormatException();
                }
            }
        }
        #endregion

        #region DefaultEncoder class
        /// <summary>
        /// Provides no encoding, returns plaintext
        /// </summary>
        public class DefaultEncoder : IQueryStringEncoder
        {
            public string Encode(string s)
            {
                return s;
            }

            public string Decode(string s)
            {
                return s;
            }

            public string Prefix
            {
                get
                {
                    return "";
                }
            }
        }
        #endregion

        #region IQueryStringEncoder interface
        public interface IQueryStringEncoder
        {
            string Encode(string s);
            string Decode(string s);
            /// <summary>
            /// The recommended prefix is a '~'
            /// </summary>
            string Prefix
            {
                get;
            }
        }
        #endregion
    }
}
