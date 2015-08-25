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
using ExpressiveAnnotations.MvcUnobtrusive.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusive.Validators
{
    /// <summary>
    ///     Base class for expressive validators.
    /// </summary>
    /// <typeparam name="T">Any type derived from <see cref="ExpressiveAttribute" /> class.</typeparam>
    public abstract class ExpressiveValidator<T> : DataAnnotationsModelValidator<T> where T : ExpressiveAttribute
    {
        private readonly object _locker = new object();

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
                var fieldsId = string.Format("fields.{0}", attribId);
                var constsId = string.Format("consts.{0}", attribId);
                var parsersId = string.Format("parsers.{0}", attribId);

                FieldsMap = HttpRuntime.Cache.Get(fieldsId) as IDictionary<string, string>;
                ConstsMap = HttpRuntime.Cache.Get(constsId) as IDictionary<string, object>;
                ParsersMap = HttpRuntime.Cache.Get(parsersId) as IDictionary<string, string>;
                FieldAttributeType = string.Format("{0}.{1}", typeof(T).FullName, annotatedField).ToLowerInvariant();

                if (!Cached)
                {
                    lock (_locker)
                    {
                        if (!Cached)
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

                            AssertNoNamingCollisionsAtCorrespondingSegments();
                            HttpRuntime.Cache.Insert(fieldsId, FieldsMap);
                            HttpRuntime.Cache.Insert(constsId, ConstsMap);
                            HttpRuntime.Cache.Insert(parsersId, ParsersMap);

                            attribute.Compile(metadata.ContainerType);
                        }
                    }
                }

                Expression = attribute.Expression;
                FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);
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

        private bool Cached
        {
            get { return FieldsMap != null || ConstsMap != null; }
        }

        /// <summary>
        ///     Provides unique validation type within current annotated field range, when multiple annotations are used (required for client-side).
        /// </summary>
        /// <param name="baseName">Base name.</param>
        /// <returns>
        ///     Unique validation type within current request.
        /// </returns>
        protected string ProvideUniqueValidationType(string baseName)
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
    }
}
