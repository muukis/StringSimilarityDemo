using System;
using System.Globalization;

namespace StringSimilarityDemo.Common
{
    public static class ConsoleLogger
    {
        public static readonly ConsoleColor OriginalColor = Console.ForegroundColor;
        
        public static ConsoleColor VerboseColor = ConsoleColor.DarkGray;
        public static ConsoleColor NormalColor = ConsoleColor.Gray;
        public static ConsoleColor WarningColor = ConsoleColor.DarkYellow;
        public static ConsoleColor ErrorColor = ConsoleColor.Red;
        public static ConsoleColor SuccessColor = ConsoleColor.DarkGreen;

        public static Options Options;

        public static void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        private static string GetManipulatedMessage(object message, bool writeTimeStamp)
        {
            return writeTimeStamp ? $"[{DateTime.Now.ToString("dd.MM.yyyy HH:mm.ss,ff", CultureInfo.InvariantCulture)}] {message}" : message?.ToString();
        }

        public static void Write(object message, ConsoleColor? color = null, bool writeTimeStamp = false)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }

                Console.Write(GetManipulatedMessage(message, writeTimeStamp));
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        public static void WriteLine(object message = null, ConsoleColor? color = null, bool writeTimeStamp = false)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }

                if (message != null)
                {
                    Console.WriteLine(GetManipulatedMessage(message, writeTimeStamp));
                }
                else
                {
                    Console.WriteLine();
                }
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        public static void Verbose(object message, bool writeTimeStamp = false)
        {
            if (Options?.Verbose ?? false)
            {
                WriteLine(message, VerboseColor, writeTimeStamp);
            }
        }

        public static void Normal(object message, bool writeTimeStamp = false)
        {
            WriteLine(message, NormalColor, writeTimeStamp);
        }

        public static void Warning(object message, bool writeTimeStamp = false)
        {
            WriteLine(message, WarningColor, writeTimeStamp);
        }

        public static void Error(object message, bool writeTimeStamp = false)
        {
            WriteLine(message, ErrorColor, writeTimeStamp);
        }

        public static void Success(object message, bool writeTimeStamp = false)
        {
            WriteLine(message, SuccessColor, writeTimeStamp);
        }
    }
}
