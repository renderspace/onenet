using System;
using System.Collections.Generic;
using System.Text;
using MsSqlDBUtility;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace OneMaintenance
{
    public class ContentHelper
    {
        public void ProcessFileUrls()
        {
            List<ContentObject> contentObjects = new List<ContentObject>();

            // retrieve from db
            SqlParameter[] paramsToPass = new SqlParameter[0];
            string sql = @"SELECT content_fk_id, language_fk_id, html, teaser 
                           FROM content_data_store";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                while (reader.Read())
                {
                    ContentObject content = new ContentObject();
                    content.ContentId = reader.GetInt32(0);
                    content.LanguageId = reader.GetInt32(1);
                    content.Html = reader.GetString(2);
                    content.Teaser = reader.GetString(3);
                    contentObjects.Add(content);
                }
            }

            // retrieve file data
            foreach (ContentObject content in contentObjects)
            {
                string pattern = @"/_present/_util/getfile.aspx\?fileid=(\d)*";

                // Get all of the matches.
                MatchCollection mc = Regex.Matches(content.Html + content.Teaser, pattern,
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.IgnoreCase);

                foreach (Match m in mc)
                {
                    FileObject file = new FileObject();
                    file.FileId = Int32.Parse(m.Value.Replace("/_present/_util/getfile.aspx?fileid=", ""));

                    // retrieve file name
                    paramsToPass = new SqlParameter[1];
                    paramsToPass[0] = new SqlParameter("@fileId", file.FileId);

                    sql = "SELECT name FROM [dbo].[files] WHERE id=@fileId";

                    using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
                    {
                        if (reader.Read())
                        {
                            file.FileName = reader.GetString(0);
                        }
                    }

                    content.Files.Add(file);
                }
            }
            
            // replace old urls with new urls and save
            foreach (ContentObject content in contentObjects)
            {
                if (content.Files.Count > 0)
                {
                    foreach (FileObject file in content.Files)
                    {
                        string pattern = @"/_present/_util/getfile.aspx?fileid=" + file.FileId;
                        content.Html = content.Html.Replace(pattern, "/_files/" + file.FileId + "/" + file.FileName);
                        content.Teaser = content.Teaser.Replace(pattern, "/_files/" + file.FileId + "/" + file.FileName);
                    }

                    paramsToPass = new SqlParameter[4];
                    paramsToPass[0] = new SqlParameter("@contentId", content.ContentId);
                    paramsToPass[1] = new SqlParameter("@languageId", content.LanguageId);
                    paramsToPass[2] = new SqlParameter("@html", content.Html);
                    paramsToPass[3] = new SqlParameter("@teaser", content.Teaser);

                    sql = @"UPDATE [dbo].[content_data_store] 
                        SET html=@html, 
                            teaser=@teaser
                        WHERE content_fk_id=@contentId AND language_fk_id=@languageId";
                    SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
                    Console.WriteLine("Changed: contentId=" + content.ContentId + " languageId=" + content.LanguageId + "\n");
                }
            }
        }
    }

    internal class ContentObject
    {
        int contentId;
        int languageId;
        string html = string.Empty;
        string teaser = string.Empty;
        private List<FileObject> files = new List<FileObject>();

        public int ContentId { get { return contentId; } set { contentId = value; } }
        public int LanguageId { get { return languageId; } set { languageId = value; } }
        public string Html { get { return html; } set { html = value; } }
        public string Teaser { get { return teaser; } set { teaser = value; } }
        public List<FileObject> Files { get { return files; } set { files = value; } }
    }

    internal class FileObject
    {
        int fileId;
        string fileName = string.Empty;

        public int FileId { get { return fileId; } set { fileId = value; } }
        public string FileName { get { return fileName; } set { fileName = value; } }
    }
}
