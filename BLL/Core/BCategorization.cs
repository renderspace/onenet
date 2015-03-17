using System;
using System.Collections.Generic;
using System.Threading;
using One.Net.BLL.DAL;


namespace One.Net.BLL
{
    internal class BCategorization : BusinessBaseClass
    {
        private static readonly DbCategorization categorizationDB = new DbCategorization();
        private static readonly BInternalContent contentB = new BInternalContent();

        /// <summary>
        /// Categorizes categorizedItemId against given category
        /// Existing categorization is first removed, before new categorization is entered.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categorizedItemId"></param>
        public void Categorize(BOCategory category, int categorizedItemId)
        {
            if (category != null && category.Id.HasValue)
            {
                RemoveCategorizationFromItem(category, categorizedItemId);
                categorizationDB.Categorize(category, categorizedItemId);
            }
            else
            {
                throw new ArgumentException("category is null");
            }
        }
        
        /// <summary>
        /// Removes all categorization of this type, then categorizes against list of categories
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="categorizedItemId"></param>
        public void Categorize(List<BOCategory> categories, int categorizedItemId)
        {
            if (categories != null)
            {
                if (categories.Count > 0)
                {
                    if (categories[0].IsManyToMany)
                    {
                        RemoveCategorizationFromItem(categories[0].Type, categorizedItemId);
                        foreach (BOCategory category in categories)
                            categorizationDB.Categorize(category, categorizedItemId);
                    }
                    else
                        throw new Exception("Attempting n:m categorization on 1:n category");
                }
            }
            else
                throw new ArgumentException("categories is null");
        }

        /// <summary>
        /// Remove all categorization for given categorizedItemId and category type
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="categorizedItemId"></param>
        public void RemoveCategorizationFromItem(string categoryType, int categorizedItemId)
        {
            List<BOCategory> currentCategories = this.ListAssignedToItem(categoryType, categorizedItemId, false);
            foreach (BOCategory category in currentCategories)
            {
                ClearCache(category.Id.Value);
            }
            categorizationDB.RemoveCategorizationFromItem(categoryType, categorizedItemId);
        }

        /// <summary>
        /// Remove specific categorization for given categorizedItemId and category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categorizedItemId"></param>
        public void RemoveCategorizationFromItem(BOCategory category, int categorizedItemId)
        {
            if (category != null && category.Id.HasValue)
            {
                categorizationDB.RemoveCategorizationFromItem(category, categorizedItemId);
                ClearCache(category.Id.Value);
            }
            else
            {
                throw new ArgumentException("category is null");
            }
        }

        /// <summary>
        /// Returns a fully populated category object 
        /// including textual data using CurrentThread language.
        /// This method is cached based on publishFlag.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="showUntranslated"></param>
        /// <returns>null if category doesn't exist.</returns>
        public BOCategory Get(int id, string type, bool showUntranslated)
        {
            BOCategory category = this.Get(id, showUntranslated);
            if (category != null && category.Type != type)
            {
                throw new Exception("Category is not of " + type + " type");
            }
            return category;
        }

        private BOCategory Get(int id, bool showUntranslated)
        {
            BOCategory category = null;
            bool useCache = !showUntranslated;
            if (useCache)
                category = OCache.Get(CACHE_LANG_PREFIX + CAT_CACHE_ID(id)) as BOCategory;

            if (category == null)
            {
                category = GetUnCached(id, showUntranslated);
                if (useCache)
                    OCache.Max(CACHE_LANG_PREFIX + CAT_CACHE_ID(id), category);
            }
            return category;
        }

        private BOCategory GetUnCached(int id, bool showUntranslated)
        {
            BOCategory category = categorizationDB.Get(id, showUntranslated, LanguageId);

            if (showUntranslated && category != null && category.ContentId.HasValue && category.MissingTranslation)
                category.Title = BInternalContent.GetContentTitleInAnyLanguage(category.ContentId.Value);

            return category;
        }

