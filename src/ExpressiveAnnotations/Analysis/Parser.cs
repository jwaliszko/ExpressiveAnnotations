/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExpressiveAnnotations.Functions;

namespace ExpressiveAnnotations.Analysis
{
    /* EBNF grammar:
     * 
     * exp         => cond-exp
     * cond-exp    => l-or-exp ["?" exp ":" exp]       // right associative (right recursive)
     * l-or-exp    => l-and-exp  ("||" l-and-exp)*     // left associative (non-recursive alternative to left recursion: [l-or-exp  "||"] l-and-exp)
     * l-and-exp   => b-and-exp ("&&" b-and-exp)*
     * b-or-exp    => xor-exp ("|" xor-exp)*
     * xor-exp     => b-and-exp ("^" b-and-exp)*
     * b-and-exp   => eq-exp ("&" eq-exp)*
     * eq-exp      => rel-exp (("==" | "!=") rel-exp)*
     * rel-exp     => shift-exp ((">" | ">=" | "<" | "<=") shift-exp)*
     * shift-exp   => add-exp (("<<" | ">>") add-exp)*
     * add-exp     => mul-exp (("+" | "-") mul-exp)*
     * mul-exp     => unary-exp (("*" | "/" | "%")  unary-exp)*
     * unary-exp   => ("+" | "-" | "!" | "~") unary-exp | primary-exp
     * primary-exp => null-lit | bool-lit | num-lit | string-lit | arr-access | id-access | "(" exp ")"
     * 
     * arr-access  => arr-lit |                                                                         // [a,b,c]
     *                arr-lit "[" exp "]" ("[" exp "]" | "." identifier)*                               // [a,b,c][d][e].f.g
     * 
     * id-access   => identifier |                                                                      // a
     *                identifier ("[" exp "]" | "." identifier)* |                                      // a[b].c
     *                func-call ("[" exp "]" | "." identifier)*                                         // a(b,c,d)[e].f.g
     *                
     * func-call   => identifier "(" [exp-list] ")"
     * 
     * null-lit    => "null"
     * bool-lit    => "true" | "false"
     * num-lit     => int-lit | float-lit
     * int-lit     => dec-lit | bin-lit | hex-lit
     * array-lit   => "[" [exp-list] "]"
     * 
     * exp-list    => exp ("," exp)*
     */

    /// <summary>
    ///     Performs the syntactic analysis of a specified logical expression within given context.
    /// </summary>
    /// <remarks>
    ///     Type is thread safe.
    /// </remarks>
    public sealed class Parser
    {
        private readonly object _locker = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        public Parser()
        {
            Fields = new Dictionary<string, Expression>();
            Consts = new Dictionary<string, object>();
        }

        private Stack<Token> TokensToProcess { get; set; }
        private Stack<Token> TokensProcessed { get; set; }
        private string ExprString { get; set; }
        private Expr Expr { get; set; }
        private Type ContextType { get; set; }
        private Expression ContextExpression { get; set; }
        private Expression SyntaxTree { get; set; }
        private IDictionary<string, Expression> Fields { get; set; }
        private IDictionary<string, object> Consts { get; set; }
        private IFunctionsProvider FuncProvider { get; set; }
        private IDictionary<string, IList<LambdaExpression>> Functions
            => FuncProvider == null ? new Dictionary<string, IList<LambdaExpression>>() : FuncProvider.GetFunctions();

        /// <summary>
        ///     Parses a specified expression into expression tree within given context.
        /// </summary>
        /// <typeparam name="TContext">The type identifier of the context within which the expression is interpreted.</typeparam>
        /// <typeparam name="TResult">The type identifier of the expected evaluation result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///     A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">expression;Expression not provided.</exception>
        /// <exception cref="ParseErrorException"></exception>
        public Func<TContext, TResult> Parse<TContext, TResult>(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression not provided.");

            lock (_locker)
            {
                try
                {
                    Clear();
                    ContextType = typeof (TContext);
                    var param = Expression.Parameter(typeof (TContext));
                    ContextExpression = param;
                    ExprString = expression;
                    Expr = new Expr(expression);
                    Tokenize();
                    SyntaxTree = ParseExpression();
                    AssertEndOfExpression();
                    var convTree = Expression.Convert(SyntaxTree, typeof (TResult));
                    var lambda = Expression.Lambda<Func<TContext, TResult>>(convTree, param);
                    return lambda.Compile();
                }
                catch (ParseErrorException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ParseErrorException("Parse fatal error.", e);
                }
            }
        }

