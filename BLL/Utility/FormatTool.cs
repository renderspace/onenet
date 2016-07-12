using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace One.Net.BLL
{
    public static class FormatTool
    {
        public static double UnixTicks(this DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static string ToCommaSeparatedValues<T>(this List<T> list)
        {
            return ToSeparatedValues(list, ",");
        }

        private static string ToSeparatedValues<T>(this List<T> list, string separator)
        {
            if (list == null) return "";
            var strBuilder = new StringBuilder(128);
            for (var i = 0; i < list.Count; i++)
            {
                strBuilder.Append(list[i].ToString());
                if (i + 1 != list.Count)
                    strBuilder.Append(separator);
            }
            return strBuilder.ToString();
        }

        public static int GetInteger(object val)
        {
            int ret = -1;
            if (val != null)
            {
                if (!int.TryParse(val.ToString(), out ret)) ret = -1;
            }
            return ret;
        }

        public static long GetLong(object val)
        {
            long ret = -1;
            if (val != null)
            {
                if (!long.TryParse(val.ToString(), out ret)) ret = -1;
            }
            return ret;
        }

        public static bool GetBoolean(object val)
        {
            bool ret = false;
            if (val != null)
            {
                string strVal = val.ToString();
                strVal = strVal.Trim().ToLower();
                if (strVal == "1" || strVal == "true" || strVal == "yes")
                    ret = true;
            }
            return ret;
        }

        public static string GetFileExtension(string fileUri)
        {
            string file = Regex.Replace(fileUri, @"^.*/", "");
            if (file.Contains("."))
                return Regex.Replace(file, @"^.*\.", "");
            else
                return "";
        }

        public static string GetFileName(string fileUri)
        {
            return Regex.Replace(fileUri, @"^.*/", "");
        }
    }
}
