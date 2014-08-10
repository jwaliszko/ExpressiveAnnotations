using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressiveAnnotations.Analysis
{
    /* EBNF GRAMMAR:
     * 
     * expression => or-exp
     * or-exp     => and-exp [ "||" or-exp ]
     * and-exp    => not-exp [ "&&" and-exp ]
     * not-exp    => rel-exp | "!" not-exp
     * rel-exp    => add-exp [ rel-op add-exp ]
     * add-exp    => mul-exp add-exp'
     * add-exp'   => "+" add-exp | "-" add-exp
     * mul-exp    => val mul-exp'
     * mul-exp'   => "*" mul-exp | "/" mul-exp 
     * rel-op     => "==" | "!=" | ">" | ">=" | "<" | "<="     
     * val        => "null" | int | float | bool | string | func | "(" or-exp ")"
     */

    /// <summary>
    /// Performs syntactic analysis of provided logical expression within given context.
    /// </summary>
    public sealed class Parser
    {
        private Stack<Token> Tokens { get; set; }        
        private Type ContextType { get; set; }
        private Expression ContextExpression { get; set; }
        private IDictionary<string, Type> Fields { get; set; }
        private IDictionary<string, object> Consts { get; set; }        
        private IDictionary<string, IList<LambdaExpression>> Functions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        public Parser()
        {
            Fields = new Dictionary<string, Type>();
            Consts = new Dictionary<string, object>();            
            Functions = new Dictionary<string, IList<LambdaExpression>>();
        }

        /// <summary>
        /// Parses the specified logical expression into expression tree within given object context.
        /// </summary>
        /// <typeparam name="Context">The type identifier of the context within which the expression is interpreted.</typeparam>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        /// A delegate containing the compiled version of the lambda expression described by created expression tree.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Func<Context, bool> Parse<Context>(string expression)
        {
            try
            {
                Clear();
                ContextType = typeof (Context);
                var param = Expression.Parameter(typeof (Context));
                ContextExpression = param;
                InitTokenizer(expression);
                var expressionTree = ParseExpression();
                var lambda = Expression.Lambda<Func<Context, bool>>(expressionTree, param);
                return lambda.Compile();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format("Parsing failed. Invalid expression: {0}", expression), e);
            }
        }

        /// <summary>
        /// Parses the specified logical expression and builds expression tree.
        /// </summary>
        /// <param name="context">The type instance of the context within which the expression is interpreted.</param>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        /// A delegate containing the compiled version of the lambda expression described by produced expression tree.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Func<object, bool> Parse(Type context, string expression)
        {
            try
            {
                Clear();
                ContextType = context;
                var param = Expression.Parameter(typeof (object));
                ContextExpression = Expression.Convert(param, context);
                InitTokenizer(expression);
                var expressionTree = ParseExpression();
                var lambda = Expression.Lambda<Func<object, bool>>(expressionTree, param);
                return lambda.Compile();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format("Parsing failed. Invalid expression: {0}", expression), e);
            }
        }

        /// <summary>
        /// Adds function signature to the parser context.
        /// </summary>
        /// <typeparam name="Result">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<Result>(string name, Expression<Func<Result>> func)
        {
            if(!Functions.ContainsKey(name))
                Functions[name] = new List<LambdaExpression>();
            Functions[name].Add(func);
        }

        /// <summary>
        /// Adds function signature to the parser context.
        /// </summary>
        /// <typeparam name="Arg1">First argument.</typeparam>
        /// <typeparam name="Result">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<Arg1, Result>(string name, Expression<Func<Arg1, Result>> func)
        {
            if (!Functions.ContainsKey(name))
                Functions[name] = new List<LambdaExpression>();
            Functions[name].Add(func);
        }

        /// <summary>
        /// Adds function signature to the parser context.
        /// </summary>
        /// <typeparam name="Arg1">First argument.</typeparam>
        /// <typeparam name="Arg2">Second argument.</typeparam>
        /// <typeparam name="Result">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<Arg1, Arg2, Result>(string name, Expression<Func<Arg1, Arg2, Result>> func)
        {
            if (!Functions.ContainsKey(name))
                Functions[name] = new List<LambdaExpression>();
            Functions[name].Add(func);
        }

        /// <summary>
        /// Adds function signature to the parser context.
        /// </summary>
        /// <typeparam name="Arg1">First argument.</typeparam>
        /// <typeparam name="Arg2">Second argument.</typeparam>
        /// <typeparam name="Arg3">Third argument.</typeparam>
        /// <typeparam name="Result">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction<Arg1, Arg2, Arg3, Result>(string name, Expression<Func<Arg1, Arg2, Arg3, Result>> func)
        {
            if (!Functions.ContainsKey(name))
                Functions[name] = new List<LambdaExpression>();
            Functions[name].Add(func);
        }

        /// <summary>
        /// Gets properties names and types extracted from parsed expression within specified context.
        /// </summary>
        /// <returns>
        /// Dictionary containing names and types.
        /// </returns>
        public IDictionary<string, Type> GetFields()
        {
            return Fields;
        }

        /// <summary>
        /// Gets constants names and values extracted from parsed expression within specified context.
        /// </summary>
        /// <returns>
        /// Dictionary containing names and values.
        /// </returns>
        public IDictionary<string, object> GetConsts()
        {
            return Consts;
        }

        private void Clear()
        {
            Fields.Clear();
            Consts.Clear();
        }

        private void InitTokenizer(string expression)
        {
            var lexer = new Lexer();
            Tokens = new Stack<Token>(lexer.Analyze(expression).Reverse());
        }

        private TokenType PeekType()
        {
            return Tokens.Peek().Type;
        }

        private object PeekValue()
        {
            return Tokens.Peek().Value;
        }

        private void ReadToken()
        {
            Tokens.Pop();
        }

        private Expression ParseExpression()
        {
            var expr = ParseOrExp();
            if (PeekType() != TokenType.EOF)
                throw new InvalidOperationException(string.Format("Parsing expected to be completed. Unexpected token: {0}", PeekValue()));
            return expr;
        }

        private Expression ParseOrExp()
        {
            var arg1 = ParseAndExp();
            if (PeekType() != TokenType.OR)
                return arg1;
            ReadToken();
            var arg2 = ParseOrExp();
            return Expression.OrElse(arg1, arg2); // short-circuit evaluation
        }

        private Expression ParseAndExp()
        {
            var arg1 = ParseNotExp();
            if (PeekType() != TokenType.AND)
                return arg1;
            ReadToken();
            var arg2 = ParseAndExp();
            return Expression.AndAlso(arg1, arg2); // short-circuit evaluation
        }

        private Expression ParseNotExp()
        {
            if (PeekType() != TokenType.NOT)
                return ParseRelExp();
            ReadToken();
            return Expression.Not(ParseNotExp()); // allows multiple negations
        }

        private Expression ParseRelExp()
        {
            var arg1 = ParseAddExp();
            if (!new[] {TokenType.LT, TokenType.LE, TokenType.GT, TokenType.GE, TokenType.EQ, TokenType.NEQ}.Contains(PeekType()))
                return arg1;
            var oper = PeekType();
            ReadToken();
            var arg2 = ParseAddExp();

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (oper)
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

        private Expression ParseAddExp()
        {
            var arg = ParseMulExp();
            return ParseAddExpInternal(arg);
        }        

        private Expression ParseAddExpInternal(Expression arg1)
        {
            if (!new[] {TokenType.ADD, TokenType.SUB}.Contains(PeekType()))
                return arg1;
            var oper = PeekType();
            ReadToken();
            var arg2 = ParseMulExp();

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (oper)
            {
                case TokenType.ADD:
                    return ParseAddExpInternal(
                        (arg1.Type == typeof (string) || arg2.Type == typeof (string))
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
            var arg = ParseVal();
            return ParseMulExpInternal(arg);
        }

        private Expression ParseMulExpInternal(Expression arg1)
        {
            if (!new[] {TokenType.MUL, TokenType.DIV}.Contains(PeekType()))
                return arg1;
            var oper = PeekType();
            ReadToken();
            var arg2 = ParseVal();

            Helper.MakeTypesCompatible(arg1, arg2, out arg1, out arg2);
            switch (oper)
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
                    throw new InvalidOperationException(string.Format("Closing bracket missing. Unexpected token: {0}", PeekValue()));
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
                default:
                    throw new InvalidOperationException(string.Format("Expected \"null\", int, float, bool, string or func. Unexpected token: {0}", PeekValue()));
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
            return Expression.Constant(value, typeof(int));
        }

        private Expression ParseFloat()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof(double));
        }

        private Expression ParseBool()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof(bool));
        }

        private Expression ParseString()
        {
            var value = PeekValue();
            ReadToken();
            return Expression.Constant(value, typeof(string));
        }

        private Expression ParseFunc()
        {
            var name = PeekValue().ToString();
            ReadToken(); // read name

            if (PeekType() != TokenType.LEFT_BRACKET)
                return ExtractFieldExpression(name); // get property or enum

            // parse a function call
            ReadToken(); // read "("
            var args = new List<Expression>();
            while (PeekType() != TokenType.RIGHT_BRACKET) // read comma-separated arguments until we hit ")"
            {
                var arg = ParseOrExp();
                if (PeekType() == TokenType.COMMA)
                    ReadToken();
                args.Add(arg);
            }
            ReadToken(); // read ")"

            return ExtractMethodExpression(name, args); // get method call
        }        

        private Expression ExtractFieldExpression(string name)
        {
            var expression = FetchPropertyValue(name) ?? FetchEnumValue(name);
            if (expression == null)
                throw new InvalidOperationException(string.Format("Only public properties or enums are accepted. Invalid identifier: {0}", name));

            return expression;
        }        

        private Expression FetchPropertyValue(string name)
        {
            var type = ContextType;
            var expr = ContextExpression;
            var parts = name.Split('.');

            foreach (var part in parts)
            {
                var pi = type.GetProperty(part);
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
            if (parts.Count() > 1)
            {
                var enumTypeName = string.Join(".", parts.Take(parts.Count() - 1).ToList());
                var enumTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetLoadableTypes()).Where(t => t.IsEnum && t.FullName.Replace("+", ".").EndsWith(enumTypeName)).ToList();

                if (enumTypes.Count() > 1)
                    throw new InvalidOperationException(
                        string.Format("Enum {0} is ambiguous, found following:{1}",
                            enumTypeName, Environment.NewLine + string.Join(Environment.NewLine, enumTypes.Select(x => x.FullName))));

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

        private Expression ExtractMethodExpression(string name, IList<Expression> args)
        {
            var expression = FetchModelMethod(name, args) ?? FetchToolchainMethod(name, args); // firstly, try to take method from model context - if not found, take one from toolchain
            if (expression == null)
                throw new InvalidOperationException(string.Format("Function {0} accepting {1} arguments not found.", name, args.Count));

            return expression;
        }

        private Expression FetchModelMethod(string name, IList<Expression> args)
        {
            var signatures = ContextType.GetMethods()
                .Where(mi => name.Equals(mi.Name) && mi.GetParameters().Length == args.Count)
                .ToList();
            if (signatures.Count == 0)
                return null;
            if (signatures.Count > 1)
                throw new InvalidOperationException(string.Format("Function {0} accepting {1} arguments is ambiguous.", name, args.Count));

            return CreateMethodCallExpression(ContextExpression, signatures.Single(), args);
        }

        private Expression FetchToolchainMethod(string name, IList<Expression> args)
        {
            var signatures = Functions.ContainsKey(name)
                ? Functions[name].Where(f => f.Parameters.Count == args.Count).ToList()
                : new List<LambdaExpression>();
            if (signatures.Count == 0)
                return null;
            if (signatures.Count > 1)
                throw new InvalidOperationException(string.Format("Function {0} accepting {1} arguments is ambiguous.", name, args.Count));

            return CreateInvocationExpression(signatures.Single(), args, name);
        }

        private static InvocationExpression CreateInvocationExpression(LambdaExpression funcExpr, IList<Expression> parsedArgs, string funcName)
        {            
            if (funcExpr.Parameters.Count != parsedArgs.Count)
                throw new InvalidOperationException(
                    string.Format("Incorrect number of parameters provided. Function {0} expects {1}, not {2}.", funcName, funcExpr.Parameters.Count, parsedArgs.Count));

            var convertedArgs = new List<Expression>();
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i];
                var param = funcExpr.Parameters[i];
                if (arg.Type == param.Type)
                    convertedArgs.Add(arg);
                else
                {
                    try
                    {
                        convertedArgs.Add(Expression.Convert(arg, param.Type));
                    }
                    catch
                    {
                        throw new InvalidOperationException(string.Format("Argument {0} type conversion from {1} to needed {2} failed.", i, arg.Type.Name, param.Type.Name));
                    }
                }
            }
            return Expression.Invoke(funcExpr, convertedArgs);
        }

        private static MethodCallExpression CreateMethodCallExpression(Expression contextExpression, MethodInfo methodInfo, IList<Expression> parsedArgs)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Count() != parsedArgs.Count)
                throw new InvalidOperationException(
                    string.Format("Incorrect number of parameters provided. Function {0} expects {1}, not {2}.", methodInfo.Name, parameters.Count(), parsedArgs.Count));

            var convertedArgs = new List<Expression>();
            for (var i = 0; i < parsedArgs.Count; i++)
            {
                var arg = parsedArgs[i];
                var param = parameters[i];
                if (arg.Type == param.ParameterType)
                    convertedArgs.Add(arg);
                else
                {
                    try
                    {
                        convertedArgs.Add(Expression.Convert(arg, param.ParameterType));
                    }
                    catch
                    {
                        throw new InvalidOperationException(string.Format("Argument {0} type conversion from {1} to needed {2} failed.", i, arg.Type.Name, param.ParameterType.Name));
                    }
                }
            }
            return Expression.Call(contextExpression, methodInfo, convertedArgs);            
        }
    }
}
