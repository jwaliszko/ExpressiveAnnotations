using System;
using System.Collections.Generic;
using ExpressiveAnnotations.LogicalExpressionAnalysis.LexicalAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionAnalysis.SyntacticAnalysis
{
    public class PostfixParser
    {
        private readonly PostfixLexer _postfixLexer;

        public PostfixParser()
        {
            _postfixLexer = new PostfixLexer();
        }

        public bool Evaluate(string expression)
        {
            var st = new Stack<string>(_postfixLexer.Analyze(expression, true));
            var result = Evaluate(st);
            if (st.Count != 0)
                throw new ArgumentException("RPN expression parsing error. Incorrect nesting.");
            return result;
        }

        private bool Evaluate(Stack<string> st)
        {
            var top = st.Pop();

            bool x;
            if (bool.TryParse(top, out x)) 
                return x;

            var y = Evaluate(st);
            if (top == Token.NOT)
                return !y;

            x = Evaluate(st);

            switch (top)
            {
                case Token.AND:
                    x &= y;
                    break;
                case Token.OR:
                    x |= y;
                    break;
                default:
                    throw new ArgumentException(
                        string.Format("RPN expression parsing error. Token \"{0}\" not expected.", top));
            }
            return x;
        }
    }
}