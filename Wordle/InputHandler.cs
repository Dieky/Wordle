using System;
using System.Collections.Generic;
using System.Text;

namespace Wordle
{
    internal class InputHandler
    {
        public static string CheckLettersOnly()
        {
            while (true)
            {
                string? input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("No input entered. Try again.");
                    Console.WriteLine();
                    continue;
                }

                if (input == "exit") return input;

                if (input.Length != 5)
                {
                    Console.WriteLine("Please enter exactly 5 letters.");
                    Console.WriteLine();
                    continue;
                }

                if (!input.All(c => c >= 'a' && c <= 'z'))
                {
                    Console.WriteLine("Only letters a-z are allowed.");
                    Console.WriteLine();
                    continue;
                }

                return input;
            }
        }

        public static string StartGameOptions(string input)
        {

            if (!input.All(c => c >= 'a' && c <= 'z'))
            {
                throw new ArgumentException("Only letters a-z are allowed.");
            }
            return input;
        }
    }
}
