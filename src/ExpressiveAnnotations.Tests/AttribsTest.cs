using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class AttribsTest
    {
        [TestMethod]
        public void verify_attributes_uniqueness()
        {
            var attributes = typeof (Model).GetProperty("Value1")
                .GetCustomAttributes()
                .DistinctBy(x => x.TypeId)
                .ToList();
            Assert.AreEqual(2, attributes.Count); // ignores redundant attributes of the same type id, because they do nothing new (exact type name, exact expression)
            Assert.IsTrue(new[]
            {
                typeof (RequiredIfAttribute),
                typeof (AssertThatAttribute)
            }.All(x => attributes.Select(y => y.GetType()).Contains(x)));

            attributes = typeof (Model).GetProperty("Value2")
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

        [TestMethod]
        public void verify_validation_execution_for_derived_types()
        {
            // just assure that no exception is thrown during validation related to types casting and cached funcs
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
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
            Validator.TryValidateObject(firstDerived, firstContext, null, true);
            Validator.TryValidateObject(secondDerived, secondContext, null, true);
        }

        [TestMethod]
        public void verify_attributes_compilation_caching_indirectly()
        {
            const int testLoops = 10;
            var model = new Model();
            var context = new ValidationContext(model);

            var nonCached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));            
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => Validator.TryValidateObject(model, context, null, true));
                Assert.IsTrue(nonCached > cached);
            }
        }

        [TestMethod]
        public void verify_attributes_compilation_caching_directly()
        {
            const int testLoops = 10;
            List<ExpressiveAttribute> compiled = null;

            var nonCached = MeasureExecutionTime(() => compiled = typeof (Model).CompileExpressiveAttributes().ToList());
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => compiled.ForEach(x => x.Compile(typeof (Model))));
                Assert.IsTrue(nonCached > cached);
            }

            nonCached = MeasureExecutionTime(() => compiled.ForEach(x => x.Compile(typeof (Model), force: true))); // forcibly recompile already compiled expressions
            for (var i = 0; i < testLoops; i++)
            {
                var cached = MeasureExecutionTime(() => compiled.ForEach(x => x.Compile(typeof (Model))));
                Assert.IsTrue(nonCached > cached);
            }
        }

        [TestMethod]
        public void verify_parsing_error_catched_by_attribute()
        {
            var model = new BrokenModel();
            var context = new ValidationContext(model);

            try
            {
                model.Value = 0;
                Validator.TryValidateObject(model, context, null, true);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "AssertThatAttribute: validation applied to Value field failed.",
                    e.Message);
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
                model.Value = null;
                Validator.TryValidateObject(model, context, null, true);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ValidationException);
                Assert.AreEqual(
                    "RequiredIfAttribute: validation applied to Value field failed.",
                    e.Message);
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
        public void verify_default_error_message_after_validation()
        {
            AssertErrorMessage(null,
                "Assertion for #{Value1}# field is not satisfied by the following logic: 1!=1",
                "The #{Value1}# field is required by the following logic: 1==1");
        }

        [TestMethod]
        public void verify_custom_error_message_after_validation()
        {
            //string.Format("{0}", 1); -> 1
            //string.Format("{{0}}", 1); -> {0}
            //string.Format("{{{0}}}", 1); -> {1}
            //string.Format("{{{{0}}}}", 1); -> {{0}}

            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {Value1}{Value1:n}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value2:N}",
                "field: #{Value1}#, expr: 1!=1 | Value1: 0_{Value1}_, Internal.Internal.Value1: 2, _{Value2}_",
                "field: #{Value1}#, expr: 1==1 | Value1: 0_{Value1}_, Internal.Internal.Value1: 2, _{Value2}_");
            AssertErrorMessage( // all escaped
                "field: {{0}}, expr: {{1}} | Value1: {{Value1}}{{Value1:n}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value2:N}}",
                "field: {0}, expr: {1} | Value1: {Value1}{Value1:n}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value2:N}",
                "field: {0}, expr: {1} | Value1: {Value1}{Value1:n}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value2:N}");
            AssertErrorMessage(
                "field: {{{0}}}, expr: {{{1}}} | Value1: {{{Value1}}}{{{Value1:n}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value2:N}}}",
                "field: {#{Value1}#}, expr: {1!=1} | Value1: {0}{_{Value1}_}, Internal.Internal.Value1: {2}, {_{Value2}_}",
                "field: {#{Value1}#}, expr: {1==1} | Value1: {0}{_{Value1}_}, Internal.Internal.Value1: {2}, {_{Value2}_}");
            AssertErrorMessage(  // all double-escaped
                "field: {{{{0}}}}, expr: {{{{1}}}} | Value1: {{{{Value1}}}}{{{{Value1:n}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value2:N}}}}",
                "field: {{0}}, expr: {{1}} | Value1: {{Value1}}{{Value1:n}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value2:N}}",
                "field: {{0}}, expr: {{1}} | Value1: {{Value1}}{{Value1:n}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value2:N}}");

            //string.Format("{{0", 1); -> {0
            //string.Format("0}}", 1); -> 0}

            AssertErrorMessage(
                "field: {{0, expr: {{1 | Value1: {{Value1{{Value1:n, Internal.Internal.Value1: Internal.Internal.Value1}}, Internal.Internal.Value2:N}}",
                "field: {0, expr: {1 | Value1: {Value1{Value1:n, Internal.Internal.Value1: Internal.Internal.Value1}, Internal.Internal.Value2:N}",
                "field: {0, expr: {1 | Value1: {Value1{Value1:n, Internal.Internal.Value1: Internal.Internal.Value1}, Internal.Internal.Value2:N}");
        }

        [TestMethod]
        public void verify_format_exceptions_from_incorrect_custom_format_specifiers() // custom specifiers handling should throw the same formatting error as framework implementation, when incorrect nesting is detected
        {
            new[]
            {
                "{{field}", "{{field.field}", "{field}}", "{field.field}}",
                "{{field:n}", "{{field.field:n}", "{field:N}}", "{field.field:N}}"
            }.ToList().ForEach(msg =>
            {
                try
                {
                    var attrib = new AssertThatAttribute("true") {ErrorMessage = msg};
                    attrib.FormatErrorMessage("ads", "true", typeof(MsgModel));
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(FormatException));
                    Assert.AreEqual("Input string was not in a correct format.", e.Message);
                }
            });
        }

        [TestMethod]
        public void verify_that_culture_change_affects_validation_message()
        {
            AssertErrorMessage("{Lang:n}", "default", "default");

            // change culture
            var culture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("pl");

            AssertErrorMessage("{Lang:n}", "polski", "polski");

            // restore culture
            Thread.CurrentThread.CurrentUICulture = culture;
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
                MemberName = "#{Value1}#"
            };

            if (input != null)
                assertThat.ErrorMessage = requiredIf.ErrorMessage = input;

            
            var assertThatResult = (ValidationResult) isValid.Invoke(assertThat, new[] {new object(), context});
            var requiredIfResult = (ValidationResult) isValid.Invoke(requiredIf, new[] {null, context});

            Assert.AreEqual(assertThatOutput, assertThatResult.ErrorMessage);
            Assert.AreEqual(requiredIfOutput, requiredIfResult.ErrorMessage);
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
            [DisplayAttribute(Name = "_{Value1}_")]
            public int Value1 { get; set; }

            [DisplayAttribute(ResourceType = typeof (Resources), Name = "Value2")]
            public int Value2 { get; set; }

            public MsgModel Internal { get; set; }

            [Display(ResourceType = typeof (Resources), Name = "Lang")]
            public string Lang { get; set; }
        }

        private class BrokenModel
        {
            [RequiredIf("Value > #")]
            [AssertThat("Value > #")]
            public int? Value { get; set; }
        }
    }

    public static class Helper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof (ExpressiveAttribute)));
            var attributes = new List<ExpressiveAttribute>();

            foreach (var prop in properties)
            {
                var attribs = prop.GetCustomAttributes<ExpressiveAttribute>().ToList();
                attribs.ForEach(x => x.Compile(prop.DeclaringType));
                attributes.AddRange(attribs);
            }
            return attributes;
        }
    }
}
