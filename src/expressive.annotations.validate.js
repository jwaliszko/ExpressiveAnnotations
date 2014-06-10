/* expressive.annotations.validate.js - v2.0.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function ($) {

    var ModelHelper = {
        appendPrefix: function (value, prefix) {
            return prefix + value;
        },
        getPrefix: function (fieldName) {
            return fieldName.substr(0, fieldName.lastIndexOf('.') + 1);
        },
        extractValue: function (form, name, prefix, type) {
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

            name = ModelHelper.appendPrefix(name, prefix);
            var field = $(form).find(':input[name="' + name + '"]');
            if (field.length == 0)
                throw TypeHelper.String.format('DOM field {0} not found.', name);

            var value = getFieldValue(field);
            if (TypeHelper.isEmpty(value))
                return null;

            value = TypeHelper.tryParse(value, type); // convert to required type
            if (value.error)
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            return value;
        },
        deserializeObject: function (form, types, prefix) {
            var o = {};
            for (var name in types) {
                var type = types[name];
                var value = ModelHelper.extractValue(form, name, prefix, type);
                var props = name.split('.');
                var parent = o;
                for (var i = 0; i < props.length - 1; i++) {
                    name = props[i];
                    if (!parent[name]) {
                        parent[name] = new Object();
                    }
                    parent = parent[name];
                }
                name = props[props.length - 1];
                parent[name] = value;
            }
            return o;
        }
    };

    var TypeHelper = {
        Array: {
            contains: function (arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item)
                        return true;
                }
                return false;
            },
            sanatize: function (arr, item) {
                for (var i = arr.length; i--;) {
                    if (arr[i] === item)
                        arr.splice(i, 1);
                }
            }
        },
        String: {
            format: function (text, params) {
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
            compareOrdinal: function (strA, strB) {
                return strA === strB ? 0 : strA > strB ? 1 : -1;
            }
        },
        Bool: {
            tryParse: function (value) {
                if (TypeHelper.isBool(value))
                    return value;
                if (TypeHelper.isString(value)) {
                    value = value.trim().toLowerCase();
                    if (value === 'true' || value === 'false')
                        return value === 'true';
                }
                return { error: true, msg: 'Parsing error. Given value has no boolean meaning.' }
            }
        },
        Float: {
            tryParse: function (value) {
                function isNumber(n) {
                    return !isNaN(parseFloat(n)) && isFinite(n);
                };

                if (isNumber(value))
                    return parseFloat(value);
                return { error: true, msg: 'Parsing error. Given value has no numeric meaning.' }
            }
        },
        Date: {
            tryParse: function (value) {
                if (TypeHelper.isDate(value))
                    return value;
                if (TypeHelper.isString(value)) {
                    var milisec = Date.parse(value);
                    if (TypeHelper.isNumeric(milisec))
                        return new Date(milisec);
                }
                return { error: true, msg: 'Parsing error. Given value is not a string representing an RFC 2822 or ISO 8601 date.' }
            }
        },

        isEmpty: function (value) {
            return value === null || value === '' || typeof value === 'undefined' || !/\S/.test(value);
        },
        isNumeric: function (value) {
            return typeof value === 'number' && !isNaN(value);
        },
        isDate: function (value) {
            return value instanceof Date;
        },
        isString: function (value) {
            return typeof value === 'string' || value instanceof String;
        },
        isBool: function (value) {
            return typeof value === 'boolean' || value instanceof Boolean;
        },
        tryParse: function (value, type) {
            var result;
            switch (type) {
                case 'datetime':
                    result = TypeHelper.Date.tryParse(value);
                    break;
                case 'numeric':
                    result = TypeHelper.Float.tryParse(value);
                    break;
                case 'string':
                    result = (value || '').toString();
                    break;
                case 'bool':
                    result = TypeHelper.Bool.tryParse(value);
                    break;
                default:
                    result = { error: true }
            }
            return result.error ? { error: true } : result;
        }
    };

    $.validator.unobtrusive.adapters.add('assertthat', ['expression', 'types'], function (options) {
        options.rules['assertthat'] = {
            prefix: ModelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            types: $.parseJSON(options.params.types)
        };
        if (options.message)
            options.messages['assertthat'] = options.message;
    });

    $.validator.unobtrusive.adapters.add('requiredif', ['expression', 'types'], function (options) {
        options.rules['requiredif'] = {
            prefix: ModelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            types: $.parseJSON(options.params.types)
        };
        if (options.message)
            options.messages['requiredif'] = options.message;
    });

    $.validator.addMethod('assertthat', function (value, element, params) {
        if (!TypeHelper.isEmpty(value)) { // check if the field is non-empty (continue if so, otherwise skip condition verification)
            var model = ModelHelper.deserializeObject(params.form, params.types, params.prefix);
            with (model) {
                if (!eval(params.expression)) // check if the assertion condition is not satisfied
                    return false; // assertion not satisfied => notify
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredif', function (value, element, params) {
        var boolValue = TypeHelper.Bool.tryParse(value); // check if the field is empty or false (continue if so, otherwise skip condition verification)
        if (TypeHelper.isEmpty(value) || (element.type === 'radio' && (!boolValue.error && !boolValue))) {
            var model = ModelHelper.deserializeObject(params.form, params.types, params.prefix);
            with (model) {
                if (eval(params.expression)) // check if the requirement condition is satisfied
                    return false; // requirement confirmed => notify
            }
        }
        return true;
    }, '');

})(jQuery);
