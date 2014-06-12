using System;
using System.Collections.Generic;
using System.Linq;
using ExpressiveAnnotations.Analysis.LexicalAnalysis;

namespace ExpressiveAnnotations.Analysis.SyntacticAnalysis
{
    /// <summary>
    /// Performs syntactic analysis of provided boolean expression given in postfix notation.
    /// </summary>
    internal class PostfixParser
    {
        private Tokenizer PostfixTokenizer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostfixParser" /> class.
        /// </summary>
        public PostfixParser()
        {
            PostfixTokenizer = new Tokenizer(new[] {@"true", @"false", @"&&", @"\|\|", @"\!"});
        }

        /// <summary>
        /// Evaluates the specified boolean expression.
        /// </summary>
        /// <param name="expression">The boolean expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">RPN expression parsing error. Incorrect nesting.</exception>
        public bool Evaluate(string expression)
        {
            var st = new Stack<string>(PostfixTokenizer.Analyze(expression));
            var result = Evaluate(st);
            if (st.Any())
                throw new ArgumentException("RPN expression parsing error. Incorrect nesting.");
            return result;
        }

        private bool Evaluate(Stack<string> st)
        {
            var top = st.Pop();
            if (new[] {"true", "false"}.Contains(top))
                return bool.Parse(top);

            var y = Evaluate(st);
            if (top == "!")
                return !y;

            var x = Evaluate(st);
            switch (top)
            {
                case "&&":
                    x = x && y;
                    break;
                case "||":
                    x = x || y;
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("RPN expression parsing error. Token {0} not expected.", top));
            }

            return x;
        }
    }
}
