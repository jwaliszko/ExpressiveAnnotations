using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpressiveAnnotations.Functions;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    //public class CustomFunctionsProvider : IFunctionsProvider
    //{
    //    public IDictionary<string, IList<LambdaExpression>> GetFunctions()
    //    {
    //        return new Dictionary<string, IList<LambdaExpression>>
    //        {
    //            {"IntArrayLength", new LambdaExpression[] {(Expression<Func<int[], int>>) (array => array.Length)}},
    //            {"StringArrayLength", new LambdaExpression[] {(Expression<Func<string[], int>>) (array => array.Length)}},
    //            {"IntArrayContains", new LambdaExpression[] {(Expression<Func<int?, int[], bool>>) ((value, array) => value != null && array.Contains((int)value))}}
    //        };
    //    }
    //}

    public static class CustomToolchain
    {
        public static void Register()
        {
            // Toolchain.Instance.Recharge(new CustomFunctionsProvider()); // load complately new set of functions...
            // ...or simply add some new ones to existing toolchain:
            Toolchain.Instance.AddFunction<int[], int>("IntArrayLength", array => array.Length);
            Toolchain.Instance.AddFunction<string[], int>("StringArrayLength", array => array.Length);
            Toolchain.Instance.AddFunction<int?, int[], bool>("IntArrayContains", (value, array) => value != null && array.Contains((int)value));
        }
    }
}
