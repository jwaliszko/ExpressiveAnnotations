using ExpressiveAnnotations.ConditionalAttributes;
using ExpressiveAnnotations.Tests.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    /// <summary>
    /// Tests basic stuff, just for the peace of mind. More detailed tests are done by expressions 
    /// testing and indirectly by RequiredIfAttribute tests (which uses exact common logic inside).
    /// </summary>
    [TestClass]    
    public class RequiredIExpressionfAttributeTest
    {
        private class Model
        {
            public bool GoAbroad { get; set; }

            public string Country { get; set; }

            public string NextCountry { get; set; }

            [RequiredIfExpression(
                /* interpretation => GoAbroad == true && NextCountry != "Other" && NextCountry == [value from Country] */
                Expression = "{0} && !{1} && {2}",
                DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
                TargetValues = new object[] {true, "Other", "[Country]"})]
            public string ReasonForTravel { get; set; }
        }

        [TestMethod]
        public void Verify_if_not_required()
        {
            var model = new Model {GoAbroad = true, Country = "Poland", NextCountry = "France"};
            Assert.IsTrue(model.IsValid(m => m.ReasonForTravel));

            model = new Model {GoAbroad = true, Country = "Other", NextCountry = "Other"};
            Assert.IsTrue(model.IsValid(m => m.ReasonForTravel));

            model = new Model {GoAbroad = false, Country = "Poland", NextCountry = "Poland"};
            Assert.IsTrue(model.IsValid(m => m.ReasonForTravel));
        }

        [TestMethod]
        public void Verify_if_required()
        {
            var model = new Model { GoAbroad = true, Country = "Poland", NextCountry = "   poland   " };
            Assert.IsFalse(model.IsValid(m => m.ReasonForTravel));
        }
    }
}
