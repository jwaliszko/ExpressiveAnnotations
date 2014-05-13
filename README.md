#ExpressiveAnnotations - annotation-based conditional validation

[**ExpressiveAnnotations**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations) is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given implementation of RequiredIf and RequiredIfExpression attributes allows to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more compacted, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###What are brief examples of usage?

For sample usages go to [**demo project**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcWebSample).

* Simplest, using *RequiredIfAttribute* which *provides conditional attribute to calculate validation result based on related property value*:
 
 ```
[RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
public string PassportNumber { get; set; }
```

 This construction says that *passport number is required, if go abroad option is selected*. Because it is simple, let's move forward to another usage sample of this attribute:

 ```
[RequiredIf(
        DependentProperty = "ContactDetails.Email",
        TargetValue = "*",
        ErrorMessage = "You have to authorize us to make contact.")]
public bool AgreeToContact { get; set; }
```

 This one means that *if email is not empty (has any value), boolean value indicating contact permission has to be true*. What's more, we can see that nested properties are supported by the mechanism. The last thing shown here is star character `*` used as target value - it is special character which stands for any value.

* More complex, using *RequiredIfExpressionAttribute* which *provides conditional attribute to calculate validation result based on related properties values and relations between them, which are defined in logical expression*:
 
 ```
[RequiredIfExpression(
		Expression = "{0} && !{1} && {2}",
        DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
        TargetValues = new object[] {true, "Other", "[Country]"},
        ErrorMessage = "If you plan to go abroad, why do you 
                        want to visit the same country twice?")]
public string ReasonForTravel { get; set; }
```

 How such an expression should be understood?

 ```GoAbroad == true && !(NextCountry == "Other") && NextCountry == [value from Country]```
 
 Besides parsing interpretation of the conditional expression, this sample shows as well that instead of hardcoding there is also possibility for dynamic extraction of target values from other fields, by providing their names inside square parentheses `[]`.

 Finally, if we are slightly familiar with this syntax above, let's move to even more enriched use case of the same attribute *(valid from version >= 1.2)*:
 
 ```
[RequiredIfExpression(
		Expression = "{0} && ( (!{1} && {2}) || ({3} && {4}) )",
		DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry", "Age", "Age"},
		RelationalOperators = new[] {"==", "==", "==", ">", "<="},
		TargetValues = new object[] {true, "Other", "[Country]", 24, 55},
        ErrorMessage = "If you plan to go abroad and you are between 25 and 55 or plan to 
						visit the same foreign country twice, write down your reasons.")]
public string ReasonForTravel { get; set; }
```

 So, how such an expression should be understood this time instead?

 ```
 GoAbroad == true 
 && ( (NextCountry != "Other" && NextCountry == [value from Country]) 
       || Age ∈ (24, 55> 
     )
```

 Comparing to the previous example, this one basically introduces the usage of relational operators. Such operators describe relationships between dependent properties and corresponding target values (you should be also aware, that if relational operators are not explicitly provided, these relationships are by default defined by equality operator - just like in the example before).

###How to construct conditional validation attributes?
#####Signatures:

```
RequiredIfAttribute([string DependentProperty],
                    [object TargetValue],
					[string RelationalOperator], ...)

    DependentProperty  - Field from which runtime value is extracted.
    TargetValue        - Expected value for dependent field. Instead of hardcoding there is also
                         possibility for dynamic extraction of target value from other field, by
                         providing its name inside square parentheses. Star character stands for 
						 any value.
	RelationalOperator - Operator describing relation between dependent property and target value.
						 Available operators: ==, !=, >, >=, <, <=. If this property is not 
						 provided, default relation is ==.
```
```
RequiredIfExpressionAttribute([string Expression],
                              [string[] DependentProperties],
                              [object[] TargetValues],
							  [string[] RelationalOperators], ...)

    Expression          - Logical expression based on which requirement condition is calculated.
                          If condition is fulfilled, error message is displayed. Attribute logic
                          replaces one or more format items in specific expression string with
                          comparison results of dependent fields and corresponding target values.
                          Available expression tokens are: &&, ||, !, {, }, numbers and whitespaces.
    DependentProperties - Dependent fields from which runtime values are extracted.
    TargetValues        - Expected values for corresponding dependent fields. Instead of hardcoding
                          there is also possibility for dynamic extraction of target values from
                          other fields, by providing their names inside square parentheses. Star 
                          character stands for any value.
	RelationalOperators - Operators describing relations between dependent properties and target 
						  values. Available operators: ==, !=, >, >=, <, <=. If this property is 
						  not provided, default relation for all operands is ==.
```

