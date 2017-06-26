using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Reflection;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public static class Extensions
    {
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, bool numericValues = true)
        {
            return EnumDropDownListFor(htmlHelper, expression, numericValues, null);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, bool numericValues, object htmlAttributes)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var type = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
            if (!type.IsEnum)
                throw new ArgumentException
                    ("Given parameter expression has to indicate enum type.", nameof(expression));
            var values = Enum.GetValues(type).Cast<TEnum>();

            var items = values.Select(value => new SelectListItem
            {
                Text = GetEnumDisplayText(value),
                Value = numericValues
                    ? string.Format(CultureInfo.InvariantCulture, $"{Convert.ChangeType(value, value.GetType().GetEnumUnderlyingType())}")
                    : string.Format(CultureInfo.InvariantCulture, $"{value}"),
                Selected = value.Equals(metadata.Model)
            });

            if (metadata.IsNullableValueType) // if the enum is nullable, add an empty item to the collection
                items = new[] {new SelectListItem()}.Concat(items);

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }

        private static string GetEnumDisplayText<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attrib = field.GetCustomAttributes<DisplayAttribute>().FirstOrDefault();
            return attrib != null ? attrib.GetName() : value.ToString();
        }
    }
}
