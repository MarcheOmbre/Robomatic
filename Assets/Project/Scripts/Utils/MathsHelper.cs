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
        
        // Thanks to https://discussions.unity.com/t/whats-the-most-efficient-way-to-rotate-a-vector2-of-a-certain-angle-around-the-axis-orthogonal-to-the-plane-they-describe/98886
        public static Vector2 Rotate(this Vector2 vector, float delta) {
            
            var sin = Mathf.Sin(delta * Mathf.Deg2Rad);
            var cos = Mathf.Cos(delta * Mathf.Deg2Rad);
		
            var tx = vector.x;
            var ty = vector.y;
            vector.x = cos * tx - sin * ty;
            vector.y = sin * tx + cos * ty;
            
            return vector;
        }
    }
}