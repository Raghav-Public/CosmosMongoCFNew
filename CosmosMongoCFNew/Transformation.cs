using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosMongoCFNew
{
    public class Transformation
    {
        public string Type { get; set; }
        public string SourceKeys { get; set; }
        public string DestinationKey { get; set; }
        public string Delimiter { get; set; }

        public Transformation(string type,
                              string sourceKeys,
                              string destinationKey,
                              string delimiter)
        {
            this.Type = type.ToUpper();
            this.SourceKeys = sourceKeys;
            this.DestinationKey = destinationKey;
            this.Delimiter = delimiter;
        }

        public MongoDB.Bson.BsonDocument Execute(MongoDB.Bson.BsonDocument _doc)
        {
            switch (Type)
            {
                case "NONE":
                    return _doc;
                    break;
                case "AGGRIGATION":
                    return Aggrigate(_doc);
                    break;
                    //case "DELETION":
                    //    break;
            }
            return _doc;
        }
        private MongoDB.Bson.BsonDocument Aggrigate(MongoDB.Bson.BsonDocument _doc)
        {
            try
            {
                string destinationValue = "";
                string[] sourceKeyArr = SourceKeys.Split(',');
                foreach (string sourceKey in sourceKeyArr)
                {
                    if (_doc[sourceKey] != null)
                    {
                        destinationValue += _doc[sourceKey] + Delimiter;
                    }
                }
                if (destinationValue.EndsWith(Delimiter))
                {
                    destinationValue = destinationValue.TrimEnd(Delimiter.ToCharArray());
                }
                _doc.Add(DestinationKey, destinationValue);
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.Message);
                throw;
            }
            return _doc;
        }
    }
}
