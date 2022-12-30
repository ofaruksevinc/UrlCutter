using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UrlCutter.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string pass { get; set; }
        public bool Role { get; set; }
        public int? Kısaltma { get; set; }
    }
}
