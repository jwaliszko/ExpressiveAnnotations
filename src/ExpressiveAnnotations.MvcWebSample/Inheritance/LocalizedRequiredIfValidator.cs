using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Web.Mvc;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.MvcWebSample.Inheritance
{
    public class LocalizedRequiredIfValidator : ExpressiveValidator<LocalizedRequiredIfAttribute>
    {
        public LocalizedRequiredIfValidator(ModelMetadata metadata, ControllerContext context, LocalizedRequiredIfAttribute attribute)
            : base(metadata, context, attribute)
        {
            AllowEmpty = attribute.AllowEmptyStrings;
        }

        private bool AllowEmpty { get; set; }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = GetBasicRule("requiredif");
            rule.ValidationParameters.Add("allowempty", ToJson(AllowEmpty));
            yield return rule;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // code that deals with disposables should be consistent (and classes should be resilient to multiple Dispose() calls)
        private static string ToJson(object data)
        {
            Debug.Assert(data != null);

            var stringBuilder = new StringBuilder();
            var jsonSerializer = new JsonSerializer();
            using (var stringWriter = new StringWriter(stringBuilder))
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonSerializer.Serialize(jsonTextWriter, data);
                return stringBuilder.ToString();
            }
        }
    }
}
