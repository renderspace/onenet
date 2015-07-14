using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class PagedList<T> : List<T>
    {
        private int currentPage;
        private int allRecords;
        private int recordsPerPage;

        public int CurrentPage
        {
            get { return this.currentPage; }
            set { this.currentPage = value; }
        }

        public int AllRecords
        {
            get { return this.allRecords; }
            set { this.allRecords = value; }
        }

        public int RecordsPerPage
        {
            get { return this.recordsPerPage; }
            set { this.recordsPerPage = value; }
        }
        
        public new PagedList<T> GetRange(int index, int count)
        {
			if (index >= 0 && count >= 0)
			{
				PagedList<T> tempList = new PagedList<T>();
				tempList.CurrentPage = this.CurrentPage;
				tempList.AllRecords = this.AllRecords;
				tempList.RecordsPerPage = this.RecordsPerPage;
				tempList.AddRange(base.GetRange(index, count));
				return tempList;
			}
			else
			{
				PagedList<T> tempList = new PagedList<T>();
				tempList.CurrentPage = this.CurrentPage;
				tempList.AllRecords = this.AllRecords;
				tempList.RecordsPerPage = this.RecordsPerPage;
				return tempList;
			}
        }
    }
}