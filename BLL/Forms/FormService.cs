using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace One.Net.Forms
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class FormService : IFormService
    {
        protected static BForm formB = new BForm();

        public DTOForm Get(int id)
        {
            var form = formB.Get(id);

            if (form != null)
            {
                var result = new DTOForm(form);
                return result;
            }
            return null;
        }

        public string Ping()
        {
            return "PONG";
        }
    }
}
