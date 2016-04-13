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
        public void verify_valid_tokens_extraction()
        {
            const string expression =
                @"! || && == != < <= > >= + - * / % ~ & ^ | << >> () [] . ? :
                  null true false 123 0.3e-2 0b1010 0xFF '\'\na\n b\nc\n\'' メidメ";

            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression).ToArray();
            Assert.Equal(37, tokens.Length);
            Assert.Equal("!", tokens[0].Value);
            Assert.Equal(TokenType.L_NOT, tokens[0].Type);
            Assert.Equal("||", tokens[1].Value);
            Assert.Equal(TokenType.L_OR, tokens[1].Type);
            Assert.Equal("&&", tokens[2].Value);
            Assert.Equal(TokenType.L_AND, tokens[2].Type);
            Assert.Equal("==", tokens[3].Value);
            Assert.Equal(TokenType.EQ, tokens[3].Type);
            Assert.Equal("!=", tokens[4].Value);
            Assert.Equal(TokenType.NEQ, tokens[4].Type);
            Assert.Equal("<", tokens[5].Value);
            Assert.Equal(TokenType.LT, tokens[5].Type);
            Assert.Equal("<=", tokens[6].Value);
            Assert.Equal(TokenType.LE, tokens[6].Type);
            Assert.Equal(">", tokens[7].Value);
            Assert.Equal(TokenType.GT, tokens[7].Type);
            Assert.Equal(">=", tokens[8].Value);
            Assert.Equal(TokenType.GE, tokens[8].Type);
            Assert.Equal("+", tokens[9].Value);
            Assert.Equal(TokenType.ADD, tokens[9].Type);
            Assert.Equal("-", tokens[10].Value);
            Assert.Equal(TokenType.SUB, tokens[10].Type);
            Assert.Equal("*", tokens[11].Value);
            Assert.Equal(TokenType.MUL, tokens[11].Type);
            Assert.Equal("/", tokens[12].Value);
            Assert.Equal(TokenType.DIV, tokens[12].Type);
            Assert.Equal("%", tokens[13].Value);
            Assert.Equal(TokenType.MOD, tokens[13].Type);
            Assert.Equal("~", tokens[14].Value);
            Assert.Equal(TokenType.B_NOT, tokens[14].Type);
            Assert.Equal("&", tokens[15].Value);
            Assert.Equal(TokenType.B_AND, tokens[15].Type);
            Assert.Equal("^", tokens[16].Value);
            Assert.Equal(TokenType.XOR, tokens[16].Type);
            Assert.Equal("|", tokens[17].Value);
            Assert.Equal(TokenType.B_OR, tokens[17].Type);
            Assert.Equal("<<", tokens[18].Value);
            Assert.Equal(TokenType.L_SHIFT, tokens[18].Type);
            Assert.Equal(">>", tokens[19].Value);
            Assert.Equal(TokenType.R_SHIFT, tokens[19].Type);
            Assert.Equal("(", tokens[20].Value);
            Assert.Equal(TokenType.L_PAR, tokens[20].Type);
            Assert.Equal(")", tokens[21].Value);
            Assert.Equal(TokenType.R_PAR, tokens[21].Type);
            Assert.Equal("[", tokens[22].Value);
            Assert.Equal(TokenType.L_BRACKET, tokens[22].Type);
            Assert.Equal("]", tokens[23].Value);
            Assert.Equal(TokenType.R_BRACKET, tokens[23].Type);
            Assert.Equal(".", tokens[24].Value);
            Assert.Equal(TokenType.PERIOD, tokens[24].Type);
            Assert.Equal("?", tokens[25].Value);
            Assert.Equal(TokenType.QMARK, tokens[25].Type);
            Assert.Equal(":", tokens[26].Value);
            Assert.Equal(TokenType.COLON, tokens[26].Type);
            Assert.Equal(null, tokens[27].Value);
            Assert.Equal(TokenType.NULL, tokens[27].Type);
            Assert.Equal(true, tokens[28].Value);
            Assert.Equal(TokenType.BOOL, tokens[28].Type);
            Assert.Equal(false, tokens[29].Value);
            Assert.Equal(TokenType.BOOL, tokens[29].Type);
            Assert.Equal(123, tokens[30].Value);
            Assert.Equal(TokenType.INT, tokens[30].Type);
            Assert.Equal(0.3e-2, tokens[31].Value);
            Assert.Equal(TokenType.FLOAT, tokens[31].Type);
            Assert.Equal(10, tokens[32].Value);
            Assert.Equal(TokenType.BIN, tokens[32].Type);
            Assert.Equal(255, tokens[33].Value);
            Assert.Equal(TokenType.HEX, tokens[33].Type);
            Assert.Equal("'\r\na\r\n b\r\nc\r\n'", tokens[34].Value); // used alternatively to verbatim string (new line \n in expression string literal has been replaced by windows \r\n)
            Assert.Equal(TokenType.STRING, tokens[34].Type);
            Assert.Equal("メidメ", tokens[35].Value);
            Assert.Equal(TokenType.ID, tokens[35].Type);          
            Assert.Equal(string.Empty, tokens[36].Value);
            Assert.Equal(TokenType.EOF, tokens[36].Type);
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
            Assert.Equal("Invalid token.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => lexer.Analyze("true\r\n&& @"));
            Assert.Equal("Invalid token.", e.Error);
            Assert.Equal(new Location(2, 4), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => lexer.Analyze("'John's cat'"));
            Assert.Equal("Invalid token.", e.Error);
            Assert.Equal(new Location(1, 12), e.Location, new LocationComparer());
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
        public void verify_bin_token_extraction()
        {
            AssertToken("0b0", 0, TokenType.BIN);
            AssertToken("0b1", 1, TokenType.BIN);
            AssertToken("0b11111111", 255, TokenType.BIN);

            AssertNotToken("0b", TokenType.BIN);
            AssertNotToken("b0", TokenType.BIN);
            AssertNotToken("0b2", TokenType.INT);
        }

        [Fact]
        public void verify_hex_token_extraction()
        {
            AssertToken("0x0", 0x0, TokenType.HEX);
            AssertToken("0x01234", 0x01234, TokenType.HEX);
            AssertToken("0x56789", 0x56789, TokenType.HEX);
            AssertToken("0xFF", 0xFF, TokenType.HEX);
            AssertToken("0xff", 0xff, TokenType.HEX);
            AssertToken("0xaBcDeF", 0xaBcDeF, TokenType.HEX);

            AssertNotToken("0x", TokenType.HEX);
            AssertNotToken("xF", TokenType.HEX);
            AssertNotToken("0xG", TokenType.HEX);
        }

        [Fact]
        public void verify_float_token_extraction()
        {
            AssertToken("0e0", 0e0, TokenType.FLOAT);
            AssertToken(".2", .2, TokenType.FLOAT);
            AssertToken("3.14", 3.14, TokenType.FLOAT);
            AssertToken("5e6", 5e6, TokenType.FLOAT);
            AssertToken("5e-6", 5e-6, TokenType.FLOAT);
            AssertToken("5e+6", 5e+6, TokenType.FLOAT);
            AssertToken("9.0E-10", 9.0E-10, TokenType.FLOAT);
            AssertToken(".11e10", .11e10, TokenType.FLOAT);

            AssertNotToken("1.1.1", TokenType.FLOAT);
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
        public void verify_id_token_extraction()
        {
            AssertToken("_", "_", TokenType.ID);
            AssertToken("__", "__", TokenType.ID);
            AssertToken("a", "a", TokenType.ID);
            AssertToken("asd", "asd", TokenType.ID);
            AssertToken("a_a", "a_a", TokenType.ID);
            AssertToken("A", "A", TokenType.ID);
            AssertToken("a1", "a1", TokenType.ID);
            AssertToken("_123", "_123", TokenType.ID);
            AssertToken("メ", "メ", TokenType.ID);

            AssertNotToken("1", TokenType.ID);
            AssertNotToken("1.1", TokenType.ID);
            AssertNotToken("a.", TokenType.ID);
            AssertNotToken("a..a", TokenType.ID);
            AssertNotToken("a.1", TokenType.ID);
            AssertNotToken("foo()", TokenType.ID); // brackets are not part of the token
            AssertNotToken("[]", TokenType.ID);
            AssertNotToken("[0]", TokenType.ID);
            AssertNotToken("_[0].1", TokenType.ID);
            AssertNotToken("_[0]_", TokenType.ID);
            AssertNotToken("_[0][0]", TokenType.ID);
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
                Assert.EndsWith("Invalid token.", e.Message);
            }
        }
    }
}
