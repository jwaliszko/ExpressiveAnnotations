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
                            "(NextCountry != \"european country\" && Compare(NextCountry, Country.Name) == 0) " +
                            "|| (Age > 24 && Age <= 55.5)" +
                        ")";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression);
            Assert.AreEqual(tokens.Length, 30);
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
            Assert.AreEqual(tokens[10].Value, "Compare");
            Assert.AreEqual(tokens[11].Value, "(");
            Assert.AreEqual(tokens[12].Value, "NextCountry");
            Assert.AreEqual(tokens[13].Value, ",");
            Assert.AreEqual(tokens[14].Value, "Country.Name");
            Assert.AreEqual(tokens[15].Value, ")");
            Assert.AreEqual(tokens[16].Value, "==");
            Assert.AreEqual(tokens[17].Value, 0);
            Assert.AreEqual(tokens[18].Value, ")");
            Assert.AreEqual(tokens[19].Value, "||");
            Assert.AreEqual(tokens[20].Value, "(");
            Assert.AreEqual(tokens[21].Value, "Age");
            Assert.AreEqual(tokens[22].Value, ">");
            Assert.AreEqual(tokens[23].Value, 24);
            Assert.AreEqual(tokens[24].Value, "&&");
            Assert.AreEqual(tokens[25].Value, "Age");
            Assert.AreEqual(tokens[26].Value, "<=");
            Assert.AreEqual(tokens[27].Value, 55.5f);
            Assert.AreEqual(tokens[28].Value, ")");
            Assert.AreEqual(tokens[29].Value, ")");

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
