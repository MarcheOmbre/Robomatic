using System;
using Project.Scripts.Interpreters;

namespace Project.Scripts.Entities
{
    [Flags]
    [Serializable]
    [AuthorizedType]
    public enum EntityType
    {
        None = 0,
        
        Player = 1 << 5,
        
        Balloon = 1 << 15
    }
}