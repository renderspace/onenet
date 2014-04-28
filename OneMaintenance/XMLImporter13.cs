using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Configuration;

using One.Net.BLL;
using Recipes.BLL;

namespace OneMaintenance
{
    public class XMLImporter13
    {
        public string SiteImportFileName;
        public string ArticlesImportFileName;
        public string RecipesImportFileName;
        public string FilesImportFolder;
        private int webSiteId;
        private int languageId;
        private int pageId = -1;
        private int folderId = -1;

        private string importFolder;

        int depth = -1;
        protected static BWebsite webSiteB = new BWebsite();
        private static readonly BTextContent textContentB = new BTextContent();
        private static readonly BArticle articleB = new BArticle();
        private static readonly BFileSystem fileSystemB = new BFileSystem();
        private static readonly BComment commentB = new BComment();

        //protected  static B contentB = new BContent();
        Dictionary<string, int> placeholders = new Dictionary<string, int>();

        public XMLImporter13()
        {
            
        }

        public string ImportFolder
        {
            get { return importFolder; }
            set
            {
                importFolder = value;
                SiteImportFileName = importFolder + "\\site.xml";
                ArticlesImportFileName = importFolder + "\\articles.xml";
                RecipesImportFileName = importFolder + "\\recipes.xml";
                FilesImportFolder = importFolder + "\\files";
            }
        }

        public int LanguageId
        {
            get { return languageId; }
            set { languageId = value; }
        }

        public int WebSiteId
        {
            get { return webSiteId; }
            set { webSiteId = value; }
        }

        public int PageId
        {
            get { return pageId; }
            set { pageId = value; }
        }

        public int FolderId
        {
            get { return folderId; }
            set { folderId = value; }
        }

        public void Import()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            XmlTextReader rdr = null;
            rdr = new XmlTextReader(SiteImportFileName);
            rdr.WhitespaceHandling = WhitespaceHandling.None;

            List<BOTemplate> templates = webSiteB.ListTemplates("3");
            Dictionary<string, BOTemplate> myTemplateList = new Dictionary<string, BOTemplate>();
            foreach (BOTemplate boTemplate in templates)
            {
                myTemplateList.Add(boTemplate.Name, boTemplate);   
            }
            int textContentModuleId = -1;
            int articleModuleId = -1;
            int storesModuleId = -1;
            List<BOModule> modules = BWebsite.ListModules();
            foreach (BOModule module in modules)
            {
                if (module.Name.Equals("TextContent"))
                    textContentModuleId = module.Id;
                else if (module.Name.Equals("Article"))
                    articleModuleId = module.Id;
                else if (module.Name.Equals("StoresFilter"))
                    storesModuleId = module.Id;
                //ddlModuleTypes.Items.Add(new ListItem(ResourceManager.GetString("$" + module.Name), module.Id.ToString()));
            }

            Dictionary<int, int> pageMapping = new Dictionary<int, int>();
            

