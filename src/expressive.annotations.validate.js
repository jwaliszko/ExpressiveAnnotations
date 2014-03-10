/* expressive.annotations.validate.js
 * client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * license: http://www.opensource.org/licenses/mit-license.php */

(function($) {
    var appendModelPrefix = function(val, pref) {
        return pref + val;
    };

    var extractValue = function(form, dependentProperty, prefix) {
        var dependentField = $(form).find(':input[name="' + appendModelPrefix(dependentProperty, prefix) + '"]')[0];
        var dependentValue = $(dependentField).is(':checkbox')
            ? $(dependentField).is(':checked').toString()
            : $(dependentField).val();

        dependentValue = (dependentValue != null && dependentValue != 'undefined') ? dependentValue.toLowerCase() : '';
        return dependentValue;
    };

    var fetchTargetValue = function(form, targetValue, prefix) {
        var patt = new RegExp('\\[(.+)\\]');
        if (patt.test(targetValue)) {
            var targetProperty = targetValue.substring(1, targetValue.length - 1);
            targetValue = $($(form).find(':input[name="' + appendModelPrefix(targetProperty, prefix) + '"]')[0]).val();
        }

        targetValue = (targetValue != null && targetValue != 'undefined') ? targetValue.toLowerCase() : '';
        return targetValue;
    };

    $.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'targetvalue'], function(options) {
        var getModelPrefix = function(fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        };
        options.rules['requiredif'] = {
            prefix: getModelPrefix(options.element.name),
            form: options.form,
            dependentproperty: options.params.dependentproperty,
            targetvalue: options.params.targetvalue
        };
        if (options.message) {
            options.messages['requiredif'] = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredifexpression', ['dependentproperties', 'targetvalues', 'expression'], function(options) {
        var getModelPrefix = function(fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        };
        options.rules['requiredifexpression'] = {
            prefix: getModelPrefix(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            targetvalues: $.parseJSON(options.params.targetvalues),
            expression: options.params.expression
        };
        if (options.message) {
            options.messages['requiredifexpression'] = options.message;
        }
    });

    $.validator.addMethod('requiredif', function(value, element, params) {
        var dependentValue = extractValue(params.form, params.dependentproperty, params.prefix);
        var targetValue = fetchTargetValue(params.form, params.targetvalue, params.prefix);
        if ((dependentValue == targetValue) || (dependentValue != '' && targetValue == '*'))
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)            
            return value != null && value != '';
        return true;
    }, '');

    $.validator.addMethod('requiredifexpression', function(value, element, params) {
        var tokens = new Array();
        var length = params.dependentproperties.length;
        for (var i = 0; i < length; i++) {
            var dependentValue = extractValue(params.form, params.dependentproperties[i], params.prefix);
            var targetValue = fetchTargetValue(params.form, params.targetvalues[i], params.prefix);
            var result = ((dependentValue == targetValue) || (dependentValue != '' && targetValue == '*'));
            tokens.push(result.toString());
        }
        var composedExpression = String.format(params.expression, tokens);
        var evaluator = new Evaluator();
        if (evaluator.compute(composedExpression))
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)
            return value != null && value != '';
        return true;
    }, '');

})(jQuery);
