using ExpressiveAnnotations.LogicalExpressionAnalysis.SyntacticAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionAnalysis
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
            return _parser.Evaluate(_converter.Convert(expression));
        }
    }
}
