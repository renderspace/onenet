namespace One.Net.BLL
{
    internal class BLICategorization : BCategorization
    {
        protected override int LanguageId
        {
            get
            {
                return 1279;
            }
        }

        /// <summary>
        /// Change given BOCategory.
        /// Super class BOInternalContent is also changed to reflect new data.
        /// Removes entry from cache.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        public new void ChangeCategory(BOCategory category, string type)
        {
            category.LanguageId = LanguageId;
            base.ChangeCategory(category, type);
        }
    }
}
