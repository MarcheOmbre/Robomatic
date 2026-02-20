using System;
using NUnit.Framework;

namespace Project.Scripts.Utils.Tests
{
    public class EnumExtensionsTests
    {
        [Flags]
        private enum TestEnum
        {
            None = 0,
            
            Value1 = 1 << 0,
            Value2 = 1 << 1,
            Value3 = 1 << 2,
            Value4 = 1 << 3,
            
            All = ~None
        }
        
        [Test]
        public void HasFlag_NoneFlag() => Assert.IsFalse(TestEnum.None.HasMultipleFlags());
        
        [Test]
        [TestCase(TestEnum.Value1)]
        [TestCase(TestEnum.Value2)]
        [TestCase(TestEnum.Value3)]
        [TestCase(TestEnum.Value4)]
        public void HasFlag_OneFlag(Enum value) => Assert.IsFalse(value.HasMultipleFlags());
        
        [Test]
        public void HasFlag_AllFlags() => Assert.IsTrue(TestEnum.All.HasMultipleFlags());
    }
}