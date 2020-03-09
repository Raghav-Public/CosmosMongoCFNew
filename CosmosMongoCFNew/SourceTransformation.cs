using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CosmosMongoCFNew
{
    public class SourceTransformation
    {
        //{'key':'postDate', 'type':'date', 'fields':['YEAR', 'MONTH'] }
        public string Key { get; set; }
        public string Format { get; set; }
        public string TransformationType { get; set; }

        public SourceTransformation() { }

        public string Execute(MongoDB.Bson.BsonDocument _doc)
        {
            if (TransformationType.ToUpper() == "DATE")
            {
                return GetDateTransformedString(_doc);
            }
            return "";
        }

        private string GetDateTransformedString(MongoDB.Bson.BsonDocument _doc)
        {
            DateTime bdate = _doc.GetValue(Key).AsDateTime;
            return bdate.ToString(Format);
        }
    }
}
