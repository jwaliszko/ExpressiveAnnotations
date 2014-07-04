/* expressive.annotations.validate.js - v1.4.1
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function($, analyser) {

    'use strict';

    var modelPrefix, miscHelper, attributeInternals, expressionAttributeInternals;

    modelPrefix = {
        append: function(value, prefix) {
            return prefix + value;
        },
        get: function(fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        }
    };

    miscHelper = {
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

            var field, fieldValue, parsedValue;

            name = modelPrefix.append(name, prefix);            
            field = $(form).find(':input[name="' + name + '"]');
            if (field.length === 0) {
                throw analyser.typeHelper.String.format('DOM field {0} not found.', name);
            }

            fieldValue = getFieldValue(field);
            if (fieldValue === undefined || fieldValue === null || fieldValue === '') { // field value not set
                return null;
            }

            parsedValue = analyser.typeHelper.tryParse(fieldValue, type); // convert to required type
            if (parsedValue.error) {
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            }

            return parsedValue;
        },
        tryExtractName: function(targetValue) {
            if (analyser.typeHelper.isString(targetValue)) {
                var patt = new RegExp('\\[(.+)\\]');
                if (patt.test(targetValue)) {
                    return targetValue.substring(1, targetValue.length - 1);
                }
            }
            return { error: true };
        }
    };

    attributeInternals = {
        verify: function(params) {
            var dependentValue, targetValue, targetPropertyName, comparer;

            dependentValue = miscHelper.extractValue(params.form, params.dependentproperty, params.prefix, params.type);
            targetValue = params.targetvalue;

            targetPropertyName = miscHelper.tryExtractName(targetValue);
            if (!targetPropertyName.error) {
                targetValue = miscHelper.extractValue(params.form, targetPropertyName, params.prefix, params.type);
            }

            comparer = new analyser.Comparer();
            return comparer.compute(dependentValue, targetValue, params.relationaloperator, params.sensitivecomparisons);
        }
    };

    expressionAttributeInternals = {
        verify: function(params) {
            var tokens, comparer, length, i, dependentValue, targetValue, targetPropertyName, result, composedExpression, evaluator;

            tokens = [];
            comparer = new analyser.Comparer();
            length = params.dependentproperties.length;

            for (i = 0; i < length; i++) {
                dependentValue = miscHelper.extractValue(params.form, params.dependentproperties[i], params.prefix, params.types[i]);
                targetValue = params.targetvalues[i];

                targetPropertyName = miscHelper.tryExtractName(targetValue);
                if (!targetPropertyName.error) {
                    targetValue = miscHelper.extractValue(params.form, targetPropertyName, params.prefix, params.types[i]);
                }

                result = comparer.compute(dependentValue, targetValue, params.relationaloperators[i], params.sensitivecomparisons);
                tokens.push(result.toString());
            }

            composedExpression = analyser.typeHelper.String.format(params.expression, tokens);
            evaluator = new analyser.Evaluator();
            return evaluator.compute(composedExpression);
        }
    };

    $.validator.unobtrusive.adapters.add('assertthat', ['dependentproperty', 'relationaloperator', 'targetvalue', 'type', 'sensitivecomparisons'], function(options) {
        options.rules.assertthat = {
            prefix: modelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            relationaloperator: $.parseJSON(options.params.relationaloperator),
            targetvalue: $.parseJSON(options.params.targetvalue),
            type: $.parseJSON(options.params.type),
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons)
        };
        if (options.message) {
            options.messages.assertthat = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('assertthatexpression', ['dependentproperties', 'relationaloperators', 'targetvalues', 'types', 'expression', 'sensitivecomparisons'], function(options) {
        options.rules.assertthatexpression = {
            prefix: modelPrefix.get(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            relationaloperators: $.parseJSON(options.params.relationaloperators),
            targetvalues: $.parseJSON(options.params.targetvalues),
            types: $.parseJSON(options.params.types),
            expression: options.params.expression,
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons)            
        };
        if (options.message) {
            options.messages.assertthatexpression = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'relationaloperator', 'targetvalue', 'type', 'sensitivecomparisons', 'allowemptyorfalse', 'modeltype'], function (options) {
        options.rules.requiredif = {
            prefix: modelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            relationaloperator: $.parseJSON(options.params.relationaloperator),
            targetvalue: $.parseJSON(options.params.targetvalue),
            type: $.parseJSON(options.params.type),
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons),
            allowemptyorfalse: $.parseJSON(options.params.allowemptyorfalse),
            modeltype: $.parseJSON(options.params.modeltype)

        };
        if (options.message) {
            options.messages.requiredif = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredifexpression', ['dependentproperties', 'relationaloperators', 'targetvalues', 'types', 'expression', 'sensitivecomparisons', 'allowemptyorfalse', 'modeltype'], function (options) {
        options.rules.requiredifexpression = {
            prefix: modelPrefix.get(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            relationaloperators: $.parseJSON(options.params.relationaloperators),
            targetvalues: $.parseJSON(options.params.targetvalues),
            types: $.parseJSON(options.params.types),
            expression: options.params.expression,
            sensitivecomparisons: $.parseJSON(options.params.sensitivecomparisons),
            allowemptyorfalse: $.parseJSON(options.params.allowemptyorfalse),
            modeltype: $.parseJSON(options.params.modeltype)
        };
        if (options.message) {
            options.messages.requiredifexpression = options.message;
        }
    });

    $.validator.addMethod('assertthat', function(value, element, params) {
        if (!(value === undefined || value === null || value === '')) { // check if the field value is set (continue if so, otherwise skip condition verification)
            if (!attributeInternals.verify(params)) { // check if the assertion condition is not satisfied
                return false; // assertion not satisfied => notify
            }
        }
        return true;
    }, '');

    $.validator.addMethod('assertthatexpression', function(value, element, params) {
        if (!(value === undefined || value === null || value === '')) {
            if (!expressionAttributeInternals.verify(params)) {
                return false;
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredif', function (value, element, params) {
        if (params.modeltype === 'bool') {
            var boolValue = analyser.typeHelper.Bool.tryParse(value);
            if (boolValue.error /* conversion fail indicates that field value is not set - required */ || (!boolValue && !params.allowemptyorfalse)) {
                if (attributeInternals.verify(params)) { // check if the requirement condition is satisfied
                    return false; // requirement confirmed => notify
                }
            }
        }
        if (value === undefined || value === null || value === '' // check if the field value is not set (undefined, null or empty string treated at client as null at server)
            || (!/\S/.test(value) && !params.allowemptyorfalse)) {
            if (attributeInternals.verify(params)) {
                return false;
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredifexpression', function (value, element, params) {
        if (params.modeltype === 'bool') {
            var boolValue = analyser.typeHelper.Bool.tryParse(value);
            if (boolValue.error || (!boolValue && !params.allowemptyorfalse)) {
                if (expressionAttributeInternals.verify(params)) {
                    return false;
                }
            }
        }
        if (value === undefined || value === null || value === ''
            || (!/\S/.test(value) && !params.allowemptyorfalse)) {
            if (expressionAttributeInternals.verify(params)) {
                return false;
            }
        }
        return true;
    }, '');

}(jQuery, logicalExpressionsAnalyser));
