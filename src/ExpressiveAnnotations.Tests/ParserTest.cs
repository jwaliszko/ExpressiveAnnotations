using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    public class Utility
    {
        public enum Stability
        {
            High,
            Low,
            Uncertain
        }

        public enum StabilityBytes : byte
        {
            High,
            Low,
            Uncertain
        }
    }

    internal enum Stability
    {
        Good,
        Bad,
        Unknown
    }

    [TestClass]
    public class ParserTest
    {
        private enum YesNo
        {
            Yes,
            No,
            Uncertain,
        }

        private class Model
        {
            public Model SubModel { get; set; }

            public DateTime Date { get; set; }
            public int? Number { get; set; }
            public bool Flag { get; set; }
            public string Text { get; set; }
            public byte SmallerNumber { get; set; }
            public Utility.Stability? PoliticalStability { get; set; }
            public Utility.StabilityBytes? PoliticalStabilityBytes { get; set; }

            public DateTime NextWeek()
            {
                return DateTime.Now.AddDays(7);
            }

            public int IncNumber(int number)
            {
                return ++number;
            }

            public int DecNumber(int number)
            {
                return --number;
            }
        }

        [TestMethod]
        public void verify_logic_without_context()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse<object>("YesNo.Yes == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("YesNo.Yes < YesNo.No").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("YesNo.Yes - YesNo.No == -1").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("!true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("!false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("false == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("true != false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("!true == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("!!true == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("!!!true == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true && true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false && false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("true && false").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false && true").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("true || true").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("false || false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("true || false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("false || true").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("0 == 0 && 1 < 2").Invoke(null));

            Assert.IsFalse(parser.Parse<object>("0 != 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 >= 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 <= 0").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("0 == 0").Invoke(null));

            Assert.IsFalse(parser.Parse<object>("0 == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("-1 == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 != 1").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("0 >= 1").Invoke(null));
            Assert.IsFalse(parser.Parse<object>("0 > 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 <= 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("0 < 1").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 + 2 == 3").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1 - 2 == -1").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("null == null").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' == ' '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' != '  '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'asd' == 'asd'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' != null").Invoke(null));
            
            Assert.IsTrue(parser.Parse<object>("'a' + 'b' == 'ab'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' + '' == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' + null == ' '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'a' + 0 + 'ab' + null + 'abc' == \"a0ababc\"").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 + 2 + 3 + 4 == 10").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1 - 2 + 3 - 4 == -2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("-1 - 2 - 3 - 4 == -10").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("3 - (4 - 5) == 4").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1 - -1 == 2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("(1 - 2) + ((3 - 4) - 5) == -7").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 * 2 == 2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1 * 2 * 3 == 6").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("2 / 2 == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("4 / 2 / 2 == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("2 * 2 / 2 == 2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("4 / 2 * 2 == 4").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1.2 * 2 == 2.4").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1.2 / 2 == 0.6").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1.2 + 2 == 3.2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1.2 - 2 == -0.8").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 - 2 -(6 / ((2*1.5 - 1) + 1)) * -2 + 1/2/1 == 3.50").Invoke(null));            

            Assert.IsTrue(parser.Parse<Model>("'abc' == Trim(' abc ')").Invoke(null));
            Assert.IsTrue(parser.Parse<Model>("CompareOrdinal('a', 'a') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<Model>("Length('abc' + 'cde') >= Length(Trim(' abc def ')) - 2 - -1").Invoke(null));
        }

        [TestMethod]
        public void verify_logic_with_context()
        {
            var model = new Model
            {
                Date = DateTime.Now,
                Number = 0,
                Flag = true,
                Text = "hello world",
                SmallerNumber = 13,
                PoliticalStability = Utility.Stability.High,
                PoliticalStabilityBytes = Utility.StabilityBytes.High,

                SubModel = new Model
                {
                    Date = DateTime.Now.AddDays(1),
                    Number = 1,
                    Flag = false,
                    Text = " hello world ",
                    SmallerNumber = 1,
                    PoliticalStability = null,
                    PoliticalStabilityBytes = null,
                }
            };

            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse(model.GetType(), "Number != null").Invoke(model));
            Assert.IsTrue(parser.Parse(model.GetType(), "SubModel.Number / 2 == 0.5").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("SmallerNumber != 1").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SmallerNumber == 13").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.SmallerNumber / 2 == 0.5").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("PoliticalStability == 0").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStability == Utility.Stability.High").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStability < Utility.Stability.Low").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStability == null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStability != Utility.Stability.High").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("PoliticalStabilityBytes == 0").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStabilityBytes == Utility.StabilityBytes.High").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStabilityBytes < Utility.StabilityBytes.Low").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStabilityBytes == null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStabilityBytes != Utility.StabilityBytes.High").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("Flag").Invoke(model));
            Assert.IsFalse(parser.Parse<Model>("!Flag").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Flag && true").Invoke(model));

            Assert.IsFalse(parser.Parse<Model>("SubModel.Flag").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("!SubModel.Flag").Invoke(model));
            Assert.IsFalse(parser.Parse<Model>("SubModel.Flag && true").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("Number < SubModel.Number").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("SubModel.Date < NextWeek()").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("IncNumber(0) == SubModel.Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("IncNumber(Number) == SubModel.Number").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("DecNumber(SubModel.Number) == Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("DecNumber(Number) == 0").Invoke(model.SubModel));

            Assert.IsTrue(parser.Parse<Model>("SubModel.Date > Today()").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("'hello world' == Trim(SubModel.Text)").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("CompareOrdinal(Text, Trim(SubModel.Text)) == 0").Invoke(model));

            const string expression =
                "Flag == true " +
                    "&& (" +
                            "(Text != \"hello world\" && Date < SubModel.Date) " +
                            "|| ((Number >= 0 && Number < 1) && PoliticalStability == Utility.Stability.High" +
                            " && PoliticalStabilityBytes == Utility.StabilityBytes.High)" +
                        ")";
            var func = parser.Parse(model.GetType(), expression);
            Assert.IsTrue(func(model));

            var parsedFields = parser.GetFields();
            var expectedFields = new Dictionary<string, Type>
            {
                {"Flag", typeof (bool)},
                {"Text", typeof (string)},
                {"Date", typeof (DateTime)},
                {"SubModel.Date", typeof (DateTime)},
                {"Number", typeof (int?)},
                {"PoliticalStability", typeof (Utility.Stability?)},
                {"PoliticalStabilityBytes", typeof (Utility.StabilityBytes?)}
            };
            Assert.AreEqual(expectedFields.Count, parsedFields.Count);
            Assert.IsTrue(
                expectedFields.Keys.All(
                    key => parsedFields.ContainsKey(key) &&
                           EqualityComparer<Type>.Default.Equals(expectedFields[key], parsedFields[key])));

            var parsedConsts = parser.GetConsts();
            var expectedConsts = new Dictionary<string, object>
            {
                {"Utility.Stability.High", Utility.Stability.High},
                {"Utility.StabilityBytes.High", Utility.StabilityBytes.High}
            };
            Assert.AreEqual(expectedConsts.Count, parsedConsts.Count);
            Assert.IsTrue(
                expectedConsts.Keys.All(
                    key => parsedConsts.ContainsKey(key) &&
                           EqualityComparer<object>.Default.Equals(expectedConsts[key], parsedConsts[key])));
        }

        [TestMethod]
        public void verify_short_circuit_evaluation()
        {
            var parser = new Parser();
            parser.AddFunction<string, bool>("NonEmpty", str => str.Length > 0);

            try
            {
                parser.Parse<object>("NonEmpty(null)").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // below, the exception should not be thrown as above
            // reason? - first argument is suffient to determine the value of the expression so the second one is not going to be evaluated
            Assert.IsFalse(parser.Parse<object>("false && NonEmpty(null)").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("true || NonEmpty(null)").Invoke(null));
        }

        [TestMethod]
        public void verify_enumeration_ambiguity()
        {
            var parser = new Parser();

            try
            {
                parser.Parse<object>("Stability.High == 0").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    "Parsing failed. Invalid expression: Stability.High == 0",
                    e.Message);

                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    "Enum Stability is ambiguous, found following:" + Environment.NewLine +
                    "ExpressiveAnnotations.Tests.Utility+Stability" + Environment.NewLine +
                    "ExpressiveAnnotations.Tests.Stability",
                    e.InnerException.Message);
            }
        }

        public enum Vehicle
        {
            Car,
            Truck
        }

        public class Carriage
        {
            public int Car { get; set; }
            public int Truck { get; set; }
        }

        public class SampleOne
        {
            public SampleOne()
            {
                Vehicle = Vehicle.Car;

                Assert.IsTrue(0 == (int)Vehicle.Car);
                Assert.IsTrue(Vehicle == Vehicle.Car);
            }

            public Vehicle Vehicle { get; set; }
        }

        public class SampleTwo
        {
            public SampleTwo()
            {
                Vehicle = new Carriage { Car = -1 };

                Assert.IsTrue(-1 == Vehicle.Car);
                Assert.IsTrue(Vehicle.Car != (int)ParserTest.Vehicle.Car);
            }

            public Carriage Vehicle { get; set; }
        }

        [TestMethod]
        public void verify_naming_collisions()
        {
            var parser = new Parser();

            var one = new SampleOne();
            var two = new SampleTwo();

            Assert.IsTrue(parser.Parse<SampleOne>("0 == Vehicle.Car").Invoke(one));
            Assert.AreEqual(0, parser.GetFields().Count);
            Assert.AreEqual(1, parser.GetConsts().Count);
            Assert.IsTrue(parser.Parse<SampleOne>("Vehicle == Vehicle.Car").Invoke(one));
            Assert.AreEqual(1, parser.GetFields().Count);
            Assert.AreEqual(1, parser.GetConsts().Count);

            Assert.IsTrue(parser.Parse<SampleTwo>("-1 == Vehicle.Car").Invoke(two));
            Assert.AreEqual(1, parser.GetFields().Count);
            Assert.AreEqual(0, parser.GetConsts().Count);
            Assert.IsTrue(parser.Parse<SampleTwo>("Vehicle.Car != ParserTest.Vehicle.Car").Invoke(two));
            Assert.AreEqual(1, parser.GetFields().Count);
            Assert.AreEqual(1, parser.GetConsts().Count);
        }
    }
}
