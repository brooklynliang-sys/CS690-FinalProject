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
            .ThenBy(x => x.Type)
            .ToList();
    }

    public bool HasItems() => _watchlist.Count > 0;

    public bool AddItem(string title, WatchItemType type, out string message)
    {
        var normalizedTitle = title.Trim();

        bool exists = _watchlist.Any(w =>
            string.Equals(w.Title, normalizedTitle, StringComparison.OrdinalIgnoreCase) &&
            w.Type == type);

        if (exists)
        {
            message = "An item with the same title and type already exists.";
            return false;
        }

        _watchlist.Add(new WatchItem
        {
            Id = Guid.NewGuid(),
            Title = normalizedTitle,
            Type = type,
            Status = WatchStatus.NotStarted
        });

        Save();
        message = "Item added successfully.";
        return true;
    }

    public WatchItem? GetItemById(Guid id) => _watchlist.FirstOrDefault(x => x.Id == id);

    public bool RemoveItemById(Guid id)
    {
        var item = GetItemById(id);
        if (item is null)
        {
            return false;
        }

        _watchlist.Remove(item);
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

    public bool MarkInProgressById(Guid id)
    {
        var item = GetItemById(id);
        if (item is null)
        {
            return false;
        }

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
        if (item is null || item.Type != WatchItemType.TVShow)
        {
            return false;
        }

        item.LastWatchedSeason = season;
        item.LastWatchedEpisode = episode;
        item.Status = WatchStatus.InProgress;

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
            .Where(x => x.Title.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Type)
            .ToList();
    }

    public List<WatchItem> FilterByStatus(WatchStatus status)
    {
        return _watchlist
            .Where(x => x.Status == status)
            .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Type)
            .ToList();
    }

    public string GetResumeMessageById(Guid id)
    {
        var item = GetItemById(id);
        if (item is null)
        {
            return "Item not found.";
        }

        if (item.Type == WatchItemType.Movie)
        {
            return item.Status switch
            {
                WatchStatus.Completed => $"{item.Title} is already completed.",
                WatchStatus.InProgress => $"Resume {item.Title}.",
                _ => $"No progress recorded yet for {item.Title}."
            };
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

    public void Save() => _storage.Save(_watchlist);
}
