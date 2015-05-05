using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Web;
using System.Security.Principal;

namespace One.Net.BLL.Service
{
    // syncs Thread.CurrentPrincipal in WCF with whatever is set 
    // by the HTTP pipeline on Context.User (optional)
    public class MembershipAuthorizationPolicy : IAuthorizationPolicy
    {
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            HttpContext context = HttpContext.Current;

            if (context != null)
            {
                evaluationContext.Properties["Principal"] = context.User;
                evaluationContext.Properties["Identities"] = new List<IIdentity>() { context.User.Identity };
            }

            return true;
        }

        public System.IdentityModel.Claims.ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return "MembershipAuthorizationPolicy"; }
        }
    }
}
