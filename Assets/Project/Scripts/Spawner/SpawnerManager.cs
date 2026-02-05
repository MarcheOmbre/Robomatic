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

        
        public AEntity Spawn(AEntity prefab)
        {
            if(spawns.Count <= 0)
                throw new InvalidOperationException("No spawn available.");
            
            foreach (var spawn in spawns)
            {
                if(spawn.TrySpawn(prefab, out var entity))
                    return entity;
            }
            
            throw new InvalidOperationException("No compatible spawn available.");
        }
    }
}