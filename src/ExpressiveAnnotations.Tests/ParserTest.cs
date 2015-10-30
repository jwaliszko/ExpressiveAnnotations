using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressiveAnnotations.Analysis;
using ExpressiveAnnotations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressiveAnnotations.Tests
{
    public class Utility
    {
        public const string Const = "outside";

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

    public enum Dog
    {
        Collie,
        Spaniel,
        Terrier
    }

    public enum HotDog
    {
        Beef,
        Pork,
        Other
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

    public enum IntEnum
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
        public enum Vehicle
        {
            Car,
            Truck,
            Uncertain,
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

            Assert.IsTrue(parser.Parse<object>("true != !true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("!!true != !!!true").Invoke(null));

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
            Assert.IsFalse(parser.Parse<object>("null != null").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' == ' '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' != '  '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'asd' == 'asd'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' != null").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("'a' + 'b' == 'ab'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'' + '' == ''").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("' ' + null == ' '").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("'a' + 0 + 'ab' + null + 'abc' == 'a0ababc'").Invoke(null));

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

            Assert.IsTrue(parser.Parse<object>("0e0 == 0").Invoke(null));
            Assert.IsTrue(parser.Parse<object>(".2 == 0.2").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("3.14 == 3.14").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("5e6 == 5000000").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("5e-6 == 5E-06").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("5e+6 == 5000000").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("9.0E-10 == 9E-10").Invoke(null));
            Assert.IsTrue(parser.Parse<object>(".11e10 == 1100000000").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1+2==3").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1-2==-1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1*2>-1").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("1/2==0.5").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("-1*-+- -+-1==-1").Invoke(null)); // weird construction, but since C# and JavaScript allows it, our language also doesn't mind
            Assert.IsTrue(parser.Parse<object>("- - -1+'a'+'b'+null+''+'c'+1+2=='-1abc12'").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("1 - 2 -(6 / ((2*1.5 - 1) + 1)) * -2 + 1/2/1 == 3.50").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("-.11e-10+.11e-10==.0-.0").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("'abc' == Trim(' abc ')").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Length(null) + Length('abc' + 'cde') >= Length(Trim(' abc def ')) - 2 - -1").Invoke(null));
        }

        [TestMethod]
        public void verify_logic_with_context()
        {
            var now = DateTime.Now;
            var model = new Model
            {
                NDate = now,
                Date = now,
                NSpan = now - new DateTime(1999, 1, 1),
                Span = new TimeSpan(0),
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
                Array = new[]
                {
                    new Model {Number = -1, Array = new[] {new Model {Number = -2}}},
                    new Model {Number = 1, Array = new[] {new Model {Number = 2}}}
                },
                Collection = new CustomCollection<Model>(),                
                SubModel = new Model
                {
                    NDate = now.AddDays(1),
                    Date = now.AddDays(1),
                    Number = 1,
                    Flag = false,
                    Text = " hello world ",
                    PoliticalStability = null,
                }
            };

            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse(model.GetType(), "Number < 1").Invoke(model));
            Assert.IsTrue(parser.Parse(model.GetType(), "Number == 0").Invoke(model));
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

            Assert.IsTrue(parser.Parse<Model>("Const != Utility.Const").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("Flag").Invoke(model));
            Assert.IsFalse(parser.Parse<Model>("!Flag").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Flag && true").Invoke(model));

            Assert.IsFalse(parser.Parse<Model>("SubModel.Flag").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("!SubModel.Flag").Invoke(model));
            Assert.IsFalse(parser.Parse<Model>("SubModel.Flag && true").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("Number < SubModel.Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Date <= NDate").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("NDate != null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Span <= NSpan").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("NSpan != null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("SubModel.Date < NextWeek()").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("IncNumber(0) == SubModel.Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("IncNumber(Number) == SubModel.Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Today() - Today() == Span").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Today() - Today() == Date - Date").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Today() - Today() == Span - Span").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Today() - Today() == NSpan - NSpan").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Span + NSpan == NSpan + Span").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Now() - Span > Date - NSpan").Invoke(model));            

            Assert.IsTrue(parser.Parse<Model>("DecNumber(SubModel.Number) == Number").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("DecNumber(Number) == 0").Invoke(model.SubModel));

            Assert.IsTrue(parser.Parse<Model>("SubModel.Date > Today()").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("'hello world' == Trim(SubModel.Text)").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("CompareOrdinal(Text, Trim(SubModel.Text)) == 0").Invoke(model));

            Assert.IsTrue(parser.Parse<Model>("Guid1 != Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Guid2 == Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Guid1 != Guid2").Invoke(model));

            model.NDate = null;
            model.NSpan = null;
            Assert.IsTrue(parser.Parse<Model>("NDate == null && Date != NDate").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("NSpan == null && Span != NSpan").Invoke(model));

            var subModel = new Model();
            var newModel = new Model {SubModel = subModel, SubModelObject = subModel};
            Assert.IsTrue(parser.Parse<Model>("SubModel == SubModelObject").Invoke(newModel));

            const string expression =
                @"Flag == !false
                      && (
                             (Text != 'hello world' && Date < SubModel.Date)
                             || (
                                    (Number >= 0 && Number < 1) && PoliticalStability == Utility.Stability.High
                                )
                         ) 
                      && Const + Utility.Const == 'insideoutside'";
            var func = parser.Parse(model.GetType(), expression);
            Assert.IsTrue(func(model));

            parser.GetFields()["Flag"] = null; // try to mess up with internal fields - original data should not be affected
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

            parser.GetConsts()["Const"] = null; // try to mess up with internal fields - original data should not be affected
            var parsedConsts = parser.GetConsts();            
            var expectedConsts = new Dictionary<string, object>
            {
                {"Utility.Stability.High", Utility.Stability.High},
                {"Const", Model.Const},
                {"Utility.Const", Utility.Const}
            };
            Assert.AreEqual(expectedConsts.Count, parsedConsts.Count);
            Assert.IsTrue(
                expectedConsts.Keys.All(
                    key => parsedConsts.ContainsKey(key) &&
                           EqualityComparer<object>.Default.Equals(expectedConsts[key], parsedConsts[key])));

            Assert.IsTrue(parser.Parse<Model>("Array[0] != null && Array[1] != null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Array[0].Number + Array[0].Array[0].Number + Array[1].Number + Array[1].Array[0].Number == 0").Invoke(model));

            model.Collection[0] = new Model {Number = -1, Collection = new CustomCollection<Model>()};
            model.Collection[0].Collection[0] = new Model {Number = -2};
            model.Collection[1] = new Model {Number = 1, Collection = new CustomCollection<Model>()};
            model.Collection[1].Collection[0] = new Model {Number = 2};

            Assert.IsTrue(parser.Parse<Model>("Collection[0] != null && Collection[1] != null").Invoke(model));
            Assert.IsTrue(parser.Parse<Model>("Collection[0].Number + Collection[0].Collection[0].Number + Collection[1].Number + Collection[1].Collection[0].Number == 0").Invoke(model));
        }

        [TestMethod]
        public void verify_logic_with_derived_context()
        {
            var parser = new Parser();
            var firstDerived = new FirstDerived {Value = true};
            var secondDerived = new SecondDerived {Value = true};

            Assert.IsTrue(parser.Parse(firstDerived.GetType(), "Value").Invoke(firstDerived));
            Assert.IsTrue(parser.Parse(secondDerived.GetType(), "Value").Invoke(secondDerived));

            Assert.IsTrue(parser.Parse(typeof(ModelBase), "Value").Invoke(firstDerived));
            Assert.IsTrue(parser.Parse(typeof(ModelBase), "Value").Invoke(secondDerived));
        }

        [TestMethod]
        public void verify_non_bool_expression_failure()
        {
            var parser = new Parser();

            try
            {
                parser.Parse<object>("1").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message.StartsWith("Parse fatal error."));
            }
        }

        [TestMethod]
        public void verify_methods_overloading() // overloading concept exists, when there are two methods of the same name, but different signature
        {
            // methods overloading is based on the number of arguments
            var parser = new Parser();
            parser.AddFunction("Whoami", () => "utility method");
            parser.AddFunction<int, string>("Whoami", i => string.Format("utility method {0}", i));
            parser.AddFunction<int, string, string>("Whoami", (i, s) => string.Format("utility method {0} - {1}", i, s));

            Assert.IsTrue(parser.Parse<object>("Whoami() == 'utility method'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Whoami(1) == 'utility method 1'").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Whoami(2, 'final') == 'utility method 2 - final'").Invoke(null));
        }

        [TestMethod]
        public void verify_methods_overriding() // overriding concept exists, when there are two methods of the same name and signature, but different implementation
        {
            var parser = new Parser();

            // register utility methods
            parser.AddFunction("Whoami", () => "utility method");
            parser.AddFunction<int, string>("Whoami", i => string.Format("utility method {0}", i));

            var model = new ModelWithMethods();

            // redefined model methods take precedence
            Assert.IsTrue(parser.Parse<ModelWithMethods>("Whoami() == 'model method'").Invoke(model));
            Assert.IsTrue(parser.Parse<ModelWithMethods>("Whoami(1) == 'model method 1'").Invoke(model));
        }

        [TestMethod]
        public void verify_methods_ambiguity()
        {
            // since arguments types are not taken under consideration for methods overloading, following logic should fail
            var parser = new Parser();
            parser.AddFunction<int, string>("Whoami", i => string.Format("utility method {0}", i));
            parser.AddFunction<string, string>("Whoami", s => string.Format("utility method {0}", s));

            parser.AddFunction<string, string, string>("Glue", (s1, s2) => string.Concat(s1, s2));
            parser.AddFunction<int, int, string>("Glue", (i1, i2) => string.Concat(i1, i2));

            try
            {
                Assert.IsTrue(parser.Parse<object>("Whoami(0) == 'utility method 0'").Invoke(null));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Whoami(0) == 'utility method 0' ...
    ^--- Function 'Whoami' accepting 1 argument is ambiguous.",
                    e.Message);
            }

            try
            {
                Assert.IsTrue(parser.Parse<object>("Glue('a', 'b') == 'ab'").Invoke(null));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Glue('a', 'b') == 'ab' ...
    ^--- Function 'Glue' accepting 2 arguments is ambiguous.",
                    e.Message);
            }

            // not only built-in, but also context extracted methods are subjected to the same rules
            parser = new Parser();
            var model = new ModelWithAmbiguousMethods();

            try
            {
                Assert.IsTrue(parser.Parse<ModelWithAmbiguousMethods>("Whoami(0) == 'model method 0'").Invoke(model));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Whoami(0) == 'model method 0' ...
    ^--- Function 'Whoami' accepting 1 argument is ambiguous.",
                    e.Message);
            }
        }

        [TestMethod]
        public void verify_implicit_type_conversion()
        {
            var parser = new Parser();
            parser.AddFunction<object, string>("Whoami", o => string.Format("utility method {0}", o));
            parser.AddFunction<int, string, string>("Whoami", (i, s) => string.Format("utility method {0} - {1}", i, s));

            Assert.IsTrue(parser.Parse<object>("Whoami('0') == 'utility method 0'").Invoke(null)); // successful conversion from String to Object
            Assert.IsTrue(parser.Parse<object>("Whoami(1, '2') == 'utility method 1 - 2'").Invoke(null)); // types matched, no conversion needed

            try
            {
                Assert.IsTrue(parser.Parse<object>("Whoami('1', '2') == 'utility method 1 - 2'").Invoke(null));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 8:
... '1', '2') == 'utility method 1 - 2' ...
    ^--- Function 'Whoami' 1st argument implicit conversion from 'System.String' to expected 'System.Int32' failed.",
                    e.Message);
            }

            try
            {
                Assert.IsTrue(parser.Parse<object>("Whoami(1, 2) == 'utility method 1 - 2'").Invoke(null));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 11:
... 2) == 'utility method 1 - 2' ...
    ^--- Function 'Whoami' 2nd argument implicit conversion from 'System.Int32' to expected 'System.String' failed.",
                    e.Message);
            }
        }

        [TestMethod]
        public void verify_short_circuit_evaluation()
        {
            var parser = new Parser();
            parser.AddFunction<object, bool>("CastToBool", obj => (bool) obj);

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
            var parser = new Parser();

            // Ensure that this doesn't consider Dog and HotDog enums to be ambiguous
            Assert.IsTrue(parser.Parse<object>("Dog.Collie == 0").Invoke(null));

            try
            {
                parser.Parse<object>("Stability.High == 0").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Stability.High == 0 ...
    ^--- Enum 'Stability' is ambiguous, found following:
'ExpressiveAnnotations.Tests.Utility+Stability',
'ExpressiveAnnotations.Tests.Stability'.",
                    e.Message);
            }
        }

        [TestMethod]
        public void verify_invalid_func_identifier()
        {
            var parser = new Parser();
            var model = new Model();

            try
            {
                parser.Parse<Model>("NotMe == 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... NotMe == 0 ...
    ^--- Only public properties, constants and enums are accepted. Identifier 'NotMe' not known.",
                    e.Message);
            }
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
        public void verify_toolchain_methods_logic()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            Assert.IsTrue(parser.Parse<object>("Now() > Today()").Invoke(null));
            Assert.IsTrue(parser.Parse<object>("Date(1985, 2, 20) < Date(1985, 2, 20, 0, 0, 1)").Invoke(null));

            Assert.IsTrue(parser.Parse<object>("TimeSpan(1, 0, 0, 0) > TimeSpan(0, 1, 0, 0)").Invoke(null));
            //Assert.IsTrue(parser.Parse<object>("(TimeSpan(0, 0, 0, 0)).TotalMilliseconds == 0").Invoke(null)); // foo().Prop - to be supported?
            //Assert.IsTrue(parser.Parse<object>("(TimeSpan(1, 2, 3, 4)).TotalMilliseconds == 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000").Invoke(null));

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

            Assert.IsTrue(parser.Parse<object>(@"IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == true").Invoke(null));
            Assert.IsTrue(parser.Parse<object>(@"IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>(@"IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.IsTrue(parser.Parse<object>(@"IsRegexMatch('John\'s cat named ""\\\'""\n (Backslash Quote)', '^\\d+$') == false").Invoke(null));
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

        [TestMethod]
        public void verify_various_parsing_errors()
        {
            var parser = new Parser();
            parser.RegisterMethods();
            parser.AddFunction<int, int, int>("Max", (x, y) => Math.Max(x, y));

            try
            {
                parser.Parse<object>("1++ +1==2").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 2:
... ++ +1==2 ...
    ^--- Unexpected token: '++'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("true # false").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... # false ...
    ^--- Invalid token.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("'abc' - 'abc'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... - 'abc' ...
    ^--- Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("0 + null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 3:
... + null ...
    ^--- Operator '+' cannot be applied to operands of type 'System.Int32' and 'null'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("0 / null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 3:
... / null ...
    ^--- Operator '/' cannot be applied to operands of type 'System.Int32' and 'null'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("true && null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... && null ...
    ^--- Operator '&&' cannot be applied to operands of type 'System.Boolean' and 'null'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("true || null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... || null ...
    ^--- Operator '||' cannot be applied to operands of type 'System.Boolean' and 'null'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("!null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... !null ...
    ^--- Operator '!' cannot be applied to operand of type 'null'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("'abc' * 'abc'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... * 'abc' ...
    ^--- Operator '*' cannot be applied to operands of type 'System.String' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("1 + 2 + 'abc' - 'abc' > 0").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 15:
... - 'abc' > 0 ...
    ^--- Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>(
                    @"1 - 2
    - (6 / ((2*'1.5' - 1) + 1)) * -2 
    + 1/2/1 == 3.50").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 2, column 15:
... *'1.5' - 1) + 1)) * -2 ...
    ^--- Operator '*' cannot be applied to operands of type 'System.Int32' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>(
                    @"1 - 2
    - 6
    + 1/x/1 == 3.50").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 3, column 9:
... x/1 == 3.50 ...
    ^--- Only public properties, constants and enums are accepted. Identifier 'x' not known.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("WriteLine('hello')").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... WriteLine('hello') ...
    ^--- Function 'WriteLine' not known.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("Max(1.1)").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Max(1.1) ...
    ^--- Function 'Max' accepting 1 argument not found.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("Max(1.1, 1.2, 'a')").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Max(1.1, 1.2, 'a') ...
    ^--- Function 'Max' accepting 3 arguments not found.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>(
                    @"Max(1, 
      Max(1, 'a')) == 1.1").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 2, column 14:
... 'a')) == 1.1 ...
    ^--- Function 'Max' 2nd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("Now() && Today()").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... && Today() ...
    ^--- Operator '&&' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("'a' >= 'b'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 5:
... >= 'b' ...
    ^--- Operator '>=' cannot be applied to operands of type 'System.String' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("!'a'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... !'a' ...
    ^--- Operator '!' cannot be applied to operand of type 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("!! Today()").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 2:
... ! Today() ...
    ^--- Operator '!' cannot be applied to operand of type 'System.DateTime'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("0 == '0'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 3:
... == '0' ...
    ^--- Operator '==' cannot be applied to operands of type 'System.Int32' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("0.1 != '0'").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 5:
... != '0' ...
    ^--- Operator '!=' cannot be applied to operands of type 'System.Double' and 'System.String'.",
                    e.Message);
            }

            try
            {
                parser.Parse<object>("'asd' > null").Invoke(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... > null ...
    ^--- Operator '>' cannot be applied to operands of type 'System.String' and 'null'.",
                    e.Message);
            }

            try
            {                
                var model = new Model();
                parser.Parse<Model>("Date + Date").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... + Date ...
    ^--- Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("NDate + NDate").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... + NDate ...
    ^--- Operator '+' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Nullable`1[System.DateTime]'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date + NDate").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... + NDate ...
    ^--- Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.Nullable`1[System.DateTime]'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date == 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... == 0 ...
    ^--- Operator '==' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date != 'asd'").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... != 'asd' ...
    ^--- Operator '!=' cannot be applied to operands of type 'System.DateTime' and 'System.String'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date > 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... > 0 ...
    ^--- Operator '>' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date > null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... > null ...
    ^--- Operator '>' cannot be applied to operands of type 'System.DateTime' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date > SubModelObject").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... > SubModelObject ...
    ^--- Operator '>' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.",
                    e.Message);
            }

            try
            {
                var model = new Model();                
                parser.Parse<Model>("NDate > SubModelObject").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... > SubModelObject ...
    ^--- Operator '>' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Object'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("NDate > null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... > null ...
    ^--- Operator '>' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("NDate != 'asd'").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... != 'asd' ...
    ^--- Operator '!=' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.String'.",
                    e.Message);
            }

            try
            {
                var bag = new Bag {Lexer = new Lexer(), Parser = new Parser()};
                parser.Parse<Bag>("Lexer != Parser").Invoke(bag);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... != Parser ...
    ^--- Operator '!=' cannot be applied to operands of type 'ExpressiveAnnotations.Analysis.Lexer' and 'ExpressiveAnnotations.Analysis.Parser'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("SubModelObject > SubModelObject").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 16:
... > SubModelObject ...
    ^--- Operator '>' cannot be applied to operands of type 'System.Object' and 'System.Object'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null > null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... > null ...
    ^--- Operator '>' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null + null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... + null ...
    ^--- Operator '+' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null - null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... - null ...
    ^--- Operator '-' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("'asd' - null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... - null ...
    ^--- Operator '-' cannot be applied to operands of type 'System.String' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null / null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... / null ...
    ^--- Operator '/' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null && null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... && null ...
    ^--- Operator '&&' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null || null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... || null ...
    ^--- Operator '||' cannot be applied to operands of type 'null' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null == 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... == 0 ...
    ^--- Operator '==' cannot be applied to operands of type 'null' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("null < 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... < 0 ...
    ^--- Operator '<' cannot be applied to operands of type 'null' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Cash == null").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... == null ...
    ^--- Operator '==' cannot be applied to operands of type 'System.Decimal' and 'null'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Date + 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... + 0 ...
    ^--- Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("NDate - 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... - 0 ...
    ^--- Operator '-' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("Span + 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 6:
... + 0 ...
    ^--- Operator '+' cannot be applied to operands of type 'System.TimeSpan' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model();
                parser.Parse<Model>("NSpan - 0").Invoke(model);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 7:
... - 0 ...
    ^--- Operator '-' cannot be applied to operands of type 'System.Nullable`1[System.TimeSpan]' and 'System.Int32'.",
                    e.Message);
            }

            try
            {
                var model = new Model {Items = new List<Model> {new Model()}};
                Assert.IsTrue(parser.Parse<Model>("Items[0] != null").Invoke(model));
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.AreEqual(
                    @"Parse error on line 1, column 1:
... Items[0] != null ...
    ^--- Identifier 'Items' either does not represent an array type or does not declare indexer.",
                    e.Message);
            }
        }

        private class Model
        {
            public const string Const = "inside";

            public object SubModelObject { get; set; }
            public Model SubModel { get; set; }

            public DateTime? NDate { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan? NSpan { get; set; }
            public TimeSpan Span { get; set; }
            public int? Number { get; set; }
            public bool Flag { get; set; }
            public string Text { get; set; }
            public decimal Cash { get; set; }
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

            public Model[] Array { get; set; } // array
            public IEnumerable<Model> Items { get; set; } // collection without indexer
            public CustomCollection<Model> Collection { get; set; } // collection with indexer, like e.g List<>

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

        private class ModelWithAmbiguousMethods
        {
            public string Whoami(string s)
            {
                return string.Format("model method {0}", s);
            }

            public string Whoami(int i)
            {
                return string.Format("model method {0}", i);
            }
        }

        private class ModelWithMethods
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

                Assert.IsTrue(0 == (int) Vehicle.Car);
                Assert.IsTrue(Vehicle == Vehicle.Car);
            }

            public Vehicle Vehicle { get; set; }
        }

        public class SampleTwo
        {
            public SampleTwo()
            {
                Vehicle = new Carriage {Car = -1};

                Assert.IsTrue(-1 == Vehicle.Car);
                Assert.IsTrue(Vehicle.Car != (int) ParserTest.Vehicle.Car);
            }

            public Carriage Vehicle { get; set; }
        }

        public class Bag
        {
            public Parser Parser { get; set; }
            public Lexer Lexer { get; set; }
        }

        private enum YesNo
        {
            Yes,
            No,
            Uncertain
        }

        private abstract class ModelBase
        {
            public bool Value { get; set; }
        }

        private class FirstDerived : ModelBase { }
        private class SecondDerived : ModelBase { }

        private class CustomCollection<T>
        {
            private readonly T[] _elements = new T[100]; // backing store

            [IndexerName("Element")] // change the default indexer property name (Item)
            public T this[int index]
            {
                get { return _elements[index]; }
                set { _elements[index] = value; }
            }
        }
    }
}
