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
    }
}
