//using System.Web.UI.WebControls;

namespace One.Net.BLL
{
    public enum SortDir
    {
        Ascending,
        Descending
    }

	public class ListingState
	{
        int? recordsPerPage = null;
        int? firstRecordIndex = null;

        int toRecordIndex = 0, fromRecordIndex = 0;

	    private int offSet = 0;

        SortDir sortDirection = SortDir.Ascending;

        string sortField = "";

		public ListingState() {}

		public ListingState(int? recordsPerPage, int? firstRecordIndex, SortDir sortDirection, string sortField)
		{
			this.recordsPerPage = recordsPerPage;
			this.sortDirection = sortDirection;
			this.sortField = sortField;
            this.firstRecordIndex = firstRecordIndex;
		}

        public string GetCacheIdentifier()
        {
            return "LS_" + recordsPerPage + "_" + sortDirection + "_" + sortField + "_" + firstRecordIndex + "_" + offSet;
        }

	    public ListingState(SortDir sortDirection, string sortField)
		{
			this.recordsPerPage = this.firstRecordIndex = null;
			this.sortDirection = sortDirection;
			this.sortField = sortField;
		}

		public int? RecordsPerPage
		{
			get { return recordsPerPage; }
			set { recordsPerPage = value; }
		}

        public int? FirstRecordIndex
        {
            get { return firstRecordIndex; }
            set { firstRecordIndex = value; }
        }

        private void CalculateFromTo()
        {
            if (FirstRecordIndex.HasValue && RecordsPerPage.HasValue)
            {
                fromRecordIndex = FirstRecordIndex.Value + 1 + offSet;
                toRecordIndex = (fromRecordIndex + RecordsPerPage.Value) - 1 + offSet;
            }
            if (fromRecordIndex < 0)
                fromRecordIndex = 0;
            if (toRecordIndex < RecordsPerPage.Value - 1 + offSet)
                toRecordIndex = RecordsPerPage.Value - 1 + offSet;
        }

        public int DbFromRecordIndex
        {
            get
            {
                CalculateFromTo();
                return fromRecordIndex;
            }
        }

        public int DbToRecordIndex
        {
            get
            {
                CalculateFromTo();
                return toRecordIndex;
            }
        }

        public string DbSortDirection
        {
            get { return (SortDirection == SortDir.Ascending) ? "ASC" : "DESC"; }
        }

        public SortDir SortDirection
		{
			get { return sortDirection; }
			set { sortDirection = value; }
		}

		public string SortField
		{
			get { return sortField; }
			set { sortField = value; }
		}

        public bool UsesSorting
        {
            get { return !string.IsNullOrEmpty(sortField); }
        }

        public bool UsesPaging
        {
            get { return recordsPerPage.HasValue && firstRecordIndex.HasValue; }
        }

	    public int OffSet
	    {
	        get { return offSet; }
	        set { offSet = value; }
	    }
	}
}