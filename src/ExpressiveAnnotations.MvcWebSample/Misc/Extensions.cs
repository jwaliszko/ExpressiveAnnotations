using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public static class Extensions
    {
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression) 
        {
            return EnumDropDownListFor(htmlHelper, expression, null);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {            
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);            
            var type = Nullable.GetUnderlyingType(metadata.ModelType) ?? metadata.ModelType;
            if (!type.IsEnum)
                throw new ArgumentException("Given parameter expression has to indicate enum type.", "expression");
            var values = Enum.GetValues(type).Cast<TEnum>();

            var items = values.Select(value => new SelectListItem
            {
                Text = GetEnumDisplayText(value),
                Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture),
                Selected = value.Equals(metadata.Model)
            });

            if (metadata.IsNullableValueType) // if the enum is nullable, add an empty item to the collection
                items = new[] {new SelectListItem()}.Concat(items);

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }     

        private static string GetEnumDisplayText<TEnum>(TEnum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attrib = field.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return attrib != null ? attrib.GetName() : value.ToString();
        }
    }
}