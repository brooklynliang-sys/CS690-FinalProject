using WatchList;

const string DataFileName = "watchlist.json";

Console.Title = "Chloe Watchlist Tracker (Iteration 2)";

var storage = new WatchlistStorage(DataFileName);
var manager = new WatchlistManager(storage);
var ui = new ConsoleUI(manager);

ui.Run();