            foreach(BOPlaceHolder placeholder in webSiteB.ListPlaceHolders())
            {
                placeholders.Add(placeholder.Name, placeholder.Id.Value);
            }

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    if (rdr.LocalName.Equals("page"))
                    {
                        if (rdr.HasAttributes)
                        {
                            Console.WriteLine("\tP------------------");
                            BOPage page = new BOPage();
                            page.PublishFlag = false;
                            page.BreakPersistance = bool.Parse(rdr.GetAttribute("break_persistance"));
                            page.MenuGroup = Int32.Parse(rdr.GetAttribute("menu_group"));
                            page.Order = Int32.Parse(rdr.GetAttribute("order"));
                            page.ParLink = rdr.GetAttribute("par_link");
                            string requestedTemplateName = rdr.GetAttribute("template_name");
                            int requestedPageId = Int32.Parse(rdr.GetAttribute("id"));
                            int requestedParentId = Int32.Parse(rdr.GetAttribute("parentId"));

                            rdr.MoveToContent();
                            rdr.ReadToDescendant("title");
                            page.Title = rdr.ReadString();
                            page.WebSiteId = webSiteId;
                            page.LanguageId = languageId;


                            if (myTemplateList.ContainsKey(requestedTemplateName))
                            {
                                page.Template = myTemplateList[requestedTemplateName];
                            }
                            else
                            {
                                BOTemplate tpl = new BOTemplate();
                                tpl.Name = requestedTemplateName;
                                tpl.Type = "3";
                                webSiteB.ChangeTemplate(tpl);
                                myTemplateList.Add(requestedTemplateName, tpl);
                                page.Template = tpl;
                            }

                            if (PageId > 0 && requestedPageId == requestedParentId)
                                page.ParentId = PageId;
                            else if (requestedPageId == requestedParentId)
                                page.ParentId = null;
                            else
                                page.ParentId = pageMapping[requestedParentId];

                            //page.Id = requestedPageId;

                            Console.WriteLine(requestedPageId);

                            webSiteB.ChangePage(page);
                            pageMapping.Add(requestedPageId, page.Id);
                            int noOfAllImportedInstances = 0;
                            int noOfImportedInstances = 0;
                            // INSTANCES

                            Dictionary<string, BOSetting> pageSettings = new Dictionary<string, BOSetting>();
                            rdr.ReadToNextSibling("settings");
                            ReadSettings(rdr, pageSettings);
                            // Current possition is supposed to be end of page settings. So we move by one.
                            rdr.Read();
                            if (rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("page"))
                            {
                                Console.WriteLine("---------- end page ---------- ");
                            }
                            else if (rdr.IsEmptyElement && rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("instances"))
                            {
                                Console.WriteLine("empty instances");
                            }
                            else if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("instances")) 
                            {
                                while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("instances")))
                                {
                                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("instance"))
                                    {
                                        BOModuleInstance moduleInstance = new BOModuleInstance();
                                        moduleInstance.PageId = page.Id;
                                        moduleInstance.PublishFlag = false;
                                        moduleInstance.PendingDelete = false;
                                        moduleInstance.Changed = true;
                                        moduleInstance.Order = -1;
                                        moduleInstance.PlaceHolderId = 1;
                                        moduleInstance.PersistFrom = 0;
                                        moduleInstance.PersistTo = 0;
                                        moduleInstance.PlaceHolderId =
                                            CheckPlaceHolder(rdr.GetAttribute("place_holder_name"));

                                        string instanceName = rdr.GetAttribute("name");

                                        Dictionary<string, BOSetting> settings = new Dictionary<string, BOSetting>();
                                        rdr.ReadToDescendant("settings");
                                        ReadSettings(rdr, settings);
                                        if (instanceName.Equals("text_content") && textContentModuleId > 0)
                                        {
                                            moduleInstance.ModuleId = textContentModuleId;
                                            webSiteB.ChangeModuleInstance(moduleInstance);
                                            rdr.ReadToNextSibling("content");
                                            BOInternalContent content = new BOInternalContent();
                                            ReadContent(rdr, content, null);
                                            textContentB.ChangeTextContent(moduleInstance.Id, content.Title,
                                                                           content.SubTitle,
                                                                           content.Teaser, content.Html);
                                            noOfImportedInstances++;
                                        }
                                        else if (instanceName.Equals("Article") && articleModuleId > 0)
                                        {
                                            moduleInstance.ModuleId = articleModuleId;
                                            moduleInstance.Name = "Article";
                                            webSiteB.ChangeModuleInstance(moduleInstance);
                                            if (settings.ContainsKey("RecordsPerPage"))
                                            {
                                                BOSetting sett = new BOSetting();
                                                sett.Name = "LimitNoArticles";
                                                sett.Type = "Int";
                                                sett.UserVisibility = "NORMAL";
                                                sett.Value = settings["RecordsPerPage"].Value;
                                                moduleInstance.Settings[sett.Name] = sett; 
                                            }
                                            if (settings.ContainsKey("RegularID"))
                                            {
                                                BOSetting sett = new BOSetting();
                                                sett.Name = "RegularsList";
                                                sett.Type = "String";
                                                sett.UserVisibility = "NORMAL";
                                                sett.Value = settings["RegularID"].Value;
                                                moduleInstance.Settings[sett.Name] = sett; 
                                            }
                                            webSiteB.ChangeModuleInstance(moduleInstance);
                                            noOfImportedInstances++;
                                        }
                                        else if (instanceName.Equals("Stores") && storesModuleId > 0)
                                        {
                                            moduleInstance.ModuleId = storesModuleId;
                                            webSiteB.ChangeModuleInstance(moduleInstance);
                                            noOfImportedInstances++;
                                        }
                                        Console.WriteLine("\tI-" + instanceName);
                                        noOfAllImportedInstances++;
                                    }
                                }
                                Console.WriteLine("\tP-" + page.ToString() + " with " + noOfAllImportedInstances + " / " + noOfImportedInstances + " instances.");
                            }
                            else
                            {
                                Console.Write(rdr.Name + rdr.IsStartElement());
                                Console.WriteLine("---------- SKIPPED ---------- ");
                            }
                        }
                    }

                }
            }
            Console.Write("end");
        }

        public void ImportFiles(string fromFile)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            XmlTextReader rdr = new XmlTextReader(fromFile);
            rdr.WhitespaceHandling = WhitespaceHandling.None;
            Dictionary<int, int> path = new Dictionary<int, int>();

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    if (rdr.LocalName.Equals("folders"))
                    {
                        Console.WriteLine("found folders");

                        int currentFolderId = FolderId;
                        
                        bool foundRootFolder = true;

                        while (rdr.Read())
                        {
                            if (rdr.NodeType == XmlNodeType.Element)
                                switch(rdr.Name)
                                {
                                    case "folder":
                                        //currentFolderId = Int32.Parse(rdr.GetAttribute("id"));
                                        /*
                                        if (!foundRootFolder && rdr.Depth == 2 && rdr.GetAttribute("name").Equals("/"))
                                        {
                                            foundRootFolder = true;
                                            depth = -1;
                                            //path.Add(depth, currentFolderId);
                                            //parentFolderId = Int32.Parse(rdr.GetAttribute("id"));
                                        }
                                        if(foundRootFolder)
                                        {
                                            string note = "";
                                            int currentDepth = rdr.Depth - 2;
                                            int previousDepth = depth;

                                            if (previousDepth == currentDepth)
                                                note = " ";
                                            else if (previousDepth < currentDepth)
                                            {
                                                if (!path.ContainsKey(currentDepth))
                                                {
                                                    path.Add(currentDepth, currentFolderId);
                                                    note = " parent folder changed (DOWN) ";
                                                }
                                                else
                                                    note = " parent folder changed (DOWN-EXISTING) ";
                                            }
                                            else if (previousDepth > currentDepth)
                                            {
                                                int c = 0;
                                                for (int i = previousDepth; i > currentDepth; i-- )
                                                {
                                                    path.Remove(i);
                                                    c++;
                                                }
                                                note = " parent folder changed (UP) by " + c + " | ";
                                            }
                                            depth = currentDepth;

                                            int? parentFolderId = currentDepth == 0
                                                                     ? (int?) null
                                                                     : path[currentDepth - 1];

                                            ChangeFolder(currentFolderId, parentFolderId, rdr.GetAttribute("name"));

                                            StringBuilder info = new StringBuilder();
                                            info.Append('*', depth);
                                            info.Append(" Folder " + currentFolderId + "/" + (parentFolderId.HasValue ? parentFolderId.Value.ToString() : "NULL")  + note);
                                            Console.Write(info);
                                            Console.WriteLine();
                                        }
                                         */
                                        break;
                                    case "files":
                                        if(foundRootFolder)
                                            ReadFiles(rdr, currentFolderId);
                                        break;

                                }
                        }
                    }
                }
            }
            Console.Write("end");
        }

        private void RenderPath(Dictionary<int, int> path)
        {
            foreach (KeyValuePair<int, int> pair in path)
            {
                Console.Write(pair.Value + "/");
            }
        }

        private void ChangeFolder(int folderId, int? parentFolderId, string folderName)
        {
            BOCategory folder = new BOCategory();

            folder.Type = BOFile.FOLDER_CATEGORIZATION_TYPE;
            folder.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            folder.Title = folderName;
            folder.Teaser = folder.SubTitle = folder.Html = string.Empty;
            folder.IsSelectable = true;
            folder.ParentId = parentFolderId;
            folder.Id = folderId;

            fileSystemB.ChangeFolder(folder);
        }

        private void ReadFiles(XmlReader rdr, int folderId)
        {
            Dictionary<int, int> folderMap = GetRegularFolderMap();

            if (!rdr.Name.Equals("files"))
            {
                Console.WriteLine("incorrect call to ReadFiles");
                return;
            }
            while (!(rdr.IsEmptyElement && rdr.Name.Equals("files")) && rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
            {
                if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("file") && rdr.IsEmptyElement)
                {
                    BOFile file = new BOFile();
                    file.Id = null; // Int32.Parse(rdr.GetAttribute("id"));
                    file.MimeType = rdr.GetAttribute("mime_type");

                    FileInfo fi = new FileInfo(rdr.GetAttribute("name"));
                    file.Name = fi.Name;
                    file.Extension = fi.Extension;

                    file.Size = Int32.Parse(rdr.GetAttribute("size"));

                    file.Folder = fileSystemB.GetFolder(folderId );
                    string FullPath = FilesImportFolder + "\\" + file.Id + "\\" + file.Name;

                    StringBuilder info = new StringBuilder();
                    //info.Append(' ', depth);
                    if (System.IO.File.Exists(FullPath))
                    {
                        byte[] fileBytes = File.ReadAllBytes(FullPath);

                        file.File = fileBytes;

//                        BOFile existingFile = fileSystemB.Get(file.Id.Value);
//                        if (existingFile == null)
                            fileSystemB.Change(file);
                        //else
                        //    info.Append("******* Exisitng file ********* Overwrite skipped");

                        info.Append("  -- " + file.Id);
                    }
                    else
                        info.Append("  -- Missing File data for " + file);


                    Console.WriteLine(info);
                    //Console.WriteLine(FullPath);
                    //Console.WriteLine(System.IO.File.Exists(FullPath));
                }
            }
        }

        private int CheckPlaceHolder(string placeHolderName)
        {
            if (!placeholders.ContainsKey(placeHolderName))
            {
                BOPlaceHolder placeHolder = new BOPlaceHolder();
                placeHolder.Name = placeHolderName;
                webSiteB.ChangePlaceHolder(placeHolder);   
                placeholders.Add(placeHolder.Name, placeHolder.Id.Value);
            }
            return placeholders[placeHolderName];
        }

        private void ReadSettings (XmlReader rdr, IDictionary<string, BOSetting> settings)
        {
            if (!rdr.Name.Equals("settings"))
            {
                Console.WriteLine("incorrect call to ReadSettings");
                return;
            }
            while (!(rdr.IsEmptyElement && rdr.Name.Equals("settings")) && rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("settings")))
            {
                if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("setting") && rdr.IsEmptyElement)
                {
                    BOSetting setting = new BOSetting();
                    setting.Name = rdr.GetAttribute("key");
                    setting.Value = rdr.GetAttribute("value");
                    setting.Type = rdr.GetAttribute("type");
                    settings.Add(setting.Name, setting);
                }
            }
        }

        public void ImportRecipes()
        {
            BRecipe recipeB = new BRecipe();

            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            XmlTextReader rdr = new XmlTextReader(RecipesImportFileName);
            rdr.WhitespaceHandling = WhitespaceHandling.None;

            // mealtype
            const string mt_appetizerId = "10721";
            const string mt_soupId = "10722";
            const string mt_main_courseId = "10723";
            const string mt_sauceId = "10724";
            const string mt_saladId = "10725";
            const string mt_dessertId = "10726";

            // difficulty
            const string d_simple = "10717";
            const string d_demanding = "10718";
            const string d_very_demanding = "10719";

            BOCategory folder = fileSystemB.GetFolder(folderId);

            List<BORecipe> importedRecipes = new List<BORecipe>();
            // Dictionary<recipe, List<oldFileId>>
            Dictionary<BORecipe, List<int>> importedFileIds = new Dictionary<BORecipe, List<int>>();
            Dictionary<int, int> oldNewFileIds = new Dictionary<int, int>();

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("recipes"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("recipes")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("recipe"))
                        {
                            BORecipe recipe = new BORecipe();

                            int recipeId = Int32.Parse(rdr.GetAttribute("id"));

                            recipe.AverageVote = Decimal.Parse(rdr.GetAttribute("average_rating"));

                            string ct = rdr.GetAttribute("cook_time");

                            if (ct.Length < 2)
                                recipe.CookTime = new DateTime(2000, 1, 1);
                            else
                                recipe.CookTime = DateTime.Parse(ct);

                            recipe.ForPersons = Int32.Parse(rdr.GetAttribute("for_persons"));

                            string pt = rdr.GetAttribute("prep_time");

                            if (pt.Length < 2)
                                recipe.PrepTime = new DateTime(2000, 1, 1);
                            else
                                recipe.PrepTime = DateTime.Parse(pt);

                            recipe.TotalVotes = Int32.Parse(rdr.GetAttribute("total_voters"));
                            recipe.Lcid = languageId;

                            Console.WriteLine("RecipeId:" + recipeId.ToString());

                            string ingredientsStr = "<ul>";
                            rdr.Read();
                            if ( rdr.Name.Equals("recipe_ingredients") && !rdr.IsEmptyElement)
                            {
                                while (rdr.Read() &&
                                       !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("recipe_ingredients")))
                                {
                                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("recipe_ingredient"))
                                    {
                                        string ingredient = rdr.GetAttribute("Ingredient");
                                        string unit = rdr.GetAttribute("Unit");
                                        string amount = rdr.GetAttribute("Amount");
                                        int amt = 0;
                                        Int32.TryParse(amount, out amt);

                                        Console.WriteLine("->Ingredient:" + ingredient);

                                        ingredientsStr += @"<li>"
                                                          + "<strong>" + ingredient + ":</strong>"
                                                          + " " + amount + " "
                                                          + "<em>" + GetSingularPlural(unit, amt) + "</em>"
                                                          + "</li>\n";
                                    }
                                }
                            }
                            ingredientsStr += "</ul>";

                            if (ingredientsStr != "<ul class=\"nobl ing\"></ul>")
                                recipe.Ingredients = ingredientsStr;

                            if (rdr.Read() && rdr.Name.Equals("recipe_categories") && !rdr.IsEmptyElement)
                            {
                                while (rdr.Read() &&
                                       !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("recipe_categories")))
                                {
                                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("recipe_category"))
                                    {
                                        string categoryId = rdr.GetAttribute("Id");
                                        string categoryName = rdr.GetAttribute("Category");

                                        Console.WriteLine("->Category:" + categoryName);

                                        switch (categoryId)
                                        {
                                            case mt_appetizerId:
                                                recipe.MealType = "appetizer";
                                                break;
                                            case mt_soupId:
                                                recipe.MealType = "soup";
                                                break;
                                            case mt_main_courseId:
                                                recipe.MealType = "main_course";
                                                break;
                                            case mt_sauceId:
                                                recipe.MealType = "sauce";
                                                break;
                                            case mt_saladId:
                                                recipe.MealType = "salad";
                                                break;
                                            case mt_dessertId:
                                                recipe.MealType = "dessert";
                                                break;
                                            case d_simple:
                                                recipe.Difficulty = "simple";
                                                break;
                                            case d_demanding:
                                                recipe.Difficulty = "demanding";
                                                break;
                                            case d_very_demanding:
                                                recipe.Difficulty = "very_demanding";
                                                break;
                                            default:
                                               
                                                /*BORecipeCategory category = recipeB.FindCategory(categoryName, languageId);
                                                if (category == null)
                                                {
                                                    category = new BORecipeCategory();
                                                    category.CategoryName = categoryName;
                                                    category.LanguageId = languageId;
                                                    recipeB.ChangeCategory(category);
                                                }
                                                recipe.Categories.Add(category);*/
                                                break;
                                        }
                                    }
                                }
                            }

                            if (rdr.Read() && rdr.Name.Equals("content") && !rdr.IsEmptyElement)
                            {
                                if (rdr.ReadToFollowing("title"))
                                    recipe.Title = rdr.ReadString();
                                if (rdr.ReadToFollowing("subtitle"))
                                    recipe.SubTitle = rdr.ReadString();
                                if (rdr.ReadToFollowing("teaser"))
                                    recipe.Teaser = rdr.ReadString();
                                if (rdr.ReadToFollowing("html"))
                                    recipe.Preparation = rdr.ReadString();

                                Console.WriteLine("->Title:" + recipe.Title);
                                rdr.Read();
                            }

                            List<int> fileIds = new List<int>();
                            if (rdr.Read() && rdr.Name.Equals("files") && !rdr.IsEmptyElement)
                            {
                                while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
                                {
                                    if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("file"))
                                    {
                                        fileIds.Add(Int32.Parse(rdr.GetAttribute("id")));
                                        Console.WriteLine("->File:" + rdr.GetAttribute("id"));
                                    }
                                }
                            }

                            importedFileIds.Add(recipe, fileIds);
                            importedRecipes.Add(recipe);
                        }
                    }
                }
                else if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("files"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("file"))
                        {
                            int importedFileId = Int32.Parse(rdr.GetAttribute("id"));
                            BOFile file = new BOFile();
                            file.MimeType = rdr.GetAttribute("mime_type");

                            FileInfo fi = new FileInfo(rdr.GetAttribute("name"));
                            file.Extension = fi.Extension.Replace(".", "");

                            if (string.IsNullOrEmpty(file.Extension))
                                file.Name = fi.Name.TrimEnd('.');
                            else 
                                file.Name = fi.Name.Replace(file.Extension, "").TrimEnd('.');

                            file.Size = Int32.Parse(rdr.GetAttribute("size"));
                            file.Folder = folder;

                            string fullPath = FilesImportFolder + "\\" + importedFileId + "\\" + file.Name + "." + file.Extension;

                            StringBuilder info = new StringBuilder();
                            //info.Append(' ', depth);
                            if (System.IO.File.Exists(fullPath))
                            {
                                byte[] fileBytes = File.ReadAllBytes(fullPath);
                                file.File = fileBytes;

                                fileSystemB.Change(file);

                                if ( !oldNewFileIds.ContainsKey( importedFileId ))
                                    oldNewFileIds.Add(importedFileId, file.Id.Value);

                                Console.WriteLine("Imported file: " + file.Id);
                                info.Append("  -- " + file.Id);
                            }
                            else
                                info.Append("  -- Missing File data for " + file);
                        }
                    }
                }
            }

            // ok all recipes and files have been retreived. files have been stored to db
            // now process recipe files.

            foreach(BORecipe recipe in importedRecipes)
            {
                recipe.Preparation = FixRecipeImages(recipe.Preparation, oldNewFileIds);
                recipe.ImageLink = FindTeaserImage(recipe.Preparation);
                recipeB.ChangeRecipe(recipe);
                Console.WriteLine("Stored Recipe ID=" + recipe.Id.Value);
            }
        }

        private static string FixArticleImages(string raw, IDictionary<int, int> oldFileIdVsNewFileId )
        {
            // From BuildInternalStructures
            string tagRegex = "<{1,1}(\\w+\\b)(([ ]*(\\w+)[ ]*=[ ]*\\\"[^<>\\\"]*?\\\")+)[ ]*/*>{1,1}";
            string attrRegex = "(\\w+)[ ]*=[ ]*\\\"([^<>\\\"]+?)\\\"[ ]*";

            Regex tagFinder = new Regex(tagRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex attrFinder = new Regex(attrRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            MatchCollection matches = tagFinder.Matches(raw);
            foreach (Match match in matches)
            {
                string originalStr = match.Groups[0].ToString().Trim();
                string tagName = match.Groups[1].ToString().Trim();
                string attributes = match.Groups[2].ToString();
                MatchCollection attrMatches = attrFinder.Matches(attributes);

                if (tagName.ToLower() == "img")
                {
                    int newFileId = 0;
                    string alt = string.Empty;
                    string cssClass = string.Empty;
                    int width = 0;
                    int height = 0;

                    foreach (Match attrMatch in attrMatches)
                    {
                        string attrName = attrMatch.Groups[1].Value;
                        string attrValue = attrMatch.Groups[2].Value;

                        switch (attrName.ToLower())
                        {
                            case "src":
                                {
                                    if (attrValue.Contains("aspx?fileid"))
                                    {
                                        int oldFileId = GetOldFileId(attrValue);
                                        newFileId = oldFileIdVsNewFileId[oldFileId];
                                    }
                                    else if (attrValue.Contains("/?fileid"))
                                    {
                                        int oldFileId = GetOldFileId(attrValue);
                                        newFileId = oldFileIdVsNewFileId[oldFileId];
                                    }
                                    break;
                                }
                            case "alt":
                                alt = attrValue;
                                break;
                            case "class":
                                cssClass = attrValue;
                                break;
                            case "height":
                                int.TryParse(attrValue, out height);
                                break;
                            case "width":
                                int.TryParse(attrValue, out width);
                                break;
                        }
                    }

                    BOFile file = fileSystemB.Get(newFileId);
                    string newString = @"<img src=""/_files/" + newFileId + @"/" + file.Name + "." + file.Extension;
                                       
                    if (width > 0)
                        newString += @"?w=" + width;
                    if (height > 0)
                    {
                        if (width <= 0)
                            newString += @"?h=" + height;
                        else
                            newString += @"&h=" + height;
                    }

                    newString += @""" alt=""" + alt + @""" class=""" + cssClass + @""" />";

                    raw = raw.Replace(originalStr, newString);
                }
            }

            return raw;            
        }

        private static string FixRecipeImages(string raw, IDictionary<int, int> oldNewFileIds )
        {
            // From BuildInternalStructures
            string tagRegex = "<{1,1}(\\w+\\b)(([ ]*(\\w+)[ ]*=[ ]*\\\"[^<>\\\"]*?\\\")+)[ ]*/*>{1,1}";
            string attrRegex = "(\\w+)[ ]*=[ ]*\\\"([^<>\\\"]+?)\\\"[ ]*";

            Regex tagFinder = new Regex(tagRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex attrFinder = new Regex(attrRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            MatchCollection matches = tagFinder.Matches(raw);
            foreach (Match match in matches)
            {
                string originalStr = match.Groups[0].ToString().Trim();
                string tagName = match.Groups[1].ToString().Trim();
                string attributes = match.Groups[2].ToString();
                MatchCollection attrMatches = attrFinder.Matches(attributes);

                if (tagName.ToLower() == "img")
                {
                    int newFileId = 0;
                    string alt = string.Empty;
                    string cssClass = string.Empty;
                    int width = 0;
                    int height = 0;

                    foreach (Match attrMatch in attrMatches)
                    {
                        string attrName = attrMatch.Groups[1].Value;
                        string attrValue = attrMatch.Groups[2].Value;

                        switch (attrName.ToLower())
                        {
                            case "src":
                                {
                                    if (attrValue.Contains("aspx?fileid"))
                                    {
                                        int oldFileId = GetOldFileId(attrValue);
                                        newFileId = oldNewFileIds[oldFileId];
                                    }
                                    break;
                                }
                            case "alt":
                                alt = attrValue;
                                break;
                            case "class":
                                cssClass = attrValue;
                                break;
                            case "height":
                                int.TryParse(attrValue, out height);
                                break;
                            case "width":
                                int.TryParse(attrValue, out width);
                                break;
                        }
                    }

                    BOFile file = fileSystemB.Get(newFileId);
                    string newString = @"<img src=""/_files/" + newFileId + @"/" + file.Name + "." + file.Extension;

                    if (width > 0)
                        newString += @"?w=" + width;
                    if (height > 0)
                    {
                        if (width <= 0)
                            newString += @"?h=" + height;
                        else
                            newString += @"&h=" + height;
                    }

                    newString += @""" alt=""" + alt + @""" class=""" + cssClass + @""" />";

                    raw = raw.Replace(originalStr, newString);

                    raw = raw.Replace(originalStr, newString);
                }
            }

            return raw;
        }

        private static int GetOldFileId(string attributeValue)
        {
    		Regex fileIdFinder = new Regex("fileid=(\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            int temp = 0;
            MatchCollection matches = fileIdFinder.Matches(attributeValue);
            if (matches != null && matches.Count > 0)
            {
                int.TryParse(matches[0].Groups[1].Value, out temp);
            }
            return temp;
        }

        private static string FindTeaserImage(string raw)
        {
            string teaserImage = string.Empty;

            // From BuildInternalStructures
            string tagRegex = "<{1,1}(\\w+\\b)(([ ]*(\\w+)[ ]*=[ ]*\\\"[^<>\\\"]*?\\\")+)[ ]*/*>{1,1}";
            string attrRegex = "(\\w+)[ ]*=[ ]*\\\"([^<>\\\"]+?)\\\"[ ]*";

            Regex tagFinder = new Regex(tagRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex attrFinder = new Regex(attrRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            MatchCollection matches = tagFinder.Matches(raw);
            foreach (Match match in matches)
            {
                string tagName = match.Groups[1].ToString().Trim();
                string attributes = match.Groups[2].ToString();
                MatchCollection attrMatches = attrFinder.Matches(attributes);

                if (tagName.ToLower() == "img")
                {
                    foreach (Match attrMatch in attrMatches)
                    {
                        string attrName = attrMatch.Groups[1].Value;
                        string attrValue = attrMatch.Groups[2].Value;

                        if (attrName.ToLower() == "src")
                        {
                            teaserImage = attrValue;
                            break;
                        }
                    }
                }
            }
            return teaserImage;
        }

        protected static string GetSingularPlural(string rawLabels, int position)
        {
            string currentLabel = string.Empty;
            string[] labels = rawLabels.Split(';');

            for (int i = 0; i < labels.Length; i++)
            {
                if (i == (position - 1))
                {
                    currentLabel = labels[i];
                    break;
                }
                // If we are at the end of the array and position we seek is greater 
                // than the count we have reached then return the last available value.
                else if ((i == (labels.Length - 1)) && ((position - 1) > i))
                {
                    currentLabel = labels[i];
                    break;
                }
            }
            return currentLabel;
        }

        private static Dictionary<int, int> GetRegularFolderMap()
        {
            Dictionary<int, int> map = new Dictionary<int, int>(); // new regular id, folder id

            map.Add(115, 11608);
            map.Add(116, 11608);
            map.Add(117, 11608);
            map.Add(118, 11608);
            map.Add(119, 11608);
            map.Add(120, 11608);
            map.Add(121, 11608);
            map.Add(122, 11608);
            map.Add(123, 11608);
            map.Add(124, 11608);
            map.Add(125, 11608);
            map.Add(126, 11608);
            map.Add(127, 11608);
            map.Add(128, 11609);
            map.Add(129, 11610);
            map.Add(130, 11611);
            map.Add(131, 11612);
            map.Add(132, 11613);
            map.Add(133, 11614);
            map.Add(134, 11615);
            map.Add(135, 11616);
            map.Add(136, 11618);
            map.Add(137, 11619);
            map.Add(138, 11620);
            map.Add(139, 11621);
            map.Add(140, 11622);
            map.Add(141, 11622);
            map.Add(142, 11622);
            map.Add(143, 11622);
            map.Add(144, 11622);
            map.Add(145, 11622);
            map.Add(146, 11635);
            map.Add(147, 11636);
            map.Add(148, 11637);
            map.Add(149, 11638);
            map.Add(150, 11639);
            map.Add(151, 11640);

            return map;
        }

        private static Dictionary<int, int> GetRegularMap()
        {
            Dictionary<int, int> map = new Dictionary<int, int>(); // old regular id, new regular id

            map.Add(96, 115);
            map.Add(51, 116);
            map.Add(69, 117);
            map.Add(99, 118);
            map.Add(18, 119);
            map.Add(27, 120);
            map.Add(44, 121);
            map.Add(95, 122);
            map.Add(21, 123);
            map.Add(103, 124);
            map.Add(45, 125);
            map.Add(13, 126);
            map.Add(22, 127);
            map.Add(31, 128);
            map.Add(105, 129);
            map.Add(33, 130);
            map.Add(100, 131);
            map.Add(34, 132);
            map.Add(32, 133);
            map.Add(102, 134);
            map.Add(4, 135);
            map.Add(3, 136);
            map.Add(43, 137);
            map.Add(14, 138);
            map.Add(42, 139);
            map.Add(28, 140);
            map.Add(68, 141);
            map.Add(19, 142);
            map.Add(29, 143);
            map.Add(30, 144);
            map.Add(5, 145);
            map.Add(26, 146);
            map.Add(23, 147);
            map.Add(101, 148);
            map.Add(25, 149);
            map.Add(24, 150);
            map.Add(46, 151);

            return map;
        }

        public void ImportArticles()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            XmlTextReader rdr = new XmlTextReader(ArticlesImportFileName);
            rdr.WhitespaceHandling = WhitespaceHandling.None;

            Dictionary<int, BORegular> regularMapping = new Dictionary<int, BORegular>();
            Dictionary<BORecipe, List<int>> importedFileIds = new Dictionary<BORecipe, List<int>>();
            Dictionary<int, int> oldNewFileIds = new Dictionary<int, int>();

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("regulars"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("regulars")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("regular"))
                        {
                            BORegular regular = new BORegular();
                            int regularId = Int32.Parse(rdr.GetAttribute("id"));
                            regular.Id = regularId;
                            regular.LanguageId = languageId;
                            regular.Title = ""; regular.SubTitle = "";
                            regular.Teaser = ""; regular.Html = "";
                            if (rdr.ReadToFollowing("title"))
                                regular.Title = rdr.ReadString();
                            if (rdr.ReadToFollowing("subtitle"))
                                regular.SubTitle = rdr.ReadString();

                            articleB.ChangeRegular(regular);
                            regularMapping.Add(regularId, regular);
                            Console.WriteLine("R-" + regular.Id + " " + regular.Title);
                        }
                    }
                }
                else if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("articles"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("articles")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("article"))
                        {
                            string articleId = rdr.GetAttribute("id");

                            BOArticle article = new BOArticle();
                            article.Id = Int32.Parse(rdr.GetAttribute("id"));
                            article.IsChanged = true;
                            article.PublishFlag = false;
                            article.DisplayDate = DateTime.Parse(rdr.GetAttribute("published_date"));
                            int regularId = Int32.Parse(rdr.GetAttribute("regular_id"));
                            BORegular reg = regularMapping[regularId];
                            article.Regulars.Add(reg);
                            rdr.Read();
                            ReadContent(rdr, article, null);
                            articleB.ChangeArticle(article);
                            Console.WriteLine("A-" + articleId + " " + article.Title);
                        }
                    }
                }
            }
            Console.WriteLine("end");
        }

        public void ImportArticlesSpecial()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageId);
            XmlTextReader rdr = new XmlTextReader(ArticlesImportFileName);
            rdr.WhitespaceHandling = WhitespaceHandling.None;

            Dictionary<int, BORegular> regularMapping = new Dictionary<int, BORegular>(); // newly mapped regular Id against regular object
            Dictionary<int, int> regularIdVsFolderId = GetRegularFolderMap();

            List<BOArticle> importedArticles = new List<BOArticle>(); // articles are stored here while all files are gathered and entered into db. then, article html and teaser have their old fileids replaced with new ones.
            Dictionary<BOArticle, List<BOComment>> importedComments = new Dictionary<BOArticle, List<BOComment>>(); // mapping article comments
            Dictionary<int, int> regular13VsRegular16Map = GetRegularMap(); // hardcoded values from email
            Dictionary<int, int> oldFileIdVsNewRegularId = new Dictionary<int, int>(); // so that we know into which folder to store the file
            Dictionary<int, int> oldFileIdVsNewFileId = new Dictionary<int, int>();
            Dictionary<BOArticle, List<int>> importedArticleFileIds = new Dictionary<BOArticle, List<int>>();

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("regulars"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("regulars")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("regular"))
                        {
                            BORegular regular = new BORegular();
                            int regularId = Int32.Parse(rdr.GetAttribute("id"));
                            regularId = regular13VsRegular16Map[regularId]; // if special import... then you want to assigned the 1.6 regularId from map.
                            regular.Id = regularId;
                            regular.LanguageId = languageId;
                            regular.Title = ""; regular.SubTitle = "";
                            regular.Teaser = ""; regular.Html = "";
                            if (rdr.ReadToFollowing("title"))
                                regular.Title = rdr.ReadString();
                            if (rdr.ReadToFollowing("subtitle"))
                                regular.SubTitle = rdr.ReadString();

                            articleB.ChangeRegular(regular);
                            regularMapping.Add(regularId, regular);
                            Console.WriteLine("R-" + regular.Id + " " + regular.Title);
                        }
                    }
                }
                else if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("articles"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("articles")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("article"))
                        {
                            string articleId = rdr.GetAttribute("id");
                            int oldContentId = Int32.Parse(rdr.GetAttribute("contentId"));
                            BOArticle article = new BOArticle();
                            article.Id = null; //Int32.Parse(rdr.GetAttribute("id"));
                            article.IsChanged = true;
                            article.PublishFlag = false;
                            article.DisplayDate = DateTime.Parse(rdr.GetAttribute("published_date"));
                            int regularId = Int32.Parse(rdr.GetAttribute("regular_id"));
                            regularId = regular13VsRegular16Map[regularId]; // if special import... then you want to assigne the 1.6 regularId from map.
                            BORegular reg = regularMapping[regularId];
                            article.Regulars.Add(reg);

                            article.LanguageId = languageId;
                            article.Title = "";
                            article.SubTitle = "";
                            article.Teaser = "";
                            article.Html = "";

                            if (rdr.Read() && rdr.Name.Equals("content") && !rdr.IsEmptyElement)
                            {
                                if (rdr.ReadToFollowing("title"))
                                    article.Title = rdr.ReadString();
                                if (rdr.ReadToFollowing("subtitle"))
                                    article.SubTitle = rdr.ReadString();
                                if (rdr.ReadToFollowing("teaser"))
                                    article.Teaser = rdr.ReadString();
                                if (rdr.ReadToFollowing("html"))
                                    article.Html = rdr.ReadString();

                                Console.WriteLine("->Title:" + article.Title);
                                rdr.Read();
                            }

                            if ( rdr.Read() && rdr.Name.Equals("files") && !rdr.IsEmptyElement)
                            {
                                List<int> fileIds = new List<int>();
                                while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
                                {
                                    if (rdr.Name.Equals("file"))
                                    {
                                        int fileId = Int32.Parse(rdr.GetAttribute("id"));
                                        if (!oldFileIdVsNewRegularId.ContainsKey(fileId))
                                            oldFileIdVsNewRegularId.Add(fileId, regularId); // storing oldFileId against new regular Id ( so that we know which regular is associated with which old fileId ... this will later be used to know where to store files
                                        fileIds.Add(fileId);
                                        Console.Write(fileId + ",");
                                    }
                                }
                                importedArticleFileIds.Add(article, fileIds);
                            }

                            List<BOComment> comments = new List<BOComment>();                            
                            if (rdr.Read() && rdr.Name.Equals("comments") && !rdr.IsEmptyElement)
                            {
                                while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("comments")))
                                {
                                    if (rdr.Name.Equals("comment"))
                                    {
                                        BOComment comment = new BOComment();

                                        comment.Changed = true;
                                        comment.Comment = rdr.GetAttribute("comment");
                                        comment.CommentedAt = DateTime.Parse(rdr.GetAttribute("date_submitted"));
                                        comment.Email = rdr.GetAttribute("email");
                                        comment.Id = null;
                                        comment.IsNew = true;
                                        comment.MarkedForDeletion = false;
                                        comment.Name = rdr.GetAttribute("name");
                                        comment.Publish = false;
                                        comment.Title = rdr.GetAttribute("title");

                                        comments.Add(comment);
                                    }
                                }
                            }
                            importedComments.Add(article, comments);
                            importedArticles.Add(article);
                            Console.WriteLine("A-" + articleId + " " + article.Title);
                        }
                    }
                }
                else if (rdr.NodeType == XmlNodeType.Element && rdr.LocalName.Equals("files"))
                {
                    while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
                    {
                        if (rdr.NodeType == XmlNodeType.Element && rdr.Name.Equals("file"))
                        {
                            int importedFileId = Int32.Parse(rdr.GetAttribute("id"));
                            Console.WriteLine("OFID ID=" + importedFileId);
                            int regularIdAssociatedWithOldFileId = oldFileIdVsNewRegularId[importedFileId];
                            int folderIdAssociatedWithRegularId = regularIdVsFolderId[regularIdAssociatedWithOldFileId];
                            BOCategory folder = fileSystemB.GetFolder(folderIdAssociatedWithRegularId);
                            if ( folder == null )
                                folder = fileSystemB.GetFolder(FolderId);
                            if (folder == null)
                                folder = fileSystemB.GetFolder(3);
                            if ( folder == null)
                                throw new Exception("no folder to enter files to");

                            BOFile file = new BOFile();
                            file.MimeType = rdr.GetAttribute("mime_type");

                            FileInfo fi = new FileInfo(rdr.GetAttribute("name"));
                            file.Extension = fi.Extension.Replace(".", "");

                            if (string.IsNullOrEmpty(file.Extension))
                                file.Name = fi.Name.TrimEnd('.');
                            else
                                file.Name = fi.Name.Replace(file.Extension, "").TrimEnd('.');

                            file.Size = Int32.Parse(rdr.GetAttribute("size"));
                            file.Folder = folder;

                            string fullPath = FilesImportFolder + "\\" + importedFileId + "\\" + file.Name + "." + file.Extension;

                            StringBuilder info = new StringBuilder();
                            //info.Append(' ', depth);
                            if (System.IO.File.Exists(fullPath))
                            {
                                byte[] fileBytes = File.ReadAllBytes(fullPath);
                                file.File = fileBytes;

                                fileSystemB.Change(file);

                                if (!oldFileIdVsNewFileId.ContainsKey(importedFileId))
                                    oldFileIdVsNewFileId.Add(importedFileId, file.Id.Value);

                                Console.WriteLine("Imported file: " + file.Id);
                                info.Append("  -- " + file.Id);
                            }
                            else
                                info.Append("  -- Missing File data for " + file);
                        }
                    }
                }
            }

            // ok all articles and files have been retreived. files have been stored to db
            // now process article files.
            foreach (BOArticle article in importedArticles)
            {
                List<BOComment> comments = importedComments[article];

                article.Html = FixArticleImages(article.Html, oldFileIdVsNewFileId);
                article.Teaser = FixArticleImages(article.Teaser, oldFileIdVsNewFileId);
                articleB.ChangeArticle(article);

                Console.WriteLine("Stored article ID=" + article.Id.Value);

                foreach (BOComment comment in comments)
                {
                    comment.ContentId = article.ContentId.Value;
                    commentB.Change(comment);
                }

                Console.WriteLine("Stored comments of article ID=" + article.Id.Value);
            }

            Console.WriteLine("end");
        }

        private void ReadContent(XmlReader rdr, BOInternalContent content, Dictionary<int, int> articleContentIdRegularMap)
        {
            content.LanguageId = languageId;
            content.Title = "";
            content.SubTitle = "";
            content.Teaser = "";
            content.Html = "";
            if (!rdr.Name.Equals("content"))
            {
                Console.WriteLine("incorrect call to ReadContent");
                return;
            }

            int contentId = -1;
            if (rdr.ReadToFollowing("contentId"))
                contentId = Int32.Parse(rdr.ReadString());
            if (rdr.ReadToFollowing("title"))
                content.Title = rdr.ReadString();
            if (rdr.ReadToFollowing("subtitle"))
                content.SubTitle = rdr.ReadString();
            if (rdr.ReadToFollowing("teaser"))
                content.Teaser = rdr.ReadString();
            if (rdr.ReadToFollowing("html"))
                content.Html = rdr.ReadString();
            if (rdr.ReadToFollowing("files") && !rdr.IsEmptyElement)
            {
                while (rdr.Read() && !(rdr.NodeType == XmlNodeType.EndElement && rdr.Name.Equals("files")))
                {
                    if(rdr.Name.Equals("file"))
                    {
                        int fileId = Int32.Parse(rdr.GetAttribute("id"));
                        Console.Write(fileId + ",");
                    }
                }
            }
        }
    }
}

