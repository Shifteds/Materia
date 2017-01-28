namespace Materia
{
    internal static class Logger
    {
        public static void Log(string message = null)
        {
            Verse.Log.Message($"[Materia] {message}");
        }

        public static void Error(string message = null)
        {
            Verse.Log.Error($"[Materia] {message}");
        }
    }
}
