#ExpressiveAnnotations - annotation-based conditional validation

[**ExpressiveAnnotations**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations) is a small .NET library which provides annotation-based conditional validation mechanisms. Given implementation of **RequiredIf** and **RequiredIfExpression** attributes allows to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more compacted, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###What are brief examples of usage?

For sample usages go to [**demo project**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcWebSample).

* Simplest, using **RequiredIfAttribute** which *provides conditional attribute to calculate validation result based on related property value*:
 
 ```
[RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
public string PassportNumber { get; set; }
```

 This construction says that passport number is required, if go abroad option is selected. Because it is simple, let's move forward to another usage version of this attribute:

 ```
[RequiredIf(
        DependentProperty = "ContactDetails.Email",
        TargetValue = "*",
        ErrorMessage = "You have to authorize us to make contact.")]
public bool AgreeToContact { get; set; }
```

 This one mean that if email is not empty, boolean property value has to be true. We can see that nested properties are supported by the mechanism. The other thing is star character `*` used as target value - it is special character which stands for *any value*.

* More complex, using **RequiredIfExpressionAttribute** which *provides conditional attribute to calculate validation result based on related properties values and relations between them, which are defined in logical expression*:
 
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

 ```GoAbroad == true && NextCountry != "Other" && NextCountry == [value from Country]```
 
 Besides parsing interpretation of the conditional expression, this sample shows as well that instead of hardcoding there is also possibility for dynamic extraction of target values from other fields, by providing their names inside square parentheses `[]`.

###How to construct conditional validation attributes?

```
RequiredIfAttribute([string DependentProperty], 
                    [object TargetValue], ...)

    DependentProperty - Field from which runtime value is extracted.
    TargetValue       - Expected value for dependent field. Instead of hardcoding there is also
                        possibility for dynamic extraction of target value from other field, by
                        providing its name inside square parentheses. Star character stands for 
						any value.
```
```
RequiredIfExpressionAttribute([string Expression], 
                              [string DependentProperty], 
                              [object TargetValue], ...)

    Expression        - Logical expression based on which requirement condition is calculated.
                        If condition is fulfilled, error message is displayed. Attribute logic
                        replaces one or more format items in specific expression string with
                        comparison results of dependent fields and corresponding target values.
                        Available expression tokens are: &&, ||, !, {, }, numbers and whitespaces.
    DependentProperty - Dependent fields from which runtime values are extracted.
    TargetValue       - Expected values for corresponding dependent fields. Instead of hardcoding
                        there is also possibility for dynamic extraction of target values from
                        other fields, by providing their names inside square parentheses. Star 
						character stands for any value.
```

Sample `{0} || !{1}` expression evaluation steps:

1. Expression is interpreted as: 

 ```
    (DependentProperties[0] == TargetValues[0]) || (DependentProperties[1] != TargetValues[1])
    '------------------.   .------------------'    '------------------.   .------------------'
                        \ /                                            \ / 
                        {0}                                            {1}
```
 
 Note: Arrays indexes of dependent properties and its values are given in expression inside curly parentheses `{}`.
2. Arrays values are extracted and compared. Boolean computation results are inserted into corresponding brackets, let's say:

 ```(true) || (false)```
3. Such preprocessed expression is now converted from infix notation, to reflect Reverse Polish Notation (RPN) syntax:

 ```true false ||```
4. Finally postfix expression is evaluated to give validation result. Here it is true (condition fulfilled), so error message is risen.

###What is the context behind this implementation? 

Simplicity. Declarative validation is more convenient. Cleaner and more compacted code goes hand in hand with it.

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
