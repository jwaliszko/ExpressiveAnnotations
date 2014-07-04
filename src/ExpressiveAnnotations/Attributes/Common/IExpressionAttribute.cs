namespace ExpressiveAnnotations.Attributes.Common
{
    /// <summary>
    /// Defines contract for complex conditional attributes.
    /// </summary>
    internal interface IExpressionAttribute
    {
        /// <summary>
        /// Gets or sets the logical expression based on which requirement condition is computed. 
        /// Available expression tokens: &amp;&amp;, ||, !, {, }, numbers and whitespaces.
        /// </summary>
        string Expression { get; set; }

        /// <summary>
        /// Gets or sets the names of dependent fields from which runtime values are extracted.
        /// </summary>
        string[] DependentProperties { get; set; }

        /// <summary>
        /// Gets or sets the expected values for corresponding dependent fields (wildcard character * stands for any value). There is also 
        /// possibility of values runtime extraction from backing fields, by providing their names [inside square brackets].
        /// </summary>
        object[] TargetValues { get; set; }

        /// <summary>
        /// Gets or sets the relational operators describing relations between dependent fields and corresponding target values.
        /// Available operators: ==, !=, >, >=, &lt;, &lt;=. If this property is not provided, equality operator == is used by default.
        /// </summary>
        string[] RelationalOperators { get; set; }

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
