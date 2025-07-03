namespace Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Author { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Content { get; set; } = "";
        public int Replies { get; set; }
        public int Likes { get; set; }
    }
}
