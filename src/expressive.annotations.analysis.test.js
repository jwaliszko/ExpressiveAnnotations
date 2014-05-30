///<reference path="./expressive.annotations.analysis.js"/>

(function(wnd, analyser) { //scoping function (top-level, usually anonymous, function that prevents global namespace pollution)

    wnd.module("logical expressions analysis");
    //debugger; //enable firebug for all web pages

    test("Verify_infix_lexer_logic", function() {
        var expression = "( true && (true) ) || false";
        var lexer = new analyser.InfixLexer();

        var tokens = lexer.analyze(expression, false);
        wnd.equal(tokens.length, 15);
        wnd.equal(tokens[0], "(");
        wnd.equal(tokens[1], " ");
        wnd.equal(tokens[2], "true");
        wnd.equal(tokens[3], " ");
        wnd.equal(tokens[4], "&&");
        wnd.equal(tokens[5], " ");
        wnd.equal(tokens[6], "(");
        wnd.equal(tokens[7], "true");
        wnd.equal(tokens[8], ")");
        wnd.equal(tokens[9], " ");
        wnd.equal(tokens[10], ")");
        wnd.equal(tokens[11], " ");
        wnd.equal(tokens[12], "||");
        wnd.equal(tokens[13], " ");
        wnd.equal(tokens[14], "false");

        tokens = lexer.analyze(expression, true);
        wnd.equal(tokens.length, 9);
        wnd.equal(tokens[0], "(");
        wnd.equal(tokens[1], "true");
        wnd.equal(tokens[2], "&&");
        wnd.equal(tokens[3], "(");
        wnd.equal(tokens[4], "true");
        wnd.equal(tokens[5], ")");
        wnd.equal(tokens[6], ")");
        wnd.equal(tokens[7], "||");
        wnd.equal(tokens[8], "false");

        wnd.raises(function() {
            lexer.analyze("true + false", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"+ false\".";
        });
        wnd.raises(function() {
            lexer.analyze("true && 7", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"7\".";
        });
    });

    test("Verify_postfix_lexer_logic", function() {
        var expression = "true true && false ||";
        var lexer = new analyser.PostfixLexer();

        var tokens = lexer.analyze(expression, false);
        wnd.equal(tokens.length, 9);
        wnd.equal(tokens[0], "true");
        wnd.equal(tokens[1], " ");
        wnd.equal(tokens[2], "true");
        wnd.equal(tokens[3], " ");
        wnd.equal(tokens[4], "&&");
        wnd.equal(tokens[5], " ");
        wnd.equal(tokens[6], "false");
        wnd.equal(tokens[7], " ");
        wnd.equal(tokens[8], "||");

        tokens = lexer.analyze(expression, true);
        wnd.equal(tokens.length, 5);
        wnd.equal(tokens[0], "true");
        wnd.equal(tokens[1], "true");
        wnd.equal(tokens[2], "&&");
        wnd.equal(tokens[3], "false");
        wnd.equal(tokens[4], "||");

        wnd.raises(function() {
            lexer.analyze("true && (false)", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"(false)\".";
        });
        wnd.raises(function() {
            lexer.analyze("true + 7", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"+ 7\".";
        });
    });

    test("Verify_infix_to_postfix_conversion", function() {
        var converter = new analyser.InfixToPostfixConverter();

        wnd.equal(converter.convert("()"), "");
        wnd.equal(converter.convert("( true && (true) ) || false"), "true true && false ||");
        wnd.equal(converter.convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
                "true true false true || || || true true false false true true true false || && && || || && && false && ||");
        wnd.equal(converter.convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

        wnd.raises(function() {
            converter.convert("(");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        wnd.raises(function() {
            converter.convert(")");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        wnd.raises(function() {
            converter.convert("(( true )");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        wnd.raises(function() {
            converter.convert("( true && false ))");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
    });

    test("Verify_postfix_parser", function() {
        var parser = new analyser.PostfixParser();

        wnd.ok(parser.evaluate("true"));
        wnd.ok(!parser.evaluate("false"));

        wnd.ok(parser.evaluate("true true &&"));
        wnd.ok(!parser.evaluate("true false &&"));
        wnd.ok(!parser.evaluate("false true &&"));
        wnd.ok(!parser.evaluate("false false &&"));

        wnd.ok(parser.evaluate("true true ||"));
        wnd.ok(parser.evaluate("true false ||"));
        wnd.ok(parser.evaluate("false true ||"));
        wnd.ok(!parser.evaluate("false false ||"));

        wnd.ok(parser.evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

        wnd.raises(function() {
            parser.evaluate("(true)");
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"(true)\".";
        });
        wnd.raises(function () {
            parser.evaluate("");
        }, function (err) {
            return err === "Stack empty.";
        });
        wnd.raises(function () {
            parser.evaluate(null);
        }, function (err) {
            return err === "Stack empty.";
        });
    });

    test("Verify_complex_expression_evaluation", function() {
        var evaluator = new analyser.Evaluator();

        wnd.ok(evaluator.compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
        wnd.ok(evaluator.compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));

        wnd.raises(function () {
            evaluator.compute("");
        }, function (err) {
            return err === "Logical expression computation failed. Expression is broken.";
        });
        wnd.raises(function () {
            evaluator.compute(null);
        }, function (err) {
            return err === "Logical expression computation failed. Expression is broken.";
        });
    });

    test("Verify_comparison_equals_non_empty", function () {
        var comparer = new analyser.Comparer();

        wnd.ok(comparer.compute("aAa", "aAa", "=="));
        wnd.ok(comparer.compute(0, 0, "=="));
        wnd.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "=="));
        wnd.ok(comparer.compute({}, {}, "=="));
        wnd.ok(comparer.compute({ error: true }, { error: true }, "=="));
        wnd.ok(comparer.compute(["a", "b"], ["a", "b"], "=="));

        wnd.ok(!comparer.compute("aAa", "aAa ", "=="));
        wnd.ok(!comparer.compute("aAa", " aAa", "=="));
        wnd.ok(!comparer.compute("aAa", "aaa", "=="));
        wnd.ok(!comparer.compute(0, 1, "=="));
        wnd.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:01 GMT"), "=="));
        wnd.ok(!comparer.compute({ error: true }, { error: false }, "=="));
        wnd.ok(!comparer.compute(["a", "b"], ["a", "B"], "=="));
    });

    test("Verify_comparison_equals_empty", function () {
        var comparer = new analyser.Comparer();

        wnd.ok(comparer.compute("", "", "=="));
        wnd.ok(comparer.compute(" ", " ", "=="));
        wnd.ok(comparer.compute("\t", "\n", "=="));
        wnd.ok(comparer.compute(null, null, "=="));
        wnd.ok(comparer.compute("", " ", "=="));
        wnd.ok(comparer.compute("\n\t ", null, "=="));
        wnd.ok(comparer.compute(null, undefined, "=="));
    });

    test("Verify_comparison_greater_and_less", function () {
        var comparer = new analyser.Comparer();

        // assumption - arguments provided have exact types

        wnd.ok(comparer.compute("a", "A", ">"));
        wnd.ok(comparer.compute("a", "A", ">="));
        wnd.ok(comparer.compute("abcd", "ABCD", ">"));
        wnd.ok(comparer.compute("abcd", "ABCD", ">="));
        wnd.ok(comparer.compute(1, 0, ">"));
        wnd.ok(comparer.compute(1, 0, ">="));
        wnd.ok(comparer.compute(0, -1, ">"));
        wnd.ok(comparer.compute(0, -1, ">="));
        wnd.ok(comparer.compute(1.1, 1.01, ">"));
        wnd.ok(comparer.compute(1.1, 1.01, ">="));
        wnd.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), ">"));
        wnd.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), ">="));

        wnd.ok(!comparer.compute("a", null, ">"));
        wnd.ok(!comparer.compute("a", null, ">="));
        wnd.ok(!comparer.compute(null, "a", ">"));
        wnd.ok(!comparer.compute(null, "a", ">="));
        
        wnd.ok(!comparer.compute("a", "A", "<"));
        wnd.ok(!comparer.compute("a", "A", "<="));
        wnd.ok(!comparer.compute("abcd", "ABCD", "<"));
        wnd.ok(!comparer.compute("abcd", "ABCD", "<="));
        wnd.ok(!comparer.compute(1, 0, "<"));
        wnd.ok(!comparer.compute(1, 0, "<="));
        wnd.ok(!comparer.compute(0, -1, "<"));
        wnd.ok(!comparer.compute(0, -1, "<="));
        wnd.ok(!comparer.compute(1.1, 1.01, "<"));
        wnd.ok(!comparer.compute(1.1, 1.01, "<="));
        wnd.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "<"));
        wnd.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "<="));

        wnd.ok(!comparer.compute("a", null, "<"));
        wnd.ok(!comparer.compute("a", null, "<="));
        wnd.ok(!comparer.compute(null, "a", "<"));
        wnd.ok(!comparer.compute(null, "a", "<="));
        

        wnd.raises(function () {
            comparer.compute({}, {}, ">");
        }, function (err) {
            return err === "Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.";
        });
    });

    wnd.module("type helpers");

    test("Verify_typehelper_array_contains", function () {
        wnd.ok(analyser.TypeHelper.Array.contains(["a"], "a"));
        wnd.ok(analyser.TypeHelper.Array.contains(["a", "b"], "a"));
        wnd.ok(analyser.TypeHelper.Array.contains(["a", "b"], "b"));
        wnd.ok(!analyser.TypeHelper.Array.contains(["a", "b"], "c"));
    });

    test("Verify_typehelper_array_sanatize", function () {
        var array = ["a"];
        analyser.TypeHelper.Array.sanatize(["a"], "");
        wnd.deepEqual(["a"], array);

        array = ["a", "a"];
        analyser.TypeHelper.Array.sanatize(array, "a");
        wnd.deepEqual([], array);

        array = ["a", "b"];
        analyser.TypeHelper.Array.sanatize(array, "");
        wnd.deepEqual(["a", "b"], array);

        array = ["a", "b", "c", "a", "b"];
        analyser.TypeHelper.Array.sanatize(array, "b");
        wnd.deepEqual(["a", "c", "a"], array);
    });

    test("Verify_typehelper_string_format", function () {
        wnd.equal(analyser.TypeHelper.String.format("{0}", "a"), "a");
        wnd.equal(analyser.TypeHelper.String.format("{0}{1}", "a", "b"), "ab");
        wnd.equal(analyser.TypeHelper.String.format("{0}{0}", "a", "b"), "aa");
        wnd.equal(analyser.TypeHelper.String.format("{0}{0}", "a"), "aa");

        wnd.equal(analyser.TypeHelper.String.format("{0}", ["a"]), "a");
        wnd.equal(analyser.TypeHelper.String.format("{0}{1}", ["a", "b"]), "ab");
        wnd.equal(analyser.TypeHelper.String.format("{0}{0}", ["a", "b"]), "aa");
        wnd.equal(analyser.TypeHelper.String.format("{0}{0}", ["a"]), "aa");
    });

    test("Verify_typehelper_bool_tryparse", function () {
        wnd.equal(analyser.TypeHelper.Bool.tryParse(false), false);
        wnd.equal(analyser.TypeHelper.Bool.tryParse("false"), false);
        wnd.equal(analyser.TypeHelper.Bool.tryParse("False"), false);
        wnd.equal(analyser.TypeHelper.Bool.tryParse(true), true);
        wnd.equal(analyser.TypeHelper.Bool.tryParse("true"), true);
        wnd.equal(analyser.TypeHelper.Bool.tryParse("True"), true);
        
        var result = analyser.TypeHelper.Bool.tryParse("asd");
        wnd.equal(result.error, true);
        wnd.equal(result.msg, "Parsing error. Given value has no boolean meaning.");
    });

    test("Verify_typehelper_float_tryparse", function () {
        // integer literals
        wnd.equal(analyser.TypeHelper.Float.tryParse("-1"), -1); // negative integer string
        wnd.equal(analyser.TypeHelper.Float.tryParse("0"), 0); // zero string
        wnd.equal(analyser.TypeHelper.Float.tryParse("1"), 1); // positive integer string
        wnd.equal(analyser.TypeHelper.Float.tryParse(-1), -1); // negative integer number
        wnd.equal(analyser.TypeHelper.Float.tryParse(0), 0); // zero integer number
        wnd.equal(analyser.TypeHelper.Float.tryParse(1), 1); // positive integer number
        wnd.equal(analyser.TypeHelper.Float.tryParse(0xFF), 255); // hexadecimal integer literal

        // floating-point literals
        wnd.equal(analyser.TypeHelper.Float.tryParse("-1.1"), -1.1); // negative floating point string
        wnd.equal(analyser.TypeHelper.Float.tryParse("1.1"), 1.1); // positive floating point string
        wnd.equal(analyser.TypeHelper.Float.tryParse(-1.1), -1.1); // negative floating point number
        wnd.equal(analyser.TypeHelper.Float.tryParse(1.1), 1.1); // positive floating point number
        wnd.equal(analyser.TypeHelper.Float.tryParse("314e-2"), 3.14); // exponential notation string 
        wnd.equal(analyser.TypeHelper.Float.tryParse(314e-2), 3.14); // exponential notation

        // non-numeric valuer
        var result = analyser.TypeHelper.Float.tryParse(""); // empty string
        wnd.equal(result.error, true);
        wnd.equal(result.msg, "Parsing error. Given value has no numeric meaning.");

        wnd.ok(analyser.TypeHelper.Float.tryParse(" ").error); // whitespace character
        wnd.ok(analyser.TypeHelper.Float.tryParse("\t").error); // tab characters
        wnd.ok(analyser.TypeHelper.Float.tryParse("asd").error); // non-numeric character string
        wnd.ok(analyser.TypeHelper.Float.tryParse("true").error); // boolean true
        wnd.ok(analyser.TypeHelper.Float.tryParse("false").error); // boolean false
        wnd.ok(analyser.TypeHelper.Float.tryParse("asd123").error); // number with preceding non-numeric characters
        wnd.ok(analyser.TypeHelper.Float.tryParse("123asd").error); // number with trailling non-numeric characters
        wnd.ok(analyser.TypeHelper.Float.tryParse(undefined).error); // undefined value
        wnd.ok(analyser.TypeHelper.Float.tryParse(null).error); // null value
        wnd.ok(analyser.TypeHelper.Float.tryParse(NaN).error); // NaN value
        wnd.ok(analyser.TypeHelper.Float.tryParse(Infinity).error); // infinity primitive
        wnd.ok(analyser.TypeHelper.Float.tryParse(+Infinity).error); // positive Infinity
        wnd.ok(analyser.TypeHelper.Float.tryParse(-Infinity).error); // negative Infinity
        wnd.ok(analyser.TypeHelper.Float.tryParse(new Date(Date.now())).error); // date object
        wnd.ok(analyser.TypeHelper.Float.tryParse({}).error); // empty object
    });

    test("Verify_typehelper_date_tryparse", function () {
        var now = Date.now();
        wnd.deepEqual(analyser.TypeHelper.Date.tryParse(new Date(now)), new Date(now));
        wnd.deepEqual(analyser.TypeHelper.Date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), new Date(807926400000));
        wnd.deepEqual(analyser.TypeHelper.Date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), new Date(0));
        wnd.deepEqual(analyser.TypeHelper.Date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), new Date(14400000));

        var result = analyser.TypeHelper.Date.tryParse("");
        wnd.equal(result.error, true);
        wnd.equal(result.msg, "Parsing error. Given value is not a string representing an RFC 2822 or ISO 8601 date.");
    });

    test("Verify_typehelper_is_empty", function () {
        wnd.ok(analyser.TypeHelper.isEmpty(null));
        wnd.ok(analyser.TypeHelper.isEmpty(undefined));
        wnd.ok(analyser.TypeHelper.isEmpty(""));
        wnd.ok(analyser.TypeHelper.isEmpty(" "));
        wnd.ok(analyser.TypeHelper.isEmpty("\t"));
        wnd.ok(analyser.TypeHelper.isEmpty("\n"));
        wnd.ok(analyser.TypeHelper.isEmpty("\n\t "));
    });

    test("Verify_typehelper_is_numeric", function () {
        wnd.ok(analyser.TypeHelper.isNumeric(1));
        wnd.ok(!analyser.TypeHelper.isNumeric(NaN));
        wnd.ok(!analyser.TypeHelper.isNumeric("1"));
    });

    test("Verify_typehelper_is_date", function () {
        wnd.ok(analyser.TypeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")));
        wnd.ok(!analyser.TypeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"));
        wnd.ok(!analyser.TypeHelper.isDate(807926400000));
    });

    test("Verify_typehelper_is_string", function () {
        wnd.ok(analyser.TypeHelper.isString(""));
        wnd.ok(analyser.TypeHelper.isString("123"));
        wnd.ok(!analyser.TypeHelper.isString(123));
        wnd.ok(!analyser.TypeHelper.isString({}));
        wnd.ok(!analyser.TypeHelper.isString(null));
        wnd.ok(!analyser.TypeHelper.isString(undefined));
    });

    test("Verify_typehelper_is_bool", function () {
        wnd.ok(analyser.TypeHelper.isBool(true));
        wnd.ok(!analyser.TypeHelper.isBool("true"));
        wnd.ok(!analyser.TypeHelper.isBool(0));
    });

}(window, LogicalExpressionsAnalyser));
