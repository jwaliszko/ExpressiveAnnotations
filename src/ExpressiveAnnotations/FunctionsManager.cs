/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressiveAnnotations
{
    /// <summary>
    ///     Registers new functions of predefined signatures.
    /// </summary>
    /// <seealso cref="ExpressiveAnnotations.IFunctionsManager" />
    /// <seealso cref="ExpressiveAnnotations.IFunctionsProvider" />    
    public class FunctionsManager : IFunctionsManager, IFunctionsProvider
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FunctionsManager" /> class.
        /// </summary>
        public FunctionsManager()
        {
            Functions = new Dictionary<string, IList<LambdaExpression>>();
        }

        private IDictionary<string, IList<LambdaExpression>> Functions { get; }

        /// <summary>
        ///     Loads complately new provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public void Recharge(IFunctionsProvider provider)
        {
            Functions.Clear();
            foreach (var func in provider.GetFunctions())
            {
                Functions.Add(func.Key, func.Value);
            }
        }

        /// <summary>
        ///     Gets functions for the <see cref="ExpressiveAnnotations.Analysis.Parser" />.
        /// </summary>
        /// <returns>
        ///     Registered functions.
        /// </returns>
        public IDictionary<string, IList<LambdaExpression>> GetFunctions()
        {
            return Functions.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        ///     Register function of arbitrary signature for the parser.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        public void AddFunction(string name, LambdaExpression func)
        {
            PersistFunction(name, func);
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

        private void PersistFunction(string name, LambdaExpression func)
        {
            if (!Functions.ContainsKey(name))
                Functions[name] = new List<LambdaExpression>();
            Functions[name].Add(func);
        }
    }
}
