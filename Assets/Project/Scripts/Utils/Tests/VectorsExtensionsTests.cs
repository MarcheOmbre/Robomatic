using NUnit.Framework;
using UnityEngine;

namespace Project.Scripts.Utils.Tests
{
    public class VectorsExtensionsTests
    {
        private const float RotationDelta = 0.000001f;
        
        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(1, -1, 0)]
        [TestCase(-1, 0, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, -1)]
        public void Test_XZToXYVector2(float x, float y, float z)
        {
            var vector3 = new Vector3(x, y, z);
            var vector2 = vector3.XZToXYVector2();
            
            Assert.AreEqual(vector3.x, vector2.x);
            Assert.AreEqual(vector3.z, vector2.y);
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        [TestCase(1, 0)]
        [TestCase(1, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, 1)]
        public void Test_XYToXZVector3(float x, float y)
        {
            var vector2 = new Vector2(x, y);
            var vector3 = vector2.XYToXZVector3();
            
            Assert.AreEqual(vector2.x, vector3.x);
            Assert.AreEqual(vector2.y, vector3.z);
        }

        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 5)]
        [TestCase(0, -1, 5)]
        [TestCase(1, 0, -5)]
        [TestCase(1, -1, -5)]
        [TestCase(-1, 0, -35)]
        [TestCase(-1, 1, 35)]
        [TestCase(-1, -1, 70)]
        public void Test_Rotate(float x, float y, float angle)
        {
            var vector2 = new Vector2(x, y);
            var vector2Rotation = vector2.Rotate(angle);

            var vector2ConfirmationRotation = Quaternion.Euler(Vector3.back * angle) * vector2Rotation;
            Assert.AreEqual(vector2.x, vector2ConfirmationRotation.x, RotationDelta);
            Assert.AreEqual(vector2.y, vector2ConfirmationRotation.y, RotationDelta);
        }
    }
}