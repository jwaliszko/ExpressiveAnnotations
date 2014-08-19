namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    /// Token identifier.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Logical conjunction.
        /// </summary>
        AND,

        /// <summary>
        /// Logical disjunction.
        /// </summary>
        OR,

        /// <summary>
        /// Logical negation.
        /// </summary>
        NOT,        

        /// <summary>
        /// Greater than or equal to.
        /// </summary>
        GE,

        /// <summary>
        /// Less than or equal to.
        /// </summary>
        LE,

        /// <summary>
        /// Greater than.
        /// </summary>
        GT,

        /// <summary>
        /// Less than.
        /// </summary>
        LT,

        /// <summary>
        /// Equal to.
        /// </summary>
        EQ,

        /// <summary>
        /// Not equal to.
        /// </summary>
        NEQ,

        /// <summary>
        /// Addition.
        /// </summary>
        ADD,

        /// <summary>
        /// Subtraction.
        /// </summary>
        SUB,

        /// <summary>
        /// Multiplication.
        /// </summary>
        MUL,

        /// <summary>
        /// Division.
        /// </summary>
        DIV,

        /// <summary>
        /// Left bracket.
        /// </summary>
        LEFT_BRACKET,

        /// <summary>
        /// Right bracket.
        /// </summary>
        RIGHT_BRACKET,

        /// <summary>
        /// Comma.
        /// </summary>
        COMMA,

        /// <summary>
        /// NULL.
        /// </summary>
        NULL,

        /// <summary>
        /// Integer value.
        /// </summary>
        INT,

        /// <summary>
        /// Boolean value.
        /// </summary>
        BOOL,

        /// <summary>
        /// Float value.
        /// </summary>
        FLOAT,

        /// <summary>
        /// String.
        /// </summary>
        STRING,

        /// <summary>
        /// Guid.
        /// </summary>
        GUID,

        /// <summary>
        /// Function.
        /// </summary>
        FUNC,
        
        /// <summary>
        /// EOF.
        /// </summary>
        EOF
    }
}
