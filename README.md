![logo](logo.png)

#<a id="expressiveannotations-annotation-based-conditional-validation">ExpressiveAnnotations<sup><sup><sup>[annotation-based conditional validation]</sup></sup></sup></a>

[![Build status](https://img.shields.io/appveyor/ci/JaroslawWaliszko/ExpressiveAnnotations.svg)](https://ci.appveyor.com/project/JaroslawWaliszko/ExpressiveAnnotations)
[![Release](https://img.shields.io/github/release/JaroslawWaliszko/ExpressiveAnnotations.svg)](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/releases/latest)
[![License](http://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

ExpressiveAnnotations is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given `RequiredIf` and `AssertThat` attributes allow to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more condensed, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###Table of contents
 - [What is the context behind this implementation?](#what-is-the-context-behind-this-implementation)
 - [`RequiredIf` vs. `AssertThat` - where is the difference?](#requiredif-vs-assertthat---where-is-the-difference)
 - [What are brief examples of usage?](#what-are-brief-examples-of-usage)
 - [Declarative vs. imperative programming - what is it about?](#declarative-vs-imperative-programming---what-is-it-about)
 - [How to construct conditional validation attributes?](#how-to-construct-conditional-validation-attributes)
   - [Signatures](#signatures)
   - [Implementation](#implementation)
   - [Traps](#traps)
   - [Built-in functions](#built-in-functions)
 - [What about the support of ASP.NET MVC client-side validation?](#what-about-the-support-of-aspnet-mvc-client-side-validation)
 - [Frequently asked questions](#frequently-asked-questions)
   - [Is it possible to compile all usages of annotations at once?](#is-it-possible-to-compile-all-usages-of-annotations-at-once) <sup>(re server-side)</sup>
   - [What if there is no built-in function I need?](#what-if-there-is-no-built-in-function-i-need) <sup>(re client and server-side)</sup>
   - [How to cope with values of custom types?](#how-to-cope-with-values-of-custom-types) <sup>(re client-side)</sup>
   - [How to cope with dates given in non-standard formats?](#how-to-cope-with-dates-given-in-non-standard-formats) <sup>(re client-side)</sup>
   - [What if `ea` variable is already used by another library?](#what-if-ea-variable-is-already-used-by-another-library) <sup>(re client-side)</sup>
   - [How to control frequency of dependent fields validation?](#how-to-control-frequency-of-dependent-fields-validation) <sup>(re client-side)</sup>
   - [How to boost web console verbosity for debug purposes?](#how-to-boost-web-console-verbosity-for-debug-purposes) <sup>(re client-side)</sup>
   - [Is there a possibility to perform asynchronous validation?](#is-there-a-possibility-to-perform-asynchronous-validation) <sup>(re client-side)</sup>
   - [What if my question is not covered by FAQ section?](#what-if-my-question-is-not-covered-by-faq-section)
 - [Installation](#installation)
 - [Contributors](#contributors)
 - [License](#license)

###<a id="what-is-the-context-behind-this-implementation">What is the context behind this implementation?</a>

There are number of cases where the concept of metadata is used for justified reasons. Attributes are one of the ways to associate complementary information with existing data. Such annotations may also define the correctness of data. Declarative validation when [compared](#declarative-vs-imperative-programming---what-is-it-about) to imperative approach seems to be more convenient in many cases. Clean, compact code - all validation logic defined within the model scope. Simple to write, obvious to read.

###<a id="requiredif-vs-assertthat---where-is-the-difference">`RequiredIf` vs. `AssertThat` - where is the difference?</a>

* `RequiredIf` - if value is not yet provided, check whether it is required (annotated field is required to be non-null, when given condition is satisfied),
* `AssertThat` - if value is already provided, check whether the condition is met (non-null annotated field is considered as valid, when given condition is satisfied).

###<a id="what-are-brief-examples-of-usage">What are brief examples of usage?</a>

If you'll be interested in comprehensive examples afterwards, take a look inside chosen demo project:

* [**ASP.NET MVC web sample**](src/ExpressiveAnnotations.MvcWebSample),
* [**WPF MVVM desktop sample**](src/ExpressiveAnnotations.MvvmDesktopSample).

For the time being, to keep your ear to the ground, let's walk through few exemplary code snippets:
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

Finally, take a brief look at following construction:
```C#
[RequiredIf(@"GoAbroad == true
              && (
                     (NextCountry != 'Other' && NextCountry == Country)
                     || (Age > 24 && Age <= 55)
                 )")]
public string ReasonForTravel { get; set; }
```

Restriction above is slightly more complex than its predecessors, but still can be quickly understood (reason for travel has to be provided if you plan to go abroad and, either want to visit the same definite country twice, or are between 25 and 55).

###<a id="declarative-vs-imperative-programming---what-is-it-about">Declarative vs. imperative programming - what is it about?</a>

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

###<a id="how-to-construct-conditional-validation-attributes">How to construct conditional validation attributes?</a>

#####<a id="signatures">Signatures</a>

```
RequiredIfAttribute(
    string expression,
    [bool AllowEmptyStrings], 
	[int Priority]           ...) - Validation attribute which indicates that annotated 
                                    field is required when computed result of given logical 
                                    expression is true.
AssertThatAttribute(
    string expression,
	[int Priority]           ...) - Validation attribute, executed for non-null annotated 
                                    field, which indicates that assertion given in logical 
                                    expression has to be satisfied, for such field to be 
                                    considered as valid.

expression        - The logical expression based on which specified condition is computed.
AllowEmptyStrings - Gets or sets a flag indicating whether the attribute should allow empty 
                    or whitespace strings. False by default.
Priority          - Gets or sets the hint, available for any concerned external components, 
                    indicating the order in which this attribute should be executed among 
                    others of its kind, i.e. ExpressiveAttribute. Value is optional and not
					set by default, which means that execution order is undefined.
```

Full API documentation *(probably not useful at all, since the note above covers almost exhaustively what is actually needed to work with EA)* generated with [Sandcastle](https://sandcastle.codeplex.com/) (with the support of [SHFB](http://shfb.codeplex.com/)), can be downloaded in the form of compiled HTML help file from [here](doc/api/api.chm?raw=true) (only C# API, no JavaScript there).

#####<a id="implementation">Implementation</a>

Implementation core is based on top-down recursive descent [logical expressions parser](src/ExpressiveAnnotations/Analysis/Parser.cs), with a single token of lookahead ([LL(1)](http://en.wikipedia.org/wiki/LL_parser)), which runs on the following [EBNF-like](http://en.wikipedia.org/wiki/Extended_Backus–Naur_Form) grammar:
```
expression => or-exp
or-exp     => and-exp [ "||" or-exp ]
and-exp    => rel-exp [ "&&" and-exp ]
rel-exp    => not-exp [ rel-op not-exp ]
not-exp    => add-exp | "!" not-exp
add-exp    => mul-exp add-exp'
add-exp'   => "+" add-exp | "-" add-exp
mul-exp    => val mul-exp'
mul-exp'   => "*" mul-exp | "/" mul-exp
rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="
val        => "null" | int | float | bool | string | func | "(" or-exp ")"
```
Terminals are expressed in quotes. Each nonterminal is defined by a rule in the grammar except for *int*, *float*, *bool*, *string* and *func*, which are assumed to be implicitly defined (*func* can be an enum value as well as constant, property or function name).

Logical expressions should be built according to the syntax defined by grammar, with the usage of following components:

* binary operators: `||`, `&&`, `!`,
* relational operators: `==`, `!=`,`<`, `<=`, `>`, `>=`,
* arithmetic operators: `+`, `-`, `*`, `/`,	
* curly brackets: `(`, `)`,
* square brackets: `[`, `]`,
* alphanumeric characters with the support of `,`, `.`, `_`, `'` and whitespaces, used to synthesize suitable literals:
  * null literal: `null`, 
  * integer number literals, e.g. `123`, 
  * real number literals, e.g. `1.5` or `-0.3e-2`,
  * boolean literals: `true` and `false`,
  * string literals: `'in single quotes'` (internal quote escape sequence is `\'`, character representing new line no matter the platform is `\n`),
  * func literals:
      * property names, e.g. `SomeProperty`,
	  * constants, e.g. `SomeType.CONST`,
      * enum values, e.g. `SomeEnumType.SomeValue`,
	  * arrays indexing, e.g. `SomeArray[0]`,
	  * function invocations, e.g. `SomeFunction(...)`.

Specified expression string is parsed and converted into [expression tree](http://msdn.microsoft.com/en-us/library/bb397951.aspx) structure. A delegate containing compiled version of the lambda expression described by produced expression tree is returned as a result of the parser job. Such delegate is then invoked for specified model object. As a result of expression evaluation, boolean flag is returned, indicating that expression is true or false.

For the sake of performance optimization, expressions provided to attributes are compiled only once. Such compiled lambdas are then cached inside attributes instances and invoked for any subsequent validation requests without recompilation.

When working with ASP.NET MVC stack, unobtrusive client-side validation mechanism is [additionally available](#what-about-the-support-of-aspnet-mvc-client-side-validation). Client receives unchanged expression string from server. Such an expression is then evaluated using JavaScript [`eval()`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/eval) method within the context of reflected model object. Such a model, analogously to the server-side one, is basically deserialized DOM form (with some type-safety assurances and registered toolchain methods).

#####<a id="traps">Traps</a>

Attention needed when dealing with `null` - there are discrepancies between C# and JavaScript, e.g.

* `null + "text"` - in C# `"text"`, in JS `"nulltext"`,
* `2 * null`      - in C# `null`  , in JS `0`,
* `null > -1`     - in C# `false` , in JS `true`,
* and more...

#####<a id="built-in-functions">Built-in functions</a>

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
* `bool IsUrl(string str)`
    * Indicates whether a specified string represents valid url (null-safe).
* `bool IsRegexMatch(string str, string regex)`
    * Indicates whether the regular expression finds a match in the input string (null-safe).
* `Guid Guid(string str)`
    * Initializes a new instance of the Guid structure by using the value represented by a specified string.

###<a id="what-about-the-support-of-aspnet-mvc-client-side-validation">What about the support of ASP.NET MVC client-side validation?</a>

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
3. Include [**expressive.annotations.validate.js**](src/expressive.annotations.validate.js) script in your page (it should be included in bundle below jQuery validation files):

    ```JavaScript
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.validate.js"></script>
    ```

For supplementary reading visit the [installation section](#installation).

###<a id="frequently-asked-questions">Frequently asked questions</a>

#####<a id="is-it-possible-to-compile-all-usages-of-annotations-at-once">Is it possible to compile all usages of annotations at once?</a>

Yes, a list of types with annotations can be collected and compiled collectively. It can be useful, e.g. during unit tesing phase, when without the necessity of your main application startup, all the compile-time errors (syntax errors, typechecking errors) done to your expressions can be discovered. The following extension is helpful:

```
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

```
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
```
compiled.ForEach(x => x.Compile(typeof (SomeModel));
```
do nothing (due to optimization purposes), unless invoked with enabled recompilation switch:
```
compiled.ForEach(x => x.Compile(typeof (SomeModel), force: true); 
```
Finally, this reveals compile-time errors only, you can still can get runtime errors though, e.g.:

```
var parser = new Parser();
parser.AddFunction<object, bool>("CastToBool", obj => (bool) obj);

parser.Parse<object>("CastToBool(null)"); // compilation succeeds
parser.Parse<object>("CastToBool(null)").Invoke(null); // invocation fails (type casting err)
```

#####<a id="what-if-there-is-no-built-in-function-i-need">What if there is no built-in function I need?</a>

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

#####<a id="how-to-cope-with-values-of-custom-types">How to cope with values of custom types?</a>

If you need to handle value string extracted from DOM field in any non built-in way, you can redefine given type-detection logic. The default mechanism recognizes and handles automatically types identified as: `timespan`, `datetime`, `numeric`, `string`, `bool` and `guid`. If non of them is matched for a particular field, JSON deserialization is invoked. You can provide your own deserializers though. The process is as follows:

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
        ea.addValueParser('customparser', function(value) {
		    return ... // handle exctracted field value string on your own
        });
    ```

#####<a id="how-to-cope-with-dates-given-in-non-standard-formats">How to cope with dates given in non-standard formats?</a>

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

#####<a id="what-if-ea-variable-is-already-used-by-another-library">What if `ea` variable is already used by another library?</a>

Use `noConflict()` method. In case of naming collision return control of the `ea` variable back to its origins. Old references of `ea` are saved during ExpressiveAnnotations initialization - `noConflict()` simply restores them:
```JavaScript
<script src="another.js"></script>
<script src="expressive.annotations.validate.js"></script>
<script>
    var expann = ea.noConflict();
    expann.addMethod... // do something with ExpressiveAnnotations
    ea... // do something with original ea variable
```

#####<a id="how-to-control-frequency-of-dependent-fields-validation">How to control frequency of dependent fields validation?</a>

When a field value is modified, validation results for some other fields, directly dependent on currenty modified one, may be affected. To control the frequency of when dependent fields validation is triggered, change default `ea.settings.dependencyTriggers` settings. It is a string containing one or more DOM field event types (such as *change*, *keyup* or custom event names), associated with currently modified field, for which fields directly dependent on are validated.

Default value is *'change keyup'* (for more information check `eventType` parameter of jQuery [`bind()`](http://api.jquery.com/bind) method). If you want to turn this feature off entirely, set it to *undefined* (validation will be fired on form submit attempt only).
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

#####<a id="how-to-boost-web-console-verbosity-for-debug-purposes">How to boost web console verbosity for debug purposes?</a>

If you need more insightful overview of what client-side script is doing (including warnings if detected) enable logging:
```JavaScript
<script>
    ea.settings.debug = true; // output debug messages to the web console 
							  // (should be disabled for release code)
```

#####<a id="#is-there-a-possibility-to-perform-asynchronous-validation">Is there a possibility to perform asynchronous validation?</a>

Currently not. Although there is an ongoing work on [async-work branch](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/async-work), created especially for asynchronous-related ideas. If you feel you'd like to contribute, either by providing better solution, review code or just test what is currently there, your help is always highly appreciated.

#####<a id="what-if-my-question-is-not-covered-by-faq-section">What if my question is not covered by FAQ section?</a>

If you're searching for an answer to some other problem, not covered by this document, try to browse through [already posted issues](../../issues?q=label%3Aquestion) labelled by *question* tag, or possibly have a look [at Stack Overflow](http://stackoverflow.com/search?q=expressiveannotations).

###<a id="installation">Installation</a>

Simplest way is using the [NuGet](https://www.nuget.org) Package Manager Console:

* [complete package](https://www.nuget.org/packages/ExpressiveAnnotations) - both assemblies and the script included (allows [complete MVC validation](#what-about-the-support-of-aspnet-mvc-client-side-validation)):

    [![NuGet complete](https://img.shields.io/nuget/dt/ExpressiveAnnotations.svg)](http://nuget.org/packages/ExpressiveAnnotations)

    ###`PM> Install-Package ExpressiveAnnotations`

* [minimal package](https://www.nuget.org/packages/ExpressiveAnnotations.dll) - core assembly only (MVC-related client-side coating components excluded):

    [![NuGet minimal](https://img.shields.io/nuget/dt/ExpressiveAnnotations.dll.svg)](http://nuget.org/packages/ExpressiveAnnotations.dll)

    ###`PM> Install-Package ExpressiveAnnotations.dll`

###<a id="contributors">Contributors</a>

[GitHub Users](../../graphs/contributors)

Special thanks to Szymon Malczak

###<a id="license">License</a>

Copyright (c) 2014 Jaroslaw Waliszko

Licensed MIT: http://opensource.org/licenses/MIT
