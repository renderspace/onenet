using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace One.Net.BLL
{
    public class BAudit
    {
        private static readonly BInternalContent contentB = new BInternalContent();
        protected static int LanguageId { get { return Thread.CurrentThread.CurrentCulture.LCID; } }

        public List<BOInternalContentAudit> ListAudits(int contentId)
        {
            return contentB.ListAudits(contentId, LanguageId);
        }

        public BOInternalContentAudit GetAudit(string guid)
        {
            return contentB.GetAudit(guid);
        }
    }
}
