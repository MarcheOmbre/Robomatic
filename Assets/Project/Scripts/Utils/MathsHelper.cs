using System;
using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class MathsHelper
    {
        public static Vector2 GetCircleNearestPoint(Vector2 center, float radius, Vector2 point)
        {
            // Negative radius is not allowed
            if(radius < 0)
                throw new ArgumentException("Radius must be positive");
            
            // No radius means center point
            if(radius == 0)
                return center;
            
            // If the center equals the point, return the top of the circle
            if (center == point)
            {
                center.y += radius;
                return center;
            }
            
            var normalizedCenterToPointVector = (point - center).normalized;
            return center + normalizedCenterToPointVector * radius;
        }
        
        public static float GetCircleAngleFromCircumferenceDistance(float distance, float radius)
        {
            if(radius <= 0)
                throw new ArgumentException("Radius must be positive and non-zero");
            
            if (distance == 0)
                return 0;
            
            var circumference = 2 * Mathf.PI * radius;
            return distance / circumference * 360f;
        }
        
        
    }
}