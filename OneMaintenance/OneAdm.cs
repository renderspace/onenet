using System;
using System.Collections.Generic;
using System.Text;

using RJH.CommandLineHelper;

namespace OneMaintenance
{
    class OneAdm
    {
        public class Burek
        {
            private bool m_showHelp = false;
            private int folderId = -1;
            private int pageId = -1;

            private string folder;
            private int webSiteId, languageId;

            private AdmMode selectedAdmMode = AdmMode.ShowUsage;

            public enum AdmMode
            {
                ShowUsage,
                SanityCheck,
                ConvertFileLinks,
                Typo3ArticlesImport,
                RecursiveDeleteFolders,
                Import13,
                ImportArticles,
                ImportArticlesSpecial,
                ImportRecipes,
                ImportFiles,
                ImportAll
            };

            [CommandLineSwitch("Help", "Show some additional help")]
            public bool ShowHelp
            {
                get { return m_showHelp; }
                set { m_showHelp = value; }
            }

            [CommandLineSwitch("Mode", "Mode switch")]
            public AdmMode Mode
            {
                get { return selectedAdmMode; }
                set { selectedAdmMode = value; }
            }

            [CommandLineSwitch("FolderId", "Folder Id")]
            public int FolderId
            {
                get { return folderId; }
                set { folderId = value; }
            }

            [CommandLineSwitch("PageId", "Page Id")]
            public int PageId
            {
                get { return pageId; }
                set { pageId = value; }
            }

            [CommandLineSwitch("Folder", "Folder")]
            public string Folder
            {
                get { return folder; }
                set { folder = value; }
            }

            [CommandLineSwitch("WebSiteId", "WebSite Id")]
            public int WebSiteId
            {
                get { return webSiteId; }
                set { webSiteId = value; }
            }

            [CommandLineSwitch("LanguageId", "Language Id")]
            public int LanguageId
            {
                get { return languageId; }
                set { languageId = value; }
            }



