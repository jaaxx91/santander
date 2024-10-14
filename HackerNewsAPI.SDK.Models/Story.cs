namespace HackerNewsAPI.SDK.Models
{
    public class Story
    {
        public string By { get; set; }

        public int Descendants { get; set; }

        public int Id { get; set; }

        public int[] Kids { get; set; }

        public int Score { get; set; }

        public double Time { get; set; }

        public DateTime CreatedOn => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(Time)
            .ToLocalTime();

        public string Title { get; set; }

        public string Url { get; set; }


    }
}
