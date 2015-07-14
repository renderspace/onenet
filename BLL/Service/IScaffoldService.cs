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
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IScaffoldService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Ping")]
        [Description(" public Ping()")]
        string Ping();

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ListItemsForRelation?relationId={relationId}&primaryKey={primaryKey}")]
        [Description("ListItemsForRelation(int relationId, int primaryKey)")]
        List<SerializableJsonDictionary<string, string>> ListItemsForRelation(int relationId, int primaryKey);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, 
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "GetItem?virtualTableId={virtualTableId}&primaryKey={primaryKey}")]
        [Description(" public DTOItem GetItem(int virtualTableId, int primaryKey)")]
        DTOItem GetItem(int virtualTableId, int primaryKey);

        [WebInvoke(Method = "POST", 
            RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "ChangeItem?virtualTableId={virtualTableId}&primaryKey={primaryKey}")]
        [Description("bool ChangeItem(DTOItem item, int virtualTableId, int primaryKey)")]
        bool ChangeItem(DTOItem item, int virtualTableId, int primaryKey); //

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "DeleteItem?virtualTableId={virtualTableId}&primaryKey={primaryKey}")]
        [Description(" public DTOItem GetItem(int virtualTableId, int primaryKey)")]
        bool DeleteItem(int virtualTableId, int primaryKey);

        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetForeignKeyOptions?virtualTableId={virtualTableId}&columnId={columnId}&limit={limit}")]
        [Description("public List<KeyValuePair<string, string>> GetForeignKeyOptions(int virtualTableId, int columnId, int limit)")]
        List<KeyValuePair<string, string>> GetForeignKeyOptions(int virtualTableId, int columnId, int limit);


        /*
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "Demo")]
        [Description("bool Demo(string item, int virtualTableId, int primaryKey)")]
        bool Demo(string item, int virtualTableId, int primaryKey); */
    }
}
