using System;
using ExpressiveAnnotations.Misc;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.LogicalExpressionsAnalysis
{
    public static class Comparer
    {
        public static bool Compute(object dependentValue, object targetValue, string relationalOperator)
        {
            switch (relationalOperator)
            {
                case "==":
                    return Compare(dependentValue, targetValue);
                case "!=":
                    return !Compare(dependentValue, targetValue);
                case ">":
                    return Greater(dependentValue, targetValue);
                case ">=":
                    return !Less(dependentValue, targetValue);
                case "<":
                    return Less(dependentValue, targetValue);
                case "<=":
                    return !Greater(dependentValue, targetValue);
            }

            throw new ArgumentException(string.Format("Relational operator \"{0}\" is invalid. Available operators: ==, !=, >, >=, <, <=.", relationalOperator));
        }

        public static bool Compare(object dependentValue, object targetValue)
        {
            if (dependentValue.IsEmpty() && targetValue.IsEmpty())
                return true;
            return JsonConvert.SerializeObject(dependentValue) == JsonConvert.SerializeObject(targetValue)
                   || (!dependentValue.IsEmpty() && string.Equals(targetValue as string, "*"));
        }

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
