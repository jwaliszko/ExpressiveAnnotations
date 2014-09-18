using System;
using System.Collections.Generic;
using System.Linq;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    public class Utility
    {
        public const string CONST = "outside";

        public enum Stability
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

    public enum SbyteEnum : sbyte
    {
        First = 1,
        Second = 2
    }
    public enum ByteEnum : byte
    {
        First = 1,
        Second = 2
    }
    public enum ShortEnum : short
    {
        First = 1,
        Second = 2
    }
    public enum UshortEnum : ushort
    {
        First = 1,
        Second = 2
    }
    public enum IntEnum : int
    {
        First = 1,
        Second = 2
    }
    public enum UintEnum : uint
    {
        First = 1,
        Second = 2
    }
    public enum LongEnum : long
    {
        First = 1,
        Second = 2
    }
    public enum UlongEnum : ulong
    {
        First = 1,
        Second = 2
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
            public const string CONST = "inside";

            public Model SubModel { get; set; }

            public DateTime Date { get; set; }
            public int? Number { get; set; }
            public bool Flag { get; set; }
            public string Text { get; set; }
            public Utility.Stability? PoliticalStability { get; set; }

            public SbyteEnum? SbyteNumber { get; set; }
            public ByteEnum? ByteNumber { get; set; }
            public ShortEnum? ShortNumber { get; set; }
            public UshortEnum? UshortNumber { get; set; }
            public IntEnum? IntNumber { get; set; }
            public UintEnum? UintNumber { get; set; }
            public LongEnum? LongNumber { get; set; }
            public UlongEnum? UlongNumber { get; set; }

            public Guid? Guid1 { get; set; }
            public Guid? Guid2 { get; set; }

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

        private class ModelWithoutMethod
        {
        }

        private class ModelWithMethod
        {
            public string Whoami()
            {
                return "model method";
            }

            public string Whoami(int i)
            {
                return string.Format("model method {0}", i);
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
            Assert.IsTrue(parser.Parse<object>("2 - 1 > 0").Invoke(null));

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

            //ToDo: modify engine to handle this
            //Assert.IsTrue(parser.Parse<object>("1+2==3").Invoke(null));
            //Assert.IsTrue(parser.Parse<object>("1-2==-1").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("1*2>-1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1/2==0.5").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 - 2 -(6 / ((2*1.5 - 1) + 1)) * -2 + 1/2/1 == 3.50").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("\"abc\" == Trim(\" abc \")").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("Length(null) + Length('abc' + 'cde') >= Length(Trim(' abc def ')) - 2 - -1").Invoke(null));

            try
            {
                parser.Parse<object>("1").Invoke(null); // non-bool exp
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    "Parsing failed. Invalid expression: 1",
                    e.Message);

                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is ArgumentException);
                Assert.AreEqual(
                    "Expression of type 'System.Int32' cannot be used for return type 'System.Boolean'",
                    e.InnerException.Message);
            }
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
                PoliticalStability = Utility.Stability.High,

                SbyteNumber = SbyteEnum.First,
                ByteNumber = ByteEnum.First,
                ShortNumber = ShortEnum.First,
                UshortNumber = UshortEnum.First,
                IntNumber = IntEnum.First,
                UintNumber = UintEnum.First,
                LongNumber = LongEnum.First,
                UlongNumber = UlongEnum.First,
                Guid1 = Guid.NewGuid(),
                Guid2 = Guid.Empty,

                SubModel = new Model
                {
                    Date = DateTime.Now.AddDays(1),
                    Number = 1,
                    Flag = false,
                    Text = " hello world ",
                    PoliticalStability = null,
                }
            };

            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse(model.GetType(), "Number != null").Invoke(model));
            Assert.IsTrue(parser.Parse(model.GetType(), "SubModel.Number / 2 == 0.5").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("SbyteNumber / SbyteEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("ByteNumber / ByteEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("ShortNumber / ShortEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("UshortNumber / UshortEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("IntNumber / IntEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("UintNumber / UintEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("LongNumber / LongEnum.Second == 0.5").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("UlongNumber / UlongEnum.Second == 0.5").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("PoliticalStability == 0").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStability == Utility.Stability.High").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("PoliticalStability < Utility.Stability.Low").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStability == null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.PoliticalStability != Utility.Stability.High").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("CONST != Utility.CONST").Invoke(model));

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

            Assert.IsTrue(parser.Parse<Model>("Guid1 != Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Guid2 == Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Guid1 != Guid2").Invoke(model));

            const string expression =
                "Flag == true " +
                    "&& (" +
                            "(Text != \"hello world\" && Date < SubModel.Date) " +
                            "|| (" +
                                    "(Number >= 0 && Number < 1) && PoliticalStability == Utility.Stability.High" +
                                ")" +
                        ")" +
                    "&& CONST + Utility.CONST == 'insideoutside'";
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
                {"PoliticalStability", typeof (Utility.Stability?)}                
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
                {"CONST", Model.CONST},
                {"Utility.CONST", Utility.CONST}
            };
            Assert.AreEqual(expectedConsts.Count, parsedConsts.Count);
            Assert.IsTrue(
                expectedConsts.Keys.All(
                    key => parsedConsts.ContainsKey(key) &&
                           EqualityComparer<object>.Default.Equals(expectedConsts[key], parsedConsts[key])));            
        }

        [TestMethod]
        public void verify_methods_overriding()
        {
            var m1 = new ModelWithoutMethod();
            var m2 = new ModelWithMethod();

            var parser = new Parser();
            parser.AddFunction("Whoami", () => "utility method");
            parser.AddFunction<int, string>("Whoami", i => string.Format("utility method {0}", i));

            Assert.IsTrue(parser.Parse<ModelWithoutMethod>("Whoami() == 'utility method'").Invoke(m1));
            Assert.IsTrue(parser.Parse<ModelWithoutMethod>("Whoami(2) == 'utility method 2'").Invoke(m1));

            // model methods take precedence
            Assert.IsTrue(parser.Parse<ModelWithMethod>("Whoami() == 'model method'").Invoke(m2));
            Assert.IsTrue(parser.Parse<ModelWithMethod>("Whoami(2) == 'model method 2'").Invoke(m2));
        }

        [TestMethod]
        public void verify_short_circuit_evaluation()
        {
            var parser = new Parser();
            parser.AddFunction<object, bool>("CastToBool", obj => (bool)obj);

            try
            {
                parser.Parse<object>("CastToBool(null)").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // below, the exception should not be thrown as above
            // reason? - first argument is suffient to determine the value of the expression so the second one is not going to be evaluated
            Assert.IsFalse(parser.Parse<object>("false && CastToBool(null)").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("true || CastToBool(null)").Invoke(null));
        }

        [TestMethod]
        public void verify_enumeration_ambiguity()
        {
            try
            {
                var parser = new Parser();
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

        [TestMethod]
        public void verify_invalid_func_identifier()
        {
            try
            {
                var model = new Model();
                var parser = new Parser();

                parser.Parse<Model>("NotMe == 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    "Parsing failed. Invalid expression: NotMe == 0",
                    e.Message);

                Assert.IsNotNull(e.InnerException);
                Assert.IsTrue(e.InnerException is InvalidOperationException);
                Assert.AreEqual(
                    "Only public properties, constants and enums are accepted. Invalid identifier: NotMe",
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

        [TestMethod]
        public void verify_toolchain_methods()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse<object>("Now() > Today()").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Length('0123') == 4").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Length('    ') == 4").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Length(null) == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Length('') == 0").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Trim(' a b c ') == 'a b c'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Trim(null) == null").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Trim('') == ''").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Concat(' a ', ' b ') == ' a  b '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Concat(null, null) == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Concat('', '') == ''").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Concat(' a ', ' b ', ' c ') == ' a  b  c '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Concat(null, null, null) == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Concat('', '', '') == ''").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("CompareOrdinal(' abc ', ' ABC ') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('a', 'a') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('a', 'A') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('A', 'a') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('a', 'b') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('b', 'a') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal(null, 'a') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('a', null) == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal(' ', 'a') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('a', ' ') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal(null, '') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('', null) == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal(null, null) == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinal('', '') == 0").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase(' abc ', ' ABC ') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'a') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'A') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('A', 'a') == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'b') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('b', 'a') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase(null, 'a') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('a', null) == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase(' ', 'a') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('a', ' ') == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase(null, '') == -1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('', null) == 1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase(null, null) == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("CompareOrdinalIgnoreCase('', '') == 0").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("StartsWith(' ab c', ' A') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWith(' ab c', ' a') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWith(' ', ' ') == true").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("StartsWith('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWith(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWith('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWith(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase(' ab c', ' A') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase(' ab c', ' a') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase(' ', ' ') == true").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("StartsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("EndsWith(' ab c', ' C') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWith(' ab c', ' c') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWith(' ', ' ') == true").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("EndsWith('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWith(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWith('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWith(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase(' ab c', ' C') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase(' ab c', ' c') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase(' ', ' ') == true").Invoke(null));            
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("EndsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Contains(' ab c', 'B ') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains(' ab c', 'b ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains(' ', ' ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Contains(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase(' ab c', 'B ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase(' ab c', 'b ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("ContainsIgnoreCase(null, null) == false").Invoke(null));            

            Assert.IsTrue(parser.Parse<object>("IsNullOrWhiteSpace(' ') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsNullOrWhiteSpace(null) == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsNullOrWhiteSpace('') == true").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("IsDigitChain('0123456789') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsDigitChain(null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsDigitChain('') == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("IsNumber('-0.3e-2') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsNumber(null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsNumber('') == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("IsEmail('nickname@domain.com') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsEmail(null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsEmail('') == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("IsUrl('http://www.github.com/') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsUrl(null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsUrl('') == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch('', '') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch(null, '') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch('', null) == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("IsRegexMatch(null, null) == false").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("Guid('a1111111-1111-1111-1111-111111111111') == Guid('A1111111-1111-1111-1111-111111111111')").Invoke(null));

            try
            {
                parser.Parse<object>("Guid('abc') == Guid('abc')").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is FormatException);
                Assert.AreEqual(
                    "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).",
                    e.Message);
            }
        }
    }
}
