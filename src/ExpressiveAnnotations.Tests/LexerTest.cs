using System;
using ExpressiveAnnotations.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void Verify_lexer_logic()
        {
            const string expression =
                "GoAbroad == true " +
                    "&& (" +
                            "(NextCountry != \"european country\" && NextCountry == Country.Name) " +
                            "|| (Age > 24 && Age <= 55.5)" +
                        ")";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression);
            Assert.AreEqual(tokens.Length, 25);
            Assert.AreEqual(tokens[0].Value, "GoAbroad");
            Assert.AreEqual(tokens[1].Value, "==");
            Assert.AreEqual(tokens[2].Value, true);
            Assert.AreEqual(tokens[3].Value, "&&");
            Assert.AreEqual(tokens[4].Value, "(");
            Assert.AreEqual(tokens[5].Value, "(");
            Assert.AreEqual(tokens[6].Value, "NextCountry");
            Assert.AreEqual(tokens[7].Value, "!=");
            Assert.AreEqual(tokens[8].Value, "european country");
            Assert.AreEqual(tokens[9].Value, "&&");
            Assert.AreEqual(tokens[10].Value, "NextCountry");
            Assert.AreEqual(tokens[11].Value, "==");
            Assert.AreEqual(tokens[12].Value, "Country.Name");
            Assert.AreEqual(tokens[13].Value, ")");
            Assert.AreEqual(tokens[14].Value, "||");
            Assert.AreEqual(tokens[15].Value, "(");
            Assert.AreEqual(tokens[16].Value, "Age");
            Assert.AreEqual(tokens[17].Value, ">");
            Assert.AreEqual(tokens[18].Value, 24);
            Assert.AreEqual(tokens[19].Value, "&&");
            Assert.AreEqual(tokens[20].Value, "Age");
            Assert.AreEqual(tokens[21].Value, "<=");
            Assert.AreEqual(tokens[22].Value, 55.5f);
            Assert.AreEqual(tokens[23].Value, ")");
            Assert.AreEqual(tokens[24].Value, ")");

            try
            {
                lexer.Analyze("true + false");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at + false.");
            }

            try
            {
                lexer.Analyze("true && ^");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at ^.");
            }
        }
    }
}
