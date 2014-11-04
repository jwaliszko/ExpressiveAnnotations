/* https://github.com/JaroslawWaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jaroslaw Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators
{
    public abstract class ExpressiveValidator<T> : DataAnnotationsModelValidator<T> where T:ExpressiveAttribute
    {
        private readonly object _locker = new object();

        protected ExpressiveValidator(ModelMetadata metadata, ControllerContext context, T attribute)
            : base(metadata, context, attribute)
        {
            try
            {
                AnnotatedField = string.Format("{0}.{1}", metadata.ContainerType.FullName, metadata.PropertyName).ToLowerInvariant();
                var attribId = string.Format("{0}.{1}", attribute.TypeId, AnnotatedField).ToLowerInvariant();
                var fieldsId = string.Format("fields.{0}", attribId);
                var constsId = string.Format("consts.{0}", attribId);
                FieldsMap = HttpRuntime.Cache.Get(fieldsId) as IDictionary<string, string>;
                ConstsMap = HttpRuntime.Cache.Get(constsId) as IDictionary<string, object>;

                if (CacheEmpty)
                {
                    lock (_locker)
                    {
                        if (CacheEmpty)
                        {
                            var parser = new Parser();
                            parser.RegisterMethods();
                            parser.Parse(metadata.ContainerType, attribute.Expression);

                            FieldsMap = parser.GetFields().ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value));
                            ConstsMap = parser.GetConsts();

                            Assert.NoNamingCollisionsAtCorrespondingSegments(FieldsMap.Keys, ConstsMap.Keys);
                            HttpContext.Current.Cache.Insert(fieldsId, FieldsMap);
                            HttpContext.Current.Cache.Insert(constsId, ConstsMap);

                            attribute.Compile(metadata.ContainerType);
                        }
                    }
                }

                Expression = attribute.Expression;
                FormattedErrorMessage = attribute.FormatErrorMessage(metadata.GetDisplayName(), attribute.Expression);
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    string.Format("{0}: validation applied to {1} field failed.",
                        GetType().Name, metadata.PropertyName), e);
            }
        }

        protected string Expression { get; set; }
        protected string FormattedErrorMessage { get; set; }
        protected IDictionary<string, string> FieldsMap { get; set; }
        protected IDictionary<string, object> ConstsMap { get; set; }
        protected string AnnotatedField { get; set; }
        protected bool CacheEmpty
        {
            get { return FieldsMap == null && ConstsMap == null; }
        }

        protected string ValidTypeSuffix()
        {
            var count = Storage.Get<int>(AnnotatedField) + 1;
            Assert.AttribsQuantityAllowed(count);

            Storage.Set(AnnotatedField, count);
            return count == 1 ? string.Empty : char.ConvertFromUtf32(95 + count);
        }
    }
}
