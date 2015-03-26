using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ComponentModel;
using System.ServiceModel.Web;
using System.Collections;

namespace One.Net.BLL.Service
{
    [ServiceContract]
    public interface IScaffoldService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListItemsForRelation?relationId={relationId}&primaryKey={primaryKey}")]
        [Description("ListItemsForRelation(int relationId, int primaryKey)")]
        List<SerializableJsonDictionary<string, string>> ListItemsForRelation(int relationId, int primaryKey);
    }
}
