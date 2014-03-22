using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressiveAnnotations.Tests.Misc
{
    internal static class ModelHelper 
    {
        public static bool IsValid<T, R>(this T item, Expression<Func<T, R>> expression)
        {
            var prop = GetPropertyName(item, expression);
            var value = typeof(T).GetProperty(prop).GetValue(item, null);

            var results = new List<ValidationResult>();
            var vc = new ValidationContext(item, null, null) { MemberName = prop };
            var valid = Validator.TryValidateProperty(value, vc, results);
            return valid;
        }

        private static string GetPropertyName<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda)
        {            
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression \"{0}\" refers to a method, not a property.", propertyLambda));
            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression \"{0}\" refers to a field, not a property.", propertyLambda));

            var type = source.GetType();
            if (propInfo.ReflectedType != null 
                && (type != propInfo.ReflectedType 
                    && !type.IsSubclassOf(propInfo.ReflectedType)))
                throw new ArgumentException(string.Format("Expresion \"{0}\" refers to a property that is not from type {1}.", propertyLambda, type));

            return propInfo.Name;
        }
    }
}
