using System;

namespace Project.Scripts.Interpreters
{
    /// <summary>
    /// Attribute to use on allowed types that do not contain AuthorizedMethod attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class AuthorizedType : Attribute
    {
    }
    
    /// <summary>
    /// Attribute to use on allowed methods. No need to add the AuthorizedType attribute to the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizedMethod : Attribute
    {
    }
}