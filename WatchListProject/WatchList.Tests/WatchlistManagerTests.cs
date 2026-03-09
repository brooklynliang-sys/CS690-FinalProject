using Xunit;

namespace WatchList.Tests;

public class WatchlistManagerTests
{
    private static WatchlistManager CreateManager()
    {
        var storage = new WatchlistStorage("test_watchlist.json");
        storage.Save(new List<WatchItem>());
        return new WatchlistManager(storage);
    }

    [Fact]
    public void AddItem_ShouldAddNewItem()
    {
        var manager = CreateManager();

        bool result = manager.AddItem("Breaking Bad", WatchItemType.TVShow, out var message);
        var items = manager.GetAllItems();

        Assert.True(result);
        Assert.Single(items);
        Assert.Equal("Breaking Bad", items[0].Title);
        Assert.Equal(WatchItemType.TVShow, items[0].Type);
        Assert.Equal(WatchStatus.NotStarted, items[0].Status);
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
    public void RemoveItemById_ShouldRemoveCorrectItem()
    {
        var manager = CreateManager();

        manager.AddItem("Zebra Show", WatchItemType.TVShow, out _);
        manager.AddItem("Alpha Show", WatchItemType.TVShow, out _);
        manager.AddItem("Moon Movie", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();
        var itemToRemove = items[2]; // choose from displayed sorted list

        bool removed = manager.RemoveItemById(itemToRemove.Id);
        var updatedItems = manager.GetAllItems();

        Assert.True(removed);
        Assert.DoesNotContain(updatedItems, x => x.Id == itemToRemove.Id);
        Assert.Equal(2, updatedItems.Count);
    }

    [Fact]
    public void RemoveItemById_ShouldReturnFalse_WhenIdNotFound()
    {
        var manager = CreateManager();

        manager.AddItem("Breaking Bad", WatchItemType.TVShow, out _);

        bool removed = manager.RemoveItemById(Guid.NewGuid());

        Assert.False(removed);
        Assert.Single(manager.GetAllItems());
    }

    [Fact]
    public void UpdateProgressById_ShouldUpdateSeasonEpisodeAndStatus()
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
    public void UpdateProgressById_ShouldReturnFalse_WhenIdNotFound()
    {
        var manager = CreateManager();

        bool updated = manager.UpdateProgressById(Guid.NewGuid(), 1, 1);

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
    public void MarkCompletedById_ShouldReturnFalse_WhenIdNotFound()
    {
        var manager = CreateManager();

        bool marked = manager.MarkCompletedById(Guid.NewGuid());

        Assert.False(marked);
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

    [Fact]
    public void FilterByStatus_ShouldReturnOnlyMatchingStatus()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        manager.AddItem("Interstellar", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();
        manager.MarkCompletedById(items[0].Id);

        var results = manager.FilterByStatus(WatchStatus.Completed);

        Assert.Single(results);
        Assert.Equal(WatchStatus.Completed, results[0].Status);
    }

    [Fact]
    public void GetResumeMessageById_ShouldReturnSeasonEpisodeMessage()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        var item = manager.GetAllItems().First();

        manager.UpdateProgressById(item.Id, 3, 7);
        string message = manager.GetResumeMessageById(item.Id);

        Assert.Equal("Resume Dark at Season 3, Episode 7.", message);
    }

    [Fact]
    public void GetResumeMessageById_ShouldReturnNoProgressMessage_WhenNoEpisodeRecorded()
    {
        var manager = CreateManager();

        manager.AddItem("Dark", WatchItemType.TVShow, out _);
        var item = manager.GetAllItems().First();

        string message = manager.GetResumeMessageById(item.Id);

        Assert.Equal("No episode progress recorded yet for Dark.", message);
    }

    [Fact]
    public void GetAllItems_ShouldReturnItemsSortedByTitle()
    {
        var manager = CreateManager();

        manager.AddItem("Zebra", WatchItemType.TVShow, out _);
        manager.AddItem("Alpha", WatchItemType.TVShow, out _);
        manager.AddItem("Moon", WatchItemType.Movie, out _);

        var items = manager.GetAllItems();

        Assert.Equal("Alpha", items[0].Title);
        Assert.Equal("Moon", items[1].Title);
        Assert.Equal("Zebra", items[2].Title);
    }
}