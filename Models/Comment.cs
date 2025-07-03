namespace Models
{
    public class Comment
    {
        public string Author { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
