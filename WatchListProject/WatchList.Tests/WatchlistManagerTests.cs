using WatchList;
using Xunit;

namespace WatchList.Tests;

public class WatchlistManagerTests
{
    private static WatchlistManager CreateManager()
    {
        var storage = new WatchlistStorage($"test-{Guid.NewGuid()}.json");
        storage.Save(new List<WatchItem>());
        return new WatchlistManager(storage);
    }

    [Fact]
    public void AddItem_ShouldStoreTitleAndType()
    {
        var manager = CreateManager();

        bool result = manager.AddItem("Breaking Bad", WatchItemType.TVShow, out var message);
        var item = Assert.Single(manager.GetAllItems());

        Assert.True(result);
        Assert.Equal("Breaking Bad", item.Title);
        Assert.Equal(WatchItemType.TVShow, item.Type);
        Assert.Equal(WatchStatus.NotStarted, item.Status);
        Assert.Equal("Item added successfully.", message);
    }

    [Fact]
    public void AddItem_ShouldRejectDuplicateTitleAndType()
    {
        var manager = CreateManager();

        manager.AddItem("Breaking Bad", WatchItemType.TVShow, out _);
        bool result = manager.AddItem("Breaking Bad", WatchItemType.TVShow, out var message);

        Assert.False(result);
        Assert.Equal("An item with the same title and type already exists.", message);
        Assert.Single(manager.GetAllItems());
    }

    [Fact]
    public void GetAllItems_ShouldDisplayAllSavedItemsSorted()
    {
        var manager = CreateManager();

        manager.AddItem("Zebra", WatchItemType.TVShow, out _);
        manager.AddItem("Alpha", WatchItemType.TVShow, out _);
        manager.AddItem("Moon", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();

        Assert.Equal(3, items.Count);
        Assert.Equal("Alpha", items[0].Title);
        Assert.Equal("Moon", items[1].Title);
        Assert.Equal("Zebra", items[2].Title);
    }

    [Fact]
    public void UpdateProgressById_ShouldUpdateLastWatchedEpisode_AndSetInProgress()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        var item = manager.GetAllItems().First();

        bool updated = manager.UpdateProgressById(item.Id, 2, 5);
        var updatedItem = manager.GetItemById(item.Id);

        Assert.True(updated);
        Assert.NotNull(updatedItem);
        Assert.Equal(2, updatedItem!.LastWatchedSeason);
        Assert.Equal(5, updatedItem.LastWatchedEpisode);
        Assert.Equal(WatchStatus.InProgress, updatedItem.Status);
    }

    [Fact]
    public void UpdateProgressById_ShouldReturnFalse_ForMovie()
    {
        var manager = CreateManager();

        manager.AddItem("Interstellar", WatchItemType.Movie, out _);
        var item = manager.GetAllItems().First();

        bool updated = manager.UpdateProgressById(item.Id, 1, 1);

        Assert.False(updated);
    }

    [Fact]
    public void MarkCompletedById_ShouldSetStatusToCompleted()
    {
        var manager = CreateManager();

        manager.AddItem("Interstellar", WatchItemType.Movie, out _);
        var item = manager.GetAllItems().First();

        bool marked = manager.MarkCompletedById(item.Id);
        var updatedItem = manager.GetItemById(item.Id);

        Assert.True(marked);
        Assert.NotNull(updatedItem);
        Assert.Equal(WatchStatus.Completed, updatedItem!.Status);
    }

    [Fact]
    public void FilterByStatus_ShouldReturnOnlyMatchingStatus()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        manager.AddItem("Interstellar", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();
        manager.MarkCompletedById(items[0].Id);

        var results = manager.FilterByStatus(WatchStatus.Completed);

        var onlyItem = Assert.Single(results);
        Assert.Equal(WatchStatus.Completed, onlyItem.Status);
    }

    [Fact]
    public void GetResumeMessageById_ShouldShowSelectedItemProgress()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        var item = manager.GetAllItems().First();

        manager.UpdateProgressById(item.Id, 3, 7);
        string message = manager.GetResumeMessageById(item.Id);

        Assert.Equal("Resume Dark at Season 3, Episode 7.", message);
    }

    [Fact]
    public void GetResumeMessageById_ShouldShowMovieResumeMessage()
    {
        var manager = CreateManager();

        manager.AddItem("Interstellar", WatchItemType.Movie, out _);
        var item = manager.GetAllItems().First();
        manager.MarkInProgressById(item.Id);

        string message = manager.GetResumeMessageById(item.Id);

        Assert.Equal("Resume Interstellar.", message);
    }

    [Fact]
    public void RemoveItemById_ShouldRemoveCorrectItem_AndUpdateWatchList()
    {
        var manager = CreateManager();

        manager.AddItem("Zebra Show", WatchItemType.TVShow, out _);
        manager.AddItem("Alpha Show", WatchItemType.TVShow, out _);
        manager.AddItem("Moon Movie", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();
        var itemToRemove = items[2];

        bool removed = manager.RemoveItemById(itemToRemove.Id);
        var updatedItems = manager.GetAllItems();

        Assert.True(removed);
        Assert.DoesNotContain(updatedItems, x => x.Id == itemToRemove.Id);
        Assert.Equal(2, updatedItems.Count);
    }

    [Fact]
    public void SearchByTitle_ShouldReturnMatchingItems()
    {
        var manager = CreateManager();

        manager.AddItem("Breaking Bad", WatchItemType.TVShow, out _);
        manager.AddItem("Bad Boys", WatchItemType.Movie, out _);
        manager.AddItem("Friends", WatchItemType.TVShow, out _);

        var results = manager.SearchByTitle("Bad");

        Assert.Equal(2, results.Count);
        Assert.All(results, item => Assert.Contains("Bad", item.Title, StringComparison.OrdinalIgnoreCase));
    }
}
