using WatchList;
using Xunit;

namespace WatchList.Tests;

public class WatchlistManagerTests
{
    [Fact]
    public void AddItem_AddsNewItem_WhenItemDoesNotExist()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        bool added = manager.AddItem("Dark", WatchItemType.TVShow, out string message);

        Assert.True(added);
        Assert.Equal("Item added successfully.", message);
        Assert.Single(manager.GetAllItems());
    }

    [Fact]
    public void AddItem_RejectsDuplicateTitleAndType()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        bool added = manager.AddItem("Dark", WatchItemType.TVShow, out string message);

        Assert.False(added);
        Assert.Equal("An item with the same title and type already exists.", message);
        Assert.Single(manager.GetAllItems());
    }

    [Fact]
    public void SearchByTitle_ReturnsMatchingItems()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        manager.AddItem("Breaking Bad", WatchItemType.TVShow, out _);
        manager.AddItem("The Batman", WatchItemType.Movie, out _);

        var results = manager.SearchByTitle("Break");

        Assert.Single(results);
        Assert.Equal("Breaking Bad", results[0].Title);
    }

    [Fact]
    public void FilterByStatus_ReturnsOnlyMatchingStatus()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        manager.AddItem("Severance", WatchItemType.TVShow, out _);
        manager.MarkCompleted(2);

        var results = manager.FilterByStatus(WatchStatus.Completed);

        Assert.Single(results);
        Assert.Equal("Severance", results[0].Title);
    }

    [Fact]
    public void UpdateProgress_SetsSeasonEpisodeAndStatus()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        bool updated = manager.UpdateProgress(1, 2, 5);
        var item = manager.GetItemByIndex(1);

        Assert.True(updated);
        Assert.NotNull(item);
        Assert.Equal(2, item!.LastWatchedSeason);
        Assert.Equal(5, item.LastWatchedEpisode);
        Assert.Equal(WatchStatus.InProgress, item.Status);
    }

    [Fact]
    public void RemoveItemByIndex_RemovesItemSuccessfully()
    {
        string fileName = $"test-{Guid.NewGuid()}.json";
        var storage = new WatchlistStorage(fileName);
        var manager = new WatchlistManager(storage);

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        bool removed = manager.RemoveItemByIndex(1);

        Assert.True(removed);
        Assert.Empty(manager.GetAllItems());
    }
}
