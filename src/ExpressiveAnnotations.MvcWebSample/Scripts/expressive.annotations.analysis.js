/* expressive.annotations.analysis.js - v1.3.1
 * script responsible for logical expressions parsing and computation 
 * e.g. suppose there is "true && !false" expression given, and we need to know its final logical value:
 *     var evaluator = new Evaluator();
 *     var result = evaluator.compute("true && !false")
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

var LogicalExpressionsAnalyser = (function() {

    var TypeHelper = {
        Array: {
            contains: function(arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item)
                        return true;
                }
                return false;
            },
            sanatize: function(arr, item) {
                for (var i = arr.length; i--;) {
                    if (arr[i] === item)
                        arr.splice(i, 1);
                }
            }
        },
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
            tryParse: function(value) {
                function isNumber(n) {
                    return !isNaN(parseFloat(n)) && isFinite(n);
                };

                if (isNumber(value))
                    return parseFloat(value);
                return { error: true, msg: 'Parsing error. Given value has no numeric meaning.' }
            }
        },
        Date: {
            tryParse: function(value) {
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

        isEmpty: function(value) {
            return value === null || value === '' || typeof value === 'undefined' || !/\S/.test(value);
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

    function Comparer() {
        this.compute = function(dependentValue, targetValue, relationalOperator, sensitiveComparisons) {
            switch (relationalOperator) {
                case '==':
                    return equal(dependentValue, targetValue, sensitiveComparisons);
                case '!=':
                    return !equal(dependentValue, targetValue, sensitiveComparisons);
                case '>':
                    return greater(dependentValue, targetValue);
                case '>=':
                    return greater(dependentValue, targetValue) || equal(dependentValue, targetValue, sensitiveComparisons);
                case '<':
                    return less(dependentValue, targetValue);
                case '<=':
                    return less(dependentValue, targetValue) || equal(dependentValue, targetValue, sensitiveComparisons);
            }

            throw TypeHelper.String.format('Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.', relationalOperator);
        };

        var equal = function(dependentValue, targetValue, sensitiveComparisons) {
            if (TypeHelper.isEmpty(dependentValue) && TypeHelper.isEmpty(targetValue))
                return true;
            if (!TypeHelper.isEmpty(dependentValue) && targetValue === '*')
                return true;
            var date = TypeHelper.Date.tryParse(targetValue); // parsing here? - it is an exception when incompatible types are allowed, because date targets can be provided as strings
            if (TypeHelper.isDate(dependentValue) && !date.error)
                return dependentValue.getTime() == date.getTime();
            return sensitiveComparisons
                ? JSON.stringify(dependentValue) === JSON.stringify(targetValue)
                : JSON.stringify(dependentValue).toLowerCase() === JSON.stringify(targetValue).toLowerCase();
        };
        var greater = function(dependentValue, targetValue) {
            if (TypeHelper.isNumeric(dependentValue) && TypeHelper.isNumeric(targetValue))
                return dependentValue > targetValue;
            if (TypeHelper.isDate(dependentValue) && TypeHelper.isDate(targetValue))
                return dependentValue > targetValue;
            if (TypeHelper.isString(dependentValue) && TypeHelper.isString(targetValue))
                return TypeHelper.String.compareOrdinal(dependentValue, targetValue) > 0;
            var date = TypeHelper.Date.tryParse(targetValue);
            if (TypeHelper.isDate(dependentValue) && !date.error)
                return dependentValue > date;
            if (TypeHelper.isEmpty(dependentValue) || TypeHelper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        };
        var less = function(dependentValue, targetValue) {
            if (TypeHelper.isNumeric(dependentValue) && TypeHelper.isNumeric(targetValue))
                return dependentValue < targetValue;
            if (TypeHelper.isDate(dependentValue) && TypeHelper.isDate(targetValue))
                return dependentValue < targetValue;
            if (TypeHelper.isString(dependentValue) && TypeHelper.isString(targetValue))
                return TypeHelper.String.compareOrdinal(dependentValue, targetValue) < 0;
            var date = TypeHelper.Date.tryParse(targetValue);
            if (TypeHelper.isDate(dependentValue) && !date.error)
                return dependentValue < date;
            if (TypeHelper.isEmpty(dependentValue) || TypeHelper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        }
    }

    function Tokenizer(patterns) {
        var _patterns = patterns;
        var _expression;
        var _token;

        this.analyze = function(expression) {
            var tokens = new Array();
            if (expression === null || expression === '')
                return tokens;

            _expression = expression;
            while (next())
                tokens.push(_token);
            return tokens;
        };

        var next = function() {
            _expression = _expression.trim();
            if (_expression === null || _expression === '')
                return false;

            for (var i = 0; i < _patterns.length; i++) {
                var regex = new RegExp(TypeHelper.String.format('^{0}', _patterns[i]));
                var value = regex.exec(_expression);
                if (value != null) {
                    _token = value.toString();
                    _expression = _expression.substr(_token.length);
                    return true;
                }
            }

            throw TypeHelper.String.format('Lexer error. Unexpected token started at {0}.', _expression);
        };
    }

    function InfixToPostfixConverter() {
        var _infixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!', '\\(', '\\)']);

        var isInfixOperator = function(token) {
            return TypeHelper.Array.contains(['&&', '||', '!', '(', ')'], token);
        };
        var isPostfixOperator = function(token) {
            return TypeHelper.Array.contains(['&&', '||', '!'], token);
        };
        var isUnaryOperator = function(token) {
            return '!' === token;
        };
        var isLeftBracket = function(token) {
            return '(' === token;
        };
        var isRightBracket = function(token) {
            return ')' === token;
        };
        var containsLeftBracket = function(st) {
            return TypeHelper.Array.contains(st, '(');
        };

        this.convert = function(expression) {
            var tokens = _infixTokenizer.analyze(expression);
            var operators = new Array();
            var output = new Array();

            var length = tokens.length;
            for (var i = 0; i < length; i++) {
                var token = tokens[i];
                if (isInfixOperator(token)) {
                    if (isRightBracket(token)) {
                        if (!containsLeftBracket(operators))
                            throw 'Infix expression parsing error. Incorrect nesting.';

                        popNestedOperators(operators, output);
                        popCorrespondingUnaryOperators(operators, output);
                    } else
                        operators.push(token);
                } else {
                    output.push(token);
                    popCorrespondingUnaryOperators(operators, output);
                }
            }

            if (operators.length > 0 && containsLeftBracket(operators))
                throw 'Infix expression parsing error. Incorrect nesting.';

            popRemainingOperators(operators, output);
            return output.join(' ');
        };

        var popNestedOperators = function(operators, output) {
            var length = operators.length;
            for (var i = 0; i < length; i++) {
                var top = operators.pop();
                if (isPostfixOperator(top))
                    output.push(top);
                if (isLeftBracket(top))
                    break;
            }
        };
        var popCorrespondingUnaryOperators = function(operators, output) {
            var length = operators.length;
            for (var i = 0; i < length; i++) {
                var top = operators[operators.length - 1]; // peek
                if (isUnaryOperator(top) && isPostfixOperator(top)) {
                    top = operators.pop();
                    output.push(top);
                } else
                    break;
            }
        };
        var popRemainingOperators = function(operators, output) {
            var length = operators.length;
            for (var i = 0; i < length; i++) {
                var top = operators.pop();
                if (isPostfixOperator(top))
                    output.push(top);
            }
        };
    }

    function PostfixParser() {
        var _postfixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!']);

        this.evaluate = function(expression) {
            var st = _postfixTokenizer.analyze(expression, true);
            var result = evaluate(st);
            if (st.length != 0)
                throw 'RPN expression parsing error. Incorrect nesting.';
            return result;
        };

        var evaluate = function(st) {
            if (st.length === 0)
                throw 'Stack empty.';
            var top = st.pop();

            if ('true' === top || 'false' === top)
                return top === 'true';

            var y = evaluate(st);
            if (top === '!')
                return !y;

            var x = evaluate(st);

            switch (top) {
                case '&&':
                    x &= y;
                    break;
                case '||':
                    x |= y;
                    break;
                default:
                    throw TypeHelper.String.format('RPN expression parsing error. Token {0} not expected.', top);
            }
            return x;
        };
    }

    function Evaluator() {
        var _converter = new InfixToPostfixConverter();
        var _parser = new PostfixParser();

        this.compute = function(expression) {
            try {
                return _parser.evaluate(_converter.convert(expression));
            } catch (e) {
                throw 'Logical expression computation failed. Expression is broken.';
            }
        };
    }

    return {
        Tokenizer: Tokenizer,
        InfixToPostfixConverter: InfixToPostfixConverter,
        PostfixParser: PostfixParser,
        Evaluator: Evaluator,
        Comparer: Comparer,
        TypeHelper: TypeHelper        
    };

})();
