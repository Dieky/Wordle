using System.Text;


namespace Wordle
{
    internal class LogicEngine
    {
        private static readonly string path = Path.Combine(AppContext.BaseDirectory, "word-bank.csv");
        private readonly List<string> wordBank = File.ReadAllLines(path).ToList();
        private Random random = new Random();

        private string correctWord { get; set; }

        // 2D array: rows = attempts (6), columns = letters per word (5)
        private char[,] wordMatrix { get; set; }
        private (char, Feedback)[][]? charsUsed { get; set; }

        private int currentAttempt = 0;
        private int wordLength = 5;

        private enum Feedback
        {
            Absent,
            Present,
            Correct,
            None
        }

        public LogicEngine()
        {
            int index = random.Next(wordBank.Count);
            this.correctWord = wordBank[index];
            this.wordMatrix = new char[6, 5];
            ResetCharsUsed();
        }

        public LogicEngine(string correctWord)
        {

            if (string.IsNullOrWhiteSpace(correctWord) || correctWord.Length != 5)
            {
                throw new ArgumentException();
            }
            this.correctWord = correctWord.ToLower();
            this.wordMatrix = new char[6, 5];
            ResetCharsUsed();

        }


        public (bool, int) Play()
        {
            bool isWin = false;
            Console.Clear();

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter a 5-letter guess or type 'exit' to quit:");

                    string? input = InputHandler.CheckLettersOnly();

                    if (input == "exit")
                    {
                        break;
                    }

                    if (input.Length != 5)
                    {
                        throw new ArgumentException();
                    }

                    // Store the guess in the matrix
                    for (int c = 0; c < 5; c++)
                    {
                        wordMatrix[currentAttempt, c] = input[c];
                    }

                    currentAttempt++;

                    // Check win
                    if (input == correctWord)
                    {
                        PrintMatrix();
                        isWin = true;
                        break;
                    }

                    Console.Clear();
                    PrintMatrix();
                    Console.WriteLine();
                    PrintCharsUsed();
                    Console.WriteLine();

                    if (currentAttempt >= 6)
                    {
                        Console.WriteLine($"Out of attempts. The correct word was: {correctWord}");
                        break;
                    }
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid input. Please enter a 5-letter word. \n");
                    continue;
                }

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

        private void PrintMatrix()
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
                            UpdateCharsUsed(letter, Feedback.Correct);
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case Feedback.Present:
                            UpdateCharsUsed(letter, Feedback.Present);
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        default:
                            UpdateCharsUsed(letter, Feedback.Absent);
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

        private void ResetCharsUsed()
        {
            this.charsUsed = new (char, Feedback)[][]
            {
                [ ( 'q', Feedback.None ), ( 'w', Feedback.None ), ( 'e', Feedback.None ), ( 'r', Feedback.None ), ( 't', Feedback.None ), ( 'y', Feedback.None ), ( 'u', Feedback.None ), ( 'i', Feedback.None ), ( 'o', Feedback.None ), ( 'p', Feedback.None ) ],
                [ ( 'a', Feedback.None ), ( 's', Feedback.None ), ( 'd', Feedback.None ), ( 'f', Feedback.None ), ( 'g', Feedback.None ), ( 'h', Feedback.None ), ( 'j', Feedback.None ), ( 'k', Feedback.None ), ( 'l', Feedback.None ) ],
                [ ( 'z', Feedback.None ), ( 'x', Feedback.None ), ( 'c', Feedback.None ), ( 'v', Feedback.None ), ( 'b', Feedback.None ), ( 'n', Feedback.None ), ( 'm', Feedback.None ) ]
            };

        }

        private int GetFeedbackPriority(Feedback feedback)
        {
            return feedback switch
            {
                Feedback.Correct => 3,
                Feedback.Present => 2,
                Feedback.Absent => 1,
                _ => 0,
            };
        }

        private void UpdateCharsUsed(char letter, Feedback feedback)
        {
            for (int i = 0; i < this.charsUsed.Length; i++)
            {
                for (int j = 0; j < this.charsUsed[i].Length; j++)
                {
                    // Only update if the new feedback has a higher priority than the existing feedback
                    int priority = GetFeedbackPriority(feedback);
                    int existingPriority = GetFeedbackPriority(this.charsUsed[i][j].Item2);
                    if (this.charsUsed[i][j].Item1 == letter)
                    {
                        if (priority > existingPriority)
                        {
                            this.charsUsed[i][j].Item2 = feedback;
                        }
                        return;
                    }
                }
            }
        }

        private void PrintCharsUsed()
        {
            foreach (var row in this.charsUsed)
            {
                foreach (var (letter, feedback) in row)
                {
                    switch (feedback)
                    {
                        case Feedback.Correct:
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case Feedback.Present:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                        case Feedback.Absent:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        default:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }

                    Console.Write($" {letter} ");

                    // reset to original between letters to avoid wide background spans
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(' ');
                }

                Console.WriteLine();
            }
        }
    }
}
