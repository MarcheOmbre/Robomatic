using System;

namespace Project.Scripts.Interpreters.Exceptions.Abstracts
{
    public abstract class APlayerCodeException : Exception
    {
        private static string FormatException(string methodName, string message) => $"{methodName}: {message}";
        
        protected APlayerCodeException(string methodName, string message) : base(FormatException(methodName, message)) { }

        protected APlayerCodeException (string methodName, string message, Exception innerException) : base (FormatException(methodName, message), innerException) {}
    }
}