namespace ExpressiveAnnotations.Attributes
{
    /// <summary>
    /// Defines contract for simple conditional attributes.
    /// </summary>
    public interface IAttribute
    {
        /// <summary>
        /// Gets or sets the name of dependent field from which runtime value is extracted.
        /// </summary>
        string DependentProperty { get; set; }

        /// <summary>
        /// Gets or sets the expected value for dependent field (wildcard character * stands for any value). There is also possibility 
        /// of value runtime extraction from backing field, by providing its name [inside square brackets].
        /// </summary>
        object TargetValue { get; set; }

        /// <summary>
        /// Gets or sets the relational operator describing relation between dependent field and target value. Available operators: 
        /// ==, !=, >, >=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        string RelationalOperator { get; set; }

        /// <summary>
        /// Gets or sets whether the string comparisons are case sensitive or not.
        /// </summary>
        bool SensitiveComparisons { get; set; }

        /// <summary>
        /// Formats the error message.
        /// </summary>
        /// <param name="displayName">The user-visible name of the required field to include in the formatted message.</param>
        /// <param name="preprocessedExpression">The user-visible expression to include in the formatted message.</param>
        /// <returns>The localized message to present to the user.</returns>
        string FormatErrorMessage(string displayName, string preprocessedExpression);
    }
}
