using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using ExpressiveAnnotations.Analysis;
using Xunit;

namespace ExpressiveAnnotations.Tests
{
    public class LexerTest
    {
        [Fact]
        public void verify_empty_expression_analysis()
        {
            var lexer = new Lexer();
            var tokens = lexer.Analyze(string.Empty).ToArray();
            Assert.Equal(1, tokens.Length);
            Assert.Equal(string.Empty, tokens[0].Value);
            Assert.Equal(TokenType.EOF, tokens[0].Type);
        }

        [Fact]
        public void verify_complex_expression_analysis()
        {
            const string expression =
                @"GoAbroad == true
                      && (
                             (NextCountry != 'european country' && Compare(NextCountry, Country.Name) == 0)
                             || (Age > 24 && Age <= 55.5)

                             &&(1.1+2*2>1-2/2+Array[0].Value=='\'\na\n b\nc\n\'')
                         )";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();
            Assert.Equal(49, tokens.Length);
            Assert.Equal("GoAbroad", tokens[0].Value);
            Assert.Equal(TokenType.FUNC, tokens[0].Type);
            Assert.Equal("==", tokens[1].Value);
            Assert.Equal(TokenType.EQ, tokens[1].Type);
            Assert.Equal(true, tokens[2].Value);
            Assert.Equal(TokenType.BOOL, tokens[2].Type);
            Assert.Equal("&&", tokens[3].Value);
            Assert.Equal(TokenType.AND, tokens[3].Type);
            Assert.Equal("(", tokens[4].Value);
            Assert.Equal(TokenType.LEFT_BRACKET, tokens[4].Type);
            Assert.Equal("(", tokens[5].Value);
            Assert.Equal(TokenType.LEFT_BRACKET, tokens[5].Type);
            Assert.Equal("NextCountry", tokens[6].Value);
            Assert.Equal(TokenType.FUNC, tokens[6].Type);
            Assert.Equal("!=", tokens[7].Value);
            Assert.Equal(TokenType.NEQ, tokens[7].Type);
            Assert.Equal("european country", tokens[8].Value);
            Assert.Equal(TokenType.STRING, tokens[8].Type);
            Assert.Equal("&&", tokens[9].Value);
            Assert.Equal(TokenType.AND, tokens[9].Type);
            Assert.Equal("Compare", tokens[10].Value);
            Assert.Equal(TokenType.FUNC, tokens[10].Type);
            Assert.Equal("(", tokens[11].Value);
            Assert.Equal(TokenType.LEFT_BRACKET, tokens[11].Type);
            Assert.Equal("NextCountry", tokens[12].Value);
            Assert.Equal(TokenType.FUNC, tokens[12].Type);
            Assert.Equal(",", tokens[13].Value);
            Assert.Equal(TokenType.COMMA, tokens[13].Type);
            Assert.Equal("Country.Name", tokens[14].Value);
            Assert.Equal(TokenType.FUNC, tokens[14].Type);
            Assert.Equal(")", tokens[15].Value);
            Assert.Equal(TokenType.RIGHT_BRACKET, tokens[15].Type);
            Assert.Equal("==", tokens[16].Value);
            Assert.Equal(TokenType.EQ, tokens[16].Type);
            Assert.Equal(0, tokens[17].Value);
            Assert.Equal(TokenType.INT, tokens[17].Type);
            Assert.Equal(")", tokens[18].Value);
            Assert.Equal(TokenType.RIGHT_BRACKET, tokens[18].Type);
            Assert.Equal("||", tokens[19].Value);
            Assert.Equal(TokenType.OR, tokens[19].Type);
            Assert.Equal("(", tokens[20].Value);
            Assert.Equal(TokenType.LEFT_BRACKET, tokens[20].Type);
            Assert.Equal("Age", tokens[21].Value);
            Assert.Equal(TokenType.FUNC, tokens[21].Type);
            Assert.Equal(">", tokens[22].Value);
            Assert.Equal(TokenType.GT, tokens[22].Type);
            Assert.Equal(24, tokens[23].Value);
            Assert.Equal(TokenType.INT, tokens[23].Type);
            Assert.Equal("&&", tokens[24].Value);
            Assert.Equal(TokenType.AND, tokens[24].Type);
            Assert.Equal("Age", tokens[25].Value);
            Assert.Equal(TokenType.FUNC, tokens[25].Type);
            Assert.Equal("<=", tokens[26].Value);
            Assert.Equal(TokenType.LE, tokens[26].Type);
            Assert.Equal(55.5d, tokens[27].Value);
            Assert.Equal(TokenType.FLOAT, tokens[27].Type);
            Assert.Equal(")", tokens[28].Value);
            Assert.Equal(TokenType.RIGHT_BRACKET, tokens[28].Type);
            Assert.Equal("&&", tokens[29].Value);
            Assert.Equal(TokenType.AND, tokens[29].Type);
            Assert.Equal("(", tokens[30].Value);
            Assert.Equal(TokenType.LEFT_BRACKET, tokens[30].Type);
            Assert.Equal(1.1, tokens[31].Value);
            Assert.Equal(TokenType.FLOAT, tokens[31].Type);
            Assert.Equal("+", tokens[32].Value);
            Assert.Equal(TokenType.ADD, tokens[32].Type);
            Assert.Equal(2, tokens[33].Value);
            Assert.Equal(TokenType.INT, tokens[33].Type);
            Assert.Equal("*", tokens[34].Value);
            Assert.Equal(TokenType.MUL, tokens[34].Type);
            Assert.Equal(2, tokens[35].Value);
            Assert.Equal(TokenType.INT, tokens[35].Type);
            Assert.Equal(">", tokens[36].Value);
            Assert.Equal(TokenType.GT, tokens[36].Type);
            Assert.Equal(1, tokens[37].Value);
            Assert.Equal(TokenType.INT, tokens[37].Type);
            Assert.Equal("-", tokens[38].Value);
            Assert.Equal(TokenType.SUB, tokens[38].Type);
            Assert.Equal(2, tokens[39].Value);
            Assert.Equal(TokenType.INT, tokens[39].Type);
            Assert.Equal("/", tokens[40].Value);
            Assert.Equal(TokenType.DIV, tokens[40].Type);
            Assert.Equal(2, tokens[41].Value);
            Assert.Equal(TokenType.INT, tokens[41].Type);
            Assert.Equal("+", tokens[42].Value);
            Assert.Equal(TokenType.ADD, tokens[42].Type);
            Assert.Equal("Array[0].Value", tokens[43].Value);
            Assert.Equal(TokenType.FUNC, tokens[43].Type);
            Assert.Equal("==", tokens[44].Value);
            Assert.Equal(TokenType.EQ, tokens[44].Type);
            Assert.Equal("'\r\na\r\n b\r\nc\r\n'", tokens[45].Value); // used alternatively to verbatim string (new line \n in expression has been replaced by windows \r\n)
            Assert.Equal(TokenType.STRING, tokens[45].Type);
            Assert.Equal(")", tokens[46].Value);
            Assert.Equal(TokenType.RIGHT_BRACKET, tokens[46].Type);
            Assert.Equal(")", tokens[47].Value);
            Assert.Equal(TokenType.RIGHT_BRACKET, tokens[47].Type);
            Assert.Equal(string.Empty, tokens[48].Value);
            Assert.Equal(TokenType.EOF, tokens[48].Type);
        }

