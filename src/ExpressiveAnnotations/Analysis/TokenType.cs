/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

namespace ExpressiveAnnotations.Analysis
{
    /// <summary>
    ///     Token type.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        ///     Logical conjunction (i.e. &amp;&amp;).
        /// </summary>
        AND,

        /// <summary>
        ///     Logical disjunction (i.e. ||).
        /// </summary>
        OR,

        /// <summary>
        ///     Logical negation (i.e. !).
        /// </summary>
        NOT,

        /// <summary>
        ///     Greater than or equal to (i.e. &gt;=).
        /// </summary>
        GE,

        /// <summary>
        ///     Less than or equal to (i.e. &lt;=).
        /// </summary>
        LE,

        /// <summary>
        ///     Greater than (i.e. &gt;).
        /// </summary>
        GT,

        /// <summary>
        ///     Less than (i.e. &lt;).
        /// </summary>
        LT,

        /// <summary>
        ///     Equal to (i.e. ==).
        /// </summary>
        EQ,

        /// <summary>
        ///     Not equal to (i.e. !=).
        /// </summary>
        NEQ,

        /// <summary>
        ///     Addition (i.e. +).
        /// </summary>
        ADD,

        /// <summary>
        ///     Subtraction (i.e. -).
        /// </summary>
        SUB,

        /// <summary>
        ///     Multiplication (i.e. *).
        /// </summary>
        MUL,

        /// <summary>
        ///     Division (i.e. /).
        /// </summary>
        DIV,

        /// <summary>
        ///     Prefix/postfix inc (i.e. ++).
        /// </summary>
        INC,

        /// <summary>
        ///     Prefix/postfix dec (i.e. --).
        /// </summary>
        DEC,

        /// <summary>
        ///     Left bracket (i.e. ().
        /// </summary>
        LEFT_BRACKET,

        /// <summary>
        ///     Right bracket (i.e. )).
        /// </summary>
        RIGHT_BRACKET,

        /// <summary>
        ///     Comma (i.e. ,).
        /// </summary>
        COMMA,

        /// <summary>
        ///     NULL (i.e. null).
        /// </summary>
        NULL,

        /// <summary>
        ///     Integer value (e.g. 123).
        /// </summary>
        INT,

        /// <summary>
        ///     Boolean value (i.e. true, false).
        /// </summary>
        BOOL,

        /// <summary>
        ///     Float value (e.g. 1.5, -0.3e-2).
        /// </summary>
        FLOAT,

        /// <summary>
        ///     String (e.g. 'in single quotes').
        /// </summary>
        STRING,

        /// <summary>
        ///     Function (i.e. SomeProperty, SomeType.CONST, SomeEnumType.SomeValue, SomeArray[0], SomeFunction(...)).
        /// </summary>
        FUNC,

        /// <summary>
        ///     End of expression.
        /// </summary>
        EOF
    }
}
