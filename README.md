#ExpressiveAnnotations - annotation-based conditional validation

<sub>**Notice: This document describes latest implementation. For previous version (concept) &lt; 2.0 take a look at [EA1 branch](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/EA1).**</sub>

ExpressiveAnnotations is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given RequiredIf and AssertThat attributes allow to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more condensed, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###RequiredIf vs AssertThat attribute?

* RequiredIf - if value is not yet provided, check whether it is required (annotated field is required when given condition is satisfied)
* AssertThat - if value is already provided, check whether the condition is met (non-null annotated field is considered as valid when given condition is satisfied)

###What are brief examples of usage?

For sample usages go to demo projects: [**ASP.NET MVC web sample**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcWebSample) or [**WPF MVVM desktop sample**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvvmDesktopSample).

Let's walk through some snippets, starting from the one below:
```
[RequiredIf("GoAbroad == true")]
public string PassportNumber { get; set; }
```
Here we are saying that annotated field is required when condition given in the logical expression is fulfilled (passport number is required, if go abroad field has true boolean value). Simple enough, let's move to another variation:
```
[AssertThat("ReturnDate >= Today()")]
public DateTime? ReturnDate { get; set; }
```
This time another attribute is used. We are not validating field requirement as before. Field value is not required to be provided - it is allowed to be null now. Nevertheless, if some value is already given, it needs to be correct. In the other words, this attribute puts restriction on field, which needs to be satisfied for such field to be considered as valid (restriction verification is executed for non-null field). Here, the value of return date field needs to be greater than or equal to the date returned by `Today()` utility function (function which simply returns current day). Both types of attributes may be combined:
```
[RequiredIf("ContactDetails.Email != null")]
[AssertThat("AgreeToContact == true")]
public bool? AgreeToContact { get; set; }
```
This one means, that if email is provided (non-null), we are forced to authorize someone to contact us (boolean value indicating contact permission has to be true). What is more, we can see here that nested properties (enums and constants either) are supported by expressions parser. Finally take a quick look at following construction:
```
[RequiredIf("GoAbroad == true " +
			"&& (" +
					"(NextCountry != 'Other' && NextCountry == Country) " +
					"|| (Age > 24 && Age <= 55)" +
				")")]
public string ReasonForTravel { get; set; }
```
<sub>Notice: Expression is splitted into multiple lines because such a form makes it easier to understand.</sub>

This time, the presented expression is much more complex that its predecessors, but still can be intuitively understood when looking at it (reason for travel field has to be provided if you plan to go abroad, and want to visit the same foreign country twice or you are between 25 and 55).

###How to construct conditional validation attributes?
#####Signatures:
```
RequiredIfAttribute(string expression,
                    [bool AllowEmptyStrings] ...) - Validation attribute which indicates that 
					                                annotated field is required when computed 
											 		result of given logical expression is true.
AssertThatAttribute(string expression ...)        - Validation attribute, executed for non-null 
                                                    annotated field, which indicates that 
													assertion given in logical expression has 
													to be satisfied, for such field to be 
													considered as valid.

  expression        - The logical expression based on which requirement condition is computed.
  AllowEmptyStrings - Gets or sets a flag indicating whether the attribute should allow empty or
	                  whitespace strings.
```
#####Implementation:

