using System.Collections.Generic;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Utils;

namespace Project.Scripts.Entities
{
    public class EntitiesManager : MonoBehaviourSingleton<EntitiesManager>
    {
        public IReadOnlyCollection<AEntity> Entities => entities;
        
        private readonly HashSet<AEntity> entities = new();
        
        
        public void Subscribe(AEntity entity)
        {
            entities.Add(entity);
        }
        
        public void Unsubscribe(AEntity entity)
        {
            entities.Remove(entity);
        }
    }
}