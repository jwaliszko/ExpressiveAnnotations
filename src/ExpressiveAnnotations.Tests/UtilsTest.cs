using System;
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
        public void verify_serialization_of_parse_exception() // not really required since this exception is not expected to travel through various app domains (nevertheless implemented to satisfy good practices)
        {
            var ex = new ParseErrorException("Operator '!' cannot be applied to operand of type 'System.String'", new Location(1, 5));

            // sanity check: make sure custom properties are set before serialization
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.String'", ex.Message);
            Assert.NotNull(ex.Location);
            Assert.Equal(1, ex.Location.Line);
            Assert.Equal(5, ex.Location.Column);

            // save the full ToString() value, including the exception message and stack trace.
            var exceptionToString = ex.ToString();

            // round-trip the exception: serialize and de-serialize with a BinaryFormatter
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                // "save" object state
                bf.Serialize(ms, ex);
                // re-use the same stream for de-serialization
                ms.Seek(0, 0);
                // replace the original exception with de-serialized one
                ex = (ParseErrorException)bf.Deserialize(ms);
            }

            // make sure custom properties are preserved after serialization
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.String'", ex.Message);
            Assert.NotNull(ex.Location);
            Assert.Equal(1, ex.Location.Line);
            Assert.Equal(5, ex.Location.Column);

            // double-check that the exception message and stack trace (owned by the base Exception) are preserved
            Assert.Equal(exceptionToString, ex.ToString());
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

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractDisplayName(typeof (Model), "Internal.Value123"));
            Assert.Equal("Display name extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Helper.ExtractDisplayName(typeof (Model), "NoName"));
            Assert.Equal("No display name provided for NoName field. Use either Display attribute or DisplayName attribute.\r\nParameter name: NoName", e.Message);
        }

        [Fact]
        public void verify_fields_names_extraction_based_on_their_display_names() // Display attribute or DisplayName attribute used as a workaround for field name extraction in older versions of MVC where MemberName was not provided in ValidationContext
        {
            Assert.Equal("Value1", typeof (Model).GetMemberNameByDisplayName("Value_1"));
            Assert.Equal("Value2", typeof (Model).GetMemberNameByDisplayName("_{Value2}_"));
            Assert.Equal("Internal", typeof (Model).GetMemberNameByDisplayName("internal"));
        }

        [Fact]
        public void type_load_exceptions_are_handled_and_null_type_instances_are_filtered_out()
        {
            var typeProviderMock = new Mock<ITypeProvider>();

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {null}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new[] {typeof (object), null}, null));
            Assert.Equal(1, typeProviderMock.Object.GetLoadableTypes().Count());
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
