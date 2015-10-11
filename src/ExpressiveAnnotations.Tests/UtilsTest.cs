using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExpressiveAnnotations.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void verify_serialization_of_parse_exception() // not really required since this exception is not expected to travel through various app domains (nevertheless implemented to satisfy good practices)
        {
            var ex = new ParseErrorException("Operator '!' cannot be applied to operand of type 'System.String'", new Location(1, 5));

            // sanity check: make sure custom properties are set before serialization
            Assert.AreEqual("Operator '!' cannot be applied to operand of type 'System.String'", ex.Message);
            Assert.IsNotNull(ex.Location);
            Assert.AreEqual(1, ex.Location.Line);
            Assert.AreEqual(5, ex.Location.Column);

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
            Assert.AreEqual("Operator '!' cannot be applied to operand of type 'System.String'", ex.Message);
            Assert.IsNotNull(ex.Location);
            Assert.AreEqual(1, ex.Location.Line, "ex.ValidationErrors[0]");
            Assert.AreEqual(5, ex.Location.Column, "ex.ValidationErrors[1]");

            // double-check that the exception message and stack trace (owned by the base Exception) are preserved
            Assert.AreEqual(exceptionToString, ex.ToString());
        }

        [TestMethod]
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

            Assert.AreEqual(1, ExpressiveAnnotations.Helper.ExtractValue(model, "Value1"));
            Assert.AreEqual(11, ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value1"));
            Assert.AreEqual(null, ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value2"));

            try
            {
                ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value123");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
                Assert.AreEqual("Value extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);
            }

            model.Internal = null;
            try
            {
                ExpressiveAnnotations.Helper.ExtractValue(model, "Internal.Value1");
                Assert.Fail();
            }
            catch(Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
                Assert.AreEqual("Value extraction interrupted. Field Internal is null.\r\nParameter name: Internal.Value1", e.Message);
            }
        }

        [TestMethod]
        public void verify_display_names_extraction_from_given_type()
        {
            // name provided explicitly
            Assert.AreEqual("Value_1", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Value1"));
            Assert.AreEqual("Value_1", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value1"));
            
            // name provided in resources
            Assert.AreEqual("Value_2", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Value2"));
            Assert.AreEqual("Value_2", ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value2"));

            try
            {
                ExpressiveAnnotations.Helper.ExtractDisplayName(typeof(Model), "Internal.Value123");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
                Assert.AreEqual("Display name extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);
            }
        }

        private class Model
        {
            [Display(Name = "Value_1")]
            public int? Value1 { get; set; }
            [DisplayAttribute(ResourceType = typeof(Resources), Name = "Value2")]
            public int? Value2 { get; set; }

            public Model Internal { get; set; }
        }
    }
}
