using Project.Scripts.Entities.Abstracts;
using UnityEngine;

namespace Project.Scripts.Entities
{
    public class SimpleEntity : AEntity
    {
        public override float Radius => radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        
        public override EntityType EntityType => EntityType.Unknown;

        
        [SerializeField][Min(0)] private float radius = 1f;
        
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected() => Gizmos.DrawWireSphere(transform.position, Radius);
#endif
    }
}