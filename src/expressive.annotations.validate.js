/* expressive.annotations.validate.js - v2.2.1
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
                if (value !== undefined && value !== null) {
                    return value.toString();
                }
                return { error: true, msg: 'Given value was not recognized as a valid string.' };
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
                return { error: true, msg: 'Given value was not recognized as a valid boolean.' };
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
                return { error: true, msg: 'Given value was not recognized as a valid float.' };
            }
        },
        date: {
            tryParse: function(value) {
                if (typeHelper.isDate(value)) {
                    return value.getTime();
                }
                if (typeHelper.isString(value)) {
                    if (typeof api.settings.parseDate === 'function') {
                        return api.settings.parseDate(value); // custom parsing of date string given in non-standard format
                    }
                    var milisec = Date.parse(value); // default parsing of string representing an RFC 2822 or ISO 8601 date
                    if (!/Invalid|NaN/.test(milisec)) {
                        return milisec;
                    }
                }
                return { error: true, msg: 'Given value was not recognized as a valid RFC 2822 or ISO 8601 date.' };
            }
        },
        guid: {
            tryParse: function(value) {
                if (typeHelper.isGuid(value)) {
                    return value.toUpperCase();;
                }
                return { error: true, msg: 'Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).' };
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
        isGuid: function(value) {
            return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value); // basic check
        },
        tryParse: function(value, type) {
            switch (type) {
                case 'datetime':
                    return typeHelper.date.tryParse(value);
                case 'numeric':
                    return typeHelper.float.tryParse(value);
                case 'string':
                    return typeHelper.string.tryParse(value);
                case 'bool':
                    return typeHelper.bool.tryParse(value);
                case 'guid':
                    return typeHelper.guid.tryParse(value);
                default:
                    return { error: true, msg: typeHelper.string.format('Supported types: datetime, numeric, string, bool and guid. Invalid target type: {0}', type) };
            }
        }
    },

    toolchain = {
        methods: {},
        addMethod: function (name, func) { // add multiple function signatures to methods object (methods overloading, based only on numbers of arguments)
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
                if (strA === strB)
                    return 0;
                if (strA !== null && strB === null)
                    return 1;
                if (strA === null && strB !== null)
                    return -1;                
                return strA > strB ? 1 : -1;
            });
            this.addMethod("CompareOrdinalIgnoreCase", function(strA, strB) {
                strA = (strA !== null && strA !== undefined) ? strA.toLowerCase() : null;
                strB = (strB !== null && strB !== undefined) ? strB.toLowerCase() : null;
                return this.CompareOrdinal(strA, strB);
            });            
            this.addMethod("StartsWith", function(str, prefix) {
                return str !== null && str !== undefined && prefix !== null && prefix !== undefined && str.slice(0, prefix.length) === prefix;
            });
            this.addMethod("StartsWithIgnoreCase", function(str, prefix) {
                str = (str !== null && str !== undefined) ? str.toLowerCase() : null;
                prefix = (prefix !== null && prefix !== undefined) ? prefix.toLowerCase() : null;
                return this.StartsWith(str, prefix);
            });
            this.addMethod("EndsWith", function(str, suffix) {
                return str !== null && str !== undefined && suffix !== null && suffix !== undefined && str.slice(-suffix.length) === suffix;
            });
            this.addMethod("EndsWithIgnoreCase", function(str, suffix) {
                str = (str !== null && str !== undefined) ? str.toLowerCase() : null;
                suffix = (suffix !== null && suffix !== undefined) ? suffix.toLowerCase() : null;
                return this.EndsWith(str, suffix);
            });
            this.addMethod("Contains", function(str, substr) {
                return str !== null && str !== undefined && substr !== null && substr !== undefined && str.indexOf(substr) > -1;
            });
            this.addMethod("ContainsIgnoreCase", function(str, substr) {
                str = (str !== null && str !== undefined) ? str.toLowerCase() : null;
                substr = (substr !== null && substr !== undefined) ? substr.toLowerCase() : null;
                return this.Contains(str, substr);
            });
            this.addMethod("IsNullOrWhiteSpace", function(str) {
                return str === null || !/\S/.test(str);
            });
            this.addMethod("IsDigitChain", function(str) {
                return /^\d+$/.test(str);
            });
            this.addMethod("IsNumber", function(str) {
                return /^[\+-]?\d*\.?\d+(?:[eE][\+-]?\d+)?$/.test(str);
            });
            this.addMethod("IsEmail", function(str) {
                // taken from HTML5 specification: http://www.w3.org/TR/html5/forms.html#e-mail-state-(type=email)
                return /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/.test(str);
            });
            this.addMethod("IsUrl", function(str) {
                // contributed by Diego Perini: https://gist.github.com/dperini/729294 (https://mathiasbynens.be/demo/url-regex)
                return /^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/\S*)?$/i.test(str);
            });
            this.addMethod("IsRegexMatch", function(str, regex) {
                return str !== null && str !== undefined && regex !== null && regex !== undefined && new RegExp(regex).test(str);
            });
            this.addMethod("Guid", function(str) {
                var guid = typeHelper.guid.tryParse(str);
                if (guid.error) {
                    throw guid.msg;
                }
                return guid;
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
                throw typeHelper.string.format('DOM field {0} not found.', name);
            }

            fieldValue = getFieldValue(field);
            if (fieldValue === undefined || fieldValue === null || fieldValue === '') { // field value not set
                return null;
            }

            parsedValue = typeHelper.tryParse(fieldValue, type); // convert to required type
            if (parsedValue.error) {
                throw typeHelper.string.format('DOM field {0} value conversion to {1} failed. {2}', name, type, parsedValue.msg);
            }

            return parsedValue;
        },
        deserializeObject: function(form, fieldsMap, constsMap, prefix) {
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
            for (name in fieldsMap) {
                if (fieldsMap.hasOwnProperty(name)) {
                    type = fieldsMap[name];
                    value = this.extractValue(form, name, prefix, type);
                    buildFieldInternal(name, value, o);
                }
            }
            for (name in constsMap) {
                if (constsMap.hasOwnProperty(name)) {
                    value = constsMap[name];
                    buildFieldInternal(name, value, o);
                }
            }
            toolchain.registerMethods(o);
            return o;
        }
    },

    annotations = ' abcdefghijklmnopqrstuvwxyz'.split(''), // suffixes for attributes annotating single field multiple times
        
    backup = window.ea, // map over the ea in case of overwrite

    api = {
        settings: {
            parseDate: undefined // provide implementation to parse date in non-standard format
                                 // e.g., suppose DOM field date is given in dd/mm/yyyy format:
                                 // parseDate = function(str) { // input string is given as a raw value extracted from DOM element
                                 //     var arr = str.split('/'); return new Date(arr[2], arr[1] - 1, arr[0]).getTime(); // return milliseconds since January 1, 1970, 00:00:00 UTC
                                 // }
        },
        addMethod: function(name, func) {
            toolchain.addMethod(name, func);
        },
        noConflict: function() {
            if (window.ea === this) {
                window.ea = backup;
            }
            return this;
        },
        ___6BE7863DC1DB4AFAA61BB53FF97FE169 : {
            typeHelper: typeHelper,
            modelHelper: modelHelper,
            toolchain: toolchain
        }
    };

    $.each(annotations, function(idx, val) { // should be optimized in terms of memory consumption (redundant handlers shouldn't be generated, there needs to be exactly as many handlers as there are unique annotations)
        var adapter = 'assertthat' + val.trim();
        $.validator.unobtrusive.adapters.add(adapter, ['expression', 'fieldsmap', 'constsmap'], function(options) {
            options.rules[adapter] = {
                prefix: modelHelper.getPrefix(options.element.name),
                form: options.form,
                expression: $.parseJSON(options.params.expression),
                fieldsmap: $.parseJSON(options.params.fieldsmap),
                constsmap: $.parseJSON(options.params.constsmap)
            };
            if (options.message) {
                options.messages[adapter] = options.message;
            }
        });
    });

    $.each(annotations, function(idx, val) {
        var adapter = 'requiredif' + val.trim();
        $.validator.unobtrusive.adapters.add(adapter, ['expression', 'fieldsmap', 'constsmap', 'allowempty'], function(options) {
            options.rules[adapter] = {
                prefix: modelHelper.getPrefix(options.element.name),
                form: options.form,
                expression: $.parseJSON(options.params.expression),
                fieldsmap: $.parseJSON(options.params.fieldsmap),
                constsmap: $.parseJSON(options.params.constsmap),
                allowempty: $.parseJSON(options.params.allowempty)
            };
            if (options.message) {
                options.messages[adapter] = options.message;
            }
        });
    });

    $.each(annotations, function(idx, val) {
        var method = 'assertthat' + val.trim();
        $.validator.addMethod(method, function(value, element, params) {
            value = $(element).attr('type') === 'checkbox' ? $(element).is(':checked') : value; // special treatment for checkbox, because when unchecked, false value should be retrieved instead of undefined
            if (!(value === undefined || value === null || value === '')) { // check if the field value is set (continue if so, otherwise skip condition verification)
                var model = modelHelper.deserializeObject(params.form, params.fieldsmap, params.constsmap, params.prefix);
                with (model) {
                    if (!eval(params.expression)) { // check if the assertion condition is not satisfied
                        return false; // assertion not satisfied => notify
                    }
                }
            }
            return true;
        }, '');
    });

    $.each(annotations, function(idx, val) {
        var method = 'requiredif' + val.trim();
        $.validator.addMethod(method, function(value, element, params) {
            value = $(element).attr('type') === 'checkbox' ? $(element).is(':checked') : value;
            if (value === undefined || value === null || value === '' // check if the field value is not set (undefined, null or empty string treated at client as null at server)
                || (!/\S/.test(value) && !params.allowempty)) {
                var model = modelHelper.deserializeObject(params.form, params.fieldsmap, params.constsmap, params.prefix);
                with (model) {
                    if (eval(params.expression)) {
                        return false;
                    }
                }
            }
            return true;
        }, '');
    });
    
    window.ea = api; // expose some tiny api to the ea global object

}(jQuery, window));
