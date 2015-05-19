using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.Web.Caching;
using MsSqlDBUtility;
using System.Security;
// for old stuff

namespace One.Net.BLL.DAL
{
	public class DbWebsite
	{
        public PagedList<BOPage> ListPages(int webSiteId, int languageId, bool publishFlag, bool filterChanged = false)
        {
            PagedList<BOPage> pages = new PagedList<BOPage>();

            var paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@websiteID", webSiteId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);
            paramsToPass[2] = new SqlParameter("@publishFlag", publishFlag);


            string sql = @"SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
            c.principal_modified_by, c.date_modified, c.votes, c.score, c.id ContentId, p.id PageId, p.pages_fk_id, p.menu_group, p.idx, 
			t.id TemplateId, t.name, level, pending_delete, changed, 
            break_persistence, web_site_fk_id, redirectToUrl, [viewGroups], [editGroups], [requireSSL], sub_route_url
     FROM [dbo].[pages] p	
     INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id = p.content_fk_id AND cds.language_fk_id = @languageId
     INNER JOIN [dbo].[content] c ON c.id = p.content_fk_id
     INNER JOIN [dbo].[template] t ON t.id = p.template_fk_id AND template_type = '3'
	   WHERE p.publish = @publishFlag AND p.web_site_fk_id=@websiteID";
                if (filterChanged)
                {
                    sql += " AND p.changed = 1";
                }
                    
           sql += " ORDER BY PageId ASC";

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                sql, paramsToPass))
            {
                while (rdr.Read())
                {
                    BOPage page = new BOPage();
                    PopulatePage(rdr, page, languageId);
                    LoadPageLinks(page, publishFlag, languageId);
                    LoadPageSettings(page, publishFlag);
                    pages.Add(page);
                }

                if (rdr.NextResult())
                {
                    if (rdr.Read())
                    {
                        pages.AllRecords = rdr.GetInt32(0);
                    }
                }
            }

