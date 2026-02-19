using System;
using System.Collections.Generic;
using Project.Scripts.Entities.Abstracts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Scripts.Entities
{
    public class EntitiesManager
    {
        public IReadOnlyList<AEntity> Entities => entities;
        
        private readonly List<AEntity> entities = new();
        
        
        public AEntity Spawn(AEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            
            var spawnedEntity = Object.Instantiate(entity, Vector3.zero, Quaternion.identity);
            spawnedEntity.Initialize(this);
            
            entities.Add(spawnedEntity);
            return spawnedEntity;
        }

        public void Despawn(AEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));
            
            if(!entities.Contains(entity))
                throw new ApplicationException("Entity not spawned using this manager, cannot despawn.");
            
            Object.Destroy(entity.gameObject);
            entities.Remove(entity);
        }
    }
}