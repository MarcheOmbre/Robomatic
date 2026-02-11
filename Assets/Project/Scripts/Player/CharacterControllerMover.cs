using System;
using Project.Scripts.Entities.Abstracts;
using Project.Scripts.Interpreters.Exceptions;
using Project.Scripts.Services.Components;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Player
{
    public class CharacterControllerMover : IMover
    {
        private const float DistanceThreshold = 1f;

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


        public CharacterControllerMover(CharacterController controller, ISpeedConfiguration configuration)
        {
            characterController = controller ??
                                  throw new ArgumentNullException(nameof(controller),
                                      "Character controller cannot be null");
            speedConfiguration = configuration ??
                                 throw new ArgumentNullException(nameof(configuration),
                                     "Speed configuration cannot be null");
        }


        private static bool ReachedPosition(Vector2 currentPosition, Vector2 targetPosition) =>
            Vector3.SqrMagnitude(targetPosition - currentPosition) <= DistanceThreshold;
        
        private static Vector2 Vector2FromVector3(Vector3 vector) => new(vector.x, vector.z);
        
        private static Vector3 Vector3FromVector2(Vector2 vector) => new(vector.x, 0, vector.y);
        
        
        private void Move(Vector2 position)
        {
            var newPosition = Vector3.ProjectOnPlane(Vector3FromVector2(position) - characterController.transform.position, Vector3.up);
            characterController.Move(newPosition);
        }
        

        public bool LookAt(Vector2 direction)
        {
            var rotateDirectionTarget = Vector3FromVector2(direction.normalized);
            if (rotateDirectionTarget == Vector3.zero)
                throw new VectorZeroException(nameof(LookAt));

            var currentRotation = characterController.transform.rotation;
            var targetRotation = Quaternion.LookRotation(rotateDirectionTarget, Vector3.up);

            var lastRotation = characterController.transform.rotation;
            
            characterController.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation,
                speedConfiguration.RotationSpeed * RotationDeltaTime);
            
            return lastRotation == characterController.transform.rotation;
        }

        public bool LookAt(AEntity entity)
        {
            if (entity is null)
                throw new NullEntityException(nameof(LookAt));

            var entityPositionVector2 = Vector2FromVector3(entity.transform.position);
            var characterPositionVector2 = Vector2FromVector3(characterController.transform.position);
            
            return LookAt(entityPositionVector2 - characterPositionVector2);
        }

        public void MoveToward(Vector2 direction, float? speed)
        {
            if(direction == Vector2.zero)
                return;
            
            speed ??= speedConfiguration.TranslationSpeed;
            speed = Mathf.Clamp(speed.Value, 0, speedConfiguration.TranslationSpeed);
            
            var position = Vector2FromVector3(characterController.transform.position) + 
                           direction.normalized * speed.Value * MoveDeltaTime;
            Move(position);
        }
        
        public bool Reach(Vector2 position)
        {
            var characterPositionVector2 = Vector2FromVector3(characterController.transform.position);

            if (!LookAt(position - characterPositionVector2))
                return false;
            
            var remainingDistance = Vector3.Distance(characterPositionVector2, position);
            MoveToward(position - characterPositionVector2, remainingDistance);
            
            return ReachedPosition(characterPositionVector2, position);
        }

        public bool Reach(AEntity entity)
        {
            if (entity is null)
                throw new NullEntityException(nameof(Reach));

            var reachedPosition = Reach(Vector2FromVector3(entity.transform.position));
            return reachedPosition || characterController.bounds.Intersects(entity.MainCollider.bounds);
        }

        public void TurnAround(Vector2 center, bool clockWise = true, float? radius = null)
        {
            // If try to turn around itself, make it turn!
            var position = Vector2FromVector3(characterController.transform.position);
            if (center == position)
            {
                var nextDirection = speedConfiguration.RotationSpeed * RotationDeltaTime * (clockWise ? -1 : 1);
                var lookDirectionVector2 = Vector2FromVector3(characterController.transform.forward);
                LookAt(lookDirectionVector2.Rotate(nextDirection));
                return;
            }

            // Compute circumference
            var centerToCurrentPosition = position - center;
            radius ??= centerToCurrentPosition.magnitude;
            var nearestPoint = MathsHelper.GetCircleNearestPoint(center, radius.Value, position);

            // Move to the nearest point on the circle ;
            if (!ReachedPosition(position, nearestPoint))
            {
                Reach(nearestPoint);
                return;
            }

            // Move to the next point on the circle
            var angleToRotate = MathsHelper.GetCircleAngleFromCircumferenceDistance(
                    speedConfiguration.TranslationSpeed * MoveDeltaTime, radius.Value);
            
            // Rotate around the center
            var nextPoint = center + centerToCurrentPosition.Rotate(angleToRotate * (clockWise ? 1 : -1));
            Move(nextPoint);
        }

        public void TurnAround(AEntity entity, bool clockWise = true)
        {
            if (entity is null)
                return;

            TurnAround(Vector2FromVector3(entity.transform.position), clockWise);
        }
    }
}