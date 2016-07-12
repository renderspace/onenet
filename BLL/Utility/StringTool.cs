using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Web;

using System.Text;
using System.Reflection;
using System.IO;
using One.Net.BLL.Utility;

namespace One.Net.BLL
{

    public static class StringTool
    {
        private static readonly Random random = new Random();

        public static T ParseToEnum<T>(this string str) where T : struct
        {
            try
            {
                T res = (T)Enum.Parse(typeof(T), str);
                if (!Enum.IsDefined(typeof(T), res)) return default(T);
                return res;
            }
            catch
            {
                return default(T);
            }
        } 

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + " ...";
        }

        public static string GetTextContentFromResource(string fileName)
        {
            string result = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("One.Net.BLL." + fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

        public static byte[] GetFileContentFromResource(string fileName)
        {
            byte[] result = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("One.Net.BLL." + fileName))
            {
                using (var reader = new BinaryReader(stream))
                {
                    result = reader.ReadAllBytes();
                }
            }
            return result;
        }

        public static List<string> SplitString(string toSplit, char separator)
        {
            List<string> splitted = new List<string>();

            if (toSplit != null && toSplit.Length > 0)
            {
                string[] iSplitted = toSplit.Split(separator);
                foreach (string s in iSplitted)
                {
                    if (s.Trim().Length > 0)
                        splitted.Add(s.Trim());
                }
            }
            return splitted;
        }

        public static List<string> SplitString(string toSplit)
        {
            List<string> splitted = new List<string>();

            if (toSplit != null && toSplit.Length > 0)
            {
                string[] iSplitted = toSplit.Split(new char[] { ' ', ';', ',' });
                foreach (string s in iSplitted)
                {
                    if (s.Trim().Length > 0)
                        splitted.Add(s.Trim());
                }
            }
            return splitted;
        }

        public static string GetHtmlAttributeValue(string str, string name)
        {
            string regex = @"[\s]{1}" + @name + @"=""[\w\d\s=@\-\#/\.:;?_,\(\)&^\x00-\x80]*""";
            Regex finder = new Regex(regex, RegexOptions.IgnoreCase);
            MatchCollection matches = finder.Matches(str);
            if (matches.Count < 1)
                return "";
            string answer = matches[0].ToString();
            finder = new Regex(@name + @"=""", RegexOptions.IgnoreCase);
            answer = finder.Replace(answer, "");
            if (answer.EndsWith("\""))
                answer = answer.Substring(0, answer.Length - 1);
            return answer.Trim();
        }

        public static List<int> SplitStringToIntegers(string list)
        {
            List<int> ret = new List<int>();

            if (list != null)
            {
                string[] splittedInts = list.Split(new char[] {',', ';', ' '});

                foreach (string str in splittedInts)
                {
                    int i = FormatTool.GetInteger(str.Trim());
                    if (i >= 0)
                        ret.Add(i);
                }
            }

            return ret;
        }

        public static string StripHtmlTags(this string str)
        {
            Regex stripper = new Regex("<(.|\n)+?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string strNoTags = stripper.Replace(str, "");
            return HttpUtility.HtmlDecode(strNoTags);
        }

        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();

            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(25 * random.NextDouble() + 65));
                builder.Append(ch);
            }

            if (lowerCase)
            {
                return builder.ToString().ToLower();
            }
            return builder.ToString();
        }

        public static Color HexStringToColor(string hexColor)
        {
            string hc = ExtractHexDigits(hexColor);
            if (hc.Length != 6)
            {
                // you can choose whether to throw an exception
                //throw new ArgumentException("hexColor is not exactly 6 digits.");
                return Color.Empty;
            }
            string r = hc.Substring(0, 2);
            string g = hc.Substring(2, 2);
            string b = hc.Substring(4, 2);
            Color color = Color.Empty;
            try
            {
                int ri
                   = Int32.Parse(r, System.Globalization.NumberStyles.HexNumber);
                int gi
                   = Int32.Parse(g, System.Globalization.NumberStyles.HexNumber);
                int bi
                   = Int32.Parse(b, System.Globalization.NumberStyles.HexNumber);
                color = Color.FromArgb(ri, gi, bi);
            }
            catch
            {
                // you can choose whether to throw an exception
                //throw new ArgumentException("Conversion failed.");
                return Color.Empty;
            }
            return color;
        }
        /// <summary>
        /// Extract only the hex digits from a string.
        /// </summary>
        public static string ExtractHexDigits(string input)
        {
            // remove any characters that are not digits (like #)
            Regex isHexDigit
               = new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
            string newnum = "";
            foreach (char c in input)
            {
                if (isHexDigit.IsMatch(c.ToString()))
                    newnum += c.ToString();
            }
            return newnum;
        }

    }
}
