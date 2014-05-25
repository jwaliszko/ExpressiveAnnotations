using System;
using System.Collections.Generic;
using System.Linq;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis
{
    internal class InfixToPostfixConverter
    {
        private readonly InfixLexer _infixLexer;

        public InfixToPostfixConverter()
        {
            _infixLexer = new InfixLexer();
        }

        private bool IsInfixOperator(string token)
        {
            var op = new[] {Token.AND, Token.OR, Token.NOT, Token.LEFT, Token.RIGHT};
            return op.Contains(token);
        }

        private bool IsPostfixOperator(string token)
        {
            var op = new[] {Token.AND, Token.OR, Token.NOT};
            return op.Contains(token);
        }

        private bool IsUnaryOperator(string token)
        {
            var op = new[] {Token.NOT};
            return op.Contains(token);
        }

        private bool IsLeftBracket(string token)
        {
            return Token.LEFT.Equals(token);
        }

        private bool IsRightBracket(string token)
        {
            return Token.RIGHT.Equals(token);
        }

        private bool ContainsLeftBracket(Stack<string> st)
        {
            return st.Contains(Token.LEFT);
        }

        public string Convert(string expression)
        {
            var tokens = _infixLexer.Analyze(expression, true);
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