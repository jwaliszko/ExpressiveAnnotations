/* expressive.annotations.analysis.js - v1.0.0
 * script responsible for logical expressions parsing and computation 
 * e.g. suppose there is "(true) && (!false)" expression given, and we need to know its final logical value:
 *     var evaluator = new Evaluator();
 *     var result = evaluator.compute("(true) && (!false)")
 * this script is a part of client side component of ExpresiveAnnotations - annotation-based conditional validation library
 * (c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
 * license: http://www.opensource.org/licenses/mit-license.php */

var BooleanExpressionsAnalyser = (function() {

    var Utils = {
        Array: {
            sanatize: function(arr, item) {
                for (var i = arr.length; i--;) {
                    if (arr[i] === item)
                        arr.splice(i, 1);
                }
            },
            contains: function(arr, item) {
                var i = arr.length;
                while (i--) {
                    if (arr[i] === item)
                        return true;
                }
                return false;
            }
        },
        String: {
            format: function(text, params) {
                if (typeof params === 'string') {
                    text = text.replace('{0}', params);
                    return text;
                }
                var length = params.length;
                for (var i = 0; i < length; i++) {
                    text = text.replace('{' + i + '}', params[i]);
                }
                return text;
            }
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
        var _regex = new RegExp(Utils.String.format('^{0}', regex));

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
                Utils.Array.sanatize(tokens, Token.SPACE);
            return tokens;
        };

        var next = function() {
            if (_expression == '')
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

            throw Utils.String.format('Lexer error. Unexpected token started at "{0}".', _expression);
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
            return Utils.Array.contains(op, token);
        };

        var isPostfixOperator = function(token) {
            var op = [Token.AND, Token.OR, Token.NOT];
            return Utils.Array.contains(op, token);
        };

        var isUnaryOperator = function(token) {
            var op = [Token.NOT];
            return Utils.Array.contains(op, token);
        };

        var isLeftBracket = function(token) {
            return Token.LEFT == token;
        };

        var isRightBracket = function(token) {
            return Token.RIGHT == token;
        };

        var containsLeftBracket = function(st) {
            return Utils.Array.contains(st, Token.LEFT);
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
                var top = operators[operators.length - 1]; //peek
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

            if ('true' == top || 'false' == top)
                return top == 'true';

            var y = evaluate(st);
            if (top == Token.NOT)
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
                throw Utils.String.format('RPN expression parsing error. Token "{0}" not expected.', top);
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
        Utils: Utils
    };

})();
