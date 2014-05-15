using ExpressiveAnnotations.ConditionalAttributes;
using ExpressiveAnnotations.Tests.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ExpressiveAnnotations.Tests
{
    /// <summary>
    /// Tests basic stuff, just for the peace of mind. More detailed tests are done by expressions 
    /// testing and indirectly by RequiredIfAttribute tests (which uses exact common logic inside).
    /// </summary>
    [TestClass]
    public class RequiredIExpressionfAttributeTest
    {
        private enum Stability
        {
            High,
            Low,
            Uncertain,
        }

        private class Model
        {            
            public bool GoAbroad { get; set; }

            public string Country { get; set; }
            public string NextCountry { get; set; }

            [RequiredIfExpression(
                Expression = "{0} && !{1} && {2}",
                DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
                TargetValues = new object[] {true, "Other", "[Country]"})]
            public string ReasonForTravel { get; set; }

            public Stability? PoliticalStability { get; set; }

            [RequiredIfExpression(
                Expression = "!{0}",
                DependentProperties = new[] {"PoliticalStability"},
                TargetValues = new object[] {Stability.High})]
            public bool AwareOfTheRisks { get; set; }
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

            model = new Model { AwareOfTheRisks = true, PoliticalStability = Stability.Uncertain };
            Assert.IsTrue(model.IsValid(m => m.AwareOfTheRisks));

            model = new Model { AwareOfTheRisks = false, PoliticalStability = Stability.High };
            Assert.IsTrue(model.IsValid(m => m.AwareOfTheRisks));
        }

        [TestMethod]
        public void Verify_if_required()
        {
            var model = new Model {GoAbroad = true, Country = "Poland", NextCountry = "Poland"};
            Assert.IsFalse(model.IsValid(m => m.ReasonForTravel));

            model = new Model { AwareOfTheRisks = false, PoliticalStability = Stability.Uncertain };
            Assert.IsFalse(model.IsValid(m => m.AwareOfTheRisks));
        }
    }
}
