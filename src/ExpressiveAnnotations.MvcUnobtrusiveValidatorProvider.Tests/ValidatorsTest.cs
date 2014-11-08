using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Tests
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

        public class Model
        {
            public int Value { get; set; }
        }
    }
}
