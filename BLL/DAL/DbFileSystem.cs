using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Web.Caching;
using System.Collections.Generic;
using MsSqlDBUtility;

namespace One.Net.BLL.DAL
{
    public class DbFileSystem 
    {
        public void ChangeMeta(BOFile file)
        {
            SqlParameter[] paramsToPass = new SqlParameter[7];
            paramsToPass[0] = new SqlParameter("@Id", file.Id);
            paramsToPass[1] = new SqlParameter("@MimeType", file.MimeType);
            paramsToPass[2] = new SqlParameter("@Name", file.Name);
            paramsToPass[3] = new SqlParameter("@Size", file.Size);
            paramsToPass[4] = new SqlParameter("@Extension", file.Extension);
            paramsToPass[5] = SqlHelper.GetNullable("ContentId", file.ContentId);
            paramsToPass[6] = new SqlParameter("@CurrentPrincipal", Thread.CurrentPrincipal.Identity.Name);

            if (file.Id.HasValue)
            {
                int affectedRows = SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                    @"UPDATE [dbo].[files]
                      SET   mime_type=@MimeType,
                            name=@Name,
                            size=@Size,
                            extension=@Extension,
                            content_fk_id=@ContentId,
                            principal_modified_by=@CurrentPrincipal,
                            modified=GetDate()
                            WHERE id=@Id", 
                     paramsToPass);
                if (affectedRows != 1)
                {
                    paramsToPass[0].Direction = ParameterDirection.InputOutput;
                    paramsToPass[0].DbType = DbType.Int32;
                    SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                        @"SET identity_insert [dbo].[files] ON; INSERT INTO [dbo].[files]
                      (mime_type, name, size, extension, content_fk_id, principal_created_by, id)
                       VALUES 
                      (@MimeType, @Name, @Size, @Extension, @ContentId, @CurrentPrincipal, @Id); SET @Id=SCOPE_IDENTITY(); SET identity_insert [dbo].[files] OFF",
                           paramsToPass);
                    file.Id = Int32.Parse(paramsToPass[0].Value.ToString());
                }
            }
            else
            {
                paramsToPass[0].Direction = ParameterDirection.Output;
                paramsToPass[0].DbType = DbType.Int32;
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, 
                    @"INSERT INTO [dbo].[files]
                      (mime_type, name, size, extension, content_fk_id, principal_created_by)
                       VALUES 
                      (@MimeType, @Name, @Size, @Extension, @ContentId, @CurrentPrincipal); SET @Id=SCOPE_IDENTITY()", 
                       paramsToPass);
                file.Id = Int32.Parse(paramsToPass[0].Value.ToString());
            }
        }

        public void ChangeData(BOFile file)
        {
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@Id", file.Id);
            paramsToPass[1] = new SqlParameter("@File", file.File);

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                    @"UPDATE [dbo].[files]
                      SET content_data_store=@File
                      WHERE id=@Id",
                     paramsToPass);
        }
        
        public void Delete(int fileId)
        {
            // Note file_belongs_to info is deleted by ON DELETE CASCADE
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@Id", fileId);

            string sql = "DELETE FROM [dbo].[files] WHERE id=@Id";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text,
                sql, paramsToPass);
        }

        public BOFile Get(int fileId,  int languageId)
        {
            BOFile file = null;
            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@Id", fileId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            string sql = DbHelper.CONTENT_SELECT_PART + @"f.id, f.mime_type, f.name, f.content_fk_id, f.extension, f.size, f.created, f.modified
            FROM files f
            LEFT JOIN [dbo].[content] c ON  f.content_fk_id = c.id
            LEFT JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageId
            WHERE f.id = @Id";

            using ( var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                sql, paramsToPass))
            {
                if (reader.Read())
                {
                    file = new BOFile();
                    if (!reader.IsDBNull(13))
                    {
                        file.Content = new BOInternalContent();
                        file.Content.ContentId = reader.GetInt32(13);
                        DbHelper.PopulateContent(reader, file.Content, languageId);
                    }
                    file.Id = reader.GetInt32(10);
                    file.MimeType = reader.GetString(11);
                    file.Name = reader.GetString(12);
                    file.Extension = reader.GetString(14);
                    file.Size = reader.GetInt32(15);
					file.Created = reader.GetDateTime(16);
					file.Modified = reader[17] == DBNull.Value ? null as DateTime? : reader.GetDateTime(17);
                }
            }

            return file;
        }

        public byte[] GetFileBytes(int fileId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@Id", fileId);

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
            @"SELECT content_data_store FROM [dbo].[files] WHERE id = @Id", paramsToPass))
            {
                if (reader.Read())
                {
                    return (byte[])reader[0];
                }
            }
            return null;
        }

        public List<BOFile> ListFolder(int folderId, int languageId, string sortBy, SortDir sortDir)
        {
            List<BOFile> list = new List<BOFile>();

            SqlParameter[] paramsToPass = new SqlParameter[2];
            paramsToPass[0] = new SqlParameter("@folderId", folderId);
            paramsToPass[1] = new SqlParameter("@languageId", languageId);

            var sql = @"f.id, f.mime_type, f.name, f.content_fk_id, f.extension, f.size, f.created, f.modified
                    FROM files f
                    INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = f.id AND table_name = '[dbo].[files]'
                    LEFT JOIN [dbo].[content] c ON  f.content_fk_id = c.id
                    LEFT JOIN [dbo].[content_data_store] cds on c.id = cds.content_fk_id AND language_fk_id = @languageId
                    WHERE cbt.ucategories_fk_id = @folderId ";

            if (!string.IsNullOrEmpty(sortBy))
                sql += " ORDER BY " + sortBy + (sortDir == SortDir.Ascending ? " ASC " : " DESC ");

            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text,
                DbHelper.CONTENT_SELECT_PART + sql, paramsToPass))
            {
                while (reader.Read())
                {
                    BOFile file = new BOFile();
                    if (!reader.IsDBNull(13))
                    {
                        file.Content = new BOInternalContent();
                        file.Content.ContentId = reader.GetInt32(13);
                        DbHelper.PopulateContent(reader, file.Content, languageId);
                    }
                    file.Id = reader.GetInt32(10);
                    file.MimeType = reader.GetString(11);
                    file.Name = reader.GetString(12);
                    file.Extension = reader.GetString(14);
                    file.Size = reader.GetInt32(15);
					file.Created = reader.GetDateTime(16);
					file.Modified = reader[17] == DBNull.Value ? null as DateTime? : reader.GetDateTime(17);
                    list.Add(file);
                }
            }
            return list;
        }

        public void ClearFormFileUse(int fileId)
        {
            SqlParameter[] paramsToPass = new SqlParameter[1];
            paramsToPass[0] = new SqlParameter("@Id", fileId);

            string sql = @"UPDATE [dbo].[nform_submitted_answer] 
                                SET files_fk_id=NULL
                                WHERE files_fk_id=@Id";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass);
        }

//        public List<string> ListFileUses(int fileId)
//        {
//            List<string> uses = new List<string>();

//            SqlParameter[] paramsToPass = new SqlParameter[1];
//            paramsToPass[0] = new SqlParameter("@fileId", fileId);

//            // Check use of files with Forms
//            string sql =
//                @"SELECT nsa.nform_answer_fk_id
//                  FROM nform_submitted_answer nsa
//                  WHERE nsa.files_fk_id=@fileId";

//            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, CommandType.Text, sql, paramsToPass))
//            {
//                if (reader.Read())
//                {
//                    uses.Add(BOForm.FILE_USE_FORMS);
//                }
//            }

//            return uses;
//        }
    }
}
