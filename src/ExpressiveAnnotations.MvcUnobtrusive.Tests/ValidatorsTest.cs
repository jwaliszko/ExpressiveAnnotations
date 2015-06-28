using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExpressiveAnnotations.MvcUnobtrusive.Tests
{
    [TestClass]
    public class ValidatorsTest
    {
        [TestInitialize]
        public void Setup()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest(string.Empty, "http://tempuri.org", string.Empty),
                new HttpResponse(new StringWriter())
                );
        }

        [TestMethod]
        public void verify_client_validation_rules()
        {
            var model = new Model();
            var assertAttributes = Enumerable.Range(0, 28).Select(x => new AssertThatAttribute(string.Format("Value > {0}", x))).ToArray();
            var requirAttributes = Enumerable.Range(0, 28).Select(x => new RequiredIfAttribute(string.Format("Value > {0}", x))).ToArray();

            var metadata = GetModelMetadata(model);
            var controllerContext = GetControllerContext();

            try
            {
                for (var i = 0; i < assertAttributes.Length; i++)
                {
                    var attribute = assertAttributes[i];
                    var validator = new AssertThatValidator(metadata, controllerContext, attribute);
                    var rule = validator.GetClientValidationRules().Single();
                    var suffix = i == 0 ? string.Empty : char.ConvertFromUtf32(96 + i);
                    Assert.AreEqual(string.Format("assertthat{0}", suffix), rule.ValidationType);
                }
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.Message);
            }

            try
            {
                for (var i = 0; i < requirAttributes.Length; i++)
                {
                    var attribute = requirAttributes[i];
                    var validator = new RequiredIfValidator(metadata, controllerContext, attribute);
                    var rule = validator.GetClientValidationRules().Single();
                    var suffix = i == 0 ? string.Empty : char.ConvertFromUtf32(96 + i);
                    Assert.AreEqual(string.Format("requiredif{0}", suffix), rule.ValidationType);
                } 
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.Message);
            }
        }

        [TestMethod]
        public void client_validation_rules_are_json_formatting_insensitive()
        {
            var settings = JsonConvert.DefaultSettings;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var model = new Model {Value = 1, Status = State.High};
            var metadata = GetModelMetadata(model);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value > 0 && Status == ValidatorsTest.State.High"));
            var assertRule = assert.GetClientValidationRules().Single();
            
            Assert.AreEqual("{\"Value\":\"numeric\",\"Status\":\"numeric\"}", (string)assertRule.ValidationParameters["fieldsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"ValidatorsTest.State.High\":0}", (string)assertRule.ValidationParameters["constsmap"], false, CultureInfo.InvariantCulture);
            // expression is not a json actually, but tested for the sake of clarity
            Assert.AreEqual("Value > 0 && Status == ValidatorsTest.State.High", (string)assertRule.ValidationParameters["expression"], false, CultureInfo.InvariantCulture);

            var requir = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value > 0 && Status == ValidatorsTest.State.High"));
            var requirRule = requir.GetClientValidationRules().Single();
            
            Assert.AreEqual("{\"Value\":\"numeric\",\"Status\":\"numeric\"}", (string)requirRule.ValidationParameters["fieldsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"ValidatorsTest.State.High\":0}", (string)requirRule.ValidationParameters["constsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("false", (string)requirRule.ValidationParameters["allowempty"], false, CultureInfo.InvariantCulture);
            // expression is not a json actually, but tested for the sake of clarity
            Assert.AreEqual("Value > 0 && Status == ValidatorsTest.State.High", (string)requirRule.ValidationParameters["expression"], false, CultureInfo.InvariantCulture);

            JsonConvert.DefaultSettings = settings; // reset settings to original state
        }

        [TestMethod]
        public void possible_naming_colission_at_client_side_are_detected()
        {
            // A.B.C = 0    {"A":{"B":{"C":0}}}
            // A.D = true   {"A":{"D":true}}
            // can be merged into: {"A":{"B":{"C":0},"D":true}}

            // A.B.C = 0    {"A":{"B":{"C":0}}}
            // A.B = true   {"A":{"B":true}}
            // cannot be merged at 1st level - B object would be overwritten

            string name;
            int level;
            Assert.IsFalse(Helper.SegmentsCollide(new string[0], new string[0], out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new[] {"A"}, new string[0], out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new string[0], new[] {"A"}, out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new[] {"A"}, new[] {"B"}, out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new[] {"A.A"}, new[] {"A.B"}, out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new[] {"A.B.C"}, new[] {"A.B.D"}, out name, out level));
            Assert.IsFalse(Helper.SegmentsCollide(new[] {"A.B.C", "A.B.E"}, new[] {"B.B", "B.C", "B.E"}, out name, out level));

            Assert.AreEqual(null, name);
            Assert.AreEqual(level, -1);

            Assert.IsTrue(Helper.SegmentsCollide(new[] {"A"}, new[] {"A"}, out name, out level));
            Assert.AreEqual("A", name);
            Assert.AreEqual(level, 0);

            Assert.IsTrue(Helper.SegmentsCollide(new[] {"A.B"}, new[] {"A.B"}, out name, out level));
            Assert.AreEqual("B", name);
            Assert.AreEqual(level, 1);

            Assert.IsTrue(Helper.SegmentsCollide(new[] {"A.B.C"}, new[] {"A.B"}, out name, out level));
            Assert.AreEqual("B", name);
            Assert.AreEqual(level, 1);
        }

        [TestMethod]
        public void verify_validators_caching()
        {
            const int testLoops = 10;
            var model = new Model();
            var metadata = GetModelMetadata(model);
            var controllerContext = GetControllerContext();
            
            var nonCached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("true")));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("true")));
                Assert.IsTrue(nonCached > cached);
            }

            nonCached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("true")));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("true")));
                Assert.IsTrue(nonCached > cached);
            }
        }

        private long MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedTicks;
        }

        private ModelMetadata GetModelMetadata(Model model)
        {
            var metadata = new ModelMetadata(ModelMetadataProviders.Current, model.GetType(), () => model, model.Value.GetType(), "Value");
            return metadata;
        }

        private ControllerContext GetControllerContext()
        {
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.HttpMethod).Returns("GET");
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(c => c.Request).Returns(request.Object);
            var controllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), new Mock<ControllerBase>().Object);
            return controllerContext;
        }

        public enum State
        {
            High,
            Low
        }

        public class Model
        {
            public int Value { get; set; }
            public State Status { get; set; }
        }
    }
}
