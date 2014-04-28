using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using MsSqlDBUtility;
using System.Security.Cryptography;

namespace One.Net.BLL
{
    public class BRedirects
    {
        private const string ItemCacheKey = "Core:Item:BORedirect:";
        private const string ListCacheKey = "Core:BRedirect:List<BORedirect>";

        private const string ShorturlCharsLcase = "abcdefgijkmnopqrstwxyz";
        private const string ShorturlCharsUcase = "ABCDEFGHJKLMNPQRSTWXYZ";
        private const string ShorturlCharsNumeric = "23456789";

        public static string ShortenUrl(string uri)
        {
            var shortUri = "";
            var link = GetShortenedLink(uri);
            if (link != null)
            {
                shortUri = link.FromLink;
            }
            else
            {
                do
                {
                    link = new BORedirect
                    {
                        ToLink = uri,
                        FromLink= "/" + Shorten()
                    };
                } while (!Change(link));
                shortUri = link.FromLink;
            }
            return shortUri;
        }

        public static BORedirect Get(int redirectId)
        {
            var redirect = OCache.Get(ItemCacheKey + redirectId) as BORedirect;

            if (redirect == null)
            {
                var cmdParams = new[] { new SqlParameter("@redirectId", redirectId) };
                var sql = @"SELECT id, from_link, to_link, is_shortener, created FROM dbo.redirects WHERE id = @redirectId";
                using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql, cmdParams))
                {
                    if (reader.Read())
                    {
                        redirect = PopulateRedirect(reader);
                        OCache.Max(ItemCacheKey + redirectId, redirect);
                    }
                }
            }
            return redirect;
        }

        public static BORedirect Find(string fromLink)
        {
            var redirect = OCache.Get(ItemCacheKey + fromLink) as BORedirect;

            if (redirect == null)
            {
                List<BORedirect> list = List();

                if (list != null)
                {
                    redirect = list.Find(
                        delegate(BORedirect r)
                        {
                            return r.FromLink == fromLink;
                        }
                    );
                    if (redirect != null)
                        OCache.Max(ItemCacheKey + fromLink, redirect);
                }
            }

            return redirect;
        }

        public static BORedirect Get(string fromLink)
        {
            var redirect = OCache.Get(ItemCacheKey + fromLink) as BORedirect;

            if (redirect == null)
            {
                var cmdParams = new[] { new SqlParameter("@fromLink", fromLink) };
                var sql = @"SELECT id, from_link, to_link, is_shortener, created FROM dbo.redirects WHERE from_link = @fromLink";
                using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql, cmdParams))
                {
                    if (reader.Read())
                    {
                        redirect = PopulateRedirect(reader);
                        OCache.Max(ItemCacheKey + fromLink, redirect);
                    }
                }

            }
            return redirect;
        }

        private static BORedirect PopulateRedirect(IDataRecord reader)
        {
            var redirect = new BORedirect();
            redirect.Id = (int)reader["id"];
            redirect.FromLink = (string)reader["from_link"];
            redirect.ToLink = (string)reader["to_link"];
            redirect.IsShortener = (bool)reader["is_shortener"];
            redirect.Created = (DateTime)reader["created"];
            return redirect;
        }

        public static List<BORedirect> List()
        {
            var list = OCache.Get(ListCacheKey) as List<BORedirect>;

            if (list == null)
            {
                list = new List<BORedirect>();

                var sql = @"SELECT * FROM dbo.redirects rt";

                using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql))
                {
                    while (reader.Read())
                        list.Add(PopulateRedirect(reader));
                }

                OCache.Max(ListCacheKey, list);
            }

            return list;
        }

        public static PagedList<BORedirect> List(ListingState state)
        {
            var list = new PagedList<BORedirect>();
            var cmdParams = new[]
				{
					new SqlParameter("@FirstRecord", state.DbFromRecordIndex),
					new SqlParameter("@LastRecord", state.DbToRecordIndex)
				};
            var sql = @"SELECT DISTINCT rt.id, ROW_NUMBER() OVER (ORDER BY " + state.SortField + " " + state.DbSortDirection + @") AS rownum
						INTO #pagedlist
						FROM dbo.redirects rt
						GROUP BY rt.id, " + state.SortField + @"
						SELECT #pagedlist.id, from_link, to_link,  is_shortener, created
						FROM #pagedlist
                        INNER JOIN redirects r ON #pagedlist.id=r.id
						WHERE rownum BETWEEN @FirstRecord AND @LastRecord
						ORDER BY rownum;
						SELECT COUNT(id) FROM #pagedlist";
            using (var reader = SqlHelper.ExecuteReader(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql, cmdParams))
            {
                while (reader.Read())
                    list.Add(PopulateRedirect(reader));

                if (reader.NextResult() && reader.Read())
                    list.AllRecords = reader.GetInt32(0);
            }

            return list;
        }

        public static bool Change(BORedirect redirect)
        {
            if (redirect == null)
                throw new ArgumentException("Redirect is null");

            var cmdParams = new List<SqlParameter>
			{
				new SqlParameter("@id", redirect.Id.HasValue ? redirect.Id.Value : (object)DBNull.Value),
				new SqlParameter("@fromLink", redirect.FromLink),
				new SqlParameter("@toLink", redirect.ToLink),
                new SqlParameter("@isShortener", redirect.IsShortener)

			};
            var sql = @"IF EXISTS (SELECT * FROM [dbo].[redirects] WHERE id = @id)
						BEGIN
							UPDATE [dbo].[redirects]
							SET [from_link] = @fromLink,
								[to_link] = @toLink,
                                is_shortener = @isShortener
							WHERE id = @id
							SELECT CAST(@id as int)
						END
						ELSE
						BEGIN
							INSERT INTO [dbo].[redirects]([from_link],[to_link], is_shortener)
							VALUES (@fromLink, @toLink, @isShortener)
							SELECT CAST(SCOPE_IDENTITY() as int)
						END";
            redirect.Id = (int)SqlHelper.ExecuteScalar(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql, cmdParams.ToArray());
            
            OCache.Remove(ItemCacheKey + redirect.FromLink);
            OCache.Remove(ItemCacheKey + redirect.Id.Value);
            return redirect.Id > 0;
        }

        public static void Delete(int redirectId)
        {
            var redirect = Get(redirectId);
            if (redirect != null)
            {
                var cmdParams = new[] { new SqlParameter("@id", redirectId) };
                var sql = @"DELETE FROM [dbo].[redirects] WHERE id = @id";
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnStringMain, System.Data.CommandType.Text, sql, cmdParams);
                OCache.Remove(ItemCacheKey + redirect.FromLink);
                OCache.Remove(ItemCacheKey + redirect.Id.Value);
            }
        }


        private static BORedirect GetShortenedLink(string uri)
        {
            return List().Find(l => string.Compare(l.FromLink, uri, true) == 0);
        }

        private static string Shorten()
        {
            // Create a local array containing supported short-url characters grouped by types.
            var charGroups = new[] 
			{
				ShorturlCharsLcase.ToCharArray(),
				ShorturlCharsUcase.ToCharArray(),
				ShorturlCharsNumeric.ToCharArray()
			};

            // Use this array to track the number of unused characters in each character group.
            var charsLeftInGroup = new int[charGroups.Length];

            // Initially, all characters in each group are not used.
            for (var i = 0; i < charsLeftInGroup.Length; i++)
                charsLeftInGroup[i] = charGroups[i].Length;

            // Use this array to track (iterate through) unused character groups.
            int[] leftGroupsOrder = new int[charGroups.Length];

            // Initially, all character groups are not used.
            for (int i = 0; i < leftGroupsOrder.Length; i++)
                leftGroupsOrder[i] = i;

            // Because we cannot use the default randomizer, which is based on the
            // current time (it will produce the same "random" number within a
            // second), we will use a random number generator to seed the
            // randomizer.

            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            int seed = (randomBytes[0] & 0x7f) << 24 | randomBytes[1] << 16 | randomBytes[2] << 8 | randomBytes[3];

            // Now, this is real randomization.
            var random = new Random(seed);

            // This array will hold short-url characters.
            char[] shortUrl = null;

            // Allocate appropriate memory for the short-url.
            shortUrl = new char[random.Next(5, 5)];

            // Index of the next character to be added to short-url.
            int nextCharIdx;

            // Index of the next character group to be processed.
            int nextGroupIdx;

            // Index which will be used to track not processed character groups.
            int nextLeftGroupsOrderIdx;

            // Index of the last non-processed character in a group.
            int lastCharIdx;

            // Index of the last non-processed group.
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            // Generate short-url characters one at a time.
            for (int i = 0; i < shortUrl.Length; i++)
            {
                // If only one character group remained unprocessed, process it;
                // otherwise, pick a random character group from the unprocessed
                // group list. To allow a special character to appear in the
                // first position, increment the second parameter of the Next
                // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                if (lastLeftGroupsOrderIdx == 0)
                    nextLeftGroupsOrderIdx = 0;
                else
                    nextLeftGroupsOrderIdx = random.Next(0,
                                                         lastLeftGroupsOrderIdx);

                // Get the actual index of the character group, from which we will
                // pick the next character.
                nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                // Get the index of the last unprocessed characters in this group.
                lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                if (lastCharIdx == 0)
                    nextCharIdx = 0;
                else
                    nextCharIdx = random.Next(0, lastCharIdx + 1);

                // Add this character to the short-url.
                shortUrl[i] = charGroups[nextGroupIdx][nextCharIdx];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                    charsLeftInGroup[nextGroupIdx] =
                                              charGroups[nextGroupIdx].Length;
                // There are more unprocessed characters left.
                else
                {
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] =
                                    charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIdx]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                // There are more unprocessed groups left.
                else
                {
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(shortUrl);
        }
    }
}
