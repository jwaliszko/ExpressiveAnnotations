/* expressive.annotations.analysis.js - v1.4.2
 * script responsible for logical expressions parsing and computation 
 * e.g. suppose there is "true && !false" expression given, and we need to know its final logical value:
 *     var evaluator = new Evaluator();
 *     var result = evaluator.compute("true && !false")
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

(function(window) {
    'use strict';
var
    typeHelper = {
        array: {
            contains: function(arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item) {
                        return true;
                    }
                }
                return false;
            },
            sanatize: function(arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item) {
                        arr.splice(i, 1);
                    }
                }
            }
        },
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

    expressionsAnalyser = (function(helper) { // module pattern
        function Comparer() {
            function equal(dependentValue, targetValue, sensitiveComparisons) {
                if (!(dependentValue === undefined || dependentValue === null || dependentValue === '' || !/\S/.test(dependentValue)) && targetValue === '*') { // wildcard target doesn't allow null, empty or whitespace strings
                    return true;
                }
                var date = helper.date.tryParse(targetValue); // parsing here? - it is an exception when incompatible types are allowed, because date targets can be provided as strings
                if (helper.isDate(dependentValue) && !date.error) {
                    return dependentValue.getTime() === date.getTime();
                }
                return sensitiveComparisons
                    ? JSON.stringify(dependentValue) === JSON.stringify(targetValue)
                    : JSON.stringify(dependentValue).toLowerCase() === JSON.stringify(targetValue).toLowerCase();
            }
            function greater(dependentValue, targetValue) {
                if (helper.isNumeric(dependentValue) && helper.isNumeric(targetValue)) {
                    return dependentValue > targetValue;
                }
                if (helper.isDate(dependentValue) && helper.isDate(targetValue)) {
                    return dependentValue > targetValue;
                }
                if (helper.isString(dependentValue) && helper.isString(targetValue)) {
                    return helper.string.compareOrdinal(dependentValue, targetValue) > 0;
                }
                var date = helper.date.tryParse(targetValue);
                if (helper.isDate(dependentValue) && !date.error) {
                    return dependentValue > date;
                }
                if (dependentValue === undefined || dependentValue === null || dependentValue === '' || targetValue === undefined || targetValue === null || targetValue === '') {
                    return false;
                }

                throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
            }
            function less(dependentValue, targetValue) {
                if (helper.isNumeric(dependentValue) && helper.isNumeric(targetValue)) {
                    return dependentValue < targetValue;
                }
                if (helper.isDate(dependentValue) && helper.isDate(targetValue)) {
                    return dependentValue < targetValue;
                }
                if (helper.isString(dependentValue) && helper.isString(targetValue)) {
                    return helper.string.compareOrdinal(dependentValue, targetValue) < 0;
                }
                var date = helper.date.tryParse(targetValue);
                if (helper.isDate(dependentValue) && !date.error) {
                    return dependentValue < date;
                }
                if (dependentValue === undefined || dependentValue === null || dependentValue === '' || targetValue === undefined || targetValue === null || targetValue === '') {
                    return false;
                }

                throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
            }

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

                throw helper.string.format('Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.', relationalOperator);
            };
        }

        function Tokenizer(patterns) { // constructor (constructor functions that must be used with the new prefix should start with a capital letter - JavaScript issues neither a compile-time nor a run-time warning if a required new keyword is omitted which can be disastrous, so the capitalization convention is really helpful)
            var expr, token;
        
            function next() { // private function, shorthand  for var next = function next(...) {...};
                var i, regex, value; // all variables defined at the top of the function - JavaScript does not have block scope (only functions have scope), so defining variables in blocks can confuse programmers who are experienced with other C family languages

                expr = expr.trim();
                if (expr === null || expr === '') {
                    return false;
                }
            
                for (i = 0; i < patterns.length; i++) {
                    regex = new RegExp(helper.string.format('^{0}', patterns[i]));
                    value = regex.exec(expr);
                    if (value !== null && value !== undefined) {
                        token = value.toString();
                        expr = expr.substr(token.length);
                        return true;
                    }
                }

                throw helper.string.format('Tokenizer error. Unexpected token started at {0}.', expr);
            }

            this.analyze = function(expression) { // privileged method, able to access the private variables and methods, is itself accessible to the public methods and the outside
                var tokens = [];
                if (expression === null || expression === '') {
                    return tokens;
                }

                expr = expression;
                while (next()) {
                    tokens.push(token);
                }
                return tokens;
            };
        }

        function InfixParser() {
            var infixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!', '\\(', '\\)']);

            function isInfixOperator(token) {
                return helper.array.contains(['&&', '||', '!', '(', ')'], token);
            }
            function isPostfixOperator(token) {
                return helper.array.contains(['&&', '||', '!'], token);
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
                return helper.array.contains(st, '(');
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

            this.convert = function(expression) {
                var tokens = infixTokenizer.analyze(expression),
                    length = tokens.length,
                    operators = [],
                    output = [],
                    i, token;

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
            var postfixTokenizer = new Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!']);

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
                        throw helper.string.format('RPN expression parsing error. Token {0} not expected.', top);
                }

                return x;
            }

            this.evaluate = function(expression) {
                var st = postfixTokenizer.analyze(expression, true), result = evaluate(st);
                if (st.length !== 0) {
                    throw 'RPN expression parsing error. Incorrect nesting.';
                }
                return result;
            };
        }

        function Evaluator() {
            var infixParser = new InfixParser(),
                postfixParser = new PostfixParser();

            this.compute = function(expression) {
                try {
                    var postfixExpression = infixParser.convert(expression);
                    return postfixParser.evaluate(postfixExpression);
                } catch (e) {
                    throw 'Logical expression computation failed. Expression is broken.';
                }
            };
        }

        return { // expose some private members for the external usage
            Tokenizer: Tokenizer,
            InfixParser: InfixParser,
            PostfixParser: PostfixParser,
            Evaluator: Evaluator,
            Comparer: Comparer
        };
    }(typeHelper)); // execute and return (when a function is invoked immediately, the entire invocation expression should be wrapped in brackets - it is clear then, that the value being produced is the result of the function invocation and not the function itself)

    // expose ea to the global object (current external scope is not really required, module or object literal patterns would be enough to prevent global namespace pollution, but it is always fun to play with javascript)
    window.ea = {
        analyser: expressionsAnalyser,
        helper: typeHelper
    };

}(window)); // immediately execute this anonymous function, passing a reference to the global scope
