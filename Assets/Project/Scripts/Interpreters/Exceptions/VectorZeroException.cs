using System;
using Project.Scripts.Interpreters.Exceptions.Abstracts;

namespace Project.Scripts.Interpreters.Exceptions
{
    public class VectorZeroException : APlayerCodeException
    {
        private const string ErrorMessageType = "Vector cannot be zero";
        
        public VectorZeroException(string methodName) : base(methodName, ErrorMessageType) { }

        public VectorZeroException(string methodName, Exception innerException) : base(methodName, ErrorMessageType, innerException) { }
    }
}