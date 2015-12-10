/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpressiveAnnotations.Analysis
{
    /* EBNF GRAMMAR:
     * 
     * expression => or-exp
     * or-exp     => and-exp [ "||" or-exp ]
     * and-exp    => rel-exp [ "&&" and-exp ]
     * rel-exp    => not-exp [ rel-op not-exp ]
     * not-exp    => add-exp | "!" not-exp
     * add-exp    => mul-exp add-exp'
     * add-exp'   => "+" add-exp | "-" add-exp
     * mul-exp    => val mul-exp'
     * mul-exp'   => "*" mul-exp | "/" mul-exp
     * rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="
     * val        => "null" | int | float | bool | string | func | "(" or-exp ")"
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
            Fields = new Dictionary<string, Type>();
            Consts = new Dictionary<string, object>();
            Functions = new Dictionary<string, IList<LambdaExpression>>();
        }

        private Stack<Token> TokensToProcess { get; set; }
        private Stack<Token> TokensProcessed { get; set; }
        private Type ContextType { get; set; }
        private Expression ContextExpression { get; set; }
        private IDictionary<string, Type> Fields { get; set; }
        private IDictionary<string, object> Consts { get; set; }
        private IDictionary<string, IList<LambdaExpression>> Functions { get; set; }

        /// <summary>
        ///     Parses a specified logical expression into expression tree within given context.
        /// </summary>
        /// <typeparam name="TContext">The type identifier of the context within which the expression is interpreted.</typeparam>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        ///     A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Func<TContext, bool> Parse<TContext>(string expression)
        {
            lock (_locker)
            {
                try
                {
                    Clear();
                    ContextType = typeof (TContext);
                    var param = Expression.Parameter(typeof (TContext));
                    ContextExpression = param;
                    Tokenize(expression);
                    var expressionTree = ParseExpression();
                    var lambda = Expression.Lambda<Func<TContext, bool>>(expressionTree, param);
                    return lambda.Compile();
                }
                catch (ParseErrorException e)
                {
                    throw new InvalidOperationException(BuildParseError(e, expression), e);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Parse fatal error. {e.Message}", e);
                }
            }
        }

        /// <summary>
        ///     Parses a specified logical expression into expression tree within given context.
        /// </summary>
        /// <param name="context">The type instance of the context within which the expression is interpreted.</param>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        ///     A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Func<object, bool> Parse(Type context, string expression)
        {
            lock (_locker)
            {
                try
                {
                    Clear();
                    ContextType = context;
                    var param = Expression.Parameter(typeof (object));
                    ContextExpression = Expression.Convert(param, context);
                    Tokenize(expression);
                    var expressionTree = ParseExpression();
                    var lambda = Expression.Lambda<Func<object, bool>>(expressionTree, param);
                    return lambda.Compile();
                }
                catch (ParseErrorException e)
                {
                    throw new InvalidOperationException(BuildParseError(e, expression));
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Parse fatal error. {e.Message}", e);
                }
            }
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TResult>(string name, Expression<Func<TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TResult>(string name, Expression<Func<TArg1, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TResult>(string name, Expression<Func<TArg1, TArg2, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TArg5">Fifth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TArg4">Fourth argument.</typeparam>
        /// <typeparam name="TArg5">Fifth argument.</typeparam>
        /// <typeparam name="TArg6">Sixth argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> func)
        {
            PersistFunction(name, func);
        }

        /// <summary>
        ///     Gets names and types of properties extracted from specified expression within given context.
        /// </summary>
        /// <returns>
        ///     Dictionary containing names and types.
        /// </returns>
        public IDictionary<string, Type> GetFields()
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

        private void PersistFunction(string name, LambdaExpression func)
        {
            lock (_locker)
            {
                if (!Functions.ContainsKey(name))
                    Functions[name] = new List<LambdaExpression>();
                Functions[name].Add(func);
            }
        }

        private void Clear()
        {
            Fields.Clear();
            Consts.Clear();
        }

        private void Tokenize(string expression)
        {
            var lexer = new Lexer();
            var tokens = lexer.Analyze(expression);
            TokensToProcess = new Stack<Token>(tokens.Reverse());
            TokensProcessed = new Stack<Token>();
        }

        private string BuildParseError(ParseErrorException e, string expression)
        {
            var pos = e.Location;
            return pos == null
                ? $"Parse error: {e.Message}"
                : $"Parse error on line {pos.Line}, column {pos.Column}:{expression.TakeLine(pos.Line - 1).Substring(pos.Column - 1).Indicator()}{e.Message}";
        }

        private TokenType PeekType()
        {
            return TokensToProcess.Peek().Type;
        }

        private object PeekValue()
        {
            return TokensToProcess.Peek().Value;
        }

        private void ReadToken()
        {
            TokensProcessed.Push(TokensToProcess.Pop());
        }

        private Token PeekToken(int depth = 0)
        {
            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), "Depth can not be negative, surprisingly.");

            return depth == 0
                ? TokensToProcess.Peek() // for 0 depth take crrent context
                : TokensProcessed.Skip(depth - 1).First(); // otherwise dig through processed tokens
        }

        private Expression ParseExpression()
        {
            var expr = ParseOrExp();
            if (PeekType() != TokenType.EOF)
                throw new ParseErrorException(
                    $"Unexpected token: '{PeekValue()}'.",
                    PeekToken().Location);
            return expr;
        }

        private Expression ParseOrExp()
        {
            var arg1 = ParseAndExp();
            if (PeekType() != TokenType.OR)
                return arg1;
            var oper = PeekToken();
            ReadToken();
            var arg2 = ParseOrExp();

            AssertArgsNotNullLiterals(arg1, arg2, oper);
            if (!arg1.Type.IsBool() || !arg2.Type.IsBool())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.",
                    oper.Location);

            return Expression.OrElse(arg1, arg2); // short-circuit evaluation
        }

        private Expression ParseAndExp()
        {
            var arg1 = ParseRelExp();
            if (PeekType() != TokenType.AND)
                return arg1;
            var oper = PeekToken();
            ReadToken();
            var arg2 = ParseAndExp();

            AssertArgsNotNullLiterals(arg1, arg2, oper);
            if (!arg1.Type.IsBool() || !arg2.Type.IsBool())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.",
                    oper.Location);

            return Expression.AndAlso(arg1, arg2); // short-circuit evaluation
        }

        private Expression ParseRelExp()
        {
            var arg1 = ParseNotExp();
            if (!new[] {TokenType.LT, TokenType.LE, TokenType.GT, TokenType.GE, TokenType.EQ, TokenType.NEQ}.Contains(PeekType()))
                return arg1;
            var oper = PeekToken();
            ReadToken();
            var arg2 = ParseNotExp();

            var type1 = arg1.Type;
            var type2 = arg2.Type;
            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);

            if (oper.Type == TokenType.EQ || oper.Type == TokenType.NEQ)
            {
                if (type1.IsNonNullableValueType() && arg2.IsNullLiteral())
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and 'null'.",
                        oper.Location);
                if (arg1.IsNullLiteral() && type2.IsNonNullableValueType())
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and '{type2}'.",
                        oper.Location);

                if (arg1.Type != arg2.Type
                    && !arg1.IsNullLiteral() && !arg2.IsNullLiteral()
                    && !arg1.Type.IsObject() && !arg2.Type.IsObject())
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.",
                        oper.Location);
            }
            else
            {
                AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
                if (!(arg1.Type.IsNumeric() && arg2.Type.IsNumeric()) 
                    && !(arg1.Type.IsDateTime() && arg2.Type.IsDateTime()) 
                    && !(arg1.Type.IsTimeSpan() && arg2.Type.IsTimeSpan()))
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.",
                        oper.Location);                
            }

            switch (oper.Type)
            {
                case TokenType.LT:
                    return Expression.LessThan(arg1, arg2);
                case TokenType.LE:
                    return Expression.LessThanOrEqual(arg1, arg2);
                case TokenType.GT:
                    return Expression.GreaterThan(arg1, arg2);
                case TokenType.GE:
                    return Expression.GreaterThanOrEqual(arg1, arg2);
                case TokenType.EQ:
                    return Expression.Equal(arg1, arg2);
                case TokenType.NEQ:
                    return Expression.NotEqual(arg1, arg2);
                default:
                    throw new ArgumentOutOfRangeException(); // never gets here
            }
        }

        private Expression ParseNotExp()
        {
            if (PeekType() != TokenType.NOT)
                return ParseAddExp();
            var oper = PeekToken();
            ReadToken();
            var arg = ParseNotExp(); // allow multiple negations

            if (arg.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type 'null'.",
                    oper.Location);            
            if (!arg.Type.IsBool())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operand of type '{arg.Type}'.",
                    oper.Location);

            return Expression.Not(arg);
        }

        private Expression ParseAddExp()
        {
            var sign = UnifySign();
            var arg = ParseMulExp();

            if (sign == TokenType.SUB)
                arg = InverseNumber(arg);

            return ParseAddExpInternal(arg);
        }

        private Expression ParseAddExpInternal(Expression arg1)
        {
            if (!new[] {TokenType.ADD, TokenType.SUB}.Contains(PeekType()))
                return arg1;
            var oper = PeekToken();
            ReadToken();
            var sign = UnifySign();
            var arg2 = ParseMulExp();

            if (sign == TokenType.SUB)
                arg2 = InverseNumber(arg2);

            var type1 = arg1.Type;
            var type2 = arg2.Type;
            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            
            if (!arg1.Type.IsString() && !arg2.Type.IsString())
            {
                AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
                if (!(arg1.Type.IsNumeric() && arg2.Type.IsNumeric()) 
                    && !(arg1.Type.IsDateTime() && arg2.Type.IsDateTime()) 
                    && !(arg1.Type.IsTimeSpan() && arg2.Type.IsTimeSpan()) 
                    && !(arg1.Type.IsDateTime() && arg2.Type.IsTimeSpan()))
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.",
                        oper.Location);
            }

            if (oper.Type == TokenType.ADD)
                if (arg1.Type.IsDateTime() && arg2.Type.IsDateTime())
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.",
                        oper.Location);
            if (oper.Type == TokenType.SUB)
            {
                AssertArgsNotNullLiterals(arg1, type1, arg2, type2, oper);
                if (arg1.Type.IsString() && arg2.Type.IsString())
                    throw new ParseErrorException(
                        $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and '{type2}'.",
                        oper.Location);
            }

            switch (oper.Type)
            {
                case TokenType.ADD:
                    return ParseAddExpInternal(
                        (arg1.Type.IsString() || arg2.Type.IsString())
                            ? Expression.Add(
                                Expression.Convert(arg1, typeof (object)),
                                Expression.Convert(arg2, typeof (object)),
                                typeof (string).GetMethod("Concat", new[] {typeof (object), typeof (object)})) // convert string + string into a call to string.Concat
                            : Expression.Add(arg1, arg2));
                case TokenType.SUB:
                    return ParseAddExpInternal(Expression.Subtract(arg1, arg2));
                default:
                    throw new ArgumentOutOfRangeException(); // never gets here
            }
        }

        private Expression ParseMulExp()
        {
            var sgn = UnifySign();
            var arg = ParseVal();

            if (sgn == TokenType.SUB)
                arg = InverseNumber(arg);

            return ParseMulExpInternal(arg);
        }

        private Expression ParseMulExpInternal(Expression arg1)
        {
            if (!new[] {TokenType.MUL, TokenType.DIV}.Contains(PeekType()))
                return arg1;
            var oper = PeekToken();
            ReadToken();
            var sign = UnifySign();
            var arg2 = ParseVal();

            if (sign == TokenType.SUB)
                arg2 = InverseNumber(arg2);

            if (!arg1.Type.IsNumeric() || !arg2.Type.IsNumeric())
            {
                AssertArgsNotNullLiterals(arg1, arg2, oper);
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{arg1.Type}' and '{arg2.Type}'.",
                    oper.Location);
            }

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (oper.Type)
            {
                case TokenType.MUL:
                    return ParseMulExpInternal(Expression.Multiply(arg1, arg2));
                case TokenType.DIV:
                    return ParseMulExpInternal(Expression.Divide(arg1, arg2));
                default:
                    throw new ArgumentOutOfRangeException(); // never gets here
            }
        }

        private Expression ParseVal()
        {
            if (PeekType() == TokenType.LEFT_BRACKET)
            {
                ReadToken();
                var arg = ParseOrExp();
                if (PeekType() != TokenType.RIGHT_BRACKET)
                    throw new ParseErrorException(
                        $"Closing bracket missing. Unexpected token: '{PeekValue()}'.",
                        PeekToken().Location);
                ReadToken();
                return arg;
            }

            switch (PeekType())
            {
                case TokenType.NULL:
                    return ParseNull();
                case TokenType.INT:
                    return ParseInt();
                case TokenType.FLOAT:
                    return ParseFloat();
                case TokenType.BOOL:
                    return ParseBool();
                case TokenType.STRING:
                    return ParseString();
                case TokenType.FUNC:
                    return ParseFunc();
                case TokenType.EOF:
                    throw new ParseErrorException(
                        "Expected \"null\", int, float, bool, string or func. Unexpected end of expression.",
                        PeekToken().Location);
                default:
                    throw new ParseErrorException(
                        $"Expected \"null\", int, float, bool, string or func. Unexpected token: '{PeekValue()}'.",
                        PeekToken().Location);
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

        private Expression ParseFunc()
        {
            var func = PeekToken();
            var name = func.Value.ToString();
            ReadToken(); // read name

            if (PeekType() != TokenType.LEFT_BRACKET)
                return ExtractFieldExpression(name); // get property or enum

            // parse a function call
            ReadToken(); // read "("
            var args = new List<Tuple<Expression, Location>>();
            while (PeekType() != TokenType.RIGHT_BRACKET) // read comma-separated arguments until we hit ")"
            {
                var tkn = PeekToken();
                var arg = ParseOrExp();
                if (PeekType() == TokenType.COMMA)
                    ReadToken();
                args.Add(new Tuple<Expression, Location>(arg, tkn.Location));
            }
            ReadToken(); // read ")"

            return ExtractMethodExpression(name, args, func.Location); // get method call
        }

        private Expression ExtractFieldExpression(string name)
        {
            var expression = FetchPropertyValue(name) ?? FetchEnumValue(name) ?? FetchConstValue(name);
            if (expression == null)
                throw new ParseErrorException(
                    $"Only public properties, constants and enums are accepted. Identifier '{name}' not known.",
                    PeekToken(1).Location);

            return expression;
        }

        private Expression FetchPropertyValue(string name)
        {
            var type = ContextType;
            var expr = ContextExpression;
            var parts = name.Split('.');

            var regex = new Regex(@"([a-zA-z_0-9]+)\[([0-9]+)\]"); // regex matching array element access

            foreach (var part in parts)
            {
                PropertyInfo pi;

                var match = regex.Match(part);                
                if (match.Success)
                {
                    var partName = match.Groups[1].Value;                    
                    var idx = int.Parse(match.Groups[2].Value);

                    pi = type.GetProperty(partName);
                    if (pi == null)
                        return null;

                    var property = Expression.Property(expr, pi);
                    if (pi.PropertyType.IsArray) // check if we have an array type
                    {
                        expr = Expression.ArrayIndex(property, Expression.Constant(idx));
                        type = pi.PropertyType.GetElementType();
                        continue;
                    }

                    // not an array - check if the type declares indexer otherwise
                    pi = pi.PropertyType.GetProperties().FirstOrDefault(p => p.GetIndexParameters().Any()); // look for indexer property (usually called Item...)
                    if (pi != null)
                    {
                        expr = Expression.Property(property, pi.Name, Expression.Constant(idx));
                        type = pi.PropertyType;
                        continue;
                    }

                    throw new ParseErrorException(
                        $"Identifier '{name.Substring(0, name.Length - 2 - idx.ToString().Length)}' either does not represent an array type or does not declare indexer.",
                        PeekToken(1).Location);
                }

                pi = type.GetProperty(part);
                if (pi == null)
                    return null;

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            Fields[name] = type;
            return expr;
        }

        private Expression FetchEnumValue(string name)
        {
            var parts = name.Split('.');
            if (parts.Length > 1)
            {
                var enumTypeName = string.Join(".", parts.Take(parts.Length - 1).ToList());
                var enumTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetLoadableTypes())
                    .Where(t => t.IsEnum && string.Concat(".", t.FullName.Replace("+", ".")).EndsWith(string.Concat(".", enumTypeName)))
                    .ToList();

                if (enumTypes.Count > 1)
                    throw new ParseErrorException(
                        $"Enum '{enumTypeName}' is ambiguous, found following:{Environment.NewLine}{string.Join("," + Environment.NewLine, enumTypes.Select(x => $"'{x.FullName}'"))}.",
                        PeekToken(1).Location);

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

        private Expression FetchConstValue(string name)
        {
            FieldInfo constant;
            var parts = name.Split('.');
            if (parts.Length > 1)
            {
                var constTypeName = string.Join(".", parts.Take(parts.Length - 1).ToList());
                var constants = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetLoadableTypes())
                    .Where(t => string.Concat(".", t.FullName.Replace("+", ".")).EndsWith(string.Concat(".", constTypeName)))
                    .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name.Equals(parts.Last())))                    
                    .ToList();

                if (constants.Count > 1)
                    throw new ParseErrorException(
                        $"Constant '{name}' is ambiguous, found following:{Environment.NewLine}{string.Join("," + Environment.NewLine, constants.Select(x => x.ReflectedType != null ? $"'{x.ReflectedType.FullName}.{x.Name}'" : $"'{x.Name}'"))}.",
                        PeekToken(1).Location);

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
            AssertMethodNameExistence(name, funcPos);
            var expression = FetchModelMethod(name, args, funcPos) ?? FetchToolchainMethod(name, args, funcPos); // firstly, try to take method from model context - if not found, take one from toolchain
            if (expression == null)
                throw new ParseErrorException(
                    $"Function '{name}' accepting {args.Count} argument{(args.Count == 1 ? string.Empty : "s")} not found.",
                    funcPos);

            return expression;
        }

        private Expression FetchModelMethod(string name, IList<Tuple<Expression, Location>> args, Location funcPos)
        {
            var signatures = ContextType.GetMethods()
                .Where(mi => name.Equals(mi.Name) && mi.GetParameters().Length == args.Count)
                .ToList();
            if (signatures.Count == 0)
                return null;
            AssertNonAmbiguity(signatures.Count, name, args.Count, funcPos);

            return CreateMethodCallExpression(ContextExpression, args, signatures.Single());
        }

        private Expression FetchToolchainMethod(string name, IList<Tuple<Expression, Location>> args, Location funcPos)
        {
            var signatures = Functions.ContainsKey(name)
                ? Functions[name].Where(f => f.Parameters.Count == args.Count).ToList()
                : new List<LambdaExpression>();
            if (signatures.Count == 0)
                return null;
            AssertNonAmbiguity(signatures.Count, name, args.Count, funcPos);

            return CreateInvocationExpression(signatures.Single(), args, name);
        }

        private InvocationExpression CreateInvocationExpression(LambdaExpression funcExpr, IList<Tuple<Expression, Location>> parsedArgs, string funcName)
        {
            AssertParamsEquality(funcExpr.Parameters.Count, parsedArgs.Count, funcName);

            var convertedArgs = new List<Expression>();
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i].Item1;
                var pos = parsedArgs[i].Item2;
                var param = funcExpr.Parameters[i];
                convertedArgs.Add(arg.Type == param.Type
                    ? arg
                    : ConvertArgument(arg, param.Type, funcName, i + 1, pos));
            }
            return Expression.Invoke(funcExpr, convertedArgs);
        }

        private MethodCallExpression CreateMethodCallExpression(Expression contextExpression, IList<Tuple<Expression, Location>> parsedArgs, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            AssertParamsEquality(parameters.Length, parsedArgs.Count, methodInfo.Name);

            var convertedArgs = new List<Expression>();
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i].Item1;
                var pos = parsedArgs[i].Item2;
                var param = parameters[i];
                convertedArgs.Add(arg.Type == param.ParameterType
                    ? arg
                    : ConvertArgument(arg, param.ParameterType, methodInfo.Name, i + 1, pos));
            }
            return Expression.Call(contextExpression, methodInfo, convertedArgs);
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
                    argPos);
            }
        }

        private TokenType UnifySign()
        {
            var operators = new List<TokenType>();
            while (true)
            {
                if (!new[] {TokenType.ADD, TokenType.SUB}.Contains(PeekType()))
                    return operators.Count(x => x.Equals(TokenType.SUB))%2 == 1
                        ? TokenType.SUB
                        : TokenType.ADD;

                operators.Add(PeekType());
                ReadToken();
            }
        }

        private Expression InverseNumber(Expression arg)
        {
            Expression zero = Expression.Constant(0);
            Helper.MakeTypesCompatible(zero, arg, out zero, out arg);
            return Expression.Subtract(zero, arg);
        }

        private void AssertMethodNameExistence(string name, Location funcPos)
        {
            if (!Functions.ContainsKey(name) && !ContextType.GetMethods().Any(mi => name.Equals(mi.Name)))
                throw new ParseErrorException(
                    $"Function '{name}' not known.",
                    funcPos);
        }

        private void AssertNonAmbiguity(int signatures, string funcName, int args, Location funcPos)
        {
            if (signatures > 1)
                throw new ParseErrorException(
                    $"Function '{funcName}' accepting {args} argument{(args == 1 ? string.Empty : "s")} is ambiguous.",
                    funcPos);
        }

        private void AssertParamsEquality(int expected, int actual, string funcName)
        {
            if (expected != actual)
                throw new ParseErrorException(
                    $"Incorrect number of arguments provided. Function '{funcName}' expects {expected}, not {actual}.",
                    null);
        }

        private void AssertArgsNotNullLiterals(Expression arg1, Expression arg2, Token oper)
        {
            AssertArgsNotNullLiterals(arg1, arg1.Type, arg2, arg2.Type, oper);
        }

        private void AssertArgsNotNullLiterals(Expression arg1, Type type1, Expression arg2, Type type2, Token oper)
        {
            if (arg1.IsNullLiteral() && arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and 'null'.",
                    oper.Location);
            if (!arg1.IsNullLiteral() && arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type '{type1}' and 'null'.",
                    oper.Location);
            if (arg1.IsNullLiteral() && !arg2.IsNullLiteral())
                throw new ParseErrorException(
                    $"Operator '{oper.Value}' cannot be applied to operands of type 'null' and '{type2}'.",
                    oper.Location);
        }
    }
}
