using System;
using System.Collections.Generic;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis
{
    internal class PostfixParser
    {
        private Tokenizer PostfixTokenizer { get; set; }

        public PostfixParser()
        {
            PostfixTokenizer = new Tokenizer(new[] {@"true", @"false", @"&&", @"\|\|", @"\!"});
        }

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
                    x &= y;
                    break;
                case "||":
                    x |= y;
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("RPN expression parsing error. Token {0} not expected.", top));
            }
            return x;
        }
    }
}
