using System;
using System.Globalization;
using System.Linq;
using System.Threading;
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
            Assert.AreEqual(45, tokens.Length);
            Assert.AreEqual("GoAbroad", tokens[0].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[0].Type);
            Assert.AreEqual("==", tokens[1].Value);
            Assert.AreEqual(TokenType.EQ, tokens[1].Type);
            Assert.AreEqual(true, tokens[2].Value);
            Assert.AreEqual(TokenType.BOOL, tokens[2].Type);
            Assert.AreEqual("&&", tokens[3].Value);
            Assert.AreEqual(TokenType.AND, tokens[3].Type);
            Assert.AreEqual("(", tokens[4].Value);
            Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[4].Type);
            Assert.AreEqual("(", tokens[5].Value);
            Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[5].Type);
            Assert.AreEqual("NextCountry", tokens[6].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[6].Type);
            Assert.AreEqual("!=", tokens[7].Value);
            Assert.AreEqual(TokenType.NEQ, tokens[7].Type);
            Assert.AreEqual("european country", tokens[8].Value);
            Assert.AreEqual(TokenType.STRING, tokens[8].Type);
            Assert.AreEqual("&&", tokens[9].Value);
            Assert.AreEqual(TokenType.AND, tokens[9].Type);
            Assert.AreEqual("Compare", tokens[10].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[10].Type);
            Assert.AreEqual("(", tokens[11].Value);
            Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[11].Type);
            Assert.AreEqual("NextCountry", tokens[12].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[12].Type);
            Assert.AreEqual(",", tokens[13].Value);
            Assert.AreEqual(TokenType.COMMA, tokens[13].Type);
            Assert.AreEqual("Country.Name", tokens[14].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[14].Type);
            Assert.AreEqual(")", tokens[15].Value);
            Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[15].Type);
            Assert.AreEqual("==", tokens[16].Value);
            Assert.AreEqual(TokenType.EQ, tokens[16].Type);
            Assert.AreEqual(0, tokens[17].Value);
            Assert.AreEqual(TokenType.INT, tokens[17].Type);
            Assert.AreEqual(")", tokens[18].Value);
            Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[18].Type);
            Assert.AreEqual("||", tokens[19].Value);
            Assert.AreEqual(TokenType.OR, tokens[19].Type);
            Assert.AreEqual("(", tokens[20].Value);
            Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[20].Type);
            Assert.AreEqual("Age", tokens[21].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[21].Type);
            Assert.AreEqual(">", tokens[22].Value);
            Assert.AreEqual(TokenType.GT, tokens[22].Type);
            Assert.AreEqual(24, tokens[23].Value);
            Assert.AreEqual(TokenType.INT, tokens[23].Type);
            Assert.AreEqual("&&", tokens[24].Value);
            Assert.AreEqual(TokenType.AND, tokens[24].Type);
            Assert.AreEqual("Age", tokens[25].Value);
            Assert.AreEqual(TokenType.FUNC, tokens[25].Type);
            Assert.AreEqual("<=", tokens[26].Value);
            Assert.AreEqual(TokenType.LE, tokens[26].Type);
            Assert.AreEqual(55.5d, tokens[27].Value);
            Assert.AreEqual(TokenType.FLOAT, tokens[27].Type);
            Assert.AreEqual(")", tokens[28].Value);
            Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[28].Type);
            Assert.AreEqual("&&", tokens[29].Value);
            Assert.AreEqual(TokenType.AND, tokens[29].Type);
            Assert.AreEqual("(", tokens[30].Value);
            Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[30].Type);
            Assert.AreEqual(1, tokens[31].Value);
            Assert.AreEqual(TokenType.INT, tokens[31].Type);
            Assert.AreEqual("+", tokens[32].Value);
            Assert.AreEqual(TokenType.ADD, tokens[32].Type);
            Assert.AreEqual(2, tokens[33].Value);
            Assert.AreEqual(TokenType.INT, tokens[33].Type);
            Assert.AreEqual("*", tokens[34].Value);
            Assert.AreEqual(TokenType.MUL, tokens[34].Type);
            Assert.AreEqual(2, tokens[35].Value);
            Assert.AreEqual(TokenType.INT, tokens[35].Type);
            Assert.AreEqual(">", tokens[36].Value);
            Assert.AreEqual(TokenType.GT, tokens[36].Type);
            Assert.AreEqual(1, tokens[37].Value);
            Assert.AreEqual(TokenType.INT, tokens[37].Type);
            Assert.AreEqual("-", tokens[38].Value);
            Assert.AreEqual(TokenType.SUB, tokens[38].Type);
            Assert.AreEqual(2, tokens[39].Value);
            Assert.AreEqual(TokenType.INT, tokens[39].Type);
            Assert.AreEqual("/", tokens[40].Value);
            Assert.AreEqual(TokenType.DIV, tokens[40].Type);
            Assert.AreEqual(2, tokens[41].Value);
            Assert.AreEqual(TokenType.INT, tokens[41].Type);
            Assert.AreEqual(")", tokens[42].Value);
            Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[42].Type);
            Assert.AreEqual(")", tokens[43].Value);
            Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[43].Type);
            Assert.AreEqual(string.Empty, tokens[44].Value);
            Assert.AreEqual(TokenType.EOF, tokens[44].Type);
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
                Assert.AreEqual("Expression not provided.\r\nParameter name: expression", e.Message);
            }

            try
            {
                lexer.Analyze("true # false");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual("Invalid token.", e.Message);

                var ctx = ((ParseErrorException) e).Location;
                Assert.AreEqual(1, ctx.Line);
                Assert.AreEqual(6, ctx.Column);
            }

            try
            {
                lexer.Analyze("true\r\n&& ^");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual("Invalid token.", e.Message);

                var ctx = ((ParseErrorException) e).Location;
                Assert.AreEqual(2, ctx.Line);
                Assert.AreEqual(4, ctx.Column);
            }

            try
            {
                lexer.Analyze("'John's cat'");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ParseErrorException);
                Assert.AreEqual("Invalid token.", e.Message);

                var ctx = ((ParseErrorException) e).Location;
                Assert.AreEqual(1, ctx.Line);
                Assert.AreEqual(12, ctx.Column);
            }
        }

        [TestMethod]
        public void verify_analysis_apathy_to_culture_settings()
        {
            var current = Thread.CurrentThread.CurrentCulture;

            var lexer = new Lexer();
            foreach (var temp in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {                
                Thread.CurrentThread.CurrentCulture = temp;

                // literals parsing should not vary across diverse cultures
                var tkn = lexer.Analyze("0.1").First(); // e.g. double literal should be always written using dot in our expressions language, no matter the culture
                Assert.AreEqual(TokenType.FLOAT, tkn.Type);
                Assert.AreEqual(0.1, tkn.Value);
            }

            Thread.CurrentThread.CurrentCulture = current;
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
            AssertToken(
                @"'Simon\'s cat named ""\\\\""\n (Double Backslash)'",
                @"Simon's cat named ""\\""
 (Double Backslash)", TokenType.STRING);
            AssertToken( // here, non-verbatim version, see \r\n which represents current environment new line (simply expressed by \n in our language)
                "'Simon\\\'s cat named \"\\\\\\\\\"\\n (Double Backslash)'",
                "Simon's cat named \"\\\\\"\r\n (Double Backslash)", TokenType.STRING);

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

            Assert.AreEqual(2, tokens.Length);
            Assert.AreEqual(value, tokens[0].Value);
            Assert.AreEqual(type, tokens[0].Type);
            Assert.AreEqual(TokenType.EOF, tokens[1].Type);
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
                Assert.AreEqual("Invalid token.", e.Message);
            }
        }
    }
}
