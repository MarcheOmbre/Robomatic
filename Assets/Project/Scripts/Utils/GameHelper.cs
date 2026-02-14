using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class GameHelper
    {
        public static bool IsInRange(Vector2 position1, float radius1, Vector2 position2, float radius2) =>
            Vector2.Distance(position1, position2) <= radius1 + radius2;
    }
}