using WatchList;
using Xunit;

namespace WatchList.Tests;

public class WatchlistStorageTests
{
    [Fact]
    public void SaveAndLoad_PreservesData()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var items = new List<WatchItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Dark",
                Type = WatchItemType.TVShow,
                Status = WatchStatus.InProgress,
                LastWatchedSeason = 1,
                LastWatchedEpisode = 3
            }
        };

        storage.Save(items);
        var loaded = storage.Load();

        Assert.Single(loaded);
        Assert.Equal("Dark", loaded[0].Title);
        Assert.Equal(1, loaded[0].LastWatchedSeason);
        Assert.Equal(3, loaded[0].LastWatchedEpisode);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        string fileName = $"missing-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);

        var loaded = storage.Load();

        Assert.Empty(loaded);
    }
}
