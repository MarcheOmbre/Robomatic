using NUnit.Framework;
using UnityEngine;

namespace Project.Scripts.Utils.Tests
{
    public class GameHelperTests
    {
        [Test]
        public void IsInRange_SamePositionRadiusZero()
        {
            var position = Vector2.zero;
            Assert.IsTrue(GameHelper.IsInRange(position, 0, position, 0));
        }
        
        [Test]
        public void IsInRange_OnRadiusLine()
        {
            var position = Vector2.zero;
            Assert.IsTrue(GameHelper.IsInRange(position, 0.5f, position + Vector2.right, 0.5f));
        }
        
        [Test]
        public void IsInRange_OffRadiusLine()
        {
            var position = Vector2.zero;
            Assert.IsFalse(GameHelper.IsInRange(position, 1f, position + Vector2.right * 3, 1f));
        }
    }
}