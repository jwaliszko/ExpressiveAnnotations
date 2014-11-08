/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Base class for expressive validation attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ExpressiveAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Constructor for expressive validation attribute.
        /// </summary>
        /// <param name="expression">The logical expression based on which specified condition is computed.</param>
        /// <param name="errorMessage">The error message to associate with a validation control.</param>
        protected ExpressiveAttribute(string expression, string errorMessage)
            : base(errorMessage)
        {
            Parser = new Parser();
            Parser.RegisterMethods();

            Expression = expression;
            CachedValidationFuncs = new Dictionary<Type, Func<object, bool>>();
        }

        /// <summary>
        ///     Gets the cached validation funcs.
        /// </summary>        
        protected Dictionary<Type, Func<object, bool>> CachedValidationFuncs { get; private set; }

        /// <summary>
        ///     Gets the parser.
        /// </summary>        
        protected Parser Parser { get; private set; }

        /// <summary>
        ///     Gets or sets the logical expression based on which specified condition is computed.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        ///     When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute" />.
        /// </summary>
        public override object TypeId
        {
            /* From MSDN (msdn.microsoft.com/en-us/library/system.attribute.typeid.aspx, msdn.microsoft.com/en-us/library/6w3a7b50.aspx): 
             * 
             * As implemented, this identifier is merely the Type of the attribute. However, it is intended that the unique identifier be used to 
             * identify two attributes of the same type. 
             * 
             * When you define a custom attribute with AttributeUsageAttribute.AllowMultiple set to true, you must override the Attribute.TypeId 
             * property to make it unique. If all instances of your attribute are unique, override Attribute.TypeId to return the object identity 
             * of your attribute. If only some instances of your attribute are unique, return a value from Attribute.TypeId that would return equality 
             * in those cases. For example, some attributes have a constructor parameter that acts as a unique key. For these attributes, return the 
             * value of the constructor parameter from the Attribute.TypeId property.
             * 
             * To summarize: 
             * TypeId is documented as being a "unique identifier used to identify two attributes of the same type". By default, TypeId is just the 
             * type of the attribute, so when two attributes of the same type are encountered, they're considered "the same" by many frameworks.
             */
            get { return string.Format("{0}[{1}]", GetType().FullName, Regex.Replace(Expression, @"\s+", string.Empty)); } /* distinguishes instances based on provided expressions - that way of TypeId creation is chosen over the alternatives below: 
                                                                                                                            *     - returning new object - it is too much, instances would be always different, 
                                                                                                                            *     - returning hash code based on expression - can lead to collisions (infinitely many strings can't be mapped injectively into any finite set - best unique identifier for string is the string itself) 
                                                                                                                            */
        }

        /// <summary>
        ///     Parses and compiles expression provided to the attribute. Compiled lambda is then cached and used for validation purposes.
        /// </summary>
        /// <param name="validationContextType">The type of the object to be validated.</param>
        /// <param name="force">Flag indicating whether parsing should be rerun despite the fact compiled lambda already exists.</param>
        public void Compile(Type validationContextType, bool force = false)
        {
            if (force)
            {
                CachedValidationFuncs[validationContextType] = Parser.Parse(validationContextType, Expression);
                return;
            }
            if (!CachedValidationFuncs.ContainsKey(validationContextType))
                CachedValidationFuncs[validationContextType] = Parser.Parse(validationContextType, Expression);
        }

        /// <summary>
        ///     Formats the error message.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="expression">The user-visible expression to include in the formatted message.</param>
        /// <returns>
        ///     The localized message to present to the user.
        /// </returns>
        public string FormatErrorMessage(string displayName, string expression)
        {
            return string.Format(ErrorMessageString, displayName, expression);
        }

        /// <summary>
        ///     Validates a specified value with respect to the associated validation attribute.
        ///     Internally used by the <see cref="ExpressiveAttribute.IsValid(object,System.ComponentModel.DataAnnotations.ValidationContext)" /> method.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected abstract ValidationResult IsValidInternal(object value, ValidationContext validationContext);

        /// <summary>
        ///     Validates a specified value with respect to the associated validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">validationContext;ValidationContext not provided.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            validationContext.MemberName = validationContext.MemberName // in case member name is a null (e.g. like in older MVC versions), try workaround - get member name using display attribute
                                           ?? validationContext.ObjectType.GetMemberNameFromDisplayAttribute(validationContext.DisplayName);
            try
            {
                return IsValidInternal(value, validationContext);
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    string.Format("{0}: validation applied to {1} field failed.",
                        GetType().Name, validationContext.MemberName), e);
            }
        }
    }
}
