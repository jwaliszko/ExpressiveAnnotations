/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    /// <summary>
    ///     Model validator for <see cref="AssertThatAttribute" />.
    /// </summary>
    public class AssertThatValidator : DataAnnotationsModelValidator<AssertThatAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssertThatValidator" /> class.
        /// </summary>
        /// <param name="metadata">The model metadata instance.</param>
        /// <param name="context">The controller context instance.</param>
        /// <param name="attribute">The expressive assertion attribute instance.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
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
                throw new ValidationException(
                    string.Format("{0}: validation applied to {1} field failed.",
                    GetType().Name, metadata.PropertyName), e);
            }
        }

        private string Expression { get; set; }
        private string FormattedErrorMessage { get; set; }
        private IDictionary<string, string> FieldsMap { get; set; }
        private IDictionary<string, object> ConstsMap { get; set; }
        private string AnnotatedField { get; set; }

        /// <summary>
        ///     Retrieves a collection of client validation rules (rules sent to browsers).
        /// </summary>
        /// <returns>
        ///     A collection of client validation rules.
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
                ValidationType = string.Format("assertthat{0}", suffix)
            };

            rule.ValidationParameters.Add("expression", Expression);
            rule.ValidationParameters.Add("fieldsmap", FieldsMap.ToJson());
            rule.ValidationParameters.Add("constsmap", ConstsMap.ToJson());
            yield return rule;
        }
    }
}
