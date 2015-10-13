/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusive.Validators
{
    /// <summary>
    ///     Base class for expressive validators.
    /// </summary>
    /// <typeparam name="T">Any type derived from <see cref="ExpressiveAttribute" /> class.</typeparam>
    public abstract class ExpressiveValidator<T> : DataAnnotationsModelValidator<T> where T : ExpressiveAttribute
    {
        private static readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();

        /// <summary>
        ///     Constructor for expressive model validator.
        /// </summary>
        /// <param name="metadata">The model metadata instance.</param>
        /// <param name="context">The controller context instance.</param>
        /// <param name="attribute">The expressive attribute instance.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        protected ExpressiveValidator(ModelMetadata metadata, ControllerContext context, T attribute)
            : base(metadata, context, attribute)
        {
            try
            {
                var annotatedField = string.Format("{0}.{1}", metadata.ContainerType.FullName, metadata.PropertyName).ToLowerInvariant();
                var attribId = string.Format("{0}.{1}", attribute.TypeId, annotatedField).ToLowerInvariant();
                FieldAttributeType = string.Format("{0}.{1}", typeof(T).FullName, annotatedField).ToLowerInvariant();

                var item = _cache.GetOrAdd(attribId, _ =>
                {
                    var parser = new Parser();
                    parser.RegisterMethods();
                    parser.Parse(metadata.ContainerType, attribute.Expression);

                    FieldsMap = parser.GetFields().ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value));
                    ConstsMap = parser.GetConsts();
                    ParsersMap = metadata.ContainerType.GetProperties()
                        .Where(p => FieldsMap.Keys.Contains(p.Name) || metadata.PropertyName == p.Name) // narrow down number of parsers sent to client
                        .Select(p => new
                        {
                            PropertyName = p.Name,
                            ParserAttribute = p.GetCustomAttributes(typeof (ValueParserAttribute), false) // use this version over generic one (.NET4.0 support)
                                    .Cast<ValueParserAttribute>()
                                    .SingleOrDefault()
                        }).Where(x => x.ParserAttribute != null)
                        .ToDictionary(x => x.PropertyName, x => x.ParserAttribute.ParserName);

                    IDictionary<string, Guid> errFieldsMap;
                    FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression, metadata.ContainerType, out errFieldsMap); // fields names, in contrast to values, do not change in runtime, so will be provided in message (less code in js)
                    ErrFieldsMap = errFieldsMap;

                    AssertNoNamingCollisionsAtCorrespondingSegments();
                    attribute.Compile(metadata.ContainerType);

                    return new CacheItem
                    {
                        FieldsMap = FieldsMap,
                        ConstsMap = ConstsMap,
                        ParsersMap = ParsersMap,
                        ErrFieldsMap = ErrFieldsMap,
                        FormattedErrorMessage = FormattedErrorMessage
                    };
                });

                FieldsMap = item.FieldsMap;
                ConstsMap = item.ConstsMap;
                ParsersMap = item.ParsersMap;
                ErrFieldsMap = item.ErrFieldsMap;
                FormattedErrorMessage = item.FormattedErrorMessage;

                Expression = attribute.Expression;
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    string.Format("{0}: validation applied to {1} field failed.", GetType().Name, metadata.PropertyName), 
                    e);
            }
        }

        /// <summary>
        ///     Gets the expression.
        /// </summary>
        protected string Expression { get; private set; }

        /// <summary>
        ///     Gets the formatted error message.
        /// </summary>
        protected string FormattedErrorMessage { get; private set; }

        /// <summary>
        ///     Gets fields names and corresponding guid identifiers obfuscating such fields in error message string.
        /// </summary>
        protected IDictionary<string, Guid> ErrFieldsMap { get; private set; }

        /// <summary>
        ///     Gets names and coarse types of properties extracted from specified expression within given context.
        /// </summary>
        protected IDictionary<string, string> FieldsMap { get; private set; }

        /// <summary>
        ///     Gets properties names and parsers registered for them via <see cref="ValueParserAttribute" />.
        /// </summary>
        protected IDictionary<string, string> ParsersMap { get; private set; }

        /// <summary>
        ///     Gets names and values of constants extracted from specified expression within given context.
        /// </summary>
        protected IDictionary<string, object> ConstsMap { get; private set; }

        private string FieldAttributeType { get; set; }

        /// <summary>
        ///     Generates client validation rule with the basic set of parameters.
        /// </summary>
        ///     <param name="type">The validation type.</param>
        /// <returns>
        ///     Client validation rule with the basic set of parameters.
        /// </returns>
        protected ModelClientValidationRule GetBasicRule(string type)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormattedErrorMessage,
                ValidationType = ProvideUniqueValidationType(type)
            };

            rule.ValidationParameters.Add("expression", Expression.ToJson());            

            Debug.Assert(FieldsMap != null);
            if (FieldsMap != null && FieldsMap.Any())
                rule.ValidationParameters.Add("fieldsmap", FieldsMap.ToJson());
            Debug.Assert(ConstsMap != null);
            if (ConstsMap != null && ConstsMap.Any())
                rule.ValidationParameters.Add("constsmap", ConstsMap.ToJson());
            Debug.Assert(ParsersMap != null);
            if (ParsersMap != null && ParsersMap.Any())
                rule.ValidationParameters.Add("parsersmap", ParsersMap.ToJson());
            Debug.Assert(ErrFieldsMap != null);
            if (ErrFieldsMap != null && ErrFieldsMap.Any())
                rule.ValidationParameters.Add("errfieldsmap", ErrFieldsMap.ToJson());

            return rule;
        }

        /// <summary>
        ///     Provides unique validation type within current annotated field range, when multiple annotations are used (required for client-side).
        /// </summary>
        /// <param name="baseName">Base name.</param>
        /// <returns>
        ///     Unique validation type within current request.
        /// </returns>
        private string ProvideUniqueValidationType(string baseName)
        {
            return string.Format("{0}{1}", baseName, AllocateSuffix());
        }

        private string AllocateSuffix()
        {
            var count = RequestStorage.Get<int>(FieldAttributeType) + 1;
            AssertAttribsQuantityAllowed(count);

            RequestStorage.Set(FieldAttributeType, count);
            return count == 1 ? string.Empty : char.ConvertFromUtf32(95 + count); // single lowercase letter from latin alphabet or an empty string
        }

        private void AssertNoNamingCollisionsAtCorrespondingSegments()
        {
            string name;
            int level;
            if (Helper.SegmentsCollide(FieldsMap.Keys, ConstsMap.Keys, out name, out level))
                throw new InvalidOperationException(
                    string.Format("Naming collisions cannot be accepted by client-side - {0} part at level {1} is ambiguous.", name, level));
        }

        private void AssertAttribsQuantityAllowed(int count)
        {
            const int max = 27;
            if (count > max)
                throw new InvalidOperationException(
                    string.Format("No more than {0} unique attributes of the same type can be applied for a single field or property.", max));
        }

        private class CacheItem
        {
            public IDictionary<string, string> FieldsMap { get; set; }
            public IDictionary<string, object> ConstsMap { get; set; }
            public IDictionary<string, string> ParsersMap { get; set; }
            public IDictionary<string, Guid> ErrFieldsMap { get; set; }
            public string FormattedErrorMessage { get; set; }
        }
    }
}
