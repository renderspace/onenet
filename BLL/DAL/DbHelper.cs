using System;
using System.Data;

namespace One.Net.BLL.DAL
{
    public class DbHelper
    {
        internal const string CONTENT_SELECT_PART = @"SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, 
                    c.date_created, c.principal_modified_by, c.date_modified, c.votes, c.score, ";

        /// <summary>
        /// reader 0-7 should contain BOContent elements.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="content"></param>
        /// <param name="languageId"></param>
        internal static void PopulateContent(IDataReader reader, BOInternalContent content, int languageId)
        {
            if (reader[3] != DBNull.Value)
            {
                content.LanguageId = languageId;
                content.Title = reader.GetString(0);
                content.SubTitle = reader.GetString(1);
                content.Teaser =  reader.GetString(2);
                content.Html=  reader.GetString(3);
                content.PrincipalCreated =  reader.GetString(4);
                content.DateCreated = reader.GetDateTime(5);

                if (reader[6] != DBNull.Value && reader[7] != DBNull.Value)
                {
                    content.PrincipalModified = reader.GetString(6);
                    content.DateModified = reader.GetDateTime(7);
                }
            }
            else
            {
                content.MissingTranslation = true;
            }
        }
    }
}
