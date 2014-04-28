using System;

namespace One.Net.BLL
{
    [Serializable]
    public class BOCategory : BOInternalContent, ICloneable
    {
        private int? id = null;
        private int? parentId = null;
        private int? childCount = null;
        private string type;
        private bool isSelectable = false;
        private bool isPrivate = false;
//        private BOCategory parent;

        public BOCategory()
            : base()
        { }

        public int? Id { get { return id; } set { id = value; } }
        public int? ParentId { get { return parentId; } set { parentId = value; } }
        public int? ChildCount { get { return childCount; } set { childCount = value; } }

        public bool IsSelectable { get { return isSelectable; } set { isSelectable = value; } }
        public bool IsPrivate { get { return isPrivate; } set { isPrivate = value; } }

        public bool IsTreeNode { get { return type.Contains("tree"); } }
        public bool IsManyToMany { get { return type.Contains("n:m"); } }

        public string Type { get { return type; } set { type = value; } }

        public BOCategory(int? id, int? parentId, int contentId, string type, bool isSelectable, bool isPrivate)
            : base()
        {
            this.id = id;
            this.parentId = parentId;
            this.type = type;
            this.ContentId = contentId;
            this.isSelectable = isSelectable;
            this.isPrivate = isPrivate;
        }

        public override string ToString()
        {
            if (IsComplete && Id.HasValue)
                return "BOCategory id:" + Id.Value + " " + Title;
            else if (Id.HasValue)
                return "BOCategory id:" + Id.Value;
            else
                return "BOCategory";
        }

        /// <summary>
        /// Create a new object in memory and deep copy everything from existing object.
        /// </summary>
        /// <returns></returns>
        public Object Clone()
        {
            BOCategory cat = new BOCategory();
            this.CloneContent(cat);

            cat.Id = this.Id;
            cat.Type = this.Type;
            cat.IsSelectable = this.IsSelectable;
            cat.ParentId = this.ParentId;
            cat.ChildCount = this.ChildCount;
            cat.IsPrivate = this.IsPrivate;

            return cat;
        }
    

    }
}
