namespace WatchList;

public class WatchlistManager
{
    private readonly WatchlistStorage _storage;
    private readonly List<WatchItem> _watchlist;

    public WatchlistManager(WatchlistStorage storage)
    {
        _storage = storage;
        _watchlist = _storage.Load();
    }

    public List<WatchItem> GetAllItems()
    {
        return _watchlist
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool HasItems()
    {
        return _watchlist.Count > 0;
    }

    public bool AddItem(string title, WatchItemType type, out string message)
    {
        bool exists = _watchlist.Any(w =>
            string.Equals(w.Title, title, StringComparison.OrdinalIgnoreCase) &&
            w.Type == type);

        if (exists)
        {
            message = "An item with the same title and type already exists.";
            return false;
        }

        _watchlist.Add(new WatchItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Type = type,
            Status = WatchStatus.NotStarted,
            LastWatchedSeason = null,
            LastWatchedEpisode = null
        });

        Save();
        message = "Item added successfully.";
        return true;
    }

    public bool RemoveItemByIndex(int oneBasedIndex)
    {
        int index = oneBasedIndex - 1;

        if (index < 0 || index >= _watchlist.Count)
        {
            return false;
        }

        _watchlist.RemoveAt(index);
        Save();
        return true;
    }

    public bool RemoveItemById(Guid id)
    {
        var item = _watchlist.FirstOrDefault(x => x.Id == id);

        if (item is null)
        {
            return false;
        }

        _watchlist.Remove(item);
        Save();
        return true;
    }

    public WatchItem? GetItemByIndex(int oneBasedIndex)
    {
        int index = oneBasedIndex - 1;

        if (index < 0 || index >= _watchlist.Count)
        {
            return null;
        }

        return _watchlist[index];
    }

    public WatchItem? GetItemById(Guid id)
    {
        return _watchlist.FirstOrDefault(x => x.Id == id);
    }

    public bool MarkCompleted(int oneBasedIndex)
    {
        var item = GetItemByIndex(oneBasedIndex);

        if (item is null)
        {
            return false;
        }

        item.Status = WatchStatus.Completed;
        Save();
        return true;
    }

    public bool MarkCompletedById(Guid id)
    {
        var item = GetItemById(id);

        if (item is null)
        {
            return false;
        }

        item.Status = WatchStatus.Completed;
        Save();
        return true;
    }

    public bool UpdateProgress(int oneBasedIndex, int season, int episode)
    {
        var item = GetItemByIndex(oneBasedIndex);

        if (item is null)
        {
            return false;
        }

        item.LastWatchedSeason = season;
        item.LastWatchedEpisode = episode;

        if (item.Status != WatchStatus.Completed)
        {
            item.Status = WatchStatus.InProgress;
        }

        Save();
        return true;
    }

    public bool UpdateProgressById(Guid id, int season, int episode)
    {
        var item = GetItemById(id);

        if (item is null)
        {
            return false;
        }

        item.LastWatchedSeason = season;
        item.LastWatchedEpisode = episode;

        if (item.Status != WatchStatus.Completed)
        {
            item.Status = WatchStatus.InProgress;
        }

        Save();
        return true;
    }

    public List<WatchItem> SearchByTitle(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<WatchItem>();
        }

        return _watchlist
            .Where(x => x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public List<WatchItem> FilterByStatus(WatchStatus status)
    {
        return _watchlist
            .Where(x => x.Status == status)
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public string GetResumeMessage(int oneBasedIndex)
    {
        var item = GetItemByIndex(oneBasedIndex);

        if (item is null)
        {
            return "Item not found.";
        }

        if (item.LastWatchedSeason.HasValue && item.LastWatchedEpisode.HasValue)
        {
            return $"Resume {item.Title} at Season {item.LastWatchedSeason.Value}, Episode {item.LastWatchedEpisode.Value}.";
        }

        if (item.LastWatchedEpisode.HasValue)
        {
            return $"Resume {item.Title} at Episode {item.LastWatchedEpisode.Value}.";
        }

        return $"No episode progress recorded yet for {item.Title}.";
    }

    public string GetResumeMessageById(Guid id)
    {
        var item = GetItemById(id);

        if (item is null)
        {
            return "Item not found.";
        }

        if (item.LastWatchedSeason.HasValue && item.LastWatchedEpisode.HasValue)
        {
            return $"Resume {item.Title} at Season {item.LastWatchedSeason.Value}, Episode {item.LastWatchedEpisode.Value}.";
        }

        if (item.LastWatchedEpisode.HasValue)
        {
            return $"Resume {item.Title} at Episode {item.LastWatchedEpisode.Value}.";
        }

        return $"No episode progress recorded yet for {item.Title}.";
    }

    public void Save()
    {
        _storage.Save(_watchlist);
    }
}