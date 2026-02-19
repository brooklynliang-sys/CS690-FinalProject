using System.Text.Json;
using System.Text.Json.Serialization;

const string DataFileName = "watchlist.json";

var storage = new WatchlistStorage(DataFileName);
var watchlist = storage.Load();

Console.Title = "Chloe Watchlist Tracker (Iteration 1)";

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Watchlist Tracker ===");
    Console.WriteLine("1) Add watch item");
    Console.WriteLine("2) View watch list");
    Console.WriteLine("3) Update watching progress");
    Console.WriteLine("4) Resume watching (show last progress)");
    Console.WriteLine("5) Remove watch item");
    Console.WriteLine("0) Exit");
    Console.WriteLine();

    int choice = PromptInt("Select an option", min: 0, max: 5);

    switch (choice)
    {
        case 1:
            AddWatchItem(watchlist, storage);
            break;
        case 2:
            ViewWatchList(watchlist);
            break;
        case 3:
            UpdateProgress(watchlist, storage);
            break;
        case 4:
            ResumeWatching(watchlist);
            break;
        case 5:
            RemoveWatchItem(watchlist, storage);
            break;
        case 0:
            storage.Save(watchlist);
            Console.WriteLine("\nSaved. Goodbye!");
            return;
    }
}

static void AddWatchItem(List<WatchItem> watchlist, WatchlistStorage storage)
{
    Console.Clear();
    Console.WriteLine("=== Add Watch Item ===");

    string title = PromptNonEmpty("Title");
    var type = PromptEnum<WatchItemType>("Type (Movie/TVShow)");

    // Optional: Avoid duplicates by title+type (friendly warning, still allow)
    bool exists = watchlist.Any(w =>
        string.Equals(w.Title, title, StringComparison.OrdinalIgnoreCase) && w.Type == type);

    if (exists)
    {
        Console.WriteLine("\nNote: An item with the same title/type already exists.");
        Console.Write("Add anyway? (y/n): ");
        var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (ans != "y")
        {
            Console.WriteLine("\nCancelled. Press Enter to return...");
            Console.ReadLine();
            return;
        }
    }

    var item = new WatchItem
    {
        Id = Guid.NewGuid(),
        Title = title.Trim(),
        Type = type,
        Status = WatchStatus.NotStarted,
        LastWatchedEpisode = null
    };

    watchlist.Add(item);
    storage.Save(watchlist);

    Console.WriteLine("\nAdded and saved. Press Enter to return...");
    Console.ReadLine();
}

static void ViewWatchList(List<WatchItem> watchlist)
{
    Console.Clear();
    Console.WriteLine("=== Watch List ===");

    if (watchlist.Count == 0)
    {
        Console.WriteLine("No items yet.");
        Console.WriteLine("\nPress Enter to return...");
        Console.ReadLine();
        return;
    }

    PrintList(watchlist);

    Console.WriteLine("\nPress Enter to return...");
    Console.ReadLine();
}

static void UpdateProgress(List<WatchItem> watchlist, WatchlistStorage storage)
{
    Console.Clear();
    Console.WriteLine("=== Update Watching Progress ===");

    if (watchlist.Count == 0)
    {
        Console.WriteLine("No items to update.");
        Console.WriteLine("\nPress Enter to return...");
        Console.ReadLine();
        return;
    }

    PrintList(watchlist);
    Console.WriteLine();

    int index = PromptInt("Choose an item number to update", 1, watchlist.Count) - 1;
    var item = watchlist[index];

    Console.Clear();
    Console.WriteLine("=== Update Item ===");
    Console.WriteLine($"{item.Title} [{item.Type}]");
    Console.WriteLine($"Current Status: {item.Status}");
    Console.WriteLine($"Last Watched Episode: {(item.LastWatchedEpisode.HasValue ? item.LastWatchedEpisode.Value.ToString() : "N/A")}");
    Console.WriteLine();
    Console.WriteLine("1) Update last watched episode");
    Console.WriteLine("2) Mark as completed");
    Console.WriteLine("0) Cancel");
    Console.WriteLine();

    int action = PromptInt("Select an action", 0, 2);

    if (action == 0) return;

    if (action == 1)
    {
        if (item.Type == WatchItemType.Movie)
        {
            Console.WriteLine("\nThis is a Movie. Episode tracking may not apply, but you can still store a number (e.g., part/segment).");
        }

        int ep = PromptInt("Enter last watched episode number (>= 1)", 1, 1_000_000);
        item.LastWatchedEpisode = ep;

        // If user updates progress, consider it InProgress unless already Completed.
        if (item.Status != WatchStatus.Completed)
            item.Status = WatchStatus.InProgress;

        storage.Save(watchlist);
        Console.WriteLine("\nUpdated and saved. Press Enter to return...");
        Console.ReadLine();
        return;
    }

    if (action == 2)
    {
        item.Status = WatchStatus.Completed;
        storage.Save(watchlist);
        Console.WriteLine("\nMarked completed and saved. Press Enter to return...");
        Console.ReadLine();
        return;
    }
}

