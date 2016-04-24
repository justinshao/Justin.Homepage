using System;
using System.ComponentModel.DataAnnotations;

namespace Justin.Homepage.Models
{
    public class Article
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string KeyWords { get; set; }
        public DateTime Time { get; set; }
        [Required]
        public string Html { get; set; }

        public static readonly Article Empty =
            new Article { Html = "hello, world" };

        public override bool Equals(object obj)
        {
            var other = obj as Article;

            if (other == null)
                return false;

            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
