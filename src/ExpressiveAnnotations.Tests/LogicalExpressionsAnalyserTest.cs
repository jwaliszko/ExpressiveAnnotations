using System;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Analysis.LexicalAnalysis;
using ExpressiveAnnotations.Analysis.SyntacticAnalysis;
using ExpressiveAnnotations.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class LogicalExpressionsAnalyserTest
    {
        [TestMethod]
        public void Verify_tokenizer_logic()
        {
            const string expression = "( true && (true) ) || false";
            var tokenizer = new Tokenizer(new[] { @"true", @"false", @"&&", @"\|\|", @"\!", @"\(", @"\)" });

            var tokens = tokenizer.Analyze(expression);
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
                tokenizer.Analyze("true + false");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Tokenizer error. Unexpected token started at + false.");
            }

            try
            {
                tokenizer.Analyze("true && 7");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Tokenizer error. Unexpected token started at 7.");
            }
        }

        [TestMethod]
        public void Verify_infix_to_postfix_conversion()
        {
            var converter = new InfixParser();
            Assert.AreEqual(converter.Convert("()"), "");
            Assert.AreEqual(converter.Convert("( true && (true) ) || false"), "true true && false ||");
            Assert.AreEqual(converter.Convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
                "true true false true || || || true true false false true true true false || && && || || && && false && ||");
            Assert.AreEqual(converter.Convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

            try
            {
                converter.Convert("(");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert(")");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert("(( true )");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert("( true && false ))");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }
        }

        [TestMethod]
        public void Verify_postfix_parser()
        {
            var parser = new PostfixParser();

            Assert.IsTrue(parser.Evaluate("true"));
            Assert.IsTrue(!parser.Evaluate("false"));

            Assert.IsTrue(parser.Evaluate("true true &&"));
            Assert.IsTrue(!parser.Evaluate("true false &&"));
            Assert.IsTrue(!parser.Evaluate("false true &&"));
            Assert.IsTrue(!parser.Evaluate("false false &&"));

            Assert.IsTrue(parser.Evaluate("true true ||"));
            Assert.IsTrue(parser.Evaluate("true false ||"));
            Assert.IsTrue(parser.Evaluate("false true ||"));
            Assert.IsTrue(!parser.Evaluate("false false ||"));

            Assert.IsTrue(parser.Evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

            try
            {
                parser.Evaluate("(true)");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Tokenizer error. Unexpected token started at (true).");
            }

            try
            {
                parser.Evaluate(" ");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Stack empty.");
            }

            try
            {
                parser.Evaluate("");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Stack empty.");
            }

            try
            {
                parser.Evaluate(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Stack empty.");
            }
        }

        [TestMethod]
        public void Verify_complex_expression_evaluation()
        {
            var evaluator = new Evaluator();
            Assert.IsTrue(evaluator.Compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
            Assert.IsTrue(evaluator.Compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));

            try
            {
                evaluator.Compute(" ");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Logical expression computation failed. Expression is broken.");
            }

            try
            {
                evaluator.Compute("");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Logical expression computation failed. Expression is broken.");
            }

            try
            {
                evaluator.Compute(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Logical expression computation failed. Expression is broken.");
            }
        }

        [TestMethod]
        public void Verify_comparison_options()
        {
            Assert.IsTrue(Comparer.Compute("aAa", "aAa", "==", true));
            Assert.IsTrue(!Comparer.Compute("aAa", "aaa", "==", true));

            Assert.IsTrue(!Comparer.Compute("aAa", "aAa", "!=", true));
            Assert.IsTrue(Comparer.Compute("aAa", "aaa", "!=", true));

            Assert.IsTrue(Comparer.Compute("aAa", "aAa", "==", false));
            Assert.IsTrue(Comparer.Compute("aAa", "aaa", "==", false));

            Assert.IsTrue(!Comparer.Compute("aAa", "aAa", "!=", false));
            Assert.IsTrue(!Comparer.Compute("aAa", "aaa", "!=", false));
        }

        [TestMethod]
        public void Verify_comparison_equals_non_empty()
        {
            Assert.IsTrue(Comparer.Compute("aAa", "aAa", "==", true));
            Assert.IsTrue(Comparer.Compute(0, 0, "==", true));
            Assert.IsTrue(Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), "==", true));
            Assert.IsTrue(Comparer.Compute(new {}, new {}, "==", true));
            Assert.IsTrue(Comparer.Compute(new {error = true}, new {error = true}, "==", true));
            Assert.IsTrue(Comparer.Compute(new[] {"a", "b"}, new[] {"a", "b"}, "==", true));

            Assert.IsTrue(!Comparer.Compute("aAa", "aAa ", "==", true));
            Assert.IsTrue(!Comparer.Compute("aAa", " aAa ", "==", true));
            Assert.IsTrue(!Comparer.Compute("aAa", "aaa", "==", true));
            Assert.IsTrue(!Comparer.Compute(0, 1, "==", true));
            Assert.IsTrue(!Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), "==", true));
            Assert.IsTrue(!Comparer.Compute(new {error = true}, new {error = false}, "==", true));
            Assert.IsTrue(!Comparer.Compute(new[] {"a", "b"}, new[] {"a", "B"}, "==", true));
        }

        [TestMethod]
        public void Verify_comparison_equals_empty()
        {
            Assert.IsTrue(Comparer.Compute("", "", "==", true));
            Assert.IsTrue(Comparer.Compute(" ", " ", "==", true));
            Assert.IsTrue(Comparer.Compute("\t", "\n", "==", true));
            Assert.IsTrue(Comparer.Compute(null, null, "==", true));
            Assert.IsTrue(Comparer.Compute("", " ", "==", true));
            Assert.IsTrue(Comparer.Compute("\n\t ", null, "==", true));
        }

        [TestMethod]
        public void Verify_comparison_greater_and_less()
        {
            // assumption - arguments provided have exact types

            Assert.IsTrue(Comparer.Compute("a", "A", ">", true));
            Assert.IsTrue(Comparer.Compute("a", "A", ">=", true));
            Assert.IsTrue(Comparer.Compute("abcd", "ABCD", ">", true));
            Assert.IsTrue(Comparer.Compute("abcd", "ABCD", ">", true));
            Assert.IsTrue(Comparer.Compute(1, 0, ">", true));
            Assert.IsTrue(Comparer.Compute(1, 0, ">=", true));
            Assert.IsTrue(Comparer.Compute(0, -1, ">", true));
            Assert.IsTrue(Comparer.Compute(0, -1, ">=", true));
            Assert.IsTrue(Comparer.Compute(1.1, 1.01, ">", true));
            Assert.IsTrue(Comparer.Compute(1.1, 1.01, ">=", true));
            Assert.IsTrue(Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), ">", true));
            Assert.IsTrue(Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), ">=", true));

            Assert.IsTrue(!Comparer.Compute("a", null, ">", true));
            Assert.IsTrue(!Comparer.Compute("a", null, ">=", true));
            Assert.IsTrue(!Comparer.Compute(null, "a", ">", true));
            Assert.IsTrue(!Comparer.Compute(null, "a", ">=", true));

            Assert.IsTrue(!Comparer.Compute("a", "A", "<", true));
            Assert.IsTrue(!Comparer.Compute("a", "A", "<=", true));
            Assert.IsTrue(!Comparer.Compute("abcd", "ABCD", "<", true));
            Assert.IsTrue(!Comparer.Compute("abcd", "ABCD", "<=", true));
            Assert.IsTrue(!Comparer.Compute(1, 0, "<", true));
            Assert.IsTrue(!Comparer.Compute(1, 0, "<=", true));
            Assert.IsTrue(!Comparer.Compute(0, -1, "<", true));
            Assert.IsTrue(!Comparer.Compute(0, -1, "<=", true));
            Assert.IsTrue(!Comparer.Compute(1.1, 1.01, "<", true));
            Assert.IsTrue(!Comparer.Compute(1.1, 1.01, "<=", true));
            Assert.IsTrue(!Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), "<", true));
            Assert.IsTrue(!Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), "<=", true));

            Assert.IsTrue(!Comparer.Compute("a", null, "<", true));
            Assert.IsTrue(!Comparer.Compute("a", null, "<=", true));
            Assert.IsTrue(!Comparer.Compute(null, "a", "<", true));
            Assert.IsTrue(!Comparer.Compute(null, "a", "<=", true));

            try
            {
                Comparer.Compute(new {}, new {}, ">", true);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message ==
                              "Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
            }
        }

        [TestMethod]
        public void Verify_composed_error_message()
        {
            const string expression = "{0} && ( (!{1} && {2}) || ({3} && {4}) ) && {5} && {6} && {7} || {1}";
            var dependentProperties = new[] {"aaa", "bbb",  "ccc",  "ddd",  "ddd",  "eee",  "fff",  "ggg"};
            var relationalOperators = new[] {"==",  "==",   "==",   ">",    "<=",   "!=",   "!=",   "=="};
            var targetValues = new object[] {true,  "xXx",  "[yYy]",-1,     1.2,    null,   "*",    ""};

            Assert.AreEqual(
                MiscHelper.ComposeExpression(expression, dependentProperties, targetValues, relationalOperators),
                "(aaa == true) && ( (!(bbb == \"xXx\") && (ccc == [yYy])) || ((ddd > -1) && (ddd <= 1.2)) ) && (eee != null) && (fff != *) && (ggg == \"\") || (bbb == \"xXx\")");
        }

        [TestMethod]
        public void Verify_typehelper_is_empty()
        {
            object nullo = null;
            Assert.IsTrue(nullo.IsEmpty());
            Assert.IsTrue("".IsEmpty());
            Assert.IsTrue(" ".IsEmpty());
            Assert.IsTrue("\t".IsEmpty());
            Assert.IsTrue("\n".IsEmpty());
            Assert.IsTrue("\n\t ".IsEmpty());
        }

        [TestMethod]
        public void Verify_typehelper_is_numeric()
        {
            Assert.IsTrue(1.1f.IsNumeric());
            Assert.IsTrue(1.IsNumeric());
            Assert.IsTrue(!"1".IsNumeric());

            Assert.IsTrue(typeof(int).IsNumeric());
            Assert.IsTrue(typeof(int?).IsNumeric());
        }

        [TestMethod]
        public void Verify_typehelper_is_date()
        {
            Assert.IsTrue(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT").IsDateTime());
            Assert.IsTrue(!"Wed, 09 Aug 1995 00:00:00 GMT".IsDateTime());
            Assert.IsTrue(!807926400000.IsDateTime());

            Assert.IsTrue(typeof(DateTime).IsDateTime());
            Assert.IsTrue(typeof(DateTime?).IsDateTime());
        }

        [TestMethod]
        public void Verify_typehelper_is_string()
        {
            object nullo = null;
            Assert.IsTrue("".IsString());
            Assert.IsTrue("123".IsString());
            Assert.IsTrue(!123.IsString());
            Assert.IsTrue(!new {}.IsString());
            Assert.IsTrue(!nullo.IsString());
        }

        [TestMethod]
        public void Verify_typehelper_is_bool()
        {
            Assert.IsTrue(true.IsBool());
            Assert.IsTrue(!"true".IsBool());
            Assert.IsTrue(!0.IsBool());

            Assert.IsTrue(typeof(bool).IsBool());
            Assert.IsTrue(typeof(bool?).IsBool());
        }
    }
}
