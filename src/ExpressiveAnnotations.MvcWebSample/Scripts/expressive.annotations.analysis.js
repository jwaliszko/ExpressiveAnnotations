/* expressive.annotations.analysis.js - v1.2.1
 * script responsible for logical expressions parsing and computation 
 * e.g. suppose there is "true && !false" expression given, and we need to know its final logical value:
 *     var evaluator = new Evaluator();
 *     var result = evaluator.compute("true && !false")
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * copyright (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * licensed MIT: http://www.opensource.org/licenses/mit-license.php */

var LogicalExpressionAnalyser = (function() {

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
            compareOrdinal: function(strA, strB) {
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
                function isNumber (n) {
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
                return { error: true, msg: 'Parsing error. Given value is not a string representing an RFC2822 or ISO 8601 date.' }
            }
        },
        
        isEmpty: function (value) {
            return value === null || value === '' || typeof value === 'undefined' || !/\S/.test(value);
        },
        isNumeric: function (value) {
            return typeof value === 'number';
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
        this.compute = function (dependentValue, targetValue, relationalOperator) {
            switch (relationalOperator) {
                case '==':
                    return compare(dependentValue, targetValue);
                case '!=':
                    return !compare(dependentValue, targetValue);
                case '>':
                    return greater(dependentValue, targetValue);
                case '>=':
                    return !less(dependentValue, targetValue);
                case '<':
                    return less(dependentValue, targetValue);
                case '<=':
                    return !greater(dependentValue, targetValue);
            }

            throw TypeHelper.String.format('Relational operator {0} is invalid. Available operators: ==, !=, >, >=, <, <=.', relationalOperator);
        };

        var compare = function (dependentValue, targetValue) {
            return (dependentValue === targetValue)
                || (TypeHelper.isString(dependentValue) && TypeHelper.isString(targetValue)
                    && TypeHelper.String.compareOrdinal(dependentValue, targetValue) === 0)
                || (!TypeHelper.isEmpty(dependentValue) && targetValue === '*')
                || (TypeHelper.isEmpty(dependentValue) && TypeHelper.isEmpty(targetValue));
        };
        var greater = function (dependentValue, targetValue) {
            if (TypeHelper.isNumeric(dependentValue) && TypeHelper.isNumeric(targetValue))
                return dependentValue > targetValue;
            if (TypeHelper.isDate(dependentValue) && TypeHelper.isDate(targetValue))
                return dependentValue > targetValue;
            if (TypeHelper.isString(dependentValue) && TypeHelper.isString(targetValue))
                return TypeHelper.String.compareOrdinal(dependentValue, targetValue) > 0;
            if (TypeHelper.isEmpty(dependentValue) || TypeHelper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        };
        var less = function (dependentValue, targetValue) {
            if (TypeHelper.isNumber(dependentValue) && TypeHelper.isNumber(targetValue))
                return dependentValue < targetValue;
            if (TypeHelper.isDate(dependentValue) && TypeHelper.isDate(targetValue))
                return dependentValue < targetValue;
            if (TypeHelper.isString(dependentValue) && TypeHelper.isString(targetValue))
                return TypeHelper.String.compareOrdinal(dependentValue, targetValue) < 0;
            if (TypeHelper.isEmpty(dependentValue) || TypeHelper.isEmpty(targetValue))
                return false;

            throw 'Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.';
        }
    };

    var Token = {
        TRUE: 'true',
        FALSE: 'false',
        AND: '&&',
        OR: '||',
        NOT: '!',
        LEFT: '(',
        RIGHT: ')',
        SPACE: ' '
    };

    function RegexMatcher(regex) {
        var _regex = new RegExp(TypeHelper.String.format('^{0}', regex));

        this.match = function(text) {
            var success = _regex.test(text);
            if (success) {
                var str = _regex.exec(text).toString();
                return str.length;
            }
            return 0;
        };
    };

    function TokenDefinition(regex, token) {
        this.matcher = new RegexMatcher(regex);
        this.token = token;
    };

    function Lexer(tokenDefinitions) {
        var _tokenDefinitions = tokenDefinitions;
        var _token;
        var _expression;

        this.analyze = function(expression, removeEmptyTokens) {
            _expression = expression;
            var tokens = new Array();
            while (next()) {
                tokens.push(_token);
            }
            if (removeEmptyTokens)
                TypeHelper.Array.sanatize(tokens, Token.SPACE);
            return tokens;
        };

        var next = function() {
            if (_expression === '')
                return false;

            var length = _tokenDefinitions.length;
            for (var i = 0; i < length; i++) {
                var def = _tokenDefinitions[i];
                var matched = def.matcher.match(_expression);
                if (matched > 0) {
                    _token = def.token;
                    _expression = _expression.substr(matched);
                    return true;
                }
            }

            throw TypeHelper.String.format('Lexer error. Unexpected token started at "{0}".', _expression);
        };
    }

    function InfixLexer() {
        var _lexer = new Lexer([
            new TokenDefinition('true', Token.TRUE),
            new TokenDefinition('false', Token.FALSE),
            new TokenDefinition('&&', Token.AND),
            new TokenDefinition('\\|\\|', Token.OR),
            new TokenDefinition('\\!', Token.NOT),
            new TokenDefinition('\\(', Token.LEFT),
            new TokenDefinition('\\)', Token.RIGHT),
            new TokenDefinition('\\s', Token.SPACE)
        ]);

        this.analyze = function(expression, removeEmptyTokens) {
            return _lexer.analyze(expression, removeEmptyTokens);
        };
    }

    function PostfixLexer() {
        var _lexer = new Lexer([
            new TokenDefinition('true', Token.TRUE),
            new TokenDefinition('false', Token.FALSE),
            new TokenDefinition('&&', Token.AND),
            new TokenDefinition('\\|\\|', Token.OR),
            new TokenDefinition('\\!', Token.NOT),
            new TokenDefinition('\\s', Token.SPACE)
        ]);

        this.analyze = function(expression, removeEmptyTokens) {
            return _lexer.analyze(expression, removeEmptyTokens);
        };
    }

    function InfixToPostfixConverter() {
        var _infixLexer = new InfixLexer();

        var isInfixOperator = function(token) {
            var op = [Token.AND, Token.OR, Token.NOT, Token.LEFT, Token.RIGHT];
            return TypeHelper.Array.contains(op, token);
        };
        var isPostfixOperator = function(token) {
            var op = [Token.AND, Token.OR, Token.NOT];
            return TypeHelper.Array.contains(op, token);
        };
        var isUnaryOperator = function(token) {
            var op = [Token.NOT];
            return TypeHelper.Array.contains(op, token);
        };
        var isLeftBracket = function(token) {
            return Token.LEFT === token;
        };
        var isRightBracket = function(token) {
            return Token.RIGHT === token;
        };
        var containsLeftBracket = function(st) {
            return TypeHelper.Array.contains(st, Token.LEFT);
        };

        this.convert = function(expression) {
            var tokens = _infixLexer.analyze(expression, true);
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
        var _postfixLexer = new PostfixLexer();

        this.evaluate = function(expression) {
            var st = _postfixLexer.analyze(expression, true);
            var result = evaluate(st);
            if (st.length != 0)
                throw 'RPN expression parsing error. Incorrect nesting.';
            return result;
        };

        var evaluate = function(st) {
            var top = st.pop();

            if ('true' === top || 'false' === top)
                return top === 'true';

            var y = evaluate(st);
            if (top === Token.NOT)
                return !y;

            var x = evaluate(st);

            switch (top) {
            case Token.AND:
                x &= y;
                break;
            case Token.OR:
                x |= y;
                break;
            default:
                throw TypeHelper.String.format('RPN expression parsing error. Token "{0}" not expected.', top);
            }
            return x;
        };
    }

    function Evaluator() {
        var _converter = new InfixToPostfixConverter();
        var _parser = new PostfixParser();

        this.compute = function(expression) {
            return _parser.evaluate(_converter.convert(expression));
        };
    }

    return {
        InfixLexer: InfixLexer,
        PostfixLexer: PostfixLexer,
        InfixToPostfixConverter: InfixToPostfixConverter,
        PostfixParser: PostfixParser,
        Evaluator: Evaluator,
        TypeHelper: TypeHelper,
        Comparer: Comparer
    };

})();
