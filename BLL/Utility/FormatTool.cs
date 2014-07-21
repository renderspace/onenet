using System.Text.RegularExpressions;

namespace One.Net.BLL
{
    public class FormatTool
    {
        public static int GetInteger(object val)
        {
            int ret = -1;
            if (val != null)
            {
                if (!int.TryParse(val.ToString(), out ret)) ret = -1;
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
