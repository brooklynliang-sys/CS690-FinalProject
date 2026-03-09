using WatchList;
using Xunit;

namespace WatchList.Tests;

public class ConsoleUITests
{
    [Fact]
    public void FormatItem_IncludesSeasonEpisodeWhenPresent()
    {
        var item = new WatchItem
        {
            Title = "Dark",
            Type = WatchItemType.TVShow,
            Status = WatchStatus.InProgress,
            LastWatchedSeason = 2,
            LastWatchedEpisode = 4
        };

        string result = ConsoleUI.FormatItem(item, 1);

        Assert.Contains("Dark", result);
        Assert.Contains("TVShow", result);
        Assert.Contains("InProgress", result);
        Assert.Contains("S2 Ep 4", result);
    }

    [Fact]
    public void FormatItem_ShowsNoProgressWhenMissing()
    {
        var item = new WatchItem
        {
            Title = "The Batman",
            Type = WatchItemType.Movie,
            Status = WatchStatus.NotStarted
        };

        string result = ConsoleUI.FormatItem(item, 1);

        Assert.Contains("No progress", result);
    }
}
