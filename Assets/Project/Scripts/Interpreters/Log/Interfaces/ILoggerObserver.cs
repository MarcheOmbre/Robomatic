namespace Project.Scripts.Interpreters.Log.Interfaces
{
    public interface ILoggerObserver
    {
        public void OnLogCode(LogType logType, int? line, object reference, string message);
        
        public void OnLogInterpreter(LogType logType, string message);
    }
}