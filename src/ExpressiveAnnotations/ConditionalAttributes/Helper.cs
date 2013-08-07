using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    internal class Helper
    {
        internal static object FetchTargetValue(object targetValue, ValidationContext validationContext)
        {
            var value = targetValue as string;
            if (value != null)
            {
                var containerType = validationContext.ObjectInstance.GetType();
                var match = Regex.Match(value, @"^\[(.+)\]$");
                if (match.Success)
                {
                    var fieldName = match.Groups[1].Value;
                    var field = containerType.GetProperty(fieldName);
                    if (field == null)
                    {
                        throw new ArgumentException("Target value cannot be dynamically extracted from \"{0}\" field.",
                                                    fieldName);
                    }
                    return field.GetValue(validationContext.ObjectInstance, null);
                }
            }
            return targetValue;
        }
    }
}
