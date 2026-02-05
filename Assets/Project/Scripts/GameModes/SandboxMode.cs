using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.GameModes.Abstracts;
using Project.Scripts.Spawner;
using UnityEngine;

namespace Project.Scripts.GameModes
{
    [Serializable]
    [CreateAssetMenu(fileName = "SandboxMode", menuName = "Game Modes/Sandbox Mode", order = 0)]
    public class SandboxMode : AGameMode
    {
        private HashSet<AEntity> spawnedEntities = new();
        
        [SerializeField] private Player.Player playerPrefab;
        [SerializeField] private Balloon balloonPrefab;
        
        public override void Initialize()
        {
            spawnedEntities.Add(SpawnerManager.Instance.Spawn(playerPrefab));
            spawnedEntities.Add(SpawnerManager.Instance.Spawn(balloonPrefab));
        }

        public override async Task Update()
        {
            while (true)
            {
                await Awaitable.EndOfFrameAsync();
            }
        }

        public override void Cleanup()
        {
            foreach (var entity in spawnedEntities)
                Destroy(entity.gameObject);
            
            spawnedEntities.Clear();
        }
    }
}