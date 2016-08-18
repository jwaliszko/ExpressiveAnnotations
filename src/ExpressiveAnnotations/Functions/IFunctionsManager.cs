/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Linq.Expressions;

namespace ExpressiveAnnotations.Functions
{
    /// <summary>
    ///     Registers new functions of predefined signatures.
    /// </summary>
    public interface IFunctionsManager
    {
        /// <summary>
        ///     Loads complately new provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        void Recharge(IFunctionsProvider provider);

        /// <summary>
        ///     Register function of arbitrary signature for the parser.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        void AddFunction(string name, LambdaExpression func);

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        void AddFunction<TResult>(string name, Expression<Func<TResult>> func);

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        void AddFunction<TArg1, TResult>(string name, Expression<Func<TArg1, TResult>> func);

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        void AddFunction<TArg1, TArg2, TResult>(string name, Expression<Func<TArg1, TArg2, TResult>> func);

        /// <summary>
        ///     Registers function signature for the parser.
        /// </summary>
        /// <typeparam name="TArg1">First argument.</typeparam>
        /// <typeparam name="TArg2">Second argument.</typeparam>
        /// <typeparam name="TArg3">Third argument.</typeparam>
        /// <typeparam name="TResult">Type identifier of returned result.</typeparam>
        /// <param name="name">Function name.</param>
        /// <param name="func">Function lambda.</param>
        void AddFunction<TArg1, TArg2, TArg3, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TResult>> func);

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
        void AddFunction<TArg1, TArg2, TArg3, TArg4, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> func);

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
        void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> func);

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
        void AddFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(string name, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> func);
    }
}