        [Fact]
        public void verify_lexer_analyze_invalid_parameter()
        {
            var lexer = new Lexer();
            var e = Assert.Throws<ArgumentNullException>(() => lexer.Analyze(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);
        }

        [Fact]
        public void verify_invalid_tokens_detection()
        {
            var lexer = new Lexer();

            var e = Assert.Throws<ParseErrorException>(() => lexer.Analyze("true # false"));
            Assert.Equal("Invalid token.", e.Message);
            Assert.Equal(1, e.Location.Line);
            Assert.Equal(6, e.Location.Column);

            e = Assert.Throws<ParseErrorException>(() => lexer.Analyze("true\r\n&& ^"));
            Assert.Equal("Invalid token.", e.Message);
            Assert.Equal(2, e.Location.Line);
            Assert.Equal(4, e.Location.Column);

            e = Assert.Throws<ParseErrorException>(() => lexer.Analyze("'John's cat'"));
            Assert.Equal("Invalid token.", e.Message);
            Assert.Equal(1, e.Location.Line);
            Assert.Equal(12, e.Location.Column);
        }

        [Fact]
        public void verify_analysis_apathy_to_culture_settings()
        {
            var current = Thread.CurrentThread.CurrentCulture;

            var lexer = new Lexer();
            foreach (var temp in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {                
                Thread.CurrentThread.CurrentCulture = temp;

                // literals parsing should not vary across diverse cultures
                var tkn = lexer.Analyze("0.1").First(); // e.g. double literal should be always written using dot in our expressions language, no matter the culture
                Assert.Equal(TokenType.FLOAT, tkn.Type);
                Assert.Equal(0.1, tkn.Value);
            }

            Thread.CurrentThread.CurrentCulture = current;
        }

        [Fact]
        public void verify_float_token_extraction()
        {
            AssertToken("1.0", 1.0, TokenType.FLOAT);
            AssertToken("0.3e-2", 0.3e-2, TokenType.FLOAT);

            AssertNotToken("1", TokenType.FLOAT);
            AssertNotToken("a", TokenType.FLOAT);
        }

        [Fact]
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

        [Fact]
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
            AssertToken("arr[0]", "arr[0]", TokenType.FUNC);
            AssertToken("a[0]._", "a[0]._", TokenType.FUNC);
            AssertToken("a[0].a", "a[0].a", TokenType.FUNC);
            AssertToken("a[0].a1", "a[0].a1", TokenType.FUNC);
            AssertToken("__[0].__[1]", "__[0].__[1]", TokenType.FUNC);
            AssertToken("_[0].a[1].a1[2]", "_[0].a[1].a1[2]", TokenType.FUNC);

            AssertNotToken("1", TokenType.FUNC);
            AssertNotToken("a.", TokenType.FUNC);
            AssertNotToken("a..a", TokenType.FUNC);
            AssertNotToken("a.1", TokenType.FUNC);
            AssertNotToken("a.+", TokenType.FUNC);
            AssertNotToken("foo()", TokenType.FUNC); // brackets are not part of the token
            AssertNotToken("[]", TokenType.FUNC);
            AssertNotToken("[0]", TokenType.FUNC);
            AssertNotToken("_[0].1", TokenType.FUNC);
            AssertNotToken("_[0]_", TokenType.FUNC);
            AssertNotToken("_[0][0]", TokenType.FUNC);
        }

        private static void AssertToken(string expression, object value, TokenType type)
        {
            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();

            Assert.Equal(2, tokens.Length); // tested token + EOF token
            Assert.Equal(value, tokens[0].Value);
            Assert.Equal(type, tokens[0].Type);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        private static void AssertNotToken(string expression, TokenType type)
        {
            try
            {
                var lexer = new Lexer();
                var tokens = lexer.Analyze(expression).ToArray();

                var recognized = tokens.Length == 2 && tokens[0].Type == type && tokens[1].Type == TokenType.EOF;
                Assert.False(recognized);
            }
            catch (ParseErrorException e)
            {
                Assert.Equal("Invalid token.", e.Message);
            }
        }
    }
}
