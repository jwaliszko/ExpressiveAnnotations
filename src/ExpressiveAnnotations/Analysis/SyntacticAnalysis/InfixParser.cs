using System;
using System.Collections.Generic;
using System.Linq;
using ExpressiveAnnotations.Analysis.LexicalAnalysis;

namespace ExpressiveAnnotations.Analysis.SyntacticAnalysis
{
    /// <summary>
    /// Performs syntactic analysis of provided boolean expression given in infix notation.
    /// </summary>
    internal class InfixParser
    {
        private Tokenizer InfixTokenizer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfixParser" /> class.
        /// </summary>
        public InfixParser()
        {
            InfixTokenizer = new Tokenizer(new[] {@"true", @"false", @"&&", @"\|\|", @"\!", @"\(", @"\)"});
        }

        private bool IsInfixOperator(string token)
        {
            return new[] {"&&", "||", "!", "(", ")"}.Contains(token);
        }

        private bool IsPostfixOperator(string token)
        {
            return new[] {"&&", "||", "!"}.Contains(token);
        }

        private bool IsUnaryOperator(string token)
        {
            return "!".Equals(token);
        }

        private bool IsLeftBracket(string token)
        {
            return "(".Equals(token);
        }

        private bool IsRightBracket(string token)
        {
            return ")".Equals(token);
        }

        private bool ContainsLeftBracket(Stack<string> st)
        {
            return st.Contains("(");
        }

        /// <summary>
        /// Converts the specified boolean expression given in infix form into postfix one.
        /// </summary>
        /// <param name="expression">The boolean expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Infix expression parsing error. Incorrect nesting.
        /// </exception>
        public string Convert(string expression)
        {
            var tokens = InfixTokenizer.Analyze(expression);
            var operators = new Stack<string>();
            var output = new Stack<string>();

            foreach (var token in tokens)
            {
                if (IsInfixOperator(token))
                {
                    if (IsRightBracket(token))
                    {
                        if (!ContainsLeftBracket(operators))
                            throw new ArgumentException("Infix expression parsing error. Incorrect nesting.");

                        PopNestedOperators(operators, output);
                        PopCorrespondingUnaryOperators(operators, output);
                    }
                    else
                        operators.Push(token);
                }
                else
                {
                    output.Push(token);
                    PopCorrespondingUnaryOperators(operators, output);
                }
            }

            if (operators.Count > 0 && ContainsLeftBracket(operators))
                throw new ArgumentException("Infix expression parsing error. Incorrect nesting.");

            PopRemainingOperators(operators, output);
            return string.Join(" ", output.Reverse());
        }

        private void PopNestedOperators(Stack<string> operators, Stack<string> output)
        {
            var length = operators.Count;
            for (var i = 0; i < length; i++)
            {
                var top = operators.Pop();
                if (IsPostfixOperator(top))
                    output.Push(top);
                if (IsLeftBracket(top))
                    break;
            }
        }

        private void PopCorrespondingUnaryOperators(Stack<string> operators, Stack<string> output)
        {
            var length = operators.Count;
            for (var i = 0; i < length; i++)
            {
                var top = operators.Peek();
                if (IsUnaryOperator(top) && IsPostfixOperator(top))
                {
                    top = operators.Pop();
                    output.Push(top);
                }
                else
                    break;
            }
        }

        private void PopRemainingOperators(Stack<string> operators, Stack<string> output)
        {
            var length = operators.Count;
            for (var i = 0; i < length; i++)
            {
                var top = operators.Pop();
                if (IsPostfixOperator(top))
                    output.Push(top);
            }
        }
    }
}
