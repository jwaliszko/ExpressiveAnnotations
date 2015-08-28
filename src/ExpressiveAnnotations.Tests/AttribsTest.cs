using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public void verify_default_error_messages_before_validation()
        {
            AssertErrorMessage(null,
                "Assertion for FieldName field is not satisfied by the following logic: 1!=1",
                "The FieldName field is required by the following logic: 1==1",
                false);
        }

        [TestMethod]
        public void verify_default_error_messages_after_validation()
        {
            AssertErrorMessage(null,
                "Assertion for FieldName field is not satisfied by the following logic: 1!=1",
                "The FieldName field is required by the following logic: 1==1",
                true);
        }

        [TestMethod]
        public void verify_values_extraction_for_error_messages_before_validation() // before validation custom formatters are not applied (and e.g. are sent in unchanged form to client)
        {
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                "field: FieldName, expr: 1!=1 | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                "field: FieldName, expr: 1==1 | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                false);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                "field: FieldName, expr: 1!=1 | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                "field: FieldName, expr: 1==1 | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                false);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{{Value1}}}{{{Value1}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value1}}}",
                "field: FieldName, expr: 1!=1 | Value1: {{{Value1}}}{{{Value1}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value1}}}",
                "field: FieldName, expr: 1==1 | Value1: {{{Value1}}}{{{Value1}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value1}}}",
                false);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{{{Value1}}}}{{{{Value1}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value1}}}}",
                "field: FieldName, expr: 1!=1 | Value1: {{{{Value1}}}}{{{{Value1}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value1}}}}",
                "field: FieldName, expr: 1==1 | Value1: {{{{Value1}}}}{{{{Value1}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value1}}}}",
                false);
        }

        [TestMethod]
        public void verify_values_extraction_for_error_messages_after_validation()
        {
            //string.Format("{0}", 1); -> 1
            //string.Format("{{0}}", 1); -> {0}
            //string.Format("{{{0}}}", 1); -> {1}
            //string.Format("{{{{0}}}}", 1); -> {{0}}

            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                "field: FieldName, expr: 1!=1 | Value1: 00, Internal.Internal.Value1: 2, 2",
                "field: FieldName, expr: 1==1 | Value1: 00, Internal.Internal.Value1: 2, 2",
                true);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                "field: FieldName, expr: 1!=1 | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                "field: FieldName, expr: 1==1 | Value1: {Value1}{Value1}, Internal.Internal.Value1: {Internal.Internal.Value1}, {Internal.Internal.Value1}",
                true);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{{Value1}}}{{{Value1}}}, Internal.Internal.Value1: {{{Internal.Internal.Value1}}}, {{{Internal.Internal.Value1}}}",
                "field: FieldName, expr: 1!=1 | Value1: {0}{0}, Internal.Internal.Value1: {2}, {2}",
                "field: FieldName, expr: 1==1 | Value1: {0}{0}, Internal.Internal.Value1: {2}, {2}",
                true);
            AssertErrorMessage(
                "field: {0}, expr: {1} | Value1: {{{{Value1}}}}{{{{Value1}}}}, Internal.Internal.Value1: {{{{Internal.Internal.Value1}}}}, {{{{Internal.Internal.Value1}}}}",
                "field: FieldName, expr: 1!=1 | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                "field: FieldName, expr: 1==1 | Value1: {{Value1}}{{Value1}}, Internal.Internal.Value1: {{Internal.Internal.Value1}}, {{Internal.Internal.Value1}}",
                true);
        }

        private static void AssertErrorMessage(string input, string assertThatOutput, string requiredIfoutput, bool afterValidation)
        {
            var assertThat = new AssertThatAttribute("1!=1");
            var requiredIf = new RequiredIfAttribute("1==1");

            var isValid = typeof (ExpressiveAttribute).GetMethod("IsValid", BindingFlags.NonPublic | BindingFlags.Instance);
            var context = new ValidationContext(new Model
            {
                Value1 = 0,
                Internal = new Model
                {
                    Value1 = 1,
                    Internal = new Model {Value1 = 2}
                }
            })
            {
                MemberName = "FieldName"
            };

            if (input != null)
                assertThat.ErrorMessage = requiredIf.ErrorMessage = input;

            var assertThatError = assertThat.FormatErrorMessage(context.MemberName, assertThat.Expression);
            var requiredIfError = requiredIf.FormatErrorMessage(context.MemberName, requiredIf.Expression);

            if (afterValidation)
            {
                var assertThatResult = (ValidationResult) isValid.Invoke(assertThat, new[] {new object(), context});
                var requiredIfResult = (ValidationResult) isValid.Invoke(requiredIf, new[] {null, context});

                assertThatError = assertThatResult.ErrorMessage;
                requiredIfError = requiredIfResult.ErrorMessage;
            }

            Assert.AreEqual(assertThatOutput, assertThatError);
            Assert.AreEqual(requiredIfoutput, requiredIfError);
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

            public Model Internal { get; set; }
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
