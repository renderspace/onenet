using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ComponentModel;
using System.ServiceModel.Web;
using System.Collections;

namespace One.Net.Forms
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IFormService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();


        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Get?id={id}")]
        [Description(" public Get(int id)")]
        DTOForm Get(int id);
    }
}
