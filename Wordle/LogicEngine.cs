using System.Security.Cryptography;
using System.Text;
using System.IO;


namespace Wordle
{
    internal class LogicEngine
    {
        private static readonly string path = Path.Combine(AppContext.BaseDirectory, "word-bank.csv");
        private readonly List<string> wordBank = File.ReadAllLines(path).ToList();
        private Random random = new Random();

        public LogicEngine()
        {
            int index = random.Next(wordBank.Count);
            this.correctWord = wordBank[index];
            this.wordMatrix = new char[6, 5];
        }

        public LogicEngine(string correctWord)
        {

            if (string.IsNullOrWhiteSpace(correctWord) || correctWord.Length != 5)
            {
                throw new ArgumentException("Correct word must be a non-empty string of exactly 5 letters.");
            }
            this.correctWord = correctWord.ToLower();
            this.wordMatrix = new char[6, 5];


        }
        private string correctWord { get; set; }

        // 2D array: rows = attempts (6), columns = letters per word (5)
        private char[,] wordMatrix { get; set; }

        private int currentAttempt = 0;
        private int wordLength = 5;

        private enum Feedback
        {
            Absent,
            Present,
            Correct
        }

        public (bool, int) Play()
        {
            bool isGameOver = false;
            bool isWin = false;
            Console.Clear();

            while (!isGameOver)
            {
                Console.WriteLine("Enter a 5-letter guess or type 'exit' to quit:");

                string? input = Console.ReadLine();

                input = input.Trim().ToLower();

                if (input.Length != 5)
                {
                    Console.WriteLine("Please enter exactly 5 letters.");
                    continue;
                }

                // Store the guess in the matrix
                for (int c = 0; c < 5; c++)
                {
                    wordMatrix[currentAttempt, c] = input[c];
                }

                // Check win
                if (input == correctWord)
                {
                    PrintMatrix();
                    isWin = true;
                    break;
                }

                if (currentAttempt >= 6)
                {
                    Console.WriteLine("No attempts left. Game over.");
                    break;
                }

                currentAttempt++;

                if (currentAttempt >= 6)
                {
                    Console.WriteLine($"Out of attempts. The correct word was: {correctWord}");
                    break;
                }

                if (isGameOver)
                {
                    Console.WriteLine();
                    Console.WriteLine("Game Over. Press any key to exit.");
                    Console.ReadKey();
                    break;
                }

                Console.Clear();
                PrintMatrix();
                Console.WriteLine();
            }
            return (isWin, currentAttempt);
        }


        // Compute feedback for a single 5-letter guess against correctWord
        private Feedback[] ApplyCorrectnessFeedback(string guess)
        {
            var result = new Feedback[wordLength];

            // First for loop mark correct letters and build counts of remaining letters in the correctWord
            var remaining = new Dictionary<char, int>();
            for (int i = 0; i < wordLength; i++)
            {
                if (guess[i] == correctWord[i])
                {
                    result[i] = Feedback.Correct;
                }
                else
                {
                    char a = correctWord[i];
                    if (remaining.ContainsKey(a))
                    {
                        remaining[a]++;
                    }
                    else
                    {
                        remaining[a] = 1;
                    }
                }
            }

            // Second for loop mark present (yellow) or absent
            for (int i = 0; i < 5; i++)
            {
                if (result[i] == Feedback.Correct) continue;

                char g = guess[i];
                if (remaining.TryGetValue(g, out int count) && count > 0)
                {
                    result[i] = Feedback.Present;
                    remaining[g] = count - 1;
                }
                else
                {
                    result[i] = Feedback.Absent;
                }
            }

            return result;
        }

        public void PrintMatrix()
        {
            var originalBg = Console.BackgroundColor;
            var originalFg = Console.ForegroundColor;

            // First for loop iterates over the rows (attempts) 
            for (int row = 0; row < 6; row++)
            {
                var sb = new StringBuilder(5);
                bool hasLetter = false;

                // Second for loop iterates over the columns (letters)
                for (int c = 0; c < 5; c++)
                {
                    char letter = wordMatrix[row, c];
                    if (letter == '\0')
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append(letter);
                        hasLetter = true;
                    }
                }

                if (!hasLetter)
                {
                    // empty row
                    Console.WriteLine("_ _ _ _ _");
                    continue;
                }

                string guess = sb.ToString();
                var feedback = ApplyCorrectnessFeedback(guess);

                // Third for loop iterates over the columns (letters) again but this time applies the feedback colors
                for (int c = 0; c < 5; c++)
                {
                    char letter = wordMatrix[row, c];

                    switch (feedback[c])
                    {
                        case Feedback.Correct:
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case Feedback.Present:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        default:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }

                    Console.Write($" {letter} ");

                    // reset to original between letters to avoid wide background spans
                    Console.BackgroundColor = originalBg;
                    Console.ForegroundColor = originalFg;
                    Console.Write(' ');
                }

                Console.WriteLine();
            }

            Console.BackgroundColor = originalBg;
            Console.ForegroundColor = originalFg;
        }
    }
}
