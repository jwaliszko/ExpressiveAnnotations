/* expressive.annotations.validate.js - v2.0.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function($) {

    var typeHelper, modelHelper;

    typeHelper = {
        String: {
            format: function(text, params) {
                var i;
                if (params instanceof Array) {
                    for (i = 0; i < params.length; i++) {
                        text = text.replace(new RegExp('\\{' + i + '\\}', 'gm'), params[i]);
                    }
                    return text;
                }
                for (i = 0; i < arguments.length - 1; i++) {
                    text = text.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i + 1]);
                }
                return text;
            },
            compareOrdinal: function(strA, strB) {
                return strA === strB ? 0 : strA > strB ? 1 : -1;
            }
        },
        Bool: {
            tryParse: function(value) {
                if (typeHelper.isBool(value)) {
                    return value;
                }
                if (typeHelper.isString(value)) {
                    value = value.trim().toLowerCase();
                    if (value === 'true' || value === 'false') {
                        return value === 'true';
                    }
                }
                return { error: true, msg: 'Parsing error. Given value has no boolean meaning.' };
            }
        },
        Float: {
            tryParse: function(value) {
                function isNumber(n) {
                    return !isNaN(parseFloat(n)) && isFinite(n);
                }

                if (isNumber(value)) {
                    return parseFloat(value);
                }
                return { error: true, msg: 'Parsing error. Given value has no numeric meaning.' };
            }
        },
        Date: {
            tryParse: function(value) {
                if (typeHelper.isDate(value)) {
                    return value;
                }
                if (typeHelper.isString(value)) {
                    var milisec = Date.parse(value);
                    if (typeHelper.isNumeric(milisec)) {
                        return new Date(milisec);
                    }
                }
                return { error: true, msg: 'Parsing error. Given value is not a string representing an RFC 2822 or ISO 8601 date.' };
            }
        },

        isEmpty: function(value) {
            return value === null || value === '' || value === undefined || !/\S/.test(value);
        },
        isNumeric: function(value) {
            return typeof value === 'number' && !isNaN(value);
        },
        isDate: function(value) {
            return value instanceof Date;
        },
        isString: function(value) {
            return typeof value === 'string' || value instanceof String;
        },
        isBool: function(value) {
            return typeof value === 'boolean' || value instanceof Boolean;
        },
        tryParse: function(value, type) {
            var result;
            switch (type) {
                case 'datetime':
                    result = typeHelper.Date.tryParse(value);
                    break;
                case 'numeric':
                    result = typeHelper.Float.tryParse(value);
                    break;
                case 'string':
                    result = (value || '').toString();
                    break;
                case 'bool':
                    result = typeHelper.Bool.tryParse(value);
                    break;
                default:
                    result = { error: true };
            }
            return result.error ? { error: true } : result;
        }
    };

    modelHelper = {
        appendPrefix: function(value, prefix) {
            return prefix + value;
        },
        getPrefix: function(fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        },
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

            name = modelHelper.appendPrefix(name, prefix);
            var field, value;

            field = $(form).find(':input[name="' + name + '"]');
            if (field.length === 0) {
                throw typeHelper.String.format('DOM field {0} not found.', name);
            }

            value = getFieldValue(field);
            if (typeHelper.isEmpty(value)) {
                return null;
            }

            value = typeHelper.tryParse(value, type); // convert to required type
            if (value.error) {
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            }
            return value;
        },
        deserializeObject: function(form, typesMap, prefix) {
            var o = {}, name, type, value, props, parent, i;
            for (name in typesMap) {
                if (typesMap.hasOwnProperty(name)) {
                    type = typesMap[name];
                    value = modelHelper.extractValue(form, name, prefix, type);
                    props = name.split('.');
                    parent = o;
                    for (i = 0; i < props.length - 1; i++) {
                        name = props[i];
                        if (!parent[name]) {
                            parent[name] = {};
                        }
                        parent = parent[name];
                    }
                    name = props[props.length - 1];
                    parent[name] = value;
                }
            }
            return o;
        }
    };

    $.validator.unobtrusive.adapters.add('assertthat', ['expression', 'typesmap'], function(options) {
        options.rules.assertthat = {
            prefix: modelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            typesmap: $.parseJSON(options.params.typesmap)
        };
        if (options.message) {
            options.messages.assertthat = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredif', ['expression', 'typesmap'], function (options) {
        options.rules.requiredif = {
            prefix: modelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            typesmap: $.parseJSON(options.params.typesmap)
        };
        if (options.message) {
            options.messages.requiredif = options.message;
        }
    });

    $.validator.addMethod('assertthat', function(value, element, params) {
        if (!typeHelper.isEmpty(value)) { // check if the field is non-empty (continue if so, otherwise skip condition verification)
            var model = modelHelper.deserializeObject(params.form, params.typesmap, params.prefix);
            with (model) {
                if (!eval(params.expression)) // check if the assertion condition is not satisfied
                    return false; // assertion not satisfied => notify
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredif', function(value, element, params) {
        var boolValue = typeHelper.Bool.tryParse(value); // check if the field is empty or false (continue if so, otherwise skip condition verification)
        if (typeHelper.isEmpty(value) || (element.type === 'radio' && (!boolValue.error && !boolValue))) {
            var model = modelHelper.deserializeObject(params.form, params.typesmap, params.prefix);
            with (model) {
                if (eval(params.expression)) // check if the requirement condition is satisfied
                    return false; // requirement confirmed => notify
            }
        }
        return true;
    }, '');

}(jQuery));
