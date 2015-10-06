using System;
using System.Collections.Generic;

namespace One.Net.BLL
{
    [Serializable]
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }

        public int AllRecords { get; set; }

        public int RecordsPerPage { get; set; }

        public PagedList()
        {
        }

        public PagedList(IEnumerable<T> source) : base(source)
        {
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