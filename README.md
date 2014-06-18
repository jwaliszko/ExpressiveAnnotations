#ExpressiveAnnotations - annotation-based conditional validation

<sub>**Notice: This document describes latest implementation. For previous version (concept) &lt; 2.0 take a look at [EA1 branch](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/EA1).**</sub>

ExpressiveAnnotations is a small .NET and JavaScript library, which provides annotation-based conditional validation mechanisms. Given implementations of RequiredIf and AssertThat attributes allows to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more compacted, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###RequiredIf vs AssertThat attributes?

RequiredIf indicates that annotated field is required, when given condition is fulfilled. AssertThat on the other hand indicates, that non-null annotated field is considered as valid, when given condition is fulfilled.

###What are brief examples of usage?

For sample usages go to [**demo project**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcWebSample).

```
[RequiredIf("GoAbroad == true")]
public string PassportNumber { get; set; }
```

Here we are saying that annotated field is required when dependent field has appropriate value (passport number is required, if go abroad option is selected). Simple enough, let's move to another variation:

```
[RequiredIf("ContactDetails.Email != null")]
public bool AgreeToContact { get; set; }
```

This one means, that if email is non-null, boolean value indicating contact permission has to be true. What is more, we can see here that nested properties are supported by the mechanism. 

```
[AssertThat("ReturnDate >= Today()")]
public DateTime? ReturnDate { get; set; }
```

Here return date needs to be greater than or equal to the date given in utility function returning current day. This time we are not validating field requirement as before. Now attribute puts restriction on field, which needs to be satisfied for such field to be considered as valid (restriction verification is executed for non-empty field).
 
```
[RequiredIf("GoAbroad == true " +
			"&& (" +
					"(NextCountry != 'Other' && NextCountry == Country) " +
					"|| (Age > 24 && Age <= 55)" +
				")")]
public string ReasonForTravel { get; set; }
```

<sub>Notice: Expression is splitted into multiple lines because is more meaningful and easier to understand.</sub>

This time, the expression above is much more complex that its predecessors, but still can be easily understand. Isn't it?

###How to construct conditional validation attributes?
#####Signatures:

```
RequiredIfAttribute([string Expression],
                    [bool AllowEmptyOrFalse] ...) - Validation attribute which indicates that 
					                                annotated field is required when computed 
													result of given logical expression is true.
AssertThatAttribute([string Expression], ...)     - Validation attribute, executed for non-null 
                                                    annotated field, which indicates that 
													assertion given in logical expression has 
													to be satisfied, for such field to be 
													considered as valid.

  Expression        - Gets or sets the logical expression based on which requirement condition 
	                  is computed.
  AllowEmptyOrFalse - Gets or sets a flag indicating whether the attribute should allow empty or
	                  whitespace strings or false boolean values (null never allowed).
```

#####Implementation:

Implementation core is based on logical expressions parser, which is based on the following grammar:

```
expression => or-exp
or-exp     => and-exp [ "||" or-exp ]
and-exp    => not-exp [ "&&" and-exp ]
not-exp    => [ "!" ] rel-exp
rel-exp    => val [ rel-op val ]
rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="
val        => "null" | int | float | bool | string | func | "(" or-exp ")"
```

At server side, expression string is parsed and converted into [expression trees](http://msdn.microsoft.com/en-us/library/bb397951.aspx). At client side, pure expression string is evaluated within the context of created model object.

###What is the context behind this implementation? 

Declarative validation, when compared to imperative approach, seems to be more convenient in many cases. Clean, compact code - all validation logic can be defined within the model metadata.

###What is the difference between declarative and imperative programming?

With **declarative** programming, you write logic that expresses what you want, but not necessarily how to achieve it. You declare your desired results, but not the step-by-step.

In our example it is more about metadata, e.g.

```
[RequiredIf("GoAbroad == true " +
			"&& (" +
					"(NextCountry != 'Other' && NextCountry == Country) " +
					"|| (Age > 24 && Age <= 55)" +
				")")]
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
            typeof(AssertThatAttribute), typeof(AssertThatValidator));
```			
3. Include [**expressive.annotations.validate.js**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/blob/master/src/expressive.annotations.validate.js) scripts in your page (do not forget standard jQuery validation scripts):

 ```
    <script src="/Scripts/jquery.validate.js"></script>
    <script src="/Scripts/jquery.validate.unobtrusive.js"></script>
    ...
    <script src="/Scripts/expressive.annotations.validate.js"></script>
```

Alternatively, using the NuGet Package Manager Console (currently only [previous version](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/EA1) is published):

###`PM> Install-Package ExpressiveAnnotations`
