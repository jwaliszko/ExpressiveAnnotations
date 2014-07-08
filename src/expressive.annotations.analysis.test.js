///<reference path="./expressive.annotations.analysis.js"/>

//debugger; // enable firebug (preferably, check 'on for all web pages' option) for the debugger to aunch 
(function(window, analyser, helper) { //scoping function (top-level, usually anonymous, prevents global namespace pollution)

window.module("expressions analysis");

    test("verify_tokenization", function() {
        var expression = "( true && (true) ) || false";
        var tokenizer = new analyser.Tokenizer(['true', 'false', '&&', '\\|\\|', '\\!', '\\(', '\\)']);

        var tokens = tokenizer.analyze(expression);
        window.equal(tokens.length, 9);
        window.equal(tokens[0], "(");
        window.equal(tokens[1], "true");
        window.equal(tokens[2], "&&");
        window.equal(tokens[3], "(");
        window.equal(tokens[4], "true");
        window.equal(tokens[5], ")");
        window.equal(tokens[6], ")");
        window.equal(tokens[7], "||");
        window.equal(tokens[8], "false");

        window.raises(function() {
            tokenizer.analyze("true + false");
        }, function(err) {
            return err === "Tokenizer error. Unexpected token started at + false.";
        });
        window.raises(function() {
            tokenizer.analyze("true && 7");
        }, function(err) {
            return err === "Tokenizer error. Unexpected token started at 7.";
        });
    });

    test("verify_infix_parsing", function() {
        var converter = new analyser.InfixParser();

        window.equal(converter.convert("()"), "");
        window.equal(converter.convert("( true && (true) ) || false"), "true true && false ||");
        window.equal(converter.convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
                "true true false true || || || true true false false true true true false || && && || || && && false && ||");
        window.equal(converter.convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

        window.raises(function() {
            converter.convert("(");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        window.raises(function() {
            converter.convert(")");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        window.raises(function() {
            converter.convert("(( true )");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        window.raises(function() {
            converter.convert("( true && false ))");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
    });

    test("verify_postfix_parsing", function() {
        var parser = new analyser.PostfixParser();

        window.ok(parser.evaluate("true"));
        window.ok(!parser.evaluate("false"));

        window.ok(parser.evaluate("true true &&"));
        window.ok(!parser.evaluate("true false &&"));
        window.ok(!parser.evaluate("false true &&"));
        window.ok(!parser.evaluate("false false &&"));

        window.ok(parser.evaluate("true true ||"));
        window.ok(parser.evaluate("true false ||"));
        window.ok(parser.evaluate("false true ||"));
        window.ok(!parser.evaluate("false false ||"));

        window.ok(parser.evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

        window.raises(function() {
            parser.evaluate("(true)");
        }, function(err) {
            return err === "Tokenizer error. Unexpected token started at (true).";
        });
        window.raises(function () {
            parser.evaluate(" ");
        }, function (err) {
            return err === "Stack empty.";
        });
        window.raises(function () {
            parser.evaluate("");
        }, function (err) {
            return err === "Stack empty.";
        });
        window.raises(function () {
            parser.evaluate(null);
        }, function (err) {
            return err === "Stack empty.";
        });
    });

    test("verify_complex_expressions_evaluation", function() {
        var evaluator = new analyser.Evaluator();

        window.ok(evaluator.compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
        window.ok(evaluator.compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));

        window.raises(function () {
            evaluator.compute(" ");
        }, function (err) {
            return err === "Logical expression computation failed. Expression is broken.";
        });
        window.raises(function () {
            evaluator.compute("");
        }, function (err) {
            return err === "Logical expression computation failed. Expression is broken.";
        });
        window.raises(function () {
            evaluator.compute(null);
        }, function (err) {
            return err === "Logical expression computation failed. Expression is broken.";
        });
    });

    test("verify_comparison_options", function () {
        var comparer = new analyser.Comparer();

        window.ok(comparer.compute("aAa", "aAa", "==", true));
        window.ok(!comparer.compute("aAa", "aaa", "==", true));

        window.ok(!comparer.compute("aAa", "aAa", "!=", true));
        window.ok(comparer.compute("aAa", "aaa", "!=", true));

        window.ok(comparer.compute("aAa", "aAa", "==", false));
        window.ok(comparer.compute("aAa", "aaa", "==", false));

        window.ok(!comparer.compute("aAa", "aAa", "!=", false));
        window.ok(!comparer.compute("aAa", "aaa", "!=", false));
    });

    test("verify_equality_of_non_empty_elements", function() {
        var comparer = new analyser.Comparer();

        window.ok(comparer.compute("aAa", "aAa", "==", true));
        window.ok(comparer.compute(0, 0, "==", true));
        window.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "==", true));
        window.ok(comparer.compute({}, {}, "==", true));
        window.ok(comparer.compute({ error: true }, { error: true }, "==", true));
        window.ok(comparer.compute(["a", "b"], ["a", "b"], "==", true));

        window.ok(!comparer.compute("aAa", "aAa ", "==", true));
        window.ok(!comparer.compute("aAa", " aAa", "==", true));
        window.ok(!comparer.compute("aAa", "aaa", "==", true));
        window.ok(!comparer.compute(0, 1, "==", true));
        window.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:01 GMT"), "==", true));
        window.ok(!comparer.compute({ error: true }, { error: false }, "==", true));
        window.ok(!comparer.compute(["a", "b"], ["a", "B"], "==", true));
    });

    test("verify_equality_of_empty_elements", function() {
        var comparer = new analyser.Comparer();

        window.ok(comparer.compute("", "", "==", true));
        window.ok(comparer.compute(" ", " ", "==", true));
        window.ok(comparer.compute(null, null, "==", true));

        window.ok(!comparer.compute("", "*", "==", true));
        window.ok(!comparer.compute(" ", "*", "==", true));
        window.ok(!comparer.compute(null, "*", "==", true));

        window.ok(!comparer.compute("\t", "\n", "==", true));        
        window.ok(!comparer.compute("", " ", "==", true));
        window.ok(!comparer.compute("", null, "==", true));
        window.ok(!comparer.compute(null, undefined, "==", true));
    });

    test("verify_greater_and_less_comparisons", function() {
        var comparer = new analyser.Comparer();
        // assumption - arguments provided have exact types

        window.ok(comparer.compute("a", "A", ">", true));
        window.ok(comparer.compute("a", "A", ">=", true));
        window.ok(comparer.compute("abcd", "ABCD", ">", true));
        window.ok(comparer.compute("abcd", "ABCD", ">=", true));
        window.ok(comparer.compute(1, 0, ">", true));
        window.ok(comparer.compute(1, 0, ">=", true));
        window.ok(comparer.compute(0, -1, ">", true));
        window.ok(comparer.compute(0, -1, ">=", true));
        window.ok(comparer.compute(1.1, 1.01, ">", true));
        window.ok(comparer.compute(1.1, 1.01, ">=", true));
        window.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), ">", true));
        window.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), ">=", true));

        window.ok(!comparer.compute("a", null, ">", true));
        window.ok(!comparer.compute("a", null, ">=", true));
        window.ok(!comparer.compute(null, "a", ">", true));
        window.ok(!comparer.compute(null, "a", ">=", true));

        window.ok(!comparer.compute("a", "A", "<", true));
        window.ok(!comparer.compute("a", "A", "<=", true));
        window.ok(!comparer.compute("abcd", "ABCD", "<", true));
        window.ok(!comparer.compute("abcd", "ABCD", "<=", true));
        window.ok(!comparer.compute(1, 0, "<", true));
        window.ok(!comparer.compute(1, 0, "<="));
        window.ok(!comparer.compute(0, -1, "<", true));
        window.ok(!comparer.compute(0, -1, "<=", true));
        window.ok(!comparer.compute(1.1, 1.01, "<", true));
        window.ok(!comparer.compute(1.1, 1.01, "<=", true));
        window.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "<", true));
        window.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "<=", true));

        window.ok(!comparer.compute("a", null, "<", true));
        window.ok(!comparer.compute("a", null, "<=", true));
        window.ok(!comparer.compute(null, "a", "<", true));
        window.ok(!comparer.compute(null, "a", "<=", true));

        window.raises(function() {
            comparer.compute({}, {}, ">", true);
        }, function(err) {
            return err === "Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.";
        });
    });

