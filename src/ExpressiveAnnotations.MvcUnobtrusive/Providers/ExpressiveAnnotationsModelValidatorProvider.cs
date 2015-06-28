/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
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
    ///     Validator provider which automatically registers adapters for expressive validation attributes 
    ///     and additionally respects their processing priorities (if provided) when validation is executed.
    /// </summary>
    public class ExpressiveAnnotationsModelValidatorProvider : DataAnnotationsModelValidatorProvider
    {
        public ExpressiveAnnotationsModelValidatorProvider()
        {
            RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            RegisterAdapter(typeof(AssertThatAttribute), typeof(AssertThatValidator));
        }

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
