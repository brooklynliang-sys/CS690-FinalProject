using System.Text.Json.Serialization;

namespace WatchList;

public enum WatchItemType
{
    Movie,
    TVShow
}

public enum WatchStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class WatchItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public WatchItemType Type { get; set; }
    public WatchStatus Status { get; set; }
    public int? LastWatchedSeason { get; set; }
    public int? LastWatchedEpisode { get; set; }

    [JsonIgnore]
    public string ProgressText
    {
        get
        {
            if (LastWatchedSeason.HasValue && LastWatchedEpisode.HasValue)
            {
                return $"S{LastWatchedSeason.Value} Ep {LastWatchedEpisode.Value}";
            }

            if (LastWatchedEpisode.HasValue)
            {
                return $"Ep {LastWatchedEpisode.Value}";
            }

            return "No progress";
        }
    }
}
