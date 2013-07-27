using System;
using System.Collections.Generic;
using ExpressiveAnnotations.BooleanExpressionsAnalyser.LexicalAnalysis;

namespace ExpressiveAnnotations.BooleanExpressionsAnalyser.SyntacticAnalysis
{
    public class RpnParser
    {
        private readonly RpnLexer _rpnLexer;

        public RpnParser()
        {
            _rpnLexer = new RpnLexer();
        }

        public bool Evaluate(string expression)
        {
            var st = new Stack<string>(_rpnLexer.Analyze(expression, true));
            var result = Evaluate(st);
            if (st.Count != 0)
            {
                throw new ArgumentException("RPN expression parsing error. Incorrect nesting.");
            }
            return result;
        }

        private bool Evaluate(Stack<string> st)
        {
            var top = st.Pop();
            bool x;
            if (!bool.TryParse(top, out x))
            {
                var y = Evaluate(st);
                if (top == "!")
                {
                    return !y;
                }
                x = Evaluate(st);

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
                            string.Format("RPN expression parsing error. Token \"{0}\" not expected.", top));
                }
            }

            return x;
        }
    }
}