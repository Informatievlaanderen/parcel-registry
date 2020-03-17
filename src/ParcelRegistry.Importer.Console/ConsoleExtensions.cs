namespace ParcelRegistry.Importer.Console
{
    using System;

    internal static class ConsoleExtensions
    {
        public static void WaitFor(ConsoleKey key)
        {
            ConsoleKeyInfo input;
            do
            {
                input = Console.ReadKey();
            } while (input.Key != key);
        }
    }

    public static class MapLogging
    {
        public static Action<string> Log { get; set; }

        static MapLogging() => Log = s => { };
    }
}