window.module("type helper");

    test("verify_array_storage", function () {
        window.ok(helper.array.contains(["a"], "a"));
        window.ok(helper.array.contains(["a", "b"], "a"));
        window.ok(helper.array.contains(["a", "b"], "b"));
        window.ok(!helper.array.contains(["a", "b"], "c"));
    });

    test("verify_array_sanatization", function () {
        var array = ["a"];
        helper.array.sanatize(["a"], "");
        window.deepEqual(["a"], array);

        array = ["a", "a"];
        helper.array.sanatize(array, "a");
        window.deepEqual([], array);

        array = ["a", "b"];
        helper.array.sanatize(array, "");
        window.deepEqual(["a", "b"], array);

        array = ["a", "b", "c", "a", "b"];
        helper.array.sanatize(array, "b");
        window.deepEqual(["a", "c", "a"], array);
    });

    test("verify_string_formatting", function () {
        window.equal(helper.string.format("{0}", "a"), "a");
        window.equal(helper.string.format("{0}{1}", "a", "b"), "ab");
        window.equal(helper.string.format("{0}{0}", "a", "b"), "aa");
        window.equal(helper.string.format("{0}{0}", "a"), "aa");

        window.equal(helper.string.format("{0}", ["a"]), "a");
        window.equal(helper.string.format("{0}{1}", ["a", "b"]), "ab");
        window.equal(helper.string.format("{0}{0}", ["a", "b"]), "aa");
        window.equal(helper.string.format("{0}{0}", ["a"]), "aa");
    });

    test("verify_bool_parsing", function () {
        window.equal(helper.bool.tryParse(false), false);
        window.equal(helper.bool.tryParse("false"), false);
        window.equal(helper.bool.tryParse("False"), false);
        window.equal(helper.bool.tryParse(true), true);
        window.equal(helper.bool.tryParse("true"), true);
        window.equal(helper.bool.tryParse("True"), true);
        
        var result = helper.bool.tryParse("asd");
        window.equal(result.error, true);
        window.equal(result.msg, "Parsing error. Given value has no boolean meaning.");
    });

    test("verify_float_parsing", function () {
        // integer literals
        window.equal(helper.float.tryParse("-1"), -1); // negative integer string
        window.equal(helper.float.tryParse("0"), 0); // zero string
        window.equal(helper.float.tryParse("1"), 1); // positive integer string
        window.equal(helper.float.tryParse(-1), -1); // negative integer number
        window.equal(helper.float.tryParse(0), 0); // zero integer number
        window.equal(helper.float.tryParse(1), 1); // positive integer number
        window.equal(helper.float.tryParse(0xFF), 255); // hexadecimal integer literal

        // floating-point literals
        window.equal(helper.float.tryParse("-1.1"), -1.1); // negative floating point string
        window.equal(helper.float.tryParse("1.1"), 1.1); // positive floating point string
        window.equal(helper.float.tryParse(-1.1), -1.1); // negative floating point number
        window.equal(helper.float.tryParse(1.1), 1.1); // positive floating point number
        window.equal(helper.float.tryParse("314e-2"), 3.14); // exponential notation string 
        window.equal(helper.float.tryParse(314e-2), 3.14); // exponential notation

        // non-numeric valuer
        var result = helper.float.tryParse(""); // empty string
        window.equal(result.error, true);
        window.equal(result.msg, "Parsing error. Given value has no numeric meaning.");

        window.ok(helper.float.tryParse(" ").error); // whitespace character
        window.ok(helper.float.tryParse("\t").error); // tab characters
        window.ok(helper.float.tryParse("asd").error); // non-numeric character string
        window.ok(helper.float.tryParse("true").error); // boolean true
        window.ok(helper.float.tryParse("false").error); // boolean false
        window.ok(helper.float.tryParse("asd123").error); // number with preceding non-numeric characters
        window.ok(helper.float.tryParse("123asd").error); // number with trailling non-numeric characters
        window.ok(helper.float.tryParse(undefined).error); // undefined value
        window.ok(helper.float.tryParse(null).error); // null value
        window.ok(helper.float.tryParse(NaN).error); // NaN value
        window.ok(helper.float.tryParse(Infinity).error); // infinity primitive
        window.ok(helper.float.tryParse(+Infinity).error); // positive Infinity
        window.ok(helper.float.tryParse(-Infinity).error); // negative Infinity
        window.ok(helper.float.tryParse(new Date(Date.now())).error); // date object
        window.ok(helper.float.tryParse({}).error); // empty object
    });

    test("verify_date_parsing", function () {
        var now = Date.now();
        window.deepEqual(helper.date.tryParse(new Date(now)), new Date(now));
        window.deepEqual(helper.date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), new Date(807926400000));
        window.deepEqual(helper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), new Date(0));
        window.deepEqual(helper.date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), new Date(14400000));

        var result = helper.date.tryParse("");
        window.equal(result.error, true);
        window.equal(result.msg, "Parsing error. Given value is not a string representing an RFC 2822 or ISO 8601 date.");
    });

    test("verify_numeric_recognition", function () {
        window.ok(helper.isNumeric(1));
        window.ok(!helper.isNumeric(NaN));
        window.ok(!helper.isNumeric("1"));
    });

    test("verify_date_recognition", function () {
        window.ok(helper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")));
        window.ok(!helper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"));
        window.ok(!helper.isDate(807926400000));
    });

    test("verify_string_recognition", function () {
        window.ok(helper.isString(""));
        window.ok(helper.isString("123"));
        window.ok(!helper.isString(123));
        window.ok(!helper.isString({}));
        window.ok(!helper.isString(null));
        window.ok(!helper.isString(undefined));
    });

    test("verify_bool_recognition", function () {
        window.ok(helper.isBool(true));
        window.ok(!helper.isBool("true"));
        window.ok(!helper.isBool(0));
    });

}(window, ea.analyser, ea.helper));
