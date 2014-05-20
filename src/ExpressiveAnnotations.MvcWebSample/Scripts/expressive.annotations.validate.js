/* expressive.annotations.validate.js - v1.2.0
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
        extractValue: function (form, dependentProperty, prefix, type) {
            function getFieldValue(field) {
                var fieldType = $(field).attr('type');
                switch (fieldType) {
                    case 'checkbox':
                        return $(field).is(':checked');
                    case 'radio':
                        return $(field).filter(':checked').val();
                    default:
                        return $(field).val();
                }
            }
            var dependentField = $(form).find(':input[name="' + ModelPrefix.append(dependentProperty, prefix) + '"]');
            var fieldValue = getFieldValue(dependentField);
            if (Helper.isEmpty(fieldValue))
                return null;

            var dependentValue = Helper.tryParse(fieldValue, type); // convert to required type
            if (dependentValue.error)
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            return dependentValue;
        },
        tryExtractPropertyName: function (targetValue) {
            if (Helper.isString(targetValue)) {
                var patt = new RegExp('\\[(.+)\\]');
                if (patt.test(targetValue)) {
                    var targetProperty = targetValue.substring(1, targetValue.length - 1);
                    return targetProperty;
                }
            }
            return { error: true }
        },
        compute: function (dependentValue, targetValue, relationalOperator) {
            switch (relationalOperator) {
                case '==':
                    return Helper.compare(dependentValue, targetValue);
                case "!=":
                    return !Helper.compare(dependentValue, targetValue);
                case ">":
                    return Helper.greater(dependentValue, targetValue);
                case ">=":
                    return !Helper.less(dependentValue, targetValue);
                case "<":
                    return Helper.less(dependentValue, targetValue);
                case "<=":
                    return !Helper.greater(dependentValue, targetValue);
            }

            throw analyser.Utils.String.format('Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.', relationalOperator);
        },
        compare: function (dependentValue, targetValue) {
            return (dependentValue === targetValue)
                    || (Helper.isString(dependentValue) && Helper.isString(targetValue)
                        && dependentValue.trim().toLowerCase() === targetValue.trim().toLowerCase())
                    || (!Helper.isEmpty(dependentValue) && targetValue === '*')
                    || (Helper.isEmpty(dependentValue) && Helper.isEmpty(targetValue));
        },
        greater: function (dependentValue, targetValue) {            
            if (Helper.isNumber(dependentValue) && Helper.isNumber(targetValue))
                return dependentValue > targetValue;
            if (Helper.isDate(dependentValue) && Helper.isDate(targetValue))
                return dependentValue > targetValue;
            if (Helper.isString(dependentValue) && Helper.isString(targetValue))
                return dependentValue.toLowerCase().localeCompare(targetValue.toLowerCase()) > 0;
            if (Helper.isEmpty(dependentValue) || Helper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        },
        less: function (dependentValue, targetValue) {            
            if (Helper.isNumber(dependentValue) && Helper.isNumber(targetValue))
                return dependentValue < targetValue;
            if (Helper.isDate(dependentValue) && Helper.isDate(targetValue))
                return dependentValue < targetValue;
            if (Helper.isString(dependentValue) && Helper.isString(targetValue))
                return dependentValue.toLowerCase().localeCompare(targetValue.toLowerCase()) < 0;
            if (Helper.isEmpty(dependentValue) || Helper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        },
        isDate: function (value) {
            return value instanceof Date;
        },
        isNumber: function(value) {
            return !isNaN(parseFloat(value)) && isFinite(value);
        },
        isString: function (value) {
            return typeof value === 'string' || value instanceof String;
        },
        isEmpty: function (value) {
            return value === null || value === '' || typeof (value) === 'undefined' || (Helper.isString(value) && value.trim() === '');
        },
        tryParse: function (value, type) {
            var result;
            switch (type) {
                case "datetime":
                    result = analyser.Utils.Date.tryParse(value);
                    break;
                case "numeric":
                    result = analyser.Utils.Float.tryParse(value);
                    break;
                case "string":
                    result = value.toString();
                    break;
                case "bool":
                    result = analyser.Utils.Bool.tryParse(value);
                    break;
                default:
                    result = { error: true }
            }
            return result.error ? { error: true } : result;
        }
    };

    $.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'relationaloperator', 'targetvalue', 'type'], function (options) {
        options.rules['requiredif'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperty: $.parseJSON(options.params.dependentproperty),
            relationaloperator: $.parseJSON(options.params.relationaloperator),
            targetvalue: $.parseJSON(options.params.targetvalue),
            type: $.parseJSON(options.params.type)
        };
        if (options.message)
            options.messages['requiredif'] = options.message;
    });

    $.validator.unobtrusive.adapters.add('requiredifexpression', ['dependentproperties', 'relationaloperators', 'targetvalues', 'types', 'expression'], function (options) {
        options.rules['requiredifexpression'] = {
            prefix: ModelPrefix.get(options.element.name),
            form: options.form,
            dependentproperties: $.parseJSON(options.params.dependentproperties),
            relationaloperators: $.parseJSON(options.params.relationaloperators),
            targetvalues: $.parseJSON(options.params.targetvalues),
            types: $.parseJSON(options.params.types),
            expression: options.params.expression
        };
        if (options.message)
            options.messages['requiredifexpression'] = options.message;
    });

    $.validator.addMethod('requiredif', function(value, element, params) {
        var dependentValue = Helper.extractValue(params.form, params.dependentproperty, params.prefix, params.type);
        var targetValue = params.targetvalue;

        var targetPropertyName = Helper.tryExtractPropertyName(targetValue);
        if (!targetPropertyName.error)
            targetValue = Helper.extractValue(params.form, targetPropertyName, params.prefix, params.type);

        if (Helper.compute(dependentValue, targetValue, params.relationaloperator)) {
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)
            var boolValue = analyser.Utils.Bool.tryParse(value);
            if (Helper.isEmpty(value) || (!boolValue.error && !boolValue))
                return false;
        }
        return true;
    }, '');

    $.validator.addMethod('requiredifexpression', function(value, element, params) {
        var tokens = new Array();
        var length = params.dependentproperties.length;
        for (var i = 0; i < length; i++) {
            var dependentValue = Helper.extractValue(params.form, params.dependentproperties[i], params.prefix, params.types[i]);
            var targetValue = params.targetvalues[i];

            var targetPropertyName = Helper.tryExtractPropertyName(targetValue);
            if (!targetPropertyName.error)
                targetValue = Helper.extractValue(params.form, targetPropertyName, params.prefix, params.types[i]);

            var result = Helper.compute(dependentValue, targetValue, params.relationaloperators[i]);
            tokens.push(result.toString());
        }

        var composedExpression = analyser.Utils.String.format(params.expression, tokens);
        var evaluator = new analyser.Evaluator();
        if (evaluator.compute(composedExpression)) {
            // match (condition fulfilled) => means we should try to validate this field (check if required value is provided)
            var boolValue = analyser.Utils.Bool.tryParse(value);
            if (Helper.isEmpty(value) || (!boolValue.error && !boolValue))
                return false;
        }
        return true;
    }, '');

})(jQuery, BooleanExpressionsAnalyser);
