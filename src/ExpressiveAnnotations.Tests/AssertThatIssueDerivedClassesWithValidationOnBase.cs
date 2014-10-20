using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class AssertThatIssueDerivedClassesWithValidationOnBase
    {
        public abstract class ModelBase
        {
            [AssertThat("SomeOtherValue > 1")]
            public object SomeValue { get; set; }

            public int SomeOtherValue { get; set; }
        }

        public class FirstDerived : ModelBase { }
        public class SecondDerived : ModelBase { }

        [TestMethod]
        public void ShouldNotThrowExceptionOnSecondType()
        {
            var firstDerived = new FirstDerived { SomeValue = this, SomeOtherValue = 0 };
            var secondDerived = new SecondDerived { SomeValue = this, SomeOtherValue = 0 };

            var firstContext = new ValidationContext(firstDerived, serviceProvider: null, items: null);
            var secondContext = new ValidationContext(secondDerived, serviceProvider: null, items: null);

            var results = new LinkedList<ValidationResult>();
            Validator.TryValidateObject(firstDerived, firstContext, results, true);
            Validator.TryValidateObject(secondDerived, secondContext, results, true);
        }

        [TestMethod]
        public void OrderDoesNotMatter()
        {
            var firstDerived = new FirstDerived { SomeValue = this, SomeOtherValue = 0 };
            var secondDerived = new SecondDerived { SomeValue = this, SomeOtherValue = 0 };

            var firstContext = new ValidationContext(firstDerived, serviceProvider: null, items: null);
            var secondContext = new ValidationContext(secondDerived, serviceProvider: null, items: null);

            var results = new LinkedList<ValidationResult>();
            Validator.TryValidateObject(secondDerived, secondContext, results, true);
            Validator.TryValidateObject(firstDerived, firstContext, results, true);
        }
    }
}