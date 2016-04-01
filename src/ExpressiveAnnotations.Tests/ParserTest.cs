using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressiveAnnotations.Analysis;
using Xunit;

namespace ExpressiveAnnotations.Tests
{
    public class Utility
    {
        public const string Const = "outside1";

        public enum Stability
        {
            High,
            Low,
            Uncertain
        }
    }

    public class Tools
    {
        public class Utility
        {
            public const string Const = "outside2";
        }
    }

    internal enum Stability
    {
        Good,
        Bad,
        Unknown
    }

    public class Dogs
    {
        public const int Const = 0;
    }

    public class HotDogs
    {
        public const int Const = 1;
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

    public class ParserTest
    {
        public enum Vehicle
        {
            Car,
            Truck,
            Uncertain,
        }

        [Fact]
        public void verify_logic_without_context()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            Assert.True(parser.Parse<object>("YesNo.Yes == 0").Invoke(null));
            Assert.True(parser.Parse<object>("YesNo.Yes < YesNo.No").Invoke(null));
            Assert.True(parser.Parse<object>("YesNo.Yes - YesNo.No == -1").Invoke(null));

            Assert.True(parser.Parse<object>("true").Invoke(null));
            Assert.False(parser.Parse<object>("false").Invoke(null));
            Assert.False(parser.Parse<object>("!true").Invoke(null));
            Assert.True(parser.Parse<object>("!false").Invoke(null));

            Assert.True(parser.Parse<object>("true == true").Invoke(null));
            Assert.True(parser.Parse<object>("false == false").Invoke(null));
            Assert.True(parser.Parse<object>("true != false").Invoke(null));

            Assert.True(parser.Parse<object>("!true == false").Invoke(null));
            Assert.True(parser.Parse<object>("!!true == true").Invoke(null));
            Assert.True(parser.Parse<object>("!!!true == false").Invoke(null));

            Assert.True(parser.Parse<object>("true != !true").Invoke(null));
            Assert.True(parser.Parse<object>("!!true != !!!true").Invoke(null));

            Assert.True(parser.Parse<object>("true && true").Invoke(null));
            Assert.False(parser.Parse<object>("false && false").Invoke(null));
            Assert.False(parser.Parse<object>("true && false").Invoke(null));
            Assert.False(parser.Parse<object>("false && true").Invoke(null));

            Assert.True(parser.Parse<object>("true || true").Invoke(null));
            Assert.False(parser.Parse<object>("false || false").Invoke(null));
            Assert.True(parser.Parse<object>("true || false").Invoke(null));
            Assert.True(parser.Parse<object>("false || true").Invoke(null));

            Assert.True(parser.Parse<object>("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false").Invoke(null));
            Assert.True(parser.Parse<object>("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))").Invoke(null));

            Assert.True(parser.Parse<object>("0 == 0 && 1 < 2").Invoke(null));

            Assert.False(parser.Parse<object>("0 != 0").Invoke(null));
            Assert.True(parser.Parse<object>("0 >= 0").Invoke(null));
            Assert.True(parser.Parse<object>("0 <= 0").Invoke(null));
            Assert.True(parser.Parse<object>("0 == 0").Invoke(null));
            Assert.False(parser.Parse<object>("0 == 1").Invoke(null));
            Assert.True(parser.Parse<object>("-1 == -1").Invoke(null));
            Assert.True(parser.Parse<object>("0 != 1").Invoke(null));
            Assert.False(parser.Parse<object>("0 >= 1").Invoke(null));
            Assert.False(parser.Parse<object>("0 > 1").Invoke(null));
            Assert.True(parser.Parse<object>("0 <= 1").Invoke(null));
            Assert.True(parser.Parse<object>("0 < 1").Invoke(null));

            Assert.True(parser.Parse<object>("1 + 2 == 3").Invoke(null));
            Assert.True(parser.Parse<object>("1 - 2 == -1").Invoke(null));
            Assert.True(parser.Parse<object>("2 - 1 > 0").Invoke(null));

            Assert.True(parser.Parse<object>("null == null").Invoke(null));
            Assert.False(parser.Parse<object>("null != null").Invoke(null));
            Assert.True(parser.Parse<object>("'' == ''").Invoke(null));
            Assert.True(parser.Parse<object>("' ' == ' '").Invoke(null));
            Assert.True(parser.Parse<object>("' ' != '  '").Invoke(null));
            Assert.True(parser.Parse<object>("'asd' == 'asd'").Invoke(null));
            Assert.True(parser.Parse<object>("'asd' != 'ASD'").Invoke(null));
            Assert.True(parser.Parse<object>("'' != null").Invoke(null));

            Assert.True(parser.Parse<object>("'a' + 'b' == 'ab'").Invoke(null));
            Assert.True(parser.Parse<object>("'' + '' == ''").Invoke(null));
            Assert.True(parser.Parse<object>("' ' + null == ' '").Invoke(null));
            Assert.True(parser.Parse<object>("'a' + 0 + 'ab' + null + 'abc' == 'a0ababc'").Invoke(null));

