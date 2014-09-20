#ExpressiveAnnotations<sup><sup><sup>[annotation-based conditional validation]</sup></sup></sup>

<sub>**Notice: This document describes initial concept (version &lt; 2.0). For latest implementation take a look at [master branch](https://github.com/JaroslawWaliszko/ExpressiveAnnotations).**</sub>

ExpressiveAnnotations is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given `RequiredIf`, `RequiredIfExpression`, `AssertThat` and `AssertThatExpression` attributes allow to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more condensed, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###What is the context behind this implementation?

There are number of cases where the concept of metadata is used for justified reasons. Attributes are one of the ways to associate complementary information with existing data. Such annotations may also define the correctness of data. Declarative validation when [compared](#declarative-vs-imperative-programming---what-is-it-about) to imperative approach seems to be more convenient in many cases. Clean, compact code - all validation logic defined within the model scope. Simple to write, obvious to read.

###RequiredIf vs. AssertThat - where is the difference?

* `RequiredIf/Expression` - if value is not yet provided, check whether it is required (annotated field is required to be non-null, when given condition is satisfied),
* `AssertThat/Expression` - if value is already provided, check whether the condition is met (non-null annotated field is considered as valid, when given condition is satisfied).

###What are brief examples of usage?

If you'll be interested in comprehensive examples afterwards, take a look inside [**demo project**](src/ExpressiveAnnotations.MvcWebSample).

* Simplest (without logical expressions):
 
    ```C#
    [RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
    public string PassportNumber { get; set; }
    ```

    Here we are saying that annotated field is required when dependent field has appropriate value (passport number is required, if go abroad option is selected). Simple enough, let's move to another variation:

    ```C#
    [RequiredIf(
        DependentProperty = "ContactDetails.Email",
        TargetValue = "*",
        ErrorMessage = "You have to authorize us to make contact.")]
    public bool AgreeToContact { get; set; }
    ```

    This one means, that if email is non-empty, boolean value indicating contact permission has to be true. What is more, we can see here that nested properties are supported by the mechanism. The last thing shown is wildcard character `*` used as target value. It is special character which stands for any non-empty value (other than null, empty or whitespace strings).

    ```C#
    [AssertThat(
        DependentProperty = "ReturnDate",
        RelationalOperator = ">=",
        TargetValue = "[Today]")]
    public DateTime? ReturnDate { get; set; }
    ```

    Here return date needs to be greater than or equal to the date given in target value. This time we are not validating field requirement as before. Now attribute puts restriction on field, which needs to be satisfied for such field to be considered as valid (restriction verification is executed for non-null field). 
 
    The second thing shown here is the possibility of dynamic extraction of target values from backing fields, by providing their names inside square brackets `[]` (here today field value is extracted). Therefore not only hardcoded values are accepted as target values (nevertheless date can be hardcoded as RFC 2822 or ISO 8601 formatted string). 
 
    Comparing to the previous examples, this one introduces in addition one more new concept, namely the usage of explicitly provided relational operators. Such operators indicate relationships between dependent fields values and corresponding target values. If relational operators are not explicitly provided, relationships are by default defined by equality operator `==` (just like in the preceding examples).

* More complex (using logical expressions):
 
    ```C#
    [RequiredIfExpression(
        Expression = "{0} && !{1} && {2}",
        DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
        TargetValues = new object[] {true,       "Other",       "[Country]"},
        ErrorMessage = "If you plan to travel abroad, why visit the same country twice?")]
    public string ReasonForTravel { get; set; }
    ```

    Here we are saying that annotated field is required when computed result of given logical expression is true. How such an expression should be understood?

    ```C#
    GoAbroad == true && !(NextCountry == "Other") && NextCountry == value_from_country
    ```

    Finally, if we are slightly familiar with this syntax, let's move to even more enriched use case:
 
    ```C#
    [RequiredIfExpression(
        Expression = "{0} && ( (!{1} && {2}) || ({3} && {4}) )",
        DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry", "Age", "Age"},
        RelationalOperators = new[] {"==",       "==",          "==",          ">",   "<="},
        TargetValues = new object[] {true,       "Other",       "[Country]",   24,    55},
        ErrorMessage = "If you plan to go abroad and you are between 25 and 55 or plan to 
                        visit the same foreign country twice, write down your reasons.")]
    public string ReasonForTravel { get; set; }
    ```

    So, how such an expression should be understood this time?

    ```C#
    GoAbroad == true 
    && ( 
         (NextCountry != "Other" && NextCountry == value_from_country) 
         || Age ∈ (24, 55> 
    )
    ``` 

###How to construct conditional validation attributes?
#####Signatures:

```
RequiredIfAttribute(
    [string DependentProperty],
    [object TargetValue],
    [string RelationalOperator],
    [bool SensitiveComparisons],
    [bool AllowEmptyOrFalse] ...)    - Validation attribute which indicates that annotated
                                       field is required when dependent field has appropriate
                                       value.
AssertThatAttribute(
    [string DependentProperty],
    [object TargetValue],
    [string RelationalOperator],
    [bool SensitiveComparisons] ...) - Validation attribute, executed for non-null annotated
                                       field, which indicates that given assertion has to be
                                       satisfied, for such field to be considered as valid.

DependentProperty    - Gets or sets the name of dependent field, from which runtime value is
                       extracted.
TargetValue          - Gets or sets the expected value for dependent field (wildcard
                       character * stands for any non-empty value). Instead of hardcoding
                       there is also possibility of value runtime extraction from backing
                       field, by providing its name [inside square brackets].
RelationalOperator   - Gets or sets the relational operator indicating relation between
                       dependent field and target value. Available operators: ==, !=, >, >=,
                       <, <=. If this property is not provided, equality operator == is used
                       by default.
SensitiveComparisons - Gets or sets whether the string comparisons are case sensitive or not.
AllowEmptyOrFalse    - Gets or sets a flag indicating whether the attribute should
                       allow empty or whitespace strings or false boolean values (null never
                       allowed).
```
```
RequiredIfExpressionAttribute(
    [string Expression],
    [string[] DependentProperties],
    [object[] TargetValues],
    [string[] RelationalOperators],
    [bool SensitiveComparisons],
    [bool AllowEmptyOrFalse] ...)    - Validation attribute which indicates that annotated
                                       field is required when computed result of given
                                       logical expression is true.
AssertThatExpressionAttribute(
    [string Expression],
    [string[] DependentProperties],
    [object[] TargetValues],
    [string[] RelationalOperators],
    [bool SensitiveComparisons] ...) - Validation attribute, executed for non-null annotated
                                       field, which indicates that assertion given in logical
                                       expression has to be satisfied, for such field to be
                                       considered as valid.

Expression           - Gets or sets the logical expression based on which requirement
                       condition is computed. Available expression tokens: &&, ||, !, {, },
                       numbers and whitespaces.
DependentProperties  - Gets or sets the names of dependent fields from which runtime values
                       are extracted.
TargetValues         - Gets or sets the expected values for corresponding dependent fields
                       (wildcard character * stands for any non-empty value). There is also
                       possibility of values runtime extraction from backing fields, by
                       providing their names [inside square brackets].
RelationalOperators  - Gets or sets the relational operators indicating relations between
                       dependent fields and corresponding target values. Available operators:
                       ==, !=, >, >=, <, <=. If this property is not provided, equality
                       operator == is used by default.
SensitiveComparisons - Gets or sets whether the string comparisons are case sensitive or not.
AllowEmptyOrFalse    - Gets or sets a flag indicating whether the attribute should allow
                       empty or whitespace strings or false boolean values (null never
                       allowed).
```

#####Theoretical background:
Logical expression is an expression in which relationship between operands is specified by logical operators AND `&&` and OR `||`. The logical operator NOT `!` is used to negate logical variables or constants. It is the type of operator (AND, OR) that characterizes the expression as logical, not the type of operand. Basic logical expression consists of three parts: two operands and one operator. Operands on the other hand can be logical variables or other expressions, such as relational expressions. Relational expressions are characterized by relational operators EQ `==`, NE `!=`, GT `>`, GE `>=`, LT `<`, LE `<=`. 

#####Logical expressions schematic interpretation:

 ```
       == (by default), !=, >, >=, <, <=         binary logical operators
                   .---------.                .----.
(!)(DependProps[0] RelOpers[0] TargetVals[0]) ||, && (!)(DependProps[1] RelOpers[1] TargetVals[1]) …
 | '----------------------------------------'         | '----------------------------------------' ^
 |     {0} - 0th operand (relational expr)            |     {1} - 1st operand (relational expr)    |
 |                                                    |     {n} - nth operand ---------------------'
 '----------------------------------------------------'> unary logical operators (optional)
```

<sub>Notice: Schematic view uses abbreviated names.</sub>

#####Evaluation steps for sample `{0} && !{1}` logical expression:

1. Operands `{n}` are expanded into basic relational expressions:

    ```DependentProperties[n] RelationalOperators[n] TargetValues[n]```

    <sub>Notice: Arrays indexes of dependent fields and its corresponding target values given inside curly brackets `{}`.</sub>
2. Based on the assumption from previous step, our expression is interpreted as:

    ```(DependentProperties[0] == TargetValues[0]) && !(DependentProperties[1] == TargetValues[1])```

    <sub>Notice: Interpretation based on assumption that relational operators are not provided, so equality operator `==` is taken by default when computing operands.</sub>
3. Values are extracted from arrays and computed (compared for equality in this case). Next, computation results (boolean flags) are injected into corresponding brackets, let's say:

    ```true && !false```
4. Such preprocessed expression is then converted from infix notation syntax into postfix one using [shunting-yard algorithm](http://en.wikipedia.org/wiki/Shunting-yard_algorithm):

    ```true false ! &&```
5. Reverse Polish Notation (RPN) expression is finally evaluated using [postfix algorithm](http://en.wikipedia.org/wiki/Reverse_Polish_notation) to give validation result: 

    ```true false ! &&      =>      true true &&      =>      true```

    Here the computed result is true, which means that condition is fulfilled, so: 
    * when `RequiredIfExpression` used - error message is risen if annotated field value is not provided (i.e. is null, empty or whitespace string or has false boolean meaning),
    * when `AssertThatExpression` used - no error (successful validation).

###<a id="declarative-vs-imperative-programming---what-is-it-about">Declarative vs. imperative programming - what is it about?</a>

With **declarative** programming you write logic that expresses *what* you want, but not necessarily *how* to achieve it. You declare your desired results, but not the necessarily step-by-step.

In our case, this concept is materialized by attributes, e.g.
```C#
[RequiredIfExpression(
    Expression = "{0} && !{1} && {2}",
    DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
    TargetValues = new object[] {true,       "Other",       "[Country]"},
    ErrorMessage = "If you plan to travel abroad, why visit the same country twice?")]
public string ReasonForTravel { get; set; }
```
With **imperative** programming you define the control flow of the computation which needs to be done. You tell the compiler what you want, exactly step by step.

If we choose this way instead of model fields decoration, it has negative impact on the complexity of the code. Logic responsible for validation is now implemented somewhere else in our application e.g. inside controllers actions instead of model class itself:
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

###What about the support of ASP.NET MVC client-side validation?

Client-side validation is fully supported. Enable it for your web project within the next few steps:

1. Reference both assemblies to your project: core [**ExpressiveAnnotations.dll**](src/ExpressiveAnnotations) and subsidiary [**ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.dll**](src/ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider),
2. In Global.asax register required validators (`IClientValidatable` interface is not directly implemented by the attribute, to avoid coupling of ExpressionAnnotations assembly with System.Web.Mvc dependency):

    ```C#
    protected void Application_Start()
    {
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (RequiredIfAttribute), typeof (RequiredIfValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (RequiredIfExpressionAttribute), typeof (RequiredIfExpressionValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (AssertThatAttribute), typeof (AssertThatValidator));
        DataAnnotationsModelValidatorProvider.RegisterAdapter(
            typeof (AssertThatExpressionAttribute), typeof (AssertThatExpressionValidator));
    ```			
3. Include [**expressive.annotations.analysis.js**](src/expressive.annotations.analysis.js) and [**expressive.annotations.validate.js**](src/expressive.annotations.validate.js) scripts in your page (do not forget standard jQuery validation scripts):

    ```JavaScript
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.analysis.js"></script>
    <script src="/Scripts/expressive.annotations.validate.js"></script>
    ```

Alternatively, using the [NuGet](https://www.nuget.org/packages/ExpressiveAnnotations/) Package Manager Console:

###`PM> Install-Package ExpressiveAnnotations`
