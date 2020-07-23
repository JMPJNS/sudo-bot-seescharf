namespace SudoBot.Models
{
    public class AnilistItem: ListItem
    {
        public string Url;
        public string ImageUrl;
        public int TotalEpisodes;
        public int WatchedEpisodes;
        
        public AnilistItem(string name, ulong userId, string url): base(name, userId)
        {
            Url = url;
            ImageUrl = "https://cdn.jmp.blue/1R4A78de.png";
            TotalEpisodes = 0;
            WatchedEpisodes = 0;
        }

        public override string GetTypeName()
        {
            return "Anilist";
        }
    }
}