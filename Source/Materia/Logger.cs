namespace Materia
{
    internal static class Logger
    {
        public static void Log(object sender = null, string message = null)
        {
            Verse.Log.Message(sender != null ? $"[Materia][{nameof(sender)}] {message}" : $"[Materia] {message}");
        }

        public static void Error(object sender = null, string message = null)
        {
            Verse.Log.Error(sender != null ? $"[Materia][{nameof(sender)}] {message}" : $"[Materia] {message}");
        }
    }
}
