using System.Linq;
using System.Reflection;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class AttribTest
    {
        private class Model
        {
            [RequiredIf("1 > 0")]
            [RequiredIf("1 > 0")]
            [AssertThat("1 > 0")]
            [AssertThat("1 > 0")]
            public int Value1 { get; set; }

            [RequiredIf("1 > 0")]
            [RequiredIf("2 > 0")]
            [AssertThat("3 > 0")]
            [AssertThat("4 > 0")]
            public int Value2 { get; set; }
        }

        [TestMethod]
        public void verify_attributes_uniqueness()
        {
            var attributes = typeof(Model).GetProperty("Value1")
                .GetCustomAttributes()
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.AreEqual(2, attributes.Count); // ignores redundant attributes of the same type id, because they do nothing new (exact type name, exact expression)
            Assert.IsTrue(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));

            attributes = typeof(Model).GetProperty("Value2")
                .GetCustomAttributes()
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.AreEqual(4, attributes.Count); // all type ids are unique (despite the same type names of some attributes, they contain different expressions)
            Assert.IsTrue(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));
        }
    }
}
