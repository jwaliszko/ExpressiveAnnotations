/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;

namespace ExpressiveAnnotations.MvcUnobtrusive.Providers
{
    /// <summary>
    ///     Data annotations validator provider which automatically registers adapters for expressive validation attributes, i.e. <see cref="ExpressiveAttribute" />, 
    ///     and additionally respects their processing priorities (if <see cref="ExpressiveAttribute.Priority" /> is specified) when validation is executed.
    /// </summary>
    /// <remarks>
    ///     Attributes with highest priority (lowest value) will be processed in first place. Attributes without explicitly proivided priorities will be processed later, 
    ///     without any specific order.
    /// </remarks>
    public class ExpressiveAnnotationsModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExpressiveAnnotationsModelValidatorProvider" /> class.
        /// </summary>
        public ExpressiveAnnotationsModelValidatorProvider()
        {
            RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            RegisterAdapter(typeof(AssertThatAttribute), typeof(AssertThatValidator));
        }

        /// <summary>
        ///     Gets a list of validators.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="context">The context.</param>
        /// <param name="attributes">The list of validation attributes.</param>
        /// <returns>
        ///     A list of validators.
        /// </returns>
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            var allAttribs = attributes.ToList();
            var orderedAttribs = allAttribs
                .Where(x => x is ExpressiveAttribute)
                .Cast<ExpressiveAttribute>()
                .Where(x => x.GetPriority().HasValue)
                .OrderBy(x => x.Priority)
                .Cast<Attribute>()
                .ToList();
            
            var setToRemove = new HashSet<Attribute>(orderedAttribs);
            allAttribs.RemoveAll(setToRemove.Contains); // allAttribs variable contains only chaotic attribs now
            orderedAttribs.AddRange(allAttribs); // chaotic attribs are added after ordered ones
            
            return base.GetValidators(metadata, context, orderedAttribs);
        }
    }
}
