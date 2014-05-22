using System;
using ExpressiveAnnotations.ConditionalAttributes;
using ExpressiveAnnotations.Tests.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class RequiredIfAttributeTest
    {
        private class Model
        {
            public class SecretModel
            {
                public bool KnowSomething { get; set; }
                public string ShowSecretData { get; set; }
            }

            public SecretModel Secret { get; set; }

            public bool GoAbroad { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public string Email { get; set; }

            [RequiredIf(DependentProperty = "Email", TargetValue = "*")]
            public string SecondEmail { get; set; }

            [RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
            public string PassportNumber { get; set; }

            [RequiredIf(DependentProperty = "GoAbroad", TargetValue = "true")]
            public string CreditCardNumber { get; set; }

            [RequiredIf(DependentProperty = "GoAbroad", TargetValue = "[Secret.ShowSecretData]")]
            public string DebetCardNumber { get; set; }

            [RequiredIf(DependentProperty = "Email", TargetValue = "jaroslaw.waliszko@gmail.com")]
            public string WhatToDoAboutInsomnia { get; set; }

            [RequiredIf(DependentProperty = "Secret.KnowSomething", TargetValue = true)]
            public string WhatIsTheSecret { get; set; }

            [RequiredIf(DependentProperty = "StartDate", TargetValue = "[EndDate]")]
            public bool OneDayVisit { get; set; }
        }

        [TestMethod]
        public void Verify_if_not_required()
        {
            var model = new Model {GoAbroad = false};
            Assert.IsTrue(model.IsValid(m => m.PassportNumber));

            model = new Model {Email = "someone@fromsomewhere.com"};
            Assert.IsTrue(model.IsValid(m => m.WhatToDoAboutInsomnia));

            model = new Model {Email = "jaroslaw.waliszko@gmail.com", WhatToDoAboutInsomnia = "write a script"};
            Assert.IsTrue(model.IsValid(m => m.WhatToDoAboutInsomnia));

            model = new Model { Email = "  jaroslaw.waliszko@gmail.com  ", WhatToDoAboutInsomnia = "write a script" };
            Assert.IsTrue(model.IsValid(m => m.WhatToDoAboutInsomnia));

            model = new Model { Email = "  jaroslaw.waliszko@gmail.com  ", WhatToDoAboutInsomnia = "" };
            Assert.IsTrue(model.IsValid(m => m.WhatToDoAboutInsomnia));

            model = new Model {Email = "Jaroslaw.Waliszko@gmail.com", WhatToDoAboutInsomnia = ""};
            Assert.IsTrue(model.IsValid(m => m.WhatToDoAboutInsomnia));            
        }

        [TestMethod]
        public void Verify_if_required()
        {
            var model = new Model { GoAbroad = true, PassportNumber = null };
            Assert.IsFalse(model.IsValid(m => m.PassportNumber));

            model = new Model {GoAbroad = true, PassportNumber = ""};
            Assert.IsFalse(model.IsValid(m => m.PassportNumber));

            model = new Model {GoAbroad = true, PassportNumber = "   "};
            Assert.IsFalse(model.IsValid(m => m.PassportNumber));            

            model = new Model {Email = "jaroslaw.waliszko@gmail.com", WhatToDoAboutInsomnia = ""};
            Assert.IsFalse(model.IsValid(m => m.WhatToDoAboutInsomnia));
        }

        [TestMethod]
        public void Verify_if_nesting_works()
        {
            var model = new Model
            {
                Secret = new Model.SecretModel {KnowSomething = true},
                WhatIsTheSecret = "the secret is..."
            };
            Assert.IsTrue(model.IsValid(m => m.WhatIsTheSecret));
        }

        [TestMethod]
        public void Verify_if_target_value_extraction_works()
        {
            var model = new Model
            {
                StartDate = new DateTime(2014, 3, 22),
                EndDate = new DateTime(2014, 3, 22),
                OneDayVisit = false
            };
            Assert.IsFalse(model.IsValid(m => m.OneDayVisit));

            model = new Model
            {
                StartDate = new DateTime(2014, 3, 22),
                EndDate = new DateTime(2014, 3, 22),
                OneDayVisit = true
            };
            Assert.IsTrue(model.IsValid(m => m.OneDayVisit));

            model = new Model
            {
                StartDate = new DateTime(2014, 3, 22),
                EndDate = new DateTime(2014, 3, 23),
            };
            Assert.IsTrue(model.IsValid(m => m.OneDayVisit));
        }

        [TestMethod]
        public void Verify_wildcard()
        {
            var model = new Model
            {
                Email = "jaroslaw.waliszko@gmail.com",
                SecondEmail = "asd"
            };
            Assert.IsTrue(model.IsValid(m => m.SecondEmail));

            model = new Model
            {
                Email = "jaroslaw.waliszko@gmail.com",
                SecondEmail = ""
            };
            Assert.IsFalse(model.IsValid(m => m.SecondEmail));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_types_consistency_requirement()
        {
            try
            {
                var model = new Model { GoAbroad = true, CreditCardNumber = null };
                model.IsValid(m => m.CreditCardNumber);
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(
                    "Type mismatch detected in RequiredIfAttribute definition for CreditCardNumber field. Types consistency is required for GoAbroad field and its corresponding, " +
                    "explicitly provided, target value (explicit target values, if null or *, does not interfere with types consistency assertions).",
                    e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Verify_types_consistency_requirement_for_backend_property()
        {
            try
            {
                var model = new Model { GoAbroad = true, DebetCardNumber = null, Secret = new Model.SecretModel { ShowSecretData = null } };
                model.IsValid(m => m.DebetCardNumber);
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(
                    "Type mismatch detected in RequiredIfAttribute definition for DebetCardNumber field. Types consistency is required for GoAbroad field and its corresponding, " +
                    "provided indirectly through backing field, target value.",
                    e.Message);
                throw;
            }
        }
    }
}
