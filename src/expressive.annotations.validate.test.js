/// <reference path="../extern/qunit-1.18.0.js" />
/// <reference path="../extern/jquery-1.8.2.js" />
/// <reference path="../extern/jquery.validate-1.10.0.js" />
/// <reference path="../extern/jquery.validate.unobtrusive-3.1.1.js" />

/// <reference path="./expressive.annotations.validate.js" />

(function($, qunit, ea, eapriv) {
    // equal( actual, expected [, message ] )

    qunit.testDone(function() { // reset state for further tests
        ea.settings.apply({
            debug: false,
            dependencyTriggers: 'change keyup'
        });
    });

    qunit.module("type helper");

    qunit.test("verify_array_storage", function() {
        // debugger; // enable firebug (preferably, check 'on for all web pages' option) for the debugger to launch
        qunit.ok(eapriv.typeHelper.array.contains(["a"], "a"), "single element array contains its only item");
        qunit.ok(eapriv.typeHelper.array.contains(["a", "b"], "a"), "multiple elements array contains its first item");
        qunit.ok(eapriv.typeHelper.array.contains(["a", "b"], "b"), "multiple elements array contains its last item");
        qunit.ok(!eapriv.typeHelper.array.contains(["a", "b"], "c"), "multiple elements array does not contain unknown item");
    });

    qunit.test("verify_object_keys_extraction", function() {
        var assocArray = [];
        assocArray["one"] = "lorem";
        assocArray["two"] = "ipsum";
        assocArray["three"] = "dolor";
        var keys = eapriv.typeHelper.object.keys(assocArray);
        qunit.deepEqual(keys, ["one", "two", "three"], "keys of associative array properly extracted");

        var model = {
            one: "lorem",
            two: "ipsum",
            three: "dolor"
        };
        keys = eapriv.typeHelper.object.keys(model);
        qunit.deepEqual(keys, ["one", "two", "three"], "names of object properties properly extracted");
    });

    qunit.test("verify_string_formatting", function() {
        qunit.equal(eapriv.typeHelper.string.format("{0}", "a"), "a", "string.format({0}, 'a') succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{1}", "a", "b"), "ab", "string.format({0}{1}, 'a', 'b') succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{0}", "a", "b"), "aa", "string.format({0}{0}, 'a', 'b') succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{0}", "a"), "aa", "string.format({0}{0}, 'a') succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}", { a: true }), "{\n    \"a\": true\n}", "string.format({0}, object) succeed");
        qunit.equal(eapriv.typeHelper.string.format("a{0}b", "$'"), "a$'b", "string.format({0}, '$\'') succeed");

        qunit.equal(eapriv.typeHelper.string.format("{0}", ["a"]), "a", "string.format({0}, ['a']) succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{1}", ["a", "b"]), "ab", "string.format({0}{1}, ['a', 'b']) succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{0}", ["a", "b"]), "aa", "string.format({0}{0}, ['a', 'b']) succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}{0}", ["a"]), "aa", "string.format({0}{0}, ['a']) succeed");
        qunit.equal(eapriv.typeHelper.string.format("{0}", [{ a: true }]), "{\n    \"a\": true\n}", "string.format({0}, [object]) succeed");
        qunit.equal(eapriv.typeHelper.string.format("a{0}b", ["$'"]), "a$'b", "string.format({0}, ['$\'']) succeed");
    });

    qunit.test("verify_type_parsing", function() {
        var result = eapriv.typeHelper.tryParse(undefined, "string");
        qunit.ok(result.error, "broken string parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid string.", "broken string parsing error message thrown");

        result = eapriv.typeHelper.tryParse("asd", "bool");
        qunit.ok(result.error, "broken bool parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid boolean.", "broken bool parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "numeric");
        qunit.ok(result.error, "broken number parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid float.", "broken number parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "datetime");
        qunit.ok(result.error, "broken datetime parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "broken datetime parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "timespan");
        qunit.ok(result.error, "broken timespan parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "broken timespan parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "guid");
        qunit.ok(result.error, "broken guid parsing error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "broken guid parsing error message thrown");

        result = eapriv.typeHelper.tryParse("{", "object");
        qunit.ok(result.error, "broken json string to object parse error thrown");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        qunit.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)
    });

    qunit.test("verify_non_standard_date_format_parsing", function() {
        var expected = new Date("August, 11 2014").getTime();
        var actual = eapriv.typeHelper.tryParse("11/08/2014", "datetime", undefined);
        qunit.notEqual(actual, expected, "default date parse fails to understand non-standard dd/mm/yyyy format");

        ea.addValueParser("nonStandard", function(value) {
            var arr = value.split('/');
            var date = new Date(arr[2], arr[1] - 1, arr[0]);
            return date.getTime();
        });
        actual = eapriv.typeHelper.tryParse("11/08/2014", "datetime", "fieldname", "nonStandard");
        qunit.equal(actual, expected, "custom value parser properly handles non-standard dd/mm/yyyy format");
    });

    qunit.test("verify_non_standard_object_format_parsing", function() {        
        var result = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", undefined);
        qunit.ok(result.error, "default object parse fails to understand non-json format");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        qunit.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)

        ea.addValueParser("xml_first xml_second", function(value) {
            return $.parseXML(value);
        });
        expected = $.parseXML("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>");
        var actual1 = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", "fieldname", "xml_first");
        var actual2 = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", "fieldname", "xml_second");

        // deep and not deep equality fails because of some security issue, only brief equality comparison is done
        qunit.ok(!actual1.error && !actual2.error, "XML parse success in multiple parsers");
        qunit.equal(actual1.contentType, expected.contentType, "first custom value parser properly handles non-json format");
        qunit.equal(actual2.contentType, expected.contentType, "second custom value parser properly handles non-json format");
    });

    qunit.test("verify_string_parsing", function() {
        qunit.equal(eapriv.typeHelper.string.tryParse("abc"), "abc", "string to string parse succeed");
        qunit.equal(eapriv.typeHelper.string.tryParse(123), "123", "int to string parse succeed");
        qunit.equal(eapriv.typeHelper.string.tryParse(0.123), "0.123", "float to string parse succeed");
        qunit.equal(eapriv.typeHelper.string.tryParse(false), "false", "bool to string parse succeed");

        var result = eapriv.typeHelper.string.tryParse(undefined);
        qunit.ok(result.error, "undefined to string parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid string.", "undefined to string parse error message thrown");
    });

    qunit.test("verify_bool_parsing", function() {
        qunit.equal(eapriv.typeHelper.bool.tryParse(false), false, "false bool to bool parse succeed");
        qunit.equal(eapriv.typeHelper.bool.tryParse("false"), false, "'false' string to bool parse succeed");
        qunit.equal(eapriv.typeHelper.bool.tryParse("False"), false, "'False' string to bool parse succeed");
        qunit.equal(eapriv.typeHelper.bool.tryParse(true), true, "true bool to bool parse succeed");
        qunit.equal(eapriv.typeHelper.bool.tryParse("true"), true, "'true' string to bool parse succeed");
        qunit.equal(eapriv.typeHelper.bool.tryParse("True"), true, "'True' string to bool parse succeed");

        var result = eapriv.typeHelper.bool.tryParse("asd");
        qunit.ok(result.error, "random string to bool parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid boolean.", "random string to bool parse error message thrown");
    });

    qunit.test("verify_number_parsing", function() {
        // integer literals
        qunit.equal(eapriv.typeHelper.number.tryParse("-1"), -1, "negative integer string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse("0"), 0, "zero string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse("1"), 1, "positive integer string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(-1), -1, "negative integer number to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(0), 0, "zero integer number to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(1), 1, "positive integer number to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(0xFF), 255, "hexadecimal integer literal to float parse succeed");

        // floating-point literals
        qunit.equal(eapriv.typeHelper.number.tryParse("-1.1"), -1.1, "negative floating point string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse("1.1"), 1.1, "positive floating point string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(-1.1), -1.1, "negative floating point number to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(1.1), 1.1, "positive floating point number to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse("314e-2"), 3.14, "exponential notation string to float parse succeed");
        qunit.equal(eapriv.typeHelper.number.tryParse(314e-2), 3.14, "exponential notation number to float parse succeed");

        // non-numeric values
        var result = eapriv.typeHelper.number.tryParse(""); // empty string
        qunit.ok(result.error, "empty string to float parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid float.", "empty string to float parse error message thrown");

        qunit.ok(eapriv.typeHelper.number.tryParse(" ").error, "whitespace character to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("\t").error, "tab character to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("asd").error, "non-numeric character string to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("true").error, "boolean true to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("false").error, "boolean false to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("asd123").error, "number with preceding non-numeric characters to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse("123asd").error, "number with trailling non-numeric characters to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(undefined).error, "undefined value to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(null).error, "null value to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(NaN).error, "NaN value to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(Infinity).error, "Infinity primitive to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(+Infinity).error, "positive Infinity to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(-Infinity).error, "negative Infinity to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse(new Date(Date.now())).error, "date object to float parse error thrown");
        qunit.ok(eapriv.typeHelper.number.tryParse({}).error, "empty object to float parse error thrown");
    });

    qunit.test("verify_date_parsing", function() {
        var now = Date.now();
        qunit.equal(eapriv.typeHelper.date.tryParse(new Date(now)), now, "date object to date parse succeed");
        qunit.equal(eapriv.typeHelper.date.tryParse("Aug 9, 1995"), new Date("Aug 9, 1995").getTime(), "casual date string to date parse succeed");
        qunit.equal(eapriv.typeHelper.date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), 807926400000, "ISO date string to date parse succeed");
        qunit.equal(eapriv.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), 0, "Jan 1st, 1970 ISO date string to date parse succeed");
        qunit.equal(eapriv.typeHelper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), 14400000, "4h shift to Jan 1st, 1970 ISO date string to date parse succeed");

        qunit.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.date.tryParse(new Date())), "datetime parsing returns number (of milliseconds)");
        qunit.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.date.tryParse("Aug 9, 1995")), "datetime parsing returns number (of milliseconds)");

        var result = eapriv.typeHelper.date.tryParse("");
        qunit.ok(result.error, "empty string to date parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "empty string to date parse error message thrown");

        result = eapriv.typeHelper.date.tryParse(1997);
        qunit.ok(result.error, "integer to date parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "integer to date parse error message thrown");
    });

    qunit.test("verify_timespan_parsing", function() {
        qunit.equal(eapriv.typeHelper.timespan.tryParse("1.02:03:04.9999999"), 999 + 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000, "serialized .NET timespan string with days parse succeed");
        qunit.equal(eapriv.typeHelper.timespan.tryParse("01:02:03.9999999"), 999 + 3 * 1000 + 2 * 60 * 1000 + 1 * 60 * 60 * 1000, "serialized .NET timespan string without days parse succeed");
        qunit.equal(eapriv.typeHelper.timespan.tryParse("01:02:03"), 3 * 1000 + 2 * 60 * 1000 + 1 * 60 * 60 * 1000, "serialized .NET timespan string without days or milliseconds parse succeed");
        qunit.equal(eapriv.typeHelper.timespan.tryParse("1.02:03:04"), 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000, "serialized .NET timespan string without milliseconds parse succeed");
        qunit.equal(eapriv.typeHelper.timespan.tryParse("10675199.02:48:05.4775807"), 477 + 5 * 1000 + 48 * 60 * 1000 + 2 * 60 * 60 * 1000 + 10675199 * 24 * 60 * 60 * 1000, "serialized .NET timespan string max value parse succeed");
        qunit.equal(eapriv.typeHelper.timespan.tryParse("-10675199.02:48:05.4775808"), 0 - 477 - 5 * 1000 - 48 * 60 * 1000 - 2 * 60 * 60 * 1000 - 10675199 * 24 * 60 * 60 * 1000, "serialized .NET timespan string min value parse succeed");

        qunit.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.timespan.tryParse("1.02:03:04.9999999")), "serialized .NET timespan string parsing returns number (of milliseconds)");

        var result = eapriv.typeHelper.timespan.tryParse("");
        qunit.ok(result.error, "empty string to timespan parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "empty string to timespan parse error message thrown");

        result = eapriv.typeHelper.timespan.tryParse(1997);
        qunit.ok(result.error, "integer to timespan parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "integer to timespan parse error message thrown");
    });

    qunit.test("verify_guid_parsing", function() {
        qunit.equal(eapriv.typeHelper.guid.tryParse("a1111111-1111-1111-1111-111111111111"), "A1111111-1111-1111-1111-111111111111", "guid string parse succeed, case insensitivity confirmed");

        var result = eapriv.typeHelper.guid.tryParse("");
        qunit.ok(result.error, "empty string to guid parse error thrown");
        qunit.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "empty string to guid parse error message thrown");
    });

    qunit.test("verify_object_parsing", function() {
        var jsonString = '{"Val":"a","Arr":[1,2]}';
        qunit.deepEqual(eapriv.typeHelper.object.tryParse(jsonString), $.parseJSON(jsonString));

        var result = eapriv.typeHelper.object.tryParse("{");
        qunit.ok(result.error, "broken json string to object parse error thrown");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        qunit.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)
    });

    qunit.test("verify_numeric_type_recognition", function() {
        qunit.ok(eapriv.typeHelper.isNumeric(-1), "negative integer recognized as number");
        qunit.ok(eapriv.typeHelper.isNumeric(1), "positive integer recognized as number");
        qunit.ok(eapriv.typeHelper.isNumeric(0.123), "float recognized as number");

        qunit.ok(!eapriv.typeHelper.isNumeric(NaN), "NaN not recognized as number");
        qunit.ok(!eapriv.typeHelper.isNumeric("1"), "integer string not recognized as number");
    });

    qunit.test("verify_date_type_recognition", function() {
        qunit.ok(eapriv.typeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")), "date object recognized as date");

        qunit.ok(!eapriv.typeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"), "date string not recognized as date");
        qunit.ok(!eapriv.typeHelper.isDate(807926400000), "float (ticks number) not recognized as date");
    });

    qunit.test("verify_timespan_type_recognition", function() {
        qunit.ok(eapriv.typeHelper.isTimeSpan("1.02:03:04.9999999"), "serialized .NET timespan string with days recognized");
        qunit.ok(eapriv.typeHelper.isTimeSpan("01:02:03.9999999"), "serialized .NET timespan string without days recognized");
        qunit.ok(eapriv.typeHelper.isTimeSpan("01:02:03"), "serialized .NET timespan string without days or milliseconds recognized");
        qunit.ok(eapriv.typeHelper.isTimeSpan("1.02:03:04"), "serialized .NET timespan string without milliseconds recognized");
        qunit.ok(eapriv.typeHelper.isTimeSpan("10675199.02:48:05.4775807"), "serialized .NET timespan string max value recognized");
        qunit.ok(eapriv.typeHelper.isTimeSpan("-10675199.02:48:05.4775808"), "serialized .NET timespan string min value recognized");

        qunit.ok(!eapriv.typeHelper.isTimeSpan(""), "incorrect timespan string detected");
    });

    qunit.test("verify_string_type_recognition", function() {
        qunit.ok(eapriv.typeHelper.isString(""), "empty string recognized as string");
        qunit.ok(eapriv.typeHelper.isString("123"), "random string recognized as string");

        qunit.ok(!eapriv.typeHelper.isString(123), "integer not recognized as string");
        qunit.ok(!eapriv.typeHelper.isString({}), "empty object not recognized as string");
        qunit.ok(!eapriv.typeHelper.isString(null), "null not recognized as string");
        qunit.ok(!eapriv.typeHelper.isString(undefined), "undefined not recognized as string");
    });

    qunit.test("verify_bool_type_recognition", function() {
        qunit.ok(eapriv.typeHelper.isBool(true), "true bool recognized as bool");
        qunit.ok(eapriv.typeHelper.isBool(false), "false bool recognized as bool");

        qunit.ok(!eapriv.typeHelper.isBool("true"), "'true' string not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool("True"), "'True' string not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool("false"), "'false' string not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool("False"), "'False' string not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool(""), "empty string not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool(0), "0 integer not recognized as bool");        
        qunit.ok(!eapriv.typeHelper.isBool(1), "positive integer not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool(-1), "negative integer not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool({}), "empty object not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool(null), "null not recognized as bool");
        qunit.ok(!eapriv.typeHelper.isBool(undefined), "undefined not recognized as bool");
    });

    qunit.test("verify_guid_string_recognition", function() {
        qunit.ok(eapriv.typeHelper.isGuid("541422A6-FEAD-4CD9-8466-5419AA63DBE1"), "uppercased guid string recognized as guid");
        qunit.ok(eapriv.typeHelper.isGuid("541422a6-fead-4cd9-8466-5419aa63dbe1"), "lowercased guid string recognized as guid");

        qunit.ok(!eapriv.typeHelper.isGuid(1), "integer not recognized as guid");
        qunit.ok(!eapriv.typeHelper.isGuid(""), "empty string not recognized as guid");
    });

    qunit.module("model helper");

    qunit.test("verify_expression_evaluation", function() {
        var model = {
            Number: 123,
            Stability: {
                High: 0
            },
            Items: {
                7: {
                    Items: {
                        6: {
                            Val: "abc"
                        }
                    }
                }
            }
        }
        var deserizedModel = eapriv.modelHelper.deserializeObject(null, null, { "Number": 123, "Stability.High": 0, "Items[7].Items[6].Val": "abc" }, null);
        qunit.deepEqual(deserizedModel, model, 'model deserialized properly based on given consts map');

        var expression = "Number - 23 == 100 && Stability.High == 0 && Items[7].Items[6].Val == 'abc'";
        var result = eapriv.modelHelper.ctxEval(expression, model);
        qunit.ok(result, "expression evaluated correctly within given model context");

        with (model) {
            qunit.ok(result === eval(expression), "ctxEval gives the same result as native eval");
        }
    });

    qunit.module("toolchain");

    qunit.test("verify_methods_overloading", function() {
        var m = eapriv.toolchain.methods;

        eapriv.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        eapriv.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });
        eapriv.toolchain.addMethod('Whoami', function(i, s) {
            return 'method A' + i + ' - ' + s;
        });

        qunit.equal(m.Whoami(), 'method A');
        qunit.equal(m.Whoami(1), 'method A1');
        qunit.equal(m.Whoami(2, 'final'), 'method A2 - final');
    });

    qunit.test("verify_methods_overriding", function() {
        var m = eapriv.toolchain.methods;

        eapriv.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        eapriv.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });

        qunit.equal(m.Whoami(), 'method A');
        qunit.equal(m.Whoami(1), 'method A1');

        // redefine methods
        eapriv.toolchain.addMethod('Whoami', function() {
            return 'method B';
        });
        eapriv.toolchain.addMethod('Whoami', function(i) {
            return 'method B' + i;
        });

        qunit.equal(m.Whoami(), 'method B');
        qunit.equal(m.Whoami(1), 'method B1');
    });

    qunit.test("verify_toolchain_methods_logic", function() {

        var o = {};
        eapriv.toolchain.registerMethods(o);
        var m = eapriv.toolchain.methods;

        qunit.ok(eapriv.typeHelper.isNumeric(m.Now()), "Now() returns number (of milliseconds)");
        qunit.ok(eapriv.typeHelper.isNumeric(m.Today()), "Today() returns number (of milliseconds)");
        qunit.ok(eapriv.typeHelper.isNumeric(m.Date(1985, 2, 20)), "Date(y, M, d) returns number (of milliseconds)");
        qunit.ok(eapriv.typeHelper.isNumeric(m.Date(1985, 2, 20, 0, 0, 1)), "Date(y, M, d, h, m, s) returns number (of milliseconds)");

        qunit.ok(m.Now() > m.Today());
        qunit.ok(m.Date(1985, 2, 20) < m.Date(1985, 2, 20, 0, 0, 1));

        qunit.ok(m.TimeSpan(1, 0, 0, 0) > m.TimeSpan(0, 1, 0, 0));
        qunit.equal(m.TimeSpan(0, 0, 0, 0), 0);
        qunit.equal(m.TimeSpan(1, 2, 3, 4), 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000);

        qunit.equal(m.Length('0123'), 4);
        qunit.equal(m.Length('    '), 4);
        qunit.equal(m.Length(null), 0);
        qunit.equal(m.Length(''), 0);

        qunit.equal(m.Trim(' a b c '), 'a b c');
        qunit.equal(m.Trim(null), null);
        qunit.equal(m.Trim(''), '');

        qunit.equal(m.Concat(' a ', ' b '), ' a  b ');
        qunit.equal(m.Concat(null, null), '');
        qunit.equal(m.Concat('', ''), '');

        qunit.equal(m.Concat(' a ', ' b ', ' c '), ' a  b  c ');
        qunit.equal(m.Concat(null, null, null), '');
        qunit.equal(m.Concat('', '', ''), '');

        qunit.equal(m.CompareOrdinal(' abc ', ' ABC '), 1);
        qunit.equal(m.CompareOrdinal('a', 'a'), 0);
        qunit.equal(m.CompareOrdinal('a', 'A'), 1);
        qunit.equal(m.CompareOrdinal('A', 'a'), -1);
        qunit.equal(m.CompareOrdinal('a', 'b'), -1);
        qunit.equal(m.CompareOrdinal('b', 'a'), 1);
        qunit.equal(m.CompareOrdinal(null, 'a'), -1);
        qunit.equal(m.CompareOrdinal('a', null), 1);
        qunit.equal(m.CompareOrdinal(' ', 'a'), -1);
        qunit.equal(m.CompareOrdinal('a', ' '), 1);
        qunit.equal(m.CompareOrdinal(null, ''), -1);
        qunit.equal(m.CompareOrdinal('', null), 1);
        qunit.equal(m.CompareOrdinal(null, null), 0);
        qunit.equal(m.CompareOrdinal('', ''), 0);

        qunit.equal(m.CompareOrdinalIgnoreCase(' abc ', ' ABC '), 0);
        qunit.equal(m.CompareOrdinalIgnoreCase('a', 'a'), 0);
        qunit.equal(m.CompareOrdinalIgnoreCase('a', 'A'), 0);
        qunit.equal(m.CompareOrdinalIgnoreCase('A', 'a'), 0);
        qunit.equal(m.CompareOrdinalIgnoreCase('a', 'b'), -1);
        qunit.equal(m.CompareOrdinalIgnoreCase('b', 'a'), 1);
        qunit.equal(m.CompareOrdinalIgnoreCase(null, 'a'), -1);
        qunit.equal(m.CompareOrdinalIgnoreCase('a', null), 1);
        qunit.equal(m.CompareOrdinalIgnoreCase(' ', 'a'), -1);
        qunit.equal(m.CompareOrdinalIgnoreCase('a', ' '), 1);
        qunit.equal(m.CompareOrdinalIgnoreCase(null, ''), -1);
        qunit.equal(m.CompareOrdinalIgnoreCase('', null), 1);
        qunit.equal(m.CompareOrdinalIgnoreCase(null, null), 0);
        qunit.equal(m.CompareOrdinalIgnoreCase('', ''), 0);
        
        qunit.ok(!m.StartsWith(' ab c', ' A'));
        qunit.ok(m.StartsWith(' ab c', ' a'));
        qunit.ok(m.StartsWith(' ', ' '));
        qunit.ok(m.StartsWith('', ''));
        qunit.ok(!m.StartsWith(null, ''));
        qunit.ok(!m.StartsWith('', null));
        qunit.ok(!m.StartsWith(null, null));

        qunit.ok(m.StartsWithIgnoreCase(' ab c', ' A'));
        qunit.ok(m.StartsWithIgnoreCase(' ab c', ' a'));
        qunit.ok(m.StartsWithIgnoreCase(' ', ' '));
        qunit.ok(m.StartsWithIgnoreCase('', ''));
        qunit.ok(!m.StartsWithIgnoreCase(null, ''));
        qunit.ok(!m.StartsWithIgnoreCase('', null));
        qunit.ok(!m.StartsWithIgnoreCase(null, null));
        
        qunit.ok(!m.EndsWith(' ab c', ' C'));
        qunit.ok(m.EndsWith(' ab c', ' c'));
        qunit.ok(m.EndsWith(' ', ' '));
        qunit.ok(m.EndsWith('', ''));
        qunit.ok(!m.EndsWith(null, ''));
        qunit.ok(!m.EndsWith('', null));
        qunit.ok(!m.EndsWith(null, null));

        qunit.ok(m.EndsWithIgnoreCase(' ab c', ' C'));
        qunit.ok(m.EndsWithIgnoreCase(' ab c', ' c'));
        qunit.ok(m.EndsWithIgnoreCase(' ', ' '));
        qunit.ok(m.EndsWithIgnoreCase('', ''));
        qunit.ok(!m.EndsWithIgnoreCase(null, ''));
        qunit.ok(!m.EndsWithIgnoreCase('', null));
        qunit.ok(!m.EndsWithIgnoreCase(null, null));
        
        qunit.ok(!m.Contains(' ab c', 'B '));
        qunit.ok(m.Contains(' ab c', 'b '));
        qunit.ok(m.Contains(' ', ' '));
        qunit.ok(m.Contains('', ''));
        qunit.ok(!m.Contains(null, ''));
        qunit.ok(!m.Contains('', null));
        qunit.ok(!m.Contains(null, null));

        qunit.ok(m.ContainsIgnoreCase(' ab c', 'B '));
        qunit.ok(m.ContainsIgnoreCase(' ab c', 'b '));
        qunit.ok(m.ContainsIgnoreCase(' ', ' '));
        qunit.ok(m.ContainsIgnoreCase('', ''));
        qunit.ok(!m.ContainsIgnoreCase(null, ''));
        qunit.ok(!m.ContainsIgnoreCase('', null));
        qunit.ok(!m.ContainsIgnoreCase(null, null));

        qunit.ok(m.IsNullOrWhiteSpace(' '));
        qunit.ok(m.IsNullOrWhiteSpace(null));
        qunit.ok(m.IsNullOrWhiteSpace(''));

        qunit.ok(m.IsDigitChain('0123456789'));
        qunit.ok(!m.IsDigitChain('+0'));
        qunit.ok(!m.IsDigitChain('-0'));
        qunit.ok(!m.IsDigitChain(null));
        qunit.ok(!m.IsDigitChain(''));

        qunit.ok(m.IsNumber('0'));
        qunit.ok(m.IsNumber('0.0'));
        qunit.ok(m.IsNumber('10.10'));
        qunit.ok(m.IsNumber('0e0'));
        qunit.ok(m.IsNumber('.2'));
        qunit.ok(m.IsNumber('3.14'));
        qunit.ok(m.IsNumber('5e6'));
        qunit.ok(m.IsNumber('5e-6'));
        qunit.ok(m.IsNumber('5e+6'));
        qunit.ok(m.IsNumber('9.0E-10'));
        qunit.ok(m.IsNumber('.11e10'));
        qunit.ok(m.IsNumber('-0.3e-2'));
        qunit.ok(m.IsNumber('+0.3e-2'));
        qunit.ok(m.IsNumber('+0'));
        qunit.ok(m.IsNumber('-0'));
        qunit.ok(!m.IsNumber('++0'));
        qunit.ok(!m.IsNumber('--0'));
        qunit.ok(!m.IsNumber('+-0'));
        qunit.ok(!m.IsNumber(null));
        qunit.ok(!m.IsNumber(''));

        qunit.ok(m.IsEmail('nickname@domain.com'));
        qunit.ok(!m.IsEmail(null));
        qunit.ok(!m.IsEmail(''));

        qunit.ok(m.IsUrl('http://www.github.com/'));
        qunit.ok(!m.IsUrl(null));
        qunit.ok(!m.IsUrl(''));

        qunit.ok(m.IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        qunit.ok(!m.IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        qunit.ok(!m.IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        qunit.ok(m.IsRegexMatch('', ''));
        qunit.ok(!m.IsRegexMatch(null, ''));
        qunit.ok(!m.IsRegexMatch('', null));
        qunit.ok(!m.IsRegexMatch(null, null));

        qunit.equal(m.Guid('a1111111-1111-1111-1111-111111111111'), m.Guid('A1111111-1111-1111-1111-111111111111'));
    });

    qunit.test("verify_allowed_settings_setup", function() {
        ea.settings.apply({ debug: true });
        qunit.equal(ea.settings.debug, true);

        ea.settings.apply({ debug: false });
        qunit.equal(ea.settings.debug, false);

        ea.settings.apply({ dependencyTriggers: 'change paste keyup' });
        qunit.equal(ea.settings.dependencyTriggers, 'change paste keyup');

        ea.settings.apply({ dependencyTriggers: undefined });
        qunit.equal(ea.settings.dependencyTriggers, undefined);

        ea.settings.apply({ dependencyTriggers: null });
        qunit.equal(ea.settings.dependencyTriggers, null);

        ea.settings.apply({ dependencyTriggers: '' });
        qunit.equal(ea.settings.dependencyTriggers, '');
    });

    qunit.test("detect_invalid_debug_setup", function() {
        try {
            ea.settings.apply({ debug: 1 });
            qunit.fail();
        } catch (ex) {
            qunit.equal(ex, 'debug value must be a boolean (true or false)');
        }
    });

    qunit.test("detect_invalid_triggers_setup", function() {
        try {
            ea.settings.apply({ dependencyTriggers: false });
            qunit.fail();
        } catch (ex) {
            qunit.equal(ex, 'dependencyTriggers value must be a string (multiple event types can be bound at once by including each one separated by a space), null or undefined');
        }
    });

    qunit.module("full computation flow");

    qunit.test("verify_assertthat_not_computed_for_null_value", function() {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'false', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeAssertThat(null, elementMock, paramsMock);
        qunit.ok(result); // ok - satisfied despite false assertion (not invoked due to null value)

        // the same behavior for empty or undefined
        result = eapriv.computeAssertThat('', elementMock, paramsMock);
        qunit.ok(result);
        result = eapriv.computeAssertThat(undefined, elementMock, paramsMock);
        qunit.ok(result);
    });

    qunit.test("verify_requiredif_not_computed_for_non_null_value", function() {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'true', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeRequiredIf({}, elementMock, paramsMock);
        qunit.ok(result); // ok - not required despite requirement obligation (not invoked due to non-null value)

        // the same behavior for whitespace string if allowed
        paramsMock.allowEmpty = true;
        result = eapriv.computeRequiredIf(' ', elementMock, paramsMock);
        qunit.ok(result);
    });

    qunit.test("verify_assertthat_computed_for_non_null_value", function() {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'false', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeAssertThat({}, elementMock, paramsMock);
        qunit.ok(!result); // not ok - not satisfied because of false assertion
    });

    qunit.test("verify_requiredif_computed_for_null_value", function() {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'true', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeRequiredIf(null, elementMock, paramsMock);
        qunit.ok(!result); // not ok - required because of requirement obligation

        // the same behavior for empty or undefined
        result = eapriv.computeRequiredIf('', elementMock, paramsMock);
        qunit.ok(!result);
        result = eapriv.computeRequiredIf(undefined, elementMock, paramsMock);
        qunit.ok(!result);

        // the same behavior for whitespace string if not allowed
        paramsMock.allowEmpty = false;
        result = eapriv.computeRequiredIf(' ', elementMock, paramsMock);
        qunit.ok(!result);
    });

    qunit.test("verify_assertthat_complex_expression_computation", function() {

        ea.addMethod('Whoami', function() {
            return 'root';
        });

        var form =
            "<form>" +
                "<input id='f1' name='ContactDetails.Email' value='asd'>" +
            "</form>";

        var elementMock = { name: 'name' };
        var paramsMock = {
            form: $(form),
            element: $(form).find('#f1'),
            prefix: 'ContactDetails.',
            expression: "Whoami() == 'root' && IsEmail(Email)",
            fieldsMap: { Email: 'string' },
            constsMap: {},
            parsersMap: {}
        }
        var result = eapriv.computeAssertThat({}, elementMock, paramsMock);
        qunit.ok(!result);
    });

    qunit.test("verify_requiredif_complex_expression_computation", function() {

        ea.addMethod('Whoami', function() {
            return 'root';
        });

        var form =
            "<form>" +
                "<input id='f1' name='ContactDetails.Email' value='asd'>" +
            "</form>";

        var elementMock = { name: 'name' };
        var paramsMock = {
            form: $(form),
            element: $(form).find('#f1'),
            prefix: 'ContactDetails.',
            expression: "Whoami() == 'root' && !IsEmail(Email)",
            fieldsMap: { Email: 'string' },
            constsMap: {},
            parsersMap: {}
        }
        var result = eapriv.computeRequiredIf(null, elementMock, paramsMock);
        qunit.ok(!result);
    });

    qunit.test("verify_sample_form_validation", function() { // relies on form defined in test harness file (test.html)

        ea.addMethod('Whoami', function() {
            return 'root';
        });
        ea.addMethod('ArrayContains', function(value, array) {
            return $.inArray(value, array) >= 0;
        });
        ea.addValueParser('ArrayParser', function(value, field) {
            qunit.equal(field, 'ContactDetails.Letters');

            var array = value.split(',');
            return array.length === 0 ? null : array;
        });

        var validator = $('#basic_test_form').validate();
        validator.settings.ignore = ''; // enable validation for hidden fields (our entire test form is hidden) http://stackoverflow.com/q/8466643/270315

        var element = $('#basic_test_form').find('[name="ContactDetails.Email"]');
        var result = element.valid(); // trigger wait for result (all is synchronous)
        qunit.ok(!result);
        qunit.equal(validator.errorList[0].message, 'Provided email: ea@home.com (yes ea@home.com) cannot be accepted.');

        element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStability"]');
        result = element.valid();
        qunit.ok(!result);
        qunit.equal(validator.errorList[0].message, '{0}{1}');
    });

}($, QUnit, window.ea, window.ea.___6BE7863DC1DB4AFAA61BB53FF97FE169));
