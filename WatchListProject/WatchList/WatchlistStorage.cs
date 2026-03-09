using System.Text.Json;
using System.Text.Json.Serialization;

namespace WatchList;

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
            {
                return new List<WatchItem>();
            }

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<WatchItem>>(json, _jsonOptions);
            return data ?? new List<WatchItem>();
        }
        catch
        {
            return new List<WatchItem>();
        }
    }

    public void Save(List<WatchItem> watchlist)
    {
        var json = JsonSerializer.Serialize(watchlist, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
