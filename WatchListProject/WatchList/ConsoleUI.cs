namespace WatchList;

public class ConsoleUI
{
    private readonly WatchlistManager _manager;

    public ConsoleUI(WatchlistManager manager)
    {
        _manager = manager;
    }

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Watchlist Tracker ===");
            Console.WriteLine("1) Add watch item");
            Console.WriteLine("2) View watch list");
            Console.WriteLine("3) Update watching progress");
            Console.WriteLine("4) Resume watching");
            Console.WriteLine("5) Remove watch item");
            Console.WriteLine("6) Search by title");
            Console.WriteLine("7) Filter by status");
            Console.WriteLine("0) Exit");
            Console.WriteLine();

            int choice = PromptInt("Select an option", 0, 7);

            switch (choice)
            {
                case 1:
                    AddWatchItem();
                    break;
                case 2:
                    ViewWatchList();
                    break;
                case 3:
                    UpdateProgress();
                    break;
                case 4:
                    ResumeWatching();
                    break;
                case 5:
                    RemoveWatchItem();
                    break;
                case 6:
                    SearchByTitle();
                    break;
                case 7:
                    FilterByStatus();
                    break;
                case 0:
                    _manager.Save();
                    Console.WriteLine("\nSaved. Goodbye!");
                    return;
            }
        }
    }

    private void AddWatchItem()
    {
        Console.Clear();
        Console.WriteLine("=== Add Watch Item ===");

        string title = PromptNonEmpty("Title");
        var type = PromptEnum<WatchItemType>("Type (Movie/TVShow)");

        bool added = _manager.AddItem(title, type, out var message);
        Console.WriteLine();
        Console.WriteLine(message);
        Pause();
    }

    private void ViewWatchList()
    {
        Console.Clear();
        Console.WriteLine("=== Watch List ===");

        var items = _manager.GetAllItems();
        if (items.Count == 0)
        {
            Console.WriteLine("No items yet.");
            Pause();
            return;
        }

        PrintList(items);
        Pause();
    }

    private void UpdateProgress()
    {
        Console.Clear();
        Console.WriteLine("=== Update Watching Progress ===");

        var items = _manager.GetAllItems();
        if (items.Count == 0)
        {
            Console.WriteLine("No items to update.");
            Pause();
            return;
        }

        PrintList(items);
        Console.WriteLine();
        int index = PromptInt("Choose an item number to update", 1, items.Count);
        var item = items[index - 1];

        Console.Clear();
        Console.WriteLine("=== Update Item ===");
        Console.WriteLine($"{item.Title} [{item.Type}]");
        Console.WriteLine($"Current Status: {item.Status}");
        Console.WriteLine($"Current Progress: {item.ProgressText}");
        Console.WriteLine();
        Console.WriteLine("1) Update season and episode");
        Console.WriteLine("2) Mark as completed");
        Console.WriteLine("0) Cancel");
        Console.WriteLine();

        int action = PromptInt("Select an action", 0, 2);
        if (action == 0)
        {
            return;
        }

        if (action == 1)
        {
            int season = PromptInt("Enter last watched season number", 1, 1_000_000);
            int episode = PromptInt("Enter last watched episode number", 1, 1_000_000);
            _manager.UpdateProgress(index, season, episode);
            Console.WriteLine("\nProgress updated and saved.");
            Pause();
            return;
        }

        _manager.MarkCompleted(index);
        Console.WriteLine("\nMarked completed and saved.");
        Pause();
    }

    private void ResumeWatching()
    {
        Console.Clear();
        Console.WriteLine("=== Resume Watching ===");

        var items = _manager.GetAllItems();
        if (items.Count == 0)
        {
            Console.WriteLine("No items yet.");
            Pause();
            return;
        }

        PrintList(items);
        Console.WriteLine();
        int index = PromptInt("Choose an item number to view progress", 1, items.Count);
        Console.WriteLine();
        Console.WriteLine(_manager.GetResumeMessage(index));
        Pause();
    }

    private void RemoveWatchItem()
    {
        Console.Clear();
        Console.WriteLine("=== Remove Watch Item ===");

        var items = _manager.GetAllItems();
        if (items.Count == 0)
        {
            Console.WriteLine("No items to remove.");
            Pause();
            return;
        }

        PrintList(items);
        Console.WriteLine();
        int index = PromptInt("Choose an item number to remove", 1, items.Count);
        var item = items[index - 1];

        Console.Write($"\nRemove '{item.Title}'? (y/n): ");
        var answer = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
        if (answer != "y")
        {
            Console.WriteLine("\nCancelled.");
            Pause();
            return;
        }

        _manager.RemoveItemByIndex(index);
        Console.WriteLine("\nRemoved and saved.");
        Pause();
    }

    private void SearchByTitle()
    {
        Console.Clear();
        Console.WriteLine("=== Search by Title ===");
        string keyword = PromptNonEmpty("Enter title keyword");

        var results = _manager.SearchByTitle(keyword);
        Console.WriteLine();
        if (results.Count == 0)
        {
            Console.WriteLine("No matching items found.");
            Pause();
            return;
        }

        PrintList(results);
        Pause();
    }

    private void FilterByStatus()
    {
        Console.Clear();
        Console.WriteLine("=== Filter by Status ===");
        var status = PromptEnum<WatchStatus>("Status (NotStarted/InProgress/Completed)");

        var results = _manager.FilterByStatus(status);
        Console.WriteLine();
        if (results.Count == 0)
        {
            Console.WriteLine("No items found for this status.");
            Pause();
            return;
        }

        PrintList(results);
        Pause();
    }

    public static string FormatItem(WatchItem item, int itemNumber)
    {
        return $"{itemNumber}) {item.Title} [{item.Type}]  |  Status: {item.Status}  |  {item.ProgressText}";
    }

    private static void PrintList(List<WatchItem> watchlist)
    {
        for (int i = 0; i < watchlist.Count; i++)
        {
            Console.WriteLine(FormatItem(watchlist[i], i + 1));
        }
    }

    private static string PromptNonEmpty(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Please enter a value.");
        }
    }

    private static int PromptInt(string label, int min, int max)
    {
        while (true)
        {
            Console.Write($"{label} ({min}-{max}): ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine("Invalid number. Try again.");
        }
    }

    private static T PromptEnum<T>(string label) where T : struct, Enum
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (Enum.TryParse<T>(input, ignoreCase: true, out var value))
            {
                return value;
            }

            Console.WriteLine($"Invalid value. Options: {string.Join(", ", Enum.GetNames(typeof(T)))}");
        }
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress Enter to return...");
        Console.ReadLine();
    }
}
