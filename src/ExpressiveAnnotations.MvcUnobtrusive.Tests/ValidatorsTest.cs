using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace ExpressiveAnnotations.MvcUnobtrusive.Tests
{
    public class ValidatorsTest : BaseTest
    {
        [Fact]
        public void verify_client_validation_rules_collecting_for_single_model_with_multiple_annotations()
        {
            var model = new Model();
            var assertAttributes = Enumerable.Range(0, 28).Select(x => new AssertThatAttribute($"Value > {x}")).ToArray();
            var requirAttributes = Enumerable.Range(0, 28).Select(x => new RequiredIfAttribute($"Value > {x}")).ToArray();

            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var assertValidators = assertAttributes.Select(attribute => new AssertThatValidator(metadata, controllerContext, attribute)).ToList();
            var requirValidators = requirAttributes.Select(attribute => new RequiredIfValidator(metadata, controllerContext, attribute)).ToList();

            var i = 0;
            var e = Assert.Throws<ValidationException>(() =>
            {
                while (i < assertValidators.Count)
                {
                    var validator = assertValidators[i];
                    var rule = validator.GetClientValidationRules().Single();
                    var suffix = i == 0 ? string.Empty : char.ConvertFromUtf32(96 + i);
                    Assert.Equal($"assertthat{suffix}", rule.ValidationType);
                    i++;
                }
            });
            Assert.Equal(27, i); // 27 attributes passed
            Assert.Equal(
                "AssertThatValidator: collecting of client validation rules for Value field failed.",
                e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                e.InnerException.Message);

            var j = 0;
            e = Assert.Throws<ValidationException>(() =>
            {
                while (j < requirValidators.Count)
                {
                    var validator = requirValidators[j];
                    var rule = validator.GetClientValidationRules().Single();
                    var suffix = j == 0 ? string.Empty : char.ConvertFromUtf32(96 + j);
                    Assert.Equal($"requiredif{suffix}", rule.ValidationType);
                    j++;
                }
            });
            Assert.Equal(27, j); // 27 attributes passed
            Assert.Equal(
                "RequiredIfValidator: collecting of client validation rules for Value field failed.",
                e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                e.InnerException.Message);
        }

        [Fact]
        public void verify_client_validation_rules_collecting_for_multiple_models_with_single_annotation()
        {
            var models = Enumerable.Range(0, 28).Select(x => new Model()).ToList();
            var controllerContext = GetControllerContext();

            models.ForEach(model =>
            {
                var metadata = GetModelMetadata(model, m => m.Value);
                var validator = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value > 0"));
                var rule = validator.GetClientValidationRules().Single();
                Assert.Equal("assertthat", rule.ValidationType);
            });

            models.ForEach(model =>
            {
                var metadata = GetModelMetadata(model, m => m.Value);
                var validator = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value > 0"));
                var rule = validator.GetClientValidationRules().Single();
                Assert.Equal("requiredif", rule.ValidationType);
            });
        }

        [Fact]
        public void verify_client_validation_rules_collecting_for_multiple_models_with_multiple_annotations()
        {
            var models = Enumerable.Range(0, 28).Select(x => new Model()).ToList();

            var assertAttributes = Enumerable.Range(0, 28).Select(x => new AssertThatAttribute($"Value > {x}")).ToArray();
            var requirAttributes = Enumerable.Range(0, 28).Select(x => new RequiredIfAttribute($"Value > {x}")).ToArray();

            models.ForEach(model =>
            {
                var metadata = GetModelMetadata(model, m => m.Value);
                var controllerContext = GetControllerContext();
                var assertValidators = assertAttributes.Select(attribute => new AssertThatValidator(metadata, controllerContext, attribute)).ToList();

                var i = 0;
                var e = Assert.Throws<ValidationException>(() =>
                {
                    while (i < assertValidators.Count)
                    {
                        var validator = assertValidators[i];
                        var rule = validator.GetClientValidationRules().Single();
                        var suffix = i == 0 ? string.Empty : char.ConvertFromUtf32(96 + i);
                        Assert.Equal($"assertthat{suffix}", rule.ValidationType);
                        i++;
                    }
                });
                Assert.Equal(27, i); // 27 attributes passed
                Assert.Equal(
                    "AssertThatValidator: collecting of client validation rules for Value field failed.",
                    e.Message);
                Assert.IsType<InvalidOperationException>(e.InnerException);
                Assert.Equal(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.InnerException.Message);
            });

            models.ForEach(model =>
            {
                var metadata = GetModelMetadata(model, m => m.Value);
                var controllerContext = GetControllerContext();
                var requirValidators = requirAttributes.Select(attribute => new RequiredIfValidator(metadata, controllerContext, attribute)).ToList();

                var j = 0;
                var e = Assert.Throws<ValidationException>(() =>
                {
                    while (j < requirValidators.Count)
                    {
                        var validator = requirValidators[j];
                        var rule = validator.GetClientValidationRules().Single();
                        var suffix = j == 0 ? string.Empty : char.ConvertFromUtf32(96 + j);
                        Assert.Equal($"requiredif{suffix}", rule.ValidationType);
                        j++;
                    }
                });
                Assert.Equal(27, j); // 27 attributes passed
                Assert.Equal(
                    "RequiredIfValidator: collecting of client validation rules for Value field failed.",
                    e.Message);
                Assert.IsType<InvalidOperationException>(e.InnerException);
                Assert.Equal(
                    "No more than 27 unique attributes of the same type can be applied for a single field or property.",
                    e.InnerException.Message);
            });
        }

        [Fact]
        public void throw_for_client_validation_rules_collecting_when_no_httpcontext_is_available()
        {
            var model = new Model();
            var assertAttribute = new AssertThatAttribute("true");
            var requirAttribute = new RequiredIfAttribute("true");

            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var context = HttpContext.Current;
            var e = Assert.Throws<ValidationException>(() =>
            {
                var validator = new AssertThatValidator(metadata, controllerContext, assertAttribute);
                HttpContext.Current = null;
                validator.GetClientValidationRules().Single();
            });
            Assert.Equal(
                "AssertThatValidator: collecting of client validation rules for Value field failed.",
                e.Message);
            Assert.IsType<ApplicationException>(e.InnerException);
            Assert.Equal("HttpContext not available.", e.InnerException.Message);

            HttpContext.Current = context;
            e = Assert.Throws<ValidationException>(() =>
            {
                var validator = new RequiredIfValidator(metadata, controllerContext, requirAttribute);
                HttpContext.Current = null;
                validator.GetClientValidationRules().Single();

            });
            Assert.Equal(
                "RequiredIfValidator: collecting of client validation rules for Value field failed.",
                e.Message);
            Assert.IsType<ApplicationException>(e.InnerException);
            Assert.Equal("HttpContext not available.", e.InnerException.Message);
        }

        [Fact]
        public void throw_for_validators_creation_when_no_httpcontext_is_available()
        {
            var model = new Model();
            var assertAttribute = new AssertThatAttribute("true");
            var requirAttribute = new RequiredIfAttribute("true");

            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            HttpContext.Current = null;

            var e = Assert.Throws<ValidationException>(() => new AssertThatValidator(metadata, controllerContext, assertAttribute));
            Assert.Equal(
                "AssertThatValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<ApplicationException>(e.InnerException);
            Assert.Equal("HttpContext not available.", e.InnerException.Message);

            e = Assert.Throws<ValidationException>(() => new RequiredIfValidator(metadata, controllerContext, requirAttribute));
            Assert.Equal(
                "RequiredIfValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<ApplicationException>(e.InnerException);
            Assert.Equal("HttpContext not available.", e.InnerException.Message);
        }

        [Fact]
        public void throw_when_requirement_is_applied_to_field_of_non_nullable_value_type()
        {
            var model = new MisusedRequirementModel();
            var requirAttribute = new RequiredIfAttribute("true");

            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var e = Assert.Throws<ValidationException>(() => new RequiredIfValidator(metadata, controllerContext, requirAttribute));
            Assert.Equal(
                "RequiredIfValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "RequiredIfAttribute has no effect when applied to a field of non-nullable value type 'System.Int32'. Use nullable 'System.Int32?' version instead, or switch to AssertThatAttribute otherwise.",
                e.InnerException.Message);
        }

        [Fact]
        public void verify_parsing_error_catched_by_validator()
        {
            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var e = Assert.Throws<ValidationException>(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value > #")));
            Assert.Equal(
                "AssertThatValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<ParseErrorException>(e.InnerException);
            Assert.Equal(
                @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                e.InnerException.Message);

            e = Assert.Throws<ValidationException>(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value > #")));
            Assert.Equal(
                "RequiredIfValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<ParseErrorException>(e.InnerException);
            Assert.Equal(
                @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                e.InnerException.Message);
        }

        [Fact]
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

            const string expression = "Value > 0 && MathModel.PI == 3.142 && Status == ValidatorsTest.State.High && SubModel.InsensString == NInsensString";

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute(expression));
            var assertRule = assert.GetClientValidationRules().Single();

            Assert.Equal("{\"Value\":\"number\",\"Status\":\"enumeration\",\"SubModel.InsensString\":\"stringinsens\",\"NInsensString\":\"stringinsens\"}", (string) assertRule.ValidationParameters["fieldsmap"], false);
            Assert.Equal("{\"MathModel.PI\":3.142}", (string) assertRule.ValidationParameters["constsmap"], false);
            Assert.Equal("{\"ValidatorsTest.State.High\":0}", (string) assertRule.ValidationParameters["enumsmap"], false);
            Assert.Equal("{\"SubModel.InsensString\":\"stringparser\",\"Array\":\"arrayparser\"}", (string) assertRule.ValidationParameters["parsersmap"], false);
            Assert.Equal("\"Value > 0 && MathModel.PI == 3.142 && Status == ValidatorsTest.State.High && SubModel.InsensString == NInsensString\"", (string) assertRule.ValidationParameters["expression"], false);

            var requir = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute(expression));
            var requirRule = requir.GetClientValidationRules().Single();

            Assert.Equal("{\"Value\":\"number\",\"Status\":\"enumeration\",\"SubModel.InsensString\":\"stringinsens\",\"NInsensString\":\"stringinsens\"}", (string) requirRule.ValidationParameters["fieldsmap"], false);
            Assert.Equal("{\"MathModel.PI\":3.142}", (string) requirRule.ValidationParameters["constsmap"], false);
            Assert.Equal("{\"ValidatorsTest.State.High\":0}", (string) requirRule.ValidationParameters["enumsmap"], false);
            Assert.Equal("{\"SubModel.InsensString\":\"stringparser\",\"Array\":\"arrayparser\"}", (string) assertRule.ValidationParameters["parsersmap"], false);
            Assert.Equal("false", (string) requirRule.ValidationParameters["allowempty"], false);
            Assert.Equal("\"Value > 0 && MathModel.PI == 3.142 && Status == ValidatorsTest.State.High && SubModel.InsensString == NInsensString\"", (string) requirRule.ValidationParameters["expression"], false);

            JsonConvert.DefaultSettings = settings; // reset settings to original state
        }

        [Fact]
        public void empty_client_validation_rules_are_not_created()
        {
            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("1 > 2"));
            var assertRule = assert.GetClientValidationRules().Single();

            Assert.False(assertRule.ValidationParameters.ContainsKey("fieldsmap"));
            Assert.False(assertRule.ValidationParameters.ContainsKey("constsmap"));
            Assert.False(assertRule.ValidationParameters.ContainsKey("parsersmap"));
            Assert.False(assertRule.ValidationParameters.ContainsKey("errfieldsmap"));

            Assert.True(assertRule.ValidationParameters.ContainsKey("expression"));

            var requir = new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("1 > 2"));
            var requirRule = requir.GetClientValidationRules().Single();

            Assert.False(requirRule.ValidationParameters.ContainsKey("fieldsmap"));
            Assert.False(requirRule.ValidationParameters.ContainsKey("constsmap"));
            Assert.False(requirRule.ValidationParameters.ContainsKey("parsersmap"));
            Assert.False(assertRule.ValidationParameters.ContainsKey("errfieldsmap"));

            Assert.True(requirRule.ValidationParameters.ContainsKey("allowempty"));
            Assert.True(requirRule.ValidationParameters.ContainsKey("expression"));
        }

        [Fact]
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
            Assert.Equal(expected, assertRule.ErrorMessage);
        }

        [Fact]
        public void verify_that_culture_change_affects_message_sent_to_client()
        {
            var model = new MsgModel();
            var metadata = GetModelMetadata(model, m => m.Lang);
            var controllerContext = GetControllerContext();

            CulturalExecutionUI(() =>
            {
                var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("1 > 2") {ErrorMessage = "{Lang:n}"});
                var assertRule = assert.GetClientValidationRules().Single();
                Assert.Equal("default", assertRule.ErrorMessage);
            }, "en");

            // simulate next request - create new validator
            CulturalExecutionUI(() =>
            {
                var assert = new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("1 > 2") {ErrorMessage = "{Lang:n}"});
                var assertRule = assert.GetClientValidationRules().Single();
                Assert.Equal("polski", assertRule.ErrorMessage);
            }, "pl");
        }

        [Fact]
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
            Assert.False(Helper.SegmentsCollide(new string[0], new string[0], out name, out level));
            Assert.False(Helper.SegmentsCollide(new[] {"A"}, new string[0], out name, out level));
            Assert.False(Helper.SegmentsCollide(new string[0], new[] {"A"}, out name, out level));
            Assert.False(Helper.SegmentsCollide(new[] {"A"}, new[] {"B"}, out name, out level));
            Assert.False(Helper.SegmentsCollide(new[] {"A.A"}, new[] {"A.B"}, out name, out level));
            Assert.False(Helper.SegmentsCollide(new[] {"A.B.C"}, new[] {"A.B.D"}, out name, out level));
            Assert.False(Helper.SegmentsCollide(new[] {"A.B.C", "A.B.E"}, new[] {"B.B", "B.C", "B.E"}, out name, out level));

            Assert.Equal(null, name);
            Assert.Equal(level, -1);

            Assert.True(Helper.SegmentsCollide(new[] {"A"}, new[] {"A"}, out name, out level));
            Assert.Equal("A", name);
            Assert.Equal(level, 0);

            Assert.True(Helper.SegmentsCollide(new[] {"A.B"}, new[] {"A.B"}, out name, out level));
            Assert.Equal("B", name);
            Assert.Equal(level, 1);

            Assert.True(Helper.SegmentsCollide(new[] {"A.B.C"}, new[] {"A.B"}, out name, out level));
            Assert.Equal("B", name);
            Assert.Equal(level, 1);

            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var e = Assert.Throws<ValidationException>(() => new AssertThatValidator(metadata, controllerContext, new AssertThatAttribute("Value == Value.Zero")));
            Assert.Equal(
                "AssertThatValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "Naming collisions cannot be accepted by client-side - Value part at level 0 is ambiguous.",
                e.InnerException.Message);

            e = Assert.Throws<ValidationException>(() => new RequiredIfValidator(metadata, controllerContext, new RequiredIfAttribute("Value == Value.Zero")));
            Assert.Equal(
                "RequiredIfValidator: validation applied to Value field failed.",
                e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "Naming collisions cannot be accepted by client-side - Value part at level 0 is ambiguous.",
                e.InnerException.Message);
        }

        [Fact]
        public void verify_validators_caching()
        {
            const int testLoops = 10;
            var generatedCode = Enumerable.Repeat(0, 100).Select(x => "!(1 < 0*~0)")
                .Aggregate("!(1 < 0*~0)", (accumulator, item) => $"({accumulator} && {item})"); // give the parser some work (deep dive)

            var model = new Model();
            var metadata = GetModelMetadata(model, m => m.Value);
            var controllerContext = GetControllerContext();

            var assertThat = new AssertThatAttribute(generatedCode);
            var requiredIf = new RequiredIfAttribute(generatedCode);

            var nonCached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, assertThat));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new AssertThatValidator(metadata, controllerContext, assertThat));
                Assert.True(nonCached > cached);
            }

            nonCached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, requiredIf));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => new RequiredIfValidator(metadata, controllerContext, requiredIf));
                Assert.True(nonCached > cached);
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

        public enum Value
        {
            Zero,
            One
        }

        public class Model
        {
            public int? Value { get; set; }
            [ValueParser("arrayparser")]
            public int[] Array { get; set; }
            public State Status { get; set; }
            public Model SubModel { get; set; }
            [ValueParser("stringparser")]
            public StringInsens InsensString { get; set; }
            public StringInsens? NInsensString { get; set; }

            public class MathModel
            {
                public const double PI = 3.142;
            }
        }

        public class MsgModel
        {
            [Display(Name = "_{Value}_")]
            public int Value { get; set; }

            [Display(ResourceType = typeof(Resources), Name = "Lang")]
            public string Lang { get; set; }
        }

        private class MisusedRequirementModel
        {
            [RequiredIf("true")]
            public int Value { get; set; }
        }

        public struct StringInsens
        {
            private readonly string _value;

            public StringInsens(string value)
            {
                _value = value;
            }

            public bool Equals(StringInsens other)
            {
                return string.Equals(_value, other._value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is StringInsens && Equals((StringInsens)obj);
            }

            public override int GetHashCode()
            {
                return _value?.GetHashCode() ?? 0;
            }

            public static bool operator ==(StringInsens a, StringInsens b)
            {
                return string.Equals(a._value, b._value, StringComparison.CurrentCultureIgnoreCase);
            }

            public static bool operator !=(StringInsens a, StringInsens b)
            {
                return !(a == b);
            }
        }
    }
}
