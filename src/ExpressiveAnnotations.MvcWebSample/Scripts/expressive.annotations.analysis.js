/* expressive.annotations.analysis.js - v1.4.0
 * script responsible for logical expressions parsing and computation 
 * e.g. suppose there is "true && !false" expression given, and we need to know its final logical value:
 *     var evaluator = new Evaluator();
 *     var result = evaluator.compute("true && !false")
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

var logicalExpressionsAnalyser = (function() {

    'use strict';

    var typeHelper = {
        Array: {
            contains: function(arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item) {
                        return true;
                    }
                }
                return false;
            },
            sanatize: function (arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item) {
                        arr.splice(i, 1);
                    }
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

    function Comparer() {
        function equal(dependentValue, targetValue, sensitiveComparisons) {
            if (!(dependentValue === undefined || dependentValue === null || dependentValue === '' || !/\S/.test(dependentValue)) && targetValue === '*') { // wildcard target doesn't allow null, empty or whitespace strings
                return true;
            }
            var date = typeHelper.Date.tryParse(targetValue); // parsing here? - it is an exception when incompatible types are allowed, because date targets can be provided as strings
            if (typeHelper.isDate(dependentValue) && !date.error) {
                return dependentValue.getTime() === date.getTime();
            }
            return sensitiveComparisons
                ? JSON.stringify(dependentValue) === JSON.stringify(targetValue)
                : JSON.stringify(dependentValue).toLowerCase() === JSON.stringify(targetValue).toLowerCase();
        }
        function greater(dependentValue, targetValue) {
            if (typeHelper.isNumeric(dependentValue) && typeHelper.isNumeric(targetValue)) {
                return dependentValue > targetValue;
            }
            if (typeHelper.isDate(dependentValue) && typeHelper.isDate(targetValue)) {
                return dependentValue > targetValue;
            }
            if (typeHelper.isString(dependentValue) && typeHelper.isString(targetValue)) {
                return typeHelper.String.compareOrdinal(dependentValue, targetValue) > 0;
            }
            var date = typeHelper.Date.tryParse(targetValue);
            if (typeHelper.isDate(dependentValue) && !date.error) {
                return dependentValue > date;
            }
            if (dependentValue === undefined || dependentValue === null || dependentValue === '' || targetValue === undefined || targetValue === null || targetValue === '') {
                return false;
            }

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        }
        function less(dependentValue, targetValue) {
            if (typeHelper.isNumeric(dependentValue) && typeHelper.isNumeric(targetValue)) {
                return dependentValue < targetValue;
            }
            if (typeHelper.isDate(dependentValue) && typeHelper.isDate(targetValue)) {
                return dependentValue < targetValue;
            }
            if (typeHelper.isString(dependentValue) && typeHelper.isString(targetValue)) {
                return typeHelper.String.compareOrdinal(dependentValue, targetValue) < 0;
            }
            var date = typeHelper.Date.tryParse(targetValue);
            if (typeHelper.isDate(dependentValue) && !date.error) {
                return dependentValue < date;
            }
            if (dependentValue === undefined || dependentValue === null || dependentValue === '' || targetValue === undefined || targetValue === null || targetValue === '') {
                return false;
            }

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        }

        this.compute = function (dependentValue, targetValue, relationalOperator, sensitiveComparisons) {
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

            throw typeHelper.String.format('Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.', relationalOperator);
        };
    }

    function Tokenizer(patterns) {
        var _patterns, _expression, _token;

        _patterns = patterns;

        function next() {
            _expression = _expression.trim();
            if (_expression === null || _expression === '') {
                return false;
            }

            var i, regex, value;
            for (i = 0; i < _patterns.length; i++) {
                regex = new RegExp(typeHelper.String.format('^{0}', _patterns[i]));
                value = regex.exec(_expression);
                if (value !== null && value !== undefined) {
                    _token = value.toString();
                    _expression = _expression.substr(_token.length);
                    return true;
                }
            }

            throw typeHelper.String.format('Tokenizer error. Unexpected token started at {0}.', _expression);
        }

        this.analyze = function (expression) {
            var tokens = [];
            if (expression === null || expression === '') {
                return tokens;
            }

            _expression = expression;
            while (next()) {
                tokens.push(_token);
            }
            return tokens;
        };
    }

    function InfixParser() {
        var _infixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!', '\\(', '\\)']);

        function isInfixOperator(token) {
            return typeHelper.Array.contains(['&&', '||', '!', '(', ')'], token);
        }
        function isPostfixOperator(token) {
            return typeHelper.Array.contains(['&&', '||', '!'], token);
        }
        function isUnaryOperator(token) {
            return '!' === token;
        }
        function isLeftBracket(token) {
            return '(' === token;
        }
        function isRightBracket(token) {
            return ')' === token;
        }
        function containsLeftBracket(st) {
            return typeHelper.Array.contains(st, '(');
        }

        function popNestedOperators(operators, output) {
            var i, top, length;
            length = operators.length;
            for (i = 0; i < length; i++) {
                top = operators.pop();
                if (isPostfixOperator(top)) {
                    output.push(top);
                }
                if (isLeftBracket(top)) {
                    break;
                }
            }
        }
        function popCorrespondingUnaryOperators(operators, output) {
            var i, top, length;
            length = operators.length;
            for (i = 0; i < length; i++) {
                top = operators[operators.length - 1]; // peek
                if (isUnaryOperator(top) && isPostfixOperator(top)) {
                    top = operators.pop();
                    output.push(top);
                } else {
                    break;
                }
            }
        }
        function popRemainingOperators(operators, output) {
            var i, top, length;
            length = operators.length;
            for (i = 0; i < length; i++) {
                top = operators.pop();
                if (isPostfixOperator(top)) {
                    output.push(top);
                }
            }
        }

        this.convert = function (expression) {
            var tokens, length, operators, output, i, token;

            tokens = _infixTokenizer.analyze(expression);
            length = tokens.length;
            operators = [];
            output = [];            

            for (i = 0; i < length; i++) {
                token = tokens[i];
                if (isInfixOperator(token)) {
                    if (isRightBracket(token)) {
                        if (!containsLeftBracket(operators)) {
                            throw 'Infix expression parsing error. Incorrect nesting.';
                        }

                        popNestedOperators(operators, output);
                        popCorrespondingUnaryOperators(operators, output);
                    } else {
                        operators.push(token);
                    }
                } else {
                    output.push(token);
                    popCorrespondingUnaryOperators(operators, output);
                }
            }

            if (operators.length > 0 && containsLeftBracket(operators)) {
                throw 'Infix expression parsing error. Incorrect nesting.';
            }

            popRemainingOperators(operators, output);
            return output.join(' ');
        };
    }

    function PostfixParser() {
        var _postfixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!']);

        function evaluate(st) {
            if (st.length === 0) {
                throw 'Stack empty.';
            }
            var top = st.pop(), x, y;
            if ('true' === top || 'false' === top) {
                return top === 'true';
            }

            y = evaluate(st);
            if (top === '!') {
                return !y;
            }

            x = evaluate(st);
            switch (top) {
                case '&&':
                    x = x && y;
                    break;
                case '||':
                    x = x || y;
                    break;
                default:
                    throw typeHelper.String.format('RPN expression parsing error. Token {0} not expected.', top);
            }

            return x;
        }

        this.evaluate = function(expression) {
            var st = _postfixTokenizer.analyze(expression, true), result = evaluate(st);
            if (st.length !== 0) {
                throw 'RPN expression parsing error. Incorrect nesting.';
            }
            return result;
        };
    }

    function Evaluator() {
        var _infixParser, _postfixParser;

        _infixParser = new InfixParser();
        _postfixParser = new PostfixParser();

        this.compute = function(expression) {
            try {
                var postfixExpression = _infixParser.convert(expression);
                return _postfixParser.evaluate(postfixExpression);
            } catch (e) {
                throw 'Logical expression computation failed. Expression is broken.';
            }
        };
    }

    return {
        Tokenizer: Tokenizer,
        InfixParser: InfixParser,
        PostfixParser: PostfixParser,
        Evaluator: Evaluator,
        Comparer: Comparer,
        typeHelper: typeHelper
    };

}());
