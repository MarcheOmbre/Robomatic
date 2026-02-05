using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using UnityEngine;

namespace Project.Scripts.Spawner
{
    public class Spawn : MonoBehaviour
    {
        [SerializeField] private EntityType allowedEntityTypes;


        private void OnEnable()
        {
            SpawnerManager.Instance.Subscribe(this);
        }
        
        private void OnDisable()
        {
            SpawnerManager.Instance.Unsubscribe(this);
        }

        
        public bool TrySpawn(AEntity prefab, out AEntity entity)
        {
            // Check prefab
            if(prefab == null)
            {
                entity = null;
                return false;
            }
            
            // Check entity type
            if (!allowedEntityTypes.HasFlag(prefab.EntityType))
            {
                entity = null;
                return false;
            }
            
            entity = Instantiate(prefab, transform.position, transform.rotation);
            return true;
        }
    }
}