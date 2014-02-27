/*!
	ExpressiveAnnotations
	jQuery conditional validation library
	(c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
	license: http://www.opensource.org/licenses/mit-license.php
*/

jQuery.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'targetvalue'], function (options) {

    var getModelPrefix = function (fieldName) {
        return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
    };

    var prefix = getModelPrefix(options.element.name);

    var bag = new Object();
    bag.prefix = prefix;
    bag.form = options.form;
    bag.dependentproperty = options.params.dependentproperty;
    bag.targetvalue = options.params.targetvalue;

    options.rules['requiredif'] = bag;
    if (options.message) {
        options.messages['requiredif'] = options.message;
    }
}
);

jQuery.validator.addMethod('requiredif', function (value, element, params) {

    var appendModelPrefix = function (val, pref) {
        if (val.indexOf('*.') === 0) {
            val = val.replace('*.', pref);
        }
        return val;
    };

    var prefix = params.prefix;
    var form = params.form;
    var dependentProperty = params.dependentproperty;
    var targetValue = params.targetvalue;

    var dependentValue = '';
    var dependentField = $(form).find(':input[name="' + appendModelPrefix(dependentProperty, prefix) + '"]')[0];
    if ($(dependentField).is(':checkbox'))
        dependentValue = $(dependentField).is(':checked').toString();
    else
        dependentValue = $(dependentField).val();

    var patt = new RegExp('\[(.+)\]');
    if (patt.test(targetValue)) {
        var targetProperty = targetValue.substring(1, targetValue.length - 1);
        targetValue = $($(form).find(':input[name="' + appendModelPrefix(targetProperty, prefix) + '"]')[0]).val();
    }

    if ((dependentValue == targetValue)
        || (dependentValue != '' && targetValue == "*")) {
        // condition fulfilled => check if required value is provided
        return value != null && value != '';
    }
    return true;
}, '');
