using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;

using One.Net.BLL;

namespace OneMaintenance
{
    public class Typo3ArticlesImport
    {
       
        MySqlConnection conn;
        BArticle articleB = new BArticle();
        BFileSystem fileSystem = new BFileSystem();

        List<BOCategory> allFolders;
        List<BOCategory> myFolders = new List<BOCategory>();
        BOCategory rootFolder;

        public void Test()
        {
            string server = "mysql.netinet.si";
            string userid = "mtb_si";
            string password = "ableone";
            string database = "mtb_si";

            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                    server, userid, password, database);

            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();

                //MySqlCommand cmd = new MySqlCommand(@"SET NAMES latin1", conn);
                //cmd.ExecuteNonQuery();
                /*
                List<BORegular> regularList = ListRegulars();
                foreach (BORegular reg in regularList)
                {
                    articleB.ChangeRegular(reg);
                }*/

                allFolders = fileSystem.ListFolders();
                rootFolder = fileSystem.GetFolder(7);
                
                foreach (BOCategory folder in allFolders)
                {
                    if (folder.ParentId == rootFolder.Id)
                    {
                        myFolders.Add(folder);
                    }
                }

                List<int> articleList = ListArticleIds();
                
                foreach (int id in articleList)
                {
                    GetArticle(id);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private void GetArticle(int id)
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            MySqlDataReader reader = null;
            

                       


            MySqlCommand cmd = new MySqlCommand(@"SELECT tt_news.uid, tt_news.title, tt_news_cat.title, tt_news.short, 
                    tt_news.bodytext, tt_news.image, tt_news.author, tt_news_cat.uid, 
                    FROM_UNIXTIME(tt_news.crdate), FROM_UNIXTIME(tt_news.datetime),
                    imagecaption
                FROM tt_news
                JOIN tt_news_cat_mm ON tt_news.uid = tt_news_cat_mm.uid_local 
                JOIN tt_news_cat ON tt_news_cat_mm.uid_foreign = tt_news_cat.uid
                WHERE tt_news.uid = " + id.ToString(), conn);//LIMIT 50
            try
            {
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
//                    Console.Write(reader.GetString(5) + " / ");
                    BOArticle article = new BOArticle();
                    article.LanguageId = 1060;
                    article.IsChanged = true;
                    article.Id = reader.GetInt32(0);
                    article.Title = FixEncoding(reader.GetString(1));

                    string[] images = reader.GetString(5).Split(new char[] { ',' });
                    string[] imageCap = reader.GetString(10).Split(new char[] { '\n' });


                    article.Teaser = FixEncoding(reader.GetString(3));
                    article.Html = AddParagraphs(FixEncoding(reader.GetString(4)));

                    article.SubTitle = FixEncoding (reader.GetString(6));
                    article.DateCreated = reader.GetDateTime(8);
                    article.DisplayDate = reader.GetDateTime(9);

                    BORegular cat = new BORegular();
                    cat.Id = reader.GetInt32(7);
                    article.Regulars.Add(cat);

                   

                    int i = 0;
                    foreach (string s in images)
                    {
                        string proc = s.Trim();
                        if (proc.Length > 0)
                        {
                            BOFile file = RetrieveFile("http://www.mtb.si/uploads/pics/" + proc);

                            if (file != null)
                            {
                                string firstLetter = file.Name.Substring(0, 1).ToLower();
                                BOCategory folder = null;
                                foreach (BOCategory myFolder in myFolders)
                                {
                                    if (String.Compare(myFolder.Title, firstLetter, true) == 0)
                                    {
                                        folder = myFolder;
                                    }
                                }
                                if (folder == null)
                                { 
                                    folder = new BOCategory();
                                    folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
                                    folder.ParentId = rootFolder.Id;
                                    folder.Title = firstLetter;
                                    folder.SubTitle = "";
                                    folder.Teaser = "";
                                    folder.Html = "";
                                    fileSystem.ChangeFolder(folder);
                                    myFolders.Add(folder);
                                }

                                

                                List<BOFile> existingFiles = fileSystem.FindByName(folder.Id.Value, file.Name);

                                if (existingFiles.Count > 1)
                                {
                                    throw new ApplicationException("duplicate file name");
                                }
                                else if (existingFiles.Count == 1)
                                {
                                    file = existingFiles[0];
                                    Console.WriteLine("Skipping file: " + file.ToString());
                                }
                                else
                                {
                                    file.Folder = folder;
                                    fileSystem.Change(file);
                                    Console.WriteLine(folder.Title + " " + file.ToString());
                                }
                                
                                article.Html += "<p><img src=\"/_files/" + file.Id + "/" + file.Name + "\"";
                                if (imageCap.Length > i)
                                {
                                    //Title = ;
                                    article.Html += " alt=\"" + FixEncoding(imageCap[i].Trim()) + "\"";
                                }
                                article.Html += " /></p>";
                            }
                        }
                        i++;
                    }

                    articleB.ChangeArticle(article);
                    Console.WriteLine("Changed article [" + article.Id + "] " + article.Title); 
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        private string FixEncoding(string input)
        {
            input = input.Replace('è', 'č');
            input = input.Replace('¹', 'š');
            input = input.Replace('¾', 'ž');

            input = input.Replace('È', 'Č');
            input = input.Replace('©', 'Š');
            input = input.Replace('®', 'Ž');

            input = input.Replace("<b>", "<strong>");
            input = input.Replace("</b>", "</strong>");



            
            return input;
        }

        private string AddParagraphs(string input)
        {
            string[] splitedString = input.Split(new char[] { '\n' });

            string output = "";

            foreach (string s in splitedString)
            {
                string proc = s.Trim();
                if (proc.Length > 0)
                {
                    output += "<p>" + proc + "</p>\n\n";
                }
            }

            return output;
        }

        private List<BORegular> ListRegulars()
        {
            MySqlDataReader reader = null;
            List<BORegular> regularList = new List<BORegular>();

            MySqlCommand cmd = new MySqlCommand("SELECT uid, title FROM tt_news_cat", conn);
            try
            {
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BORegular regular = new BORegular();
                    regular.LanguageId = 1060;
                    regular.Id = reader.GetInt32(0);
                    regular.Title = FixEncoding(reader.GetString(1));
                    regular.SubTitle = "";
                    regular.Teaser = "";
                    regular.Html = "";
                    regularList.Add(regular);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return regularList;
        }


        private List<int> ListArticleIds()
        {
            MySqlDataReader reader = null;
            List<int> articleList = new List<int>();

            MySqlCommand cmd = new MySqlCommand(@"SELECT tt_news.uid, tt_news.title, tt_news_cat.title, tt_news.short, 
                    tt_news.bodytext, tt_news.image, tt_news.author, tt_news_cat.uid, 
                    FROM_UNIXTIME(tt_news.crdate), FROM_UNIXTIME(tt_news.datetime),
                    imagecaption
                FROM tt_news
                JOIN tt_news_cat_mm ON tt_news.uid = tt_news_cat_mm.uid_local 
                JOIN tt_news_cat ON tt_news_cat_mm.uid_foreign = tt_news_cat.uid
                WHERE tt_news.deleted = 0
                GROUP BY tt_news.uid
                ", conn);
            try
            {
                reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    articleList.Add(reader.GetInt32(0));
                    //Console.Write(reader.GetString(0) + " ");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return articleList;
        }

        private BOFile RetrieveFile(string imageSource)
        {
            string fileName = FormatTool.GetFileName(imageSource);
            string fileExtension = FormatTool.GetFileExtension(imageSource).Replace(".", "");

            Uri imageUri = new Uri(imageSource);

            System.Net.WebRequest wr = System.Net.WebRequest.Create(imageUri);

            wr.Timeout = 60;

            BOFile file = null;

            byte[] result;
            byte[] buffer = new byte[4096];

            try
            {
                using (WebResponse response = wr.GetResponse())
                {
                    string contentType = response.ContentType;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        file = new BOFile();
                        file.Name = fileName;
                        file.Extension = fileExtension;
                        file.MimeType = contentType;

                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                count = responseStream.Read(buffer, 0, buffer.Length);
                                memoryStream.Write(buffer, 0, count);

                            } while (count != 0);

                            result = memoryStream.ToArray();
                            file.Size = result.Length;
                            file.File = result;
                        }
                    }
                }
            }
            catch
            {
                file = null;
            }
            return file;
        }
    }
}
