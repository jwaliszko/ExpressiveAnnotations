/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Functions;

namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    ///     Base class for expressive validation attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ExpressiveAttribute : ValidationAttribute
    {
        private int? _priority;

        /// <summary>
        ///     Constructor for expressive validation attribute.
        /// </summary>
        /// <param name="expression">The logical expression based on which specified condition is computed.</param>
        /// <param name="errorMessage">The error message to associate with a validation control.</param>
        /// <exception cref="System.ArgumentNullException"><c>expression</c> is null</exception>
        protected ExpressiveAttribute(string expression, string errorMessage)
            : this(expression, () => errorMessage)
        {
        }

        /// <summary>
        ///     Constructor for expressive validation attribute.
        /// </summary>
        /// <param name="expression">The logical expression based on which specified condition is computed.</param>
        /// <param name="errorMessageAccessor">The error message accessor, allowing the error message to be provided dynamically.</param>
        /// <exception cref="System.ArgumentNullException"><c>expression</c> is null</exception>
        /// <remarks>
        ///     Allows for providing a resource accessor function that will be used to retrieve the error message.
        ///     An example would be to have something like, e.g.
        ///     CustomAttribute() : base( () =&gt; MyResources.MyErrorMessage ) {}.
        /// </remarks>
        protected ExpressiveAttribute(string expression, Func<string> errorMessageAccessor)
            : base(errorMessageAccessor)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression not provided.");

            Parser = new Parser();
            Parser.RegisterToolchain();

            Expression = expression;
            CachedValidationFuncs = new Dictionary<Type, Func<object, bool>>();
        }

        /// <summary>
        ///     Gets the cached validation funcs.
        /// </summary>
        protected Dictionary<Type, Func<object, bool>> CachedValidationFuncs { get; private set; }

        /// <summary>
        ///     Gets the parser associated with this attribute.
        /// </summary>
        protected Parser Parser { get; private set; }

        /// <summary>
        ///     Gets the annotated property type.
        /// </summary>
        protected Type PropertyType { get; private set; }

        /// <summary>
        ///     Gets the logical expression based on which specified condition is computed.
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        ///     Gets or sets the hint, available for any concerned external components, indicating the order in which this attribute should be
        ///     executed among others of its kind.
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
                        $"The {nameof(Priority)} property has not been set. Use the {nameof(GetPriority)} method to get the value.");
                return _priority.Value;
            }
            set { _priority = value; }
        }

        /// <summary>
        ///     When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute" />.
        /// </summary>
        public override object TypeId => $"{GetType().FullName}[{Regex.Replace(Expression, @"\s+", string.Empty)}]"; /* distinguishes instances based on provided expressions - that way of TypeId creation is chosen over the alternatives below:
                                                                                                                      *     - returning new object - it is too much, instances would be always different,
                                                                                                                      *     - returning hash code based on expression - can lead to collisions (infinitely many strings can't be mapped injectively into any finite set - best unique identifier for string is the string itself)
                                                                                                                      */

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
        /// <param name="action">The action to be invoked after compilation is done. <see cref="Parser" /> object, containing all of the computed data, is given as an input parameter.</param>
        /// <param name="force">Flag indicating whether parsing should be invoked again despite the fact compiled lambda already exists.</param>
        /// <exception cref="ParseErrorException"></exception>
        public void Compile(Type validationContextType, Action<Parser> action = null, bool force = false)
        {
            if (force)
            {
                CachedValidationFuncs[validationContextType] = Parser.Parse<bool>(validationContextType, Expression);
                action?.Invoke(Parser);
                return;
            }
            if (!CachedValidationFuncs.ContainsKey(validationContextType))
                CachedValidationFuncs[validationContextType] = Parser.Parse<bool>(validationContextType, Expression);
            action?.Invoke(Parser);
        }

        /// <summary>
        ///     Formats the error message for user.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="expression">The user-visible expression to include in the formatted message.</param>
        /// <param name="objectInstance">The anotated field declaring container instance.</param>
        /// <returns>
        ///     The localized message to present to the user.
        /// </returns>
        /// <exception cref="System.FormatException"></exception>
        /// <remarks>
        ///     This method interprets custom format specifiers, provided to error message string, for which values or display names of model fields are extracted. Specifiers should be given
        ///     in braces (curly brackets), i.e. {fieldPath[:indicator]}, e.g. {field}, {field.field:n}. Braces can be escaped by double-braces, i.e. to output a { use {{ and to output a } use }}.
        /// </remarks>
        public virtual string FormatErrorMessage(string displayName, string expression, object objectInstance)
        {
            try
            {
                IList<FormatItem> items;
                var message = PreformatMessage(displayName, expression, out items);

                message = items.Aggregate(message, (cargo, current) => current.Indicator != null && !current.Constant ? cargo.Replace(current.Uuid.ToString(), Helper.ExtractDisplayName(objectInstance.GetType(), current.FieldPath)) : cargo);
                message = items.Aggregate(message, (cargo, current) => current.Indicator == null && !current.Constant ? cargo.Replace(current.Uuid.ToString(), (Helper.ExtractValue(objectInstance, current.FieldPath) ?? string.Empty).ToString()) : cargo);
                return message;
            }
            catch (Exception e)
            {
                throw new FormatException(
                    $"Problem with error message processing. The message is following: {ErrorMessageString}", e);
            }
        }

        /// <summary>
        ///     Formats the error message for client (value indicators not resolved - replaced by guids to be dynamically extracted by client-code).
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="expression">The user-visible expression to include in the formatted message.</param>
        /// <param name="objectType">The annotated field declaring container type.</param>
        /// <param name="fieldsMap">The map containing fields names for which values should be extracted by client-side.</param>
        /// <returns>
        ///     The localized message to sent to the client-side component.
        /// </returns>
        /// <exception cref="System.FormatException"></exception>
        /// <remarks>
        ///     This method interprets custom format specifiers, provided to error message string, for which values or display names of model fields are extracted. Specifiers should be given
        ///     in braces (curly brackets), i.e. {fieldPath[:indicator]}, e.g. {field}, {field.field:n}. Braces can be escaped by double-braces, i.e. to output a { use {{ and to output a } use }}.
        /// </remarks>
        public virtual string FormatErrorMessage(string displayName, string expression, Type objectType, out IDictionary<string, Guid> fieldsMap)
        {
            try
            {
                IList<FormatItem> items;
                var message = PreformatMessage(displayName, expression, out items);

                var map = items.Where(x => x.Indicator == null && !x.Constant).Select(x => x.FieldPath).Distinct().ToDictionary(x => x, x => Guid.NewGuid()); // sanitize
                message = items.Aggregate(message, (cargo, current) => current.Indicator != null && !current.Constant ? cargo.Replace(current.Uuid.ToString(), Helper.ExtractDisplayName(objectType, current.FieldPath)) : cargo);
                message = items.Aggregate(message, (cargo, current) => current.Indicator == null && !current.Constant ? cargo.Replace(current.Uuid.ToString(), map[current.FieldPath].ToString()) : cargo);
                fieldsMap = map;
                return message;
            }
            catch (Exception e)
            {
                throw new FormatException(
                    $"Problem with error message processing. The message is following: {ErrorMessageString}", e);
            }
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
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Debug.Assert(validationContext != null);

            try
            {
                AdjustMemberName(validationContext);
                return IsValidInternal(value, validationContext);
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    $"{GetType().Name}: validation applied to {validationContext.MemberName ?? "<member unknown>"} field failed.", e);
            }
        }

        private void AdjustMemberName(ValidationContext validationContext) // fixes for: MVC <= 4 (MemberName is not provided), WebAPI 2 (MemberName states for display name)
        {
            PropertyType = null; // reset value
            if (validationContext.MemberName == null && validationContext.DisplayName == null)
                return;

            validationContext.MemberName = validationContext.MemberName ?? validationContext.DisplayName;
            var prop = validationContext.ObjectType.GetProperty(validationContext.MemberName);
            if (prop == null)
            {
                prop = validationContext.ObjectType.GetPropertyByDisplayName(validationContext.MemberName);
                if (prop == null)
                {
                    validationContext.MemberName = null;
                    return;
                }

                validationContext.MemberName = prop.Name;
            }
            PropertyType = prop.PropertyType;
        }

        private string PreformatMessage(string displayName, string expression, out IList<FormatItem> items)
        {
            var message = MessageFormatter.FormatString(ErrorMessageString, out items); // process custom format items: {fieldPath[:indicator]}, and substitute them entirely with guids, not to interfere with standard string.Format() invoked below
            message = string.Format(message, displayName, expression); // process standard format items: {index[,alignment][:formatString]}, https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx
            message = items.Aggregate(message, (cargo, current) => cargo.Replace(current.Uuid.ToString(), current.Substitute)); // give back, initially preprocessed, custom format items
            return message;
        }
    }
}