            Assert.True(parser.Parse<object>("1 + 2 + 3 + 4 == 10").Invoke(null));
            Assert.True(parser.Parse<object>("1 - 2 + 3 - 4 == -2").Invoke(null));
            Assert.True(parser.Parse<object>("-1 - 2 - 3 - 4 == -10").Invoke(null));
            Assert.True(parser.Parse<object>("3 - (4 - 5) == 4").Invoke(null));
            Assert.True(parser.Parse<object>("1 - -1 == 2").Invoke(null));
            Assert.True(parser.Parse<object>("(1 - 2) + ((3 - 4) - 5) == -7").Invoke(null));

            Assert.True(parser.Parse<object>("1 * 2 == 2").Invoke(null));
            Assert.True(parser.Parse<object>("1 * 2 * 3 == 6").Invoke(null));
            Assert.True(parser.Parse<object>("2 / 2 == 1").Invoke(null));
            Assert.True(parser.Parse<object>("4 / 2 / 2 == 1").Invoke(null));
            Assert.True(parser.Parse<object>("2 * 2 / 2 == 2").Invoke(null));
            Assert.True(parser.Parse<object>("4 / 2 * 2 == 4").Invoke(null));

            Assert.True(parser.Parse<object>("1.2 * 2 == 2.4").Invoke(null));
            Assert.True(parser.Parse<object>("1.2 / 2 == 0.6").Invoke(null));
            Assert.True(parser.Parse<object>("1.2 + 2 == 3.2").Invoke(null));
            Assert.True(parser.Parse<object>("1.2 - 2 == -0.8").Invoke(null));

            Assert.True(parser.Parse<object>("0e0 == 0").Invoke(null));
            Assert.True(parser.Parse<object>(".2 == 0.2").Invoke(null));
            Assert.True(parser.Parse<object>("3.14 == 3.14").Invoke(null));
            Assert.True(parser.Parse<object>("5e6 == 5000000").Invoke(null));
            Assert.True(parser.Parse<object>("5e-6 == 5E-06").Invoke(null));
            Assert.True(parser.Parse<object>("5e+6 == 5000000").Invoke(null));
            Assert.True(parser.Parse<object>("9.0E-10 == 9E-10").Invoke(null));
            Assert.True(parser.Parse<object>(".11e10 == 1100000000").Invoke(null));

            Assert.True(parser.Parse<object>("1+2==3").Invoke(null));
            Assert.True(parser.Parse<object>("1-2==-1").Invoke(null));
            Assert.True(parser.Parse<object>("1*2>-1").Invoke(null));
            Assert.True(parser.Parse<object>("-1*-2>-1").Invoke(null));
            Assert.True(parser.Parse<object>("1/2==0.5").Invoke(null));
            Assert.True(parser.Parse<object>("-1*-+- -+-1==-1").Invoke(null)); // weird construction, but since C# and JavaScript allows it, our language also doesn't mind
            Assert.True(parser.Parse<object>("- - -1+'a'+'b'+null+''+'c'+1+2=='-1abc12'").Invoke(null));

            Assert.True(parser.Parse<object>("1 - 2 -(6 / ((2*1.5 - 1) + 1)) * -2 + 1/2/1 == 3.50").Invoke(null));
            Assert.True(parser.Parse<object>("-.11e-10+.11e-10==.0-.0").Invoke(null));

            Assert.True(parser.Parse<object>("'abc' == Trim(' abc ')").Invoke(null));
            Assert.True(parser.Parse<object>("Length(null) + Length('abc' + 'cde') >= Length(Trim(' abc def ')) - 2 - -1").Invoke(null));
            Assert.True(parser.Parse<object>("0 == YesNo.Yes").Invoke(null));
            Assert.True(parser.Parse<object>("YesNo.Yes == 0").Invoke(null));
            Assert.True(parser.Parse<object>("YesNo.Yes != YesNo.No").Invoke(null));
        }

