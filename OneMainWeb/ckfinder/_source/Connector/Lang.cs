using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CKFinder.Connector
{
    class Lang
    {
        internal const string LANG_PATH = "lang";

        public static string getErrorMessage( int number )
        {
            System.Web.HttpRequest _Request = System.Web.HttpContext.Current.Request;

            string _LanguageCode = _Request.QueryString["langCode"];

            if ( _LanguageCode == null || !Regex.IsMatch( _LanguageCode, @"^[a-z\-]+$" ) )
                _LanguageCode = "en";

            if ( !System.IO.File.Exists( System.Web.HttpContext.Current.Server.MapPath( ( System.IO.Path.Combine( LANG_PATH, _LanguageCode + ".xml" ) ) ) ) )
                _LanguageCode = "en";

            string _LangData = Util.ReadTextFile( System.Web.HttpContext.Current.Server.MapPath( ( System.IO.Path.Combine( LANG_PATH, _LanguageCode + ".xml" ) ) ) );

            // Load the XML.
            XmlDocument _Xml = new System.Xml.XmlDocument();
            _Xml.LoadXml( _LangData );

            try
            {
                return XmlUtil.GetNodeValue( _Xml, "messages/errors/error[@number='" + number.ToString() + "']", "" );
            }
            catch ( Exception )
            {
                return XmlUtil.GetNodeValue( _Xml, "messages/errorUnknown", "" );
            }
        }
    }
}
