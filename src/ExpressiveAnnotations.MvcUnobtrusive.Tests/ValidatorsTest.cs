using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExpressiveAnnotations.MvcUnobtrusive.Tests
{
    [TestClass]
    public class ValidatorsTest : BaseTest
    {
        [TestMethod]
        public void verify_client_validation_rules_collecting()
        {
            var model = new Model();
            var assertAttributes = Enumerable.Range(0, 28).Select(x => new AssertThatAttribute(string.Format("Value > {0}", x))).ToArray();
            var requirAttributes = Enumerable.Range(0, 28).Select(x => new RequiredIfAttribute(string.Format("Value > {0}", x))).ToArray();

            var metadata = GetModelMetadata(model, m => m.Value);
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
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "AssertThatValidator: collecting of client validation rules for Value field failed.",
                    e.Message);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.InnerException.Message);
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
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "RequiredIfValidator: collecting of client validation rules for Value field failed.",
                    e.Message);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.InnerException.Message);
            }
        }

        [TestMethod]
        public void verify_parsing_error_catched_by_validator()
        {
            var model = new Model();

            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            try
            {
                new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value > #"));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "AssertThatValidator: validation applied to Value field failed.",
                    e.Message);
                Assert.IsNotNull(e.InnerException);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                    e.InnerException.Message);
            }

            try
            {
                new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value > #"));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "RequiredIfValidator: validation applied to Value field failed.",
                    e.Message);
                Assert.IsNotNull(e.InnerException);
                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                    e.InnerException.Message);
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

            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Array);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value > 0 && Status == ValidatorsTest.State.High"));
            var assertRule = assert.GetClientValidationRules().Single();
            
            Assert.AreEqual("{\"Value\":\"numeric\",\"Status\":\"numeric\"}", (string)assertRule.ValidationParameters["fieldsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"ValidatorsTest.State.High\":0}", (string)assertRule.ValidationParameters["constsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"Array\":\"arrayparser\"}", (string)assertRule.ValidationParameters["parsersmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("\"Value > 0 && Status == ValidatorsTest.State.High\"", (string)assertRule.ValidationParameters["expression"], false, CultureInfo.InvariantCulture);            

            var requir = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value > 0 && Status == ValidatorsTest.State.High"));
            var requirRule = requir.GetClientValidationRules().Single();
            
            Assert.AreEqual("{\"Value\":\"numeric\",\"Status\":\"numeric\"}", (string)requirRule.ValidationParameters["fieldsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"ValidatorsTest.State.High\":0}", (string)requirRule.ValidationParameters["constsmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("{\"Array\":\"arrayparser\"}", (string)assertRule.ValidationParameters["parsersmap"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("false", (string)requirRule.ValidationParameters["allowempty"], false, CultureInfo.InvariantCulture);
            Assert.AreEqual("\"Value > 0 && Status == ValidatorsTest.State.High\"", (string)requirRule.ValidationParameters["expression"], false, CultureInfo.InvariantCulture);

            JsonConvert.DefaultSettings = settings; // reset settings to original state
        }

        [TestMethod]
        public void empty_client_validation_rules_are_not_created()
        {
            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("1 > 2"));
            var assertRule = assert.GetClientValidationRules().Single();

            Assert.IsFalse(assertRule.ValidationParameters.ContainsKey("fieldsmap"));
            Assert.IsFalse(assertRule.ValidationParameters.ContainsKey("constsmap"));
            Assert.IsFalse(assertRule.ValidationParameters.ContainsKey("parsersmap"));
            Assert.IsFalse(assertRule.ValidationParameters.ContainsKey("errfieldsmap"));

            Assert.IsTrue(assertRule.ValidationParameters.ContainsKey("expression"));

            var requir = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("1 > 2"));
            var requirRule = requir.GetClientValidationRules().Single();

            Assert.IsFalse(requirRule.ValidationParameters.ContainsKey("fieldsmap"));
            Assert.IsFalse(requirRule.ValidationParameters.ContainsKey("constsmap"));
            Assert.IsFalse(requirRule.ValidationParameters.ContainsKey("parsersmap"));
            Assert.IsFalse(assertRule.ValidationParameters.ContainsKey("errfieldsmap"));

            Assert.IsTrue(requirRule.ValidationParameters.ContainsKey("allowempty"));
            Assert.IsTrue(requirRule.ValidationParameters.ContainsKey("expression"));
        }

        [TestMethod]
        public void verify_formatted_message_sent_to_client()
        {
            var model = new MsgModel();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("1 > 2")
            {
                ErrorMessage = "_{0}{1}{Value:n}{Value:N}{Value}{Value}_{{Value}}{{{Value}}}{{{{Value}}}}_"
            });
            var assertRule = assert.GetClientValidationRules().Single();

            var map = JsonConvert.DeserializeObject<dynamic>((string) assertRule.ValidationParameters["errfieldsmap"]);
            var expected = "_Value1 > 2_{Value}__{Value}_" + map.Value + map.Value + "_{Value}" + "{" + map.Value + "}" + "{{Value}}_";
            Assert.AreEqual(expected, assertRule.ErrorMessage);
        }

        [TestMethod]
        public void verify_that_culture_change_affects_message_sent_to_client()
        {
            var model = new MsgModel();
            var metadata = GetModelMetadata(model, m => m.Lang);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext,
                new AssertThatAttribute("1 > 2") {ErrorMessage = "{Lang:n}"});
            var assertRule = assert.GetClientValidationRules().Single();
            Assert.AreEqual("default", assertRule.ErrorMessage);

            // change culture
            var culture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl");

            // simulate next request - create new validator
            assert = new AssertThatValidator(metadata, controllerContext,
                new AssertThatAttribute("1 > 2") {ErrorMessage = "{Lang:n}"});
            assertRule = assert.GetClientValidationRules().Single();
            Assert.AreEqual("polski", assertRule.ErrorMessage);

            // restore culture
            Thread.CurrentThread.CurrentUICulture = culture;
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
            var generatedCode = string.Join(" && ", Enumerable.Repeat(0, 100).Select(x => "true")); // give the parser some work
            
            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var nonCached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute(generatedCode)));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute(generatedCode)));
                Assert.IsTrue(nonCached > cached);
            }

            nonCached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute(generatedCode)));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute(generatedCode)));
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

        public enum State
        {
            High,
            Low
        }

        public class Model
        {
            public int Value { get; set; }
            [ValueParser("arrayparser")]
            public int[] Array { get; set; }
            public State Status { get; set; }
        }

        public class MsgModel
        {
            [Display(Name = "_{Value}_")]
            public int Value { get; set; }

            [Display(ResourceType = typeof (Resources), Name = "Lang")]
            public string Lang { get; set; }
        }
    }
}