Implementation core is based on top-down recursive descent logical expressions parser, with a single token of lookahead [(LL(1))](http://en.wikipedia.org/wiki/LL_parser), which runs on the following [EBNF-like](http://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form) grammar:
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
Terminals are expressed in quotes. Each nonterminal is defined by a rule in the grammar except for int, float, bool, string and func, which are assumed to be implicitly defined (func can be an enum value as well as constant, property or function name).

Preserving the syntax defined by the grammar above, logical expressions can be built using following components:

* binary operators: `||`, `&&`, `!`,
* relational operators: `==`, `!=`,`<`, `<=`, `>`, `>=`,
* arithmetic operators: `+`, `-`, `*`, `/`,	
* brackets: `(`, `)`,
* aplhanumeric characters and whitespaces with the support of `,`, `.`, `'` and `"`, used to synthesize suitable literals: 
  * null literal: `null`, 
  * integer number literals, e.g. `123`, 
  * real number literals, e.g. `1.5` or `-0.3e-2`,
  * boolean literals: `true` and `false`,
  * string literals, e.g. `'in single quotes'` or `\"in escaped double quotes\"`, 
  * func literals:
      * property names, e.g. `SomeProperty`,
	  * constants, e.g. `SomeType.CONST`,
      * enum values, e.g. `SomeEnumType.SomeValue`,
	  * function invocations, e.g. `SomeFunction(...)`.

Provided expression string is parsed and converted into [expression tree](http://msdn.microsoft.com/en-us/library/bb397951.aspx) structure. A delegate containing compiled version of the lambda expression described by produced expression tree is returned as a result of the parser job. Such delegate is then invoked for specified model object. As a result of expression evaluation, boolean flag is returned, indicating that expression is true or false. 

When working with ASP.NET MVC stack, client side mechanism is additionally available. Client receives unchanged expression string from server. Such an expression is then evaluated using build-in [eval](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/eval) within the context of model object. Such a model, analogously to the server side one, is basically deserialized DOM form (with some type-safety assurances and registered toolchain methods).

Attention needed when coping with null (discrepancies between C# and JS), e.g.

* `null + "text"` - in C# `"text"`, in JS `"nulltext"`,
* `2 * null`      - in C# `null`  , in JS `0`,
* `null > -1`     - in C# `false` , in JS `true`.

#####Built-in functions:

Toolchain functions available out of the box at server and client side: 

* `DateTime Now()`
    * Gets the current date and time, expressed as the local time.
* `DateTime Today()`
    * Gets the current date with the time component set to 00:00:00, expressed as the local time.
* `int Length(str)`
    * Gets the number of characters in the specified string (null-safe).
* `string Trim(string str)`
    * Removes all leading and trailing white-space characters from the specified string (null-safe).
* `string Concat(string strA, string strB)`
    * Concatenates two specified strings.
* `string Concat(string strA, string strB, strC)`
    * Concatenates three specified strings.
* `int CompareOrdinal(string strA, string strB)`
    * Compares strings using ordinal sort rules. An integer that indicates the lexical relationship 
      between the two comparands is returned: 
        * less than zero    - strA is less than strB,
        * zero              - strA and strB are equal,
        * greater than zero - strA is greater than strB.
* `int CompareOrdinalIgnoreCase(string strA, string strB)`
    * Compares strings using ordinal sort rules and ignoring the case of the strings being compared.
* `bool StartsWith(string str, string prefix)`
    * Determines whether the beginning of specified string matches a specified prefix.
* `bool StartsWithIgnoreCase(string str, string prefix)`
    * Determines whether the beginning of specified string matches a specified prefix, ignoring the case of the strings.
* `bool EndsWith(string str, string suffix)`
    * Determines whether the end of specified string matches a specified suffix.
* `bool EndsWithIgnoreCase(string str, string suffix)`
    * Determines whether the end of specified string matches a specified suffix, ignoring the case of the strings.
* `bool Contains(string str, string substr)`
    * Returns a value indicating whether a specified substring occurs within a specified string.
* `bool ContainsIgnoreCase(string str, string substr)`
    * Returns a value indicating whether a specified substring occurs within a specified string, ignoring the case of the strings.
* `bool IsNullOrWhiteSpace(string str)`
    * Indicates whether a specified string is null, empty, or consists only of white-space characters.
* `bool IsDigitChain(string str)`
    * Indicates whether a specified string represents a sequence of digits.
* `bool IsNumber(string str)`
    * Indicates whether a specified string represents integer or float number.
* `bool IsEmail(string str)`
    * Indicates whether a specified string represents valid e-mail address.
* `bool IsUrl(string str)`
    * Indicates whether a specified string represents valid url.
* `bool IsRegexMatch(string str, string regex)`
    * Indicates whether the regular expression finds a match in the input string.
* `Guid Guid(string str)`
    * Initializes a new instance of the Guid structure by using the value represented by the specified string.

#####What if there is no function I need?

Create it yourself. Any custom function defined within the model class scope at server side is automatically recognized and can be used inside expressions, e.g.,
```
class Model
{
    public bool IsBloodType(string group) { return Regex.IsMatch(group, "^(A|B|AB|0)[\+-]$"); }

	[AssertThat("IsBloodType(BloodType)")] // <- function known here
    public string BloodType { get; set; }
```
 If client side validation is needed as well, function of the same signature (name and the number of parameters) must be available there. JavaScript corresponding implementation should be registered by the following instruction:
```
<script>
    // expressive.annotations.validate.js already loaded
    ea.addMethod('IsBloodType', function(group) { return /^(A|B|AB|0)[\+-]$/.test(group); });
</script>
```
###What is the context behind this implementation? 

Declarative validation, when compared to imperative approach, seems to be more convenient in many cases. Clean, compact code - all validation logic can be defined within the model metadata.

###What is the difference between declarative and imperative programming?

With **declarative** programming, you write logic that expresses what you want, but not necessarily how to achieve it. You declare your desired results, but not the step-by-step.

In our example it is more about metadata, e.g.
```
[RequiredIf("GoAbroad == true && NextCountry != 'Other' && NextCountry == Country",
	ErrorMessage = "If you plan to go abroad, why do you want to visit the same country twice?")]
public string ReasonForTravel { get; set; }
```
With **imperative** programming, you define the control flow of the computation which needs to be done. You tell the compiler what you want, step by step.

If we choose this way instead of model fields decoration, it has negative impact on the complexity of the code. Logic responsible for validation is now implemented somewhere else in our application, e.g. inside controllers actions instead of model class itself:
```
    if (!model.GoAbroad)
    {
        return View("Success");
    }
    if (model.NextCountry == "Other")
    {
        return View("Success");
    }
    if (model.NextCountry != model.Country)
    {
        return View("Success");
    }
    ModelState.AddModelError("ReasonForTravel", "If you plan to go abroad, why do you 
                                                 want to visit the same country twice?");
    return View("Home", model);
}
```
###What about the support of ASP.NET MVC client side validation?

Client side validation is **fully supported**. Enable it for your web project within the next few steps:

1. Add [**ExpressiveAnnotations.dll**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations) and [**ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.dll**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider) reference libraries to your projest,
2. In `Global.asax` register required validators (`IClientValidatable` interface is not directly implemented by the attribute, to avoid coupling of `ExpressionAnnotations` assembly with `System.Web.Mvc` dependency):
 ```    
    protected void Application_Start()
    {
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof(AssertThatAttribute), typeof(AssertThatValidator));
```			
3. Include [**expressive.annotations.validate.js**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/blob/master/src/expressive.annotations.validate.js) scripts in your page (do not forget standard jQuery validation scripts):
 ```
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.validate.js"></script>
```
Alternatively, using the [NuGet](https://www.nuget.org/packages/ExpressiveAnnotations/) Package Manager Console:

###`PM> Install-Package ExpressiveAnnotations`
