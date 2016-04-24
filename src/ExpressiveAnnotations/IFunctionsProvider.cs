/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */
 
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressiveAnnotations
{
    /// <summary>
    ///     Functions source.
    /// </summary>
    public interface IFunctionsProvider
    {
        /// <summary>
        ///     Gets functions for the <see cref="ExpressiveAnnotations.Analysis.Parser" />.
        /// </summary>
        /// <returns>
        ///     Registered functions.
        /// </returns>
        IDictionary<string, IList<LambdaExpression>> GetFunctions();
    }
}
