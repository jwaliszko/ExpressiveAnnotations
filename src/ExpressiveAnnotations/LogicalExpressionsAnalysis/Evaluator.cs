using System;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis
{
    /// <summary>
    /// Type which computes the specified logical expression.
    /// </summary>
    public class Evaluator
    {
        private readonly InfixToPostfixConverter _converter;
        private readonly PostfixParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        public Evaluator()
        {
            _converter = new InfixToPostfixConverter();
            _parser = new PostfixParser();
        }

        /// <summary>
        /// Computes the specified logical expression.
        /// </summary>
        /// <param name="expression">The logical expression.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Logical expression computation failed. Expression is broken.</exception>
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
