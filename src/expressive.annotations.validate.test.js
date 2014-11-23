/// <reference path="./packages/jQuery.1.8.2/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="./packages/jQuery.Validation.1.10.0/Content/Scripts/jquery.validate.js" />
/// <reference path="./packages/Microsoft.jQuery.Unobtrusive.Validation.3.1.1/Content/Scripts/jquery.validate.unobtrusive.js" />
/// <reference path="./expressive.annotations.validate.js" />
//debugger; // enable firebug (preferably, check 'on for all web pages' option) for the debugger to launch 
(function($, window, ea) {

    window.module("type helper");

    test("verify_array_storage", function() {
        window.ok(ea.typeHelper.array.contains(["a"], "a"));
        window.ok(ea.typeHelper.array.contains(["a", "b"], "a"));
        window.ok(ea.typeHelper.array.contains(["a", "b"], "b"));
        window.ok(!ea.typeHelper.array.contains(["a", "b"], "c"));
    });

    test("verify_array_sanatization", function() {
        var array = ["a"];
        ea.typeHelper.array.sanatize(["a"], "");
        window.deepEqual(["a"], array);

        array = ["a", "a"];
        ea.typeHelper.array.sanatize(array, "a");
        window.deepEqual([], array);

        array = ["a", "b"];
        ea.typeHelper.array.sanatize(array, "");
        window.deepEqual(["a", "b"], array);

        array = ["a", "b", "c", "a", "b"];
        ea.typeHelper.array.sanatize(array, "b");
        window.deepEqual(["a", "c", "a"], array);
    });

    test("verify_type_parsing", function() {
        var result = ea.typeHelper.tryParse(undefined, "string");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid string.");

        result = ea.typeHelper.tryParse("asd", "bool");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid boolean.");

        result = ea.typeHelper.tryParse("", "numeric");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid float.");

        result = ea.typeHelper.tryParse("", "datetime");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.");

        result = ea.typeHelper.tryParse("", "guid");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");

        result = ea.typeHelper.tryParse("", "secret");
        window.equal(result.error, true);
        window.equal(result.msg, "Supported types: datetime, numeric, string, bool and guid. Invalid target type: secret");
    });

    test("verify_non_standard_date_format_parsing", function() {
        var expected = new Date("August, 11 2014").getTime();
        var actual = ea.typeHelper.tryParse("11/08/2014", "datetime");
        window.ok(actual != expected); // standard Date.parse fails to understand dd/mm/yyyy format
        window.ea.settings.parseDate = function(str) {
            var arr = str.split('/');
            var date = new Date(arr[2], arr[1] - 1, arr[0]);
            return date.getTime();
        };
        actual = ea.typeHelper.tryParse("11/08/2014", "datetime");
        window.ok(actual == expected);
        window.ea.settings.parseDate = undefined; // reset state for further tests
    });

    test("verify_string_formatting", function() {
        window.equal(ea.typeHelper.string.format("{0}", "a"), "a");
        window.equal(ea.typeHelper.string.format("{0}{1}", "a", "b"), "ab");
        window.equal(ea.typeHelper.string.format("{0}{0}", "a", "b"), "aa");
        window.equal(ea.typeHelper.string.format("{0}{0}", "a"), "aa");

        window.equal(ea.typeHelper.string.format("{0}", ["a"]), "a");
        window.equal(ea.typeHelper.string.format("{0}{1}", ["a", "b"]), "ab");
        window.equal(ea.typeHelper.string.format("{0}{0}", ["a", "b"]), "aa");
        window.equal(ea.typeHelper.string.format("{0}{0}", ["a"]), "aa");
    });

    test("verify_string_parsing", function() {
        window.equal(ea.typeHelper.string.tryParse("abc"), "abc");
        window.equal(ea.typeHelper.string.tryParse(123), "123");
        window.equal(ea.typeHelper.string.tryParse(false), "false");

        var result = ea.typeHelper.string.tryParse(undefined);
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid string.");
    });

    test("verify_bool_parsing", function() {
        window.equal(ea.typeHelper.bool.tryParse(false), false);
        window.equal(ea.typeHelper.bool.tryParse("false"), false);
        window.equal(ea.typeHelper.bool.tryParse("False"), false);
        window.equal(ea.typeHelper.bool.tryParse(true), true);
        window.equal(ea.typeHelper.bool.tryParse("true"), true);
        window.equal(ea.typeHelper.bool.tryParse("True"), true);

        var result = ea.typeHelper.bool.tryParse("asd");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid boolean.");
    });

    test("verify_float_parsing", function() {
        // integer literals
        window.equal(ea.typeHelper.float.tryParse("-1"), -1); // negative integer string
        window.equal(ea.typeHelper.float.tryParse("0"), 0); // zero string
        window.equal(ea.typeHelper.float.tryParse("1"), 1); // positive integer string
        window.equal(ea.typeHelper.float.tryParse(-1), -1); // negative integer number
        window.equal(ea.typeHelper.float.tryParse(0), 0); // zero integer number
        window.equal(ea.typeHelper.float.tryParse(1), 1); // positive integer number
        window.equal(ea.typeHelper.float.tryParse(0xFF), 255); // hexadecimal integer literal

        // floating-point literals
        window.equal(ea.typeHelper.float.tryParse("-1.1"), -1.1); // negative floating point string
        window.equal(ea.typeHelper.float.tryParse("1.1"), 1.1); // positive floating point string
        window.equal(ea.typeHelper.float.tryParse(-1.1), -1.1); // negative floating point number
        window.equal(ea.typeHelper.float.tryParse(1.1), 1.1); // positive floating point number
        window.equal(ea.typeHelper.float.tryParse("314e-2"), 3.14); // exponential notation string 
        window.equal(ea.typeHelper.float.tryParse(314e-2), 3.14); // exponential notation

        // non-numeric values
        var result = ea.typeHelper.float.tryParse(""); // empty string
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid float.");

        window.ok(ea.typeHelper.float.tryParse(" ").error); // whitespace character
        window.ok(ea.typeHelper.float.tryParse("\t").error); // tab characters
        window.ok(ea.typeHelper.float.tryParse("asd").error); // non-numeric character string
        window.ok(ea.typeHelper.float.tryParse("true").error); // boolean true
        window.ok(ea.typeHelper.float.tryParse("false").error); // boolean false
        window.ok(ea.typeHelper.float.tryParse("asd123").error); // number with preceding non-numeric characters
        window.ok(ea.typeHelper.float.tryParse("123asd").error); // number with trailling non-numeric characters
        window.ok(ea.typeHelper.float.tryParse(undefined).error); // undefined value
        window.ok(ea.typeHelper.float.tryParse(null).error); // null value
        window.ok(ea.typeHelper.float.tryParse(NaN).error); // NaN value
        window.ok(ea.typeHelper.float.tryParse(Infinity).error); // infinity primitive
        window.ok(ea.typeHelper.float.tryParse(+Infinity).error); // positive Infinity
        window.ok(ea.typeHelper.float.tryParse(-Infinity).error); // negative Infinity
        window.ok(ea.typeHelper.float.tryParse(new Date(Date.now())).error); // date object
        window.ok(ea.typeHelper.float.tryParse({}).error); // empty object        
    });

    test("verify_date_parsing", function() {
        var now = Date.now();
        window.ok(ea.typeHelper.date.tryParse(new Date(now)) == new Date(now).getTime());
        window.ok(ea.typeHelper.date.tryParse("Aug 9, 1995") == new Date("Aug 9, 1995").getTime());
        window.ok(ea.typeHelper.date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT") == 807926400000);
        window.ok(ea.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT") == 0);
        window.ok(ea.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400") == 14400000);

        var result = ea.typeHelper.date.tryParse("");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.");
    });

    test("verify_guid_parsing", function() {
        window.equal(ea.typeHelper.guid.tryParse("a1111111-1111-1111-1111-111111111111"), "A1111111-1111-1111-1111-111111111111");

        var result = ea.typeHelper.guid.tryParse("");
        window.equal(result.error, true);
        window.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
    });

    test("verify_numeric_recognition", function() {
        window.ok(ea.typeHelper.isNumeric(1));
        window.ok(!ea.typeHelper.isNumeric(NaN));
        window.ok(!ea.typeHelper.isNumeric("1"));
    });

    test("verify_date_recognition", function() {
        window.ok(ea.typeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")));
        window.ok(!ea.typeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"));
        window.ok(!ea.typeHelper.isDate(807926400000));
    });

    test("verify_string_recognition", function() {
        window.ok(ea.typeHelper.isString(""));
        window.ok(ea.typeHelper.isString("123"));
        window.ok(!ea.typeHelper.isString(123));
        window.ok(!ea.typeHelper.isString({}));
        window.ok(!ea.typeHelper.isString(null));
        window.ok(!ea.typeHelper.isString(undefined));
    });

    test("verify_bool_recognition", function() {
        window.ok(ea.typeHelper.isBool(true));
        window.ok(!ea.typeHelper.isBool("true"));
        window.ok(!ea.typeHelper.isBool(0));
    });

    window.module("model helper");

    test("verify_model_evaluation", function() {
        var model = ea.modelHelper.deserializeObject(null, null, { "Number": 123, "Stability.High": 0 }, null);        
        with (model) {
            window.ok(eval("Number - 23 == 100 && Stability.High == 0"));
        }
    });

    window.module("toolchain");

    test("verify_methods_overloading", function() {
        var m = ea.toolchain.methods;

        ea.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        ea.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });
        ea.toolchain.addMethod('Whoami', function(i, s) {
            return 'method A' + i + ' - ' + s;
        });

        window.ok(m.Whoami() == 'method A');
        window.ok(m.Whoami(1) == 'method A1');
        window.ok(m.Whoami(2, 'final') == 'method A2 - final');
    });

    test("verify_methods_overriding", function() {
        var m = ea.toolchain.methods;

        ea.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        ea.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });

        window.ok(m.Whoami() == 'method A');
        window.ok(m.Whoami(1) == 'method A1');

        // redefine methods
        ea.toolchain.addMethod('Whoami', function() {
            return 'method B';
        });
        ea.toolchain.addMethod('Whoami', function(i) {
            return 'method B' + i;
        });

        window.ok(m.Whoami() == 'method B');
        window.ok(m.Whoami(1) == 'method B1');
    });

    test("verify_toolchain_methods_logic", function() {

        var o = {};
        ea.toolchain.registerMethods(o);
        var m = ea.toolchain.methods;

        window.ok(m.Now() > m.Today());
        window.ok(m.Date(1985, 2, 20) < m.Date(1985, 2, 20, 0, 0, 1));

        window.ok(m.Length('0123') == 4);
        window.ok(m.Length('    ') == 4);
        window.ok(m.Length(null) == 0);
        window.ok(m.Length('') == 0);

        window.ok(m.Trim(' a b c ') == 'a b c');
        window.ok(m.Trim(null) == null);
        window.ok(m.Trim('') == '');

        window.ok(m.Concat(' a ', ' b ') == ' a  b ');
        window.ok(m.Concat(null, null) == '');
        window.ok(m.Concat('', '') == '');

        window.ok(m.Concat(' a ', ' b ', ' c ') == ' a  b  c ');
        window.ok(m.Concat(null, null, null) == '');
        window.ok(m.Concat('', '', '') == '');

        window.ok(m.CompareOrdinal(' abc ', ' ABC ') == 1);
        window.ok(m.CompareOrdinal('a', 'a') == 0);
        window.ok(m.CompareOrdinal('a', 'A') == 1);
        window.ok(m.CompareOrdinal('A', 'a') == -1);
        window.ok(m.CompareOrdinal('a', 'b') == -1);
        window.ok(m.CompareOrdinal('b', 'a') == 1);
        window.ok(m.CompareOrdinal(null, 'a') == -1);
        window.ok(m.CompareOrdinal('a', null) == 1);
        window.ok(m.CompareOrdinal(' ', 'a') == -1);
        window.ok(m.CompareOrdinal('a', ' ') == 1);
        window.ok(m.CompareOrdinal(null, '') == -1);
        window.ok(m.CompareOrdinal('', null) == 1);
        window.ok(m.CompareOrdinal(null, null) == 0);
        window.ok(m.CompareOrdinal('', '') == 0);

        window.ok(m.CompareOrdinalIgnoreCase(' abc ', ' ABC ') == 0);
        window.ok(m.CompareOrdinalIgnoreCase('a', 'a') == 0);
        window.ok(m.CompareOrdinalIgnoreCase('a', 'A') == 0);
        window.ok(m.CompareOrdinalIgnoreCase('A', 'a') == 0);
        window.ok(m.CompareOrdinalIgnoreCase('a', 'b') == -1);
        window.ok(m.CompareOrdinalIgnoreCase('b', 'a') == 1);
        window.ok(m.CompareOrdinalIgnoreCase(null, 'a') == -1);
        window.ok(m.CompareOrdinalIgnoreCase('a', null) == 1);
        window.ok(m.CompareOrdinalIgnoreCase(' ', 'a') == -1);
        window.ok(m.CompareOrdinalIgnoreCase('a', ' ') == 1);
        window.ok(m.CompareOrdinalIgnoreCase(null, '') == -1);
        window.ok(m.CompareOrdinalIgnoreCase('', null) == 1);
        window.ok(m.CompareOrdinalIgnoreCase(null, null) == 0);
        window.ok(m.CompareOrdinalIgnoreCase('', '') == 0);

        window.ok(m.StartsWith(' ab c', ' A') == false);
        window.ok(m.StartsWith(' ab c', ' a') == true);
        window.ok(m.StartsWith(' ', ' ') == true);
        window.ok(m.StartsWith('', '') == true);
        window.ok(m.StartsWith(null, '') == false);
        window.ok(m.StartsWith('', null) == false);
        window.ok(m.StartsWith(null, null) == false);

        window.ok(m.StartsWithIgnoreCase(' ab c', ' A') == true);
        window.ok(m.StartsWithIgnoreCase(' ab c', ' a') == true);
        window.ok(m.StartsWithIgnoreCase(' ', ' ') == true);
        window.ok(m.StartsWithIgnoreCase('', '') == true);
        window.ok(m.StartsWithIgnoreCase(null, '') == false);
        window.ok(m.StartsWithIgnoreCase('', null) == false);
        window.ok(m.StartsWithIgnoreCase(null, null) == false);

        window.ok(m.EndsWith(' ab c', ' C') == false);
        window.ok(m.EndsWith(' ab c', ' c') == true);
        window.ok(m.EndsWith(' ', ' ') == true);
        window.ok(m.EndsWith('', '') == true);
        window.ok(m.EndsWith(null, '') == false);
        window.ok(m.EndsWith('', null) == false);
        window.ok(m.EndsWith(null, null) == false);

        window.ok(m.EndsWithIgnoreCase(' ab c', ' C') == true);
        window.ok(m.EndsWithIgnoreCase(' ab c', ' c') == true);
        window.ok(m.EndsWithIgnoreCase(' ', ' ') == true);
        window.ok(m.EndsWithIgnoreCase('', '') == true);
        window.ok(m.EndsWithIgnoreCase(null, '') == false);
        window.ok(m.EndsWithIgnoreCase('', null) == false);
        window.ok(m.EndsWithIgnoreCase(null, null) == false);

        window.ok(m.Contains(' ab c', 'B ') == false);
        window.ok(m.Contains(' ab c', 'b ') == true);
        window.ok(m.Contains(' ', ' ') == true);
        window.ok(m.Contains('', '') == true);
        window.ok(m.Contains(null, '') == false);
        window.ok(m.Contains('', null) == false);
        window.ok(m.Contains(null, null) == false);

        window.ok(m.ContainsIgnoreCase(' ab c', 'B ') == true);
        window.ok(m.ContainsIgnoreCase(' ab c', 'b ') == true);
        window.ok(m.ContainsIgnoreCase(' ', ' ') == true);
        window.ok(m.ContainsIgnoreCase('', '') == true);
        window.ok(m.ContainsIgnoreCase(null, '') == false);
        window.ok(m.ContainsIgnoreCase('', null) == false);
        window.ok(m.ContainsIgnoreCase(null, null) == false);

        window.ok(m.IsNullOrWhiteSpace(' ') == true);
        window.ok(m.IsNullOrWhiteSpace(null) == true);
        window.ok(m.IsNullOrWhiteSpace('') == true);

        window.ok(m.IsDigitChain('0123456789') == true);
        window.ok(m.IsDigitChain(null) == false);
        window.ok(m.IsDigitChain('') == false);

        window.ok(m.IsNumber('-0.3e-2') == true);
        window.ok(m.IsNumber(null) == false);
        window.ok(m.IsNumber('') == false);

        window.ok(m.IsEmail('nickname@domain.com') == true);
        window.ok(m.IsEmail(null) == false);
        window.ok(m.IsEmail('') == false);

        window.ok(m.IsUrl('http://www.github.com/') == true);
        window.ok(m.IsUrl(null) == false);
        window.ok(m.IsUrl('') == false);

        window.ok(m.IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == true);
        window.ok(m.IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false);
        window.ok(m.IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$') == false);
        window.ok(m.IsRegexMatch('', '') == true);
        window.ok(m.IsRegexMatch(null, '') == false);
        window.ok(m.IsRegexMatch('', null) == false);
        window.ok(m.IsRegexMatch(null, null) == false);

        window.ok(m.Guid('a1111111-1111-1111-1111-111111111111') == m.Guid('A1111111-1111-1111-1111-111111111111'));
    });

}($, window, window.ea.___6BE7863DC1DB4AFAA61BB53FF97FE169));