            public int Run(string[] cmdLine)
            {
                Parser parser = new Parser(System.Environment.CommandLine, this);
                parser.Parse();

                Console.WriteLine("\n\nOne.NET 1.6 Maintenance tool\n");

                switch (Mode)
                {
                    case AdmMode.ShowUsage:
                        {
                            Console.WriteLine(
                                @"    /Help - Shows help
    /Mode SanityCheck - checks the data in DB,
    /Mode ConvertFileLinks - converts from getfile.aspx to virtualfilesystem
    /Mode Typo3ArticlesImport - imports articles from Typo3 website
    /Mode RecursiveDeleteFolders - deletes empty folders by id
        /FolderId 
    /Mode Import13 - imports page structure and textContent from XML called exp.xml
    /Mode ImportArticles - imports articles from XML called articles.xml
    /Mode ImportRecipes - imports recipes from XML called recipes.xml
    /Mode ImportFiles - imports files
    /Mode ImportAll - imports whole site (structure & articles)
        /LanguageId - id of language (number)
        /Folder - path of folder containing all data to imports

        /WebSiteId  - id of website (number)
        or
        /PageId - id of root-page to attach subfolders to
");
                            break;
                        }
                    case AdmMode.SanityCheck:
                        {
                            Console.WriteLine(@" SanityCheck");
                            break;
                        }
                    case AdmMode.ConvertFileLinks:
                        {
                            ContentHelper contentHelper = new ContentHelper();
                            contentHelper.ProcessFileUrls();
                            Console.WriteLine(@" ConvertFileLinks");
                            break;
                        }
                    case AdmMode.Typo3ArticlesImport:
                        {
                            Typo3ArticlesImport imp = new Typo3ArticlesImport();
                            imp.Test();
                            Console.WriteLine(@"Typo3ArticlesImport Finished.");
                            Console.ReadKey();
                            break;
                        }
                    case AdmMode.RecursiveDeleteFolders:
                        {
                            if (FolderId > 0)
                            {
                                FolderManagement fo = new FolderManagement();
                                fo.RecursiveDeleteFolders(FolderId);
                            }
                            else
                            {
                                Console.WriteLine("Missing parameter FolderID");
                            }
                            break;
                        }
                    case AdmMode.Import13:
                        {
                            XMLImporter13 importer = new XMLImporter13();
                            if (System.IO.File.Exists(importer.SiteImportFileName))
                            {
                                importer.Import();
                            }
                            else
                            {
                                Console.WriteLine("Missing file [exp.xml] to import ");
                            }
                            break;
                        }

                    case AdmMode.ImportArticlesSpecial:
                        {
                            XMLImporter13 importer = new XMLImporter13();
                            importer.LanguageId = LanguageId;
                            importer.ImportFolder = Folder;
                            importer.WebSiteId = WebSiteId;
                            if (System.IO.File.Exists(importer.ArticlesImportFileName))
                            {
                                importer.ImportArticlesSpecial();
                            }
                            else
                            {
                                Console.WriteLine("Missing file [" + importer.ArticlesImportFileName + "] to import ");
                            }
                            break;
                        }

                    case AdmMode.ImportArticles:
                        {
                            XMLImporter13 importer = new XMLImporter13();
                            importer.LanguageId = LanguageId;
                            importer.ImportFolder = Folder;
                            importer.WebSiteId = WebSiteId;
                            if (System.IO.File.Exists(importer.ArticlesImportFileName))
                            {
                                importer.ImportArticles();
                                importer.ImportFiles(importer.ArticlesImportFileName);
                            }
                            else
                            {
                                Console.WriteLine("Missing file [" + importer.ArticlesImportFileName + "] to import ");
                            }
                            break;
                        }
                    case AdmMode.ImportRecipes:
                        {
                            XMLImporter13 importer = new XMLImporter13();
                            importer.LanguageId = LanguageId;
                            importer.ImportFolder = Folder;
                            importer.FolderId = FolderId;

                            if (System.IO.File.Exists(importer.RecipesImportFileName))
                            {
                                importer.ImportRecipes();
                            }
                            else
                            {
                                Console.WriteLine("Missing file [" + importer.RecipesImportFileName + "] to import ");
                            }

                            break;
                        }
                    case AdmMode.ImportFiles:
                        {
                            XMLImporter13 importer = new XMLImporter13();
                            importer.ImportFolder = Folder;
                            if (System.IO.Directory.Exists(importer.FilesImportFolder))
                            {
                                Console.WriteLine("not implemented");
                                //importer.ImportFiles();
                            }
                            else
                            {
                                Console.WriteLine("Missing folder [" + importer.FilesImportFolder + "] to import ");
                            }
                            break;
                        }
                    case AdmMode.ImportAll:
                        {
                            Console.WriteLine("AdmMode.ImportAll");

                            XMLImporter13 importer = new XMLImporter13();
                            importer.LanguageId = LanguageId;
                            importer.ImportFolder = Folder;
                            importer.WebSiteId = WebSiteId;
                            importer.PageId = PageId;
                            importer.FolderId = FolderId;
                            Console.WriteLine("F " + importer.ImportFolder);
                            Console.WriteLine("S " + importer.SiteImportFileName);
                            Console.WriteLine("A " + importer.ArticlesImportFileName);
                            Console.WriteLine("Site: " + importer.WebSiteId);
                            Console.WriteLine("PageId: " + importer.PageId);
                            Console.WriteLine("Language: " + importer.LanguageId);

                            if (importer.PageId > 0 && importer.WebSiteId > 0 && importer.LanguageId > 0)
                            {
                                if (System.IO.File.Exists(importer.SiteImportFileName))
                                {
                                    importer.Import();
                                    importer.ImportFiles(importer.SiteImportFileName);
                                }
                                else
                                    Console.WriteLine("SiteImportFileName doesn't exist");
                            }
                            else if (importer.WebSiteId > 0 && importer.LanguageId > 0)
                            {
                                if (System.IO.File.Exists(importer.SiteImportFileName))
                                {
                                    importer.Import();
                                    importer.ImportFiles(importer.SiteImportFileName);
                                }
                                else
                                    Console.WriteLine("SiteImportFileName doesn't exist");
                                if (System.IO.File.Exists(importer.ArticlesImportFileName))
                                {
                                    importer.ImportArticles();
                                    importer.ImportFiles(importer.ArticlesImportFileName);
                                }
                                else
                                    Console.WriteLine("ArticlesImportFileName doesn't exist");
                            }
                            break;
                        }
                }

                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return 0;
            }
        }

        static int Main(string[] args)
        {
            Burek app = new Burek();
            return app.Run(args);
        }
    }
}
