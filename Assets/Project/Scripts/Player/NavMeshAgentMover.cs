using System;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters.Exceptions;
using Project.Scripts.Services.Components;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Player
{
    public class NavMeshAgentMover : IMover
    {
        private const int MovementFrameOffset = 5;
        
        private readonly NavMeshAgent navMeshAgent;
        private readonly Collider collider;
        private readonly float stoppingDistance;
        
        private float RotationDeltaTime
        {
            get
            {
                var currentTime = Time.time;
                var deltaTime = currentTime - lastRotationTime;
                return Mathf.Min(deltaTime, Time.deltaTime);
            }
        }
        
        private readonly AEntity entity;
        private float lastRotationTime;
        private bool isMovingInProcess;


        public NavMeshAgentMover(AEntity entity, NavMeshAgent navMeshAgent, ISpeedConfiguration configuration)
        {
            this.entity = entity ?? throw new ArgumentNullException(nameof(entity), "Agent entity cannot be null");
            this.navMeshAgent = navMeshAgent ?? throw new ArgumentNullException(nameof(navMeshAgent), "NavMeshAgent cannot be null");
            stoppingDistance = navMeshAgent.stoppingDistance;
            
            this.navMeshAgent.speed = configuration.TranslationSpeed;
            this.navMeshAgent.angularSpeed = configuration.RotationSpeed;
            this.navMeshAgent.destination = entity.Position.ToVector3();
        }
        
        // ReSharper disable once AsyncVoidMethod
        private async void MoveTo(Vector2 targetPosition, float stopDistance)
        {
            // Avoid the moving process to be called too much time
            if (isMovingInProcess)
                return;
            isMovingInProcess = true;
            
            // Compute the distance
            var distance = Vector2.Distance(navMeshAgent.destination.ToVector2(), targetPosition);
            if (distance > 0)
            {
                navMeshAgent.stoppingDistance = stopDistance;
                if(!navMeshAgent.SetDestination(targetPosition.ToVector3()))
                    throw new UnaccessiblePathException(nameof(MoveTo));
            }

            // Move
            for(var i = 0; i < MovementFrameOffset; i++)
                await Awaitable.NextFrameAsync();
            Stop();
            
            // Close the process
            isMovingInProcess = false;
        }
        
        private void Stop() => navMeshAgent.destination = entity.transform.position;


        public bool LookAt(Vector2 targetDirection)
        {
            navMeshAgent.updateRotation = false;
            var rotateDirectionTarget = targetDirection.normalized.ToVector3();
            if (rotateDirectionTarget == Vector3.zero)
                throw new VectorZeroException(nameof(LookAt));

            var currentRotation = navMeshAgent.transform.rotation;
            var targetRotation = Quaternion.LookRotation(rotateDirectionTarget, Vector3.up);

            var lastRotation = navMeshAgent.transform.rotation;
            
            navMeshAgent.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, navMeshAgent.angularSpeed * RotationDeltaTime);
            
            navMeshAgent.updateRotation = true;
            return lastRotation == navMeshAgent.transform.rotation;
        }

        public bool LookAt(AEntity targetEntity)
        {
            if (targetEntity is null)
                throw new NullEntityException(nameof(LookAt));

            var entityPositionVector2 = targetEntity.Position;
            var characterPositionVector2 = entity.Position;
            
            return LookAt(entityPositionVector2 - characterPositionVector2);
        }

        public void MoveToward(Vector2 targetDirection)
        {
            if(targetDirection == Vector2.zero)
                return;

            var direction = targetDirection.normalized.ToVector3();
            var position = (entity.transform.position + direction * navMeshAgent.speed).ToVector2();
            
            MoveTo(position, stoppingDistance);
        }
        
        public bool Reach(Vector2 targetPosition)
        {
            MoveTo(targetPosition, stoppingDistance);
            
            if(navMeshAgent.pathPending)
                return false;
            
            if(navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                return false;
            
            Stop();
            return true;
        }

        public bool Reach(AEntity targetEntity)
        {
            if (targetEntity is null)
                throw new NullEntityException(nameof(Reach));
            
            var limitDistance = entity.Radius + targetEntity.Radius;
            MoveTo(targetEntity.Position, limitDistance);
            
            if(navMeshAgent.pathPending)
                return false;

            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                return false;

            Stop();
            return true;
        }

        public void TurnAround(Vector2 center, bool clockWise = true, float? radius = null)
        {
            // If try to turn around itself, make it turn!
            var position = entity.Position;
            if (center == position || radius < entity.Radius)
            {
                var nextDirection = navMeshAgent.angularSpeed * RotationDeltaTime * (clockWise ? -1 : 1);
                var lookDirectionVector2 = entity.transform.forward.ToVector2();
                LookAt(lookDirectionVector2.Rotate(nextDirection));
                return;
            }

            // Compute circumference
            var centerToCurrentPosition = position - center;
            radius ??= centerToCurrentPosition.magnitude;
     
            // Move to the next point on the circle
            var angleToRotate = MathsHelper.GetCircleAngleFromCircumferenceDistance(navMeshAgent.speed * Time.deltaTime, radius.Value);
            
            // Rotate around the center
            var nextPoint = center + centerToCurrentPosition.Rotate(angleToRotate * (clockWise ? 1 : -1));
            MoveTo(nextPoint, stoppingDistance);
        }

        public void TurnAround(AEntity targetEntity, bool clockWise = true)
        {
            if (targetEntity is null)
                return;

            TurnAround(targetEntity.Position, clockWise);
        }
    }
}