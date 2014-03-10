using System;
using ExpressiveAnnotations.BooleanExpressionAnalysis;
using ExpressiveAnnotations.BooleanExpressionAnalysis.LexicalAnalysis;
using ExpressiveAnnotations.BooleanExpressionAnalysis.SyntacticAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class BooleanExpressionsAnalyserUnitTest
    {
        [TestMethod]
        public void Verify_infix_lexer_logic()
        {
            const string expression = "( true && (true) ) || false";
            var lexer = new InfixLexer();

            var tokens = lexer.Analyze(expression, false);
            Assert.AreEqual(tokens.Length, 15);
            Assert.AreEqual(tokens[0], "(");
            Assert.AreEqual(tokens[1], " ");
            Assert.AreEqual(tokens[2], "true");
            Assert.AreEqual(tokens[3], " ");
            Assert.AreEqual(tokens[4], "&&");
            Assert.AreEqual(tokens[5], " ");
            Assert.AreEqual(tokens[6], "(");
            Assert.AreEqual(tokens[7], "true");
            Assert.AreEqual(tokens[8], ")");
            Assert.AreEqual(tokens[9], " ");
            Assert.AreEqual(tokens[10], ")");
            Assert.AreEqual(tokens[11], " ");
            Assert.AreEqual(tokens[12], "||");
            Assert.AreEqual(tokens[13], " ");
            Assert.AreEqual(tokens[14], "false");

            tokens = lexer.Analyze(expression, true);
            Assert.AreEqual(tokens.Length, 9);
            Assert.AreEqual(tokens[0], "(");
            Assert.AreEqual(tokens[1], "true");
            Assert.AreEqual(tokens[2], "&&");
            Assert.AreEqual(tokens[3], "(");
            Assert.AreEqual(tokens[4], "true");
            Assert.AreEqual(tokens[5], ")");
            Assert.AreEqual(tokens[6], ")");
            Assert.AreEqual(tokens[7], "||");
            Assert.AreEqual(tokens[8], "false");

            try
            {
                lexer.Analyze("true + false", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            try
            {
                lexer.Analyze("true && 7", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [TestMethod]
        public void Verify_postfix_lexer_logic()
        {
            const string expression = "true true && false ||";
            var lexer = new PostfixLexer();

            var tokens = lexer.Analyze(expression, false);
            Assert.AreEqual(tokens.Length, 9);
            Assert.AreEqual(tokens[0], "true");
            Assert.AreEqual(tokens[1], " ");
            Assert.AreEqual(tokens[2], "true");
            Assert.AreEqual(tokens[3], " ");
            Assert.AreEqual(tokens[4], "&&");
            Assert.AreEqual(tokens[5], " ");
            Assert.AreEqual(tokens[6], "false");
            Assert.AreEqual(tokens[7], " ");
            Assert.AreEqual(tokens[8], "||");

            tokens = lexer.Analyze(expression, true);
            Assert.AreEqual(tokens.Length, 5);
            Assert.AreEqual(tokens[0], "true");
            Assert.AreEqual(tokens[1], "true");
            Assert.AreEqual(tokens[2], "&&");
            Assert.AreEqual(tokens[3], "false");
            Assert.AreEqual(tokens[4], "||");

            try
            {
                lexer.Analyze("true && (false)", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            try
            {
                lexer.Analyze("true + 7", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [TestMethod]
        public void Verify_infix_to_postfix_conversion()
        {
            var converter = new InfixToPostfixConverter();
            Assert.AreEqual(converter.Convert("()"), "");
            Assert.AreEqual(converter.Convert("( true && (true) ) || false"), "true true && false ||");
            Assert.AreEqual(converter.Convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
                "true true false true || || || true true false false true true true false || && && || || && && false && ||");
            Assert.AreEqual(converter.Convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

            try
            {
                converter.Convert("(");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            try
            {
                converter.Convert(")");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            try
            {
                converter.Convert("(( true )");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            try
            {
                converter.Convert("( true && false ))");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [TestMethod]
        public void Verify_postfix_parser()
        {
            var parser = new PostfixParser();

            Assert.IsTrue(parser.Evaluate("true"));
            Assert.IsFalse(parser.Evaluate("false"));

            Assert.IsTrue(parser.Evaluate("true true &&"));
            Assert.IsFalse(parser.Evaluate("true false &&"));
            Assert.IsFalse(parser.Evaluate("false true &&"));
            Assert.IsFalse(parser.Evaluate("false false &&"));

            Assert.IsTrue(parser.Evaluate("true true ||"));
            Assert.IsTrue(parser.Evaluate("true false ||"));
            Assert.IsTrue(parser.Evaluate("false true ||"));
            Assert.IsFalse(parser.Evaluate("false false ||"));

            Assert.IsTrue(parser.Evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

            try
            {
                Assert.IsTrue(parser.Evaluate("(true)"));
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [TestMethod]
        public void Verify_complex_expression_evaluation()
        {
            var evaluator = new Evaluator();
            Assert.IsTrue(evaluator.Compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
            Assert.IsTrue(evaluator.Compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));
        }
    }
}
