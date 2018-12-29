using System;

namespace StringSimilarityDemo.Common
{
    public static class ConsoleLogger
    {
        public static readonly ConsoleColor OriginalColor = Console.ForegroundColor;
        
        public static ConsoleColor VerboseColor = ConsoleColor.DarkGray;
        public static ConsoleColor NormalColor = ConsoleColor.White;
        public static ConsoleColor WarningColor = ConsoleColor.DarkYellow;
        public static ConsoleColor ErrorColor = ConsoleColor.Red;
        public static ConsoleColor SuccessColor = ConsoleColor.DarkGreen;

        public static Options Options;

        public static void SetColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }

        public static void Write(object message, ConsoleColor? color = null)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                if (color.HasValue)
                {
                    Console.ForegroundColor = color.Value;
                }

                Console.Write(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        public static void WriteLine(object message = null, ConsoleColor? color = null)
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
                    Console.WriteLine(message);
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

        public static void Verbose(object message)
        {
            if (Options?.Verbose ?? false)
            {
                WriteLine(message, VerboseColor);
            }
        }

        public static void Normal(object message)
        {
            WriteLine(message, NormalColor);
        }

        public static void Warning(object message)
        {
            WriteLine(message, WarningColor);
        }

        public static void Error(object message)
        {
            WriteLine(message, ErrorColor);
        }

        public static void Success(object message)
        {
            WriteLine(message, SuccessColor);
        }
    }
}
