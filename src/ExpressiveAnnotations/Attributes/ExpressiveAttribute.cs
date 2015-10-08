/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        private int? _priority;
        private const string _fieldRegex = @"({+)[a-zA-Z_]+(?:(?:\.[a-zA-Z_])?[a-zA-Z0-9_]*)*(}+)"; // {field}, {field.field}
        private readonly IDictionary<string, string> _messageValuesMap;

        /// <summary>
        ///     Constructor for expressive validation attribute.
        /// </summary>
        /// <param name="expression">The logical expression based on which specified condition is computed.</param>
        /// <param name="errorMessage">The error message to associate with a validation control.</param>
        /// <exception cref="System.ArgumentNullException">expression;Expression not provided.</exception>
        protected ExpressiveAttribute(string expression, string errorMessage)
            : base(errorMessage)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Expression not provided.");

            Parser = new Parser();
            Parser.RegisterMethods();

            Expression = expression;
            CachedValidationFuncs = new Dictionary<Type, Func<object, bool>>();
            _messageValuesMap = new Dictionary<string, string>();
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
        ///     Gets the logical expression based on which specified condition is computed.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        ///     Gets or sets the hint, available for any concerned external components, indicating the order in which this attribute should be 
        ///     executed among others of its kind, i.e. <see cref="ExpressiveAttribute" />.
        ///     <para>
        ///         Consumers must use the <see cref="GetPriority" /> method to retrieve the value, as this property getter will throw an 
        ///         exception if the value has not been set.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     Value is optional and not set by default, which means that execution order is undefined.
        ///     <para>
        ///         Lowest value means highest priority.
        ///     </para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        ///     If the getter of this property is invoked when the value has not been explicitly set using the setter.
        /// </exception> 
        public int Priority
        {
            get
            {
                if (!_priority.HasValue)
                    throw new InvalidOperationException(
                        string.Format("The {0} property has not been set. Use the {1} method to get the value.", "Priority", "GetPriority"));
                return _priority.Value;
            }
            set { _priority = value; }
        }

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
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var attrib = obj as ExpressiveAttribute;
            return attrib != null && TypeId.Equals(attrib.TypeId);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return TypeId.GetHashCode();
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return TypeId.ToString();
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
        /// <remarks>
        ///     This method interprets custom format specifiers, provided to error message string, for which model fields values will be extracted. Specifiers should be
        ///     given in braces (curly brackets), e.g. {field}, {field.field}. Braces can be escaped by double-braces, i.e. to output a { use {{ and to output a } use }}.
        /// </remarks>
        public string FormatErrorMessage(string displayName, string expression)
        {
            var matches = Regex.Matches(ErrorMessageString, _fieldRegex);
            if (matches.Count <= 0)
                return string.Format(ErrorMessageString, displayName, expression);

            var message = ErrorMessageString;
            var substitutes = new Dictionary<string, string>();
            for (var i = 0; i < matches.Count; i++)
            {
                var arg = matches[i].Value;
                if (substitutes.ContainsKey(arg))
                    continue;

                substitutes[arg] = Guid.NewGuid().ToString();
            }

            // substitute custom format specifiers not to interfere with standard string.Format logic
            message = substitutes.Aggregate(message, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
            message = string.Format(message, displayName, expression);
            // give back custom format specifiers - standard string.Format is already done, so it will not fail
            message = substitutes.Aggregate(message, (current, kvp) => current.Replace(kvp.Value, kvp.Key));

            if (_messageValuesMap.Any())
                message = _messageValuesMap.Aggregate(message, (current, kvp) => current.Replace(kvp.Key, kvp.Value));

            return message;
        }

        /// <summary>
        ///     Gets the value of <see cref="Priority" /> if it has been set, or <c>null</c>.
        /// </summary>
        /// <returns>
        ///     When <see cref="Priority" /> has been set returns the value of that property.
        ///     <para>
        ///         When <see cref="Priority" /> has not been set returns <c>null</c>.
        ///     </para>
        /// </returns>
        public int? GetPriority()
        {
            return _priority;
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
                ExtractValuesForMessage(validationContext.ObjectInstance);
                return IsValidInternal(value, validationContext);
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    string.Format("{0}: validation applied to {1} field failed.",
                        GetType().Name, validationContext.MemberName), e);
            }
        }

        private void ExtractValuesForMessage(object context)
        {
            var matches = Regex.Matches(ErrorMessageString, _fieldRegex);
            if (matches.Count <= 0)
                return;

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var arg = match.Value;
                var leftBraces = match.Groups[1];
                var rightBraces = match.Groups[2];

                if (leftBraces.Length != rightBraces.Length)
                    throw new FormatException("Input string was not in a correct format.");

                var length = leftBraces.Length;
                // flatten each pair of braces (curly brackets) into single brace
                // in order to escape them - to output a { use {{ and to output a } use }} (just like standard string.Format does)
                var left = new string('{', length/2);
                var right = new string('}', length/2);

                var param = arg.Substring(length, arg.Length - 2 * length);
                if (length%2 != 0) // substitute param with respective value (just like string.Format does)
                {
                    if (context == null)
                        throw new ArgumentNullException("context", "Field value cannot be extracted from null model context.");
                    param = (Helper.ExtractValue(context, param) ?? string.Empty).ToString();
                }

                _messageValuesMap[arg] = string.Format("{0}{1}{2}", left, param, right);
            }
        }
    }
}
