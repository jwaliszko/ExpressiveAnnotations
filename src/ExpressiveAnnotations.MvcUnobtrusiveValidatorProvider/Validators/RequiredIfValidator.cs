using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    /// <summary>
    /// Model validator for <see cref="RequiredIfAttribute" />.
    /// </summary>
    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        private string Expression { get; set; }
        private string FormattedErrorMessage { get; set; }        
        private bool AllowEmpty { get; set; }
        private Type CallerType { get; set; }
        private IDictionary<string, string> TypesMap { get; set; }
        private IDictionary<string, Dictionary<string, int>> EnumsMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            var parser = new Parser();
            parser.RegisterMethods();
            parser.Parse(metadata.ContainerType, attribute.Expression);

            TypesMap = parser.GetMembers()
                .ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value));
            EnumsMap = parser.GetEnums()
                .ToDictionary(x => x.Key, x => Enum.GetValues(x.Value).Cast<object>().ToDictionary(v => v.ToString(), v => (int)v));

            Expression = attribute.Expression;
            FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);
            AllowEmpty = attribute.AllowEmptyStrings;
            CallerType = metadata.ModelType;
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
                ValidationType = "requiredif",
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(Expression));
            rule.ValidationParameters.Add("typesmap", JsonConvert.SerializeObject(TypesMap));
            rule.ValidationParameters.Add("enumsmap", JsonConvert.SerializeObject(EnumsMap));
            rule.ValidationParameters.Add("allowempty", JsonConvert.SerializeObject(AllowEmpty));
            rule.ValidationParameters.Add("callertype", JsonConvert.SerializeObject(Helper.GetCoarseType(CallerType)));            
            yield return rule;
        }
    }
}
