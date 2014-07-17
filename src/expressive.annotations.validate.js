/* expressive.annotations.validate.js - v2.0.0
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function($, window) {
var
    typeHelper = {
        string: {
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
            tryParse: function(value) {
                if (typeHelper.isString(value)) {
                    return value;
                }
                return { error: true, msg: 'Parsing error. Given value has no boolean meaning.' };
            }
        },
        bool: {
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
        float: {
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
        date: {
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
                    result = typeHelper.date.tryParse(value);
                    break;
                case 'numeric':
                    result = typeHelper.float.tryParse(value);
                    break;
                case 'string':
                    result = typeHelper.string.tryParse(value);
                    break;
                case 'bool':
                    result = typeHelper.bool.tryParse(value);
                    break;
                default:
                    result = { error: true };
            }
            return result.error ? { error: true } : result;
        }
    },

    toolchain = {
        methods: {},
        addMethod: function(name, func) { // add multiple function signatures to methods object (methods overloading, based only on numbers of arguments)
            var old = this.methods[name];
            this.methods[name] = function() {
                if (func.length === arguments.length) {
                    return func.apply(this, arguments);
                }
                if (typeof old === 'function') {
                    return old.apply(this, arguments);
                }
            };
        },
        registerMethods: function(o) {
            var name, body;
            this.initialize();
            for (name in this.methods) {
                if (this.methods.hasOwnProperty(name)) {
                    body = this.methods[name];
                    o[name] = body;
                }
            }
        },
        initialize: function() {
            this.addMethod("Now", function() {
                return new Date(Date.now());
            });
            this.addMethod("Today", function() {
                return new Date(this.Now().setHours(0, 0, 0, 0));
            });
            this.addMethod("Length", function(str) {
                return str !== null && str !== undefined ? str.length : 0;
            });
            this.addMethod("Trim", function(str) {
                return str !== null && str !== undefined ? str.trim() : null;
            });
            this.addMethod("Concat", function(strA, strB) {
                return [strA, strB].join('');
            });
            this.addMethod("Concat", function(strA, strB, strC) {
                return [strA, strB, strC].join('');
            });
            this.addMethod("CompareOrdinal", function(strA, strB) {
                return strA === strB ? 0 : strA > strB ? 1 : -1;
            });
            this.addMethod("CompareOrdinalIgnoreCase", function(strA, strB) {
                strA = (strA !== null && strA !== undefined) ? strA.toLowerCase() : null;
                strB = (strB !== null && strB !== undefined) ? strB.toLowerCase() : null;
                return this.CompareOrdinal(strA, strB);
            });
            this.addMethod("IsNullOrWhiteSpace", function(str) {
                return str === null || !/\S/.test(str);
            });
            this.addMethod("IsNumber", function(str) {
                return (/^[\-+]?\d+$/).test(str) || (/^[\-+]?\d*\.\d+([eE][\-+]?\d+)?$/).test(str);
            });
        }
    },

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

            name = this.appendPrefix(name, prefix);
            field = $(form).find(':input[name="' + name + '"]');
            if (field.length === 0) {
                throw typeHelper.String.format('DOM field {0} not found.', name);
            }

            fieldValue = getFieldValue(field);
            if (fieldValue === undefined || fieldValue === null || fieldValue === '') { // field value not set
                return null;
            }

            parsedValue = typeHelper.tryParse(fieldValue, type); // convert to required type
            if (parsedValue.error) {
                throw 'Data extraction fatal error. DOM value conversion to reflect required type failed.';
            }

            return parsedValue;
        },
        deserializeObject: function(form, typesMap, enumsMap, prefix) {
            function buildFieldInternal(fieldName, fieldValue, object) {
                var props, parent, i;
                props = fieldName.split('.');
                parent = object;
                for (i = 0; i < props.length - 1; i++) {
                    fieldName = props[i];
                    if (!parent[fieldName]) {
                        parent[fieldName] = {};
                    }
                    parent = parent[fieldName];
                }
                fieldName = props[props.length - 1];
                parent[fieldName] = fieldValue;
            }

            var o = {}, name, type, value;
            for (name in typesMap) {
                if (typesMap.hasOwnProperty(name)) {
                    type = typesMap[name];
                    value = this.extractValue(form, name, prefix, type);
                    buildFieldInternal(name, value, o);
                }
            }
            for (name in enumsMap) {
                if (enumsMap.hasOwnProperty(name)) {
                    value = enumsMap[name];
                    buildFieldInternal(name, value, o);
                }
            }
            toolchain.registerMethods(o);
            return o;
        }
    },
    
    // map over the ea in case of overwrite
    backup = window.ea,

    api = {
        addMethod: function(name, func) {
            toolchain.addMethod(name, func);
        },
        noConflict: function() {
            if (window.ea === this) {
                window.ea = backup;
            }
            return this;
        }
    };

    $.validator.unobtrusive.adapters.add('assertthat', ['expression', 'typesmap', 'enumsmap'], function(options) {
        options.rules.assertthat = {
            prefix: modelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            typesmap: $.parseJSON(options.params.typesmap),
            enumsmap: $.parseJSON(options.params.enumsmap)
        };
        if (options.message) {
            options.messages.assertthat = options.message;
        }
    });

    $.validator.unobtrusive.adapters.add('requiredif', ['expression', 'typesmap', 'enumsmap', 'allowempty'], function(options) {
        options.rules.requiredif = {
            prefix: modelHelper.getPrefix(options.element.name),
            form: options.form,
            expression: $.parseJSON(options.params.expression),
            typesmap: $.parseJSON(options.params.typesmap),
            enumsmap: $.parseJSON(options.params.enumsmap),
            allowempty: $.parseJSON(options.params.allowempty)      
        };
        if (options.message) {
            options.messages.requiredif = options.message;
        }
    });

    $.validator.addMethod('assertthat', function(value, element, params) {
        value = $(element).attr('type') === 'checkbox' ? $(element).is(':checked') : value; // special treatment for checkbox, because when unchecked, false value should be retrieved instead of undefined
        if (!(value === undefined || value === null || value === '')) { // check if the field value is set (continue if so, otherwise skip condition verification)
            var model = modelHelper.deserializeObject(params.form, params.typesmap, params.enumsmap, params.prefix);
            with (model) {
                if (!eval(params.expression)) { // check if the assertion condition is not satisfied
                    return false; // assertion not satisfied => notify
                }
            }
        }
        return true;
    }, '');

    $.validator.addMethod('requiredif', function(value, element, params) {
        value = $(element).attr('type') === 'checkbox' ? $(element).is(':checked') : value;
        if (value === undefined || value === null || value === '' // check if the field value is not set (undefined, null or empty string treated at client as null at server)
            || (!/\S/.test(value) && !params.allowempty)) {
            var model = modelHelper.deserializeObject(params.form, params.typesmap, params.enumsmap, params.prefix);
            with (model) {
                if (eval(params.expression)) {
                    return false;
                }
            }
        }
        return true;
    }, '');    

    // expose some tiny api to the ea global object
    window.ea = api;

}(jQuery, window));
