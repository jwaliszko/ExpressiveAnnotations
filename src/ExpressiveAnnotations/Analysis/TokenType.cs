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
        L_AND,

        /// <summary>
        ///     Logical disjunction (i.e. ||).
        /// </summary>
        L_OR,

        /// <summary>
        ///     Bitwise AND (i.e. &amp;).
        /// </summary>
        B_AND,

        /// <summary>
        ///     Bitwise inclusive OR (i.e. |).
        /// </summary>
        B_OR,

        /// <summary>
        ///     Bitwise exclusive OR (i.e. ^).
        /// </summary>
        XOR,

        /// <summary>
        ///     Logical negation (i.e. !).
        /// </summary>
        L_NOT,

        /// <summary>
        ///     Bitwise NOT (i.e. ~).
        /// </summary>
        B_NOT,

        /// <summary>
        ///     Bitwise left shift (i.e. &lt;&lt;).
        /// </summary>
        L_SHIFT,

        /// <summary>
        ///     Bitwise right shift (i.e. &gt;&gt;).
        /// </summary>
        R_SHIFT,

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
        ///     Modulo (i.e. %).
        /// </summary>
        MOD,

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
        L_BRACKET,

        /// <summary>
        ///     Right bracket (i.e. )).
        /// </summary>
        R_BRACKET,

        /// <summary>
        ///     Question mark (i.e. ?).
        /// </summary>
        QMARK,

        /// <summary>
        ///     Colon (i.e. :).
        /// </summary>
        COLON,

        /// <summary>
        ///     Comma (i.e. ,).
        /// </summary>
        COMMA,

        /// <summary>
        ///     NULL (i.e. null).
        /// </summary>
        NULL,

        /// <summary>
        ///     Integer literal (e.g. 123).
        /// </summary>
        INT,

        /// <summary>
        ///     Boolean literal (i.e. true, false).
        /// </summary>
        BOOL,

        /// <summary>
        ///     Float literal (e.g. 1.5, -0.3e-2).
        /// </summary>
        FLOAT,

        /// <summary>
        ///     Binary literal (e.g. 0b101).
        /// </summary>
        BIN,

        /// <summary>
        ///     Hexadecimal literal (e.g. 0xFF).
        /// </summary>
        HEX,

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
