/// <reference path="../extern/qunit-1.18.0.js" />
/// <reference path="../extern/jquery-1.8.2.js" />
/// <reference path="../extern/jquery.validate-1.10.0.js" />
/// <reference path="../extern/jquery.validate.unobtrusive-3.1.1.js" />

/// <reference path="./expressive.annotations.validate.js" />

(function($, qunit, ea, eapriv) {
    // equal( actual, expected [, message ] )

    qunit.begin(function() { // fires once before all tests
        window.console = (function(wndconsole) { // mock console for tests
            var message = null;
            var suspend = false;

            var log = function(msg) {
                message = msg;
                if (!suspend) {
                    wndconsole.log(msg); // call original console
                }
            }
            var warn = function(msg) {
                message = msg;
                if (!suspend) {
                    wndconsole.warn(msg);
                }
            }
            var error = function(msg) {
                message = msg;
                if (!suspend) {
                    wndconsole.error(msg);
                }
            }
            var read = function() {
                return message;
            }
            var clear = function() {
                message = null; // clear buffer
            }
            var suppress = function() {
                suspend = true; // prevent console logging (e.g. not to pollute the test-console output)
            }
            var restore = function() {
                suspend = false; // restore console logging
            }

            return { log: log, warn: warn, error: error, read: read, clear: clear, suppress: suppress, restore: restore }
        })(window.console);
    });

    qunit.testDone(function() { // reset state for subsequent test
        ea.settings.apply({
            debug: false,
            optimize: true,
            enumsAsNumbers: true,
            dependencyTriggers: 'change keyup'
        });
    });

    qunit.module("type_helper");

    qunit.test("verify_array_storage", function(assert) {
        // debugger; // enable firebug (preferably, check 'on for all web pages' option) for the debugger to launch
        assert.ok(eapriv.typeHelper.array.contains(["a"], "a"), "single element array contains its only item");
        assert.ok(eapriv.typeHelper.array.contains(["a", "b"], "a"), "multiple elements array contains its first item");
        assert.ok(eapriv.typeHelper.array.contains(["a", "b"], "b"), "multiple elements array contains its last item");
        assert.ok(!eapriv.typeHelper.array.contains(["a", "b"], "c"), "multiple elements array does not contain unknown item");
    });

    qunit.test("verify_object_keys_extraction", function(assert) {
        var assocArray = [];
        assocArray["one"] = "lorem";
        assocArray["two"] = "ipsum";
        assocArray["three"] = "dolor";
        var keys = eapriv.typeHelper.object.keys(assocArray);
        assert.deepEqual(keys, ["one", "two", "three"], "keys of associative array properly extracted");

        var model = {
            one: "lorem",
            two: "ipsum",
            three: "dolor"
        };
        keys = eapriv.typeHelper.object.keys(model);
        assert.deepEqual(keys, ["one", "two", "three"], "names of object properties properly extracted");
    });

    qunit.test("verify_string_formatting", function(assert) {
        assert.equal(eapriv.typeHelper.string.format("{0}", "a"), "a", "string.format({0}, 'a') succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}", "a", "b"), "a", "string.format({0}, 'a', 'b') succeed");
        assert.equal(eapriv.typeHelper.string.format("{1}", "a", "b"), "b", "string.format({1}, 'a', 'b') succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{1}", "a", "b"), "ab", "string.format({0}{1}, 'a', 'b') succeed");
        assert.equal(eapriv.typeHelper.string.format("{1}{0}", "a", "b"), "ba", "string.format({1}{0}, 'a', 'b') succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{0}", "a", "b"), "aa", "string.format({0}{0}, 'a', 'b') succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{0}", "a"), "aa", "string.format({0}{0}, 'a') succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}", { a: true }), "{\n    \"a\": true\n}", "string.format({0}, object) succeed");
        assert.equal(eapriv.typeHelper.string.format("a{0}b", "$'"), "a$'b", "string.format(a{0}b, '$\'') succeed");

        assert.equal(eapriv.typeHelper.string.format("{0}", ["a"]), "a", "string.format({0}, ['a']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}", ["a", "b"]), "a", "string.format({0}, ['a', 'b']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{1}", ["a", "b"]), "b", "string.format({1}, ['a', 'b']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{1}", ["a", "b"]), "ab", "string.format({0}{1}, ['a', 'b']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{1}{0}", ["a", "b"]), "ba", "string.format({1}{0}, ['a', 'b']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{0}", ["a", "b"]), "aa", "string.format({0}{0}, ['a', 'b']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}{0}", ["a"]), "aa", "string.format({0}{0}, ['a']) succeed");
        assert.equal(eapriv.typeHelper.string.format("{0}", [{ a: true }]), "{\n    \"a\": true\n}", "string.format({0}, [object]) succeed");
        assert.equal(eapriv.typeHelper.string.format("a{0}b", ["$'"]), "a$'b", "string.format(a{0}b, ['$\'']) succeed");
    });

    qunit.test("verify_string_indentation", function(assert) {
        var result = eapriv.typeHelper.string.indent("1st line\n2nd line\n3rd line");
        assert.equal(result, "1st line\n2nd line\n3rd line");
        result = eapriv.typeHelper.string.indent("1st line\n2nd line\n3rd line", 0);
        assert.equal(result, "1st line\n2nd line\n3rd line");
        result = eapriv.typeHelper.string.indent("1st line\n2nd line\n3rd line", 1);
        assert.equal(result, " 1st line\n 2nd line\n 3rd line");
        result = eapriv.typeHelper.string.indent("1st line\n2nd line\n3rd line", 5);
        assert.equal(result, "     1st line\n     2nd line\n     3rd line");
    });

    qunit.test("verify_datetime_stamp", function(assert) {
        var result = eapriv.typeHelper.datetime.stamp(new Date(2017, 07, 29, 0, 0, 0));
        assert.equal(result, "00:00:00");
        result = eapriv.typeHelper.datetime.stamp(new Date(2017, 07, 29, 1, 11, 1));
        assert.equal(result, "01:11:01");
        result = eapriv.typeHelper.datetime.stamp(new Date(2017, 07, 29, 11, 11, 11));
        assert.equal(result, "11:11:11");
    });

    qunit.test("verify_type_parsing", function(assert) {
        var result = eapriv.typeHelper.tryParse(undefined, "string");
        assert.ok(result.error, "broken string parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid string.", "broken string parsing error message thrown");

        result = eapriv.typeHelper.tryParse("asd", "bool");
        assert.ok(result.error, "broken bool parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid boolean.", "broken bool parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "number");
        assert.ok(result.error, "broken number parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid number.", "broken number parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "datetime");
        assert.ok(result.error, "broken datetime parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "broken datetime parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "timespan");
        assert.ok(result.error, "broken timespan parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "broken timespan parsing error message thrown");

        result = eapriv.typeHelper.tryParse("", "guid");
        assert.ok(result.error, "broken guid parsing error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "broken guid parsing error message thrown");

        result = eapriv.typeHelper.tryParse("{", "object");
        assert.ok(result.error, "broken json string to object parse error thrown");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        assert.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)
    });

    qunit.test("verify_non_standard_date_format_parsing", function(assert) {
        var expected = new Date("August, 11 2014").getTime();
        var actual = eapriv.typeHelper.tryParse("11/08/2014", "datetime", undefined);
        assert.notEqual(actual, expected, "default date parse fails to understand non-standard dd/mm/yyyy format");

        ea.addValueParser("nonStandard", function(value) {
            var arr = value.split('/');
            var date = new Date(arr[2], arr[1] - 1, arr[0]);
            return date.getTime();
        });
        actual = eapriv.typeHelper.tryParse("11/08/2014", "datetime", "fieldname", "nonStandard");
        assert.equal(actual, expected, "custom value parser properly handles non-standard dd/mm/yyyy format");
    });

    qunit.test("verify_non_standard_object_format_parsing", function(assert) {
        var result = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", undefined);
        assert.ok(result.error, "default object parse fails to understand non-json format");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        assert.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)

        ea.addValueParser("xml_first xml_second", function(value) {
            return $.parseXML(value);
        });
        expected = $.parseXML("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>");
        var actual1 = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", "fieldname", "xml_first");
        var actual2 = eapriv.typeHelper.tryParse("<rss version='2.0'><channel><title>RSS Title</title></channel></rss>", "object", "fieldname", "xml_second");

        // deep and not deep equality fails because of some security issue, only brief equality comparison is done
        assert.ok(!actual1.error && !actual2.error, "XML parse success in multiple parsers");
        assert.equal(actual1.contentType, expected.contentType, "first custom value parser properly handles non-json format");
        assert.equal(actual2.contentType, expected.contentType, "second custom value parser properly handles non-json format");
    });

    qunit.test("verify_string_parsing", function(assert) {
        assert.equal(eapriv.typeHelper.string.tryParse("abc"), "abc", "string to string parse succeed");
        assert.equal(eapriv.typeHelper.string.tryParse(123), "123", "int to string parse succeed");
        assert.equal(eapriv.typeHelper.string.tryParse(0.123), "0.123", "float to string parse succeed");
        assert.equal(eapriv.typeHelper.string.tryParse(false), "false", "bool to string parse succeed");

        var result = eapriv.typeHelper.string.tryParse(undefined);
        assert.ok(result.error, "undefined to string parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid string.", "undefined to string parse error message thrown");
    });

    qunit.test("verify_bool_parsing", function(assert) {
        assert.equal(eapriv.typeHelper.bool.tryParse(false), false, "false bool to bool parse succeed");
        assert.equal(eapriv.typeHelper.bool.tryParse("false"), false, "'false' string to bool parse succeed");
        assert.equal(eapriv.typeHelper.bool.tryParse("False"), false, "'False' string to bool parse succeed");
        assert.equal(eapriv.typeHelper.bool.tryParse(true), true, "true bool to bool parse succeed");
        assert.equal(eapriv.typeHelper.bool.tryParse("true"), true, "'true' string to bool parse succeed");
        assert.equal(eapriv.typeHelper.bool.tryParse("True"), true, "'True' string to bool parse succeed");

        var result = eapriv.typeHelper.bool.tryParse("asd");
        assert.ok(result.error, "random string to bool parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid boolean.", "random string to bool parse error message thrown");
    });

    qunit.test("verify_number_parsing", function(assert) {
        // integer literals
        assert.equal(eapriv.typeHelper.number.tryParse("-1"), -1, "negative integer string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse("0"), 0, "zero string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse("1"), 1, "positive integer string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(-1), -1, "negative integer number to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(0), 0, "zero integer number to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(1), 1, "positive integer number to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(0xFF), 255, "hexadecimal integer literal to float parse succeed");

        // floating-point literals
        assert.equal(eapriv.typeHelper.number.tryParse("-1.1"), -1.1, "negative floating point string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse("1.1"), 1.1, "positive floating point string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(-1.1), -1.1, "negative floating point number to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(1.1), 1.1, "positive floating point number to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse("314e-2"), 3.14, "exponential notation string to float parse succeed");
        assert.equal(eapriv.typeHelper.number.tryParse(314e-2), 3.14, "exponential notation number to float parse succeed");

        // non-numeric values
        var result = eapriv.typeHelper.number.tryParse(""); // empty string
        assert.ok(result.error, "empty string to float parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid number.", "empty string to float parse error message thrown");

        assert.ok(eapriv.typeHelper.number.tryParse(" ").error, "whitespace character to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("\t").error, "tab character to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("asd").error, "non-numeric character string to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("true").error, "boolean true to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("false").error, "boolean false to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("asd123").error, "number with preceding non-numeric characters to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse("123asd").error, "number with trailling non-numeric characters to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(undefined).error, "undefined value to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(null).error, "null value to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(NaN).error, "NaN value to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(Infinity).error, "Infinity primitive to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(+Infinity).error, "positive Infinity to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(-Infinity).error, "negative Infinity to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse(new Date(Date.now())).error, "date object to float parse error thrown");
        assert.ok(eapriv.typeHelper.number.tryParse({}).error, "empty object to float parse error thrown");
    });

    qunit.test("verify_date_parsing", function(assert) {
        var now = Date.now();
        assert.equal(eapriv.typeHelper.datetime.tryParse(new Date(now)), now, "date object to date parse succeed");
        assert.equal(eapriv.typeHelper.datetime.tryParse("Aug 9, 1995"), new Date("Aug 9, 1995").getTime(), "casual date string to date parse succeed");
        assert.equal(eapriv.typeHelper.datetime.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), 807926400000, "ISO date string to date parse succeed");
        assert.equal(eapriv.typeHelper.datetime.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), 0, "Jan 1st, 1970 ISO date string to date parse succeed");
        assert.equal(eapriv.typeHelper.datetime.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), 14400000, "4h shift to Jan 1st, 1970 ISO date string to date parse succeed");

        assert.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.datetime.tryParse(new Date())), "datetime parsing returns number (of milliseconds)");
        assert.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.datetime.tryParse("Aug 9, 1995")), "datetime parsing returns number (of milliseconds)");

        var result = eapriv.typeHelper.datetime.tryParse("");
        assert.ok(result.error, "empty string to date parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "empty string to date parse error message thrown");

        result = eapriv.typeHelper.datetime.tryParse(1997);
        assert.ok(result.error, "integer to date parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid RFC 2822 or ISO 8601 date.", "integer to date parse error message thrown");
    });

    qunit.test("verify_timespan_parsing", function(assert) {
        assert.equal(eapriv.typeHelper.timespan.tryParse("1.02:03:04.9999999"), 999 + 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000, "serialized .NET timespan string with days parse succeed");
        assert.equal(eapriv.typeHelper.timespan.tryParse("01:02:03.9999999"), 999 + 3 * 1000 + 2 * 60 * 1000 + 1 * 60 * 60 * 1000, "serialized .NET timespan string without days parse succeed");
        assert.equal(eapriv.typeHelper.timespan.tryParse("01:02:03"), 3 * 1000 + 2 * 60 * 1000 + 1 * 60 * 60 * 1000, "serialized .NET timespan string without days or milliseconds parse succeed");
        assert.equal(eapriv.typeHelper.timespan.tryParse("1.02:03:04"), 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000, "serialized .NET timespan string without milliseconds parse succeed");
        assert.equal(eapriv.typeHelper.timespan.tryParse("10675199.02:48:05.4775807"), 477 + 5 * 1000 + 48 * 60 * 1000 + 2 * 60 * 60 * 1000 + 10675199 * 24 * 60 * 60 * 1000, "serialized .NET timespan string max value parse succeed");
        assert.equal(eapriv.typeHelper.timespan.tryParse("-10675199.02:48:05.4775808"), 0 - 477 - 5 * 1000 - 48 * 60 * 1000 - 2 * 60 * 60 * 1000 - 10675199 * 24 * 60 * 60 * 1000, "serialized .NET timespan string min value parse succeed");

        assert.ok(eapriv.typeHelper.isNumeric(eapriv.typeHelper.timespan.tryParse("1.02:03:04.9999999")), "serialized .NET timespan string parsing returns number (of milliseconds)");

        var result = eapriv.typeHelper.timespan.tryParse("");
        assert.ok(result.error, "empty string to timespan parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "empty string to timespan parse error message thrown");

        result = eapriv.typeHelper.timespan.tryParse(1997);
        assert.ok(result.error, "integer to timespan parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid .NET style timespan string.", "integer to timespan parse error message thrown");
    });

    qunit.test("verify_guid_parsing", function(assert) {
        assert.equal(eapriv.typeHelper.guid.tryParse("a1111111-1111-1111-1111-111111111111"), "A1111111-1111-1111-1111-111111111111", "guid string parse succeed, case insensitivity confirmed");

        var result = eapriv.typeHelper.guid.tryParse("");
        assert.ok(result.error, "empty string to guid parse error thrown");
        assert.equal(result.msg, "Given value was not recognized as a valid guid - guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).", "empty string to guid parse error message thrown");
    });

    qunit.test("verify_object_parsing", function(assert) {
        var jsonString = '{"Val":"a","Arr":[1,2]}';
        assert.deepEqual(eapriv.typeHelper.object.tryParse(jsonString), $.parseJSON(jsonString));

        var result = eapriv.typeHelper.object.tryParse("{");
        assert.ok(result.error, "broken json string to object parse error thrown");
        var expected = "Given value was not recognized as a valid JSON.";
        var actual = result.msg.substring(0, expected.length);
        assert.equal(actual, expected); // compare only explicitly defined piece of the full message (further part is engine related)
    });

    qunit.test("verify_numeric_type_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isNumeric(-1), "negative integer recognized as number");
        assert.ok(eapriv.typeHelper.isNumeric(1), "positive integer recognized as number");
        assert.ok(eapriv.typeHelper.isNumeric(0.123), "float recognized as number");

        assert.ok(!eapriv.typeHelper.isNumeric(NaN), "NaN not recognized as number");
        assert.ok(!eapriv.typeHelper.isNumeric("1"), "integer string not recognized as number");
    });

    qunit.test("verify_date_type_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")), "date object recognized as date");

        assert.ok(!eapriv.typeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"), "date string not recognized as date");
        assert.ok(!eapriv.typeHelper.isDate(807926400000), "float (ticks number) not recognized as date");
    });

    qunit.test("verify_timespan_type_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isTimeSpan("1.02:03:04.9999999"), "serialized .NET timespan string with days recognized");
        assert.ok(eapriv.typeHelper.isTimeSpan("01:02:03.9999999"), "serialized .NET timespan string without days recognized");
        assert.ok(eapriv.typeHelper.isTimeSpan("01:02:03"), "serialized .NET timespan string without days or milliseconds recognized");
        assert.ok(eapriv.typeHelper.isTimeSpan("1.02:03:04"), "serialized .NET timespan string without milliseconds recognized");
        assert.ok(eapriv.typeHelper.isTimeSpan("10675199.02:48:05.4775807"), "serialized .NET timespan string max value recognized");
        assert.ok(eapriv.typeHelper.isTimeSpan("-10675199.02:48:05.4775808"), "serialized .NET timespan string min value recognized");

        assert.ok(!eapriv.typeHelper.isTimeSpan(""), "incorrect timespan string detected");
    });

    qunit.test("verify_string_type_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isString(""), "empty string recognized as string");
        assert.ok(eapriv.typeHelper.isString("123"), "random string recognized as string");

        assert.ok(!eapriv.typeHelper.isString(123), "integer not recognized as string");
        assert.ok(!eapriv.typeHelper.isString({}), "empty object not recognized as string");
        assert.ok(!eapriv.typeHelper.isString(null), "null not recognized as string");
        assert.ok(!eapriv.typeHelper.isString(undefined), "undefined not recognized as string");
    });

    qunit.test("verify_bool_type_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isBool(true), "true bool recognized as bool");
        assert.ok(eapriv.typeHelper.isBool(false), "false bool recognized as bool");

        assert.ok(!eapriv.typeHelper.isBool("true"), "'true' string not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool("True"), "'True' string not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool("false"), "'false' string not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool("False"), "'False' string not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(""), "empty string not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(0), "0 integer not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(1), "positive integer not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(-1), "negative integer not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool({}), "empty object not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(null), "null not recognized as bool");
        assert.ok(!eapriv.typeHelper.isBool(undefined), "undefined not recognized as bool");
    });

    qunit.test("verify_guid_string_recognition", function(assert) {
        assert.ok(eapriv.typeHelper.isGuid("541422A6-FEAD-4CD9-8466-5419AA63DBE1"), "uppercased guid string recognized as guid");
        assert.ok(eapriv.typeHelper.isGuid("541422a6-fead-4cd9-8466-5419aa63dbe1"), "lowercased guid string recognized as guid");

        assert.ok(!eapriv.typeHelper.isGuid(1), "integer not recognized as guid");
        assert.ok(!eapriv.typeHelper.isGuid(""), "empty string not recognized as guid");
    });

    qunit.module("model_helper");

    qunit.test("verify_expression_evaluation", function(assert) {
        var model = {
            Number: 123,
            Stability: {
                High: 0
            },
            Items: {
                7: {
                    Items: {
                        6: {
                            Val: "abc",
                            Arr: [1.1, 2.2]
                        }
                    }
                }
            },
            Days: [true, false]
        }
        var deserializedModel = eapriv.modelHelper.deserializeObject(
            null,  // form
            null,  // fieldsMap
            {      // constsMap
                "Number": 123,
                "Stability.High": 0,
                "Items[7].Items[6].Val": "abc",
                "Items[7].Items[6].Arr[1]": 2.2,
                "Items[7].Items[6].Arr[0]": 1.1,
                "Days[0]": true,
                "Days[1]": false
            },
            null,  // enumsMap
            null,  // parsersMap
            null); // prefix
        assert.deepEqual(deserializedModel, model, 'model not deserialized properly based on given consts map');

        var expression = "Number - 23 == 100 && Stability.High == 0 && Items[7].Items[6].Val == 'abc' && Items[7].Items[6].Arr[1] / 2 == Items[7].Items[6].Arr[0] && Days[1] == false && Days[0] == true";
        var result = eapriv.modelHelper.ctxEval(expression, model);
        assert.ok(result, "expression not evaluated correctly within given model context");

        with (model) {
            assert.ok(result === eval(expression), "ctxEval gives the same result as native eval");
        }
    });

    qunit.module("logger");

    qunit.test("indentation_provided_for_logged_messages", function(assert) {
        var actual = eapriv.logger.prep("1st line\n2nd line\n3rd line");
        var expected = "1st line\n                   2nd line\n                   3rd line";
        assert.equal(actual, expected);

        actual = eapriv.logger.prep("1st line");
        expected = "1st line"; // no new line at the end
        assert.equal(actual, expected);

        var date = new Date(2017, 07, 29, 11, 5, 0);
        actual = eapriv.logger.prep("1st line\n2nd line\n3rd line", date);
        expected = "(11:05:00): 1st line\n                   2nd line\n                   3rd line";
        assert.equal(actual, expected);

        actual = eapriv.logger.prep("1st line", date);
        expected = "(11:05:00): 1st line";
        assert.equal(actual, expected);
    });

    qunit.test("proper_prefixes_applied_to_logged_messages", function(assert) {
        window.console.clear(); // clear possible leftover from mocked console buffer
        window.console.suppress();

        eapriv.logger.info("msg");
        assert.ok(console.read() === null, "information should not be logged for non-debug mode");
        eapriv.logger.warn("msg");
        assert.ok(console.read().slice(0, 8) === "[warn] (", "warning prefix broken for non-debug mode");
        eapriv.logger.fail("msg");
        assert.ok(console.read().slice(0, 8) === "[fail] (", "failure prefix broken for non-debug mode");

        ea.settings.debug = true; // switch to debug-mode

        eapriv.logger.info("msg");
        assert.ok(console.read().slice(0, 8) === "[info] (", "information prefix broken for debug mode");
        eapriv.logger.warn("msg");
        assert.ok(console.read().slice(0, 8) === "[warn] (", "warning prefix broken for debug mode");
        eapriv.logger.fail("msg");
        assert.ok(console.read().slice(0, 8) === "[fail] (", "failure prefix broken for debug mode");

        window.console.restore();
    });

    qunit.module("toolchain");

    qunit.test("verify_methods_overloading", function(assert) {
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

        assert.equal(m.Whoami(), 'method A');
        assert.equal(m.Whoami(1), 'method A1');
        assert.equal(m.Whoami(2, 'final'), 'method A2 - final');
    });

    qunit.test("verify_methods_overriding", function(assert) {
        var m = eapriv.toolchain.methods;

        eapriv.toolchain.addMethod('Whoami', function() {
            return 'method A';
        });
        eapriv.toolchain.addMethod('Whoami', function(i) {
            return 'method A' + i;
        });

        assert.equal(m.Whoami(), 'method A');
        assert.equal(m.Whoami(1), 'method A1');

        // redefine methods
        eapriv.toolchain.addMethod('Whoami', function() {
            return 'method B';
        });
        eapriv.toolchain.addMethod('Whoami', function(i) {
            return 'method B' + i;
        });

        assert.equal(m.Whoami(), 'method B');
        assert.equal(m.Whoami(1), 'method B1');
    });

    qunit.test("verify_toolchain_methods_logic", function(assert) {

        var o = {};
        eapriv.toolchain.registerMethods(o);
        var m = eapriv.toolchain.methods;

        assert.ok(eapriv.typeHelper.isNumeric(m.Now()), "Now() returns number (of milliseconds)");
        assert.ok(eapriv.typeHelper.isNumeric(m.Today()), "Today() returns number (of milliseconds)");
        assert.ok(eapriv.typeHelper.isNumeric(m.Date(1985, 2, 20)), "Date(y, M, d) returns number (of milliseconds)");
        assert.ok(eapriv.typeHelper.isNumeric(m.Date(1985, 2, 20, 0, 0, 1)), "Date(y, M, d, h, m, s) returns number (of milliseconds)");
        assert.ok(eapriv.typeHelper.isNumeric(m.ToDate('2016-04-27')), "ToDate() returns number (of milliseconds)");

        assert.ok(m.Now() > m.Today());
        assert.ok(m.Date(1985, 2, 20) < m.Date(1985, 2, 20, 0, 0, 1));
        assert.equal(m.Date(1, 1, 1), new Date(new Date(1, 0, 1).setFullYear(1)).getTime());
        assert.equal(m.Date(1, 1, 1), m.Date(1, 1, 1, 0, 0, 0));
        assert.equal(m.ToDate('2016-04-27'), Date.parse('2016-04-27'));

        assert.ok(m.TimeSpan(1, 0, 0, 0) > m.TimeSpan(0, 1, 0, 0));
        assert.equal(m.TimeSpan(0, 0, 0, 0), 0);
        assert.equal(m.TimeSpan(1, 2, 3, 4), 4 * 1000 + 3 * 60 * 1000 + 2 * 60 * 60 * 1000 + 1 * 24 * 60 * 60 * 1000);

        assert.equal(m.Length('0123'), 4);
        assert.equal(m.Length('    '), 4);
        assert.equal(m.Length(null), 0);
        assert.equal(m.Length(''), 0);

        assert.equal(m.Trim(' a b c '), 'a b c');
        assert.equal(m.Trim(null), null);
        assert.equal(m.Trim(''), '');

        assert.equal(m.Concat(' a ', ' b '), ' a  b ');
        assert.equal(m.Concat(null, null), '');
        assert.equal(m.Concat('', ''), '');

        assert.equal(m.Concat(' a ', ' b ', ' c '), ' a  b  c ');
        assert.equal(m.Concat(null, null, null), '');
        assert.equal(m.Concat('', '', ''), '');

        assert.equal(m.CompareOrdinal(' abc ', ' ABC '), 1);
        assert.equal(m.CompareOrdinal('a', 'a'), 0);
        assert.equal(m.CompareOrdinal('a', 'A'), 1);
        assert.equal(m.CompareOrdinal('A', 'a'), -1);
        assert.equal(m.CompareOrdinal('a', 'b'), -1);
        assert.equal(m.CompareOrdinal('b', 'a'), 1);
        assert.equal(m.CompareOrdinal(null, 'a'), -1);
        assert.equal(m.CompareOrdinal('a', null), 1);
        assert.equal(m.CompareOrdinal(' ', 'a'), -1);
        assert.equal(m.CompareOrdinal('a', ' '), 1);
        assert.equal(m.CompareOrdinal(null, ''), -1);
        assert.equal(m.CompareOrdinal('', null), 1);
        assert.equal(m.CompareOrdinal(null, null), 0);
        assert.equal(m.CompareOrdinal('', ''), 0);

        assert.equal(m.CompareOrdinalIgnoreCase(' abc ', ' ABC '), 0);
        assert.equal(m.CompareOrdinalIgnoreCase('a', 'a'), 0);
        assert.equal(m.CompareOrdinalIgnoreCase('a', 'A'), 0);
        assert.equal(m.CompareOrdinalIgnoreCase('A', 'a'), 0);
        assert.equal(m.CompareOrdinalIgnoreCase('a', 'b'), -1);
        assert.equal(m.CompareOrdinalIgnoreCase('b', 'a'), 1);
        assert.equal(m.CompareOrdinalIgnoreCase(null, 'a'), -1);
        assert.equal(m.CompareOrdinalIgnoreCase('a', null), 1);
        assert.equal(m.CompareOrdinalIgnoreCase(' ', 'a'), -1);
        assert.equal(m.CompareOrdinalIgnoreCase('a', ' '), 1);
        assert.equal(m.CompareOrdinalIgnoreCase(null, ''), -1);
        assert.equal(m.CompareOrdinalIgnoreCase('', null), 1);
        assert.equal(m.CompareOrdinalIgnoreCase(null, null), 0);
        assert.equal(m.CompareOrdinalIgnoreCase('', ''), 0);

        assert.ok(!m.StartsWith(' ab c', ' A'));
        assert.ok(m.StartsWith(' ab c', ' a'));
        assert.ok(m.StartsWith(' ', ' '));
        assert.ok(m.StartsWith('', ''));
        assert.ok(!m.StartsWith(null, ''));
        assert.ok(!m.StartsWith('', null));
        assert.ok(!m.StartsWith(null, null));

        assert.ok(m.StartsWithIgnoreCase(' ab c', ' A'));
        assert.ok(m.StartsWithIgnoreCase(' ab c', ' a'));
        assert.ok(m.StartsWithIgnoreCase(' ', ' '));
        assert.ok(m.StartsWithIgnoreCase('', ''));
        assert.ok(!m.StartsWithIgnoreCase(null, ''));
        assert.ok(!m.StartsWithIgnoreCase('', null));
        assert.ok(!m.StartsWithIgnoreCase(null, null));

        assert.ok(!m.EndsWith(' ab c', ' C'));
        assert.ok(m.EndsWith(' ab c', ' c'));
        assert.ok(m.EndsWith(' ', ' '));
        assert.ok(m.EndsWith('', ''));
        assert.ok(!m.EndsWith(null, ''));
        assert.ok(!m.EndsWith('', null));
        assert.ok(!m.EndsWith(null, null));

        assert.ok(m.EndsWithIgnoreCase(' ab c', ' C'));
        assert.ok(m.EndsWithIgnoreCase(' ab c', ' c'));
        assert.ok(m.EndsWithIgnoreCase(' ', ' '));
        assert.ok(m.EndsWithIgnoreCase('', ''));
        assert.ok(!m.EndsWithIgnoreCase(null, ''));
        assert.ok(!m.EndsWithIgnoreCase('', null));
        assert.ok(!m.EndsWithIgnoreCase(null, null));

        assert.ok(!m.Contains(' ab c', 'B '));
        assert.ok(m.Contains(' ab c', 'b '));
        assert.ok(m.Contains(' ', ' '));
        assert.ok(m.Contains('', ''));
        assert.ok(!m.Contains(null, ''));
        assert.ok(!m.Contains('', null));
        assert.ok(!m.Contains(null, null));

        assert.ok(m.ContainsIgnoreCase(' ab c', 'B '));
        assert.ok(m.ContainsIgnoreCase(' ab c', 'b '));
        assert.ok(m.ContainsIgnoreCase(' ', ' '));
        assert.ok(m.ContainsIgnoreCase('', ''));
        assert.ok(!m.ContainsIgnoreCase(null, ''));
        assert.ok(!m.ContainsIgnoreCase('', null));
        assert.ok(!m.ContainsIgnoreCase(null, null));

        assert.ok(m.IsNullOrWhiteSpace(' '));
        assert.ok(m.IsNullOrWhiteSpace(null));
        assert.ok(m.IsNullOrWhiteSpace(''));

        assert.ok(m.IsDigitChain('0123456789'));
        assert.ok(!m.IsDigitChain('+0'));
        assert.ok(!m.IsDigitChain('-0'));
        assert.ok(!m.IsDigitChain(null));
        assert.ok(!m.IsDigitChain(''));

        assert.ok(m.IsNumber('0'));
        assert.ok(m.IsNumber('0.0'));
        assert.ok(m.IsNumber('10.10'));
        assert.ok(m.IsNumber('0e0'));
        assert.ok(m.IsNumber('.2'));
        assert.ok(m.IsNumber('3.14'));
        assert.ok(m.IsNumber('5e6'));
        assert.ok(m.IsNumber('5e-6'));
        assert.ok(m.IsNumber('5e+6'));
        assert.ok(m.IsNumber('9.0E-10'));
        assert.ok(m.IsNumber('.11e10'));
        assert.ok(m.IsNumber('-0.3e-2'));
        assert.ok(m.IsNumber('+0.3e-2'));
        assert.ok(m.IsNumber('+0'));
        assert.ok(m.IsNumber('-0'));
        assert.ok(!m.IsNumber('++0'));
        assert.ok(!m.IsNumber('--0'));
        assert.ok(!m.IsNumber('+-0'));
        assert.ok(!m.IsNumber(null));
        assert.ok(!m.IsNumber(''));

        assert.ok(m.IsEmail('nickname@domain.com'));
        assert.ok(!m.IsEmail(null));
        assert.ok(!m.IsEmail(''));

        assert.ok(m.IsPhone('+48 999 888 777'));
        assert.ok(m.IsPhone('(0048) 999 888 777'));
        assert.ok(m.IsPhone('(+48) 999888777'));
        assert.ok(m.IsPhone('112'));
        assert.ok(!m.IsPhone('11 22 333xx'));
        assert.ok(!m.IsPhone(null));
        assert.ok(!m.IsPhone(''));

        assert.ok(m.IsUrl('http://www.github.com/'));
        assert.ok(!m.IsUrl(null));
        assert.ok(!m.IsUrl(''));

        assert.ok(m.IsRegexMatch('-0.3e-2', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        assert.ok(!m.IsRegexMatch(null, '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        assert.ok(!m.IsRegexMatch('', '^[\\+-]?\\d*\\.?\\d+(?:[eE][\\+-]?\\d+)?$'));
        assert.ok(m.IsRegexMatch('', ''));
        assert.ok(!m.IsRegexMatch(null, ''));
        assert.ok(!m.IsRegexMatch('', null));
        assert.ok(!m.IsRegexMatch(null, null));

        assert.equal(m.Min(1), 1);
        assert.equal(m.Max(1), 1);
        assert.equal(m.Sum(1), 1);
        assert.equal(m.Average(1), 1);

        assert.equal(m.Min(1, 2, 3), 1);
        assert.equal(m.Max(1, 2, 3), 3);
        assert.equal(m.Sum(1, 2, 3), 6);
        assert.equal(m.Average(1, 2, 3), 2);

        assert.equal(m.Min([1]), 1);
        assert.equal(m.Max([1]), 1);
        assert.equal(m.Sum([1]), 1);
        assert.equal(m.Average([1]), 1);

        assert.equal(m.Min([1, 2, 3]), 1);
        assert.equal(m.Max([1, 2, 3]), 3);
        assert.equal(m.Sum([1, 2, 3]), 6);
        assert.equal(m.Average([1, 2, 3]), 2);

        var actions = [m.Min, m.Max, m.Sum, m.Average];
        for (var i = 0; i < actions.length; i++) {
            var action = actions[i];
            assert.throws(function() { action(); }, 'no arguments');
            assert.throws(function() { action([]); }, 'empty sequence');
        }

        assert.equal(m.Guid('a1111111-1111-1111-1111-111111111111'), m.Guid('A1111111-1111-1111-1111-111111111111'));
    });

    qunit.test("verify_allowed_settings_setup", function(assert) {
        window.console.clear(); // clear possible leftover from mocked console buffer
        window.console.suppress();
        ea.settings.apply({ debug: true });
        assert.equal(ea.settings.debug, true);
        assert.ok(console.read().indexOf("EA settings applied") !== -1, "EA setup not logged for debug mode");
        window.console.restore();

        ea.settings.apply({ debug: false });
        assert.equal(ea.settings.debug, false);

        ea.settings.apply({ dependencyTriggers: 'change paste keyup' });
        assert.equal(ea.settings.dependencyTriggers, 'change paste keyup');

        ea.settings.apply({ dependencyTriggers: undefined });
        assert.equal(ea.settings.dependencyTriggers, undefined);

        ea.settings.apply({ dependencyTriggers: null });
        assert.equal(ea.settings.dependencyTriggers, null);

        ea.settings.apply({ dependencyTriggers: '' });
        assert.equal(ea.settings.dependencyTriggers, '');
    });

    qunit.test("detect_invalid_debug_setup", function(assert) {
        assert.throws(
            function() {
                ea.settings.apply({ debug: 1 });
            },
            function(ex) {
                return ex.toString() === 'EA settings error: debug value must be a boolean (true or false)';
            }
        );
    });

    qunit.test("detect_invalid_optimize_setup", function(assert) {
        assert.throws(
            function() {
                ea.settings.apply({ optimize: 1 });
            },
            function(ex) {
                return ex.toString() === 'EA settings error: optimize value must be a boolean (true or false)';
            }
        );
    });

    qunit.test("detect_invalid_enums_setup", function(assert) {
        assert.throws(
            function() {
                ea.settings.apply({ enumsAsNumbers: 1 });
            },
            function(ex) {
                return ex.toString() === 'EA settings error: enumsAsNumbers value must be a boolean (true or false)';
            }
        );
    });

    qunit.test("detect_invalid_triggers_setup", function(assert) {
        assert.throws(
            function() {
                ea.settings.apply({ dependencyTriggers: false });
            },
            function(ex) {
                return ex.toString() === 'EA settings error: dependencyTriggers value must be a string (multiple event types can be bound at once by including each one separated by a space), null or undefined';
            }
        );
    });

    qunit.module("full_computation_flow");

    qunit.test("verify_assertthat_not_computed_for_null_value", function(assert) {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'false', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeAssertThat('asserthat', null, elementMock, paramsMock);
        assert.ok(result.valid); // ok - satisfied despite false assertion (not invoked due to null value)

        // the same behavior for empty or undefined
        result = eapriv.computeAssertThat('asserthat', '', elementMock, paramsMock);
        assert.ok(result.valid);
        result = eapriv.computeAssertThat('asserthat', undefined, elementMock, paramsMock);
        assert.ok(result.valid);
    });

    qunit.test("verify_requiredif_not_computed_for_non_null_value", function(assert) {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'true', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeRequiredIf('requiredif', {}, elementMock, paramsMock);
        assert.ok(result.valid); // ok - not required despite requirement obligation (not invoked due to non-null value)

        // the same behavior for whitespace string if allowed
        paramsMock.allowEmpty = true;
        result = eapriv.computeRequiredIf('requiredif', ' ', elementMock, paramsMock);
        assert.ok(result.valid);
    });

    qunit.test("verify_assertthat_computed_for_non_null_value", function(assert) {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'false', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeAssertThat('asserthat', {}, elementMock, paramsMock);
        assert.ok(!result.valid); // not ok - not satisfied because of false assertion
    });

    qunit.test("verify_requiredif_computed_for_null_value", function(assert) {

        var elementMock = { name: 'name' };
        var paramsMock = { expression: 'true', fieldsMap: {}, constsMap: {}, parsersMap: {} };
        var result = eapriv.computeRequiredIf('requiredif', null, elementMock, paramsMock);
        assert.ok(!result.valid); // not ok - required because of requirement obligation

        // the same behavior for empty or undefined
        result = eapriv.computeRequiredIf('requiredif', '', elementMock, paramsMock);
        assert.ok(!result.valid);
        result = eapriv.computeRequiredIf('requiredif', undefined, elementMock, paramsMock);
        assert.ok(!result.valid);

        // the same behavior for whitespace string if not allowed
        paramsMock.allowEmpty = false;
        result = eapriv.computeRequiredIf('requiredif', ' ', elementMock, paramsMock);
        assert.ok(!result.valid);
    });

    qunit.test("verify_assertthat_complex_expression_computation", function(assert) {

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
        var result = eapriv.computeAssertThat('asserthat', {}, elementMock, paramsMock);
        assert.ok(!result.valid);
    });

    qunit.test("verify_requiredif_complex_expression_computation", function(assert) {

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
        var result = eapriv.computeRequiredIf('requiredif', null, elementMock, paramsMock);
        assert.ok(!result.valid);
    });

}($, QUnit, window.ea, window.ea._private));
