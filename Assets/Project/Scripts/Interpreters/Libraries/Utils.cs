using JetBrains.Annotations;
using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Interpreters.Libraries
{
    public static class Utils
    {
        [UsedImplicitly]
        [AuthorizedHelper.AuthorizedMethod]
        public static bool IsInRange(Vector2 position1, float radius1, Vector2 position2, float radius2) =>
            GameHelper.IsInRange(position1, radius1, position2, radius2);
    }
}