        /// <summary>
        ///     Parses a specified expression into expression tree within given context.
        /// </summary>
        /// <param name="context">The type instance of the context within which the expression is interpreted.</param>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        ///     A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">expression;Expression not provided.</exception>
        /// <exception cref="ParseErrorException"></exception>
        public Func<object, TResult> Parse<TResult>(Type context, string expression)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "Context not provided.");
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression not provided.");

            lock (_locker)
            {
                try
                {
                    Clear();
                    ContextType = context;
                    var param = Expression.Parameter(typeof (object));
                    ContextExpression = Expression.Convert(param, context);
                    ExprString = expression;
                    Expr = new Expr(expression);
                    Tokenize();
                    SyntaxTree = ParseExpression();
                    AssertEndOfExpression();
                    var convTree = Expression.Convert(SyntaxTree, typeof (TResult));
                    var lambda = Expression.Lambda<Func<object, TResult>>(convTree, param);
                    return lambda.Compile();
                }
                catch (ParseErrorException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ParseErrorException("Parse fatal error.", e);
                }
            }
        }

        /// <summary>
        ///     Parses a specified expression into expression tree within default object context.
        /// </summary>
        /// <typeparam name="TResult">The type identifier of the expected evaluation result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///     A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">expression;Expression not provided.</exception>
        /// <exception cref="ParseErrorException"></exception>
        public Func<TResult> Parse<TResult>(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression not provided.");

            lock (_locker)
            {
                try
                {
                    Clear();
                    ContextType = typeof (object);
                    ContextExpression = Expression.Parameter(typeof (object));
                    ExprString = expression;
                    Expr = new Expr(expression);
                    Tokenize();
                    SyntaxTree = ParseExpression();
                    AssertEndOfExpression();
                    var convTree = Expression.Convert(SyntaxTree, typeof (TResult));
                    var lambda = Expression.Lambda<Func<TResult>>(convTree);
                    return lambda.Compile();
                }
                catch (ParseErrorException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new ParseErrorException("Parse fatal error.", e);
                }
            }
        }

        /// <summary>
        ///     Registers the functions provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public void RegisterFunctionsProvider(IFunctionsProvider provider)
        {
            FuncProvider = provider;
        }

        /// <summary>
        ///     Gets names and types of properties extracted from specified expression within given context.
        /// </summary>
        /// <returns>
        ///     Dictionary containing names and types.
        /// </returns>
        public IDictionary<string, Expression> GetFields()
        {
            return Fields.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        ///     Gets names and values of constants extracted from specified expression within given context.
        /// </summary>
        /// <returns>
        ///     Dictionary containing names and values.
        /// </returns>
        public IDictionary<string, object> GetConsts()
        {
            return Consts.ToDictionary(x => x.Key, x => x.Value); // shallow clone is fair enough
        }

        /// <summary>
        ///     Gets the abstract syntax tree built during parsing.
        /// </summary>
        /// <returns>
        ///     <see cref="Expression" /> that represents the tree.
        /// </returns>
        public Expression GetExpression()
        {
            return SyntaxTree;
        }

        private void Clear()
        {
            Fields.Clear();
            Consts.Clear();
            SyntaxTree = null;
        }

        private void Tokenize()
        {
            var lexer = new Lexer();
            var tokens = lexer.Analyze(ExprString);
            TokensToProcess = new Stack<Token>(tokens.Reverse());
            TokensProcessed = new Stack<Token>();
        }

        private TokenType PeekType()
        {
            return TokensToProcess.Peek().Type;
        }

        private object PeekValue()
        {
            return TokensToProcess.Peek().Value;
        }

        private string PeekRawValue() // to be displayed in error messages, e.g. instead of converted 0.1 gets raw .1
        {
            return TokensToProcess.Peek().RawValue;
        }

        private void ReadToken()
        {
            TokensProcessed.Push(TokensToProcess.Pop());
        }

        private Token PeekToken(int depth = 0)
        {
            Debug.Assert(depth >= 0); // depth can not be negative

            return depth == 0
                ? TokensToProcess.Peek() // for 0 depth take crrent context
                : TokensProcessed.Skip(depth - 1).First(); // otherwise dig through processed tokens
        }

        private Expression ParseExpression()
        {
            return ParseConditionalExpression();
        }

        private Expression ParseConditionalExpression()
        {
            var tok = PeekToken();
            var arg1 = ParseLogicalOrExp();            
            if (PeekType() == TokenType.QMARK)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseExpression();
                if (PeekType() != TokenType.COLON)
                    throw new ParseErrorException(
                        PeekType() == TokenType.EOF
                            ? "Expected colon of ternary operator. Unexpected end of expression."
                            : $"Expected colon of ternary operator. Unexpected token: '{PeekRawValue()}'.",
                        ExprString, PeekToken().Location);
                ReadToken();
                var arg3 = ParseExpression();

                arg1 = Expr.Condition(arg1, arg2, arg3, tok, oper);
            }
            return arg1;
        }

        private Expression ParseLogicalOrExp()
        {
            var arg1 = ParseLogicalAndExp();
            while (PeekType() == TokenType.L_OR)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseLogicalAndExp();

                arg1 = Expr.OrElse(arg1, arg2, oper); // short-circuit evaluation
            }
            return arg1;
        }

        private Expression ParseLogicalAndExp()
        {
            var arg1 = ParseBitwiseOrExp();
            while (PeekType() == TokenType.L_AND)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseBitwiseOrExp();

                arg1 = Expr.AndAlso(arg1, arg2, oper); // short-circuit evaluation
            }
            return arg1;
        }

        private Expression ParseBitwiseOrExp()
        {
            var arg1 = ParseXorExp();
            while (PeekType() == TokenType.B_OR)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseXorExp();

                arg1 = Expr.Or(arg1, arg2, oper); // non-short-circuit evaluation
            }
            return arg1;
        }

        private Expression ParseXorExp()
        {
            var arg1 = ParseBitwiseAndExp();
            while (PeekType() == TokenType.XOR)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseBitwiseAndExp();

                arg1 = Expr.ExclusiveOr(arg1, arg2, oper);
            }
            return arg1;
        }

        private Expression ParseBitwiseAndExp()
        {
            var arg1 = ParseEqualityExp();
            while (PeekType() == TokenType.B_AND)
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseEqualityExp();

                arg1 = Expr.And(arg1, arg2, oper); // non-short-circuit evaluation
            }
            return arg1;
        }

        private Expression ParseEqualityExp()
        {
            var arg1 = ParseRelationalExp();
            while (new[] {TokenType.EQ, TokenType.NEQ}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseRelationalExp();

                switch (oper.Type)
                {
                    case TokenType.EQ:
                        arg1 = Expr.Equal(arg1, arg2, oper);
                        break;
                    default: // assures full branch coverage
                        Debug.Assert(oper.Type == TokenType.NEQ); // http://stackoverflow.com/a/1468385/270315
                        arg1 = Expr.NotEqual(arg1, arg2, oper);
                        break;
                }
            }
            return arg1;
        }

        private Expression ParseRelationalExp()
        {
            var arg1 = ParseShiftExp();
            while (new[] {TokenType.LT, TokenType.LE, TokenType.GT, TokenType.GE}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseShiftExp();

                switch (oper.Type)
                {
                    case TokenType.LT:
                        arg1 = Expr.LessThan(arg1, arg2, oper);
                        break;
                    case TokenType.LE:
                        arg1 = Expr.LessThanOrEqual(arg1, arg2, oper);
                        break;
                    case TokenType.GT:
                        arg1 = Expr.GreaterThan(arg1, arg2, oper);
                        break;
                    default:
                        Debug.Assert(oper.Type == TokenType.GE);
                        arg1 = Expr.GreaterThanOrEqual(arg1, arg2, oper);
                        break;
                }
            }
            return arg1;
        }

        private Expression ParseShiftExp()
        {
            var arg1 = ParseAdditiveExp();
            while (new[] {TokenType.L_SHIFT, TokenType.R_SHIFT}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseAdditiveExp();

                switch (oper.Type)
                {
                    case TokenType.L_SHIFT:
                        arg1 = Expr.LeftShift(arg1, arg2, oper);
                        break;
                    default:
                        Debug.Assert(oper.Type == TokenType.R_SHIFT);
                        arg1 = Expr.RightShift(arg1, arg2, oper);
                        break;
                }
            }
            return arg1;
        }

        private Expression ParseAdditiveExp()
        {
            var arg1 = ParseMultiplicativeExp();
            while (new[] {TokenType.ADD, TokenType.SUB}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseMultiplicativeExp();

                switch (oper.Type)
                {
                    case TokenType.ADD:
                        arg1 = (arg1.Type.IsString() || arg2.Type.IsString())
                            ? Expression.Add(
                                Expression.Convert(arg1, typeof (object)),
                                Expression.Convert(arg2, typeof (object)),
                                typeof (string).GetMethod("Concat", new[] {typeof (object), typeof (object)})) // convert string + string into a call to string.Concat
                            : Expr.Add(arg1, arg2, oper);
                        break;
                    default:
                        Debug.Assert(oper.Type == TokenType.SUB);
                        arg1 = Expr.Subtract(arg1, arg2, oper);
                        break;
                }
            }
            return arg1;
        }

        private Expression ParseMultiplicativeExp()
        {
            var arg1 = ParseUnaryExp();
            while (new[] {TokenType.MUL, TokenType.DIV, TokenType.MOD}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg2 = ParseUnaryExp();

                switch (oper.Type)
                {
                    case TokenType.MUL:
                        arg1 = Expr.Multiply(arg1, arg2, oper);
                        break;
                    case TokenType.DIV:
                        arg1 = Expr.Divide(arg1, arg2, oper);
                        break;
                    default:
                        Debug.Assert(oper.Type == TokenType.MOD);
                        arg1 = Expr.Modulo(arg1, arg2, oper);
                        break;
                }
            }
            return arg1;
        }

        private Expression ParseUnaryExp()
        {
            if (new[] {TokenType.ADD, TokenType.SUB, TokenType.L_NOT, TokenType.B_NOT}.Contains(PeekType()))
            {
                var oper = PeekToken();
                ReadToken();
                var arg = ParseUnaryExp();

                switch (oper.Type)
                {
                    case TokenType.ADD:
                        return Expr.UnaryPlus(arg, oper);
                    case TokenType.SUB:
                        return Expr.Negate(arg, oper);
                    case TokenType.L_NOT:
                        return Expr.Not(arg, oper);
                    default:
                        Debug.Assert(oper.Type == TokenType.B_NOT);
                        return Expr.OnesComplement(arg, oper);
                }
            }
            return ParsePrimaryExp();
        }

        private Expression ParsePrimaryExp()
        {
            switch (PeekType())
            {
                case TokenType.NULL:
                    return ParseNull();
                case TokenType.INT:
                case TokenType.BIN:
                case TokenType.HEX:
                    return ParseInt();
                case TokenType.FLOAT:
                    return ParseFloat();
                case TokenType.BOOL:
                    return ParseBool();
                case TokenType.STRING:
                    return ParseString();
                case TokenType.L_BRACKET:
                    return ParseArrayAccess();
                case TokenType.ID:
                    return ParseIdAccess();
                case TokenType.L_PAR:
                    ReadToken(); // read "("
                    var arg = ParseExpression();
                    if (PeekType() != TokenType.R_PAR)
                        throw new ParseErrorException(
                            PeekType() == TokenType.EOF
                                ? "Expected closing parenthesis. Unexpected end of expression."
                                : $"Expected closing parenthesis. Unexpected token: '{PeekRawValue()}'.",
                            ExprString, PeekToken().Location);
                    ReadToken(); // read ")"
                    return arg;
                case TokenType.EOF:
                    throw new ParseErrorException(
                        "Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected end of expression.", ExprString, PeekToken().Location);                
                default:
                    throw new ParseErrorException(
                        $"Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected token: '{PeekRawValue()}'.", ExprString, PeekToken().Location);
            }
        }

        private Expression ParseNull()
        {
            ReadToken();
            return Expression.Constant(null);
        }

        private Expression ParseInt()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof (int));
        }

        private Expression ParseFloat()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof (double));
        }

        private Expression ParseBool()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof (bool));
        }

        private Expression ParseString()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof (string));
        }

        private Expression ParseArrayAccess()
        {
            var expr = ParseArray();
            if (PeekType() == TokenType.L_BRACKET)
            {
                Token unknownProp;
                expr = ParseAccess(expr, out unknownProp);
                if (unknownProp != null)
                    throw new ParseErrorException(
                        $"Only public properties, constants and enums are accepted. Identifier '{unknownProp.RawValue}' not known.",
                        ExprString, unknownProp.Location);
                Debug.Assert(expr != null);
            }

            return expr;
        }

        private Expression ParseIdAccess()
        {
            var originProp = PeekToken();
            ReadToken(); // read identifier

            Token unknownProp;
            if (PeekType() == TokenType.L_PAR)
            {
                var expr = ParseFuncCall(originProp);
                expr = ParseAccess(expr, out unknownProp);
                if (unknownProp != null)
                    throw new ParseErrorException(
                        $"Only public properties, constants and enums are accepted. Identifier '{unknownProp.RawValue}' not known.",
                        ExprString, unknownProp.Location);
                Debug.Assert(expr != null);
                return expr;
            }
            else
            {
                var expr = ExtractMemberAccessExpression(originProp.RawValue, ContextExpression);
                expr = ParseAccess(expr, out unknownProp);

                var start = originProp.Location.Position(ExprString);
                var length = PeekToken().Location.Position(ExprString) - start;
                var chain = ExprString.Substring(start, length).TrimEnd();

                if (expr != null)
                    Fields[chain] = expr;
                else
                {
                    expr = ExtractConstantAccessExpression(chain, originProp.Location);
                    unknownProp = unknownProp ?? originProp;
                    if (expr == null)
                        throw new ParseErrorException(
                            $"Only public properties, constants and enums are accepted. Identifier '{unknownProp.RawValue}' not known.",
                            ExprString, unknownProp.Location);
                }
                return expr;
            }
        }

        private Expression ParseArray()
        {
            ReadToken(); // read "["
            var args = new List<Expression>();
            while (PeekType() != TokenType.R_BRACKET)
            {
                var arg = ParseExpression();
                if (PeekType() == TokenType.COMMA)
                    ReadToken();
                else if (PeekType() != TokenType.R_BRACKET) // when no comma found, array literal end expected
                    throw new ParseErrorException(
                        PeekType() == TokenType.EOF
                            ? "Expected comma or closing bracket. Unexpected end of expression."
                            : $"Expected comma or closing bracket. Unexpected token: '{PeekRawValue()}'.",
                        ExprString, PeekToken().Location);
                args.Add(arg);
            }
            ReadToken(); // read "]"

            if (!args.Any())
                return Expression.NewArrayInit(typeof (object)); // empty array of objects

            var typesMatch = args.Select(x => x.Type).Distinct().Count() == 1;
            return typesMatch
                ? Expression.NewArrayInit(args[0].Type, args) // items of the same type, array type can be determined
                : Expression.NewArrayInit(typeof (object), args.Select(x => Expression.Convert(x, typeof (object)))); // items of various types, let it be an array of objects
        }

        private Expression ParseFuncCall(Token func)
        {
            var name = func.RawValue;
            ReadToken(); // read "("
            var args = new List<Tuple<Expression, Location>>();
            while (PeekType() != TokenType.R_PAR) // read comma-separated arguments until we hit ")"
            {
                var tkn = PeekToken();
                var arg = ParseExpression();
                if (PeekType() == TokenType.COMMA)
                    ReadToken();
                else if (PeekType() != TokenType.R_PAR) // when no comma found, function exit expected
                    throw new ParseErrorException(
                        PeekType() == TokenType.EOF
                            ? "Expected comma or closing parenthesis. Unexpected end of expression."
                            : $"Expected comma or closing parenthesis. Unexpected token: '{PeekRawValue()}'.",
                        ExprString, PeekToken().Location);
                args.Add(new Tuple<Expression, Location>(arg, tkn.Location));
            }
            ReadToken(); // read ")"

            return ExtractMethodExpression(name, args, func.Location); // get method call
        }
        
        private Expression ParseAccess(Expression expr, out Token unknownProp)
        {
            unknownProp = null; // unknown token
            while (new[] {TokenType.L_BRACKET, TokenType.PERIOD}.Contains(PeekType()))
            {
                var oper = PeekToken();
                switch (oper.Type)
                {
                    case TokenType.PERIOD: // parse member access
                        ReadToken(); // read "."
                        if (PeekType() != TokenType.ID)
                            throw new ParseErrorException(
                                PeekType() == TokenType.EOF
                                    ? "Expected subproperty identifier. Unexpected end of expression."
                                    : $"Expected subproperty identifier. Unexpected token: '{PeekRawValue()}'.",
                                ExprString, PeekToken().Location);
                        var currentProp = PeekToken();

                        if (expr != null)
                        {
                            expr = ExtractMemberAccessExpression(currentProp.RawValue, expr);
                            if (expr == null)
                                unknownProp = currentProp;
                        }

                        ReadToken(); // read property name
                        break;
                    default: // parse subscrit
                        Debug.Assert(PeekType() == TokenType.L_BRACKET);                        
                        ReadToken(); // read "["
                        var idxExprLoc = PeekToken().Location;
                        var idxExpr = ParseExpression();
                        if (idxExpr.Type != typeof (int))
                            throw new ParseErrorException(
                                PeekType() == TokenType.EOF
                                    ? $"Expected index of '{typeof (int)}' type. Unexpected end of expression."
                                    : $"Expected index of '{typeof (int)}' type. Type '{idxExpr.Type}' cannot be implicitly converted.",
                                ExprString, idxExprLoc);
                        if (PeekType() != TokenType.R_BRACKET)
                            throw new ParseErrorException(
                                PeekType() == TokenType.EOF
                                    ? "Expected closing bracket. Unexpected end of expression."
                                    : $"Expected closing bracket. Unexpected token: '{PeekRawValue()}'.",
                                ExprString, PeekToken().Location);

                        if (expr != null)
                        {
                            expr = ExtractMemberSubscritExpression(idxExpr, expr);
                            if (expr == null)
                                throw new ParseErrorException(
                                    "Indexing operation not supported. Subscript operator can be applied to either an array or a type declaring indexer.",
                                    ExprString, oper.Location);
                        }

                        ReadToken(); // read "]"
                        break;
                }
            }
            return expr;
        }

        private Expression ExtractMemberAccessExpression(string name, Expression context)
        {
            return FetchPropertyValue(name, context);
        }

        private Expression ExtractMemberSubscritExpression(Expression index, Expression context)
        {
            return FetchPropertyValue(index, context);
        }

        private Expression ExtractConstantAccessExpression(string name, Location pos)
        {
            return FetchEnumValue(name, pos) ?? FetchConstValue(name, pos);
        }

        private Expression FetchPropertyValue(string name, Expression context)
        {
            var pi = context.Type.GetProperty(name);
            return pi != null ? Expression.Property(context, pi) : null;
        }

        private Expression FetchPropertyValue(Expression index, Expression context)
        {
            if (context.Type.IsArray) // check if we have an array type
                return Expression.ArrayIndex(context, index);

            // not an array - check if the type declares indexer otherwise
            var pi = context.Type.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Any()); // look for indexer property (usually called Item...)
            return pi != null ? Expression.Property(context, pi.Name, index) : null;
        }

        private Expression FetchEnumValue(string name, Location pos)
        {
            var parts = name.Split('.');
            if (parts.Length > 1)
            {
                var enumTypeName = string.Join(".", parts.Take(parts.Length - 1).ToList());
                var enumTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => new AssemblyTypeProvider(a).GetLoadableTypes())
                    .Where(t => t.IsEnum && string.Concat(".", t.FullName.Replace("+", ".")).EndsWith(string.Concat(".", enumTypeName)))
                    .ToList();

                if (enumTypes.Count > 1)
                {
                    var enumList = string.Join($",{Environment.NewLine}", enumTypes.Select(x => $"'{x.FullName}'"));
                    throw new ParseErrorException(
                        $"Enum '{enumTypeName}' is ambiguous, found following:{Environment.NewLine}{enumList}.",
                        ExprString, pos);
                }

                var type = enumTypes.SingleOrDefault();
                if (type != null)
                {
                    var value = Enum.Parse(type, parts.Last());
                    Consts[name] = value;
                    return Expression.Constant(value);
                }
            }
            return null;
        }

        private Expression FetchConstValue(string name, Location pos)
        {
            FieldInfo constant;
            var parts = name.Split('.');
            if (parts.Length > 1)
            {
                var constTypeName = string.Join(".", parts.Take(parts.Length - 1).ToList());
                var constants = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => new AssemblyTypeProvider(a).GetLoadableTypes())
                    .Where(t => string.Concat(".", t.FullName.Replace("+", ".")).EndsWith(string.Concat(".", constTypeName)))
                    .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name.Equals(parts.Last())))
                    .ToList();

                if (constants.Count > 1)
                {
                    var constsList = string.Join(
                        $",{Environment.NewLine}",
                        constants.Select(x => x.ReflectedType != null
                            ? $"'{x.ReflectedType.FullName}.{x.Name}'"
                            : $"'{x.Name}'"));
                    throw new ParseErrorException(
                        $"Constant '{name}' is ambiguous, found following:{Environment.NewLine}{constsList}.",
                        ExprString, pos);
                }

                constant = constants.SingleOrDefault();
            }
            else
            {
                constant = ContextType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .SingleOrDefault(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name.Equals(name));
            }

            if (constant == null)
                return null;

            var value = constant.GetRawConstantValue();
            Consts[name] = (value as string)?.Replace(Environment.NewLine, "\n") ?? value; // in our language new line is represented by \n char (consts map is then sent to JavaScript, and JavaScript new line is also \n)
            return Expression.Constant(value);
        }

        private Expression ExtractMethodExpression(string name, IList<Tuple<Expression, Location>> args, Location funcPos)
        {
            AssertMethodExists(name, funcPos);
            var expression = FetchModelMethod(name, args, funcPos) ?? FetchToolchainMethod(name, args, funcPos); // firstly, try to take method from model context - if not found, take one from toolchain
            if (expression == null)
                throw new ParseErrorException(
                    $"Function '{name}' accepting {args.Count} argument{(args.Count == 1 ? string.Empty : "s")} not found.", ExprString, funcPos);

            return expression;
        }

        private Expression FetchModelMethod(string name, IList<Tuple<Expression, Location>> args, Location funcPos)
        {
            bool variable;
            MethodInfo func;
            var signatures = ContextType.GetMethods()
                .Where(mi => name.Equals(mi.Name) && mi.GetParameters().Length == args.Count).ToList();
            if (signatures.Count == 0)
            {
                func = ContextType.GetMethods().FirstOrDefault(mi => name.Equals(mi.Name));
                if (func == null)
                    return null;
                variable = IsVariableNumOfArgsAccepted(func);
                if (!variable)
                    return null;

                signatures = new List<MethodInfo> {func};
            }
            AssertMethodNotAmbiguous(signatures.Count, name, args.Count, funcPos);

            func = signatures.Single();
            variable = IsVariableNumOfArgsAccepted(func);
            return CreateMethodCallExpression(ContextExpression, args, func, variable);
        }

        private Expression FetchToolchainMethod(string name, IList<Tuple<Expression, Location>> args, Location funcPos)
        {
            bool variable;
            LambdaExpression func;
            var signatures = Functions.ContainsKey(name)
                ? Functions[name].Where(f => f.Parameters.Count == args.Count).ToList()
                : new List<LambdaExpression>();
            if (signatures.Count == 0)
            {
                func = Functions.ContainsKey(name) ? Functions[name].FirstOrDefault() : null;
                if (func == null)
                    return null;
                variable = IsVariableNumOfArgsAccepted(func);
                if (!variable)
                    return null;

                signatures = new List<LambdaExpression> {func};
            }
            AssertMethodNotAmbiguous(signatures.Count, name, args.Count, funcPos);

            func = signatures.Single();
            variable = IsVariableNumOfArgsAccepted(func);
            return CreateInvocationExpression(func, args, name, variable);
        }

        private bool IsVariableNumOfArgsAccepted(LambdaExpression func)
        {
            Debug.Assert(func != null);

            var arg = func.GetType().GetGenericArguments().FirstOrDefault();
            var method = arg?.GetMethods().FirstOrDefault();
            var parameter = method?.GetParameters().FirstOrDefault();
            var indicator = parameter?.GetCustomAttributes(typeof (ParamArrayAttribute), false).Any();
            return indicator != null && indicator.Value;
        }

        private bool IsVariableNumOfArgsAccepted(MethodInfo func)
        {
            Debug.Assert(func != null);

            var parameter = func.GetParameters().FirstOrDefault();
            var indicator = parameter?.GetCustomAttributes(typeof (ParamArrayAttribute), false).Any();
            return indicator != null && indicator.Value;
        }

        private InvocationExpression CreateInvocationExpression(LambdaExpression funcExpr, IList<Tuple<Expression, Location>> parsedArgs, string funcName, bool variable)
        {
            var convertedArgs = new List<Expression>();
            ParameterExpression param;
            if (!variable)
            {
                Debug.Assert(funcExpr.Parameters.Count == parsedArgs.Count);
                for (var i = 0; i < parsedArgs.Count; i++)
                {
                    var arg = parsedArgs[i].Item1;
                    var pos = parsedArgs[i].Item2;
                    param = funcExpr.Parameters[i];
                    convertedArgs.Add(arg.Type == param.Type
                        ? arg
                        : ConvertArgument(arg, param.Type, funcName, i + 1, pos));
                }
                return Expression.Invoke(funcExpr, convertedArgs);
            }

            param = funcExpr.Parameters.Single();
            var paramElemType = param.Type.GetElementType();
            var argsArray = Expression.NewArrayInit(paramElemType, convertedArgs);

            if (parsedArgs.Count == 0)
                return Expression.Invoke(funcExpr, argsArray);
            
            if (parsedArgs.Count == 1)
            {
                var bag = parsedArgs.Single();
                var arg = bag.Item1;
                var pos = bag.Item2;
                if (arg.Type.IsArray)
                {
                    convertedArgs.Add(arg.Type == param.Type
                        ? arg
                        : ConvertArgument(arg, param.Type, funcName, 1, pos));
                    return Expression.Invoke(funcExpr, convertedArgs);
                }
            }
            
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i].Item1;
                var pos = parsedArgs[i].Item2;
                convertedArgs.Add(arg.Type == paramElemType
                    ? arg
                    : ConvertArgument(arg, paramElemType, funcName, i + 1, pos));
            }
            argsArray = Expression.NewArrayInit(paramElemType, convertedArgs);
            return Expression.Invoke(funcExpr, argsArray);
        }

        private MethodCallExpression CreateMethodCallExpression(Expression ctxExpr, IList<Tuple<Expression, Location>> parsedArgs, MethodInfo methodInfo, bool variable)
        {
            var parameters = methodInfo.GetParameters();
            var convertedArgs = new List<Expression>();
            ParameterInfo param;
            if (!variable)
            {
                Debug.Assert(parameters.Length == parsedArgs.Count);                
                for (var i = 0; i < parsedArgs.Count; i++)
                {
                    var arg = parsedArgs[i].Item1;
                    var pos = parsedArgs[i].Item2;
                    param = parameters[i];
                    convertedArgs.Add(arg.Type == param.ParameterType
                        ? arg
                        : ConvertArgument(arg, param.ParameterType, methodInfo.Name, i + 1, pos));
                }
                return Expression.Call(ctxExpr, methodInfo, convertedArgs);
            }

            param = parameters.Single();
            var paramElemType = param.ParameterType.GetElementType();
            var argsArray = Expression.NewArrayInit(paramElemType, convertedArgs);

            if (parsedArgs.Count == 0)
                return Expression.Call(ctxExpr, methodInfo, argsArray);
            
            if (parsedArgs.Count == 1)
            {
                var bag = parsedArgs.Single();
                var arg = bag.Item1;
                var pos = bag.Item2;
                if (arg.Type.IsArray)
                {
                    convertedArgs.Add(arg.Type == param.ParameterType
                        ? arg
                        : ConvertArgument(arg, param.ParameterType, methodInfo.Name, 1, pos));
                    return Expression.Call(ctxExpr, methodInfo, convertedArgs);
                }
            }
            
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i].Item1;
                var pos = parsedArgs[i].Item2;
                convertedArgs.Add(arg.Type == paramElemType
                    ? arg
                    : ConvertArgument(arg, paramElemType, methodInfo.Name, i + 1, pos));
            }
            argsArray = Expression.NewArrayInit(paramElemType, convertedArgs);
            return Expression.Call(ctxExpr, methodInfo, argsArray);
        }        

        private Expression ConvertArgument(Expression arg, Type type, string funcName, int argIdx, Location argPos)
        {
            try
            {
                return Expression.Convert(arg, type);
            }
            catch
            {
                throw new ParseErrorException(
                    $"Function '{funcName}' {argIdx.ToOrdinal()} argument implicit conversion from '{arg.Type}' to expected '{type}' failed.",
                    ExprString, argPos);
            }
        }

        private void AssertMethodExists(string name, Location pos)
        {
            if (!Functions.ContainsKey(name) && !ContextType.GetMethods().Any(mi => name.Equals(mi.Name)))
                throw new ParseErrorException(
                    $"Function '{name}' not known.", ExprString, pos);
        }

        private void AssertMethodNotAmbiguous(int signatures, string name, int args, Location pos)
        {
            if (signatures > 1)
                throw new ParseErrorException(
                    $"Function '{name}' accepting {args} argument{(args == 1 ? string.Empty : "s")} is ambiguous.", 
                    ExprString, pos);
        }

        private void AssertEndOfExpression()
        {
            if (PeekType() != TokenType.EOF)
                throw new ParseErrorException(
                    $"Unexpected token: '{PeekRawValue()}'.", ExprString, PeekToken().Location);
        }
    }
}
