using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;
using One.Net.BLL.DAL;
using One.Net.BLL.Forms;
using Newtonsoft.Json;

namespace One.Net.BLL.Service
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AdminService : IAdminService
    {
        public string Ping()
        {
            return "AdminService";
        }

        public IEnumerable<DTOAuditItem> GetContentHistory(int contentId)
        {
            BAudit auditB = new BAudit();
            var audits = auditB.ListAudits(contentId);

            var result = new List<DTOAuditItem>();
            foreach (var a in audits)
            {
                result.Add(new DTOAuditItem { AuditGuid = a.AuditGuid, Title = a.Title, DisplayLastChanged = a.DisplayLastChanged });
            }
            return result;
        }
    }

    [DataContract, Newtonsoft.Json.JsonObject(MemberSerialization = Newtonsoft.Json.MemberSerialization.OptIn)]
    public class DTOAuditItem
    {
        [DataMember, JsonProperty]
        public string AuditGuid { get; set; }

        [DataMember, JsonProperty]
        public string Title { get; set; }

        [DataMember, JsonProperty]
        public string DisplayLastChanged { get; set; }

        
    }
}
