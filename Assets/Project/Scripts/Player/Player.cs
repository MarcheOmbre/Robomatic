using JetBrains.Annotations;
using Project.Scripts.Entities;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters;
using Project.Scripts.Services.Components;
using UnityEngine;

namespace Project.Scripts.Player
{
    public class Player : AEntity
    {
        private const float SpeedComputationThreshold = 0.1f;
        
        
        public override EntityType EntityType => EntityType.Player;

        public IMover Mover
        {
            [AuthorizedHelper.AuthorizedMethod] 
            get => mover;
        }

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
        
        [SerializeField] private PlayerConfiguration configuration;
        [SerializeField] private CharacterController characterController;
        
        private CharacterControllerMover mover;
        
        private Vector3 lastPosition;
        private float lastComputedSpeedTime;


        protected override void Awake() => mover = new CharacterControllerMover(characterController, configuration);
        
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