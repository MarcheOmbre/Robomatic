using System;
using Project.Scripts.Interpreters.Interfaces;

namespace Project.Scripts.Interpreters.Log
{
    public struct LogData
    {
        public DateTime SendTime;
        public IProgrammable Reference;
        public LogType LogType;
        public string Message;
        public int? Line;
    }
}