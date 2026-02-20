using JetBrains.Annotations;
using Project.Scripts.Components;
using Project.Scripts.Components.Interfaces;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using Project.Scripts.Interpreters.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Game.Entities
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Robot : AEntity, IProgrammable
    {
        public override float Radius => navMeshAgent.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        
        public override EntityType EntityType => EntityType.Robot;
        
        public string Name { get; set; }
        
        
        [SerializeField] private PlayerConfiguration configuration;
        
        
        private NavMeshAgent navMeshAgent;
        private NavMeshAgentMover mover;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            mover = new NavMeshAgentMover(this, navMeshAgent, configuration);

            Name = $"Robot {Random.Range(0, 10000)}";
        }
        
        
        #region Authorized Methods
        
        [UsedImplicitly]
        public IMover Mover
        {
            [AuthorizedHelper.AuthorizedSelfMethod] 
            get => mover;
        }
        
        #endregion
        
        
        #region Programmable
        
        public string Code { get; set; }
        
        #endregion
    }
}