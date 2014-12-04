/// <reference path="./packages/jQuery.1.8.2/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="./packages/jQuery.Validation.1.10.0/Content/Scripts/jquery.validate.js" />
/// <reference path="./packages/Microsoft.jQuery.Unobtrusive.Validation.3.1.1/Content/Scripts/jquery.validate.unobtrusive.js" />
/// <reference path="./expressive.annotations.validate.js" />

(function($, window, ea) {    
    // equal( actual, expected [, message ] )
    window.module("type helper");

    test("verify_array_storage", function() {
        // debugger; // enable firebug (preferably, check 'on for all web pages' option) for the debugger to launch
        window.ok(ea.typeHelper.array.contains(["a"], "a"), "single element array contains its only item");
        window.ok(ea.typeHelper.array.contains(["a", "b"], "a"), "multiple elements array contains its first item");
        window.ok(ea.typeHelper.array.contains(["a", "b"], "b"), "multiple elements array contains its last item");
        window.ok(!ea.typeHelper.array.contains(["a", "b"], "c"), "multiple elements array does not contain unknown item");
    });

    test("verify_object_keys_extraction", function() {
        var assocArray = [];
        assocArray["one"] = "lorem";
        assocArray["two"] = "ipsum";
        assocArray["three"] = "dolor";
        var keys = ea.typeHelper.object.keys(assocArray);
        window.deepEqual(keys, ["one", "two", "three"], "keys of associative array properly extracted");

        var model = {
            one: "lorem",
            two: "ipsum",
            three: "dolor"
        };
        keys = ea.typeHelper.object.keys(model);
        window.deepEqual(keys, ["one", "two", "three"], "names of object properties properly extracted");
    });

    test("verify_type_parsing", function() {
        var result = ea.typeHelper.tryParse(undefined, "string");
        window.ok(result.error, "broken string parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid string.", "broken string parsing error message thrown");

        result = ea.typeHelper.tryParse("asd", "bool");
        window.ok(result.error, "broken bool parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid boolean.", "broken bool parsing error message thrown");

        result = ea.typeHelper.tryParse("", "numeric");
        window.ok(result.error, "broken number parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid float.", "broken number parsing error message thrown");

        result = ea.typeHelper.tryParse("", "datetime");
        window.ok(result.error, "broken datetime parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "broken datetime parsing error message thrown");

        result = ea.typeHelper.tryParse("", "guid");
        window.ok(result.error, "broken guid parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "broken guid parsing error message thrown");

        result = ea.typeHelper.tryParse("{", "object");
        window.ok(result.error, "broken json object parsing error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid JSON. SyntaxError: JSON.parse: end of data while reading object contents at line 1 column 2 of the JSON data", "broken json object parsing error message thrown");
    });

    test("verify_non_standard_date_format_parsing", function() {
        var expected = new Date("August, 11 2014").getTime();
        var actual = ea.typeHelper.tryParse("11/08/2014", "datetime");
        window.notEqual(actual, expected, "default date parse fails to understand non-standard dd/mm/yyyy format");
        window.ea.settings.parseDate = function(str) {
            var arr = str.split('/');
            var date = new Date(arr[2], arr[1] - 1, arr[0]);
            return date.getTime();
        };
        actual = ea.typeHelper.tryParse("11/08/2014", "datetime");
        window.equal(actual, expected, "overriden parseDate properly handles non-standard dd/mm/yyyy format");

        window.ea.settings.parseDate = function(str) {
            return NaN; // simulate broken parsing logic
        };
        var result = ea.typeHelper.tryParse("11/08/2014", "datetime");
        window.ok(result.error, "overriden parseDate error thrown");
        window.equal(result.msg, "Custom date parsing is broken - number of milliseconds since January 1, 1970, 00:00:00 UTC expected to be returned.", "overriden parseDate error message thrown");

        window.ea.settings.parseDate = undefined; // reset state for further tests
    });

    test("verify_non_standard_object_format_parsing", function() {
        var expected = $.parseXML("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>");
        var actual = ea.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object");
        window.notDeepEqual(actual, expected, "default object parse fails to understand non-json format");
        window.ea.settings.parseObject = function(str) {
            return $.parseXML(str);
        };
        actual = ea.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object");
        window.deepEqual(actual, expected, "overriden parseObject properly handles non-json format");

        window.ea.settings.parseObject = undefined; // reset state for further tests
    });

    test("verify_string_formatting", function() {
        window.equal(ea.typeHelper.string.format("{0}", "a"), "a", "string.format({0}, 'a') succeed");
        window.equal(ea.typeHelper.string.format("{0}{1}", "a", "b"), "ab", "string.format({0}{1}, 'a', 'b') succeed");
        window.equal(ea.typeHelper.string.format("{0}{0}", "a", "b"), "aa", "string.format({0}{0}, 'a', 'b') succeed");
        window.equal(ea.typeHelper.string.format("{0}{0}", "a"), "aa", "string.format({0}{0}, 'a') succeed");

        window.equal(ea.typeHelper.string.format("{0}", ["a"]), "a", "string.format({0}, ['a']) succeed");
        window.equal(ea.typeHelper.string.format("{0}{1}", ["a", "b"]), "ab", "string.format({0}{1}, ['a', 'b']) succeed");
        window.equal(ea.typeHelper.string.format("{0}{0}", ["a", "b"]), "aa", "string.format({0}{0}, ['a', 'b']) succeed");
        window.equal(ea.typeHelper.string.format("{0}{0}", ["a"]), "aa", "string.format({0}{0}, ['a']) succeed");
    });

    test("verify_string_parsing", function() {
        window.equal(ea.typeHelper.string.tryParse("abc"), "abc", "string to string parse succeed");
        window.equal(ea.typeHelper.string.tryParse(123), "123", "int to string parse succeed");
        window.equal(ea.typeHelper.string.tryParse(0.123), "0.123", "float to string parse succeed");
        window.equal(ea.typeHelper.string.tryParse(false), "false", "bool to string parse succeed");

        var result = ea.typeHelper.string.tryParse(undefined);
        window.ok(result.error, "undefined to string parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid string.", "undefined to string parse error message thrown");
    });

    test("verify_bool_parsing", function() {
        window.equal(ea.typeHelper.bool.tryParse(false), false, "false bool to bool parse succeed");
        window.equal(ea.typeHelper.bool.tryParse("false"), false, "'false' string to bool parse succeed");
        window.equal(ea.typeHelper.bool.tryParse("False"), false, "'False' string to bool parse succeed");
        window.equal(ea.typeHelper.bool.tryParse(true), true, "true bool to bool parse succeed");
        window.equal(ea.typeHelper.bool.tryParse("true"), true, "'true' string to bool parse succeed");
        window.equal(ea.typeHelper.bool.tryParse("True"), true, "'True' string to bool parse succeed");

        var result = ea.typeHelper.bool.tryParse("asd");
        window.ok(result.error, "random string to bool parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid boolean.", "random string to bool parse error message thrown");
    });

    test("verify_float_parsing", function() {
        // integer literals
        window.equal(ea.typeHelper.float.tryParse("-1"), -1, "negative integer string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse("0"), 0, "zero string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse("1"), 1, "positive integer string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(-1), -1, "negative integer number to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(0), 0, "zero integer number to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(1), 1, "positive integer number to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(0xFF), 255, "hexadecimal integer literal to float parse succeed");

        // floating-point literals
        window.equal(ea.typeHelper.float.tryParse("-1.1"), -1.1, "negative floating point string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse("1.1"), 1.1, "positive floating point string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(-1.1), -1.1, "negative floating point number to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(1.1), 1.1, "positive floating point number to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse("314e-2"), 3.14, "exponential notation string to float parse succeed");
        window.equal(ea.typeHelper.float.tryParse(314e-2), 3.14, "exponential notation number to float parse succeed");

        // non-numeric values
        var result = ea.typeHelper.float.tryParse(""); // empty string
        window.ok(result.error, "empty string to float parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid float.", "empty string to float parse error message thrown");

        window.ok(ea.typeHelper.float.tryParse(" ").error, "whitespace character to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("\t").error, "tab character to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("asd").error, "non-numeric character string to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("true").error, "boolean true to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("false").error, "boolean false to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("asd123").error, "number with preceding non-numeric characters to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse("123asd").error, "number with trailling non-numeric characters to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(undefined).error, "undefined value to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(null).error, "null value to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(NaN).error, "NaN value to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(Infinity).error, "Infinity primitive to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(+Infinity).error, "positive Infinity to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(-Infinity).error, "negative Infinity to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse(new Date(Date.now())).error, "date object to float parse error thrown");
        window.ok(ea.typeHelper.float.tryParse({}).error, "empty object to float parse error thrown");
    });

    test("verify_date_parsing", function() {
        var now = Date.now();
        window.equal(ea.typeHelper.date.tryParse(new Date(now)), new Date(now).getTime(), "date object to date parse succeed");
        window.equal(ea.typeHelper.date.tryParse("Aug 9, 1995"), new Date("Aug 9, 1995").getTime(), "casual date string to date parse succeed");
        window.equal(ea.typeHelper.date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), 807926400000, "ISO date string to date parse succeed");
        window.equal(ea.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), 0, "Jan 1st, 1970 ISO date string to date parse succeed");
        window.equal(ea.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), 14400000, "4h shift to Jan 1st, 1970 ISO date string to date parse succeed");

        var result = ea.typeHelper.date.tryParse("");
        window.ok(result.error, "empty string to date parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "empty string to date parse error message thrown");
    });

    test("verify_guid_parsing", function() {
        window.equal(ea.typeHelper.guid.tryParse("a1111111-1111-1111-1111-111111111111"), "A1111111-1111-1111-1111-111111111111");

        var result = ea.typeHelper.guid.tryParse("");
        window.ok(result.error, "empty string to guid parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "empty string to guid parse error message thrown");
    });

    test("verify_object_parsing", function() {
        var jsonString = '{"Val":"a","Arr":[1,2]}';
        window.deepEqual(ea.typeHelper.object.tryParse(jsonString), $.parseJSON(jsonString));

        var result = ea.typeHelper.object.tryParse("{");
        window.ok(result.error, "broken json string to object parse error thrown");
        window.equal(result.msg, "Given value was not recognized as a valid JSON. SyntaxError: JSON.parse: end of data while reading object contents at line 1 column 2 of the JSON data", "broken json string to object parse error message thrown");
    });

    test("verify_numeric_type_recognition", function() {
        window.ok(ea.typeHelper.isNumeric(-1), "negative integer recognized as number");
        window.ok(ea.typeHelper.isNumeric(1), "positive integer recognized as number");
        window.ok(ea.typeHelper.isNumeric(0.123), "float recognized as number");

        window.ok(!ea.typeHelper.isNumeric(NaN), "NaN not recognized as number");
        window.ok(!ea.typeHelper.isNumeric("1"), "integer string not recognized as number");
    });

    test("verify_date_type_recognition", function() {
        window.ok(ea.typeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")), "date object recognized as date");

        window.ok(!ea.typeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"), "date string not recognized as date");
        window.ok(!ea.typeHelper.isDate(807926400000), "float (ticks number) not recognized as date");
    });

    test("verify_string_type_recognition", function() {
        window.ok(ea.typeHelper.isString(""), "empty string recognized as string");
        window.ok(ea.typeHelper.isString("123"), "random string recognized as string");

        window.ok(!ea.typeHelper.isString(123), "integer not recognized as string");
        window.ok(!ea.typeHelper.isString({}), "empty object not recognized as string");
        window.ok(!ea.typeHelper.isString(null), "null not recognized as string");
        window.ok(!ea.typeHelper.isString(undefined), "undefined not recognized as string");
    });

    test("verify_bool_type_recognition", function() {
        window.ok(ea.typeHelper.isBool(true), "true bool recognized as bool");
        window.ok(ea.typeHelper.isBool(false), "false bool recognized as bool");

        window.ok(!ea.typeHelper.isBool("true"), "'true' string not recognized as bool");
        window.ok(!ea.typeHelper.isBool("True"), "'True' string not recognized as bool");
        window.ok(!ea.typeHelper.isBool("false"), "'false' string not recognized as bool");
        window.ok(!ea.typeHelper.isBool("False"), "'False' string not recognized as bool");
        window.ok(!ea.typeHelper.isBool(""), "empty string not recognized as bool");
        window.ok(!ea.typeHelper.isBool(0), "0 integer not recognized as bool");        
        window.ok(!ea.typeHelper.isBool(1), "positive integer not recognized as bool");
        window.ok(!ea.typeHelper.isBool(-1), "negative integer not recognized as bool");
        window.ok(!ea.typeHelper.isBool({}), "empty object not recognized as bool");
        window.ok(!ea.typeHelper.isBool(null), "null not recognized as bool");
        window.ok(!ea.typeHelper.isBool(undefined), "undefined not recognized as bool");
    });

    test("verify_guid_string_recognition", function () {
        window.ok(ea.typeHelper.isGuid("541422A6-FEAD-4CD9-8466-5419AA63DBE1"), "uppercased guid string recognized as guid");
        window.ok(ea.typeHelper.isGuid("541422a6-fead-4cd9-8466-5419aa63dbe1"), "lowercased guid string recognized as guid");

        window.ok(!ea.typeHelper.isGuid(1), "integer not recognized as guid");
        window.ok(!ea.typeHelper.isGuid(""), "empty string not recognized as guid");
    });

    window.module("model helper");

    test("verify_model_evaluation", function() {
        var model = ea.modelHelper.deserializeObject(null, null, { "Number": 123, "Stability.High": 0 }, null);        
        with (model) {
            window.ok(eval("Number - 23 == 100 && Stability.High == 0"), "sample model deserialized and evaluated correctly");
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

        window.equal(m.Whoami(), 'method A');
        window.equal(m.Whoami(1), 'method A1');
        window.equal(m.Whoami(2, 'final'), 'method A2 - final');
    });

    test("verify_methods_overriding", function() {
        var m = ea.toolchain.methods;

        ea.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        ea.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });

        window.equal(m.Whoami(), 'method A');
        window.equal(m.Whoami(1), 'method A1');

        // redefine methods
        ea.toolchain.addMethod('Whoami', function() {
            return 'method B';
        });
        ea.toolchain.addMethod('Whoami', function(i) {
            return 'method B' + i;
        });

        window.equal(m.Whoami(), 'method B');
        window.equal(m.Whoami(1), 'method B1');
    });

    test("verify_toolchain_methods_logic", function() {

        var o = {};
        ea.toolchain.registerMethods(o);
        var m = ea.toolchain.methods;

        window.ok(m.Now() > m.Today());
        window.ok(m.Date(1985, 2, 20) < m.Date(1985, 2, 20, 0, 0, 1));

        window.equal(m.Length('0123'), 4);
        window.equal(m.Length('    '), 4);
        window.equal(m.Length(null), 0);
        window.equal(m.Length(''), 0);

        window.equal(m.Trim(' a b c '), 'a b c');
        window.equal(m.Trim(null), null);
        window.equal(m.Trim(''), '');

        window.equal(m.Concat(' a ', ' b '), ' a  b ');
        window.equal(m.Concat(null, null), '');
        window.equal(m.Concat('', ''), '');

        window.equal(m.Concat(' a ', ' b ', ' c '), ' a  b  c ');
        window.equal(m.Concat(null, null, null), '');
        window.equal(m.Concat('', '', ''), '');

        window.equal(m.CompareOrdinal(' abc ', ' ABC '), 1);
        window.equal(m.CompareOrdinal('a', 'a'), 0);
        window.equal(m.CompareOrdinal('a', 'A'), 1);
        window.equal(m.CompareOrdinal('A', 'a'), -1);
        window.equal(m.CompareOrdinal('a', 'b'), -1);
        window.equal(m.CompareOrdinal('b', 'a'), 1);
        window.equal(m.CompareOrdinal(null, 'a'), -1);
        window.equal(m.CompareOrdinal('a', null), 1);
        window.equal(m.CompareOrdinal(' ', 'a'), -1);
        window.equal(m.CompareOrdinal('a', ' '), 1);
        window.equal(m.CompareOrdinal(null, ''), -1);
        window.equal(m.CompareOrdinal('', null), 1);
        window.equal(m.CompareOrdinal(null, null), 0);
        window.equal(m.CompareOrdinal('', ''), 0);

        window.equal(m.CompareOrdinalIgnoreCase(' abc ', ' ABC '), 0);
        window.equal(m.CompareOrdinalIgnoreCase('a', 'a'), 0);
        window.equal(m.CompareOrdinalIgnoreCase('a', 'A'), 0);
        window.equal(m.CompareOrdinalIgnoreCase('A', 'a'), 0);
        window.equal(m.CompareOrdinalIgnoreCase('a', 'b'), -1);
        window.equal(m.CompareOrdinalIgnoreCase('b', 'a'), 1);
        window.equal(m.CompareOrdinalIgnoreCase(null, 'a'), -1);
        window.equal(m.CompareOrdinalIgnoreCase('a', null), 1);
        window.equal(m.CompareOrdinalIgnoreCase(' ', 'a'), -1);
        window.equal(m.CompareOrdinalIgnoreCase('a', ' '), 1);
        window.equal(m.CompareOrdinalIgnoreCase(null, ''), -1);
        window.equal(m.CompareOrdinalIgnoreCase('', null), 1);
        window.equal(m.CompareOrdinalIgnoreCase(null, null), 0);
        window.equal(m.CompareOrdinalIgnoreCase('', ''), 0);
        
        window.ok(!m.StartsWith(' ab c', ' A'));
        window.ok(m.StartsWith(' ab c', ' a'));
        window.ok(m.StartsWith(' ', ' '));
        window.ok(m.StartsWith('', ''));
        window.ok(!m.StartsWith(null, ''));
        window.ok(!m.StartsWith('', null));
        window.ok(!m.StartsWith(null, null));

        window.ok(m.StartsWithIgnoreCase(' ab c', ' A'));
        window.ok(m.StartsWithIgnoreCase(' ab c', ' a'));
        window.ok(m.StartsWithIgnoreCase(' ', ' '));
        window.ok(m.StartsWithIgnoreCase('', ''));
        window.ok(!m.StartsWithIgnoreCase(null, ''));
        window.ok(!m.StartsWithIgnoreCase('', null));
        window.ok(!m.StartsWithIgnoreCase(null, null));
        
        window.ok(!m.EndsWith(' ab c', ' C'));
        window.ok(m.EndsWith(' ab c', ' c'));
        window.ok(m.EndsWith(' ', ' '));
        window.ok(m.EndsWith('', ''));
        window.ok(!m.EndsWith(null, ''));
        window.ok(!m.EndsWith('', null));
        window.ok(!m.EndsWith(null, null));

        window.ok(m.EndsWithIgnoreCase(' ab c', ' C'));
        window.ok(m.EndsWithIgnoreCase(' ab c', ' c'));
        window.ok(m.EndsWithIgnoreCase(' ', ' '));
        window.ok(m.EndsWithIgnoreCase('', ''));
        window.ok(!m.EndsWithIgnoreCase(null, ''));
        window.ok(!m.EndsWithIgnoreCase('', null));
        window.ok(!m.EndsWithIgnoreCase(null, null));
        
        window.ok(!m.Contains(' ab c', 'B '));
        window.ok(m.Contains(' ab c', 'b '));
        window.ok(m.Contains(' ', ' '));
        window.ok(m.Contains('', ''));
        window.ok(!m.Contains(null, ''));
        window.ok(!m.Contains('', null));
        window.ok(!m.Contains(null, null));

        window.ok(m.ContainsIgnoreCase(' ab c', 'B '));
        window.ok(m.ContainsIgnoreCase(' ab c', 'b '));
        window.ok(m.ContainsIgnoreCase(' ', ' '));
        window.ok(m.ContainsIgnoreCase('', ''));
        window.ok(!m.ContainsIgnoreCase(null, ''));
        window.ok(!m.ContainsIgnoreCase('', null));
        window.ok(!m.ContainsIgnoreCase(null, null));

        window.ok(m.IsNullOrWhiteSpace(' '));
        window.ok(m.IsNullOrWhiteSpace(null));
        window.ok(m.IsNullOrWhiteSpace(''));

        window.ok(m.IsDigitChain('0123456789'));
        window.ok(!m.IsDigitChain(null));
        window.ok(!m.IsDigitChain(''));

        window.ok(m.IsNumber('-0.3e-2'));
        window.ok(!m.IsNumber(null));
        window.ok(!m.IsNumber(''));

        window.ok(m.IsEmail('nickname@domain.com'));
        window.ok(!m.IsEmail(null));
        window.ok(!m.IsEmail(''));

        window.ok(m.IsUrl('http://www.github.com/'));
        window.ok(!m.IsUrl(null));
        window.ok(!m.IsUrl(''));

        window.ok(m.IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        window.ok(!m.IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        window.ok(!m.IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        window.ok(m.IsRegexMatch('', ''), true);
        window.ok(!m.IsRegexMatch(null, ''), false);
        window.ok(!m.IsRegexMatch('', null), false);
        window.ok(!m.IsRegexMatch(null, null), false);

        window.equal(m.Guid('a1111111-1111-1111-1111-111111111111'), m.Guid('A1111111-1111-1111-1111-111111111111'));
    });

}($, window, window.ea.___6BE7863DC1DB4AFAA61BB53FF97FE169));