        [Fact]
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
                NFlag = true,
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
                Guid = Guid.NewGuid(),
                NGuid1 = Guid.NewGuid(),
                NGuid2 = Guid.Empty,
                InsensString = new StringInsens("asd"),
                NInsensString = new StringInsens("ASD"),
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
                    NFlag = false,
                    Text = " hello world ",
                    PoliticalStability = null,
                }
            };

            var parser = new Parser();
            parser.RegisterMethods();

            Assert.True(parser.Parse(model.GetType(), "Number < 1").Invoke(model));
            Assert.True(parser.Parse(model.GetType(), "Number == 0").Invoke(model));
            Assert.True(parser.Parse(model.GetType(), "Number != null").Invoke(model));
            Assert.True(parser.Parse(model.GetType(), "SubModel.Number / 2 == 0.5").Invoke(model));

            Assert.True(parser.Parse<Model>("SbyteNumber / SbyteEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("ByteNumber / ByteEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("ShortNumber / ShortEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("UshortNumber / UshortEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("IntNumber / IntEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("UintNumber / UintEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("LongNumber / LongEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model>("UlongNumber / UlongEnum.Second == 0.5").Invoke(model));

            Assert.True(parser.Parse<Model>("PoliticalStability == 0").Invoke(model));
            Assert.True(parser.Parse<Model>("PoliticalStability == Utility.Stability.High").Invoke(model));
            Assert.True(parser.Parse<Model>("PoliticalStability < Utility.Stability.Low").Invoke(model));
            Assert.True(parser.Parse<Model>("SubModel.PoliticalStability == null").Invoke(model));
            Assert.True(parser.Parse<Model>("SubModel.PoliticalStability != Utility.Stability.High").Invoke(model));

            Assert.True(parser.Parse<Model>("Const != Tools.Utility.Const").Invoke(model));

            Assert.True(parser.Parse<Model>("Flag").Invoke(model));
            Assert.False(parser.Parse<Model>("!Flag").Invoke(model));
            Assert.True(parser.Parse<Model>("Flag && true").Invoke(model));
            Assert.True(parser.Parse<Model>("NFlag == Flag").Invoke(model));
            Assert.True(parser.Parse<Model>("Flag == NFlag").Invoke(model));

            Assert.False(parser.Parse<Model>("SubModel.Flag").Invoke(model));
            Assert.True(parser.Parse<Model>("!SubModel.Flag").Invoke(model));
            Assert.False(parser.Parse<Model>("SubModel.Flag && true").Invoke(model));

            Assert.True(parser.Parse<Model>("Number < SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model>("Date <= NDate").Invoke(model));
            Assert.True(parser.Parse<Model>("NDate != null").Invoke(model));
            Assert.True(parser.Parse<Model>("Span <= NSpan").Invoke(model));
            Assert.True(parser.Parse<Model>("NSpan != null").Invoke(model));
            Assert.True(parser.Parse<Model>("SubModel.Date < NextWeek()").Invoke(model));
            Assert.True(parser.Parse<Model>("IncNumber(0) == SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model>("IncNumber(Number) == SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model>("Today() - Today() == Span").Invoke(model));
            Assert.True(parser.Parse<Model>("Today() - Today() == Date - Date").Invoke(model));
            Assert.True(parser.Parse<Model>("Today() - Today() == Span - Span").Invoke(model));
            Assert.True(parser.Parse<Model>("Today() - Today() == NSpan - NSpan").Invoke(model));
            Assert.True(parser.Parse<Model>("Span + NSpan == NSpan + Span").Invoke(model));
            Assert.True(parser.Parse<Model>("Now() - Span > Date - NSpan").Invoke(model));
            Assert.True(parser.Parse<Model>("Now() - Span > NDate - Span").Invoke(model));

            Assert.True(parser.Parse<Model>("DecNumber(SubModel.Number) == Number").Invoke(model));
            Assert.True(parser.Parse<Model>("DecNumber(Number) == 0").Invoke(model.SubModel));

            Assert.True(parser.Parse<Model>("SubModel.Date > Today()").Invoke(model));
            Assert.True(parser.Parse<Model>("'hello world' == Trim(SubModel.Text)").Invoke(model));
            Assert.True(parser.Parse<Model>("CompareOrdinal(Text, Trim(SubModel.Text)) == 0").Invoke(model));

            Assert.True(parser.Parse<Model>("NGuid1 != Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.True(parser.Parse<Model>("NGuid2 == Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.True(parser.Parse<Model>("NGuid1 != NGuid2").Invoke(model));
            Assert.True(parser.Parse<Model>("Guid != NGuid1").Invoke(model));
            Assert.True(parser.Parse<Model>("NGuid1 != Guid").Invoke(model));

            Assert.True(parser.Parse<Model>("InsensString == NInsensString").Invoke(model));

            model.NDate = null;
            model.NSpan = null;
            Assert.True(parser.Parse<Model>("NDate == null && Date != NDate").Invoke(model));
            Assert.True(parser.Parse<Model>("NSpan == null && Span != NSpan").Invoke(model));
            Assert.True(parser.Parse<Model>("Date(1, 1, 1) == MinDate").Invoke(model));

            var subModel = new Model();
            var newModel = new Model {SubModel = subModel, SubModelObject = subModel};
            Assert.True(parser.Parse<Model>("SubModel == SubModelObject").Invoke(newModel));
            Assert.True(parser.Parse<Model>("SubModelObject == SubModel").Invoke(newModel));

            const string expression =
                @"Flag == !false
                      && (
                             (Text != 'hello world' && Date < SubModel.Date)
                             || (
                                    (Number >= 0 && Number < 1) && PoliticalStability == Utility.Stability.High
                                )
                         ) 
                      && Const + Tools.Utility.Const == 'insideoutside2'";
            var func = parser.Parse(model.GetType(), expression);
            Assert.True(func(model));

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
            Assert.Equal(expectedFields.Count, parsedFields.Count);
            Assert.True(
                expectedFields.Keys.All(
                    key => parsedFields.ContainsKey(key) &&
                           EqualityComparer<Type>.Default.Equals(expectedFields[key], parsedFields[key])));

            parser.GetConsts()["Const"] = null; // try to mess up with internal fields - original data should not be affected
            var parsedConsts = parser.GetConsts();            
            var expectedConsts = new Dictionary<string, object>
            {
                {"Utility.Stability.High", Utility.Stability.High},
                {"Const", Model.Const},
                {"Tools.Utility.Const", Tools.Utility.Const}
            };
            Assert.Equal(expectedConsts.Count, parsedConsts.Count);
            Assert.True(
                expectedConsts.Keys.All(
                    key => parsedConsts.ContainsKey(key) &&
                           EqualityComparer<object>.Default.Equals(expectedConsts[key], parsedConsts[key])));

            Assert.True(parser.Parse<Model>("Array[0] != null && Array[1] != null").Invoke(model));
            Assert.True(parser.Parse<Model>("Array[0].Number + Array[0].Array[0].Number + Array[1].Number + Array[1].Array[0].Number == 0").Invoke(model));

            model.Collection[0] = new Model {Number = -1, Collection = new CustomCollection<Model>()};
            model.Collection[0].Collection[0] = new Model {Number = -2};
            model.Collection[1] = new Model {Number = 1, Collection = new CustomCollection<Model>()};
            model.Collection[1].Collection[0] = new Model {Number = 2};

            Assert.True(parser.Parse<Model>("Collection[0] != null && Collection[1] != null").Invoke(model));
            Assert.True(parser.Parse<Model>("Collection[0].Number + Collection[0].Collection[0].Number + Collection[1].Number + Collection[1].Collection[0].Number == 0").Invoke(model));
        }

        [Fact]
        public void verify_logic_with_derived_context()
        {
            var parser = new Parser();
            var firstDerived = new FirstDerived {Value = true};
            var secondDerived = new SecondDerived {Value = true};

            Assert.True(parser.Parse(firstDerived.GetType(), "Value").Invoke(firstDerived));
            Assert.True(parser.Parse(secondDerived.GetType(), "Value").Invoke(secondDerived));

            Assert.True(parser.Parse(typeof(ModelBase), "Value").Invoke(firstDerived));
            Assert.True(parser.Parse(typeof(ModelBase), "Value").Invoke(secondDerived));
        }

        [Fact]
        public void verify_non_bool_expression_failure()
        {
            var parser = new Parser();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("1").Invoke(null));
            Assert.True(e.Message.StartsWith("Parse fatal error."));

            e = Assert.Throws<ParseErrorException>(() => parser.Parse(typeof (object), "1").Invoke(null));
            Assert.True(e.Message.StartsWith("Parse fatal error."));
        }

        [Fact]
        public void verify_parser_parse_invalid_parameter()
        {
            var parser = new Parser();

            var e = Assert.Throws<ArgumentNullException>(() => parser.Parse<object>(null).Invoke(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);

            e = Assert.Throws<ArgumentNullException>(() => parser.Parse(typeof (object), null).Invoke(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);
        }

        [Fact]
        public void acceptable_methods_signatures_are_registered_correctly()
        {
            var parser = new Parser();
            parser.AddFunction("M0", () => 0);
            parser.AddFunction<int, int>("M1", i => i);
            parser.AddFunction<int, int, int>("M2", (i, j) => i + j);
            parser.AddFunction<int, int, int, int>("M3", (i, j, k) => i + j + k);
            parser.AddFunction<int, int, int, int, int>("M4", (i, j, k, l) => i + j + k + l);
            parser.AddFunction<int, int, int, int, int, int>("M5", (i, j, k, l, m) => i + j + k + l + m);
            parser.AddFunction<int, int, int, int, int, int, int>("M6", (i, j, k, l, m, n) => i + j + k + l + m + n);

            Assert.True(parser.Parse<object>("M0() == 0").Invoke(null));
            Assert.True(parser.Parse<object>("M1(1) == 1").Invoke(null));
            Assert.True(parser.Parse<object>("M2(1,1) == 2").Invoke(null));
            Assert.True(parser.Parse<object>("M3(1,1,1) == 3").Invoke(null));
            Assert.True(parser.Parse<object>("M4(1,1,1,1) == 4").Invoke(null));
            Assert.True(parser.Parse<object>("M5(1,1,1,1,1) == 5").Invoke(null));
            Assert.True(parser.Parse<object>("M6(1,1,1,1,1,1) == 6").Invoke(null));
        }

        [Fact]
        public void verify_methods_overloading() // overloading concept exists, when there are two methods of the same name, but different signature
        {
            // methods overloading is based on the number of arguments
            var parser = new Parser();
            parser.AddFunction("Whoami", () => "utility method");
            parser.AddFunction<int, string>("Whoami", i => $"utility method {i}");
            parser.AddFunction<int, string, string>("Whoami", (i, s) => $"utility method {i} - {s}");

            Assert.True(parser.Parse<object>("Whoami() == 'utility method'").Invoke(null));
            Assert.True(parser.Parse<object>("Whoami(1) == 'utility method 1'").Invoke(null));
            Assert.True(parser.Parse<object>("Whoami(2, 'final') == 'utility method 2 - final'").Invoke(null));
        }

        [Fact]
        public void verify_methods_overriding() // overriding concept exists, when there are two methods of the same name and signature, but different implementation
        {
            var parser = new Parser();

            // register utility methods
            parser.AddFunction("Whoami", () => "utility method");
            parser.AddFunction<int, string>("Whoami", i => $"utility method {i}");

            var model = new ModelWithMethods();

            // redefined model methods take precedence
            Assert.True(parser.Parse<ModelWithMethods>("Whoami() == 'model method'").Invoke(model));
            Assert.True(parser.Parse<ModelWithMethods>("Whoami(1) == 'model method 1'").Invoke(model));
        }

        [Fact]
        public void verify_toolchain_methods_ambiguity()
        {
            // since arguments types are not taken under consideration for methods overloading, following logic should fail
            var parser = new Parser();

            parser.AddFunction<int, string>("Whoami", i => $"utility method {i}");
            parser.AddFunction<string, string>("Whoami", s => $"utility method {s}");

            parser.AddFunction<string, string, string>("Glue", (s1, s2) => string.Concat(s1, s2));
            parser.AddFunction<int, int, string>("Glue", (i1, i2) => string.Concat(i1, i2));

            var e = Assert.Throws<ParseErrorException>(() => Assert.True(parser.Parse<object>("Whoami(0) == 'utility method 0'").Invoke(null)));
            Assert.Equal("Function 'Whoami' accepting 1 argument is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Glue('a', 'b') == 'ab'").Invoke(null));
            Assert.Equal("Function 'Glue' accepting 2 arguments is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_context_methods_ambiguity()
        {
            // not only built-in, but also context extracted methods are subjected to the same rules
            var parser = new Parser();
            var model = new ModelWithAmbiguousMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<ModelWithAmbiguousMethods>("Whoami(0) == 'model method 0'").Invoke(model));
            Assert.Equal("Function 'Whoami' accepting 1 argument is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_implicit_type_conversion()
        {
            var parser = new Parser();
            parser.AddFunction<object, string>("Whoami", o => $"utility method {o}");
            parser.AddFunction<int, string, string>("Whoami", (i, s) => $"utility method {i} - {s}");

            Assert.True(parser.Parse<object>("Whoami('0') == 'utility method 0'").Invoke(null)); // successful conversion from String to Object
            Assert.True(parser.Parse<object>("Whoami(1, '2') == 'utility method 1 - 2'").Invoke(null)); // types matched, no conversion needed

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Whoami('1', '2') == 'utility method 1 - 2'").Invoke(null));
            Assert.Equal("Function 'Whoami' 1st argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            Assert.Equal(new Location(1, 8), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Whoami(1, 2) == 'utility method 1 - 2'").Invoke(null));
            Assert.Equal("Function 'Whoami' 2nd argument implicit conversion from 'System.Int32' to expected 'System.String' failed.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());
        }

        [Fact]
        public void function_arguments_order_identification_is_formatted_correctly()
        {
            var parser = new Parser();
            var model = new Model();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long('', 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 1st argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, '', 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 2nd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, 2, '', 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 3rd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, 2, 3, '', 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 4th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, '', 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 11th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, '', 13)").Invoke(model));
            Assert.Equal("Function 'Long' 12th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, '')").Invoke(model));
            Assert.Equal("Function 'Long' 13th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
        }

        [Fact]
        public void verify_short_circuit_evaluation()
        {
            var parser = new Parser();
            parser.AddFunction<object, bool>("CastToBool", obj => (bool) obj);

            Assert.Throws<NullReferenceException>(() => parser.Parse<object>("CastToBool(null)").Invoke(null));

            // below, the exception should not be thrown as above
            // reason? - first argument is suffient to determine the value of the expression so the second one is not going to be evaluated
            Assert.False(parser.Parse<object>("false && CastToBool(null)").Invoke(null));
            Assert.True(parser.Parse<object>("true || CastToBool(null)").Invoke(null));
        }

        [Fact]
        public void verify_enumeration_ambiguity()
        {
            var parser = new Parser();
            
            // ensure that this doesn't consider Dog and HotDog enums to be ambiguous
            Assert.True(parser.Parse<object>("Dog.Collie == 0").Invoke(null));
            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Stability.High == 0").Invoke(null));
            Assert.Equal(
                @"Enum 'Stability' is ambiguous, found following:
'ExpressiveAnnotations.Tests.Stability',
'ExpressiveAnnotations.Tests.Utility+Stability'.",
                e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_consts_ambiguity()
        {
            var parser = new Parser();

            // ensure that this doesn't consider Dogs.Const and HotDogs.Const constants to be ambiguous
            Assert.True(parser.Parse<object>("Dogs.Const == 0").Invoke(null));
            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Utility.Const == 'outside1'").Invoke(null));
            Assert.Equal(
                @"Constant 'Utility.Const' is ambiguous, found following:
'ExpressiveAnnotations.Tests.Utility.Const',
'ExpressiveAnnotations.Tests.Tools+Utility.Const'.",
                e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_invalid_func_identifier()
        {
            var parser = new Parser();
            var model = new Model();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("NotMe == 0").Invoke(model));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'NotMe' not known.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_naming_collisions()
        {
            var parser = new Parser();

            var one = new SampleOne();
            var two = new SampleTwo();

            Assert.True(parser.Parse<SampleOne>("0 == Vehicle.Car").Invoke(one));
            Assert.Equal(0, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);
            Assert.True(parser.Parse<SampleOne>("Vehicle == Vehicle.Car").Invoke(one));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);

            Assert.True(parser.Parse<SampleTwo>("-1 == Vehicle.Car").Invoke(two));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(0, parser.GetConsts().Count);
            Assert.True(parser.Parse<SampleTwo>("Vehicle.Car != ParserTest.Vehicle.Car").Invoke(two));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);
        }

        [Fact]
        public void verify_toolchain_methods_logic()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            Assert.True(parser.Parse<object>("Now() > Today()").Invoke(null));
            Assert.True(parser.Parse<object>("Date(1985, 2, 20) < Date(1985, 2, 20, 0, 0, 1)").Invoke(null));
            Assert.True(parser.Parse<object>("Date(1, 1, 1) == Date(1, 1, 1, 0, 0, 0)").Invoke(null));

            Assert.True(parser.Parse<object>("TimeSpan(1, 0, 0, 0) > TimeSpan(0, 1, 0, 0)").Invoke(null));
            //Assert.True(parser.Parse<object>("(TimeSpan(0, 0, 0, 0)).TotalMilliseconds == 0").Invoke(null)); // foo().Prop - to be supported?
            //Assert.True(parser.Parse<object>("(TimeSpan(1, 2, 3, 4)).TotalMilliseconds == 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000").Invoke(null));

            Assert.True(parser.Parse<object>("Length('0123') == 4").Invoke(null));
            Assert.True(parser.Parse<object>("Length('    ') == 4").Invoke(null));
            Assert.True(parser.Parse<object>("Length(null) == 0").Invoke(null));
            Assert.True(parser.Parse<object>("Length('') == 0").Invoke(null));

            Assert.True(parser.Parse<object>("Trim(' a b c ') == 'a b c'").Invoke(null));
            Assert.True(parser.Parse<object>("Trim(null) == null").Invoke(null));
            Assert.True(parser.Parse<object>("Trim('') == ''").Invoke(null));

            Assert.True(parser.Parse<object>("Concat(' a ', ' b ') == ' a  b '").Invoke(null));
            Assert.True(parser.Parse<object>("Concat(null, null) == ''").Invoke(null));
            Assert.True(parser.Parse<object>("Concat('', '') == ''").Invoke(null));

            Assert.True(parser.Parse<object>("Concat(' a ', ' b ', ' c ') == ' a  b  c '").Invoke(null));
            Assert.True(parser.Parse<object>("Concat(null, null, null) == ''").Invoke(null));
            Assert.True(parser.Parse<object>("Concat('', '', '') == ''").Invoke(null));

            Assert.True(parser.Parse<object>("CompareOrdinal(' abc ', ' ABC ') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('a', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('a', 'A') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('A', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('a', 'b') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('b', 'a') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal(null, 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('a', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal(' ', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('a', ' ') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal(null, '') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal(null, null) == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinal('', '') == 0").Invoke(null));

            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase(' abc ', ' ABC ') == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'A') == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('A', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('a', 'b') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('b', 'a') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase(null, 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('a', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase(' ', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('a', ' ') == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase(null, '') == -1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase(null, null) == 0").Invoke(null));
            Assert.True(parser.Parse<object>("CompareOrdinalIgnoreCase('', '') == 0").Invoke(null));

            Assert.True(parser.Parse<object>("StartsWith(' ab c', ' A') == false").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith(' ab c', ' a') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWith(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("StartsWithIgnoreCase(' ab c', ' A') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase(' ab c', ' a') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("StartsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("EndsWith(' ab c', ' C') == false").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith(' ab c', ' c') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWith(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("EndsWithIgnoreCase(' ab c', ' C') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase(' ab c', ' c') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("EndsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("Contains(' ab c', 'B ') == false").Invoke(null));
            Assert.True(parser.Parse<object>("Contains(' ab c', 'b ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("Contains(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("Contains('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("Contains(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("Contains('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("Contains(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("ContainsIgnoreCase(' ab c', 'B ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase(' ab c', 'b ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("ContainsIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("IsNullOrWhiteSpace(' ') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNullOrWhiteSpace(null) == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNullOrWhiteSpace('') == true").Invoke(null));

            Assert.True(parser.Parse<object>("IsDigitChain('0123456789') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsDigitChain('+0') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsDigitChain('-0') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsDigitChain(null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsDigitChain('') == false").Invoke(null));

            Assert.True(parser.Parse<object>("IsNumber('0') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('0.0') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('10.10') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('0e0') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('.2') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('3.14') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('5e6') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('5e-6') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('5e+6') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('9.0E-10') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('.11e10') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('-0.3e-2') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('+0.3e-2') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('+0') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('-0') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('++0') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('--0') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('+-0') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber(null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsNumber('') == false").Invoke(null));

            Assert.True(parser.Parse<object>("IsEmail('nickname@domain.com') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsEmail(null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsEmail('') == false").Invoke(null));

            Assert.True(parser.Parse<object>("IsUrl('http://www.github.com/') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsUrl(null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsUrl('') == false").Invoke(null));

            Assert.True(parser.Parse<object>(@"IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == true").Invoke(null));
            Assert.True(parser.Parse<object>(@"IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.True(parser.Parse<object>(@"IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.True(parser.Parse<object>(@"IsRegexMatch('John\'s cat named ""\\\'""\n (Backslash Quote)', '^\\d+$') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsRegexMatch('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object>("IsRegexMatch(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsRegexMatch('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object>("IsRegexMatch(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object>("Guid('a1111111-1111-1111-1111-111111111111') == Guid('A1111111-1111-1111-1111-111111111111')").Invoke(null));

            var e = Assert.Throws<FormatException>(() => parser.Parse<object>("Guid('abc') == Guid('abc')").Invoke(null));
            Assert.Equal("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", e.Message);
        }

        public static IEnumerable<object[]> LogicalOperators
        {
            get { return new[] {"&&", "||"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("LogicalOperators")]
        public void verify_type_mismatch_errors_for_logical_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"true {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Boolean' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"Now() {oper} Today()").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"1 {oper} 2").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"YesNo.Yes {oper} YesNo.No").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.ParserTest+YesNo' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> EqualityOperators
        {
            get { return new[] {"==", "!="}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("EqualityOperators")]
        public void verify_type_mismatch_errors_for_equality_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"0 {oper} '0'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"0.1 {oper} '0'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Double' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} 'asd'").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"NDate {oper} 'asd'").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Bag>($"Lexer {oper} Parser").Invoke(new Bag { Lexer = new Lexer(), Parser = new Parser() }));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Analysis.Lexer' and 'ExpressiveAnnotations.Analysis.Parser'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Cash {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Decimal' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"Utility.Stability.High {oper} YesNo.Yes").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.Utility+Stability' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 24), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> InequalityOperators
        {
            get { return new[] {">", ">=", "<", "<="}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("InequalityOperators")]
        public void verify_type_mismatch_errors_for_inequality_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"'a' {oper} 'b'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"'asd' {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"NDate {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"NDate {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"SubModelObject {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Object' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 16), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"0 {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Span {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"Utility.Stability.High {oper} YesNo.Yes").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.Utility+Stability' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 24), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> AddSubOperators
        {
            get { return new[] {"+", "-"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("AddSubOperators")]
        public void verify_type_mismatch_errors_for_addition_and_subtraction_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"0 {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"NDate {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Span {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"NSpan {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.TimeSpan]' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"0 {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"Span {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> MulDivOperators
        {
            get { return new[] {"*", "/"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("MulDivOperators")]
        public void verify_type_mismatch_errors_for_multiplication_and_division_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"0 {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>($"'abc' {oper} 'abc'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>(
                $@"1 - 2
    - (6 / ((2{oper}'1.5' - 1) + 1)) * -2 
    + 1/2/1 == 3.50").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.String'.", e.Error);
            Assert.Equal(new Location(2, 15), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_type_mismatch_errors_for_addition_operator()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Date + Date").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("NDate + NDate").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Nullable`1[System.DateTime]'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Date + NDate").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.Nullable`1[System.DateTime]'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_type_mismatch_errors_for_subtraction_operator()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("'abc' - 'abc'").Invoke(null));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("1 + 2 + 'abc' - 'abc' > 0").Invoke(null));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 15), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("'asd' - null").Invoke(new Model()));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_type_mismatch_errors_for_negation_operator()
        {
            var parser = new Parser();
            parser.RegisterMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("!null").Invoke(null));
            Assert.Equal("Operator '!' cannot be applied to operand of type 'null'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("!'a'").Invoke(null));
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("!! Today()").Invoke(null));
            Assert.Equal("Operator '!' cannot be applied to operand of type 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_various_parsing_errors()
        {
            var parser = new Parser();
            parser.RegisterMethods();
            parser.AddFunction<int, int, int>("Max", (x, y) => Math.Max(x, y));

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("1++ +1==2").Invoke(null));
            Assert.Equal("Unexpected token: '++'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("true # false").Invoke(null));
            Assert.Equal("Invalid token.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>(
                @"1 - 2

    - 6
    + 1/x[0]/1 == 3.50").Invoke(null));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'x[0]' not known.", e.Error);
            Assert.Equal(new Location(4, 9), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("WriteLine('hello')").Invoke(null));
            Assert.Equal("Function 'WriteLine' not known.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("1 2").Invoke(null));
            Assert.Equal("Unexpected token: '2'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("(").Invoke(null));
            Assert.Equal("Expected \"null\", int, float, bool, string or func. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("(1+1").Invoke(null));
            Assert.Equal("Expected closing bracket. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("()").Invoke(null));
            Assert.Equal("Expected \"null\", int, float, bool, string or func. Unexpected token: ')'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Max(").Invoke(null));
            Assert.Equal("Expected \"null\", int, float, bool, string or func. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Max(1 2").Invoke(null));
            Assert.Equal("Function 'Max', expected comma or closing bracket. Unexpected token: '2'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Max(1.1)").Invoke(null));
            Assert.Equal("Function 'Max' accepting 1 argument not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("Max(1.1, 1.2, 'a')").Invoke(null));
            Assert.Equal("Function 'Max' accepting 3 arguments not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>(
                @"Max(1, 
      Max(1, 'a')) == 1.1").Invoke(null));
            Assert.Equal("Function 'Max' 2nd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            Assert.Equal(new Location(2, 14), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Items[0] != null").Invoke(new Model {Items = new List<Model> {new Model()}}));
            Assert.Equal("Identifier 'Items' either does not represent an array type or does not declare indexer.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>("Long(1)").Invoke(new Model()));
            Assert.Equal("Function 'Long' accepting 1 argument not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model>(@"Max(1").Invoke(new Model()));
            Assert.Equal("Function 'Max', expected comma or closing bracket. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object>("1.1.1").Invoke(null));
            Assert.Equal("Unexpected token: '.1'.", e.Error); // "unexpected" is not "invalid" - token is valid, but used in wrong context
            Assert.Equal(new Location(1, 4), e.Location, new LocationComparer());
        }

        [Fact]
        public void unicode_characters_are_supported()
        {
            var parser = new Parser();
            var model = new LocalModel {ąęćłńśóźż = "ąęćłńśóźż"};
            Assert.True(parser.Parse<LocalModel>("ąęćłńśóźż == 'ąęćłńśóźż'").Invoke(model));
        }

        private class Model
        {
            public const string Const = "inside";

            public object SubModelObject { get; set; }
            public Model SubModel { get; set; }

            public DateTime MinDate => DateTime.MinValue;

            public DateTime? NDate { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan? NSpan { get; set; }
            public TimeSpan Span { get; set; }
            public int? Number { get; set; }
            public bool Flag { get; set; }
            public bool? NFlag { get; set; }
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

            public Guid Guid { get; set; }
            public Guid? NGuid1 { get; set; }
            public Guid? NGuid2 { get; set; }

            public Model[] Array { get; set; } // array
            public IEnumerable<Model> Items { get; set; } // collection without indexer
            public CustomCollection<Model> Collection { get; set; } // collection with indexer, like e.g List<>

            public StringInsens InsensString { get; set; }
            public StringInsens? NInsensString { get; set; }

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

            public void Long(int i, int j, int k, int l, int m, int n, int o, int p, int r, int s, int t, int u, int v) { }
        }

        private class ModelWithAmbiguousMethods
        {
            public string Whoami(string s)
            {
                return $"model method {s}";
            }

            public string Whoami(int i)
            {
                return $"model method {i}";
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
                return $"model method {i}";
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

                Assert.True(0 == (int) Vehicle.Car);
                Assert.True(Vehicle == Vehicle.Car);
            }

            public Vehicle Vehicle { get; set; }
        }

        public class SampleTwo
        {
            public SampleTwo()
            {
                Vehicle = new Carriage {Car = -1};

                Assert.True(-1 == Vehicle.Car);
                Assert.True(Vehicle.Car != (int) ParserTest.Vehicle.Car);
            }

            public Carriage Vehicle { get; set; }
        }

        public class Bag
        {
            public Parser Parser { get; set; }
            public Lexer Lexer { get; set; }
        }

        public class LocalModel
        {
            public string ąęćłńśóźż { get; set; }
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

        public struct StringInsens
        {
            private readonly string _value;

            public StringInsens(string value)
            {
                _value = value;
            }

            public bool Equals(StringInsens other)
            {
                return string.Equals(_value, other._value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is StringInsens && Equals((StringInsens)obj);
            }

            public override int GetHashCode()
            {
                return _value?.GetHashCode() ?? 0;
            }

            public static bool operator ==(StringInsens a, StringInsens b)
            {
                return string.Equals(a._value, b._value, StringComparison.CurrentCultureIgnoreCase);
            }

            public static bool operator !=(StringInsens a, StringInsens b)
            {
                return !(a == b);
            }
        }
    }
}
