using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;

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
        private IDictionary<string, string> FieldsMap { get; set; }
        private IDictionary<string, object> ConstsMap { get; set; }
        private string AnnotatedField { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">The attribute.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            try
            {
                AnnotatedField = string.Format("{0}.{1}", metadata.ContainerType.FullName, metadata.PropertyName).ToLowerInvariant(); // annotated field id
                var attribId = string.Format("{0}.{1}", attribute.TypeId, AnnotatedField).ToLowerInvariant(); // global id used for cache
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
                AllowEmpty = attribute.AllowEmptyStrings;
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
            Assert.AttribsQuantityAllowed(count);

            Storage.Set(AnnotatedField, count);

            var suffix = count == 1 ? string.Empty : char.ConvertFromUtf32(95 + count);
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormattedErrorMessage,
                ValidationType = string.Format("requiredif{0}", suffix)
            };

            rule.ValidationParameters.Add("expression", Expression);
            rule.ValidationParameters.Add("fieldsmap", FieldsMap.ToJson());
            rule.ValidationParameters.Add("constsmap", ConstsMap.ToJson());
            rule.ValidationParameters.Add("allowempty", AllowEmpty.ToJson());
            yield return rule;
        }
    }
}