static void ResumeWatching(List<WatchItem> watchlist)
{
    Console.Clear();
    Console.WriteLine("=== Resume Watching ===");

    if (watchlist.Count == 0)
    {
        Console.WriteLine("No items yet.");
        Console.WriteLine("\nPress Enter to return...");
        Console.ReadLine();
        return;
    }

    PrintList(watchlist);
    Console.WriteLine();

    int index = PromptInt("Choose an item number to view progress", 1, watchlist.Count) - 1;
    var item = watchlist[index];

    Console.Clear();
    Console.WriteLine("=== Progress Details ===");
    Console.WriteLine($"{item.Title} [{item.Type}]");
    Console.WriteLine($"Status: {item.Status}");

    if (item.LastWatchedEpisode.HasValue)
        Console.WriteLine($"Resume at episode: {item.LastWatchedEpisode.Value}");
    else
        Console.WriteLine("No episode progress recorded yet.");

    Console.WriteLine("\nPress Enter to return...");
    Console.ReadLine();
}

static void RemoveWatchItem(List<WatchItem> watchlist, WatchlistStorage storage)
{
    Console.Clear();
    Console.WriteLine("=== Remove Watch Item ===");

    if (watchlist.Count == 0)
    {
        Console.WriteLine("No items to remove.");
        Console.WriteLine("\nPress Enter to return...");
        Console.ReadLine();
        return;
    }

    PrintList(watchlist);
    Console.WriteLine();

    int index = PromptInt("Choose an item number to remove", 1, watchlist.Count) - 1;
    var item = watchlist[index];

    Console.Write($"\nRemove '{item.Title}'? (y/n): ");
    var ans = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
    if (ans != "y")
    {
        Console.WriteLine("\nCancelled. Press Enter to return...");
        Console.ReadLine();
        return;
    }

    watchlist.RemoveAt(index);
    storage.Save(watchlist);

    Console.WriteLine("\nRemoved and saved. Press Enter to return...");
    Console.ReadLine();
}

static void PrintList(List<WatchItem> watchlist)
{
    for (int i = 0; i < watchlist.Count; i++)
    {
        var w = watchlist[i];
        string ep = w.LastWatchedEpisode.HasValue ? $"Ep {w.LastWatchedEpisode.Value}" : "No progress";
        Console.WriteLine($"{i + 1}) {w.Title} [{w.Type}]  |  Status: {w.Status}  |  {ep}");
    }
}

static string PromptNonEmpty(string label)
{
    while (true)
    {
        Console.Write($"{label}: ");
        var input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
            return input;
        Console.WriteLine("Please enter a value.");
    }
}

static int PromptInt(string label, int min, int max)
{
    while (true)
    {
        Console.Write($"{label} ({min}-{max}): ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out int v) && v >= min && v <= max)
            return v;
        Console.WriteLine("Invalid number. Try again.");
    }
}

static T PromptEnum<T>(string label) where T : struct, Enum
{
    while (true)
    {
        Console.Write($"{label}: ");
        var input = (Console.ReadLine() ?? "").Trim();

        if (Enum.TryParse<T>(input, ignoreCase: true, out var value))
            return value;

        Console.WriteLine($"Invalid value. Options: {string.Join(", ", Enum.GetNames(typeof(T)))}");
    }
}

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
    public string Title { get; set; } = "";
    public WatchItemType Type { get; set; }
    public WatchStatus Status { get; set; }
    public int? LastWatchedEpisode { get; set; }
}

public class WatchlistStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public WatchlistStorage(string fileName)
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public List<WatchItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<WatchItem>();

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<WatchItem>>(json, _jsonOptions);
            return data ?? new List<WatchItem>();
        }
        catch
        {
            // For Iteration 1: keep it simple—if load fails, start empty.
            return new List<WatchItem>();
        }
    }

    public void Save(List<WatchItem> watchlist)
    {
        var json = JsonSerializer.Serialize(watchlist, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
