using System;
using Project.Scripts.Components.Interfaces;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters.Exceptions;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Components
{
    public class CharacterControllerMover : IMover
    {
        private const float DistanceThreshold = 0.1f;
        private const int CircleMaxDistanceAway = 1;
        

        private readonly AEntity entity;
        private readonly CharacterController characterController;
        private readonly ISpeedConfiguration speedConfiguration;


        private float MoveDeltaTime
        {
            get
            {
                var currentTime = Time.time;
                var deltaTime = currentTime - moveLastTime;
                return Mathf.Min(deltaTime, Time.deltaTime);
            }
        }

        private float RotationDeltaTime
        {
            get
            {
                var currentTime = Time.time;
                var deltaTime = currentTime - lastRotationTime;
                return Mathf.Min(deltaTime, Time.deltaTime);
            }
        }
        
        private float moveLastTime;
        private float lastRotationTime;


        public CharacterControllerMover(AEntity entity, CharacterController controller, ISpeedConfiguration configuration)
        {
            this.entity = entity ?? throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            characterController = controller ?? throw new ArgumentNullException(nameof(controller), "Character controller cannot be null");
            speedConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Speed configuration cannot be null");
        }


        private static bool ReachedPosition(Vector2 currentPosition, Vector2 targetPosition) =>
            Vector3.SqrMagnitude(targetPosition - currentPosition) <= DistanceThreshold;
        

        
        
        private void Move(Vector2 targetPosition)
        {
            var newPosition = targetPosition - entity.Position;
            characterController.Move(newPosition);
        }
        

        public bool LookAt(Vector2 targetDirection)
        {
            var rotateDirectionTarget = targetDirection.normalized.XYToXZVector3();
            if (rotateDirectionTarget == Vector3.zero)
                throw new VectorZeroException(nameof(LookAt));

            var currentRotation = characterController.transform.rotation;
            var targetRotation = Quaternion.LookRotation(rotateDirectionTarget, Vector3.up);

            var lastRotation = characterController.transform.rotation;
            
            characterController.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation,
                speedConfiguration.RotationSpeed * RotationDeltaTime);
            
            return lastRotation == characterController.transform.rotation;
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
            
            var position = entity.Position + targetDirection.normalized * speedConfiguration.TranslationSpeed * MoveDeltaTime;
            Move(position);
        }
        
        public bool Reach(Vector2 targetPosition)
        {
            var characterPositionVector2 = entity.Position;

            if (!LookAt(targetPosition - characterPositionVector2))
                return false;
            
            MoveToward(targetPosition - characterPositionVector2);
            
            return ReachedPosition(characterPositionVector2, targetPosition);
        }

        public bool Reach(AEntity targetEntity)
        {
            if (targetEntity is null)
                throw new NullEntityException(nameof(Reach));

            var reachedPosition = Reach(targetEntity.Position);
            var isColliding = GameHelper.IsInRange(entity.Position, characterController.radius, targetEntity.Position, targetEntity.Radius);
            return reachedPosition || isColliding;
        }

        public void TurnAround(Vector2 center, bool clockWise = true, float? radius = null)
        {
            // If try to turn around itself, make it turn!
            var position = entity.Position;
            if (center == position || radius < entity.Radius)
            {
                var nextDirection = speedConfiguration.RotationSpeed * RotationDeltaTime * (clockWise ? -1 : 1);
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
            var angleToRotate = MathsHelper.GetCircleAngleFromCircumferenceDistance(speedConfiguration.TranslationSpeed, radius.Value);
            var rotationPoint = center + centerToCurrentPosition.Rotate(angleToRotate * (clockWise ? 1 : -1));

            Move(Vector2.Lerp(nearestPoint, rotationPoint, 1 - normalizedDistanceFromNearestPoint));
        }

        public void TurnAround(AEntity targetEntity, bool clockWise = true, float? radius = null)
        {
            if (targetEntity is null)
                return;

            TurnAround(targetEntity.Position, clockWise, radius);
        }
    }
}