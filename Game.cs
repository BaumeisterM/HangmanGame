namespace HangmanGame;

// Base Game class responsible for loading the word list and starting the menu
public class Game
{
    protected List<string> _wordList;

    public Game()
    {
        _wordList = LoadWords();
    }

    // Entry point used by Program.cs to start the game
    public void Run()
    {
        StartMenu menu = new StartMenu();
        menu.DisplayMenu();
    }

    // Load words from Content/Hangmanlist.txt in the application's base directory.
    // If the file is missing, a small fallback list is returned.
    private List<string> LoadWords()
    {
        // Absolute path to the debug folder
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(baseDir, "Content", "Hangmanlist.txt");

        if (!File.Exists(filePath))
        {
            Console.WriteLine("ERROR: File not found!");
            //Fallback words
            return new List<string> { "TEST", "GLOVE", "PROGRAMM" };
        }

        string content = File.ReadAllText(filePath);
        var list = content.Split(',').Where(w => !string.IsNullOrEmpty(w)).Select(w => w.Trim().ToUpper()).ToList();

        return list;
    }
}