using System;
using System.Collections.Generic;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Utils;

namespace Project.Scripts.Spawner
{
    public class SpawnerManager : MonoBehaviourSingleton<SpawnerManager>
    {
        private readonly HashSet<Spawn> spawns = new();
        
        
        public void Subscribe(Spawn spawn) => spawns.Add(spawn);

        public void Unsubscribe(Spawn spawn) => spawns.Remove(spawn);

        
        
        public AEntity Spawn(AEntity prefab, Spawn spawn = null)
        {
            if(prefab is null)
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            
            if(spawns.Count <= 0)
                throw new InvalidOperationException("No spawn available.");

            // If a specific spawn is provided, use it
            if (spawn != null)
            {
                if(!spawns.Contains(spawn))
                    throw new InvalidOperationException($"Spawn {spawn.GetType().Name} is not registered.");
                
                if (!spawn.TrySpawn(prefab, out var entity))
                    throw new InvalidOperationException($"Spawn {spawn.GetType().Name} cannot spawn {prefab.GetType().Name}.");
                
                return entity;
            }
            
            // Spawn on the first available spawn
            foreach (var registeredSpawn in spawns)
            {
                if(registeredSpawn.TrySpawn(prefab, out var entity))
                    return entity;
            }   
            
            throw new InvalidOperationException("No compatible spawn available.");
        }
    }
}