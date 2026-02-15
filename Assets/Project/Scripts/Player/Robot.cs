using JetBrains.Annotations;
using Project.Scripts.Components;
using Project.Scripts.Components.Interfaces;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Robot : AEntity
    {
        private const float SpeedComputationThreshold = 0.1f;
        
        public override float Radius => navMeshAgent.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        
        public override EntityType EntityType => EntityType.Player;
        

        public Vector3 Direction
        {
            [UsedImplicitly]
            [AuthorizedHelper.AuthorizedMethod] 
            get;
            private set;
        }
        
        public float Speed
        {
            [UsedImplicitly]
            [AuthorizedHelper.AuthorizedMethod] 
            get;
            private set;
        }
        
        public IMover Mover
        {
            [AuthorizedHelper.AuthorizedMethod] 
            get => mover;
        }
        
        
        [SerializeField] private PlayerConfiguration configuration;
        
        
        private NavMeshAgent navMeshAgent;
        private NavMeshAgentMover mover;
        private Vector3 lastPosition;
        private float lastComputedSpeedTime;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            mover = new NavMeshAgentMover(this, navMeshAgent, configuration);
        }

        private void Update()
        {
            ComputeSpeed();
        }

        private void ComputeSpeed()
        {
            var currentTime = Time.time;
            if(currentTime - lastComputedSpeedTime < SpeedComputationThreshold) 
                return;
            
            var currentPosition = transform.position;
            var direction = currentPosition - lastPosition;
            Direction = direction.normalized;
            Speed = direction.magnitude / SpeedComputationThreshold;
            lastPosition = currentPosition;
            lastComputedSpeedTime = currentTime;
        }
    }
}