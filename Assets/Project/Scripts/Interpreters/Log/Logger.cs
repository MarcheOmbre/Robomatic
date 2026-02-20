using System;
using System.Collections.Generic;
using Project.Scripts.Interpreters.Interfaces;

namespace Project.Scripts.Interpreters.Log
{
    public class Logger
    {
        public event Action<LogData> OnLogAdded = delegate { };
        
        private readonly int maxLogCount;
        private readonly List<LogData> logs = new();


        public Logger(int maxLogCount)
        {
            if(maxLogCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLogCount));
            
            this.maxLogCount = maxLogCount;
        }
        
        
        public IEnumerable<LogData> GetLogsForReference(IProgrammable reference)
        {
            if(reference is null)
                throw new ArgumentNullException(nameof(reference));
            
            return logs.FindAll(log => log.Reference == reference);
        }
        
        public void AddLog(IProgrammable reference, LogType logType, string message, int? line = null)
        {
            if(reference is null)
                throw new ArgumentNullException(nameof(reference));
            
            logs.Add(new LogData
            {
                SendTime = DateTime.Now,
                Reference = reference,
                LogType = logType,
                Message = message,
                Line = line
            });
            
            if(logs.Count > maxLogCount)
                logs.RemoveAt(0);
            
            OnLogAdded(logs[^1]);
        }
        
        public void RemoveLog(object reference)
        {
            if(reference is null)
                throw new ArgumentNullException(nameof(reference));
            
            logs.RemoveAll(log => log.Reference == reference);
        }
    }
}