#<a id="expressiveannotations-annotation-based-conditional-validation">ExpressiveAnnotations<sup><sup><sup>[annotation-based conditional validation]</sup></sup></sup></a>

<sub>**Notice: This document describes latest implementation. For previous concept (version &lt; 2.0) take a look at [EA1 branch](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/EA1).**</sub>

ExpressiveAnnotations is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given `RequiredIf` and `AssertThat` attributes allow to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more condensed, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###<a id="what-is-the-context-behind-this-implementation">What is the context behind this implementation?</a>

There are number of cases where the concept of metadata is used for justified reasons. Attributes are one of the ways to associate complementary information with existing data. Such annotations may also define the correctness of data. Declarative validation when [compared](#declarative-vs-imperative-programming---what-is-it-about) to imperative approach seems to be more convenient in many cases. Clean, compact code - all validation logic defined within the model scope. Simple to write, obvious to read.

###<a id="requiredif-vs-assertthat---where-is-the-difference">RequiredIf vs. AssertThat - where is the difference?</a>

* `RequiredIf` - if value is not yet provided, check whether it is required (annotated field is required to be non-null, when given condition is satisfied),
* `AssertThat` - if value is already provided, check whether the condition is met (non-null annotated field is considered as valid, when given condition is satisfied).

###<a id="what-are-brief-examples-of-usage">What are brief examples of usage?</a>

If you'll be interested in comprehensive examples afterwards, take a look inside chosen demo project:

* [**ASP.NET MVC web sample**](src/ExpressiveAnnotations.MvcWebSample),
* [**WPF MVVM desktop sample**](src/ExpressiveAnnotations.MvvmDesktopSample).

For the time being, to keep your ear to the ground, let's walk through few exemplary code snippets:
```C#
[RequiredIf("GoAbroad == true")]
public string PassportNumber { get; set; }
```
Above we are saying, that annotated field is required when condition given in the logical expression is satisfied (passport number is required, if go abroad field has true boolean value).

Simple enough, let's move to another variation:
```C#
[AssertThat("ReturnDate >= Today()")]
public DateTime? ReturnDate { get; set; }
```
By the usage of this attribute type, we are not validating field requirement as before - its value is allowed to be null this time. Nevertheless, if some value is already given, provided restriction needs to be satisfied (return date needs to be greater than or equal to the date returned by [`Today()` built-in function](#built-in-functions)). 

As shown below, both types of attributes may be combined (moreover, the same type can be applied multiple times for a single field):
```C#
[RequiredIf("Details.Email != null")]
[RequiredIf("Details.Phone != null")]
[AssertThat("AgreeToContact == true")]
public bool? AgreeToContact { get; set; }
```
Literal translation means, that if either email or phone is provided, you are forced to authorize someone to contact with you (boolean value indicating contact permission has to be true). What is more, we can see that nested properties are supported by [the expressions parser](#implementation). 

Finally, take a brief look at following construction:
```C#
[RequiredIf("GoAbroad == true " +
            "&& (" +
                    "(NextCountry != 'Other' && NextCountry == Country) " +
                    "|| (Age > 24 && Age <= 55)" +
                ")")]
public string ReasonForTravel { get; set; }
```
<sub>Notice: Expression is splitted into multiple lines because such a form is easier to comprehend.</sub>

Restriction above is slightly more complex than its predecessors, but still can be quickly understood (reason for travel has to be provided if you plan to go abroad and: want to visit the same definite country twice, or have between 25 and 55 years).

###<a id="how-to-construct-conditional-validation-attributes">How to construct conditional validation attributes?</a>

#####<a id="signatures">Signatures:</a>

```
RequiredIfAttribute(
    string expression,
    [bool AllowEmptyStrings] ...) - Validation attribute which indicates that annotated 
                                    field is required when computed result of given logical 
                                    expression is true.
AssertThatAttribute(
    string expression ...)        - Validation attribute, executed for non-null annotated 
                                    field, which indicates that assertion given in logical 
                                    expression has to be satisfied, for such field to be 
                                    considered as valid.

expression        - The logical expression based on which requirement condition is computed.
AllowEmptyStrings - Gets or sets a flag indicating whether the attribute should allow empty 
                    or whitespace strings.
```

#####<a id="implementation">Implementation:</a>

Implementation core is based on top-down recursive descent [logical expressions parser](src/ExpressiveAnnotations/Analysis/Parser.cs), with a single token of lookahead ([LL(1)](http://en.wikipedia.org/wiki/LL_parser)), which runs on the following [EBNF-like](http://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form) grammar:
```
expression => or-exp
or-exp     => and-exp [ "||" or-exp ]
and-exp    => not-exp [ "&&" and-exp ]
not-exp    => rel-exp | "!" not-exp
rel-exp    => add-exp [ rel-op add-exp ]
add-exp    => mul-exp add-exp'
add-exp'   => "+" add-exp | "-" add-exp
mul-exp    => val mul-exp'
mul-exp'   => "*" mul-exp | "/" mul-exp 
rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="
val        => "null" | int | float | bool | string | func | "(" or-exp ")"
```
Terminals are expressed in quotes. Each nonterminal is defined by a rule in the grammar except for *int*, *float*, *bool*, *string* and *func*, which are assumed to be implicitly defined (*func* can be an enum value as well as constant, property or function name).

Preserving the syntax defined by the grammar above, logical expressions can be built using following components:

* binary operators: `||`, `&&`, `!`,
* relational operators: `==`, `!=`,`<`, `<=`, `>`, `>=`,
* arithmetic operators: `+`, `-`, `*`, `/`,	
* brackets: `(`, `)`,
* alphanumeric characters and whitespaces with the support of `,`, `.`, `_`, `'` and `"`, used to synthesize suitable literals: 
  * null literal: `null`, 
  * integer number literals, e.g. `123`, 
  * real number literals, e.g. `1.5` or `-0.3e-2`,
  * boolean literals: `true` and `false`,
  * string literals: `'in single quotes'` (internal quote escape sequence: `\'`, `\n` character represents new line no matter the platform)
  * func literals:
      * property names, e.g. `SomeProperty`,
	  * constants, e.g. `SomeType.CONST`,
      * enum values, e.g. `SomeEnumType.SomeValue`,
	  * function invocations, e.g. `SomeFunction(...)`.

Provided expression string is parsed and converted into [expression tree](http://msdn.microsoft.com/en-us/library/bb397951.aspx) structure. A delegate containing compiled version of the lambda expression described by produced expression tree is returned as a result of the parser job. Such delegate is then invoked for specified model object. As a result of expression evaluation, boolean flag is returned, indicating that expression is true or false. 

When working with ASP.NET MVC stack, unobtrusive client-side validation mechanism is [additionally available](#what-about-the-support-of-aspnet-mvc-client-side-validation). Client receives unchanged expression string from server. Such an expression is then evaluated using JavaScript [`eval()` method](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/eval) within the context of reflected model object. Such a model, analogously to the server-side one, is basically deserialized DOM form (with some type-safety assurances and registered toolchain methods).

#####<a id="traps">Traps:</a>

Attention needed when coping with `null` (discrepancies between C# and JavaScript), e.g.

* `null + "text"` - in C# `"text"`, in JS `"nulltext"`,
* `2 * null`      - in C# `null`  , in JS `0`,
* `null > -1`     - in C# `false` , in JS `true`,
* and more...

#####<a id="built-in-functions">Built-in functions:</a>

Toolchain functions available out of the box at server- and client-side: 

* `DateTime Now()`
    * Gets the current date and time, expressed as the local time.
* `DateTime Today()`
    * Gets the current date with the time component set to 00:00:00, expressed as the local time.
* `int Length(str)`
    * Gets the number of characters in the specified string (null-safe).
* `string Trim(string str)`
    * Removes all leading and trailing white-space characters from the specified string (null-safe).
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
* `bool IsUrl(string str)`
    * Indicates whether a specified string represents valid url (null-safe).
* `bool IsRegexMatch(string str, string regex)`
    * Indicates whether the regular expression finds a match in the input string (null-safe).
* `Guid Guid(string str)`
    * Initializes a new instance of the Guid structure by using the value represented by the specified string.

###<a id="frequently-asked-questions">Frequently asked questions:</a>

#####<a id="what-if-there-is-no-function-i-need">What if there is no function I need?</a>

Create it yourself. Any custom function defined within the model class scope at server-side is automatically recognized and can be used inside expressions, e.g.
```C#
class Model
{
    public bool IsBloodType(string group) 
    { 
        return Regex.IsMatch(group, "^(A|B|AB|0)[\+-]$");
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

#####<a id="how-to-cope-with-dates-given-in-non-standard-formats">How to cope with dates given in non-standard formats?</a>

When values of DOM elements are extracted, they are converted to appropriate types. For fields containing date strings, JavaScript `Date.parse()` method is used by default. As noted in [MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse), the input parameter is:

>A string representing an RFC 2822 or ISO 8601 date (other formats may be used, but results may be unexpected)

When some non-standard format needs to be handled, simply override the default behavior and provide your own implementation. E.g. when dealing with UK format dd/mm/yyyy:
```JavaScript
<script>
    ea.settings.parseDate = function(str) {
        if (!/^\d{2}\/\d{2}\/\d{4}$/.test(str)) // in case str format is not dd/mm/yyyy...
            return Date.parse(str); // ...default date parser is used

        var arr = str.split('/');
        var date = new Date(arr[2], arr[1] - 1, arr[0]);
        return date.getTime(); // return milliseconds since January 1, 1970, 00:00:00 UTC
    }
```

#####<a id="what-if-ea-variable-is-already-used-by-another-library">What if ea variable is already used by another library?</a>

Use `noConflict()` method. In case of naming collision return control of the `ea` variable back to its origins. Old references of `ea` are saved during ExpressiveAnnotations initialization - `noConflict()` simply restores them:
```JavaScript
<script src="another.js"></script>
<script src="expressive.annotations.validate.js"></script>
<script>
    var expann = ea.noConflict();
    expann.addMethod... // do something with ExpressiveAnnotations
    ea... // do something with original ea variable
```

###<a id="declarative-vs-imperative-programming---what-is-it-about">Declarative vs. imperative programming - what is it about?</a>

With **declarative** programming you write logic that expresses *what* you want, but not necessarily *how* to achieve it. You declare your desired results, but not the necessarily step-by-step.

In our case, this concept is materialized by attributes, e.g.
```C#
[RequiredIf("GoAbroad == true && NextCountry != 'Other' && NextCountry == Country",
    ErrorMessage = "If you plan to travel abroad, why visit the same country twice?")]
public string ReasonForTravel { get; set; }
```
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

###<a id="what-about-the-support-of-aspnet-mvc-client-side-validation">What about the support of ASP.NET MVC client-side validation?</a>

Client-side validation is fully supported. Enable it for your web project within the next few steps:

1. Reference both assemblies to your project: core [**ExpressiveAnnotations.dll**](src/ExpressiveAnnotations) and subsidiary [**ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.dll**](src/ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider),
2. In Global.asax register required validators (`IClientValidatable` interface is not directly implemented by the attribute, to avoid coupling of ExpressionAnnotations assembly with System.Web.Mvc dependency):

    ```C#
    protected void Application_Start()
    {
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (AssertThatAttribute), typeof (AssertThatValidator));
    ```
3. Include [**expressive.annotations.validate.js**](src/expressive.annotations.validate.js) scripts in your page (do not forget standard jQuery validation scripts):

    ```JavaScript
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.validate.js"></script>
    ```

Alternatively, using the [NuGet](https://www.nuget.org/packages/ExpressiveAnnotations/) Package Manager Console:

###`PM> Install-Package ExpressiveAnnotations`