        /// <summary>
        /// Return all available categories for given category type.
        /// Categories are fully populated. Their content is populated using current thread language id.
        /// The list is fully cached.
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        public List<BOCategory> List(string categoryType, bool showUntranslated)
        {
            List<BOCategory> list = null;
            string LIST_CACHE_ID = CACHE_LANG_PREFIX + "_LC_" + categoryType;
            bool useCache = !showUntranslated && PublishFlag;
            
            if (useCache)
                list = OCache.Get(LIST_CACHE_ID) as List<BOCategory>;

            if (list == null)
            {
                list = categorizationDB.ListAll(categoryType, showUntranslated, LanguageId);
                if (list != null)
                {
                    if (showUntranslated)
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] != null && list[i].MissingTranslation && list[i].ContentId.HasValue)
                            {
                                list[i].Title = BInternalContent.GetContentTitleInAnyLanguage(list[i].ContentId.Value);
                            }
                        }

                    if (useCache)
                    {
                        OCache.Max(LIST_CACHE_ID, list);
                    }
                }
            }
            return list;
        }

        public List<BOCategory> List(string categoryType, bool showUntranslated, List<int> categoryFilter)
        {
            List<BOCategory> categories = List(categoryType, showUntranslated);
            if (categoryFilter != null)
            {
                List<BOCategory> outputCats = new List<BOCategory>(categories);
                FilterCategories(outputCats, categoryFilter);
                return outputCats;
            }
            return categories;
        }

        /// <summary>
        /// Private utility method which filters categoryList according to categoryFilter list of integer ids.
        /// </summary>
        /// <param name="categoryList"></param>
        /// <param name="categoryFilter"></param>
        private static void FilterCategories(List<BOCategory> categoryList, ICollection<int> categoryFilter)
        {
            if (categoryFilter.Count > 0)
                categoryList.RemoveAll(delegate(BOCategory cat) { return !categoryFilter.Contains(cat.Id.Value); });
        }

        /// <summary>
        /// Return categories for given categorizedItemId based on given categoryType
        /// Categories are fully populated. Their content is populated using current thread language id.
        /// The list is fully cached
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="categorizedItemId"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        public List<BOCategory> ListAssignedToItem(string categoryType, int categorizedItemId, bool showUntranslated)
        {
            List<BOCategory> categories = categorizationDB.ListAssignedToItem(categoryType, categorizedItemId, showUntranslated,
                                          LanguageId);

            if ( showUntranslated )
                foreach (BOCategory cat in categories)
                {
                    if ( cat.ContentId.HasValue && cat.MissingTranslation )
                        cat.Title = BInternalContent.GetContentTitleInAnyLanguage(cat.ContentId.Value);
                }
            return categories;
        }

        /// <summary>
        /// Return categories for given categorizedItemId based on given categoryType
        /// Categories are fully populated. Their content is populated using provided language id.
        /// The list is fully cached
        /// </summary>
        /// <param name="categoryType"></param>
        /// <param name="categorizedItemId"></param>
        /// <param name="showUntranslated"></param>
        /// <param name="languageId"></param>/// 
        /// <returns></returns>
        public List<BOCategory> ListAssignedToItem(string categoryType, int categorizedItemId, bool showUntranslated, int languageId)
        {
            List<BOCategory> categories = categorizationDB.ListAssignedToItem(categoryType, categorizedItemId, showUntranslated,
                                          languageId);

            if (showUntranslated)
                foreach (BOCategory cat in categories)
                {
                    if (cat.ContentId.HasValue && cat.MissingTranslation)
                        cat.Title = BInternalContent.GetContentTitleInAnyLanguage(cat.ContentId.Value);
                }
            return categories;
        }

        /// <summary>
        /// Return immediate children of folderId
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="showUntranslated"></param>
        /// <returns></returns>
        public List<BOCategory> ListChildren(int folderId, bool showUntranslated)
        {
            List<BOCategory> categories = categorizationDB.ListChildren(folderId, showUntranslated, LanguageId);

            if (showUntranslated)
                foreach (BOCategory cat in categories)
                {
                    if (cat.ContentId.HasValue && cat.MissingTranslation)
                        cat.Title = BInternalContent.GetContentTitleInAnyLanguage(cat.ContentId.Value);
                }

            return categories;
        }

        /// <summary>
        /// Change given BOCategory.
        /// Super class BOInternalContent is also changed to reflect new data.
        /// Removes entry from cache.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        public void ChangeCategory(BOCategory category, string type)
        {
            if (category != null )
            {
                if (category.Type != type || string.IsNullOrEmpty(type))
                {
                    throw new ArgumentException("Category is not of type : " + type);
                }

                if (category.Id.HasValue)
                {
                    BOCategory prevCategory = categorizationDB.Get(category.Id.Value, true, LanguageId);
                    if (prevCategory != null && prevCategory.Id.HasValue && prevCategory.Type != type)
                    {
                        throw new InvalidOperationException("Trying to change a category that already belongs to a diffrent type");
                    }
                    if (prevCategory != null && prevCategory.Id.HasValue && prevCategory.ContentId.HasValue)
                    {
                        category.ContentId = prevCategory.ContentId.Value;
                    }
                }
                contentB.Change(category);
                categorizationDB.ChangeCategory(category);
                ClearCache(category.Id.Value);
            }
            else
            {
                throw new ArgumentException("category is null");
            }
        }

        /// <summary>
        /// Delete a category by Id. 
        /// Note: content for this category is not deleted. Also, deletion fails if this category
        /// has categorized items or subcategories.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public void DeleteCategory(int id, string type)
        {
            BOCategory category = categorizationDB.Get(id, true, LanguageId);

            if (category != null && category.Id.HasValue)
            {
                if (category.Type != type || string.IsNullOrEmpty(type))
                {
                    throw new ArgumentException("Category is not of type : " + type);
                }

                if (this.ListCategorizedItems(id).Count == 0 && category.ChildCount == 0)
                {
                    categorizationDB.DeleteCategory(id);
                    contentB.Delete(category.ContentId.Value);
                    ClearCache(id);
                }
            }
            else
            {
                throw new ArgumentException("category with id=" + id + " does not exist.");
            }
        }

        /// <summary>
        /// Moves category from its existing parent category to newParent...checking that types match
        /// </summary>
        /// <param name="category"></param>
        /// <param name="newParent"></param>
        /// <param name="type"></param>
        public void MoveCategory(BOCategory category, BOCategory newParent, string type)
        {
            if (category == null || !category.Id.HasValue)
            {
                throw new ArgumentException("category is null");
            }
            else if (newParent == null || !newParent.Id.HasValue)
            {
                throw new ArgumentException("newParent is null");
            }
            else if (category.Type != type || string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("category type does not match given type (" + type + ")");
            }
            else if (newParent.Type != type || string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("newParent type does not match given type (" + type + ")");
            }
            else if (newParent.Id.Value == category.Id.Value)
            {
                throw new ArgumentException("newParent id and category id match");
            }

            category.ParentId = newParent.Id;
            this.ChangeCategory(category, type);
            ClearCache(newParent.Id.Value);
        }

        /// <summary>
        /// Method provides a list of all item ids categorized under this category.
        /// Table names can be obtained by looking at category.Type
        /// </summary>
        /// <param name="category"></param>
        /// <returns>List of integer ids of categorized items</returns>
        public List<int> ListCategorizedItems(BOCategory category)
        {
            if (category != null && category.Id.HasValue)
            {
                return categorizationDB.ListCategorizedItems(category);
            }
            else
            {
                return new List<int>();
            }
        }

        public List<int> ListCategorizedItems(int categoryId)
        {
            return ListCategorizedItems(new BOCategory(categoryId, null, 0, "", false, false));
        }

        /// <summary>
        /// Item's categorization is reassigned from one category to another.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void MoveItem(int itemId, BOCategory from, BOCategory to)
        {
            if (from != null && to != null && from.Id.HasValue && to.Id.HasValue)
            {
                this.RemoveCategorizationFromItem(from, itemId);
                this.Categorize(to, itemId);
                ClearCache(from.Id.Value);
                ClearCache(to.Id.Value);
            }
            else
            {
                throw new ArgumentException("from or to are null");
            }
        }

        #region Cache Ids

        private void ClearCache(int id)
        {
            ClearLanguageVariations(CAT_CACHE_ID(id));
        }

        private static string CAT_CACHE_ID(int catId)
        {
            return "Category_" + catId;
        }

        #endregion Cache Ids
    }
}

