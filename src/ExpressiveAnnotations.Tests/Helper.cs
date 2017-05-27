using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.Tests
{
    internal static class Helper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(ExpressiveAttribute)));
            var attributes = new List<ExpressiveAttribute>();

            foreach (var prop in properties)
            {
                var attribs = prop.GetCustomAttributes<ExpressiveAttribute>().ToList();
                attribs.ForEach(x => x.Compile(prop.DeclaringType));
                attributes.AddRange(attribs);
            }
            return attributes;
        }

        public static bool ArrayDeepEqual(this object a1, object a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;
            if (a1 == null || a2 == null)
                return false;
            if (a1.GetType() != a2.GetType())
                return false;
            if (!a1.GetType().IsArray)
                return a1.Equals(a2);

            var arr1 = (Array)a1;
            var arr2 = (Array)a2;

            if (arr1.Length != arr2.Length)
                return false;

            for (var i = 0; i < arr1.Length; i++)
            {
                var i1 = arr1.GetValue(i);
                var i2 = arr2.GetValue(i);
                if (!i1.ArrayDeepEqual(i2))
                    return false;
            }
            return true;
        }

        public static string PrefixPrint(this Expression exp)
        {
            var sb = new StringBuilder();
            exp.PrefixPrintToBuffer(sb);
            return sb.ToString();
        }

        private static void PrefixPrintToBuffer(this Expression exp, StringBuilder sb)
        {
            UnaryExpression ue;
            BinaryExpression be;
            ConditionalExpression ce;
            ConstantExpression conste;
            MemberExpression me;
            MethodCallExpression mce;
            NewArrayExpression nae;
            ParameterExpression pe;

            sb.Append("(");

            if ((ue = exp as UnaryExpression) != null)
            {
                sb.Append(ue.NodeType.Symbol());
                sb.Append(ue.Operand.PrefixPrint());
            }
            else if ((be = exp as BinaryExpression) != null)
            {
                if (exp.NodeType == ExpressionType.ArrayIndex)
                {
                    sb.Append("Index[");
                    sb.Append(be.Left.PrefixPrint());
                    sb.Append(",");
                    sb.Append(be.Right.PrefixPrint());
                    sb.Append("]");
                }
                else
                {
                    sb.Append(be.NodeType.Symbol());
                    sb.Append(be.Left.PrefixPrint());
                    sb.Append(be.Right.PrefixPrint());
                }
            }
            else if ((ce = exp as ConditionalExpression) != null)
            {
                sb.Append("?");
                sb.Append(ce.Test.PrefixPrint());
                sb.Append(":");
                sb.Append(ce.IfTrue.PrefixPrint());
                sb.Append(ce.IfFalse.PrefixPrint());
            }
            else if ((me = exp as MemberExpression) != null)
            {
                sb.Append("Prop[");
                sb.Append(me.Expression.PrefixPrint());
                sb.Append(",");
                sb.Append(me.Member.Name);
                sb.Append("]");
            }
            else if ((mce = exp as MethodCallExpression) != null)
            {
                sb.Append(mce.Method.Name);
                sb.Append($"({string.Join(",", mce.Arguments.Select(arg => arg.PrefixPrint()))})");
            }
            else if ((nae = exp as NewArrayExpression) != null)
            {
                sb.Append($"[{string.Join(",", nae.Expressions.Select(arg => arg.PrefixPrint()))}]");
            }
            else if ((pe = exp as ParameterExpression) != null)
            {
                sb.Append($"<{pe.Type.Name}>");
            }
            else if ((conste = exp as ConstantExpression) != null)
            {
                var val = conste.Value.ToString();
                if (conste.Type == typeof(double))
                    val = ((double) conste.Value).ToString(CultureInfo.InvariantCulture);
                sb.Append(val);
            }
            else
            {
                throw new NotImplementedException("Expression printout undefined.");
            }

            sb.Append(")");
        }

        private static string Symbol(this ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.OrElse:
                    return "||";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.OnesComplement:
                    return "~";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    throw new NotImplementedException("Operator printout undefined.");
            }
        }

        public static void CulturalExecution(Action action, string culture)
        {
            var temp = Thread.CurrentThread.CurrentCulture; // backup current culture
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            action();
            Thread.CurrentThread.CurrentCulture = temp; // restore culture
        }

        public static void CulturalExecutionUI(Action action, string culture)
        {
            var temp = Thread.CurrentThread.CurrentUICulture; // backup current UI culture
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture);
            action();
            Thread.CurrentThread.CurrentUICulture = temp; // restore culture
        }
    }
}
