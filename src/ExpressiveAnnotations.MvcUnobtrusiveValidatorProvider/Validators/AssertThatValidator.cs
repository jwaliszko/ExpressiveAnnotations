using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    /// <summary>
    /// Model validator for <see cref="AssertThatAttribute" />.
    /// </summary>
    public class AssertThatValidator : DataAnnotationsModelValidator<AssertThatAttribute>
    {
        private string Expression { get; set; }
        private string FormattedErrorMessage { get; set; }
        private IDictionary<string, string> TypesMap { get; set; }
        private IDictionary<string, Dictionary<string, int>> EnumsMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
            var typesId = ("AssertThatAttribute_types_" + metadata.ContainerType.FullName + "." + metadata.PropertyName).ToLowerInvariant();
            var enumsId = ("AssertThatAttribute_enums_" + metadata.ContainerType.FullName + "." + metadata.PropertyName).ToLowerInvariant();
            TypesMap = HttpRuntime.Cache.Get(typesId) as IDictionary<string, string>;
            EnumsMap = HttpRuntime.Cache.Get(enumsId) as IDictionary<string, Dictionary<string, int>>;

            if (TypesMap == null && TypesMap == null)
            {
                var parser = new Parser();
                parser.RegisterMethods();
                parser.Parse(metadata.ContainerType, attribute.Expression);

                TypesMap = parser.GetMembers()
                    .ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value));
                EnumsMap = parser.GetEnums()
                    .ToDictionary(x => x.Key, x => Enum.GetValues(x.Value).Cast<object>().ToDictionary(v => v.ToString(), v => Convert.ToInt32(v)));

                HttpContext.Current.Cache.Insert(typesId, TypesMap);
                HttpContext.Current.Cache.Insert(enumsId, EnumsMap);
            }

            Expression = attribute.Expression;
            FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);            
        }

        /// <summary>
        /// Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        /// <returns>
        /// A collection of client validation rules.
        /// </returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormattedErrorMessage,
                ValidationType = "assertthat",
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(Expression));
            rule.ValidationParameters.Add("typesmap", JsonConvert.SerializeObject(TypesMap));
            rule.ValidationParameters.Add("enumsmap", JsonConvert.SerializeObject(EnumsMap));
            yield return rule;
        }
    }
}
