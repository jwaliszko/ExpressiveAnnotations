using System;
using System.Linq;
using ExpressiveAnnotations.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void verify_logic()
        {
            const string expression =
                "GoAbroad == true " +
                    "&& (" +
                            "(NextCountry != 'european country' && Compare(NextCountry, Country.Name) == 0) " +
                            "|| (Age > 24 && Age <= 55.5)" +
                            "&& (1 + 2 > 1 - 2)" +
                        ")";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();
            Assert.AreEqual(tokens.Length, 41);
            Assert.AreEqual(tokens[0].Value, "GoAbroad");
            Assert.AreEqual(tokens[0].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[1].Value, "==");
            Assert.AreEqual(tokens[1].Type, TokenType.EQ);
            Assert.AreEqual(tokens[2].Value, true);
            Assert.AreEqual(tokens[2].Type, TokenType.BOOL);
            Assert.AreEqual(tokens[3].Value, "&&");
            Assert.AreEqual(tokens[3].Type, TokenType.AND);
            Assert.AreEqual(tokens[4].Value, "(");
            Assert.AreEqual(tokens[4].Type, TokenType.LEFT_BRACKET);
            Assert.AreEqual(tokens[5].Value, "(");
            Assert.AreEqual(tokens[5].Type, TokenType.LEFT_BRACKET);
            Assert.AreEqual(tokens[6].Value, "NextCountry");
            Assert.AreEqual(tokens[6].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[7].Value, "!=");
            Assert.AreEqual(tokens[7].Type, TokenType.NEQ);
            Assert.AreEqual(tokens[8].Value, "european country");
            Assert.AreEqual(tokens[8].Type, TokenType.STRING);
            Assert.AreEqual(tokens[9].Value, "&&");
            Assert.AreEqual(tokens[9].Type, TokenType.AND);
            Assert.AreEqual(tokens[10].Value, "Compare");
            Assert.AreEqual(tokens[10].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[11].Value, "(");
            Assert.AreEqual(tokens[11].Type, TokenType.LEFT_BRACKET);
            Assert.AreEqual(tokens[12].Value, "NextCountry");
            Assert.AreEqual(tokens[12].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[13].Value, ",");
            Assert.AreEqual(tokens[13].Type, TokenType.COMMA);
            Assert.AreEqual(tokens[14].Value, "Country.Name");
            Assert.AreEqual(tokens[14].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[15].Value, ")");
            Assert.AreEqual(tokens[15].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[16].Value, "==");
            Assert.AreEqual(tokens[16].Type, TokenType.EQ);
            Assert.AreEqual(tokens[17].Value, 0);
            Assert.AreEqual(tokens[17].Type, TokenType.INT);
            Assert.AreEqual(tokens[18].Value, ")");
            Assert.AreEqual(tokens[18].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[19].Value, "||");
            Assert.AreEqual(tokens[19].Type, TokenType.OR);
            Assert.AreEqual(tokens[20].Value, "(");
            Assert.AreEqual(tokens[20].Type, TokenType.LEFT_BRACKET);
            Assert.AreEqual(tokens[21].Value, "Age");
            Assert.AreEqual(tokens[21].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[22].Value, ">");
            Assert.AreEqual(tokens[22].Type, TokenType.GT);
            Assert.AreEqual(tokens[23].Value, 24);
            Assert.AreEqual(tokens[23].Type, TokenType.INT);
            Assert.AreEqual(tokens[24].Value, "&&");
            Assert.AreEqual(tokens[24].Type, TokenType.AND);
            Assert.AreEqual(tokens[25].Value, "Age");
            Assert.AreEqual(tokens[25].Type, TokenType.FUNC);
            Assert.AreEqual(tokens[26].Value, "<=");
            Assert.AreEqual(tokens[26].Type, TokenType.LE);
            Assert.AreEqual(tokens[27].Value, 55.5f);
            Assert.AreEqual(tokens[27].Type, TokenType.FLOAT);
            Assert.AreEqual(tokens[28].Value, ")");
            Assert.AreEqual(tokens[28].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[29].Value, "&&");
            Assert.AreEqual(tokens[29].Type, TokenType.AND);
            Assert.AreEqual(tokens[30].Value, "(");
            Assert.AreEqual(tokens[30].Type, TokenType.LEFT_BRACKET);
            Assert.AreEqual(tokens[31].Value, 1);
            Assert.AreEqual(tokens[31].Type, TokenType.INT);
            Assert.AreEqual(tokens[32].Value, "+");
            Assert.AreEqual(tokens[32].Type, TokenType.ADD);
            Assert.AreEqual(tokens[33].Value, 2);
            Assert.AreEqual(tokens[33].Type, TokenType.INT);
            Assert.AreEqual(tokens[34].Value, ">");
            Assert.AreEqual(tokens[34].Type, TokenType.GT);
            Assert.AreEqual(tokens[35].Value, 1);
            Assert.AreEqual(tokens[35].Type, TokenType.INT);
            Assert.AreEqual(tokens[36].Value, "-");
            Assert.AreEqual(tokens[36].Type, TokenType.SUB);
            Assert.AreEqual(tokens[37].Value, 2);
            Assert.AreEqual(tokens[37].Type, TokenType.INT);
            Assert.AreEqual(tokens[38].Value, ")");
            Assert.AreEqual(tokens[38].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[39].Value, ")");
            Assert.AreEqual(tokens[39].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[40].Value, string.Empty);
            Assert.AreEqual(tokens[40].Type, TokenType.EOF);

            try
            {
                lexer.Analyze("true # false");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at # false.");
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
