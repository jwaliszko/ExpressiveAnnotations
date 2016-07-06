using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Xunit;

namespace ExpressiveAnnotations.Tests
{
    public class AttribsTest
    {
        [Fact]
        public void verify_attributes_uniqueness()
        {
            var attributes = typeof (Model).GetProperty("Value1")
                .GetCustomAttributes()
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.Equal(2, attributes.Count); // ignores redundant attributes of the same type id, because they do nothing new (exact type name, exact expression)
            Assert.True(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));

            attributes = typeof (Model).GetProperty("Value2")
                .GetCustomAttributes()
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.Equal(4, attributes.Count); // all type ids are unique (despite the same type names of some attributes, they contain different expressions)
            Assert.True(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));
        }

        [Fact]
        public void no_errors_during_derived_types_validation()
        {            
            var firstDerived = new FirstDerived();
            var secondDerived = new SecondDerived();
            var firstContext = new ValidationContext(firstDerived);
            var secondContext = new ValidationContext(secondDerived);

            // initialize annotated property value with null, for deep go-through of RequiredIf logic
            firstDerived.Value1 = null;
            secondDerived.Value1 = null;
            // put into cache different funcs...
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
            // ...and use already cached ones accordingly
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);

            // initialize annotated property value with non-null, for deep go-through of AssertThat logic
            firstDerived.Value1 = new object();
            secondDerived.Value1 = new object();
            // put into cache different funcs...
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
            // ...and use already cached ones accordingly
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
        }

        [Fact]
        public void verify_attributes_compilation_caching_indirectly()
        {
            const int testLoops = 10;
            var model = new WorkModel();
            var context = new ValidationContext(model);

            model.Value = 0; // tests asserthat
            var nonCached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));
                Assert.True(nonCached > cached);
            }

            model.Value = null; // tests requiredif
            nonCached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));
                Assert.True(nonCached > cached);
            }
        }

        [Fact]
        public void verify_attributes_compilation_caching_directly()
        {
            const int testLoops = 10;
            List<ExpressiveAttribute> attribs = null;

            var nonCached = MeasureExecutionTime(() => attribs = typeof (WorkModel).CompileExpressiveAttributes().ToList());
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => attribs.ForEach(x => x.Compile(typeof (WorkModel))));
                Assert.True(nonCached > cached);
            }

            nonCached = MeasureExecutionTime(() => attribs.ForEach(x => x.Compile(typeof (WorkModel), force: true))); // forcibly recompile already compiled expressions
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => attribs.ForEach(x => x.Compile(typeof (WorkModel))));
                Assert.True(nonCached > cached);
            }
        }

        [Fact]
        public void verify_parsing_error_catched_by_attribute()
        {
            var model = new BrokenModel();
            var context = new ValidationContext(model);
            
            model.Value = 0;                
            var e = Assert.Throws<ValidationException>(() => Validator.TryValidateObject(model, context, null, true));
            Assert.Equal(
                "AssertThatAttribute: validation applied to Value field failed.", 
                e.Message);
            Assert.IsType<ParseErrorException>(e.InnerException);
            Assert.Equal(
                @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                e.InnerException.Message);            

            model.Value = null;
            e = Assert.Throws<ValidationException>(() => Validator.TryValidateObject(model, context, null, true));
            Assert.Equal(
                "RequiredIfAttribute: validation applied to Value field failed.", 
                e.Message);
            Assert.IsType<ParseErrorException>(e.InnerException);
            Assert.Equal(
                @"Parse error on line 1, column 9:
... # ...
    ^--- Invalid token.",
                e.InnerException.Message);
        }

        [Fact]
        public void verify_default_error_message_after_validation()
        {
            AssertErrorMessage(
                null,
                "Assertion for _{Value1}_ field is not satisfied by the following logic: 1!=1",
                "The _{Value1}_ field is required by the following logic: 1==1");
        }

        [Fact]
        public void verify_custom_error_message_after_validation()
        {
            //string.Format("{0}", 1); -> 1
            //string.Format("{{0}}", 1); -> {0}
            //string.Format("{{{0}}}", 1); -> {1}
            //string.Format("{{{{0}}}}", 1); -> {{0}}

            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {Value1}{Value1:n}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value2:N}",
                "field: _{Value1}_, expr: 1!=1 | Value1: 0_{Value1}_, Internal.Internal.Value1: 2, _{Value2}_",
                "field: _{Value1}_, expr: 1==1 | Value1: 0_{Value1}_, Internal.Internal.Value1: 2, _{Value2}_");
            AssertErrorMessage( // all escaped
                "field: {{0}}, expr: {{1}} | Value1: {{Value1}}{{Value1:n}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value2:N}}",
                "field: {0}, expr: {1} | Value1: {Value1}{Value1:n}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value2:N}");
            AssertErrorMessage(
                "field: {{{0}}}, expr: {{{1}}} | Value1: {{{Value1}}}{{{Value1:n}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value2:N}}}",
                "field: {_{Value1}_}, expr: {1!=1} | Value1: {0}{_{Value1}_}, Internal.Internal.Value1: {2}, {_{Value2}_}",
                "field: {_{Value1}_}, expr: {1==1} | Value1: {0}{_{Value1}_}, Internal.Internal.Value1: {2}, {_{Value2}_}");
            AssertErrorMessage(  // all double-escaped
                "field: {{{{0}}}}, expr: {{{{1}}}} | Value1: {{{{Value1}}}}{{{{Value1:n}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value2:N}}}}",
                "field: {{0}}, expr: {{1}} | Value1: {{Value1}}{{Value1:n}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value2:N}}");

            //string.Format("{{0", 1); -> {0
            //string.Format("0}}", 1); -> 0}

            AssertErrorMessage(
                "field: {{0, expr: {{1 | Value1: {{Value1{{Value1:n, Internal.Internal.Value1: Internal.Internal.Value1}}, Internal.Internal.Value2:N}}",
                "field: {0, expr: {1 | Value1: {Value1{Value1:n, Internal.Internal.Value1: Internal.Internal.Value1}, Internal.Internal.Value2:N}");
        }

        [Fact]
        public void custom_error_message_tolerates_null_value()
        {
            AssertErrorMessage("lang: '{Lang}'", "lang: ''");
        }

        [Fact]
        public void verify_format_exceptions_from_incorrect_custom_format_specifiers() // custom specifiers handling should throw the same formatting error as framework implementation, when incorrect nesting is detected
        {
            new[]
            {
                "{{field}", "{{field.field}", "{field}}", "{field.field}}",
                "{{field:n}", "{{field.field:n}", "{field:N}}", "{field.field:N}}"
            }.ToList().ForEach(msg =>
            {
                var attrib = new AssertThatAttribute("true") {ErrorMessage = msg};
                var e = Assert.Throws<FormatException>(() => attrib.FormatErrorMessage("asd", "true", null));
                Assert.Equal($"Problem with error message processing. The message is following: {msg}", e.Message);
                Assert.IsType<FormatException>(e.InnerException);
                Assert.Equal("Input string was not in a correct format.", e.InnerException.Message);

                IDictionary<string, Guid> errFieldsMap;
                e = Assert.Throws<FormatException>(() => attrib.FormatErrorMessage("asd", "true", typeof (object), out errFieldsMap));
                Assert.Equal($"Problem with error message processing. The message is following: {msg}", e.Message);
                Assert.IsType<FormatException>(e.InnerException);
                Assert.Equal("Input string was not in a correct format.", e.InnerException.Message);
            });
        }

        [Fact]
        public void verify_that_culture_change_affects_validation_message()
        {
            AssertErrorMessage("{Lang:n}", "default");

            // change culture
            var culture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl");

            AssertErrorMessage("{Lang:n}", "polski");

            // restore culture
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        [Fact]
        public void verify_attributes_identification()
        {
            var assertThat = new AssertThatAttribute("1 !=    1"); // spaces on purpose
            var requiredIf = new RequiredIfAttribute("1 ==    1");

            const string assertThatRepresentation = "ExpressiveAnnotations.Attributes.AssertThatAttribute[1!=1]";
            const string requiredIfRepresentation = "ExpressiveAnnotations.Attributes.RequiredIfAttribute[1==1]";

            Assert.Equal(assertThatRepresentation, assertThat.TypeId);
            Assert.Equal(requiredIfRepresentation, requiredIf.TypeId);

            Assert.Equal(assertThatRepresentation, assertThat.ToString());
            Assert.Equal(requiredIfRepresentation, requiredIf.ToString());

            Assert.Equal(assertThatRepresentation.GetHashCode(), assertThat.GetHashCode());
            Assert.Equal(requiredIfRepresentation.GetHashCode(), requiredIf.GetHashCode());
        }

        [Fact]
        public void verify_attributes_equality()
        {
            var assertThat = new AssertThatAttribute("1 !=    1");
            var requiredIf = new RequiredIfAttribute("1 ==    1");

            var assertThat2 = new AssertThatAttribute("1    != 1");
            var requiredIf2 = new RequiredIfAttribute("1    == 1");

            Assert.True(assertThat.Equals(assertThat2));
            Assert.True(requiredIf.Equals(requiredIf2));

            Assert.False(assertThat.Equals(null));
            Assert.False(requiredIf.Equals(null));
        }

        [Fact]
        public void verify_attribute_constructor_invalid_parameter()
        {
            var e = Assert.Throws<ArgumentNullException>(() => new AssertThatAttribute(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);
            e = Assert.Throws<ArgumentNullException>(() => new RequiredIfAttribute(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);
        }

        [Fact]
        public void throw_when_priority_is_not_provided_but_requested_explicitly()
        {
            var assertThat = new AssertThatAttribute("1!=1");
            var requiredIf = new RequiredIfAttribute("1==1");

            Assert.Null(assertThat.GetPriority());
            Assert.Null(requiredIf.GetPriority());

            var e = Assert.Throws<InvalidOperationException>(() => assertThat.Priority);
            Assert.Equal("The Priority property has not been set. Use the GetPriority method to get the value.", e.Message);
            e = Assert.Throws<InvalidOperationException>(() => requiredIf.Priority);
            Assert.Equal("The Priority property has not been set. Use the GetPriority method to get the value.", e.Message);
        }

        [Fact]
        public void display_attribute_takes_precedence_over_displayname_attribute()
        {
            var model = new DisplayModel();
            var context = new ValidationContext(model);

            var results = new List<ValidationResult>();

            model.Value3 = null;
            Validator.TryValidateObject(model, context, results, true);
            Assert.Equal("requiredif only chosen", results.Single().ErrorMessage);

            results.Clear();

            model.Value3 = new object();
            Validator.TryValidateObject(model, context, results, true);
            Assert.Equal("assertthat only chosen", results.Single().ErrorMessage);
        }

        [Fact]
        public void throw_when_requirement_is_applied_to_field_of_non_nullable_value_type()
        {
            var model = new MisusedRequirementModel();
            var context = new ValidationContext(model);
            var e = Assert.Throws<ValidationException>(() => Validator.TryValidateObject(model, context, null, true));
            Assert.Equal("RequiredIfAttribute: validation applied to Value field failed.", e.Message);
            Assert.IsType<InvalidOperationException>(e.InnerException);
            Assert.Equal(
                "RequiredIfAttribute has no effect when applied to a field of non-nullable value type 'System.Int32'. Use nullable 'System.Int32?' version instead.",
                e.InnerException.Message);
        }

        [Fact]
        public void verify_retrieval_of_correct_member_names_when_validation_context_is_broken()
        {
            var model = new HackTestModel();
            var attrib = new AssertThatAttribute("false");

            // correct path:
            Assert.Equal("Value1", GetFinalMemberName(attrib, model, 0, "Value1", "Value1"));
            Assert.Equal("Value2", GetFinalMemberName(attrib, model, 0, "Value2", "Value 2"));
            Assert.Equal("Value3", GetFinalMemberName(attrib, model, 0, "Value3", "Value 3"));

            // first issue: no member name provided (MVC <= 4)
            Assert.Equal("Value1", GetFinalMemberName(attrib, model, 0, null, "Value1"));
            Assert.Equal("Value2", GetFinalMemberName(attrib, model, 0, null, "Value 2"));
            Assert.Equal("Value3", GetFinalMemberName(attrib, model, 0, null, "Value 3"));

            // second issue: member name equals to display name (WebAPI 2)
            Assert.Equal("Value2", GetFinalMemberName(attrib, model, 0, "Value 2", "Value 2"));
            Assert.Equal("Value3", GetFinalMemberName(attrib, model, 0, "Value 3", "Value 3"));
        }

        [Fact]
        public void return_null_in_place_of_member_name_when_it_cannot_be_unambiguously_retrieved()
        {
            var model = new DisplayDuplicateModel();
            var attrib = new AssertThatAttribute("false");

            Assert.Equal(null, GetFinalMemberName(attrib, model, 0, null, "duplicate"));
        }

        private static string GetFinalMemberName(ValidationAttribute attrib, object contextModel, object mamberValue, string givenMemberName, string givenDisplayName)
        {
            return attrib.GetValidationResult(mamberValue, new ValidationContext(contextModel)
            {
                MemberName = givenMemberName,
                DisplayName = givenDisplayName
            })?.MemberNames.Single();
        }

        private static void AssertErrorMessage(string input, string output)
        {
            AssertErrorMessage(input, output, output);
        }

        private static void AssertErrorMessage(string input, string assertThatOutput, string requiredIfOutput)
        {
            var assertThat = new AssertThatAttribute("1!=1");
            var requiredIf = new RequiredIfAttribute("1==1");

            var isValid = typeof (ExpressiveAttribute).GetMethod("IsValid", BindingFlags.NonPublic | BindingFlags.Instance);
            var context = new ValidationContext(new MsgModel
            {
                Value1 = 0,                
                Internal = new MsgModel
                {
                    Value1 = 1,
                    Internal = new MsgModel {Value1 = 2}
                }
            })
            {
                MemberName = "Value1"
            };

            if (input != null)
                assertThat.ErrorMessage = requiredIf.ErrorMessage = input;
            
            var assertThatResult = (ValidationResult) isValid.Invoke(assertThat, new[] {new object(), context});
            var requiredIfResult = (ValidationResult) isValid.Invoke(requiredIf, new[] {null, context});

            Assert.Equal(assertThatOutput, assertThatResult.ErrorMessage);
            Assert.Equal(requiredIfOutput, requiredIfResult.ErrorMessage);
        }

        private long MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedTicks;
        }

        private abstract class ModelBase
        {
            [RequiredIf("Value2")]
            [AssertThat("Value2")]
            public object Value1 { get; set; }

            public bool Value2 { get; set; }
        }

        private class FirstDerived : ModelBase { }
        private class SecondDerived : ModelBase { }

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

        private class MsgModel
        {
            [Display(Name = "_{Value1}_")]
            public int? Value1 { get; set; }

            [Display(ResourceType = typeof (Resources), Name = "Value2")]
            public int Value2 { get; set; }            

            [Display(ResourceType = typeof (Resources), Name = "Lang")]
            public string Lang { get; set; }

            public MsgModel Internal { get; set; }
        }

        private class DisplayModel
        {
            [DisplayName("only")]
            public int Value1 { get; set; }

            [DisplayName("redundant")]
            [Display(Name = "chosen")]
            public int Value2 { get; set; }

            [RequiredIf("1 > 0", ErrorMessage = "requiredif {Value1:n} {Value2:n}")]
            [AssertThat("1 < 0", ErrorMessage = "assertthat {Value1:n} {Value2:n}")]
            public object Value3 { get; set; }
        }

        private class BrokenModel
        {
            [RequiredIf("Value > #")]
            [AssertThat("Value > #")]
            public int? Value { get; set; }
        }

        private class MisusedRequirementModel
        {
            [RequiredIf("true")]            
            public int Value { get; set; }
        }

        private class WorkModel
        {
            private const string HeavyExpression = // give the parser some work (deep dive)
                "((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((((!(1 < 0*~0) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0)) && !(1 < 0*~0))";

            [RequiredIf(HeavyExpression)]
            [AssertThat(HeavyExpression)]
            public int? Value { get; set; }
        }

        private class HackTestModel
        {
            public int? Value1 { get; set; }

            [Display(Name = "Value 2")]
            public int? Value2 { get; set; }

            [DisplayName("Value 3")]
            public int? Value3 { get; set; }
        }

        private class DisplayDuplicateModel
        {
            [Display(Name = "duplicate")]
            public int? Value1 { get; set; }
            [Display(Name = "duplicate")]
            public int? Value2 { get; set; }
        }
    }
}
