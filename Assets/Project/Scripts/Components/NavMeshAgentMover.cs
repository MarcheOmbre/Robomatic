using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Scripts.Components.Interfaces;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters.Exceptions;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.Components
{
    public class NavMeshAgentMover : IMover
    {
        private const int MoveFramesDurationCount = 5;
        private const int FindPointDistanceOffset = 10;
        private const int CircleMaxDistanceAway = 1;
        
        
        private float RotationDeltaTime
        {
            get
            {
                var currentTime = Time.time;
                var deltaTime = currentTime - lastRotationTime;
                return Mathf.Min(deltaTime, Time.deltaTime);
            }
        }
        
        
        private readonly NavMeshAgent navMeshAgent;
        private readonly Collider collider;
        private readonly float stoppingDistance;
        
        private readonly AEntity entity;
        private float lastRotationTime;
        private Task currentMoveTask;
        private CancellationTokenSource cancellationTokenSource;


        public NavMeshAgentMover(AEntity entity, NavMeshAgent navMeshAgent, ISpeedConfiguration configuration)
        {
            this.entity = entity ?? throw new ArgumentNullException(nameof(entity), "Agent entity cannot be null");
            this.navMeshAgent = navMeshAgent ?? throw new ArgumentNullException(nameof(navMeshAgent), "NavMeshAgent cannot be null");
            stoppingDistance = navMeshAgent.stoppingDistance;
            
            this.navMeshAgent.speed = configuration.TranslationSpeed;
            this.navMeshAgent.angularSpeed = configuration.RotationSpeed;
            this.navMeshAgent.destination = entity.Position.XYToXZVector3();
        }
        
        // ReSharper disable once AsyncVoidMethod
        private async Task MoveTo(Vector2 targetPosition, float stopDistance)
        {
            if (cancellationTokenSource is not null)
                throw new InvalidOperationException("Move is already running.");
            
            cancellationTokenSource = new CancellationTokenSource();
            
            // Compute the distance
            var distance = Vector2.Distance(navMeshAgent.destination.XZToXYVector2(), targetPosition);
            if (distance > 0)
            {
                navMeshAgent.stoppingDistance = stopDistance;
                
                // Get the nearest point on the navmesh
                NavMesh.SamplePosition(targetPosition.XYToXZVector3(), out var nearestPoint, distance + FindPointDistanceOffset, NavMesh.AllAreas);
                if(!navMeshAgent.SetDestination(nearestPoint.position))
                    throw new UnaccessiblePathException(nameof(MoveTo));
            }

            // Move
            for (var i = 0; i < MoveFramesDurationCount; i++)
            {
                await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
                
                if(cancellationTokenSource.IsCancellationRequested)
                    break;
            }
            
            Stop();
            
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
        
        private void Stop() => navMeshAgent.destination = entity.transform.position;

        private void RequestMove(Vector2 targetPosition, float stopDistance)
        {
            if(currentMoveTask is { IsCompleted: false })
                return;
            
            currentMoveTask?.Dispose();
            currentMoveTask = MoveTo(targetPosition, stopDistance);
        }
        
        

        public bool LookAt(Vector2 targetDirection)
        {
            var rotateDirectionTarget = targetDirection.normalized.XYToXZVector3();
            if (rotateDirectionTarget == Vector3.zero)
                throw new VectorZeroException(nameof(LookAt));

            var currentRotation = navMeshAgent.transform.rotation;
            var targetRotation = Quaternion.LookRotation(rotateDirectionTarget, Vector3.up);
            var lastRotation = navMeshAgent.transform.rotation;
            
            navMeshAgent.updateRotation = false;
            navMeshAgent.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, navMeshAgent.angularSpeed * RotationDeltaTime);
            lastRotationTime = Time.time;
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

            var direction = targetDirection.normalized.XYToXZVector3();
            var position = (entity.transform.position + direction * navMeshAgent.speed).XZToXYVector2();
            
            RequestMove(position, stoppingDistance);
        }
        
        public bool Reach(Vector2 targetPosition)
        {
            RequestMove(targetPosition, stoppingDistance);
            
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
            RequestMove(targetEntity.Position, limitDistance);
            
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
                var lookDirectionVector2 = entity.transform.forward.XZToXYVector2();
                LookAt(lookDirectionVector2.Rotate(nextDirection));
                return;
            }

            // Compute circumference
            var centerToCurrentPosition = position - center;
            radius ??= centerToCurrentPosition.magnitude;
            var nearestPoint = MathsHelper.GetCircleNearestPoint(center, radius.Value, position);
            
            var normalizedDistanceFromNearestPoint = Mathf.Clamp01(Vector2.Distance(nearestPoint, position) / CircleMaxDistanceAway);

            // Move to the next point on the circle
            var angleToRotate = MathsHelper.GetCircleAngleFromCircumferenceDistance(navMeshAgent.speed, radius.Value);
            var rotationPoint = center + centerToCurrentPosition.Rotate(angleToRotate * (clockWise ? 1 : -1));

            RequestMove(Vector2.Lerp(nearestPoint, rotationPoint, 1 - normalizedDistanceFromNearestPoint), stoppingDistance);
        }

        public void TurnAround(AEntity targetEntity, bool clockWise = true, float? radius = null)
        {
            if (targetEntity is null)
                return;

            TurnAround(targetEntity.Position, clockWise, radius);
        }
    }
}