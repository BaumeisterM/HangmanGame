namespace HangmanGame;

// ClassicMode implements a simple hangman game where the player
// guesses a single word with a limited number of lives.
public class ClassicMode : Game
{
    private readonly Random rnd = new Random();
    private string _secretWord;
    private int _livesUsed = 10;

    public ClassicMode() : base() { }

    // Starts the classic game loop. Reads user input from the console
    // until the player guesses the word or exhausts their lives.
    public void StartClassicGame()
    {
        _secretWord = NextSecretWord();
        string hidden = HiddenWord(_secretWord);
        string incorrectLetters = "";

        do
        {
            Console.Clear();
            Console.WriteLine($"To exit the game, write 'exit'.");
            Console.Write($"Word: {hidden}\t| Remaining: {_livesUsed}\t| Incorrect: {incorrectLetters}\t| Guess: "); ;
            string input = Console.ReadLine();

            if (input?.ToLower() == "exit") break; // Allow early exit

            if (input?.Length == 1)
            {
                // Ignore letters that were already tried
                if (incorrectLetters.Contains(input.ToUpper())) continue; // Repeat the loop

                // Penalize for incorrect guess
                if (!_secretWord.Contains(input.ToUpper())) // if word don't contains user input
                {
                    _livesUsed--; // Decrement lives used 
                    incorrectLetters += input.ToUpper(); // Add letter incorrectLetters
                }

                if (_livesUsed <= 0) break;

                // Reveal guessed letters
                hidden = UpdateHiddenWord(hidden, _secretWord, input.ToUpper()[0]);
            }
        } while (hidden.Contains('_'));

        // Show result
        Console.Clear();
        if (!hidden.Contains('_')) Console.WriteLine($"{hidden} is correct! Congratulation!");
        else Console.WriteLine("You lose!.");

        Console.ReadKey();

        Console.Clear();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(); // Wait for user input
    }

    // Reveal guessed letters in the hidden representation
    private string UpdateHiddenWord(string currentGuess, string secretWord, char guessedLetter)
    {
        char[] updateGuess = currentGuess.ToCharArray();

        for (int i = 0; i < secretWord.Length; i++)
        {
            if (secretWord[i] == guessedLetter)
            {
                updateGuess[i] = guessedLetter;
            }
        }
        return new string(updateGuess);
    }

    // Create an initial masked string of underscores matching the secret word length
    private string HiddenWord(string secretWord) =>
        new string('_', secretWord.Length); // Use secretWord to create hidden word

    // Pick the next secret word randomly from the loaded word list
    private string NextSecretWord()
    {
        int i = rnd.Next(0, _wordList.Count);
        return _wordList[i];
    }
}
