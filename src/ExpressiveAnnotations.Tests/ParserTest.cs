using System;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class ParserTest
    {
        private class Model
        {
            public Model SubModel { get; set; }

            public DateTime Date { get; set; }
            public int Number { get; set; }
            public bool Flag { get; set; }
            public string Text { get; set; }
        }

        [TestMethod]
        public void Verify()
        {
            var parser = new Parser();
            Assert.IsTrue(parser.Parse<object>("true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("!true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("!false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true && true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false && false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("true && false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false && true").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true || true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false || false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("true || false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("false || true").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("0 == 0").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("0 != 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 >= 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 <= 0").Invoke(null));

            Assert.IsFalse(parser.Parse<object>("0 == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 != 1").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("0 >= 1").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("0 > 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 <= 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 < 1").Invoke(null));
        }

        [TestMethod]
        public void VerifyContext()
        {
            var model = new Model
            {
                Date = DateTime.Now,
                Number = 0,
                Flag = true,
                Text = "hello parser",
                SubModel = new Model
                {
                    Date = DateTime.Now.AddDays(1),
                    Number = 1,
                    Flag = false,
                    Text = "hello parser"
                }
            };

            var parser = new Parser();
            Assert.IsTrue(parser.Parse(model.GetType(), "Flag").Invoke(model));
            Assert.IsFalse(parser.Parse(model.GetType(), "!Flag").Invoke(model));
            Assert.IsTrue(parser.Parse(model.GetType(), "Flag && true").Invoke(model));

            Assert.IsFalse(parser.Parse(model.GetType(), "SubModel.Flag").Invoke(model));
            Assert.IsTrue(parser.Parse(model.GetType(), "!SubModel.Flag").Invoke(model));
            Assert.IsFalse(parser.Parse(model.GetType(), "SubModel.Flag && true").Invoke(model));

            Assert.IsTrue(parser.Parse(model.GetType(), "Number < SubModel.Number").Invoke(model));

            const string expression =
                "Flag == true " +
                    "&& (" +
                            "(Text != \"hello parser\" && Date < SubModel.Date) " +
                            "|| (Number >= 0  && Number < 1)" +
                        ")";

            Assert.IsTrue(parser.Parse(model.GetType(), expression).Invoke(model));
        }
    }
}
