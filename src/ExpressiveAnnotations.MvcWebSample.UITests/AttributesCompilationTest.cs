using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExpressiveAnnotations.Attributes;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public static class Helper
    {
        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Assembly assembly)
        {
            return assembly.GetTypes().SelectMany(t => t.CompileExpressiveAttributes());
        }

        public static IEnumerable<ExpressiveAttribute> CompileExpressiveAttributes(this Type type)
        {
            var properties = type.GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(ExpressiveAttribute)));

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

    public class AttributesCompilationTest
    {
        [Fact]
        public void all_annotations_used_in_application_are_compiled_with_success() // reveals compile-time errors (no need to wait for application startup)
        {
            var applicationAssembly = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"..\..\..\ExpressiveAnnotations.MvcWebSample\bin\ExpressiveAnnotations.MvcWebSample.dll"));
            var assembly = Assembly.LoadFrom(applicationAssembly);
            var attribs = assembly.CompileExpressiveAttributes();
            Assert.Equal(25, attribs.Count());
        }
    }
}
