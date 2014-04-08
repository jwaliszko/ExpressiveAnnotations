/* expressive.annotations.validate.js - v1.1.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function ($, analyser) {

    var ModelPrefix = {
        append: function (value, prefix) {
            return prefix + value;
        },
        get: function (fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        }
    };

    var Helper = {
        extractValue: function(form, dependentProperty, prefix) {
            var dependentField = $(form).find(':input[name="' + ModelPrefix.append(dependentProperty, prefix) + '"]');
            var dependentValue = $(dependentField).val();
            if (dependentField.is(":radio")) {
                dependentValue = dependentField.filter(":checked").val();
            }
            else if (dependentField.is(":checkbox")) {
                dependentValue = dependentField.is(':checked');
            }
            return dependentValue;
        },
        fetchTargetValue: function(form, targetValue, prefix) {
            if (typeof targetValue == 'string' || targetValue instanceof String) {
                var patt = new RegExp('\\[(.+)\\]');
                if (patt.test(targetValue)) {
                    var targetProperty = targetValue.substring(1, targetValue.length - 1);
                    targetValue = $($(form).find(':input[name="' + ModelPrefix.append(targetProperty, prefix) + '"]')[0]).val();
                }
            }
            return targetValue;
        },
        compare: function(dependentValue, targetValue) {
            if (typeof dependentValue == 'string' || dependentValue instanceof String) {
                dependentValue = dependentValue.trim();
                if (dependentValue.toLowerCase() === "true")
                    dependentValue = true;
                else if (dependentValue.toLowerCase() === "false")
                    dependentValue = false;
            }
            if (typeof targetValue == 'string' || targetValue instanceof String) {
                targetValue = targetValue.trim();
            }
            return (dependentValue == targetValue) || (dependentValue != '' && targetValue == '*');
        }
    };

    $.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'targetvalue'], function(options) {
        options.rules['requiredif'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            targetvalue: $.parseJSON(options.params.targetvalue)
        };
        if (options.message) {
            options.messages['requiredif'] = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredifexpression', ['dependentproperties', 'targetvalues', 'expression'], function(options) {
        options.rules['requiredifexpression'] = {
            prefix: ModelPrefix.get(options.element.name),
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
        var dependentValue = Helper.extractValue(params.form, params.dependentproperty, params.prefix);
        var targetValue = Helper.fetchTargetValue(params.form, params.targetvalue, params.prefix);
        if (Helper.compare(dependentValue, targetValue))
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)            
            return value != null && value != '' && value != 'undefined';
        return true;
    }, '');

    $.validator.addMethod('requiredifexpression', function(value, element, params) {
        var tokens = new Array();
        var length = params.dependentproperties.length;
        for (var i = 0; i < length; i++) {
            var dependentValue = Helper.extractValue(params.form, params.dependentproperties[i], params.prefix);
            var targetValue = Helper.fetchTargetValue(params.form, params.targetvalues[i], params.prefix);
            var result = Helper.compare(dependentValue, targetValue);
            tokens.push(result.toString());
        }

        var composedExpression = analyser.Utils.String.format(params.expression, tokens);
        var evaluator = new analyser.Evaluator();
        if (evaluator.compute(composedExpression))
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)
            return value != null && value != '' && value != 'undefined';
        return true;
    }, '');

})(jQuery, BooleanExpressionsAnalyser);
