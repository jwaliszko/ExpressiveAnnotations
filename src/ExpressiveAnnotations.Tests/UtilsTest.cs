using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using ExpressiveAnnotations.Analysis;
using Moq;
using Xunit;

namespace ExpressiveAnnotations.Tests
{
    public class UtilsTest
    {
        [Fact]
        public void verify_serialization_of_basic_parse_exception() // exception is expected to travel through various app domains
        {
            var e = new ParseErrorException(@"Operator '!' cannot be applied to operand of type 'System.String'.", new ParseErrorException());

            // save the full ToString() value, including the exception message and stack trace
            var exceptionToString = e.ToString();

            // round-trip the exception: serialize and de-serialize with a BinaryFormatter
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                // "save" object state
                bf.Serialize(ms, e);
                // re-use the same stream for de-serialization
                ms.Seek(0, 0);
                // replace the original exception with de-serialized one
                e = (ParseErrorException) bf.Deserialize(ms);
            }

            // double-check that the exception message and stack trace (owned by the base Exception) are preserved
            Assert.Equal(exceptionToString, e.ToString());
        }

        [Fact]
        public void verify_serialization_of_complete_parse_exception()
        {
            var e = new ParseErrorException("Operator '!' cannot be applied to operand of type 'System.String'.", "true && !'false'", new Location(1, 9), new ParseErrorException("other"));

            // sanity check: make sure custom properties are set before serialization
            Assert.Equal(
                @"Parse error on line 1, column 9:
... !'false' ...
    ^--- Operator '!' cannot be applied to operand of type 'System.String'.",
                e.Message);
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.String'.", e.Error);
            Assert.Equal("true && !'false'", e.Expression);
            Assert.NotNull(e.Location);
            Assert.Equal(1, e.Location.Line);
            Assert.Equal(9, e.Location.Column);
            Assert.NotNull(e.InnerException);
            Assert.Equal("other", e.InnerException.Message);

            var exceptionToString = e.ToString();

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, e);
                ms.Seek(0, 0);
                e = (ParseErrorException) bf.Deserialize(ms);
            }

            // make sure custom properties are preserved after serialization
            Assert.Equal(
                @"Parse error on line 1, column 9:
... !'false' ...
    ^--- Operator '!' cannot be applied to operand of type 'System.String'.",
                e.Message);
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.String'.", e.Error);
            Assert.Equal("true && !'false'", e.Expression);
            Assert.NotNull(e.Location);
            Assert.Equal(1, e.Location.Line);
            Assert.Equal(9, e.Location.Column);
            Assert.NotNull(e.InnerException);
            Assert.Equal("other", e.InnerException.Message);

            Assert.Equal(exceptionToString, e.ToString());
        }

        [Fact]
        public void verify_fields_values_extraction_from_given_instance()
        {
            var model = new Model
            {
                Value1 = 1,
                Value2 = 2,
                Internal = new Model
                {
                    Value1 = 11,
                    Value2 = null
                }
            };

            Assert.Equal(1, ExpressiveAnnotations.Helper.ExtractValue(model, "Value1"));
            Assert.Equal(11, ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value1"));
            Assert.Equal(null, ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value2"));

            var e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractValue(model, "internal"));
            Assert.Equal("Value extraction interrupted. Field internal not found.\r\nParameter name: internal", e.Message);

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value123"));
            Assert.Equal("Value extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);

            model.Internal = null;
            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value1"));
            Assert.Equal("Value extraction interrupted. Field Internal is null.\r\nParameter name: Internal.Value1", e.Message);
        }

        [Fact]
        public void verify_display_names_extraction_from_given_type()
        {
            // name provided explicitly
            Assert.Equal("Value_1", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Value1"));
            Assert.Equal("Value_1", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value1"));

            // name provided in resources
            Assert.Equal("_{Value2}_", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Value2"));
            Assert.Equal("_{Value2}_", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value2"));

            var e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "internal"));
            Assert.Equal("Display name extraction interrupted. Field internal not found.\r\nParameter name: internal", e.Message);

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value123"));
            Assert.Equal("Display name extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "NoName"));
            Assert.Equal("No display name provided for NoName field. Use either Display attribute or DisplayName attribute.\r\nParameter name: NoName", e.Message);
        }

        [Fact]
        public void verify_fields_names_extraction_based_on_their_display_names() // Display attribute or DisplayName attribute used as a workaround for field name extraction in older versions of MVC where MemberName was not provided in ValidationContext
        {
            Assert.Equal("Value1", typeof(Model).GetPropertyByDisplayName("Value_1").Name);
            Assert.Equal("Value2", typeof(Model).GetPropertyByDisplayName("_{Value2}_").Name);
            Assert.Equal("Internal", typeof(Model).GetPropertyByDisplayName("internal").Name);
        }

        [Fact]
        public void type_load_exceptions_are_handled_and_null_type_instances_are_filtered_out()
        {
            var typeProviderMock = new Mock<ITypeProvider>();

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {null}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new[] {typeof(object), null}, null));
            Assert.Equal(1, typeProviderMock.Object.GetLoadableTypes().Count());
        }

        public static IEnumerable<object[]> ErrorData => new[]
        {
            new object[] {new Location(1,1), "\r", @"
" },
            new object[] {new Location(1,1), "\r", "\r\n" },
            new object[] {new Location(1,1), "abcde", "abcde" },
            new object[] {new Location(1,3), "cde", "abcde" },
            new object[] {new Location(1,5), "e", "abcde" },
            new object[] {new Location(2,1), "abcde", "12345\r\nabcde" },
            new object[] {new Location(2,3), "cde", "12345\r\nabcde" },
            new object[] {new Location(2,5), "e", "12345\r\nabcde" },
            new object[] {new Location(1,6), "  \r", "abcde  \r\nabcde" },
            new object[] {new Location(1,7), " \r", "abcde  \r\nabcde" },
            new object[] {new Location(1,8), "\r", "abcde  \r\nabcde" },
            new object[] {new Location(1,1), new string('a', 100), new string('a', 100) },
            new object[] {new Location(1,1), new string('a', 100), new string('a', 101) } // max 100 chars of expression is displayed
        };

        [Theory]
        [MemberData(nameof(ErrorData))]
        public void verify_error_message_construction(Location location, string indication, string expression)
        {
            Assert.Equal(
                $@"Parse error on line {location.Line}, column {location.Column}:
... {indication} ...
    ^--- error message",
                location.BuildParseError("error message", expression));
        }

        public static IEnumerable<object[]> BoundaryErrorData => new[]
        {
            new object[] {new Location(1,6), "abcde" },
            new object[] {new Location(2,6), "12345\r\nabcde" }
        };

        [Theory]
        [MemberData(nameof(BoundaryErrorData))]
        public void verify_error_message_construction_for_boundary_conditions(Location location, string expression)
        {
            Assert.Equal(
                $@"Parse error on line {location.Line}, last column: error message",
                location.BuildParseError("error message", expression));
        }

        [Fact]
        public void throw_when_non_positive_parameters_are_provided_for_error_message_construction()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 1));
            Assert.Equal("Line number should be positive.\r\nParameter name: line", e.Message);
            e = Assert.Throws<ArgumentOutOfRangeException>(() => new Location(1, 0));
            Assert.Equal("Column number should be positive.\r\nParameter name: column", e.Message);
        }

        [Fact]
        public void print_token_for_debug_purposes()
        {
            var token = new Token(TokenType.FLOAT, 1.0, "1.0", new Location(1, 2));
            Assert.Equal(@"""1.0"" FLOAT (1, 2)", token.ToString());
        }

        private class Model
        {
            [Display(Name = "Value_1")]
            public int? Value1 { get; set; }

            [Display(ResourceType = typeof(Resources), Name = "Value2")]
            public int? Value2 { get; set; }

            public string NoName { get; set; }

            [DisplayName("internal")]
            public Model Internal { get; set; }
        }
    }
}
