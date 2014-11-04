/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Validation attribute, executed for non-null annotated field, which indicates that assertion given in logical expression 
    ///     has to be satisfied, for such field to be considered as valid.
    /// </summary>    
    public sealed class AssertThatAttribute : ExpressiveAttribute
    {
        private const string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssertThatAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which assertion condition is computed.</param>
        public AssertThatAttribute(string expression)
            : base(expression, _defaultErrorMessage)
        {
        }

        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (!CachedValidationFuncs.ContainsKey(validationContext.ObjectType))
                    CachedValidationFuncs[validationContext.ObjectType] = Parser.Parse(validationContext.ObjectType, Expression);

                if (!CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the assertion condition is not satisfied
                    return new ValidationResult( // assertion not satisfied => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression),
                        new[] {validationContext.MemberName});
            }

            return ValidationResult.Success;
        }
    }
}
