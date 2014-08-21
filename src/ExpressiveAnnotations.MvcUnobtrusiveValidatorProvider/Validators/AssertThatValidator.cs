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
        private IDictionary<string, string> FieldsMap { get; set; }
        private IDictionary<string, object> ConstsMap { get; set; }
        private string AnnotatedField { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public AssertThatValidator(ModelMetadata metadata, ControllerContext context, AssertThatAttribute attribute)
            : base(metadata, context, attribute)
        {
            try
            {
                AnnotatedField = string.Format("{0}.{1}", metadata.ContainerType.FullName, metadata.PropertyName).ToLowerInvariant();
                var attribId = string.Format("{0}.{1}", attribute.TypeId, AnnotatedField).ToLowerInvariant();
                var fieldsId = string.Format("fields.{0}", attribId);
                var constsId = string.Format("consts.{0}", attribId);
                FieldsMap = HttpRuntime.Cache.Get(fieldsId) as IDictionary<string, string>;
                ConstsMap = HttpRuntime.Cache.Get(constsId) as IDictionary<string, object>;

                if (FieldsMap == null && ConstsMap == null)
                {
                    var parser = new Parser();
                    parser.RegisterMethods();
                    parser.Parse(metadata.ContainerType, attribute.Expression);

                    FieldsMap = parser.GetFields().ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value));
                    ConstsMap = parser.GetConsts();

                    Assert.NoNamingCollisionsAtCorrespondingSegments(FieldsMap.Keys, ConstsMap.Keys);
                    HttpContext.Current.Cache.Insert(fieldsId, FieldsMap);
                    HttpContext.Current.Cache.Insert(constsId, ConstsMap);

                    attribute.Compile(metadata.ContainerType);
                }

                Expression = attribute.Expression;
                FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);    
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format(
                    "Problem related to {0} attribute for {1} field with following expression specified: {2}",
                    attribute.GetType().Name, metadata.PropertyName, attribute.Expression), e);
            }
        }

        /// <summary>
        /// Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        /// <returns>
        /// A collection of client validation rules.
        /// </returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var count = Storage.Get<int>(AnnotatedField) + 1;
            Storage.Set(AnnotatedField, count);

            if (count > 27)
                throw new ApplicationException("No more than 27 unique attributes of the same type can be applied for a single field or property.");

            var suffix = count == 1 ? string.Empty : char.ConvertFromUtf32(95 + count);
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormattedErrorMessage,
                ValidationType = string.Format("assertthat{0}", suffix),
            };
            rule.ValidationParameters.Add("expression", JsonConvert.SerializeObject(Expression));
            rule.ValidationParameters.Add("fieldsmap", JsonConvert.SerializeObject(FieldsMap));
            rule.ValidationParameters.Add("constsmap", JsonConvert.SerializeObject(ConstsMap));
            yield return rule;
        }
    }
}
