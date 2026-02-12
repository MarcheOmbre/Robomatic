using System;
using System.Collections.Generic;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Utils;

namespace Project.Scripts.Entities
{
    public class EntitiesManager : MonoBehaviourSingleton<EntitiesManager>
    {
        public IReadOnlyList<AEntity> Entities => entities;
        
        private readonly List<AEntity> entities = new();
        
        
        public void Subscribe(AEntity entity)
        {
            if (entities.Contains(entity))
                throw new ApplicationException("Entity already subscribed.");
            
            entities.Add(entity);
        }
        
        public void Unsubscribe(AEntity entity)
        {
            if(!entities.Contains(entity))
                throw new ApplicationException("Entity not subscribed.");
            
            entities.Remove(entity);
        }
    }
}