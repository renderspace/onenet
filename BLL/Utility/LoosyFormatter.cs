using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Text;

namespace One.Net.BLL.Utility
{
    class LoosyFormatter : IFormatter
    {
        SerializationBinder binder;
        StreamingContext context;
        ISurrogateSelector surrogateSelector;

        public LoosyFormatter()
        {
            context = new StreamingContext(StreamingContextStates.All);
        }

        public object Deserialize(Stream serializationStream)
        {
            StreamReader sr = new StreamReader(serializationStream);
            string str = sr.ReadToEnd();
            string stLen;
            int pos, start, end, length;
            pos = 0;
            string className = null;
            // Store serialized variable name -> value pairs.
            Dictionary<string, object> fieldDict = new Dictionary<string, object>();

            while (pos < str.Length)
            {
                string fieldName;
                switch (str[pos])
                {
                    case 'O':
                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(":", start);
                        stLen = str.Substring(start, end - start);
                        length = int.Parse(stLen);
                        pos += 4 + stLen.Length + length;
                        if (string.IsNullOrEmpty(className))
                            className = str.Substring(end + 1, length);
                        else 
                            throw new ServerException("Typename defined twice");
                        break;
                    case 'N':
                        end = str.IndexOf(";", pos);
                        pos = end + 1;
                        break;
                    case 'b':

                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(":", start);
                        fieldName = str.Substring(start, end - start);
                        pos = end + 1;

                        char chBool;
                        chBool = str[pos];
                        pos += 2;
                        fieldDict[fieldName] = (chBool == '1');
                        break;
                    case 'i':
                        string stInt;
                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(":", start);
                        fieldName = str.Substring(start, end - start);

                        start = end + 1;

                        end = str.IndexOf(";", start);
                        stInt = str.Substring(start, end - start);
                        pos = end + 1;
                        fieldDict[fieldName] = Int32.Parse(stInt);
                        break;
                    case 's':
                        // fieldName
                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(":", start);
                        fieldName = str.Substring(start, end - start);
                        pos = end;
                        // length
                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(":", start);
                        stLen = str.Substring(start, end - start);
                        length = int.Parse(stLen);
                        // value
                        string value = str.Substring(end + 1, length);
                        fieldDict[fieldName] = value;
                        pos += 4 + stLen.Length + length;
                        break;
                    /*
                    case 'd':
                        string stDouble;
                        start = str.IndexOf(":", pos) + 1;
                        end = str.IndexOf(";", start);
                        st Double = str.Substring(start, end - start);
                        pos += 3 + stDouble.Length;
                        return Double.Parse(stDouble);
                    
                    default:
                        return "";*/
                    default:
                        throw new Exception("Infinite loop in Loosy formater");
                }
            }

            if (string.IsNullOrEmpty(className))
                throw new ServerException("Typename not defined!");

            Type t = Type.GetType(className);
            

            // Create object of just found type name.
            Object obj = FormatterServices.GetUninitializedObject(t);

            // Get type members.
            MemberInfo[] members = FormatterServices.GetSerializableMembers(obj.GetType(), Context);

            // Create data array for each member.
            object[] data = new object[members.Length];

            // Store for each member its value, converted from string to its type.
            for (int i = 0; i < members.Length; ++i)
            {
                FieldInfo fi = ((FieldInfo) members[i]);
                if (!fieldDict.ContainsKey(fi.Name)) 
                //throw new SerializationException("Missing field value : " + fi.Name);
                {
                }
                else
                    data[i] = fieldDict[fi.Name];
                    //data[i] = System.Convert.ChangeType(fieldDict[fi.Name], fi.FieldType);
            }

            // Populate object members with theri values and return object.
            return FormatterServices.PopulateObjectMembers(obj, members, data);
        }

        public void Serialize(System.IO.Stream serializationStream, object graph)
        {
            // Get fields that are to be serialized.
            MemberInfo[] members = FormatterServices.GetSerializableMembers(graph.GetType(), Context);
            // fields data.
            object[] objs = FormatterServices.GetObjectData(graph, members);
            StreamWriter sw = new StreamWriter(serializationStream);

            string fullName = graph.GetType().FullName;
            sw.Write("O:{0}:{1};", fullName.Length, fullName);
            for (int i = 0; i < objs.Length; ++i)
            {
                sw.Write(serialize(members[i].Name, objs[i]).ToString());
            }
            sw.Close();
        }

        private static StringBuilder serialize(string name, object obj)
        {
            if (name.Contains(":"))
                throw new ArgumentException("Field name may not contain char {':'} ");

            StringBuilder sb = new StringBuilder();
            if (obj == null)
            {
                return sb.Append("N:" + name + ";");
            }
            else if (obj is string)
            {
                string str = (string)obj;
                byte[] encodedString = Encoding.UTF8.GetBytes(str);

                return sb.Append("s:" + name + ":" + encodedString.Length + ":\"" + encodedString + "\";");
            }
            else if (obj is bool)
            {
                return sb.Append("b:" + name + ":" + (((bool)obj) ? "1" : "0") + ";");
            }
            else if (obj is int)
            {
                int i = (int)obj;
                return sb.Append("i:" + name + ":" + i + ";");
            }
            else if (obj is double)
            {
                double d = (double)obj;
                return sb.Append("d:" + name + ":" + d + ";");
            }
            else
            {
                return sb;
            }
        }

        public ISurrogateSelector SurrogateSelector
        {
            get { return surrogateSelector; }
            set { surrogateSelector = value; }
        }
        public SerializationBinder Binder
        {
            get { return binder; }
            set { binder = value; }
        }
        public StreamingContext Context
        {
            get { return context; }
            set { context = value; }
        }
    }
}
