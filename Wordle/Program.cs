namespace Wordle
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LogicEngine logicEngine;
            Console.WriteLine("Welcome to Wordle. Hit enter to start a default game or enter a 5-letter word to start playing. To exit, type 'exit' and hit enter. \n");
            while (true)
            {
                string? input = Console.ReadLine()?.Trim().ToLower();
                try
                {
                    input = InputHandler.StartGameOptions(input);
                    if (input == "exit")
                    {
                        Console.WriteLine("Exiting...");
                        break;
                    }
                    // start a default game if no input is provided, otherwise start a game with the provided word
                    var (result, attempts) = (false, 0);
                    if (input == string.Empty)
                    {
                        Console.WriteLine("Starting a default game with a random 5-letter word. \n");
                        logicEngine = new LogicEngine();
                        (result, attempts) = logicEngine.Play();
                    }
                    else
                    {
                        logicEngine = new LogicEngine(input);
                        (result, attempts) = logicEngine.Play();
                    }

                    if (result)
                    {
                        Console.WriteLine($"Congratulations! You guessed the correct word: {input} in {attempts} attempts \n");
                    }
                    else
                    {
                        Console.WriteLine($"Sorry, you did not guess the correct word: {input} \n");
                        Console.WriteLine("Hit enter to start a new game with a random word or type a word for someone else to guess. Type 'exit' and hit enter to fully exit the game. \n");
                    }
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"You entered {input}. Please enter a 5-letter word instead and hit enter. \n");
                }
            }

        }
    }
}
