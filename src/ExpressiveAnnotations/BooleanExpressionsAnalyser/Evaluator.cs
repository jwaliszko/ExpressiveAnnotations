using ExpressiveAnnotations.BooleanExpressionsAnalyser.SyntacticAnalysis;

namespace ExpressiveAnnotations.BooleanExpressionsAnalyser
{
    public class Evaluator
    {
        private readonly InfixToRpnConverter _converter;
        private readonly RpnParser _parser;

        public Evaluator()
        {
            _converter = new InfixToRpnConverter();
            _parser = new RpnParser();
        }

        public bool Compute(string expression)
        {
            return _parser.Evaluate(_converter.Convert(expression));
        }
    }
}