            return pages;
        }

        [SqlClientPermission(SecurityAction.Demand, Unrestricted = true)]
        public List<BOPage> GetSiteStructure(int webSiteId, int languageId, bool publishFlag)
        {
            List<BOPage> structure = new List<BOPage>();
            SqlParameter[] parms;
            parms = new SqlParameter[] {
					new SqlParameter("@websiteID", webSiteId),
					new SqlParameter("@LCID", languageId),
                    new SqlParameter("@publishFlag", publishFlag)
                    };
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT p.id AS ID, p.pages_fk_id AS Parent, CAST(p.publish AS int) AS publish, il.par_link, sc2.title AS Title, sc2.subtitle AS SubTitle, sc2.teaser AS Teaser, sub_route_url,
                    t.name AS Template, t.id AS TemplateID, p.content_fk_id AS ContentID, menu_group, idx, changed, pending_delete, 
                    p.level, p.redirectToUrl,  [viewGroups], [editGroups], [requireSSL], date_modified, date_created
                     FROM [dbo].pages p
                     INNER JOIN [dbo].content  sc1 ON sc1.id = p.content_fk_id 
                     INNER JOIN [dbo].content_data_store sc2 ON sc2.content_fk_id = p.content_fk_id AND sc2.language_fk_id = @LCID
                     INNER JOIN [dbo].template t ON t.id = p.template_fk_id
                     INNER JOIN [dbo].int_link il ON il.pages_fk_id = p.id AND il.pages_fk_publish= @publishFlag AND il.language_fk_id = @LCID
                     WHERE p.publish = @publishFlag AND p.web_site_fk_id = @websiteID
                     ORDER BY p.pages_fk_id, p.idx", parms)) //out sqlCacheDep,
            // beware of the p.idx order: it should be OK as long as long as pages are added using a sequenced id order.
            // if order is incorrect, we'll get an exception later when building sitemap
            {
                int _indexID = rdr.GetOrdinal("ID");
                int _indexUrl = rdr.GetOrdinal("par_link");
                int _indexTitle = rdr.GetOrdinal("Title");
                int _indexSubTitle = rdr.GetOrdinal("SubTitle");
                int _indexTeaser = rdr.GetOrdinal("Teaser");
                int _indexParent = rdr.GetOrdinal("Parent");
                int _indexPublish = rdr.GetOrdinal("publish");
                int _indexTemplate = rdr.GetOrdinal("Template");
                int _indexTemplateID = rdr.GetOrdinal("TemplateID");
                int _indexContentID = rdr.GetOrdinal("ContentID");
                int _indexMenuGroup = rdr.GetOrdinal("menu_group");
                int _indexOrder = rdr.GetOrdinal("idx");
                int _indexChanged = rdr.GetOrdinal("changed");
                int _indexPendingDelete = rdr.GetOrdinal("pending_delete");
                int _indexLevel = rdr.GetOrdinal("level");
                int _indexRedirect = rdr.GetOrdinal("redirectToUrl");

                int _indexViewGroups = rdr.GetOrdinal("viewGroups");
                int _indexEditGroups = rdr.GetOrdinal("editGroups");
                int _indexRequireSSL = rdr.GetOrdinal("requireSSL");

                int _sub_route_url = rdr.GetOrdinal("sub_route_url");

                


                while (rdr.Read())
                {
                    BOPage sitePage = new BOPage();
                    sitePage.Id = rdr.GetInt32(_indexID);

                    if (rdr[_indexParent] != DBNull.Value)
                    {
                        sitePage.ParentId = rdr.GetInt32(_indexParent);
                    }

                    sitePage.MenuGroup = rdr.GetInt32(_indexMenuGroup);
                    sitePage.Order = rdr.GetInt32(_indexOrder);
                    sitePage.LanguageId = languageId;
                    sitePage.ContentId = rdr.GetInt32(_indexContentID);
                    sitePage.Title = rdr[_indexTitle] != DBNull.Value ? rdr.GetString(_indexTitle) : "";
                    sitePage.SubTitle = rdr[_indexSubTitle] != DBNull.Value ? rdr.GetString(_indexSubTitle) : "";
                    sitePage.Teaser = rdr[_indexTeaser] != DBNull.Value ? rdr.GetString(_indexTeaser) : "";


                    sitePage.ParLink = rdr.GetString(_indexUrl);
                    sitePage.Template = new BOTemplate();
                    sitePage.Template.Type = "3";
                    sitePage.Template.Id = rdr.GetInt32(_indexTemplateID);
                    sitePage.Template.Name = rdr.GetString(_indexTemplate);
                    sitePage.IsChanged = rdr.GetBoolean(_indexChanged);
                    sitePage.MarkedForDeletion = rdr.GetBoolean(_indexPendingDelete);
                    sitePage.Level = rdr.GetInt32(_indexLevel);
                    sitePage.RedirectToUrl = rdr.GetString(_indexRedirect);
                    sitePage.FrontEndRequireGroupList = rdr.GetString(_indexViewGroups);
                    sitePage.EditRequireGroupList = rdr.GetString(_indexEditGroups);
                    sitePage.RequireSSL = rdr.GetBoolean(_indexRequireSSL);
                    if (rdr[_sub_route_url] != DBNull.Value)
                        sitePage.SubRouteUrl = rdr.GetString(_sub_route_url);

                    sitePage.DateCreated = (DateTime) rdr["date_created"];
                    sitePage.DateModified = rdr["date_modified"] == DBNull.Value ? new DateTime?() : (DateTime) rdr["date_modified"];
                    structure.Add(sitePage);
                }
            }

            return structure;
        }

        public List<int> ListChildrenIds(int pageId, bool publishFlag)
        {
            List<int> pageIds = new List<int>();
            
            SqlParameter[] parms = 
                   new SqlParameter[] {
                           new SqlParameter("@pageId", pageId),
                           new SqlParameter("@PublishFlag", publishFlag)};
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT p.id AS ID
                     FROM [dbo].pages p
                     WHERE p.pages_fk_id = @pageId AND p.publish = @PublishFlag
                     ORDER BY p.idx", parms))
            {
                while (rdr.Read())
                {
                    pageIds.Add(rdr.GetInt32(0));
                }
            }

            return pageIds;
        }

	    public List<BOWebSite> List()
		{
			List<BOWebSite> websiteList = new List<BOWebSite>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT w.id, w.content_fk_id, 
	                    (SELECT p.id FROM [dbo].[pages] p 
	                     WHERE publish = 0 AND web_site_fk_id = w.id AND pages_fk_id IS NULL) AS rootPageId
                    FROM [dbo].[web_site] w"))
            {
                while (rdr.Read())
                {
                    BOWebSite website = new BOWebSite();
                    website.Id = rdr.GetInt32(0);
                    website.ContentId = rdr.GetInt32(1);
                    if (rdr[2] != DBNull.Value)
                        website.RootPageId = rdr.GetInt32(2);
                    websiteList.Add(website);
                }
            }

            foreach (BOWebSite site in websiteList)
            {
                SqlParameter paramsToPass = new SqlParameter("@webSiteID", site.Id);
                using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT sl.name, ISNULL( ws.value, sl.default_value), sl.type, sl.user_visibility
                    FROM [dbo].[settings_list] sl 
                    LEFT JOIN [dbo].[web_site_settings] ws ON  sl.id = ws.settings_list_fk_id AND web_site_fk_id = @webSiteID
                    WHERE sl.[subsystem] = 'WebSite'", paramsToPass))
                {
                    while (rdr.Read())
                    {
                        BOSetting setting = new BOSetting(rdr.GetString(0), rdr.GetString(2), rdr.GetString(1), rdr.GetString(3));
                        site.Settings.Add(rdr.GetString(0), setting);
                    }
                }
            }
			return websiteList;
		}

		public void Change(BOWebSite website, string connString = "")
		{
            SqlParameter[] parms;
            if (website.IsNew)
            {
                if (!website.ContentId.HasValue)
                {
                    throw new InvalidOperationException("add Content before calling add website");
                }
                parms = new SqlParameter[] { 
                    new SqlParameter("@ContentId", website.ContentId),
                    new SqlParameter("@id", SqlDbType.Int)
                };
                parms[1].Direction = ParameterDirection.Output;
                parms[1].DbType = DbType.Int32;

                SqlHelper.ExecuteNonQuery(string.IsNullOrWhiteSpace(connString) ? SqlHelper.ConnStringMain : connString, CommandType.Text,
                @"INSERT INTO [dbo].[web_site] (content_fk_id) VALUES (@ContentId); SET @id=SCOPE_IDENTITY()", parms);
                website.Id = (int)parms[1].Value;

                parms = new SqlParameter[] {
                    new SqlParameter("@Id", website.Id),
                    new SqlParameter("@Name", "PrimaryLanguageId"),
                    new SqlParameter("@Value", website.LanguageId)
                };

                SqlHelper.ExecuteNonQuery(string.IsNullOrWhiteSpace(connString) ? SqlHelper.ConnStringMain : connString, CommandType.StoredProcedure, "[dbo].[ChangeWebSiteSetting]", parms);
            }

            parms = new SqlParameter[] {
                new SqlParameter("@Id", website.Id),
                new SqlParameter("@Name", SqlDbType.VarChar, 255),
                new SqlParameter("@Value", SqlDbType.VarChar, 4000)
            };

            foreach (BOSetting setting in website.Settings.Values)
            {
                parms[1].Value = setting.Name;
                parms[2].Value = setting.Value;
                SqlHelper.ExecuteNonQuery(string.IsNullOrWhiteSpace(connString) ? SqlHelper.ConnStringMain : connString, CommandType.StoredProcedure, "ChangeWebSiteSetting", parms);
            }
		}

        /* ----------------------------------------------------------- */

        public void ChangePage(BOPage page)
        {
            if (page == null)
                return;

            var p = new SqlParameter[3]  {
                new SqlParameter("@WebSiteId", page.WebSiteId),
                new SqlParameter("@PublishFlag", page.PublishFlag),
                new SqlParameter("@Id", page.Id)
            };
            var exists = (int) SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text, "SELECT COUNT(id) FROM pages WHERE id = @Id AND publish = @PublishFlag AND web_site_fk_id = @WebSiteId", p) > 0;
            var sql = "";
            if (exists)
            {
                sql = @"UPDATE [dbo].[pages] SET break_persistence = @BreakPersistance, [level] = @Level, 
		content_fk_id = @ContentId, template_fk_id = @TemplateId,
		pages_fk_publish = @PublishFlag, pages_fk_id = @ParentId, changed = @Changed, 
		menu_group = @MenuGroup, idx = @Order, web_site_fk_id = @WebSiteId, pending_delete = @PendingDelete,
		redirectToUrl = @redirectToUrl, viewGroups = @viewGroups, editGroups = @editGroups, requireSSL = @requireSSL, sub_route_url = @SubRouteUrl
		WHERE id = @Id AND publish = @PublishFlag";

            }
            else
            {
                if (!page.PublishFlag)
                {
                    p = new SqlParameter[2]  {
                        new SqlParameter("@sequence","pages"),
                        new SqlParameter("@sequence_id", 0)
                    };
                    p[1].Direction = ParameterDirection.Output;
                    SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[nextval]", p);
                    var newId = (long) p[1].Value;
                    page.Id = Convert.ToInt32(newId);
                    if (page.Order == 0)
                        page.Order = Convert.ToInt32(newId);
                }
                sql = @"INSERT INTO pages 
		(id,  break_persistence, level, publish, content_fk_id, template_fk_id,
		pages_fk_publish, pages_fk_id, changed, menu_group, idx, web_site_fk_id, pending_delete,
		redirectToUrl, viewGroups, editGroups, requireSSL, sub_route_url)
		VALUES
		(@Id, @BreakPersistance, @Level, @PublishFlag, @ContentId, @TemplateId,
		@PublishFlag, @ParentId, @Changed, @MenuGroup, @Order, @WebSiteId, @PendingDelete,
		@redirectToUrl, @viewGroups, @editGroups, @requireSSL, @SubRouteUrl )";
            }

            SqlParameter pageParentIdParam = new SqlParameter("@ParentId", SqlDbType.Int);
            pageParentIdParam.IsNullable = true;
            if (page.ParentId.HasValue)
            {
                pageParentIdParam.Value = page.ParentId.Value;
            }
            else
            {
                pageParentIdParam.Value = DBNull.Value;
            }

            var parms = new SqlParameter[] {
				new SqlParameter("@Id", page.Id),
				pageParentIdParam,
                new SqlParameter("@PublishFlag", page.PublishFlag),
                new SqlParameter("@Changed", page.IsChanged),
                new SqlParameter("@Order", page.Order),
                new SqlParameter("@PendingDelete", page.MarkedForDeletion),
                new SqlParameter("@MenuGroup", page.MenuGroup),
                new SqlParameter("@BreakPersistance", page.BreakPersistance),
                new SqlParameter("@Level", page.Level),
                new SqlParameter("@ContentId", page.ContentId),
                new SqlParameter("@TemplateId", page.Template.Id),
                new SqlParameter("@WebSiteId", page.WebSiteId),
                new SqlParameter("@redirectToUrl", page.RedirectToUrl), 
                new SqlParameter("@viewGroups", page.FrontEndRequireGroupList),
                new SqlParameter("@editGroups", page.EditRequireGroupList),
                new SqlParameter("@requireSSL", page.RequireSSL),
                new SqlParameter("@SubRouteUrl", page.SubRouteUrl)
            };

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, parms);
            //page.Id = (int) parms[0].Value;

            parms = new SqlParameter[] {
                new SqlParameter("@Id", page.Id),
				new SqlParameter("@PublishFlag", page.PublishFlag),
                new SqlParameter("@Name", SqlDbType.VarChar, 255),
                new SqlParameter("@Value", SqlDbType.VarChar, 4000)
            };
            foreach (BOSetting setting in page.Settings.Values)
            {
                parms[2].Value = setting.Name;
                parms[3].Value = setting.Value;
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "ChangePageSetting", parms);
            }

            parms = new SqlParameter[] {
                new SqlParameter("@Id", page.Id),
				new SqlParameter("@PublishFlag", page.PublishFlag),
                new SqlParameter("@languageID", page.LanguageId),
                new SqlParameter("@par_link", page.ParLink),
                new SqlParameter("@par_link_name", "[ " + page.ParLink + " ]")
            };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "ChangeIntLink", parms);
            
        }

        public void DeletePage(int pageID, bool publishFlag)
        {
            SqlParameter[] parms = new SqlParameter[] {
                new SqlParameter("@Id", pageID),
				new SqlParameter("@PublishFlag", publishFlag)
            };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure,
                @"DeletePage", parms);
        }

        public BOPage GetPage(int pageId, bool publishFlag, int languageId)
        {
            //cacheDep = null;
            //SqlCacheDependency sqlCacheDep = null;
            BOPage page = null;
            SqlParameter[] parms = new SqlParameter[] {
					new SqlParameter("@pageID", pageId),
					new SqlParameter("@LCID", languageId),
                    new SqlParameter("@publishFlag", publishFlag)
                    };
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT  cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
                          c.principal_modified_by, c.date_modified, c.votes, c.score, c.id ContentId, p.id PageId, p.pages_fk_id, p.menu_group, p.idx,
			              t.id TemplateId, t.name, level, pending_delete, changed, 
                          break_persistence, web_site_fk_id, redirectToUrl, [viewGroups], [editGroups], [requireSSL], p.sub_route_url
                  FROM [dbo].[pages] p	
                  INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id = p.content_fk_id AND cds.language_fk_id = @LCID
                  INNER JOIN [dbo].[content] c ON c.id = p.content_fk_id
                  INNER JOIN [dbo].[template] t ON t.id = p.template_fk_id AND template_type = '3'
	              WHERE p.publish = @publishFlag AND p.id = @pageID", parms))
            {
                //cacheDep = (CacheDependency)sqlCacheDep;

                if (rdr.Read())
                {
                    page = new BOPage();
                    PopulatePage(rdr, page, languageId);
                }
            }
            if (page == null)
            {
                return null;
            }

            if (page.IsChanged && !page.PublishFlag)  
            {
                // under these two conditions it makes sense to check if the page has an online version at all
                object result = SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, CommandType.Text,
                    @"SELECT id FROM [dbo].[pages] WHERE id = @pageID AND publish = 1", parms);
                if (result == null)
                {
                    page.IsNew = true;
                }
            }

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[ListPageModInstances]", parms))
            {
                while (rdr.Read())
                {
                    PopulateModuleInstances(rdr, page);
                }
            }

            LoadPageLinks(page, publishFlag, languageId);
            LoadPageSettings(page, publishFlag);

            

            return page;
        }

        protected void LoadPageLinks(BOPage page, bool publishFlag, int languageId)
        {
            var parms = new SqlParameter[] {
					new SqlParameter("@CurrentNodeID", page.Id),
					new SqlParameter("@CurrentPublishFlag", publishFlag),
                    new SqlParameter("@LanguageID", languageId)
                    };
            string par_link = "";
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[GetPath]", parms))
            {
                while (rdr.Read())
                {
                    par_link = rdr.GetString(1);
                    if (par_link.Length != 0)
                    {
                        page.URI += "/" + par_link;
                    }
                    int parPageId = rdr.GetInt32(0);
                    if (parPageId != page.Id)
                    {
                        page.parentPagesSimpleList += "pp" + parPageId + " ";
                    }
                }
            }
            if (page.IsRoot)
            {
                page.URI = "/";
            }

            page.ParLink = par_link;
        }

        protected void LoadPageSettings(BOPage page, bool publishFlag)
        {
            var parms = new SqlParameter[] {
					new SqlParameter("@PageId", page.Id),
					new SqlParameter("@PublishFlag", publishFlag)
                    };

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT sl.name AS SettingName, ISNULL(ps.value, sl.default_value) as Value,
                         sl.type Type, sl.user_visibility UserVisibility
                  FROM [dbo].[settings_list] sl
                  LEFT JOIN [dbo].[pages_settings] ps ON  sl.id = ps.settings_list_fk_id AND ps.pages_fk_publish=@PublishFlag AND ps.pages_fk_id=@PageId
                  WHERE sl.subsystem='Page'", parms))
            {
                while (rdr.Read())
                {
                    page.Settings.Add(rdr["SettingName"].ToString(), new BOSetting(rdr["SettingName"].ToString(), rdr["Type"].ToString(), rdr["Value"].ToString(), rdr["UserVisibility"].ToString()));
                }
            }
        }

        private static void PopulatePage(IDataReader rdr, BOPage sitePage, int languageId)
        {
            sitePage.ContentId = rdr.GetInt32(10);
            DbHelper.PopulateContent(rdr, sitePage, languageId);

            sitePage.Id = rdr.GetInt32(11);
            if (rdr[12] != DBNull.Value)
            {
                sitePage.ParentId = rdr.GetInt32(12);
            }
            sitePage.MenuGroup = rdr.GetInt32(13);
            sitePage.Order = rdr.GetInt32(14);
            sitePage.Template = new BOTemplate();
            sitePage.Template.Id = rdr.GetInt32(15);
            sitePage.Template.Name = rdr.GetString(16);
            sitePage.Level = rdr.GetInt32(17);
            sitePage.MarkedForDeletion = rdr.GetBoolean(18);
            sitePage.IsChanged = rdr.GetBoolean(19);
            sitePage.BreakPersistance = rdr.GetBoolean(20);
            sitePage.WebSiteId = rdr.GetInt32(21);
            sitePage.RedirectToUrl = rdr.GetString(22);
            sitePage.FrontEndRequireGroupList = rdr.GetString(23);
            sitePage.EditRequireGroupList = rdr.GetString(24);
            sitePage.RequireSSL = rdr.GetBoolean(25);
            if (rdr["sub_route_url"] != DBNull.Value)
                sitePage.SubRouteUrl = (string) rdr["sub_route_url"];
            else
                sitePage.SubRouteUrl = "";
        }

        private static void PopulateModuleInstances(IDataReader rdr, BOPage sitePage)
        {
            if (rdr[7] != DBNull.Value && rdr.GetInt32(2) == 1)
            {
                var moduleInstance = new BOModuleInstance { Id = rdr.GetInt32(7), ModuleId = rdr.GetInt32(8), Order = rdr.GetInt32(9), PageId = rdr.GetInt32(10), Name = rdr.GetString(11), PlaceHolderId =  rdr.GetInt32(12) };
                moduleInstance.IsInherited = rdr.GetInt32(1) != sitePage.Level;

                if (!sitePage.PlaceHolders.ContainsKey(moduleInstance.PlaceHolderId))
                {
                    BOPlaceHolder placeHolder = new BOPlaceHolder();
                    placeHolder.Id = moduleInstance.PlaceHolderId;
                    placeHolder.Name = rdr.GetString(13);
                    sitePage.PlaceHolders.Add(moduleInstance.PlaceHolderId, placeHolder);
                }
                if (!(sitePage.BreakPersistance && moduleInstance.IsInherited))
                {
                    sitePage.PlaceHolders[moduleInstance.PlaceHolderId].ModuleInstances.Add(moduleInstance);
                }
            }
        }

        public string GetPageUri(int pageId, bool publishFlag, int languageId)
        {
            string answer = "/";
            string nonDefaultLanguageLink = "";
            SqlParameter[] paramsToPass = new SqlParameter[3];
            paramsToPass[0] = new SqlParameter("@CurrentNodeID", pageId);
            paramsToPass[1] = new SqlParameter("@CurrentPublishFlag", publishFlag);
            paramsToPass[2] = new SqlParameter("@LanguageID", languageId);
            int i = 0;

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.StoredProcedure, "[dbo].[GetPath]", paramsToPass))
            {
                while (rdr.Read())
                {
                    string par_link = rdr.GetString(1);
                    
                    if (par_link.Length > 0 && (i == 0))
                    {
                        // We are on not on a root page for non-default language
                        nonDefaultLanguageLink = par_link;
                    }
                    else if (par_link.Length > 0)
                    {
                        answer += par_link + "/";
                    }
                }
            }
            if (i == 1 && nonDefaultLanguageLink.Length > 0)
            {
                answer += nonDefaultLanguageLink;
            }
            if (answer.Length > 1)
            {
                answer = answer.TrimEnd('/');
            }
            return answer;
        }

        public void DeleteTemplate(int templateId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@Id", templateId);

            var sql = @"DELETE FROM [dbo].[template] WHERE id=@Id";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

        public void ChangeTemplate(BOTemplate template)
        {
            SqlParameter[] paramsToPass = new SqlParameter[4];

            paramsToPass[0] = (template.Id.HasValue ? new SqlParameter("@Id", template.Id) : new SqlParameter("@Id", DBNull.Value));
            paramsToPass[1] = new SqlParameter("@Name", template.Name);
            paramsToPass[2] = new SqlParameter("@Type", template.Type);
            paramsToPass[3] = !string.IsNullOrEmpty(template.TemplateContent) ? new SqlParameter("@Content", template.TemplateContent) : new SqlParameter("@Content", System.DBNull.Value);

            string sql;
            if (template.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[template] SET name=@Name, template_type=@Type, content=@Content WHERE id=@Id";
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"INSERT INTO [dbo].[template]  (name, template_type, content) VALUES (@Name, @Type, @Content); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            if (!template.Id.HasValue)
            {
                template.Id = (int)paramsToPass[0].Value;
            }
        }


        public List<BOPlaceHolder> ListPlaceHolders()
        {
            List<BOPlaceHolder> placeHolders = new List<BOPlaceHolder>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, @"SELECT id, place_holder_id FROM [dbo].[place_holder] "))
            {
                while (rdr.Read())
                {
                    BOPlaceHolder placeHolder = new BOPlaceHolder();
                    placeHolder.Id = rdr.GetInt32(0);
                    placeHolder.Name = rdr.GetString(1);
                    placeHolders.Add(placeHolder);
                }
            }
            return placeHolders;
        }

        public void ChangePlaceHolder(BOPlaceHolder placeHolder)
        {
            var paramsToPass = new SqlParameter[4];

            paramsToPass[0] = (placeHolder.Id.HasValue ? new SqlParameter("@Id", placeHolder.Id) : new SqlParameter("@Id", DBNull.Value));
            paramsToPass[1] = new SqlParameter("@PId", placeHolder.Name);
            paramsToPass[2] = new SqlParameter("@Class", placeHolder.Name);
            paramsToPass[3] = new SqlParameter("@Type", "0");

            string sql;
            if (placeHolder.Id.HasValue)
            {
                sql = @"UPDATE [dbo].[place_holder] SET place_holder_id=@PId, place_holder_class = @Class WHERE id=@Id";
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.InputOutput;
                paramsToPass[0].SqlDbType = SqlDbType.Int;
                sql = @"INSERT INTO [dbo].[place_holder]  (place_holder_id, place_holder_class, place_holder_type) VALUES (@PId, @Class, @Type); SET @Id=SCOPE_IDENTITY();";
            }

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
            if (!placeHolder.Id.HasValue)
            {
                placeHolder.Id = (int)paramsToPass[0].Value;
            }
        }

	    public List<BOTemplate> ListTemplates()
        {
            List<BOTemplate> templates = new List<BOTemplate>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, @"SELECT id, template_type, name, content FROM [dbo].[template] ORDER BY name ASC"))
            {
                while (rdr.Read())
                {
                    BOTemplate template = new BOTemplate();
                    template.Id = rdr.GetInt32(0);
                    template.Type = rdr.GetString(1);
                    template.Name = rdr.GetString(2);
                    if (!rdr.IsDBNull(3))
                        template.TemplateContent =  rdr.GetString(3);
                    templates.Add(template);
                }
            }
            return templates;
        }

        public List<BOModule> ListModules(bool includeUsageCount)
        {
            var sql = @"SELECT id, name, module_source";
            if (includeUsageCount)
            {
                sql += ",(SELECT COUNT(id) FROM module_instance mi WHERE mi.module_fk_id = m.id AND pages_fk_publish = 0) as unpublished_instances";
                sql += ",(SELECT COUNT(id) FROM module_instance mi WHERE mi.module_fk_id = m.id AND pages_fk_publish = 1) as published_instances";
            }
            sql += @"	
FROM [dbo].[module] m
ORDER BY name ASC";

            List<BOModule> modules = new List<BOModule>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain,
                CommandType.Text, sql))
            {
                while (rdr.Read())
                {
                    BOModule module = new BOModule();
                    module.Id = rdr.GetInt32(0);
                    module.Name = rdr.GetString(1);
                    module.Source = rdr.GetString(2);
                    if (includeUsageCount)
                    { 
                        module.NoUnpublishedInstances = rdr.GetInt32(3);
                        module.NoPublishedInstances = rdr.GetInt32(4);
                    }
                    modules.Add(module);
                }
            }
            return modules;
        }

        public static DataTable ListTopModuleUsage(int id)
        {
            var result = SqlHelper.ExecuteDataset(SqlHelper.ConnStringMain, CommandType.Text, @"SELECT TOP(20) pages_fk_id AS id
                FROM module_instance 
                WHERE module_fk_id = @id AND pages_fk_publish = 0 AND pending_delete = 0", new SqlParameter("@id", id));
            return result.Tables[0];
        }

        public static BOModuleInstance GetModuleInstance(int moduleInstanceID, bool publishFlag)
        {
            BOModuleInstance instance = null;

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@moduleInstanceID", moduleInstanceID);
            paramsToPass[1] = new SqlParameter("@publishFlag", publishFlag);

            // The following SELECT will return empty if there are no settings on a module (which is completely valid).
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                @"SELECT  m.name ModuleName,
                    msl.name AS SettingName,
                    ISNULL(ms.value, msl.default_value) as value,
                    msl.type,
                    msl.user_visibility,
                    mi.id ModuleInstanceID,
                    mi.persistent_from,
                    mi.persistent_to,
                    mi.persistence_idx,
                    mi.pages_fk_publish PublishFlag,
                    mi.pages_fk_id PageID,
                    mi.module_fk_id ModuleID,
                    mi.idx ModuleOrder,
                    mi.changed Changed,
                    mi.pending_delete pendingDelete,
                    mi.place_holder_fk_id,
                    msl.options
                FROM [dbo].[module_instance] mi
		            INNER  JOIN [dbo].[module] m ON m.id = mi.module_fk_id
		            INNER JOIN [dbo].[settings_list] msl ON m.[name] =	msl.[subsystem]		
		            LEFT JOIN [dbo].[module_settings] ms 
			            ON  ms.module_instance_fk_id =  mi.id 
			            AND  msl.id = ms.settings_list_fk_id
			            AND ms.module_instance_fk_pages_fk_publish = mi.pages_fk_publish 
                    WHERE mi.id = @moduleInstanceID AND mi.pages_fk_publish = @publishFlag", paramsToPass))
            {
                while (rdr.Read())
                {
                    if (instance == null)
                    {
                        instance = new BOModuleInstance { Id = (int)rdr["ModuleInstanceID"], ModuleId = (int)rdr["ModuleID"], Order = (int)rdr["ModuleOrder"], PageId = (int)rdr["PageID"], Name = (string)rdr["ModuleName"] };
                        instance.PendingDelete = bool.Parse(rdr["PendingDelete"].ToString());
                        instance.Changed = bool.Parse(rdr["Changed"].ToString());
                        instance.PlaceHolderId = (int)rdr["place_holder_fk_id"];
                        instance.PersistFrom = (int)rdr["persistent_from"];
                        instance.PersistTo = (int)rdr["persistent_to"];
                    }
                    string settingKey = rdr["SettingName"].ToString();
                    BOSetting setting = new BOSetting(settingKey, rdr["type"].ToString(), rdr["value"].ToString(), rdr["user_visibility"].ToString());

                    // adding possible options in multipleoptins setting
                    if (rdr["options"] != DBNull.Value && rdr["options"].ToString().Length > 0)
                    {
                        List<string> options = StringTool.SplitString(rdr["options"].ToString());
                        setting.Options = new Dictionary<string, string>();
                        foreach (string s in options)
                        {
                            string[] iSplitted = s.Split(new char[] {':'});
                            if (iSplitted.Length == 1)
                            {
                                setting.Options.Add(iSplitted[0], iSplitted[0]);
                            }
                            if (iSplitted.Length == 2)
                            {
                                setting.Options.Add(iSplitted[0], iSplitted[1]);
                            }
                        }
                    }
                    instance.Settings.Add(settingKey, setting);
                }
            }
            // if module has 0 settings, previous select will return empty.
            if (instance == null)
            {
                using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                    @"SELECT	m.name name,
                            mi.id ModuleInstanceID,
                            mi.persistent_from,
                            mi.persistent_to,
                            mi.persistence_idx,
                            mi.pages_fk_publish PublishFlag,
                            mi.pages_fk_id PageID,
                            mi.module_fk_id ModuleID,
                            mi.idx ModuleOrder,
                            mi.changed Changed,
                            mi.pending_delete pendingDelete,
                            mi.place_holder_fk_id
                        FROM 	[dbo].[module_instance] mi
		                    INNER  JOIN [dbo].[module] m ON m.id = mi.module_fk_id
                            WHERE mi.id = @moduleInstanceID AND mi.pages_fk_publish = @publishFlag", paramsToPass))
                {
                    if (rdr.Read())
                    {
                        instance = new BOModuleInstance { Id = (int)rdr["ModuleInstanceID"], ModuleId = (int)rdr["ModuleID"], Order = (int)rdr["ModuleOrder"], PageId = (int)rdr["PageID"], Name = (string)rdr["name"] };
                        instance.PendingDelete = bool.Parse(rdr["PendingDelete"].ToString());
                        instance.Changed = bool.Parse(rdr["Changed"].ToString());
                        instance.PlaceHolderId = (int)rdr["place_holder_fk_id"];
                        instance.PersistFrom = (int)rdr["persistent_from"];
                        instance.PersistTo = (int)rdr["persistent_to"];
                    }
                }
            }
            return instance;
        }

        public static void ChangeModuleInstance(BOModuleInstance moduleInstance)
        {
            SqlParameter moduleInstanceIdParam = new SqlParameter("@Id", moduleInstance.Id);
            moduleInstanceIdParam.Direction = ParameterDirection.InputOutput;
            SqlParameter[] parms = new SqlParameter[] {
				moduleInstanceIdParam,
                new SqlParameter("@PageID", moduleInstance.PageId),
                new SqlParameter("@PublishFlag", moduleInstance.PublishFlag),
                new SqlParameter("@ModuleId", moduleInstance.ModuleId),
				new SqlParameter("@Order", moduleInstance.Order),
                new SqlParameter("@PersistFrom", moduleInstance.PersistFrom),
                new SqlParameter("@PersistTo", moduleInstance.PersistTo),
                new SqlParameter("@PersistOrder", 10),
                new SqlParameter("@PlaceHolderID", moduleInstance.PlaceHolderId),
                new SqlParameter("@Changed", moduleInstance.Changed),
                new SqlParameter("@PendingDelete", moduleInstance.PendingDelete)
                };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[ChangeModuleInstance]", parms);
            moduleInstance.Id = (int)parms[0].Value;
            
            parms = new SqlParameter[] {
                new SqlParameter("@Id", moduleInstance.Id),
				new SqlParameter("@PublishFlag", moduleInstance.PublishFlag),
                new SqlParameter("@Name", SqlDbType.VarChar, 255),
                new SqlParameter("@Value", SqlDbType.VarChar, 4000),
                new SqlParameter("@SubSystem", moduleInstance.Name)
            };
            foreach (BOSetting setting in moduleInstance.Settings.Values)
            {
                parms[2].Value = setting.Name;
                parms[3].Value = setting.Value;
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.StoredProcedure, "[dbo].[ChangeModuleInstanceSetting]", parms);
            }
        }

        public void DeleteModuleInstance(int moduleInstanceId)
        {
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, 
                "DELETE FROM module_instance WHERE id = @Id", new SqlParameter("@Id",moduleInstanceId));
        }

        ////////////////////////////////////////////////////////////////////////////////////

		public bool ValidateParLink(int? parentPageId, int pageId, string parLink, int websiteId)
		{
            int count = 0;

            SqlParameter[] paramsToPass = new SqlParameter[4];
            paramsToPass[0] = new SqlParameter("@pageId", pageId);
            paramsToPass[1] = new SqlParameter("@parLink", parLink);
            paramsToPass[2] = (parentPageId.HasValue ? new SqlParameter("@parentPageId", parentPageId) : new SqlParameter("@parentPageId", DBNull.Value));
            paramsToPass[3] = new SqlParameter("@websiteId", websiteId);

            string sql = @"SELECT count(par_link) FROM int_link il
                           INNER JOIN pages p ON p.id = il.pages_fk_id AND p.publish=il.pages_fk_publish
                           WHERE p.web_site_fk_id = @websiteId
                             AND p.pages_fk_id = @parentPageId
                             AND p.id != @pageId
                             AND par_link = @parLink";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }

			return (count == 0);
		}
    }
}