#####Theoretical background:
Logical expression is an expression in which relationship between operands is specified by logical operators `AND (&&)` and `OR (||)`. The logical operator `NOT (!)` is used to negate logical variables or constants. It is the type of operator `(AND, OR)` that characterizes the expression as logical, not the type of operand. Basic logical expression consists of three parts: two operands and one operator, e.g. `{idx0} && {idx1}`. Operands on the other hand can be logical variables or other expressions, such as relational expressions. Relational expressions are characterized by relational operators `EQ (==), NE (!=), GT (>), GE (>=), LT (<), LE (<=)`. In our example, operands `{idx}` are actually expanded into basic relational expressions (e.g. `DependentProperties[idx] RelationalOperators[idx] TargetValues[idx]`).

#####Logical expression schematic interpretation:

 ```
  == (default), !=, >, >=, <, <=        ||, &&           !
             /---------\             /-----------\ /------------\
(DepProps[0] RelOpers[0] TarVals[0]) BinaryLogOper (UnaryLogOper)(DepProps[1] RelOpers[1] TarVals[1])
\----------------------------------/                             \----------------------------------/
   {operand 0} (relational expr)                                    {operand 1} (relational expr)
```
Notice: Forgive the usage of abbreviated names (due to narrow space).

#####Evaluation steps for sample `{0} || !{1}` logical expression:

1. Expression is interpreted as:

 ```(DependentProperties[0] == TargetValues[0]) || !(DependentProperties[1] == TargetValues[1])```

 Notice 1: Interpretation is based on assumption that relational operators are not provided - when computing operands, equality opereator is taken by default.

 Notice 2: It's easy to guess that arrays indexes of dependent properties and its corresponding target values are given inside curly brackets `{}`.
2. Values are extracted from arrays and computed (compared for equality in this case). Next, computation results (boolean flags) are injected into corresponding brackets, let's say:

 ```(true) || (false)```
3. Such preprocessed expression is then converted from infix notation syntax into postfix one:

 ```true false ||```
4. Reverse Polish Notation (RPN) expression is finally evaluated to give validation result. Here it is true - condition fulfilled, so error message is risen.

###What is the context behind this implementation? 

Declarative validation, when compared to imperative approach, seems to be more convenient in many cases. Clean, compact code - all validation logic can be defined within the model metadata.

###What is the difference between declarative and imperative programming?

With **declarative** programming, you write logic that expresses what you want, but not necessarily how to achieve it. You declare your desired results, but not the step-by-step.

In our example it's more about metadata, e.g.

```
[RequiredIfExpression(
    Expression = "{0} && !{1} && {2}",
    DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
    TargetValues = new object[] {true, "Other", "[Country]"},
    ErrorMessage = "If you plan to go abroad, why do you 
                    want to visit the same country twice?")]
public string ReasonForTravel { get; set; }
```

With **imperative** programming, you define the control flow of the computation which needs to be done. You tell the compiler what you want, step by step.

If we choose this way instead of model fields decoration, it has negative impact on the complexity of the code. Logic responsible for validation is now implemented somewhere else in our application e.g. inside controllers actions instead of model class itself:
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
            typeof(RequiredIfExpressionAttribute), typeof(RequiredIfExpressionValidator));
```			
3. Include [**expressive.annotations.analysis.js**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/blob/master/src/expressive.annotations.analysis.js) and [**expressive.annotations.validate.js**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/blob/master/src/expressive.annotations.validate.js) scripts in your page (do not forget standard jQuery validation scripts):

 ```
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.analysis.js"></script>
    <script src="/Scripts/expressive.annotations.validate.js"></script>
```

Alternatively, using the NuGet Package Manager Console:

###`PM> Install-Package ExpressiveAnnotations`
