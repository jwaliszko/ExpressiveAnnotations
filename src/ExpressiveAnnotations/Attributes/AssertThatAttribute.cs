/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Analysis;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Validation attribute, executed for non-null annotated field, which indicates that assertion given in logical expression 
    ///     has to be satisfied, for such field to be considered as valid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AssertThatAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssertThatAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which requirement condition is computed.</param>
        public AssertThatAttribute(string expression)
            : base(_defaultErrorMessage)
        {
            Parser = new Parser();
            Parser.RegisterMethods();

            Expression = expression;
        }

        private Func<object, bool> CachedValidationFunc { get; set; }
        private Parser Parser { get; set; }

        /// <summary>
        ///     Gets or sets the logical expression based on which requirement condition is computed.
        /// </summary>
        public string Expression { get; set; }

        public override object TypeId
        {
            get { return string.Format("{0}[{1}]", GetType().FullName, Regex.Replace(Expression, @"\s+", string.Empty)); } // distinguishes instances based on provided expressions
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
                CachedValidationFunc = Parser.Parse(validationContextType, Expression);
                return;
            }
            if (CachedValidationFunc == null)
                CachedValidationFunc = Parser.Parse(validationContextType, Expression);
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
        ///     Validates a specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">validationContext;ValidationContext not provided.</exception>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException("validationContext", "ValidationContext not provided.");

            if (value != null)
            {
                if (CachedValidationFunc == null)
                    CachedValidationFunc = Parser.Parse(validationContext.ObjectType, Expression);

                if (!CachedValidationFunc(validationContext.ObjectInstance)) // check if the assertion condition is not satisfied
                    return new ValidationResult( // assertion not satisfied => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression), 
                        new[] {validationContext.MemberName});
            }

            return ValidationResult.Success;
        }
    }
}
