using System;

namespace UrlCutter.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }
        public string OrgUrl { get; set; }
        public string CutUrl { get; set; }
        public string RandomChar { get; set; }
        public string? UniqueChar  { get; set; }
        public string user {get; set; } 
        public DateTime CreatedTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
