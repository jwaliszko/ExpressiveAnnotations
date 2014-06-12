using System;
using ExpressiveAnnotations.Analysis.SyntacticAnalysis;

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Type which computes the specified boolean expression.
    /// </summary>
    public class Evaluator
    {
        private readonly InfixParser _infoxParser;
        private readonly PostfixParser _postfixParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator" /> class.
        /// </summary>
        public Evaluator()
        {
            _infoxParser = new InfixParser();
            _postfixParser = new PostfixParser();
        }

        /// <summary>
        /// Computes the specified boolean expression provided in infix notation.
        /// </summary>
        /// <param name="expression">The boolean expression.</param>
        /// <returns>Computated result.</returns>
        /// <exception cref="System.InvalidOperationException">Logical expression computation failed. Expression is broken.</exception>
        public bool Compute(string expression)
        {
            try
            {
                var postfixExpression = _infoxParser.Convert(expression);
                return _postfixParser.Evaluate(postfixExpression);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Logical expression computation failed. Expression is broken.", e);
            }
        }
    }
}
