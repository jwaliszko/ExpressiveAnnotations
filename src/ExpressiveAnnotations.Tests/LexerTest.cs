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
        public void verify_complex_expression_analysis()
        {
            const string expression =
                @"GoAbroad == true
                      && (
                             (NextCountry != 'european country' && Compare(NextCountry, Country.Name) == 0)
                             || (Age > 24 && Age <= 55.5)
                             && (1+2*2>1-2/2)
                         )";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();
            Assert.AreEqual(tokens.Length, 45);
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
            Assert.AreEqual(tokens[27].Value, 55.5d);
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
            Assert.AreEqual(tokens[34].Value, "*");
            Assert.AreEqual(tokens[34].Type, TokenType.MUL);
            Assert.AreEqual(tokens[35].Value, 2);
            Assert.AreEqual(tokens[35].Type, TokenType.INT);
            Assert.AreEqual(tokens[36].Value, ">");
            Assert.AreEqual(tokens[36].Type, TokenType.GT);
            Assert.AreEqual(tokens[37].Value, 1);
            Assert.AreEqual(tokens[37].Type, TokenType.INT);
            Assert.AreEqual(tokens[38].Value, "-");
            Assert.AreEqual(tokens[38].Type, TokenType.SUB);
            Assert.AreEqual(tokens[39].Value, 2);
            Assert.AreEqual(tokens[39].Type, TokenType.INT);
            Assert.AreEqual(tokens[40].Value, "/");
            Assert.AreEqual(tokens[40].Type, TokenType.DIV);
            Assert.AreEqual(tokens[41].Value, 2);
            Assert.AreEqual(tokens[41].Type, TokenType.INT);
            Assert.AreEqual(tokens[42].Value, ")");
            Assert.AreEqual(tokens[42].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[43].Value, ")");
            Assert.AreEqual(tokens[43].Type, TokenType.RIGHT_BRACKET);
            Assert.AreEqual(tokens[44].Value, string.Empty);
            Assert.AreEqual(tokens[44].Type, TokenType.EOF);            
        }

        [TestMethod]
        public void verify_invalid_tokens_detection()
        {
            var lexer = new Lexer();

            try
            {
                lexer.Analyze(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentNullException);
                Assert.AreEqual(e.Message, "Expression not provided.\r\nParameter name: expression");
            }

            try
            {
                lexer.Analyze("true # false");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual(e.Message, "Invalid token.");

                var ctx = ((ParseErrorException)e).Context;
                Assert.AreEqual(ctx.Expression, "# false");
                Assert.AreEqual(ctx.Line, 1);
                Assert.AreEqual(ctx.Column, 6);
            }

            try
            {
                lexer.Analyze("true && ^");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual(e.Message, "Invalid token.");

                var ctx = ((ParseErrorException)e).Context;
                Assert.AreEqual(ctx.Expression, "^");
                Assert.AreEqual(ctx.Line, 1);
                Assert.AreEqual(ctx.Column, 9);
            }

            try
            {
                lexer.Analyze("'John's cat'");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual(e.Message, "Invalid token.");

                var ctx = ((ParseErrorException)e).Context;
                Assert.AreEqual(ctx.Expression, "'");
                Assert.AreEqual(ctx.Line, 1);
                Assert.AreEqual(ctx.Column, 12);
            }
        }

        [TestMethod]
        public void verify_float_token_extraction()
        {
            AssertToken("1.0", 1.0, TokenType.FLOAT);
            AssertToken("0.3e-2", 0.3e-2, TokenType.FLOAT);

            AssertNotToken("1", TokenType.FLOAT);
            AssertNotToken("a", TokenType.FLOAT);
        }

        [TestMethod]
        public void verify_string_token_extraction()
        {
            AssertToken("''", string.Empty, TokenType.STRING);
            AssertToken("'a'", "a", TokenType.STRING);
            AssertToken("'0123'", "0123", TokenType.STRING);
            AssertToken("' a s d '", " a s d ", TokenType.STRING);
            AssertToken(@"'Simon\'s cat named ""\\\\""\n (Double Backslash)'", @"Simon's cat named ""\\""
 (Double Backslash)", TokenType.STRING);
            // below, non-verbatim version, see \r\n which represents current environment new line (simply expressed by \n in our language)
            AssertToken("'Simon\\\'s cat named \"\\\\\\\\\"\\n (Double Backslash)'", "Simon's cat named \"\\\\\"\r\n (Double Backslash)", TokenType.STRING);

            AssertNotToken("\"0123\"", TokenType.STRING); // double-quoted text is not accepted as string literal
            AssertNotToken("'John's cat'", TokenType.STRING);
        }

        [TestMethod]
        public void verify_func_token_extraction()
        {
            AssertToken("_", "_", TokenType.FUNC);
            AssertToken("__", "__", TokenType.FUNC);
            AssertToken("a", "a", TokenType.FUNC);
            AssertToken("asd", "asd", TokenType.FUNC);
            AssertToken("a_a", "a_a", TokenType.FUNC);
            AssertToken("a.a", "a.a", TokenType.FUNC);
            AssertToken("_a.a_", "_a.a_", TokenType.FUNC);
            AssertToken("A", "A", TokenType.FUNC);
            AssertToken("_._", "_._", TokenType.FUNC);
            AssertToken("a1", "a1", TokenType.FUNC);
            AssertToken("a12.a12", "a12.a12", TokenType.FUNC);
            AssertToken("a1.a2.a3", "a1.a2.a3", TokenType.FUNC);
            AssertToken("_._._", "_._._", TokenType.FUNC);
            AssertToken("_123", "_123", TokenType.FUNC);

            AssertNotToken("1", TokenType.FUNC);
            AssertNotToken("a..a", TokenType.FUNC);
            AssertNotToken("a.1", TokenType.FUNC);
            AssertNotToken("a.+", TokenType.FUNC);
        }

        private static void AssertToken(string expression, object value, TokenType type)
        {            
            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();
            
            Assert.AreEqual(tokens.Length, 2);
            Assert.AreEqual(tokens[0].Value, value);
            Assert.AreEqual(tokens[0].Type, type);
            Assert.AreEqual(tokens[1].Type, TokenType.EOF);
        }

        private static void AssertNotToken(string expression, TokenType type)
        {
            try
            {
                var lexer = new Lexer();
                var tokens = lexer.Analyze(expression).ToArray();

                Assert.IsFalse(tokens.Length == 2 && tokens[0].Type == type);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual(e.Message, "Invalid token.");
            }
        }
    }
}
