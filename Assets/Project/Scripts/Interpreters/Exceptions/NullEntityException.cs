using System;
using Project.Scripts.Interpreters.Exceptions.Abstracts;

namespace Project.Scripts.Interpreters.Exceptions
{
    public class NullEntityException : APlayerCodeException
    {
        private const string ErrorMessageType = "Entity is null";
        
        public NullEntityException(string methodName) : base(methodName, ErrorMessageType) {}
        
        public NullEntityException(string methodName, Exception innerException) : base(methodName, ErrorMessageType, innerException) { }
    }
}