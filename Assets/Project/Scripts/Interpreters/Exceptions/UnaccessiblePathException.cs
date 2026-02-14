using System;
using Project.Scripts.Interpreters.Exceptions.Abstracts;

namespace Project.Scripts.Interpreters.Exceptions
{
    public class UnaccessiblePathException : APlayerCodeException
    {
        private const string ErrorMessageType = "The path is unaccessible";
        
        public UnaccessiblePathException(string message) : base(ErrorMessageType, message)
        {
        }

        public UnaccessiblePathException(string message, Exception innerException) : base(ErrorMessageType, message, innerException)
        {
        }
    }
}