using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Scripts.Utils
{
    public static class UIExtensions
    {
        public static GameObject[] GetUIUnderPosition(Vector2 position)
        {
            var pointerData = new PointerEventData (EventSystem.current)
            {
                pointerId = -1,
                position = position
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
		
            return results.ConvertAll(result => result.gameObject).ToArray();
        }
    }
}