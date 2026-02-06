using NUnit.Framework;
using UnityEngine;

namespace Project.Scripts.Utils.Tests
{
    public class MathsHelperTests
    {
        #region GetNearestPointOnCircle

        private static void GetCircleNearestPoint(Vector2 center, float radius, Vector3 point)
        {
            if (radius < 0)
            {
                Assert.Catch<System.ArgumentException>(() => MathsHelper.GetCircleNearestPoint(center, radius, point));
                return;
            }
            
            if (radius == 0)
            {
                Assert.AreEqual(center, MathsHelper.GetCircleNearestPoint(center, radius, point));
                return;
            }

            var nearestPoint = MathsHelper.GetCircleNearestPoint(center, radius, point);
            Assert.AreEqual(Mathf.Approximately((nearestPoint - center).magnitude, radius), true);
        }

        [Test]
        public void GetCircleNearestPoint_RadiusNegative() => GetCircleNearestPoint(Vector2.zero, -1, Vector2.zero);

        [Test]
        public void GetCircleNearestPoint_RadiusZero() => GetCircleNearestPoint(Vector2.zero, 0, Vector2.zero);

        [Test]
        public void GetCircleNearestPoint_CenterEqualPoint() => GetCircleNearestPoint(Vector2.zero, 1, Vector2.zero);

        [Test]
        public void GetCircleNearestPoint_UnitVectors()
        {
            GetCircleNearestPoint(Vector2.zero, 1, Vector2.right);
            GetCircleNearestPoint(Vector2.zero, 1, Vector2.up);
        }

        [Test]
        public void GetCircleNearestPoint_ValidCases()
        {
            GetCircleNearestPoint(Vector2.zero, 1, new Vector2(0.5f, 0.5f));

            // With not-unit radius
            GetCircleNearestPoint(Vector2.zero, 2, new Vector2(0.5f, 0.5f));
            GetCircleNearestPoint(Vector2.zero, 2, new Vector2(3f, 0.5f));
            GetCircleNearestPoint(Vector2.zero, 2, new Vector2(18f, 0.5f));
            GetCircleNearestPoint(Vector2.zero, 2, new Vector2(0.5f, 18f));

            // With center that is not on zero
            GetCircleNearestPoint(new Vector2(3f, 1f), 1, new Vector2(0.5f, 0.5f));
            GetCircleNearestPoint(new Vector2(1f, 4f), 1, new Vector2(3f, 0.5f));
            GetCircleNearestPoint(new Vector2(6f, 1f), 1, new Vector2(18f, 0.5f));
        }

        #endregion

        #region GetCircleAngleFromCircumferenceDistance

        [Test]
        public void GetCircleAngleFromCircumferenceDistance_RadiusNegative()
        {
            Assert.Catch<System.ArgumentException>(() => MathsHelper.GetCircleAngleFromCircumferenceDistance(1, -1));
        }

        [Test]
        public void GetCircleAngleFromCircumferenceDistance_RadiusZero()
        {
            Assert.Catch<System.ArgumentException>(() => MathsHelper.GetCircleAngleFromCircumferenceDistance(1, 0));
        }

        [Test]
        public void GetCircleAngleFromCircumferenceDistance_DistanceZero()
        {
            Assert.AreEqual(0, MathsHelper.GetCircleAngleFromCircumferenceDistance(0, 1));
        }

        #endregion
    }
}