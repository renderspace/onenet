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

    public class StringTool
    {
        private static readonly Random random = new Random();

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
            string regex = @"[\s]{1}" + @name + @"=""[\w\d\s=@\-\#/\.:;?_,\(\)&]*""";
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

        public static string RenderAsString(List<int> categories)
        {
            char separator = ',';
            if (categories != null)
            {
                string cats = "";
                foreach (int cat in categories)
                    cats += cat.ToString() + separator;
                return cats.TrimEnd(separator);
            }
            else
            {
                return "";
            }
        }

        public static string RenderAsString(List<string> strings, char separator)
        {
            if (strings != null)
            {
                string strs = "";
                foreach (string str in strings)
                    strs += str.ToString() + separator;
                return strs.TrimEnd(separator);
            }
            else
            {
                return "";
            }
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

        public static string StripHtmlTags(string str)
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

        /// <summary>
        /// This method is primarily used by search mechanizym to obtain a string
        /// from original text where match has appeared. Its returns a substring from
        /// original with wordsBefore count backwards and wordsAfter count onwards
        /// </summary>
        /// <param name="original"></param>
        /// <param name="matchString"></param>
        /// <param name="wordsBefore"></param>
        /// <param name="wordsAfter"></param>
        /// <returns></returns>
        public static string GetStringWithWordsAndAfter(string original, string matchString, int wordsBefore, int wordsAfter)
        {
            string retValue = "";

            string[] matchedWords = matchString.Split(new char[] { ' ' });

            string[] words = original.Split(new char[] { ' ' });
            int matchedStringIndex = -1;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (matchedWords.Length > 1)
                {
                    for (int j = 0; j < matchedWords.Length; j++)
                    {
                        if (word.ToLower().IndexOf(matchedWords[j].ToLower()) > -1)
                        {
                            if (i + 1 < words.Length && j + 1 < matchedWords.Length)
                            {
                                if (words[i + 1].ToLower().IndexOf(matchedWords[j + 1].ToLower()) > -1)
                                    matchedStringIndex = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (word.ToLower().IndexOf(matchString.ToLower()) > -1)
                    {
                        matchedStringIndex = i;
                        break;
                    }
                }
            }

            if (matchedStringIndex > -1)
            {
                int startCount = Math.Max(0, matchedStringIndex - wordsBefore);
                int endCount = Math.Min(words.Length, matchedStringIndex + wordsAfter);

                for (int i = startCount; i < endCount; i++)
                {
                    if (i + 1 == endCount)
                        retValue += words[i];
                    else
                        retValue += words[i] + " ";
                }
            }
            return retValue;
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
