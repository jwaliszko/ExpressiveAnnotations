![logo](logo.png)

# <a id="expressiveannotations-annotation-based-conditional-validation">ExpressiveAnnotations<sup><sup><sup>[annotation-based conditional validation]</sup></sup></sup></a>

[![Build status](https://img.shields.io/appveyor/ci/jwaliszko/ExpressiveAnnotations.svg)](https://ci.appveyor.com/project/jwaliszko/ExpressiveAnnotations)
[![Coverage status](https://img.shields.io/codecov/c/github/jwaliszko/ExpressiveAnnotations.svg)](https://codecov.io/github/jwaliszko/ExpressiveAnnotations)
[![Release version](https://img.shields.io/github/release/jwaliszko/ExpressiveAnnotations.svg)](https://github.com/jwaliszko/ExpressiveAnnotations/releases/latest)
[![License](http://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

A small .NET and JavaScript library which provides annotation-based conditional validation mechanisms. Given attributes allow to forget about imperative way of step-by-step verification of validation conditions in many cases. Since fields validation requirements are applied as metadata, domain-related code is more condensed.

### Table of contents
 - [What is the context behind this work?](#what-is-the-context-behind-this-implementation)
 - [RequiredIf vs. AssertThat - where is the difference?](#requiredif-vs-assertthat---where-is-the-difference)
 - [Sample projects + demo](#sample-projects-+-demo)
 - [What are a brief examples of usage?](#what-are-a-brief-examples-of-usage)
 - [Declarative vs. imperative programming - what is it about?](#declarative-vs-imperative-programming---what-is-it-about)
 - [EA expressions specification](#expressions-specification)
   - [Grammar definition](#grammar-definition)
   - [Where syntax meets semantics](#where-syntax-meets-semantics)
   - [Operators precedence and associativity](#operators-precedence)
   - [Built-in functions (methods ready to be used by expressions)](#built-in-functions)
 - [How to construct conditional validation attributes?](#how-to-construct-conditional-validation-attributes)
   - [Signatures description](#signatures)
   - [Implementation details outline](#implementation)
   - [Traps (discrepancies between server- and client-side expressions evaluation)](#traps)
 - [What about the support of ASP.NET MVC client-side validation?](#what-about-the-support-of-aspnet-mvc-client-side-validation)
 - [Frequently asked questions](#frequently-asked-questions)
   - [Is it possible to compile all usages of annotations at once?](#is-it-possible-to-compile-all-usages-of-annotations-at-once) <sup>(re server-side)</sup>
   - [What if there is no built-in function I need?](#what-if-there-is-no-built-in-function-i-need) <sup>(re client and server-side)</sup>
   - [Can I have custom utility-like functions outside of my models?](#can-I-have-custom-utility-like-functions-outside-of-my-models) <sup>(re server-side)</sup>
   - [How to cope with values of custom types?](#how-to-cope-with-values-of-custom-types) <sup>(re client-side)</sup>
   - [How to cope with dates given in non-standard formats?](#how-to-cope-with-dates-given-in-non-standard-formats) <sup>(re client-side)</sup>
   - [What if "ea" variable is already used by another library?](#what-if-ea-variable-is-already-used-by-another-library) <sup>(re client-side)</sup>
   - [How to control frequency of dependent fields validation?](#how-to-control-frequency-of-dependent-fields-validation) <sup>(re client-side)</sup>
   - [Can I increase web console verbosity for debug purposes?](#can-i-increase-web-console-verbosity-for-debug-purposes) <sup>(re client-side)</sup>
   - [I prefer enum values to be rendered as numbers, is it allowed?](#i-prefer-enum-values-to-be-rendered-as-numbers-is-it-allowed) <sup>(re client-side)</sup>
   - [How to fetch field's value or its display name in error message?](#how-to-fetch-fields-value-or-its-display-name-in-error-message) <sup>(re client and server-side)</sup>
   - [Is there any event raised when validation is done?](#is-there-any-event-raised-when-validation-is-done) <sup>(re client-side)</sup>
   - [Can I decorate conditionally required fields with asterisks?](#can-i-decorate-conditionally-required-fields-with-asterisks) <sup>(re client-side)</sup>
   - [How to handle validation based on parent model field?](#how-to-handle-validation-based-on-parent-model-field) <sup>(re client and server-side)</sup>
   - [RequiredIf attribute is not working, what is wrong?](#requiredif-attribute-is-not-working-what-is-wrong) <sup>(re client and server-side)</sup>
   - [Few fields seem to be bypassed during validation, any clues?](#few-fields-seem-to-be-bypassed-during-validation-any-clues) <sup>(re client-side)</sup>
   - [Client-side validation doesn't work, how to troubleshoot it?](#client---side-validation-doesnt-work-how-to-troubleshoot-it) <sup>(re client-side)</sup>
   - [Is there a possibility to perform asynchronous validation?](#is-there-a-possibility-to-perform-asynchronous-validation) <sup>(re client-side, experimental)</sup>
   - [What if my question is not covered by FAQ section?](#what-if-my-question-is-not-covered-by-faq-section)
 - [Installation instructions](#installation)
 - [Contributors](#contributors)
 - [License](#license)

### <a id="what-is-the-context-behind-this-implementation">What is the context behind this work?</a>

There are number of cases where the concept of metadata is used for justified reasons. Attributes are one of the ways to associate complementary information with existing data. Such annotations may also define the correctness of data.

Declarative validation when [compared](#declarative-vs-imperative-programming---what-is-it-about) to imperative approach seems to be more convenient in many cases. Clean, compact code - all validation logic defined within the model scope. Simple to write, obvious to read.

### <a id="requiredif-vs-assertthat---where-is-the-difference">RequiredIf vs. AssertThat - where is the difference?</a>

* `RequiredIf` - if value is not yet provided, check whether it is required (annotated field is required to be non-null, when given condition is satisfied),
* `AssertThat` - if value is already provided, check whether the condition is met (non-null annotated field is considered as valid, when given condition is satisfied).

### <a id="sample-projects-+-demo">Sample projects + demo</a>

* [**ASP.NET MVC web sample**](src/ExpressiveAnnotations.MvcWebSample),
* [**WPF MVVM desktop sample**](src/ExpressiveAnnotations.MvvmDesktopSample).

ASP.NET MVC web sample is also hosted online - http://expressiveannotations.net/.

### <a id="what-are-a-brief-examples-of-usage">What are a brief examples of usage?</a>

This section presents few exemplary code snippets. Sample projects in the section above contain much more comprehensive set of use cases.

```C#
using ExpressiveAnnotations.Attributes;

[RequiredIf("GoAbroad == true")]
public string PassportNumber { get; set; }
```

Above we are saying, that annotated field is required when condition given in the logical expression is satisfied (passport number is required, if go abroad field has true boolean value).

Simple enough, let's move to another variation:

```C#
[AssertThat("ReturnDate >= Today()")]
public DateTime? ReturnDate { get; set; }
```

By the usage of this attribute type, we are not validating field requirement as before - its value is allowed to be null this time. Nevertheless, if some value is already given, provided restriction needs to be satisfied (return date needs to be greater than or equal to the date returned by `Today()` [built-in function](#built-in-functions)).

As shown below, both types of attributes may be combined (moreover, the same type can be applied multiple times for a single field):

```C#
[RequiredIf("Details.Email != null")]
[RequiredIf("Details.Phone != null")]
[AssertThat("AgreeToContact == true")]
public bool? AgreeToContact { get; set; }
```

Literal translation means, that if either email or phone is provided, you are forced to authorize someone to contact with you (boolean value indicating contact permission has to be true). What is more, we can see that nested properties are supported by [the expressions parser](#implementation).

The complexity of expressions may be arbitrarily increased, e.g. take a brief look at the following construction:

```C#
[RequiredIf(@"GoAbroad == true
              && (
                     (NextCountry != 'Other' && NextCountry == Country)
                     || (Age > 24 && Age <= 55)
                 )")]
public string ReasonForTravel { get; set; }
```

Restriction above, despite being more specific than its predecessors, still can be quickly understood (reason for travel has to be provided if you plan to go abroad and, either want to visit the same definite country twice, or are between 25 and 55).

Conditional operations are supported (ternary operator defined). You can imply various assertions on specific field based on certain condition (or nested conditions), e.g.

```C#
[AssertThat("Switch == 'ON' ? Voltage1 == Voltage2 : true")]
public int Voltage1 { get; set; }
```

Here, when switch is ON, voltages must be equal - otherwise everything is OK. You could express the same statement without conditional operator, i.e.

```C#
[AssertThat("Switch == 'ON' && (Voltage1 == Voltage2) || (Switch != 'ON')")]
```

but it is less verbose.

### <a id="declarative-vs-imperative-programming---what-is-it-about">Declarative vs. imperative programming - what is it about?</a>

With **declarative** programming you write logic that expresses *what* you want, but not necessarily *how* to achieve it. You declare your desired results, but not step-by-step.

In our case, this concept is materialized by attributes, e.g.

```C#
[RequiredIf("GoAbroad == true && NextCountry != 'Other' && NextCountry == Country",
    ErrorMessage = "If you plan to travel abroad, why visit the same country twice?")]
public string ReasonForTravel { get; set; }
```

Here, we're saying "Ensure the field is required according to given condition."

With **imperative** programming you define the control flow of the computation which needs to be done. You tell the compiler what you want, exactly step by step.

If we choose this way instead of model fields decoration, it has negative impact on the complexity of the code. Logic responsible for validation is now implemented somewhere else in our application, e.g. inside controllers actions instead of model class itself:

```C#
    if (!model.GoAbroad)
        return View("Success");
    if (model.NextCountry == "Other")
        return View("Success");
    if (model.NextCountry != model.Country)
        return View("Success");

    ModelState.AddModelError("ReasonForTravel",
        "If you plan to travel abroad, why visit the same country twice?");
    return View("Home", model);
}
```

Here instead, we're saying "If condition is met, return some view. Otherwise, add error message to state container. Return other view."

### <a id="expressions-specification">EA expressions specification</a>

##### <a id="grammar">Grammar definition</a>

Expressions handled by EA parser must comply with the following grammar:

```
exp         => cond-exp
cond-exp    => l-or-exp ['?' exp ':' exp]       // right associative (right recursive)
l-or-exp    => l-and-exp  ('||' l-and-exp)*     // left associative (non-recursive alternative to left recursion)
l-and-exp   => b-and-exp ('&&' b-and-exp)*
b-or-exp    => xor-exp ('|' xor-exp)*
xor-exp     => b-and-exp ('^' b-and-exp)*
b-and-exp   => eq-exp ('&' eq-exp)*
eq-exp      => rel-exp (('==' | '!=') rel-exp)*
rel-exp     => shift-exp (('>' | '>=' | '<' | '<=') shift-exp)*
shift-exp   => add-exp (('<<' | '>>') add-exp)*
add-exp     => mul-exp (('+' | '-') mul-exp)*
mul-exp     => unary-exp (('*' | '/' | '%')  unary-exp)*
unary-exp   => ('+' | '-' | '!' | '~') unary-exp | primary-exp
primary-exp => null-lit | bool-lit | num-lit | string-lit | arr-access | id-access | '(' exp ')'

arr-access  => arr-lit |
               arr-lit '[' exp ']' ('[' exp ']' | '.' identifier)*

id-access   => identifier |
               identifier ('[' exp ']' | '.' identifier)* |
               func-call ('[' exp ']' | '.' identifier)*

func-call   => identifier '(' [exp-list] ')'

null-lit    => 'null'
bool-lit    => 'true' | 'false'
num-lit     => int-lit | float-lit
int-lit     => dec-lit | bin-lit | hex-lit
array-lit   => '[' [exp-list] ']'

exp-list    => exp (',' exp)*
```

Terminals are expressed in quotes. Each nonterminal is defined by a rule in the grammar except for *dec-lit*, *bin-lit*, *hex-lit*, *float-lit*, *string-lit* and *identifier*, which are assumed to be implicitly defined (*identifier* specifies names of functions, properties, constants and enums).

Expressions are built of Unicode letters and numbers (i.e. `[L*]` and `[N*]` [categories](https://en.wikipedia.org/wiki/Unicode_character_property) respectively) with the usage of the following components:

* logical operators: `!a`, `a||b`, `a&&b`,
* comparison operators: `a==b`, `a!=b`, `a<b`, `a<=b`, `a>b`, `a>=b`,
* arithmetic operators: `+a`, `-a`, `a+b`, `a-b`, `a*b`, `a/b`, `a%b`, `~a`, `a&b`, `a^b`, `a|b`, `a<<b`, `a>>b`,
* other operators: `a()`, `a[]`, `a.b`, `a?b:c`,
* literals:
  * null, i.e. `null`,
  * boolean, i.e. `true` and `false`,
  * decimal integer, e.g. `123`,
  * binary integer (with `0b` prefix), e.g. `0b1010`,
  * hexadecimal integer (with `0x` prefix), e.g. `0xFF`,
  * float, e.g. `1.5` or `0.3e-2`,
  * string, e.g. `'in single quotes'` (internal quote escape sequence is `\'`, character representing new line is `\n`),
  * array (comma separated items within square brackets), e.g. `[1,2,3]`,
  * identifier, i.e. names of functions, properties, constants and enums.

##### <a id="where-syntax-meets-semantics">Where syntax meets semantics</a>

EA expressions syntax is defined by the grammar shown above.

Valid expressions must follow not only lexical and syntax level rules, but must have appropriate semantics as well.

Grammars, as already mentioned, naturally define the language syntax - but not only. Grammars in general contribute to the semantics a little, by defining the parse tree - thus denoting the precedence and associativity of operators. This being said, it is not just about the legal string being produced - the structure of parse tree corresponds to the order, in which various parts of the expression are to be evaluated.

Such contribution to semantics is not enough though. Grammar says nothing about the types of operands. For the expressions to have valid semantics, all the requirements on the types of operands must be realized as well.

Prior to execution of an operation, type checks and eventual type conversions are made. The result type, e.g. of a valid binary operation is, most of the time but not always, the same as the most general of the input types. When a binary operation is used then a generalization of operands is performed, to make the two operands the same (most general) type, before the operation is done. The order of generalization, from most general to most specific, is briefly described as follows: `string` (generalization done using the current locale) -> `double` -> `int`. Also nullable types are more general than their non-nullable counterparts.

##### <a id="operators-precedence">Operators precedence and associativity</a>

The following table lists the precedence and associativity of operators (listed top to bottom, in descending precedence):

<table>
    <thead>
        <tr>
            <th>Precedence</th>
            <th>Operator</th>
            <th>Description</th>
            <th>Associativity</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td valign="top" rowspan="3">1</td>
            <td>
                <code>()</code><br />
            </td>
            <td>Function call (postfix)</td>
            <td valign="top" rowspan="3">Left to right</td>
        </tr>
        <tr>
            <td>
                <code>[]</code><br />
            </td>
            <td>Subscript (postfix)</td>
        </tr>
        <tr>
            <td>
                <code>.</code>
            </td>
            <td>Member access (postfix)</td>
        </tr>
        <tr>
            <td valign="top" rowspan="2">2</td>
            <td>
                <code>+</code> <code>-</code>
            </td>
            <td>Unary plus and minus</td>
            <td valign="top" rowspan="2">Right to left</td>
        </tr>
        <tr>
            <td>
                <code>!</code> <code>~</code>
            </td>
            <td>Logical NOT and bitwise NOT (one's complement)</td>
        </tr>
        <tr>
            <td>3</td>
            <td>
                <code>*</code> <code>/</code> <code>%</code>
            </td>
            <td>Multiplication, division, and remainder</td>
            <td valign="top" rowspan="11">Left to right</td>
        </tr>
        <tr>
            <td>4</td>
            <td>
                <code>+</code> <code>-</code>
            </td>
            <td>Addition and subtraction</td>
        </tr>
        <tr>
            <td>5</td>
            <td>
                <code>&lt;&lt;</code> <code>&gt;&gt;</code>
            </td>
            <td>Bitwise left shift and right shift</td>
        </tr>
        <tr>
            <td valign="top" rowspan="2">6</td>
            <td>
                <code>&lt;</code> <code>&lt;=</code>
            </td>
            <td>Relational operators &lt; and ≤ respectively</td>
        </tr>
        <tr>
            <td>
                <code>&gt;</code> <code>&gt;=</code>
            </td>
            <td>Relational operators &gt; and ≥ respectively</td>
        </tr>
        <tr>
            <td>7</td>
            <td>
                <code>==</code> <code>!=</code>
            </td>
            <td>Equality operators = and ≠ respectively</td>
        </tr>
        <tr>
            <td>8</td>
            <td><code>&amp;</code></td>
            <td>Bitwise AND</td>
        </tr>
        <tr>
            <td>9</td>
            <td><code>^</code></td>
            <td>Bitwise XOR (exclusive OR)</td>
        </tr>
        <tr>
            <td>10</td>
            <td><code>|</code></td>
            <td>Bitwise OR (inclusive OR)</td>
        </tr>
        <tr>
            <td>11</td>
            <td><code>&amp;&amp;</code></td>
            <td>Logical AND</td>
        </tr>
        <tr>
            <td>12</td>
            <td><code>||</code></td>
            <td>Logical OR</td>
        </tr>
        <tr>
            <td>13</td>
            <td><code>?:</code></td>
            <td>Ternary conditional</td>
            <td>Right to left</td>
        </tr>
    </tbody>
</table>

##### <a id="built-in-functions">Built-in functions (methods ready to be used by expressions)</a>

As already noted, there is an option to reinforce expressions with functions, e.g.

```C#
[AssertThat("StartsWith(CodeName, 'abc.') || EndsWith(CodeName, '.xyz')")]
public string CodeName { get; set; }
```

Toolchain functions available out of the box at server- and client-side:

* `DateTime Now()`
    * Gets the current local date and time (client-side returns the number of milliseconds since January 1, 1970, 00:00:00 UTC).
* `DateTime Today()`
    * Gets the current date with the time component set to 00:00:00 (client-side returns the number of milliseconds since January 1, 1970, 00:00:00 UTC).
* `DateTime Date(int year, int month, int day)`
    * Initializes a new date to a specified year, month (months are 1-based), and day, with the time component set to 00:00:00 (client-side returns the number of milliseconds since January 1, 1970, 00:00:00 UTC).
* `DateTime Date(int year, int month, int day, int hour, int minute, int second)`
    * Initializes a new date to a specified year, month (months are 1-based), day, hour, minute, and second (client-side returns the number of milliseconds since January 1, 1970, 00:00:00 UTC).
* `DateTime ToDate(string dateString)`
    * Converts the specified string representation of a date and time to its equivalents: `DateTime` at server-side - uses .NET [`DateTime.Parse(string dateString)`](https://msdn.microsoft.com/en-us/library/vstudio/1k1skd40(v=vs.100).aspx), and number of milliseconds since January 1, 1970, 00:00:00 UTC at client-side - uses JavaScript [`Date.parse(dateString)`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse).
* `TimeSpan TimeSpan(int days, int hours, int minutes, int seconds)`
    * Initializes a new time period according to specified days, hours, minutes, and seconds (client-side period is expressed in milliseconds).
* `int Length(str)`
    * Gets the number of characters in a specified string (null-safe).
* `string Trim(string str)`
    * Removes all leading and trailing white-space characters from a specified string (null-safe).
* `string Concat(string strA, string strB)`
    * Concatenates two specified strings (null-safe).
* `string Concat(string strA, string strB, strC)`
    * Concatenates three specified strings (null-safe).
* `int CompareOrdinal(string strA, string strB)`
    * Compares strings using ordinal sort rules. An integer that indicates the lexical relationship
      between the two comparands is returned (null-safe):
        * -1    - strA is less than strB,
        * &nbsp;0    - strA and strB are equal,
        * &nbsp;1    - strA is greater than strB.
* `int CompareOrdinalIgnoreCase(string strA, string strB)`
    * Compares strings using ordinal sort rules and ignoring the case of the strings being compared (null-safe).
* `bool StartsWith(string str, string prefix)`
    * Determines whether the beginning of specified string matches a specified prefix (null-safe).
* `bool StartsWithIgnoreCase(string str, string prefix)`
    * Determines whether the beginning of specified string matches a specified prefix, ignoring the case of the strings (null-safe).
* `bool EndsWith(string str, string suffix)`
    * Determines whether the end of specified string matches a specified suffix (null-safe).
* `bool EndsWithIgnoreCase(string str, string suffix)`
    * Determines whether the end of specified string matches a specified suffix, ignoring the case of the strings (null-safe).
* `bool Contains(string str, string substr)`
    * Returns a value indicating whether a specified substring occurs within a specified string (null-safe).
* `bool ContainsIgnoreCase(string str, string substr)`
    * Returns a value indicating whether a specified substring occurs within a specified string, ignoring the case of the strings (null-safe).
* `bool IsNullOrWhiteSpace(string str)`
    * Indicates whether a specified string is null, empty, or consists only of white-space characters (null-safe).
* `bool IsDigitChain(string str)`
    * Indicates whether a specified string represents a sequence of digits (ASCII characters only, null-safe).
* `bool IsNumber(string str)`
    * Indicates whether a specified string represents integer or float number (ASCII characters only, null-safe).
* `bool IsEmail(string str)`
    * Indicates whether a specified string represents valid e-mail address (null-safe).
* `bool IsPhone(string str)`
    * Indicates whether a specified string represents valid phone number (null-safe).
* `bool IsUrl(string str)`
    * Indicates whether a specified string represents valid url (null-safe).
* `bool IsRegexMatch(string str, string regex)`
    * Indicates whether the regular expression finds a match in the input string (null-safe).
* `Guid Guid(string str)`
    * Initializes a new instance of the Guid structure by using the value represented by a specified string.
* `double Min(params double[] values)`
    * Returns the minimum value in a sequence of numeric values.
* `double Max(params double[] values)`
    * Returns the maximum value in a sequence of numeric values.
* `double Sum(params double[] values)`
    * Computes the sum of the numeric values in a sequence.
* `double Average(params double[] values)`
    * Computes the average of the numeric values in a sequence.

### <a id="how-to-construct-conditional-validation-attributes">How to construct conditional validation attributes?</a>

##### <a id="signatures">Signatures description</a>

```C#
RequiredIfAttribute(
    string expression,
    [bool AllowEmptyStrings],
    [int Priority]
    [string ErrorMessage]    ...) /* Validation attribute, executed for null-only annotated
                                   * field, which indicates that such a field is required
                                   * to be non-null, when computed result of given logical
                                   * expression is true. */
AssertThatAttribute(
    string expression,
    [int Priority]
    [string ErrorMessage]    ...) /* Validation attribute, executed for non-null annotated
                                   * field, which indicates that assertion given in logical
                                   * expression has to be satisfied, for such a field to be
                                   * considered as valid. */
```

```
expression        - The logical expression based on which specified condition is computed.
AllowEmptyStrings - Gets or sets a flag indicating whether the attribute should allow empty
                    or whitespace strings. False by default.
Priority          - Gets or sets the hint, available for any concerned external components,
                    indicating the order in which this attribute should be executed among
                    others of its kind. Value is optional and not set by default, which 
                    means that execution order is undefined.
ErrorMessage      - Gets or sets an explicit error message string. A difference to default
                    behavior is awareness of new format items, i.e. {fieldPath[:indicator]}.
                    Given in curly brackets, can be used to extract values of specified
                    fields, e.g. {field}, {field.field}, within current model context or
                    display names of such fields, e.g. {field:n}. Braces can be escaped by
                    double-braces, i.e. to output a { use {{ and to output a } use }}. The
                    same logic works for messages provided in resources.
```

Note above covers almost exhaustively what is actually needed to work with EA. Nevertheless, the full API documentation, generated with [Sandcastle](https://sandcastle.codeplex.com/) (with the support of [SHFB](http://shfb.codeplex.com/)), can be downloaded (in the form of compiled HTML help file) from [here](doc/api/api.chm?raw=true) (includes only C# API, no JavaScript part there).

##### <a id="implementation">Implementation details outline</a>

Implementation core is based on [expressions parser](src/ExpressiveAnnotations/Analysis/Parser.cs?raw=true), which runs on the grammar [shown above](#grammar-definition).

Firstly, at the lexical analysis stage, character stream of the expression is converted into token stream (whitespaces ignored, characters grouped into tokens and associated with position in the text). Next, at the syntax analysis level, abstract syntax tree is constructed according to the rules defined by the grammar. While the tree is being built, also the 3rd stage, mainly semantic analysis, is being performed. This stage is directly related to operands type checking (and eventual type conversions according to type generalization rules, when incompatible types are detected).

Based on valid expression string [expression tree](http://msdn.microsoft.com/en-us/library/bb397951.aspx) structure is finally being built. A delegate containing compiled version of the lambda expression (defined by the expression tree) is returned as a result of the parsersing mechanism. Such delegate can be then invoked for specified model object.

When expression is provided to the attribute, it should be of a boolean type. The result of its evaluation indicates whether the assertion or requirement condition is satisfied or not.

For the sake of performance optimization, expressions provided to attributes are compiled only once. Such compiled lambdas are then cached inside attributes instances and invoked for any subsequent validation requests without recompilation.

When working with ASP.NET MVC stack, unobtrusive client-side validation mechanism is [additionally available](#what-about-the-support-of-aspnet-mvc-client-side-validation). Client receives unchanged expression string from server. Such an expression is then evaluated using JavaScript [`eval()`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/eval) method within the context of reflected model object. Such a model, analogously to the server-side one, is basically deserialized DOM form (with some type-safety assurances and registered toolchain methods).

##### <a id="traps">Traps (discrepancies between server- and client-side expressions evaluation)</a>

Because client-side handles expressions in its unchanged form (as provided to attribute), attention is needed when dealing with `null` keyword - there are discrepancies between EA parser (mostly follows C# rules) and JavaScript, e.g.

* `null + "text"` - in C# `"text"`, in JS `"nulltext"`,
* `2 * null`      - in C# `null`  , in JS `0`,
* `null > -1`     - in C# `false` , in JS `true`,
* and more...

### <a id="what-about-the-support-of-aspnet-mvc-client-side-validation">What about the support of ASP.NET MVC client-side validation?</a>

Client-side validation is fully supported. Enable it for your web project within the next few steps:

1. Reference both assemblies to your project: core [**ExpressiveAnnotations.dll**](src/ExpressiveAnnotations) and subsidiary [**ExpressiveAnnotations.MvcUnobtrusive.dll**](src/ExpressiveAnnotations.MvcUnobtrusive).
2. In Global.asax register required validators (`IClientValidatable` interface is not directly implemented by the attributes, to avoid coupling of ExpressionAnnotations assembly with System.Web.Mvc dependency):

    ```C#
    using ExpressiveAnnotations.Attributes;
    using ExpressiveAnnotations.MvcUnobtrusive.Validators;

    protected void Application_Start()
    {
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (AssertThatAttribute), typeof (AssertThatValidator));
    ```
    
    Alternatively, use predefined `ExpressiveAnnotationsModelValidatorProvider` (recommended):
    
    ```C#
    using ExpressiveAnnotations.MvcUnobtrusive.Providers;

    protected void Application_Start()
    {
        ModelValidatorProviders.Providers.Remove(
            ModelValidatorProviders.Providers
                .FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
        ModelValidatorProviders.Providers.Add(
            new ExpressiveAnnotationsModelValidatorProvider());
    ```
    
    Despite the fact this provider automatically registers adapters for expressive validation attributes, it additionally respects their processing priorities when validation is performed (i.e. the [`Priority`](#signatures) property actually means something in practice).
3. Include [**expressive.annotations.validate.js**](src/expressive.annotations.validate.js?raw=true) script in your page (it should be included in bundle below jQuery validation files):

    ```JavaScript
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.validate.js"></script>
    ```

For supplementary reading visit the [installation section](#installation).

### <a id="frequently-asked-questions">Frequently asked questions</a>

##### <a id="is-it-possible-to-compile-all-usages-of-annotations-at-once">Is it possible to compile all usages of annotations at once?</a>

Yes, a complete list of types with annotations can be retrieved and compiled collectively. It can be useful, e.g. during unit testing phase, when without the necessity of your main application startup, all the compile-time errors (syntax errors, type checking errors) done to your expressions can be discovered. The following extension is helpful:

```C#
public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Type type)
{
    var properties = type.GetProperties()
        .Where(p => Attribute.IsDefined(p, typeof (ExpressiveAttribute)));
    var attributes = new List<ExpressiveAttribute>();
    foreach (var prop in properties)
    {
        var attribs = prop.GetCustomAttributes<ExpressiveAttribute>().ToList();
        attribs.ForEach(x => x.Compile(prop.DeclaringType));
        attributes.AddRange(attribs);
    }
    return attributes;
}
```

with the succeeding usage manner:

```C#
// compile all expressions for specified model:
var compiled = typeof (SomeModel).CompileExpressiveAttributes().ToList();

// ... or for current assembly:
compiled = Assembly.GetExecutingAssembly().GetTypes()
    .SelectMany(t => t.CompileExpressiveAttributes()).ToList();

// ... or for all assemblies within current domain:
compiled = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes()
        .SelectMany(t => t.CompileExpressiveAttributes())).ToList();
```

Notice that such compiled lambdas will be cached inside attributes instances stored in `compiled` list.
That means that subsequent compilation requests:

```C#
compiled.ForEach(x => x.Compile(typeof (SomeModel));
```

do nothing (due to optimization purposes), unless invoked with enabled recompilation switch:

```C#
compiled.ForEach(x => x.Compile(typeof (SomeModel), force: true);
```

Finally, this solution reveals compile-time errors only, you can still can get runtime errors though, e.g.:

```C#
var parser = new Parser();
parser.AddFunction<object, bool>("CastToBool", obj => (bool) obj);

parser.Parse<object>("CastToBool(null)"); // compilation succeeds
parser.Parse<object>("CastToBool(null)").Invoke(null); // invocation fails (type casting err)
```

##### <a id="what-if-there-is-no-built-in-function-i-need">What if there is no built-in function I need?</a>

Create it yourself. Any custom function defined within the model class scope at server-side is automatically recognized and can be used inside expressions, e.g.

```C#
class Model
{
    public bool IsBloodType(string group)
    {
        return Regex.IsMatch(group, @"^(A|B|AB|0)[\+-]$");
    }

    [AssertThat("IsBloodType(BloodType)")] // method known here (context aware expressions)
    public string BloodType { get; set; }
```

 If client-side validation is needed as well, function of the same signature (name and the number of parameters) must be available there. JavaScript corresponding implementation should be registered by the following instruction:
 
```JavaScript
<script>
    ea.addMethod('IsBloodType', function(group) {
        return /^(A|B|AB|0)[\+-]$/.test(group);
    });
```

Many signatures can be defined for a single function name. Types are not taken under consideration as a differentiating factor though. Methods overloading is based on the number of arguments only. Functions with the same name and exact number of arguments are considered as ambiguous. The next issue important here is the fact that custom methods take precedence over built-in ones. If exact signatures are provided built-in methods are simply overridden by new definitions.

##### <a id="can-I-have-custom-utility-like-functions-outside-of-my-models">Can I have custom utility-like functions outside of my models?

Sure, provide your own methods provider, or extend existing global one, i.e.

* extend existing provider:

 ```C#
    protected void Application_Start()
    {
        Toolchain.Instance.AddFunction<int[], int>("ArrayLength", array => array.Length);
```

* define new provider:

 ```C#
    public class CustomFunctionsProvider : IFunctionsProvider
    {
        public IDictionary<string, IList<LambdaExpression>> GetFunctions()
        {
            return new Dictionary<string, IList<LambdaExpression>>
            {
                {"ArrayLength", new LambdaExpression[] {(Expression<Func<int[], int>>) (array => array.Length)}}
            };
        }
    }

    protected void Application_Start()
    {
        Toolchain.Instance.Recharge(new CustomFunctionsProvider());
```

##### <a id="how-to-cope-with-values-of-custom-types">How to cope with values of custom types?</a>

If you need to handle value string extracted from DOM field in any non built-in way, you can redefine given type-detection logic. The default mechanism recognizes and handles automatically types identified as: `timespan`, `datetime`, `enumeration`, `number`, `string`, `bool` and `guid`. If non of them is matched for a particular field, JSON deserialization is invoked. You can provide your own deserializers though. The process is as follows:

* at server-side decorate your property with special attribute which gives a hint to client-side, which parser should be chosen for corresponding DOM field value deserialization:

    ```C#
    class Model
    {
        [ValueParser('customparser')]
        public CustomType SomeField { get; set; }
    ```

* at client-side register such a parser:

    ```JavaScript
    <script>
        ea.addValueParser('customparser', function(value, field) {
            // parameters: value - raw data string extracted by default from DOM element
            //             field - DOM element name for which parser was invoked
            return ... // handle extracted field value string on your own
        });
    ```

Finally, there is a possibility to override built-in conversion globally. In this case, use the type name to register your value parser - all fields of such a type will be intercepted by it, e.g.

```JavaScript
<script>
    ea.addValueParser('typename', function(value) {
        return ... // handle specified type (numeric, datetime, etc.) parsing on your own
    });
```

If you redefine default mechanism, you can still have the `ValueParser` annotation on any fields you consider exceptional - annotation gives the highest parsing priority.

##### <a id="how-to-cope-with-dates-given-in-non-standard-formats">How to cope with dates given in non-standard formats?</a>

When values of DOM elements are extracted, they are converted to appropriate types. For fields containing date strings, JavaScript `Date.parse()` method is used by default. As noted in [MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse), the input parameter is:

>A string representing an RFC 2822 or ISO 8601 date (other formats may be used, but results may be unexpected)

When some non-standard format needs to be handled, simply override the default behavior and provide your own implementation. E.g. when dealing with UK format dd/mm/yyyy, solution is:

```C#
class Model
{
    [ValueParser('ukdateparser')]
    public DateTime SomeField { get; set; }
```

```JavaScript
<script>
    ea.addValueParser('ukdateparser', function(value) {
        var arr = value.split('/');
        var date = new Date(arr[2], arr[1] - 1, arr[0]);
        return date.getTime(); // return msecs since January 1, 1970, 00:00:00 UTC
    });
```

##### <a id="what-if-ea-variable-is-already-used-by-another-library">What if "ea" variable is already used by another library?</a>

Use `noConflict()` method. In case of naming collision return control of the `ea` variable back to its origins. Old references of `ea` are saved during ExpressiveAnnotations initialization - `noConflict()` simply restores them:

```JavaScript
<script src="another.js"></script>
<script src="expressive.annotations.validate.js"></script>
<script>
    var expann = ea.noConflict(); // relinquish EA's control of the `ea` variable
    expann.addMethod... // do something with ExpressiveAnnotations
    ea... // do something with original ea variable
```

##### <a id="how-to-control-frequency-of-dependent-fields-validation">How to control frequency of dependent fields validation?</a>

When a field value is modified, validation results for some other fields, directly dependent on currenty modified one, may be affected. To control the frequency of when dependent fields validation is triggered, change default `ea.settings.dependencyTriggers` settings. It is a string containing one or more DOM field event types (such as *change*, *keyup* or custom event names), associated with currently modified field, for which fields directly dependent on are validated. Multiple event types can be bound at once by including each one separated by a space.

Default value is *'change keyup'* (for more information check `eventType` parameter of jQuery [`bind()`](http://api.jquery.com/bind) method). If you want to turn this feature off entirely, set it to empty string, *null* or *undefined* (validation will be fired on form submit attempt only).

```JavaScript
<script>
    ea.settings.dependencyTriggers = 'change'; // mute some excessive activity if you wish,
                                               // or turn it off entirely (set to undefined)
```

Alternatively, to enforce re-binding of already attached validation handlers, use following construction:

```JavaScript
<script>
    ea.settings.apply({
        dependencyTriggers: 'new set of events'
    });
```

##### <a id="can-i-increase-web-console-verbosity-for-debug-purposes">Can I increase web console verbosity for debug purposes?</a>

If you need more insightful overview of what client-side script is doing (including warnings if detected) enable logging:

```JavaScript
<script>
    ea.settings.debug = true; // output debug messages to the web console
                              // (should be disabled for release code not to introduce redundant overhead)
```

##### <a id="i-prefer-enum-values-to-be-rendered-as-numbers-is-it-allowed">I prefer enum values to be rendered as numbers, is it allowed?</a>

There is a possibility to setup of how enumeration values are internally processed: as integral numerics or string identifiers - see `enumsAsNumbers` settings flag in the script.

This setting should be consistent with the way of how input fields values are stored in HTML, e.g. `@Html.EditorFor(m => m.EnumValue)` renders to string identifier by default, in contrast to `@Html.EditorFor("EnumValue", (int)Model.EnumValue)` statement, where value is explicitly casted to `int`. The flag setup should reflect that behavior, to have the internal JS model context deserialized accordingly.

##### <a id="how-to-fetch-fields-value-or-its-display-name-in-error-message">How to fetch field's value or its display name in error message?</a>

* to get a value, wrap the field name in braces, e.g. `{field}`, or for nested fields - `{field.field}`,
* to get display name, given in `DisplayAttribute`, use additional `n` (or `N`) suffix, e.g. `{field:n}`.

Notice that `{{` is treated as the escaped bracket character.

##### <a id="is-there-any-event-raised-when-validation-is-done">Is there any event raised when validation is done?</a>

Each element validated by EA triggers one or more `eavalid` events. Attach to this event type in the following manner:

```JavaScript
<script>
    $('form').find('input, select, textarea').on('eavalid', function(e, type, valid, expr, cond, index) {
        console.log('event triggered by ' + e.currentTarget.name);
    });
```

The parameters are as follows:
* `type`  - type of the attribute for which validation was executed: `'requiredif'` or `'assertthat'`,
* `valid` - state of the validation: `true` (passed, i.e. field is either not required, required but value is provided, or assertion is satisfied) or `false` (failed, i.e. field is either required but value is not provided, or assertion is not satisfied),
* `expr`  - expression string which was evaluated,
* `cond`*  - expression evaluation result - optional parameter available when `ea.settings.optimize` flag is set to `false`, otherwise `undefined`,
* `index`* - validation execution index in a sequence of all attributes (of the same type) to be executed for a particular field: *requiredif* - 1st (index 0), *requiredifa* - 2nd (index 1) ... *requiredifz* - last (index n-1).

*where the last two parameters are available only for `'requiredif'` type of validation.

##### <a id="can-i-decorate-conditionally-required-fields-with-asterisks">Can I decorate conditionally required fields with asterisks?</a>

EA does not provide any built-in mechanisms to manipulate DOM. Nevertheless, the asterisk decoration (or any similar effect) can be relatively easily achieved with the support of a supplementary code you could write for yourself. It will be based on the [`eavalid`](#is-there-any-event-raised-when-validation-is-done) events handling. Such an event type is triggered by EA when a field validation is performed. Please take a look at the snippet below (or run [web sample](src/ExpressiveAnnotations.MvcWebSample) to see it working):

```JavaScript
<script>
    ea.settings.optimize = false; // if flag is on, requirement expression is not needlessly evaluated for non-empty fields
                                  // otherwise, it is evaluated and such an evaluation result is provided to the eavalid event
    
    $('form').find(selector).on('eavalid', function(e, type, valid, expr, cond, idx) { // verify asterisk visibility based on computed condition
        if (type === 'requiredif' && cond !== undefined) {
            if (idx === 0) { // if first of its kind...
                e.currentTarget.eacond = false; // ...reset asterisk visibility flag
            }
            e.currentTarget.eacond |= cond; // multiple requiredif attributes can be applied to a single field - remember if any condition is true

            if (e.currentTarget.eacond) {
                $(e.currentTarget).closest('li').find('.asterisk').show(); // show asterisk for required fields (no matter if valid or not)
            }
            else {
                $(e.currentTarget).closest('li').find('.asterisk').hide();
            }
        }
    });
    
    $('form').find(selector).each(function() { // apply asterisks for required or conditionally required fields
        if ($(this).attr('data-val-required') !== undefined // make sure implicit required attributes for value types are disabled i.e. DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false
            || $(this).attr('data-val-requiredif') !== undefined) {
            var div = $(this).parent('div');
            div.prepend('<span class="asterisk">*</span>');
        }
    });
    
    $('form').find(selector).each(function() { // run validation for every field to verify asterisks visibility
        if ($(this).attr('data-val-requiredif') !== undefined) {
            $(this).valid();
            $(this).removeClass('input-validation-error');
            $(this).parent().find('.field-validation-error').empty();
        }
    });
```

For the needs of this example, the code above makes assumptions:

* about the existence of `.asterisk` class in the CSS:

    ```CSS
    .asterisk {
        color: #ff0000;
        vertical-align: top;
        margin-left: -10px;
        float: left;
    }
    ```

* about specific HTML structure, where input fields are placed between the `<li></li>` tags:

    ```HTML
    <li>
        <label ...
        <input data-val=...
    </li>
    <li>
        <label ...
        <input data-val=...
    </li>
    ```

##### <a id="how-to-handle-validation-based-on-parent-model-field">How to handle validation based on parent model field?</a>

In this case you need to have a back reference to your container, i.e.

```C#
public class TParent
{
    public TParent()
    {
        Child.Parent = this;
    }

    public TChild Child { get; set; }
    public bool Flag { get; set; }
}

public class TChild
{     
    public TParent Parent { get; set; } // back-reference to parent

    [RequiredIf("Parent.Flag")]
    public string IsRequired { get; set; }
}
```

This is all you need for server-side. Unfortunately, out-of-the-box client-side support of back-references to the parent context is not implemented. EA at client-side is straightforward and designed to handle nested properties when explicitly provided to the view, without abilities to infer any relationships.

There still exists a way to handle it though. When using HTML helpers at the client-side, the more nested property, the more prefixed name. When reference to container is used in the expression, what client-side script actually does, it looks for such a property within the **current** context, i.e. within the fields of names annotated with the **current** prefix. EA internally has no idea that such a property refers to a field from a container (most likely already existing form field with different prefix).

In this exemplary case above, the `Flag` property used in the expression `Parent.Flag != null`, makes client-side EA to look for analogic input field within current scope, i.e. `Child.Parent.Flag`. The *"Child."* prefix corresponds with what HTML helper prepends to names of nested properties from the `Child` object.

The solution is to provide such a field, EA expects to find, i.e.

```C#
@Html.HiddenFor(m => m.Child.Parent.Flag) // hidden mock
```

Next, make the mocked field clone the behavior of the original (from parent) one:

```JavaScript
<script type="text/javascript">

    function Mimic(src, dest) {
        $(src).on(ea.settings.dependencyTriggers,
            function(e) {
                $(dest).val($(this).val());
                $(dest).trigger(e.type); // trigger the change for dependency validation
            });
    }

    $(document).ready(function() {
        Mimic('input[name="@Html.NameFor(m => m.Flag)"]',
              'input[name="@Html.NameFor(m => m.Child.Parent.Flag)"]');
    });

</script>
```

Please note, that this slightly hacky workaround is only needed for such a specific cases, where back-references are required to be handled by client-side EA logic.

##### <a id="requiredif-attribute-is-not-working-what-is-wrong">RequiredIf attribute is not working, what is wrong?</a>

Make sure `RequiredIf` is applied to a field which *accepts null values*.

In the other words, it is redundant to apply this attribute to a field of non-nullable [value type](https://msdn.microsoft.com/en-us/library/s1ax56ch.aspx), like e.g. `int`, which is a struct representing integral numeric type, `DateTime`, etc. Because the value of such a type is always non-null, requirement demand is constantly fulfilled. Instead, for value types use their nullable forms, e.g. `int?`, `DateTime?`, etc.

```C#
[RequiredIf("true")] // no effect...
public int Value { get; set; } // ...unless int? is used
```

```C#
[RequiredIf("true")] // no effect...
public DateTime Value { get; set; } // ...unless DateTime? is used
```

##### <a id="few-fields-seem-to-be-bypassed-during-validation-any-clues">Few fields seem to be bypassed during validation, any clues?</a>

Most likely these input fields are hidden, and `:hidden` fields validation is off by default.

Such a fields are ignored by default by jquery-validation plugin, and EA follows analogic rules. If this is true, try to enable validation for hidden fields (empty-out the ignore filter), i.e.:

```JavaScript
<script>
    $(function() { // DOM needs to be ready...
        var validator = $('form').validate(); // ...for the form to be found
        validator.settings.ignore = ''; // enable validation for ':hidden' fields
    });
```

##### <a id="client---side-validation-doesnt-work-how-to-troubleshoot-it">Client-side validation doesn't work, how to troubleshoot it?</a>

* Check whether setup is done correctly, as described in the [installation section](#what-about-the-support-of-aspnet-mvc-client-side-validation) of this document.
* Verify if the HTML code generated by the HTML helpers contains all the `data-val-*` attributes used by EA, with correct names (the same as used in related expressions).
* Check whether *jquery.validate.js* and *jquery.validate.unobtrusive.js* are not accidentaly loaded twice on the page.
* Verify if simple `Required` attribute works at client-side - if not, it most likely means your issue is not related to EA script.
* Set EA into [debug mode](#can-i-increase-web-console-verbosity-for-debug-purposes) and open the browser web console (F12). Look for suspicious EA activity (if any) or some other JavaScript exceptions which may abort further client-side execution, forcing immediate post-back to the server. The error, if any, should appear just before sending the request. Please note, if the console log is not preserved, whatever is recorded there will be cleared on page reload as soon as response arrives.
* Make sure the fields to be validated are not hidden, otherwise [enable validation of hidden fields](#few-fields-seem-to-be-bypassed-during-validation-any-clues).
* Check whether [attributes](#requiredif-vs-assertthat---where-is-the-difference) are used [properly](#requiredif-attribute-is-not-working-what-is-wrong).
* Finally, please take a look at other [FAQs](#frequently-asked-questions), or go [beyond that](#what-if-my-question-is-not-covered-by-faq-section) if solution is yet to be found.

##### <a id="is-there-a-possibility-to-perform-asynchronous-validation">Is there a possibility to perform asynchronous validation?</a> 

Currently not. Although there is an ongoing work on [async-work branch](https://github.com/jwaliszko/ExpressiveAnnotations/tree/async-work), created especially for asynchronous-related ideas. If you feel you'd like to contribute, either by providing better solution, review code or just test what is currently there, your help is always highly appreciated.

##### <a id="what-if-my-question-is-not-covered-by-faq-section">What if my question is not covered by FAQ section?</a>

If you're searching for an answer to some other problem, not covered by this document, try to browse through [already posted issues](../../issues?q=label%3Aquestion) labelled by *question* tag, or possibly have a look [at Stack Overflow](http://stackoverflow.com/search?tab=newest&q=expressiveannotations).

### <a id="installation">Installation instructions</a>

Simplest way is using the [NuGet](https://www.nuget.org) Package Manager Console:

* [complete package](https://www.nuget.org/packages/ExpressiveAnnotations) - both assemblies and the script included (allows [complete MVC validation](#what-about-the-support-of-aspnet-mvc-client-side-validation)):

    [![NuGet complete](https://img.shields.io/nuget/v/ExpressiveAnnotations.svg)](http://nuget.org/packages/ExpressiveAnnotations)

    ### `PM> Install-Package ExpressiveAnnotations`

* [minimal package](https://www.nuget.org/packages/ExpressiveAnnotations.dll) - core assembly only (MVC-related client-side coating components excluded):

    [![NuGet minimal](https://img.shields.io/nuget/v/ExpressiveAnnotations.dll.svg)](http://nuget.org/packages/ExpressiveAnnotations.dll)

    ### `PM> Install-Package ExpressiveAnnotations.dll`

### <a id="contributors">Contributors</a>

[GitHub Users](../../graphs/contributors)

Special thanks to Szymon Małczak

### <a id="license">License</a>

Copyright (c) 2014 Jarosław Waliszko

Licensed MIT: http://opensource.org/licenses/MIT
