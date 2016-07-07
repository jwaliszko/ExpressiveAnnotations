using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public void verify_precedence()
        {
            var parser = new Parser();

            parser.Parse<object, int>("1+2*3");
            Assert.Equal("(+(1)(*(2)(3)))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("(1+2)*3");
            Assert.Equal("(*(+(1)(2))(3))", parser.GetExpression().PrefixPrint());
        }

        [Fact]
        public void verify_associativity()
        {
            var parser = new Parser();

            parser.Parse<Model, int>("Sum(1,2,3)");
            Assert.Equal("(Sum((1),(2),(3)))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("[1,2][0]");
            Assert.Equal("(Index[([(1),(2)]),(0)])", parser.GetExpression().PrefixPrint());
            parser.Parse<Model, int?>("SubModel.Number");
            Assert.Equal("(Prop[(Prop[(<Model>),SubModel]),Number])", parser.GetExpression().PrefixPrint());

            parser.Parse<object, int>("+1");
            Assert.Equal("(1)", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("-1");
            Assert.Equal("(-(1))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("!true");
            Assert.Equal("(!(True))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("~1");
            Assert.Equal("(~(1))", parser.GetExpression().PrefixPrint());

            parser.Parse<object, int>("1*2*3");
            Assert.Equal("(*(*(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, double>("1.1/2.1/3.1");
            Assert.Equal("(/(/(1.1)(2.1))(3.1))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, double>("1.1%2.1%3.1");
            Assert.Equal("(%(%(1.1)(2.1))(3.1))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1+2+3");
            Assert.Equal("(+(+(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1-2-3");
            Assert.Equal("(-(-(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1>>2>>3");
            Assert.Equal("(>>(>>(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1<<2<<3");
            Assert.Equal("(<<(<<(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1<2");
            Assert.Equal("(<(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1<=2");
            Assert.Equal("(<=(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1>2");
            Assert.Equal("(>(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1>=2");
            Assert.Equal("(>=(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1==2");
            Assert.Equal("(==(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("1!=2");
            Assert.Equal("(!=(1)(2))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1^2^3");
            Assert.Equal("(^(^(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, int>("1|2|3");
            Assert.Equal("(|(|(1)(2))(3))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("true && false && true");
            Assert.Equal("(&&(&&(True)(False))(True))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("true || false || true");
            Assert.Equal("(||(||(True)(False))(True))", parser.GetExpression().PrefixPrint());
            parser.Parse<object, bool>("true ? false ? true : false : true");

            Assert.Equal("(?(True):(?(False):(True)(False))(True))", parser.GetExpression().PrefixPrint());
        }

        [Fact]
        public void verify_logic_without_context()
        {
            var parser = new Parser();
            Toolchain.Instance.AddFunction("ArrayLength", (object arr) => ((Array) arr).Length);
            parser.RegisterToolchain();            

            Assert.True(parser.Parse<object, bool>("YesNo.Yes == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("YesNo.Yes < YesNo.No").Invoke(null));
            Assert.True(parser.Parse<object, bool>("YesNo.Yes - YesNo.No == -1").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true").Invoke(null));
            Assert.False(parser.Parse<object, bool>("false").Invoke(null));
            Assert.False(parser.Parse<object, bool>("!true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("!false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("false == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("true != false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("!true == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("!!true == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("!!!true == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true != !true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("!!true != !!!true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true && true").Invoke(null));
            Assert.False(parser.Parse<object, bool>("false && false").Invoke(null));
            Assert.False(parser.Parse<object, bool>("true && false").Invoke(null));
            Assert.False(parser.Parse<object, bool>("false && true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true || true").Invoke(null));
            Assert.False(parser.Parse<object, bool>("false || false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("true || false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("false || true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))").Invoke(null));
            
            Assert.True(parser.Parse<object, bool>("(0b0101 & 0b1100) == 0b0100").Invoke(null));            
            Assert.True(parser.Parse<object, bool>("(0b0101 | 0b1100) == 0b1101").Invoke(null));            
            Assert.True(parser.Parse<object, bool>("(0b0101 ^ 0b1100) == 0b1001").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(5 & 12) == 4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(5 | 12) == 13").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(5 ^ 12) == 9").Invoke(null));

            Assert.False(parser.Parse<object, bool>("true && true && false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("false || false || true").Invoke(null));

            Assert.False(parser.Parse<object, bool>("true & true & false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("false | false | true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true ^ true ^ true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("~+5 == -6").Invoke(null));
            Assert.True(parser.Parse<object, bool>("~~5 == 5").Invoke(null));
            Assert.True(parser.Parse<object, bool>("~-6 == 5").Invoke(null));

            Assert.True(parser.Parse<object, bool>("3 >> 1 >> 1 == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 << 1 << 2 == 8").Invoke(null));
            Assert.True(parser.Parse<object, bool>("8 >> 2 >> 1 << 1 << 2 == 8").Invoke(null));

            Assert.True(parser.Parse<object, bool>("0 == 0 && 1 < 2").Invoke(null));

            Assert.False(parser.Parse<object, bool>("0 != 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 >= 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 <= 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 == 0").Invoke(null));
            Assert.False(parser.Parse<object, bool>("0 == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("-1 == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 != 1").Invoke(null));
            Assert.False(parser.Parse<object, bool>("0 >= 1").Invoke(null));
            Assert.False(parser.Parse<object, bool>("0 > 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 <= 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 < 1").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1 + 2 == 3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 - 2 == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("2 - 1 > 0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("null == null").Invoke(null));
            Assert.False(parser.Parse<object, bool>("null != null").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'' == ''").Invoke(null));
            Assert.True(parser.Parse<object, bool>("' ' == ' '").Invoke(null));
            Assert.True(parser.Parse<object, bool>("' ' != '  '").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'asd' == 'asd'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'asd' != 'ASD'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'' != null").Invoke(null));

            Assert.True(parser.Parse<object, bool>("'a' + 'b' == 'ab'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'' + '' == ''").Invoke(null));
            Assert.True(parser.Parse<object, bool>("' ' + null == ' '").Invoke(null));
            Assert.True(parser.Parse<object, bool>("'a' + 0 + 'ab' + null + 'abc' == 'a0ababc'").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1 + 2 + 3 + 4 == 10").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 - 2 + 3 - 4 == -2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("-1 - 2 - 3 - 4 == -10").Invoke(null));
            Assert.True(parser.Parse<object, bool>("3 - (4 - 5) == 4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 - -1 == 2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(1 - 2) + ((3 - 4) - 5) == -7").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1 * 2 == 2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 * 2 * 3 == 6").Invoke(null));
            Assert.True(parser.Parse<object, bool>("2 / 2 == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("4 / 2 / 2 == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("2 * 2 / 2 == 2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("4 / 2 * 2 == 4").Invoke(null));

            Assert.True(parser.Parse<object, bool>("5 % 2 == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("5 % 2 % 1 == 0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1.2 * 2 == 2.4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1.2 / 2 == 0.6").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1.2 + 2 == 3.2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1.2 - 2 == -0.8").Invoke(null));

            Assert.True(parser.Parse<object, bool>("0e0 == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>(".2 == 0.2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("3.14 == 3.14").Invoke(null));
            Assert.True(parser.Parse<object, bool>("5e6 == 5000000").Invoke(null));
            Assert.True(parser.Parse<object, bool>("5e-6 == 5E-06").Invoke(null));
            Assert.True(parser.Parse<object, bool>("5e+6 == 5000000").Invoke(null));
            Assert.True(parser.Parse<object, bool>("9.0E-10 == 9E-10").Invoke(null));
            Assert.True(parser.Parse<object, bool>(".11e10 == 1100000000").Invoke(null));

            Assert.True(parser.Parse<object, bool>("0b0 == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0b11111111 == 255").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0x0 == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0xFF == 255").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0xff == 255").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1+2==3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1-2==-1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1*2>-1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("-1*-2>-1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1/2==0.5").Invoke(null));
            Assert.True(parser.Parse<object, bool>("-1*-+- -+-1==-1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("- - -1+'a'+'b'+null+''+'c'+1+2=='-1abc12'").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1 - 2 -(6 / ((2*1.5 - 1) + 1)) * -2 + 1/2/1 == 3.50").Invoke(null));
            Assert.True(parser.Parse<object, bool>("-.11e-10+.11e-10==.0-.0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("'abc' == Trim(' abc ')").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Length(null) + Length('abc' + 'cde') >= Length(Trim(' abc def ')) - 2 - -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("0 == YesNo.Yes").Invoke(null));
            Assert.True(parser.Parse<object, bool>("YesNo.Yes == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("YesNo.Yes != YesNo.No").Invoke(null));

            Assert.True(parser.Parse<object, bool>("true ? true : false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("false ? false : true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("(true ? 1 : 2) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(false ? 1 : 2) == 2").Invoke(null));

            Assert.True(parser.Parse<object, bool>("(1 > 0 ? true : false) ? (1 > 0 ? true : false) : (false ? false : false)").Invoke(null));
            Assert.True(parser.Parse<object, bool>("(1 > 0 ? false : true) ? (false ? false : false) : (1 > 0 ? true : false)").Invoke(null));

            Assert.True(parser.Parse<object, bool>("1 > 0 ? true : false ? 1 > 0 ? true : false : false ? false : false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("1 > 0 ? true : false ? false ? false : false : 1 > 0 ? true : false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("ArrayLength([]) == 0").Invoke(new Model()));
            Assert.True(parser.Parse<object, bool>("ArrayLength([1]) == 1").Invoke(new Model()));
            Assert.True(parser.Parse<object, bool>("ArrayLength([1,2,3]) == 3").Invoke(new Model()));
            Assert.True(parser.Parse<object, bool>("[0+1,2,3][0] == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("[0+1,2,3][true ? 0 : 1] == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("[1,2,3][[1,2][[1,2][0]]] == 3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("[[1,2],[3,4]][1][0] == 3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("[[1,2],'asd'][1] == 'asd'").Invoke(null));

            Toolchain.Instance.AddFunction("Avg", (Expression<Toolchain.ParamsDelegate<double, double?>>)(items => items.Any() ? items.Average() : (double?)null));
            Assert.True(parser.Parse<object, bool>("Avg() == null").Invoke(null));

            var result = parser.Parse<object, object>("[[1,2],[3],4]").Invoke(null);
            Assert.True(result.ArrayDeepEqual(new object[] {new[] {1, 2}, new[] {3}, 4}));
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
                IntArray = new[] {1,2,3},
                IntJaggedArray = new[]
                {
                  new[] {1,2,3},
                  new[] {4,5,6},    
                },
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
            model.Collection[0] = new Model {Number = -1, Collection = new CustomCollection<Model>()};
            model.Collection[0].Collection[0] = new Model {Number = -2};
            model.Collection[1] = new Model {Number = 1, Collection = new CustomCollection<Model>()};
            model.Collection[1].Collection[0] = new Model {Number = 2};

            var parser = new Parser();
            Toolchain.Instance.AddFunction("GetModel", () => model);
            Toolchain.Instance.AddFunction("GetModels", () => new[] {model});
            parser.RegisterToolchain();

            Assert.True(parser.Parse<bool>(model.GetType(), "Number < 1").Invoke(model));
            Assert.True(parser.Parse<bool>(model.GetType(), "Number == 0").Invoke(model));
            Assert.True(parser.Parse<bool>(model.GetType(), "Number != null").Invoke(model));
            Assert.True(parser.Parse<bool>(model.GetType(), "SubModel.Number / 2 == 0.5").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("SbyteNumber / SbyteEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("ByteNumber / ByteEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("ShortNumber / ShortEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("UshortNumber / UshortEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("IntNumber / IntEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("UintNumber / UintEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("LongNumber / LongEnum.Second == 0.5").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("UlongNumber / UlongEnum.Second == 0.5").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("PoliticalStability == 0").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("PoliticalStability == Utility.Stability.High").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("PoliticalStability < Utility.Stability.Low").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("SubModel.PoliticalStability == null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("SubModel.PoliticalStability != Utility.Stability.High").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("Const != Tools.Utility.Const").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("Flag").Invoke(model));
            Assert.False(parser.Parse<Model, bool>("!Flag").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Flag && true").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NFlag == Flag").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Flag == NFlag").Invoke(model));

            Assert.False(parser.Parse<Model, bool>("SubModel.Flag").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("!SubModel.Flag").Invoke(model));
            Assert.False(parser.Parse<Model, bool>("SubModel.Flag && true").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("Number < SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Date <= NDate").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NDate != null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Span <= NSpan").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NSpan != null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("SubModel.Date < NextWeek()").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("IncNumber(0) == SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("IncNumber(Number) == SubModel.Number").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Today() - Today() == Span").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Today() - Today() == Date - Date").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Today() - Today() == Span - Span").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Today() - Today() == NSpan - NSpan").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Span + NSpan == NSpan + Span").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Now() - Span > Date - NSpan").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Now() - Span > NDate - Span").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("-NSpan == -NSpan").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("+NSpan == +NSpan").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("DecNumber(SubModel.Number) == Number").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("DecNumber(Number) == 0").Invoke(model.SubModel));

            Assert.True(parser.Parse<Model, bool>("Average() == null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Average(1,2,3) == 2").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Average(IntArray) == 2").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("SubModel.Date > Today()").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("'hello world' == Trim(SubModel.Text)").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("CompareOrdinal(Text, Trim(SubModel.Text)) == 0").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("NGuid1 != Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NGuid2 == Guid('00000000-0000-0000-0000-000000000000')").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NGuid1 != NGuid2").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Guid != NGuid1").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NGuid1 != Guid").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("InsensString == NInsensString").Invoke(model));

            model.NDate = null;
            model.NSpan = null;
            Assert.True(parser.Parse<Model, bool>("NDate == null && Date != NDate").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("NSpan == null && Span != NSpan").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Date(1, 1, 1) == MinDate").Invoke(model));

            var subModel = new Model();
            var newModel = new Model {SubModel = subModel, SubModelObject = subModel};
            Assert.True(parser.Parse<Model, bool>("SubModel == SubModelObject").Invoke(newModel));
            Assert.True(parser.Parse<Model, bool>("SubModelObject == SubModel").Invoke(newModel));

            const string expression =
                @"Flag == !false
                      && (
                             (Text != 'hello world' && Date < SubModel.Date)
                             || (
                                    (Number >= 0 && Number < 1)
                                    && Collection[true ? 0 : 1].Number < 0
                                    && PoliticalStability == Utility.Stability.High
                                )
                         ) 
                      && Const + Tools.Utility.Const == 'insideoutside2'";
            var func = parser.Parse<bool>(model.GetType(), expression);
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
                {"Collection[true ? 0 : 1].Number", typeof (int?)},
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

            Assert.True(parser.Parse<Model, bool>("Array[0] != null && Array[1] != null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Array[0].Number + Array[0].Array[0].Number + Array[1].Number + Array[1].Array[0].Number == 0").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("Collection[0] != null && Collection[1] != null").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Collection[0].Number + Collection[0].Collection[0].Number + Collection[1].Number + Collection[1].Collection[0].Number == 0").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("Collection[true ? 0 : 1].Collection[true ? [0][0] : 1].Number == -2").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("IntJaggedArray[1][2] == 6").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("IntJaggedArray[1][1 + 1] == 6").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("[SubModel][0].Number == 1").Invoke(model));

            Assert.True(parser.Parse<Model, bool>("GetModel().Number == 0").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("GetModels()[0].Number == 0").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("GetModel().SubModel.Number == 1").Invoke(model));
            Assert.True(parser.Parse<Model, bool>("GetModels()[0].SubModel.Number == 1").Invoke(model));
        }

        [Fact]
        public void verify_logic_with_derived_context()
        {
            var parser = new Parser();
            var firstDerived = new FirstDerived {Value = true};
            var secondDerived = new SecondDerived {Value = true};

            Assert.True(parser.Parse<bool>(firstDerived.GetType(), "Value").Invoke(firstDerived));
            Assert.True(parser.Parse<bool>(secondDerived.GetType(), "Value").Invoke(secondDerived));

            Assert.True(parser.Parse<bool>(typeof (ModelBase), "Value").Invoke(firstDerived));
            Assert.True(parser.Parse<bool>(typeof (ModelBase), "Value").Invoke(secondDerived));
        }

        [Fact]
        public void verify_non_bool_expression_failure()
        {
            var parser = new Parser();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("1").Invoke(null));
            Assert.Equal("Parse fatal error.", e.Message);

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<bool>(typeof (object), "1").Invoke(null));
            Assert.Equal("Parse fatal error.", e.Message);
        }

        [Fact]
        public void verify_parser_parse_invalid_parameter()
        {
            var parser = new Parser();

            var e = Assert.Throws<ArgumentNullException>(() => parser.Parse<object, bool>(null).Invoke(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);

            e = Assert.Throws<ArgumentNullException>(() => parser.Parse<bool>(typeof (object), null).Invoke(null));
            Assert.Equal("Expression not provided.\r\nParameter name: expression", e.Message);
        }

        [Fact]
        public void acceptable_methods_signatures_are_registered_correctly()
        {
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction("M0", () => 0);
            funcManager.AddFunction<int, int>("M1", i => i);
            funcManager.AddFunction<int, int, int>("M2", (i, j) => i + j);
            funcManager.AddFunction<int, int, int, int>("M3", (i, j, k) => i + j + k);
            funcManager.AddFunction<int, int, int, int, int>("M4", (i, j, k, l) => i + j + k + l);
            funcManager.AddFunction<int, int, int, int, int, int>("M5", (i, j, k, l, m) => i + j + k + l + m);
            funcManager.AddFunction<int, int, int, int, int, int, int>("M6", (i, j, k, l, m, n) => i + j + k + l + m + n);
            // custom signature
            funcManager.AddFunction("MN", (Expression<Func<int, int, int, int, int, int, int, int>>)((i, j, k, l, m, n, o) => i + j + k + l + m + n + o));

            Assert.True(parser.Parse<object, bool>("M0() == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M1(1) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M2(1,1) == 2").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M3(1,1,1) == 3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M4(1,1,1,1) == 4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M5(1,1,1,1,1) == 5").Invoke(null));
            Assert.True(parser.Parse<object, bool>("M6(1,1,1,1,1,1) == 6").Invoke(null));
            Assert.True(parser.Parse<object, bool>("MN(1,1,1,1,1,1,1) == 7").Invoke(null));
        }

        [Fact]
        public void verify_methods_overloading() // overloading concept exists, when there are two methods of the same name, but different signature
        {
            // methods overloading is based on the number of arguments
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction("Whoami", () => "utility method");
            funcManager.AddFunction<int, string>("Whoami", i => $"utility method {i}");
            funcManager.AddFunction<int, string, string>("Whoami", (i, s) => $"utility method {i} - {s}");            

            Assert.True(parser.Parse<object, bool>("Whoami() == 'utility method'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Whoami(1) == 'utility method 1'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Whoami(2, 'final') == 'utility method 2 - final'").Invoke(null));
        }

        [Fact]
        public void verify_methods_overriding() // overriding concept exists, when there are two methods of the same name and signature, but different implementation
        {
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            // register utility methods
            funcManager.AddFunction("Whoami", () => "utility method");
            funcManager.AddFunction<int, string>("Whoami", i => $"utility method {i}");

            var model = new ModelWithMethods();

            // redefined model methods take precedence
            Assert.True(parser.Parse<ModelWithMethods, bool>("Whoami() == 'model method'").Invoke(model));
            Assert.True(parser.Parse<ModelWithMethods, bool>("Whoami(1) == 'model method 1'").Invoke(model));
        }

        [Fact]
        public void verify_toolchain_methods_ambiguity()
        {
            // since arguments types are not taken under consideration for methods overloading, following logic should fail
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction<int, string>("Whoami", i => $"utility method {i}");
            funcManager.AddFunction<string, string>("Whoami", s => $"utility method {s}");

            funcManager.AddFunction<string, string, string>("Glue", (s1, s2) => string.Concat(s1, s2));
            funcManager.AddFunction<int, int, string>("Glue", (i1, i2) => string.Concat(i1, i2));

            var e = Assert.Throws<ParseErrorException>(() => Assert.True(parser.Parse<object, bool>("Whoami(0) == 'utility method 0'").Invoke(null)));
            Assert.Equal("Function 'Whoami' accepting 1 argument is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Glue('a', 'b') == 'ab'").Invoke(null));
            Assert.Equal("Function 'Glue' accepting 2 arguments is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_context_methods_ambiguity()
        {
            // not only built-in, but also context extracted methods are subjected to the same rules
            var parser = new Parser();
            var model = new ModelWithAmbiguousMethods();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<ModelWithAmbiguousMethods, bool>("Whoami(0) == 'model method 0'").Invoke(model));
            Assert.Equal("Function 'Whoami' accepting 1 argument is ambiguous.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_implicit_type_conversion()
        {
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction<object, string>("Whoami", o => $"utility method {o}");
            funcManager.AddFunction<int, string, string>("Whoami", (i, s) => $"utility method {i} - {s}");

            Assert.True(parser.Parse<object, bool>("Whoami('0') == 'utility method 0'").Invoke(null)); // successful conversion from String to Object
            Assert.True(parser.Parse<object, bool>("Whoami(1, '2') == 'utility method 1 - 2'").Invoke(null)); // types matched, no conversion needed

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Whoami('1', '2') == 'utility method 1 - 2'").Invoke(null));
            Assert.Equal("Function 'Whoami' 1st argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            Assert.Equal(new Location(1, 8), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Whoami(1, 2) == 'utility method 1 - 2'").Invoke(null));
            Assert.Equal("Function 'Whoami' 2nd argument implicit conversion from 'System.Int32' to expected 'System.String' failed.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());
        }

        [Fact]
        public void function_arguments_order_identification_is_formatted_correctly()
        {
            var parser = new Parser();
            var model = new Model();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long('', 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 1st argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, '', 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 2nd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, 2, '', 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 3rd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, 2, 3, '', 5, 6, 7, 8, 9, 10, 11, 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 4th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, '', 12, 13)").Invoke(model));
            Assert.Equal("Function 'Long' 11th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, '', 13)").Invoke(model));
            Assert.Equal("Function 'Long' 12th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, '')").Invoke(model));
            Assert.Equal("Function 'Long' 13th argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
        }

        [Fact]
        public void verify_short_circuit_evaluation()
        {
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction<object, bool>("CastToBool", obj => (bool) obj);

            Assert.Throws<NullReferenceException>(() => parser.Parse<object, bool>("CastToBool(null)").Invoke(null));

            // below, the exception should not be thrown as above
            // reason? - first argument is suffient to determine the value of the expression so the second one is not going to be evaluated
            Assert.False(parser.Parse<object, bool>("false && CastToBool(null)").Invoke(null));
            Assert.True(parser.Parse<object, bool>("true || CastToBool(null)").Invoke(null));
        }

        [Fact]
        public void verify_enumeration_ambiguity()
        {
            var parser = new Parser();
            
            // ensure that this doesn't consider Dog and HotDog enums to be ambiguous
            Assert.True(parser.Parse<object, bool>("Dog.Collie == 0").Invoke(null));
            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Stability.High == 0").Invoke(null));
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
            Assert.True(parser.Parse<object, bool>("Dogs.Const == 0").Invoke(null));
            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Utility.Const == 'outside1'").Invoke(null));
            Assert.Equal(
                @"Constant 'Utility.Const' is ambiguous, found following:
'ExpressiveAnnotations.Tests.Utility.Const',
'ExpressiveAnnotations.Tests.Tools+Utility.Const'.",
                e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_invalid_identifier()
        {
            var parser = new Parser();
            var model = new Model();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("NotMe == 0").Invoke(model));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'NotMe' not known.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_naming_collisions()
        {
            var parser = new Parser();

            var one = new SampleOne();
            var two = new SampleTwo();

            Assert.True(parser.Parse<SampleOne, bool>("0 == Vehicle.Car").Invoke(one));
            Assert.Equal(0, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);
            Assert.True(parser.Parse<SampleOne, bool>("Vehicle == Vehicle.Car").Invoke(one));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);

            Assert.True(parser.Parse<SampleTwo, bool>("-1 == Vehicle.Car").Invoke(two));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(0, parser.GetConsts().Count);
            Assert.True(parser.Parse<SampleTwo, bool>("Vehicle.Car != ParserTest.Vehicle.Car").Invoke(two));
            Assert.Equal(1, parser.GetFields().Count);
            Assert.Equal(1, parser.GetConsts().Count);
        }

        [Fact]
        public void verify_toolchain_methods_logic()
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            Assert.True(parser.Parse<object, bool>("Now() > Today()").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Date(1985, 2, 20) < Date(1985, 2, 20, 0, 0, 1)").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Date(1, 1, 1) == Date(1, 1, 1, 0, 0, 0)").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ToDate('2016-04-27') == Date(2016, 4, 27)").Invoke(null));

            var dateModel = new {Date = new DateTime(2016, 4, 27)};
            Assert.True(parser.Parse<bool>(dateModel.GetType(), "ToDate('2016-04-27') == Date").Invoke(dateModel));

            Assert.True(parser.Parse<object, bool>("TimeSpan(1, 0, 0, 0) > TimeSpan(0, 1, 0, 0)").Invoke(null));
            //Assert.True(parser.Parse<object, bool>("(TimeSpan(0, 0, 0, 0)).TotalMilliseconds == 0").Invoke(null)); // foo().Prop - to be supported?
            //Assert.True(parser.Parse<object, bool>("(TimeSpan(1, 2, 3, 4)).TotalMilliseconds == 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Length('0123') == 4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Length('    ') == 4").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Length(null) == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Length('') == 0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Trim(' a b c ') == 'a b c'").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Trim(null) == null").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Trim('') == ''").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Concat(' a ', ' b ') == ' a  b '").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Concat(null, null) == ''").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Concat('', '') == ''").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Concat(' a ', ' b ', ' c ') == ' a  b  c '").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Concat(null, null, null) == ''").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Concat('', '', '') == ''").Invoke(null));

            Assert.True(parser.Parse<object, bool>("CompareOrdinal(' abc ', ' ABC ') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('a', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('a', 'A') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('A', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('a', 'b') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('b', 'a') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal(null, 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('a', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal(' ', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('a', ' ') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal(null, '') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal(null, null) == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinal('', '') == 0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase(' abc ', ' ABC ') == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('a', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('a', 'A') == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('A', 'a') == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('a', 'b') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('b', 'a') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase(null, 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('a', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase(' ', 'a') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('a', ' ') == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase(null, '') == -1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('', null) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase(null, null) == 0").Invoke(null));
            Assert.True(parser.Parse<object, bool>("CompareOrdinalIgnoreCase('', '') == 0").Invoke(null));

            Assert.True(parser.Parse<object, bool>("StartsWith(' ab c', ' A') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith(' ab c', ' a') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWith(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase(' ab c', ' A') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase(' ab c', ' a') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("StartsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("EndsWith(' ab c', ' C') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith(' ab c', ' c') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWith(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase(' ab c', ' C') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase(' ab c', ' c') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("EndsWithIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Contains(' ab c', 'B ') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains(' ab c', 'b ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Contains(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase(' ab c', 'B ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase(' ab c', 'b ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase(' ', ' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("ContainsIgnoreCase(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("IsNullOrWhiteSpace(' ') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNullOrWhiteSpace(null) == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNullOrWhiteSpace('') == true").Invoke(null));

            Assert.True(parser.Parse<object, bool>("IsDigitChain('0123456789') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsDigitChain('+0') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsDigitChain('-0') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsDigitChain(null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsDigitChain('') == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("IsNumber('0') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('0.0') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('10.10') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('0e0') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('.2') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('3.14') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('5e6') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('5e-6') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('5e+6') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('9.0E-10') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('.11e10') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('-0.3e-2') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('+0.3e-2') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('+0') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('-0') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('++0') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('--0') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('+-0') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber(null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsNumber('') == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("IsEmail('nickname@domain.com') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsEmail(null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsEmail('') == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("IsUrl('http://www.github.com/') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsUrl(null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsUrl('') == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>(@"IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>(@"IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>(@"IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>(@"IsRegexMatch('John\'s cat named ""\\\'""\n (Backslash Quote)', '^\\d+$') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsRegexMatch('', '') == true").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsRegexMatch(null, '') == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsRegexMatch('', null) == false").Invoke(null));
            Assert.True(parser.Parse<object, bool>("IsRegexMatch(null, null) == false").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Min(1) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Max(1) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Sum(1) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Average(1) == 1").Invoke(null));

            Assert.True(parser.Parse<object, bool>("Min(1, 2, 3) == 1").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Max(1, 2, 3) == 3").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Sum(1, 2, 3) == 6").Invoke(null));
            Assert.True(parser.Parse<object, bool>("Average(1, 2, 3) == 2").Invoke(null));

            var arrModel = new
            {
                SingleElementArray = new[] {1.0},
                MultipleElementsArray = new[] {1.0, 2, 3}
            };

            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Min(SingleElementArray) == 1").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Max(SingleElementArray) == 1").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Sum(SingleElementArray) == 1").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Average(SingleElementArray) == 1").Invoke(arrModel));

            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Min(MultipleElementsArray) == 1").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Max(MultipleElementsArray) == 3").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Sum(MultipleElementsArray) == 6").Invoke(arrModel));
            Assert.True(parser.Parse<bool>(arrModel.GetType(), "Average(MultipleElementsArray) == 2").Invoke(arrModel));

            Assert.True(parser.Parse<object, bool>("Guid('a1111111-1111-1111-1111-111111111111') == Guid('A1111111-1111-1111-1111-111111111111')").Invoke(null));

            var e = Assert.Throws<FormatException>(() => parser.Parse<object, bool>("Guid('abc') == Guid('abc')").Invoke(null));
            Assert.Equal("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", e.Message);
        }

        public static IEnumerable<object[]> BitwiseOperators
        {
            get { return new[] {"&", "|", "^", "<<", ">>"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("BitwiseOperators")]
        public void verify_type_mismatch_errors_for_bitwise_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"true {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Boolean' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"true {oper} 1").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Boolean' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"Now() {oper} Today()").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());
            
            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"YesNo.Yes {oper} YesNo.No").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.ParserTest+YesNo' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"1.1 {oper} 1").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Double' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_remaining_type_mismatch_errors_for_shift_operators()
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("true >> false").Invoke(new Model()));
            Assert.Equal("Operator '>>' cannot be applied to operands of type 'System.Boolean' and 'System.Boolean'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("true << false").Invoke(new Model()));
            Assert.Equal("Operator '<<' cannot be applied to operands of type 'System.Boolean' and 'System.Boolean'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
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
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"true {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Boolean' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"Now() {oper} Today()").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"1 {oper} 2").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"YesNo.Yes {oper} YesNo.No").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.ParserTest+YesNo' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} null").Invoke(new Model()));
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
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"0 {oper} '0'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"0.1 {oper} '0'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Double' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} 'asd'").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"NDate {oper} 'asd'").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Bag, bool>($"Lexer {oper} Parser").Invoke(new Bag {Lexer = new Lexer(), Parser = new Parser()}));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Analysis.Lexer' and 'ExpressiveAnnotations.Analysis.Parser'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Cash {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Decimal' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"Utility.Stability.High {oper} YesNo.Yes").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.Utility+Stability' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 24), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> RelationalOperators
        {
            get { return new[] {">", ">=", "<", "<="}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("RelationalOperators")]
        public void verify_type_mismatch_errors_for_inequality_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"'a' {oper} 'b'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"'asd' {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"NDate {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"NDate {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"SubModelObject {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Object' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 16), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"0 {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Span {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"Utility.Stability.High {oper} YesNo.Yes").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'ExpressiveAnnotations.Tests.Utility+Stability' and 'ExpressiveAnnotations.Tests.ParserTest+YesNo'.", e.Error);
            Assert.Equal(new Location(1, 24), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> AdditiveOperators
        {
            get { return new[] {"+", "-"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("AdditiveOperators")]
        public void verify_type_mismatch_errors_for_addition_and_subtraction_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"0 {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"NDate {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Span {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"NSpan {oper} 0").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Nullable`1[System.TimeSpan]' and 'System.Int32'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"0 {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Date {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.DateTime' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"Span {oper} SubModelObject").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.TimeSpan' and 'System.Object'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"{oper} true").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operand of type 'System.Boolean'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> MultiplicativeOperators
        {
            get { return new[] {"*", "/", "%"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("MultiplicativeOperators")]
        public void verify_type_mismatch_errors_for_multiplication_and_division_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"0 {oper} null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"'abc' {oper} 'abc'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>(
                $@"1 - 2
    - (6 / ((2{oper}'1.5' - 1) + 1)) * -2 
    + 1/2/1 == 3.50").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'System.Int32' and 'System.String'.", e.Error);
            Assert.Equal(new Location(2, 15), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>($"null {oper} null").Invoke(new Model()));
            Assert.Equal($"Operator '{oper}' cannot be applied to operands of type 'null' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_remaining_type_mismatch_errors_for_addition_operator()
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Date + Date").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("NDate + NDate").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.Nullable`1[System.DateTime]' and 'System.Nullable`1[System.DateTime]'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Date + NDate").Invoke(new Model()));
            Assert.Equal("Operator '+' cannot be applied to operands of type 'System.DateTime' and 'System.Nullable`1[System.DateTime]'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_remaining_type_mismatch_errors_for_subtraction_operator()
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("'abc' - 'abc'").Invoke(null));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("1 + 2 + 'abc' - 'abc' > 0").Invoke(null));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 15), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("'asd' - null").Invoke(new Model()));
            Assert.Equal("Operator '-' cannot be applied to operands of type 'System.String' and 'null'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());
        }

        public static IEnumerable<object[]> UnaryOperators
        {
            get { return new[] {"!", "~", "+", "-"}.Select(x => new object[] {x}); }
        }

        [Theory]
        [MemberData("UnaryOperators")]
        public void verify_type_mismatch_errors_for_unary_operators(string oper)
        {
            var parser = new Parser();
            parser.RegisterToolchain();

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"{oper}null").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operand of type 'null'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"{oper}'a'").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operand of type 'System.String'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>($"{oper}Today()").Invoke(null));
            Assert.Equal($"Operator '{oper}' cannot be applied to operand of type 'System.DateTime'.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());
        }

        [Fact]
        public void verify_various_parsing_errors()
        {
            var parser = new Parser();
            var funcManager = new FunctionsManager();
            parser.RegisterFunctionsProvider(funcManager);

            funcManager.AddFunction<int, int, int>("Max", (x, y) => Math.Max(x, y));
            funcManager.AddFunction<object>("GetObject", () => new object());

            var e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("1++ +1==2").Invoke(null));
            Assert.Equal("Unexpected token: '++'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("true # false").Invoke(null));
            Assert.Equal("Invalid token.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>(
                @"1 - 2

    - 6
    + 1/x[0]/1 == 3.50").Invoke(null));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'x' not known.", e.Error);
            Assert.Equal(new Location(4, 9), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("WriteLine('hello')").Invoke(null));
            Assert.Equal("Function 'WriteLine' not known.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("1 2").Invoke(null));
            Assert.Equal("Unexpected token: '2'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("(").Invoke(null));
            Assert.Equal("Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("(1+1").Invoke(null));
            Assert.Equal("Expected closing parenthesis. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("()").Invoke(null));
            Assert.Equal("Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected token: ')'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Max(").Invoke(null));
            Assert.Equal("Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 5), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Max(1 2").Invoke(null));
            Assert.Equal("Expected comma or closing parenthesis. Unexpected token: '2'.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Max(1.1)").Invoke(null));
            Assert.Equal("Function 'Max' accepting 1 argument not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("Max(1.1, 1.2, 'a')").Invoke(null));
            Assert.Equal("Function 'Max' accepting 3 arguments not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>(
                @"Max(1, 
      Max(1, 'a')) == 1.1").Invoke(null));
            Assert.Equal("Function 'Max' 2nd argument implicit conversion from 'System.String' to expected 'System.Int32' failed.", e.Error);
            Assert.Equal(new Location(2, 14), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Long(1)").Invoke(new Model()));
            Assert.Equal("Function 'Long' accepting 1 argument not found.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>(@"Max(1").Invoke(new Model()));
            Assert.Equal("Expected comma or closing parenthesis. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("1.1.1").Invoke(null));
            Assert.Equal("Unexpected token: '.1'.", e.Error); // "unexpected" is not "invalid" - token is valid, but used in wrong context
            Assert.Equal(new Location(1, 4), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0z").Invoke(null));
            Assert.Equal("Unexpected token: 'z'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0b").Invoke(null));
            Assert.Equal("Unexpected token: 'b'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0x").Invoke(null));
            Assert.Equal("Unexpected token: 'x'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0b2").Invoke(null));
            Assert.Equal("Unexpected token: 'b2'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0xG").Invoke(null));
            Assert.Equal("Unexpected token: 'xG'.", e.Error);
            Assert.Equal(new Location(1, 2), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("0xffffffffffff").Invoke(null));
            Assert.Equal("Integral constant is too large.", e.Error);
            Assert.Equal(new Location(1, 1), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("true ? true ").Invoke(null));
            Assert.Equal("Expected colon of ternary operator. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 13), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("true ? true . false").Invoke(null));
            Assert.Equal("Expected colon of ternary operator. Unexpected token: '.'.", e.Error);
            Assert.Equal(new Location(1, 13), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Items[0]").Invoke(new Model {Items = new List<Model> {new Model()}}));
            Assert.Equal("Indexing operation not supported. Subscript operator can be applied to either an array or a type declaring indexer.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Items[1.0]").Invoke(new Model()));
            Assert.Equal("Expected index of 'System.Int32' type. Type 'System.Double' cannot be implicitly converted.", e.Error);
            Assert.Equal(new Location(1, 7), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Items[0").Invoke(new Model()));
            Assert.Equal("Expected closing bracket. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 8), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Items[0+").Invoke(new Model()));
            Assert.Equal("Expected \"null\", bool, int, float, bin, hex, string, array or id. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 9), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Prop.+").Invoke(new Model()));
            Assert.Equal("Expected subproperty identifier. Unexpected token: '+'.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Prop.").Invoke(new Model()));
            Assert.Equal("Expected subproperty identifier. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 6), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<object, bool>("[0][1 > 0 ? 0*2.0 : 0^2] == 1").Invoke(null));
            Assert.Equal("Argument types must match.", e.Error);
            Assert.Equal(new Location(1, 11), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("Collection[true ? 0 : 1].Collection[true ? [0][0] : 1].Unknown == -2").Invoke(new Model()));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'Unknown' not known.", e.Error);
            Assert.Equal(new Location(1, 56), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("GetObject()[0]").Invoke(null));
            Assert.Equal("Indexing operation not supported. Subscript operator can be applied to either an array or a type declaring indexer.", e.Error);
            Assert.Equal(new Location(1, 12), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("SubModel.Unknown1.Unknown2 == 1").Invoke(new Model()));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'Unknown1' not known.", e.Error);
            Assert.Equal(new Location(1, 10), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("GetObject().Unknown1.Unknown2 == 1").Invoke(new Model()));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'Unknown1' not known.", e.Error);
            Assert.Equal(new Location(1, 13), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("[SubModel][0].Unknown1.Unknown2 == 1").Invoke(new Model()));
            Assert.Equal("Only public properties, constants and enums are accepted. Identifier 'Unknown1' not known.", e.Error);
            Assert.Equal(new Location(1, 15), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("[1").Invoke(new Model()));
            Assert.Equal("Expected comma or closing bracket. Unexpected end of expression.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());

            e = Assert.Throws<ParseErrorException>(() => parser.Parse<Model, bool>("[1)").Invoke(new Model()));
            Assert.Equal("Expected comma or closing bracket. Unexpected token: ')'.", e.Error);
            Assert.Equal(new Location(1, 3), e.Location, new LocationComparer());
        }

        [Fact]
        public void unicode_characters_are_supported()
        {
            var parser = new Parser();
            var model = new LocalModel {ąęćłńśóźż = "ąęćłńśóźż"};
            Assert.True(parser.Parse<LocalModel, bool>("ąęćłńśóźż == 'ąęćłńśóźż'").Invoke(model));
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

            public int[] IntArray { get; set; }
            public int[][] IntJaggedArray { get; set; }

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

            public int Sum(int a, int b, int c)
            {
                return a + b + c;
            }

            public double? Average(params int[] numbers)
            {
                return numbers.Any() ? numbers.Average() : (double?) null;
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
