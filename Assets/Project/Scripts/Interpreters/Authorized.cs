using System;

namespace Project.Scripts.Interpreters
{
    // Make this attribute depending on the presence of AuthorizedClasses
    [AttributeUsage(AttributeTargets.Method)]
    public class Authorized : Attribute
    {
    }
}