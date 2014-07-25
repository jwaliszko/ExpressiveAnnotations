using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        private IDictionary<string, Type> Enums { get; set; }
        private IDictionary<string, Type> Members { get; set; }
        private Type ContextType { get; set; }
        private Expression ContextExpression { get; set; }
        private IDictionary<string, IList<LambdaExpression>> Functions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        public Parser()
        {
            Enums = new Dictionary<string, Type>();
            Members = new Dictionary<string, Type>();
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
        public Func<Context, bool> Parse<Context>(string expression)
        {
            Clear();
            ContextType = typeof(Context);
            var param = Expression.Parameter(typeof(Context));
            ContextExpression = param;
            InitTokenizer(expression);
            var expressionTree = ParseExpression();
            var lambda = Expression.Lambda<Func<Context, bool>>(expressionTree, param);
            return lambda.Compile();
        }        

        /// <summary>
        /// Parses the specified logical expression and builds expression tree.
        /// </summary>
        /// <param name="context">The type instance of the context within which the expression is interpreted.</param>
        /// <param name="expression">The logical expression.</param>
        /// <returns>
        /// A delegate containing the compiled version of the lambda expression described by produced expression tree.
        /// </returns>
        public Func<object, bool> Parse(Type context, string expression)
        {
            Clear();
            ContextType = context;
            var param = Expression.Parameter(typeof(object));
            ContextExpression = Expression.Convert(param, context);
            InitTokenizer(expression);
            var expressionTree = ParseExpression();
            var lambda = Expression.Lambda<Func<object, bool>>(expressionTree, param);
            return lambda.Compile();
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
        /// Gets the fields and properties extracted from parsed expression within specified context.
        /// </summary>
        /// <returns>
        /// Dictionary containing names and types.
        /// </returns>
        public IDictionary<string, Type> GetMembers()
        {
            return Members;
        }

        /// <summary>
        /// Gets the parsed enums extracted from parsed expression within specified context.
        /// </summary>
        /// <returns>
        /// Dictionary containing names and types.
        /// </returns>
        public IDictionary<string, Type> GetEnums()
        {
            return Enums;
        }

        private void Clear()
        {
            Members.Clear();
            Enums.Clear();
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
                throw new InvalidOperationException(string.Format("Unexpected token {0}.", PeekValue()));
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
                    throw new InvalidOperationException("Unexpected operator.");
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
                    throw new InvalidOperationException();
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
                    throw new InvalidOperationException();
            }
        }

        private Expression ParseVal()
        {            
            if (PeekType() == TokenType.LEFT_BRACKET)
            {
                ReadToken();
                var arg = ParseOrExp();
                if (PeekType() != TokenType.RIGHT_BRACKET)
                    throw new InvalidOperationException(string.Format("Unexpected token {0}. Closing bracket missing.", PeekValue()));
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
                    throw new InvalidOperationException(string.Format("Unexpected token {0}. Expected \"null\", int, float, bool, string or func.", PeekValue()));
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
            return Expression.Constant(value, typeof(float));
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
                return ExtractMemberExpression(name);

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

            // take function from model            
            var methods = ContextType.GetMethods().Where(mi => name.Equals(mi.Name)).ToList();
            if (methods.Any())
            {
                var choosen = methods.Where(x => x.GetParameters().Length == args.Count).ToList();
                if (choosen.Count == 1)
                    return Expression.Call(ContextExpression, choosen.Single(), args);
            }

            // take function from toolchain
            if(!Functions.ContainsKey(name))
                throw new InvalidOperationException(string.Format("Function {0} not found.", name));
            var signatures = Functions[name].Where(x => x.Parameters.Count == args.Count).ToList();            
            if(signatures.Count == 0)
                throw new InvalidOperationException(string.Format("Function {0} accepting {1} arguments not found.", name, args.Count));
            // signatures are only diversed by numbers of arguments
            if(signatures.Count > 1)
                throw new InvalidOperationException(string.Format("Function {0} accepting {1} arguments is ambiguous.", name, args.Count));

            return CreateLambdaCallExpression(signatures.Single(), args, name);
        }

        private Expression ExtractMemberExpression(string name)
        {
            var expr = ExtractMemberExpression(name, ContextType, ContextExpression);
            if (expr != null)
            {
                if(!Members.ContainsKey(name))
                    Members.Add(name, expr.Type);
                return expr;
            }

            var parts = name.Split('.');
            if (parts.Count() > 1)
            {
                name = string.Join(".", parts.Take(parts.Count() - 1).ToList());
                var enumTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[] { }; } })
                    .Where(t => t.IsEnum && t.FullName.EndsWith(name)).ToList();

                if (enumTypes.Count() > 1)
                    throw new ArgumentException(string.Format("Dynamic extraction failed. {0} enum identifier is ambigous. Provide namespace.", name), name);

                var type = enumTypes.SingleOrDefault();
                if (type != null)
                {
                    if (!Enums.ContainsKey(name))
                        Enums.Add(name, type);
                    return ExtractMemberExpression(parts.Last(), type, Expression.Parameter(type));
                }
            }

            throw new ArgumentException(string.Format("Dynamic extraction failed. Member {0} not found.", name), name);
        }

        private Expression ExtractMemberExpression(string name, Type type, Expression expr)
        {            
            var parts = name.Split('.');
            foreach (var part in parts)
            {
                var pi = type.GetProperty(part);
                if (pi == null)
                {
                    var fi = type.GetField(part);
                    if (fi == null)
                        return null;
                   
                    expr = fi.IsStatic ? Expression.Field(null, fi) : Expression.Field(expr, fi);
                    type = fi.FieldType;
                    continue;
                }
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            return expr;
        }

        private static InvocationExpression CreateLambdaCallExpression(LambdaExpression funcExpr, IList<Expression> parsedArgs, string funcName)
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
    }
}
