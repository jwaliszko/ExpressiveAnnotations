/* expressive.annotations.validate.js - v1.3.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function($, analyser) {

    var ModelPrefix = {
        append: function(value, prefix) {
            return prefix + value;
        },
        get: function(fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        }
    };

    var MiscHelper = {
        extractValue: function(form, name, prefix, type) {
            function getFieldValue(element) {
                var elementType = $(element).attr('type');
                switch (elementType) {
                case 'checkbox':
                    return $(element).is(':checked');
                case 'radio':
                    return $(element).filter(':checked').val();
                default:
                    return $(element).val();
                }
            }

            name = ModelPrefix.append(name, prefix);
            var field = $(form).find(':input[name="' + name + '"]');
            if (field.length == 0)
                throw analyser.TypeHelper.String.format('DOM field {0} not found.', name);

            var value = getFieldValue(field);
            if (analyser.TypeHelper.isEmpty(value))
                return null;

            value = analyser.TypeHelper.tryParse(value, type); // convert to required type
            if (value.error)
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            return value;
        },
        tryExtractName: function(targetValue) {
            if (analyser.TypeHelper.isString(targetValue)) {
                var patt = new RegExp('\\[(.+)\\]');
                if (patt.test(targetValue)) {
                    var targetProperty = targetValue.substring(1, targetValue.length - 1);
                    return targetProperty;
                }
            }
            return { error: true }
        }
    }

    var AttributeInternals = {
        verify: function(element, params) {
            var dependentValue = MiscHelper.extractValue(params.form, params.dependentproperty, params.prefix, params.type);
            var targetValue = params.targetvalue;

            var targetPropertyName = MiscHelper.tryExtractName(targetValue);
            if (!targetPropertyName.error)
                targetValue = MiscHelper.extractValue(params.form, targetPropertyName, params.prefix, params.type);

            var comparer = new analyser.Comparer();
            return comparer.compute(dependentValue, targetValue, params.relationaloperator, params.sensitivecomparisons);
        }
    }
    var ExpressionAttributeInternals = {
        verify: function (element, params) {
            var tokens = new Array();
            var length = params.dependentproperties.length;
            for (var i = 0; i < length; i++) {
                var dependentValue = MiscHelper.extractValue(params.form, params.dependentproperties[i], params.prefix, params.types[i]);
                var targetValue = params.targetvalues[i];

                var targetPropertyName = MiscHelper.tryExtractName(targetValue);
                if (!targetPropertyName.error)
                    targetValue = MiscHelper.extractValue(params.form, targetPropertyName, params.prefix, params.types[i]);

                var comparer = new analyser.Comparer();
                var result = comparer.compute(dependentValue, targetValue, params.relationaloperators[i], params.sensitivecomparisons);
                tokens.push(result.toString());
            }

            var composedExpression = analyser.TypeHelper.String.format(params.expression, tokens);
            var evaluator = new analyser.Evaluator();
            return evaluator.compute(composedExpression);
        }
    }

    $.validator.unobtrusive.adapters.add('assertthat', ['dependentproperty', 'relationaloperator', 'targetvalue', 'type', 'sensitivecomparisons'], function (options) {
        options.rules['assertthat'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            relationaloperator: $.parseJSON(options.params.relationaloperator),
            targetvalue: $.parseJSON(options.params.targetvalue),
            type: $.parseJSON(options.params.type),
            sensitivecomparisons : $.parseJSON(options.params.sensitivecomparisons)
        };
        if (options.message)
            options.messages['assertthat'] = options.message;
    });

    $.validator.unobtrusive.adapters.add('assertthatexpression', ['dependentproperties', 'relationaloperators', 'targetvalues', 'types', 'expression', 'sensitivecomparisons'], function (options) {
        options.rules['assertthatexpression'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            relationaloperators: $.parseJSON(options.params.relationaloperators),
            targetvalues: $.parseJSON(options.params.targetvalues),
            types: $.parseJSON(options.params.types),
            expression: options.params.expression,
            sensitivecomparisons : $.parseJSON(options.params.sensitivecomparisons)
        };
        if (options.message)
            options.messages['assertthatexpression'] = options.message;
    });

    $.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'relationaloperator', 'targetvalue', 'type', 'sensitivecomparisons'], function(options) {
        options.rules['requiredif'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            relationaloperator: $.parseJSON(options.params.relationaloperator),
            targetvalue: $.parseJSON(options.params.targetvalue),
            type: $.parseJSON(options.params.type),
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons)
        };
        if (options.message)
            options.messages['requiredif'] = options.message;
    });

    $.validator.unobtrusive.adapters.add('requiredifexpression', ['dependentproperties', 'relationaloperators', 'targetvalues', 'types', 'expression', 'sensitivecomparisons'], function (options) {
        options.rules['requiredifexpression'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            relationaloperators: $.parseJSON(options.params.relationaloperators),
            targetvalues: $.parseJSON(options.params.targetvalues),
            types: $.parseJSON(options.params.types),
            expression: options.params.expression,
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons)
        };
        if (options.message)
            options.messages['requiredifexpression'] = options.message;
    });

    $.validator.addMethod('assertthat', function(value, element, params) {
        if (!analyser.TypeHelper.isEmpty(value)) // check if the field is non-empty (continue if so, otherwise skip condition verification)
            if (!AttributeInternals.verify(element, params)) // check if the assertion condition is not satisfied
                return false; // assertion not satisfied => notify
        return true;
    }, '');

    $.validator.addMethod('assertthatexpression', function(value, element, params) {
        if (!analyser.TypeHelper.isEmpty(value))
            if (!ExpressionAttributeInternals.verify(element, params))
                return false;
        return true;
    }, '');

    $.validator.addMethod('requiredif', function(value, element, params) {
        var boolValue = analyser.TypeHelper.Bool.tryParse(value);
        if (analyser.TypeHelper.isEmpty(value) || (!boolValue.error && !boolValue)) // check if the field is empty or false (continue if so, otherwise skip condition verification)
            if (AttributeInternals.verify(element, params)) // check if the requirement condition is satisfied 
                return false; // requirement confirmed => notify
        return true;
    }, '');

    $.validator.addMethod('requiredifexpression', function(value, element, params) {
        var boolValue = analyser.TypeHelper.Bool.tryParse(value);
        if (analyser.TypeHelper.isEmpty(value) || (!boolValue.error && !boolValue))
            if (ExpressionAttributeInternals.verify(element, params))
                return false;
        return true;
    }, '');

})(jQuery, LogicalExpressionsAnalyser);
