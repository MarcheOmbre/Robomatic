using Project.Scripts.Entities.Abstracts;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Entities
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class SimpleEntity : AEntity
    {
        public override float Radius => navMeshObstacle.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        
        public override EntityType EntityType => EntityType.SimpleEntity;

        
        private NavMeshObstacle navMeshObstacle;
        
        private void Awake() => navMeshObstacle = GetComponent<NavMeshObstacle>();
    }
}