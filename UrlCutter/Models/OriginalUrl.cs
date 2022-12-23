using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UrlCutter.Models
{
    public class OriginalUrl
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Url { get; set; }
        public int Click { get; set; }
    }
}
