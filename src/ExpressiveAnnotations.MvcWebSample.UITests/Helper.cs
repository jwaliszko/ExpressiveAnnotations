using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public static class Helper
    {
        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Assembly assembly)
        {
            return assembly.GetTypes().SelectMany(CompileExpressiveAttributes);
        }

        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof (ExpressiveAttribute)));

            var attributes = new List<ExpressiveAttribute>();
            foreach (var prop in properties)
            {
                var attribs = prop.GetCustomAttributes<ExpressiveAttribute>().ToList();
                attribs.ForEach(x => x.Compile(prop.DeclaringType));
                attributes.AddRange(attribs);
            }
            return attributes;
        }
    }
}