using System;
using ExpressiveAnnotations.Misc;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis
{
    /// <summary>
    /// Type which computes relational operations results.
    /// </summary>
    public static class Comparer
    {
        /// <summary>
        /// Computes relation result.
        /// </summary>
        /// <param name="dependentValue">The dependent value.</param>
        /// <param name="targetValue">The target value.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static bool Compute(object dependentValue, object targetValue, string relationalOperator)
        {
            switch (relationalOperator)
            {
                case "==":
                    return Equal(dependentValue, targetValue);
                case "!=":
                    return !Equal(dependentValue, targetValue);
                case ">":
                    return Greater(dependentValue, targetValue);
                case ">=":
                    return Greater(dependentValue, targetValue) || Equal(dependentValue, targetValue);
                case "<":
                    return Less(dependentValue, targetValue);
                case "<=":
                    return Less(dependentValue, targetValue) || Equal(dependentValue, targetValue);
            }

            throw new ArgumentException(string.Format("Relational operator \"{0}\" is invalid. Available operators: ==, !=, >, >=, <, <=.", relationalOperator));
        }

        /// <summary>
        /// Computes equality comparison.
        /// </summary>
        /// <param name="dependentValue">The dependent value.</param>
        /// <param name="targetValue">The target value.</param>
        /// <returns></returns>
        public static bool Equal(object dependentValue, object targetValue)
        {
            if (dependentValue.IsEmpty() && targetValue.IsEmpty())
                return true;
            if(!dependentValue.IsEmpty() && string.Equals(targetValue as string, "*"))
                return true;
            return JsonConvert.SerializeObject(dependentValue) == JsonConvert.SerializeObject(targetValue);
        }

        /// <summary>
        /// Computes "greater than" comparison.
        /// </summary>
        /// <param name="dependentValue">The dependent value.</param>
        /// <param name="targetValue">The target value.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.</exception>
        public static bool Greater(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) > Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) > Convert.ToDateTime(targetValue);
            if (dependentValue is string && targetValue is string)
                return string.CompareOrdinal(dependentValue as string, targetValue as string) > 0;
            if (dependentValue == null || targetValue == null)
                return false;

            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
        }

        /// <summary>
        /// Computes "less than" comparison.
        /// </summary>
        /// <param name="dependentValue">The dependent value.</param>
        /// <param name="targetValue">The target value.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.</exception>
        public static bool Less(object dependentValue, object targetValue)
        {
            if (dependentValue.IsNumeric() && targetValue.IsNumeric())
                return Convert.ToDouble(dependentValue) < Convert.ToDouble(targetValue);
            if (dependentValue.IsDateTime() && targetValue.IsDateTime())
                return Convert.ToDateTime(dependentValue) < Convert.ToDateTime(targetValue);
            if (dependentValue is string && targetValue is string)
                return string.CompareOrdinal(dependentValue as string, targetValue as string) < 0;
            if (dependentValue == null || targetValue == null)
                return false;

            throw new InvalidOperationException("Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
        }
    }
}
