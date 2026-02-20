using JetBrains.Annotations;
using Project.Scripts.Game;
using Project.Scripts.Interpreters.Interfaces;
using Project.Scripts.Interpreters.Log;

namespace Project.Scripts.Interpreters.Libraries
{
    public static class Internal
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static void Log(IProgrammable reference, string message, LogType logType = LogType.Information)
        {
            var currentLogObject = References.Instance.GameManager.CodeEditor.CurrentProgrammable;
            if (currentLogObject == null)
                return;

            if (string.IsNullOrEmpty(message))
                return;
            
            References.Instance.GameManager.Logger.AddLog(reference, logType, message);
        }
    }
}