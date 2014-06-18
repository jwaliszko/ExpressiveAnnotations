/* expressive.annotations.validate.js - v2.0.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function($) {

    var typeHelper, modelHelper, toolchain;

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
            },
            tryParse: function(value) {
                if (typeHelper.isString(value)) {
                    return value;
                }
                return { error: true, msg: 'Parsing error. Given value has no boolean meaning.' };
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
                    result = typeHelper.String.tryParse(value);
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

            var field, fieldValue, parsedValue;

            name = modelHelper.appendPrefix(name, prefix);
            field = $(form).find(':input[name="' +name + '"]');
            if (field.length === 0) {
                throw typeHelper.String.format('DOM field {0} not found.', name);
            }

            fieldValue = getFieldValue(field);
            if (fieldValue === undefined || fieldValue === null || fieldValue === '') { // field value not set
                return null;
            }

            parsedValue = typeHelper.tryParse(fieldValue, type); // convert to required type
            if(parsedValue.error) {
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            }

            return parsedValue;
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
            toolchain.supplement(o);
            return o;
        }        
    };

    toolchain = {
        supplement: function(o) {
            o.Today = function() {
                var today = new Date(Date.now());
                today.setHours(0, 0, 0, 0);
                return today;
            };
            o.Trim = function(text) {
                 return (text !== undefined && text !== null) ? text.trim() : null;
            };
            o.CompareOrdinal = function(strA, strB) {
                return typeHelper.String.compareOrdinal(strA, strB);
            };
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

    $.validator.unobtrusive.adapters.add('requiredif', ['expression', 'typesmap', 'allowemptyorfalse', 'modeltype'], function (options) {
        options.rules.requiredif = {
            prefix: modelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            typesmap: $.parseJSON(options.params.typesmap),
            allowemptyorfalse: $.parseJSON(options.params.allowemptyorfalse),
            modeltype: $.parseJSON(options.params.modeltype)
        };
        if (options.message) {
            options.messages.requiredif = options.message;
        }
    });

    $.validator.addMethod('assertthat', function (value, element, params) {
        if (!(value === undefined || value === null || value === '')) { // check if the field value is set (continue if so, otherwise skip condition verification)
            var model = modelHelper.deserializeObject(params.form, params.typesmap, params.prefix);
            with (model) {
                if (!eval(params.expression)) { // check if the assertion condition is not satisfied
                    return false; // assertion not satisfied => notify
                }
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredif', function (value, element, params) {
        var model, boolValue;
        if (params.modeltype === 'bool') {
            boolValue = typeHelper.Bool.tryParse(value);
            if (boolValue.error /* conversion fail indicates that field value is not set - required */ || (!boolValue && !params.allowemptyorfalse)) {
                model = modelHelper.deserializeObject(params.form, params.typesmap, params.prefix);
                with (model) {
                    if (eval(params.expression)) { // check if the requirement condition is satisfied
                        return false; // requirement confirmed => notify
                    }
                }
            }
        }
        if (value === undefined || value === null || value === '' // check if the field value is not set (undefined, null or empty string treated at client as null at server)
            || (!/\S/.test(value) && !params.allowemptyorfalse)) {
            model = modelHelper.deserializeObject(params.form, params.typesmap, params.prefix);
            with (model) {
                if (eval(params.expression)) {
                    return false;
                }
            }
        }
        return true;
    }, '');

}(jQuery));
