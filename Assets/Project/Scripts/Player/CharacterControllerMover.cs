using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Scripts.Entities.Abstracts;
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

        private Vector2 movePositionTarget;
        private Vector3 rotateDirectionTarget;
        private float rotateSmoothVelocity;
        private CancellationTokenSource moveToPositionCancellationTokenSource;


        public CharacterControllerMover(CharacterController controller, ISpeedConfiguration configuration)
        {
            characterController = controller ??
                                  throw new ArgumentNullException(nameof(controller),
                                      "Character controller cannot be null");
            speedConfiguration = configuration ??
                                 throw new ArgumentNullException(nameof(configuration),
                                     "Speed configuration cannot be null");


            LookInDirection(Vector2FromVector3(characterController.transform.forward));
            _ = RefreshRotationProcess();
        }


        private static bool ReachedPosition(Vector2 currentPosition, Vector2 targetPosition) =>
            Vector3.SqrMagnitude(targetPosition - currentPosition) <= DistanceThreshold;
        
        private static Vector2 Vector2FromVector3(Vector3 vector) => new(vector.x, vector.z);
        
        private static Vector3 Vector3FromVector2(Vector2 vector) => new(vector.x, 0, vector.y);
        
        
        private void MoveTo(Vector2 position)
        {
            var newPosition = Vector3.ProjectOnPlane(Vector3FromVector2(position) - characterController.transform.position, Vector3.up);
            characterController.Move(newPosition);
        }


        private async Task RefreshRotationProcess()
        {
            while (true)
            {
                var currentForwardDirection = characterController.transform.forward;
                if (rotateDirectionTarget == Vector3.zero)
                    throw new ApplicationException("Cannot look at zero direction.");

                if (currentForwardDirection != rotateDirectionTarget)
                {
                    var currentRotation = characterController.transform.rotation;
                    var targetRotation = Quaternion.LookRotation(rotateDirectionTarget, Vector3.up);

                    characterController.transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation,
                        speedConfiguration.RotationSpeed * Time.deltaTime);
                }

                await Awaitable.EndOfFrameAsync();
            }
        }


        public void LookInDirection(Vector2 direction)
        {
            direction = direction.normalized;
            if (direction == Vector2.zero)
                return;
            
            rotateDirectionTarget = Vector3FromVector2(direction);
        }

        public void LookAt(AEntity entity)
        {
            if (entity is null || entity.transform == characterController.transform)
                return;

            var entityPositionVector2 = Vector2FromVector3(entity.transform.position);
            var characterPositionVector2 = Vector2FromVector3(characterController.transform.position);
            LookInDirection(entityPositionVector2 - characterPositionVector2);
        }

        public void MoveInDirection(Vector2 direction, float? speed)
        {
            if(direction == Vector2.zero)
                return;
            
            speed ??= speedConfiguration.TranslationSpeed;
            speed = Mathf.Clamp(speed.Value, 0, speedConfiguration.TranslationSpeed);
            
            var position = Vector2FromVector3(characterController.transform.position) + 
                           direction.normalized * speed.Value * Time.deltaTime;
            MoveTo(position);
        }
        
        public void MoveToPosition(Vector2 position)
        {
            var characterPositionVector2 = Vector2FromVector3(characterController.transform.position);
            if (ReachedPosition(characterPositionVector2, position))
                return;
            
            LookInDirection(position - characterPositionVector2);
            
            var remainingDistance = Vector3.Distance(characterPositionVector2, position);
            MoveInDirection(position - characterPositionVector2, remainingDistance);
        }

        public async Task WaitMoveToPosition(Vector2 position)
        {
            if (moveToPositionCancellationTokenSource is not null && position == movePositionTarget)
                return;

            moveToPositionCancellationTokenSource?.Cancel();
            moveToPositionCancellationTokenSource = new CancellationTokenSource();

            movePositionTarget = position;

            while (!ReachedPosition(Vector2FromVector3(characterController.transform.position), movePositionTarget))
            {
                MoveToPosition(movePositionTarget);
                await Awaitable.EndOfFrameAsync(moveToPositionCancellationTokenSource.Token);
            }
        }

        public void Follow(AEntity entity)
        {
            if (entity is null)
                return;

            MoveToPosition(Vector2FromVector3(entity.transform.position));
        }

        public void TurnAroundPoint(Vector2 center, bool clockWise = true, float? radius = null)
        {
            // If try to turn around itself, make it turn!
            var position = Vector2FromVector3(characterController.transform.position);
            if (center == position)
            {
                var nextDirection = speedConfiguration.RotationSpeed * Time.deltaTime * (clockWise ? -1 : 1);
                var lookDirectionVector2 = Vector2FromVector3(characterController.transform.forward);
                LookInDirection(lookDirectionVector2.Rotate(nextDirection));
                return;
            }

            // Compute circumference
            var centerToCurrentPosition = position - center;
            radius ??= centerToCurrentPosition.magnitude;
            var nearestPoint = MathsHelper.GetCircleNearestPoint(center, radius.Value, position);

            // Move to the nearest point on the circle ;
            if (!ReachedPosition(position, nearestPoint))
            {
                MoveToPosition(nearestPoint);
                return;
            }

            // Move to the next point on the circle
            var angleToRotate = MathsHelper.GetCircleAngleFromCircumferenceDistance(
                    speedConfiguration.TranslationSpeed * Time.deltaTime, radius.Value);
            
            // Rotate around the center
            var nextPoint = center + centerToCurrentPosition.Rotate(angleToRotate * (clockWise ? 1 : -1));
            MoveTo(nextPoint);
        }

        public void TurnAround(AEntity entity, bool clockWise = true)
        {
            if (entity is null)
                return;

            TurnAroundPoint(Vector2FromVector3(entity.transform.position), clockWise);
        }
    }
}