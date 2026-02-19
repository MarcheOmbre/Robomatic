using System;
using Project.Scripts.Interpreters;

namespace Project.Scripts.Entities
{
    [Flags]
    [Serializable]
    [AuthorizedHelper.AuthorizedType]
    public enum EntityType
    {
        None = 0,
        Unknown = 1 << 0,
        
        Robot = 1 << 5
    }
}