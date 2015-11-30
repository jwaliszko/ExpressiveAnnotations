using System.Linq;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusive.Providers;
using ExpressiveAnnotations.MvcUnobtrusive.Validators;
using Xunit;

namespace ExpressiveAnnotations.MvcUnobtrusive.Tests
{
    public class ProviderTest: BaseTest
    {
        [Fact]
        public void validators_ordered_by_ascending_priorities()
        {
            var model = new Model();
            var controllerContext = GetControllerContext();
            var provider = new ExpressiveAnnotationsModelValidatorProvider();
            var validators = provider.GetValidators(GetModelMetadata(model, m => m.Value), controllerContext).ToList();

            var exps = validators.Where(x => x is RequiredIfValidator)
                .Select(x => x.GetClientValidationRules()
                    .Single().ValidationParameters["expression"].ToString()
                    .Count(c => c == '!'))
                .ToList();

            Assert.Equal(4, exps.Count);
            Assert.Equal(1, exps[0]);
            Assert.Equal(0, exps[1]);
            Assert.Equal(3, exps[2]);
            Assert.Equal(2, exps[3]);

            exps = validators.Where(x => x is AssertThatValidator)
                .Select(x => x.GetClientValidationRules()
                    .Single().ValidationParameters["expression"].ToString()
                    .Count(c => c == '!'))
                .ToList();

            Assert.Equal(4, exps.Count);
            Assert.Equal(1, exps[0]);
            Assert.Equal(0, exps[1]);
            Assert.Equal(3, exps[2]);
            Assert.Equal(2, exps[3]);
        }

        private class Model
        {
            [RequiredIf("true", Priority = 0)]
            [RequiredIf("!true", Priority = -1)]
            [RequiredIf("!!true")] // no priority defind - will be moved at the and of its group
            [RequiredIf("!!!true", Priority = 1)]
            [AssertThat("true", Priority = 0)]
            [AssertThat("!true", Priority = -1)]
            [AssertThat("!!true")] // no priority defind - will be moved at the and of its group
            [AssertThat("!!!true", Priority = 1)]
            public int Value { get; set; }
        }
    }
}
