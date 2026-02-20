using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Scripts.Utils.Tests
{
    public class MonoBehaviourSingletonTests
    {
        private class TestClass : MonoBehaviourSingleton<TestClass>
        {
            // Make it public to test the method without reflection
            public new void Awake() => base.Awake();
        }
        
        private GameObject gameObject;
        
        
        [SetUp]
        public void SetUp() => gameObject = new GameObject("Test");
        
        [TearDown]
        public void TearDown() => Object.DestroyImmediate(gameObject);
        

        [Test]
        public void SingletonCreation_SingleInstance()
        {
            var component = gameObject.AddComponent<TestClass>();
            
            component.Awake();
            
            Assert.IsNotNull(TestClass.Instance);
            Assert.AreSame(component, TestClass.Instance);
        }
        
        [Test]
        public void SingletonCreation_MultipleInstances()
        {
            gameObject.AddComponent<TestClass>().Awake();
            
            
            var secondComponent = gameObject.AddComponent<TestClass>();
            Assert.Catch<InvalidOperationException>(secondComponent.Awake);
        }
    }
}