using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace One.Net.BLL
{
    public class BAudit
    {
        private static readonly BInternalContent contentB = new BInternalContent();

        public List<BOInternalContentAudit> ListAudits(int contentId, int languageId)
        {
            return contentB.ListAudits(contentId, languageId);
        }

        public BOInternalContentAudit GetAudit(string guid)
        {
            return contentB.GetAudit(guid);
        }
    }
}
