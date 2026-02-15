using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class VectorsExtensions
    {
        public static Vector2 XZToXYVector2(this Vector3 vector) => new(vector.x, vector.z);
        
        public static Vector3 XYToXZVector3(this Vector2 vector) => new(vector.x, 0, vector.y);
        
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