using System;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis
{
    public class Evaluator
    {
        private readonly InfixToPostfixConverter _converter;
        private readonly PostfixParser _parser;

        public Evaluator()
        {
            _converter = new InfixToPostfixConverter();
            _parser = new PostfixParser();
        }

        public bool Compute(string expression)
        {
            try
            {
                return _parser.Evaluate(_converter.Convert(expression));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Logical expression computation failed. Expression is broken.", e);
            }
        }
    }
}
