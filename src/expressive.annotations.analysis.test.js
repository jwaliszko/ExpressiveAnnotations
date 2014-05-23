///<reference path="./expressive.annotations.analysis.js"/>

(function(global, analyser) { //scoping function (top-level, usually anonymous, function that prevents global namespace pollution)

    global.module("expressive.annotations.analysis.test");
    //debugger; //enable firebug for all web pages

    test("Verify_infix_lexer_logic", function() {
        var expression = "( true && (true) ) || false";
        var lexer = new analyser.InfixLexer();

        var tokens = lexer.analyze(expression, false);
        global.equal(tokens.length, 15);
        global.equal(tokens[0], "(");
        global.equal(tokens[1], " ");
        global.equal(tokens[2], "true");
        global.equal(tokens[3], " ");
        global.equal(tokens[4], "&&");
        global.equal(tokens[5], " ");
        global.equal(tokens[6], "(");
        global.equal(tokens[7], "true");
        global.equal(tokens[8], ")");
        global.equal(tokens[9], " ");
        global.equal(tokens[10], ")");
        global.equal(tokens[11], " ");
        global.equal(tokens[12], "||");
        global.equal(tokens[13], " ");
        global.equal(tokens[14], "false");

        tokens = lexer.analyze(expression, true);
        global.equal(tokens.length, 9);
        global.equal(tokens[0], "(");
        global.equal(tokens[1], "true");
        global.equal(tokens[2], "&&");
        global.equal(tokens[3], "(");
        global.equal(tokens[4], "true");
        global.equal(tokens[5], ")");
        global.equal(tokens[6], ")");
        global.equal(tokens[7], "||");
        global.equal(tokens[8], "false");

        global.raises(function() {
            lexer.analyze("true + false", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"+ false\".";
        });
        global.raises(function() {
            lexer.analyze("true && 7", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"7\".";
        });
    });

    test("Verify_postfix_lexer_logic", function() {
        var expression = "true true && false ||";
        var lexer = new analyser.PostfixLexer();

        var tokens = lexer.analyze(expression, false);
        global.equal(tokens.length, 9);
        global.equal(tokens[0], "true");
        global.equal(tokens[1], " ");
        global.equal(tokens[2], "true");
        global.equal(tokens[3], " ");
        global.equal(tokens[4], "&&");
        global.equal(tokens[5], " ");
        global.equal(tokens[6], "false");
        global.equal(tokens[7], " ");
        global.equal(tokens[8], "||");

        tokens = lexer.analyze(expression, true);
        global.equal(tokens.length, 5);
        global.equal(tokens[0], "true");
        global.equal(tokens[1], "true");
        global.equal(tokens[2], "&&");
        global.equal(tokens[3], "false");
        global.equal(tokens[4], "||");

        global.raises(function() {
            lexer.analyze("true && (false)", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"(false)\".";
        });
        global.raises(function() {
            lexer.analyze("true + 7", false);
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"+ 7\".";
        });
    });

    test("Verify_infix_to_postfix_conversion", function() {
        var converter = new analyser.InfixToPostfixConverter();

        global.equal(converter.convert("()"), "");
        global.equal(converter.convert("( true && (true) ) || false"), "true true && false ||");
        global.equal(converter.convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
            "true true false true || || || true true false false true true true false || && && || || && && false && ||");
        global.equal(converter.convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

        global.raises(function() {
            converter.convert("(");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        global.raises(function() {
            converter.convert(")");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        global.raises(function() {
            converter.convert("(( true )");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
        global.raises(function() {
            converter.convert("( true && false ))");
        }, function(err) {
            return err === "Infix expression parsing error. Incorrect nesting.";
        });
    });

    test("Verify_postfix_parser", function() {
        var parser = new analyser.PostfixParser();

        global.ok(parser.evaluate("true"));
        global.ok(!parser.evaluate("false"));

        global.ok(parser.evaluate("true true &&"));
        global.ok(!parser.evaluate("true false &&"));
        global.ok(!parser.evaluate("false true &&"));
        global.ok(!parser.evaluate("false false &&"));

        global.ok(parser.evaluate("true true ||"));
        global.ok(parser.evaluate("true false ||"));
        global.ok(parser.evaluate("false true ||"));
        global.ok(!parser.evaluate("false false ||"));

        global.ok(parser.evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

        global.raises(function() {
            parser.evaluate("(true)");
        }, function(err) {
            return err === "Lexer error. Unexpected token started at \"(true)\".";
        });
    });

    test("Verify_complex_expression_evaluation", function() {
        var evaluator = new analyser.Evaluator();

        global.ok(evaluator.compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
        global.ok(evaluator.compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));
    });

    test("Verify_typehelper_array_contains", function () {
        global.ok(analyser.TypeHelper.Array.contains(["a"], "a"));
        global.ok(analyser.TypeHelper.Array.contains(["a", "b"], "a"));
        global.ok(analyser.TypeHelper.Array.contains(["a", "b"], "b"));
        global.ok(!analyser.TypeHelper.Array.contains(["a", "b"], "c"));
    });

    test("Verify_typehelper_array_sanatize", function () {
        var array = ["a"];
        analyser.TypeHelper.Array.sanatize(["a"], "");
        global.deepEqual(["a"], array);

        array = ["a", "a"];
        analyser.TypeHelper.Array.sanatize(array, "a");
        global.deepEqual([], array);

        array = ["a", "b"];
        analyser.TypeHelper.Array.sanatize(array, "");
        global.deepEqual(["a", "b"], array);

        array = ["a", "b", "c", "a", "b"];
        analyser.TypeHelper.Array.sanatize(array, "b");
        global.deepEqual(["a", "c", "a"], array);
    });

    test("Verify_typehelper_string_format", function () {
        global.equal(analyser.TypeHelper.String.format("{0}", "a"), "a");
        global.equal(analyser.TypeHelper.String.format("{0}{1}", "a", "b"), "ab");
        global.equal(analyser.TypeHelper.String.format("{0}{0}", "a", "b"), "aa");
        global.equal(analyser.TypeHelper.String.format("{0}{0}", "a"), "aa");

        global.equal(analyser.TypeHelper.String.format("{0}", ["a"]), "a");
        global.equal(analyser.TypeHelper.String.format("{0}{1}", ["a", "b"]), "ab");
        global.equal(analyser.TypeHelper.String.format("{0}{0}", ["a", "b"]), "aa");
        global.equal(analyser.TypeHelper.String.format("{0}{0}", ["a"]), "aa");
    });

    test("Verify_typehelper_bool_tryparse", function () {
        global.equal(analyser.TypeHelper.Bool.tryParse(false), false);
        global.equal(analyser.TypeHelper.Bool.tryParse("false"), false);
        global.equal(analyser.TypeHelper.Bool.tryParse("False"), false);
        global.equal(analyser.TypeHelper.Bool.tryParse(true), true);
        global.equal(analyser.TypeHelper.Bool.tryParse("true"), true);
        global.equal(analyser.TypeHelper.Bool.tryParse("True"), true);
        
        var result = analyser.TypeHelper.Bool.tryParse("asd");
        global.equal(result.error, true);
        global.equal(result.msg, "Parsing error. Given value has no boolean meaning.");
    });

    test("Verify_typehelper_float_tryparse", function () {
        // integer literals
        global.equal(analyser.TypeHelper.Float.tryParse("-1"), -1); // negative integer string
        global.equal(analyser.TypeHelper.Float.tryParse("0"), 0); // zero string
        global.equal(analyser.TypeHelper.Float.tryParse("1"), 1); // positive integer string
        global.equal(analyser.TypeHelper.Float.tryParse(-1), -1); // negative integer number
        global.equal(analyser.TypeHelper.Float.tryParse(0), 0); // zero integer number
        global.equal(analyser.TypeHelper.Float.tryParse(1), 1); // positive integer number
        global.equal(analyser.TypeHelper.Float.tryParse(0xFF), 255); // hexadecimal integer literal

        // floating-point literals
        global.equal(analyser.TypeHelper.Float.tryParse("-1.1"), -1.1); // negative floating point string
        global.equal(analyser.TypeHelper.Float.tryParse("1.1"), 1.1); // positive floating point string
        global.equal(analyser.TypeHelper.Float.tryParse(-1.1), -1.1); // negative floating point number
        global.equal(analyser.TypeHelper.Float.tryParse(1.1), 1.1); // positive floating point number
        global.equal(analyser.TypeHelper.Float.tryParse("314e-2"), 3.14); // exponential notation string 
        global.equal(analyser.TypeHelper.Float.tryParse(314e-2), 3.14); // exponential notation

        // non-numeric valuer
        var result = analyser.TypeHelper.Float.tryParse(""); // empty string
        global.equal(result.error, true);
        global.equal(result.msg, "Parsing error. Given value has no numeric meaning.");

        global.ok(analyser.TypeHelper.Float.tryParse(" ").error); // whitespace character
        global.ok(analyser.TypeHelper.Float.tryParse("\t").error); // tab characters
        global.ok(analyser.TypeHelper.Float.tryParse("asd").error); // non-numeric character string
        global.ok(analyser.TypeHelper.Float.tryParse("true").error); // boolean true
        global.ok(analyser.TypeHelper.Float.tryParse("false").error); // boolean false
        global.ok(analyser.TypeHelper.Float.tryParse("asd123").error); // number with preceding non-numeric characters
        global.ok(analyser.TypeHelper.Float.tryParse("123asd").error); // number with trailling non-numeric characters
        global.ok(analyser.TypeHelper.Float.tryParse(undefined).error); // undefined value
        global.ok(analyser.TypeHelper.Float.tryParse(null).error); // null value
        global.ok(analyser.TypeHelper.Float.tryParse(NaN).error); // NaN value
        global.ok(analyser.TypeHelper.Float.tryParse(Infinity).error); // infinity primitive
        global.ok(analyser.TypeHelper.Float.tryParse(+Infinity).error); // positive Infinity
        global.ok(analyser.TypeHelper.Float.tryParse(-Infinity).error); // negative Infinity
        global.ok(analyser.TypeHelper.Float.tryParse(new Date(Date.now())).error); // date object
        global.ok(analyser.TypeHelper.Float.tryParse({}).error); // empty object
    });

    test("Verify_typehelper_date_tryparse", function () {
        var now = Date.now();
        global.deepEqual(analyser.TypeHelper.Date.tryParse(new Date(now)), new Date(now));
        global.deepEqual(analyser.TypeHelper.Date.tryParse("Wed, 09 Aug 1995 00:00:00 GMT"), new Date(807926400000));
        global.deepEqual(analyser.TypeHelper.Date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT"), new Date(0));
        global.deepEqual(analyser.TypeHelper.Date.tryParse("Thu, 01 Jan 1970 00:00:00 GMT-0400"), new Date(14400000));

        var result = analyser.TypeHelper.Date.tryParse("");
        global.equal(result.error, true);
        global.equal(result.msg, "Parsing error. Given value is not a string representing an RFC 2822 or ISO 8601 date.");
    });

    test("Verify_typehelper_is_empty", function () {
        global.ok(analyser.TypeHelper.isEmpty(null));
        global.ok(analyser.TypeHelper.isEmpty(undefined));
        global.ok(analyser.TypeHelper.isEmpty(""));
        global.ok(analyser.TypeHelper.isEmpty(" "));
        global.ok(analyser.TypeHelper.isEmpty("\t"));
        global.ok(analyser.TypeHelper.isEmpty("\n"));
        global.ok(analyser.TypeHelper.isEmpty("\n\t "));
    });

    test("Verify_typehelper_is_numeric", function () {
        global.ok(analyser.TypeHelper.isNumeric(1));
        global.ok(!analyser.TypeHelper.isNumeric(NaN));
        global.ok(!analyser.TypeHelper.isNumeric("1"));
    });

    test("Verify_typehelper_is_date", function () {
        global.ok(analyser.TypeHelper.isDate(new Date("Wed, 09 Aug 1995 00:00:00 GMT")));
        global.ok(!analyser.TypeHelper.isDate("Wed, 09 Aug 1995 00:00:00 GMT"));
        global.ok(!analyser.TypeHelper.isDate(807926400000));
    });

    test("Verify_typehelper_is_string", function () {
        global.ok(analyser.TypeHelper.isString(""));
        global.ok(analyser.TypeHelper.isString("123"));
        global.ok(!analyser.TypeHelper.isString(123));
        global.ok(!analyser.TypeHelper.isString({}));
        global.ok(!analyser.TypeHelper.isString(null));
        global.ok(!analyser.TypeHelper.isString(undefined));
    });

    test("Verify_typehelper_is_bool", function () {
        global.ok(analyser.TypeHelper.isBool(true));
        global.ok(!analyser.TypeHelper.isBool("true"));
        global.ok(!analyser.TypeHelper.isBool(0));
    });

    test("Verify_comparison_equals_non_empty", function () {
        var comparer = new analyser.Comparer();

        global.ok(comparer.compute("aAa", "aAa", "=="));
        global.ok(comparer.compute(0, 0, "=="));
        global.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "=="));
        global.ok(comparer.compute({ }, { }, "=="));
        global.ok(comparer.compute({ error: true }, { error: true }, "=="));
        global.ok(comparer.compute(["a", "b"], ["a", "b"], "=="));        

        global.ok(!comparer.compute("aAa", "aaa", "=="));
        global.ok(!comparer.compute(0, 1, "=="));
        global.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:00 GMT"), new Date("Wed, 09 Aug 1995 00:00:01 GMT"), "=="));
        global.ok(!comparer.compute({ error: true }, { error: false }, "=="));
        global.ok(!comparer.compute(["a", "b"], ["a", "B"], "=="));
    });

    test("Verify_comparison_equals_empty", function () {
        var comparer = new analyser.Comparer();

        global.ok(comparer.compute("", "", "=="));
        global.ok(comparer.compute(" ", " ", "=="));
        global.ok(comparer.compute("\t", "\n", "=="));        
        global.ok(comparer.compute(null, null, "=="));
        global.ok(comparer.compute("", " ", "=="));
        global.ok(comparer.compute("\n\t ", null, "=="));
        global.ok(comparer.compute(null, undefined, "=="));
    });

    test("Verify_comparison_greater_and_less", function () {
        var comparer = new analyser.Comparer();

        // assumption - arguments provided have exact types

        global.ok(comparer.compute("a", "A", ">"));
        global.ok(comparer.compute("abcd", "ABCD", ">"));
        global.ok(comparer.compute(1, 0, ">"));
        global.ok(comparer.compute(0, -1, ">"));
        global.ok(comparer.compute(1.1, 1.01, ">"));
        global.ok(comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), ">"));        

        global.ok(!comparer.compute("a", "A", "<="));
        global.ok(!comparer.compute("abcd", "ABCD", "<="));
        global.ok(!comparer.compute(1, 0, "<="));
        global.ok(!comparer.compute(0, -1, "<="));
        global.ok(!comparer.compute(1.1, 1.01, "<="));
        global.ok(!comparer.compute(new Date("Wed, 09 Aug 1995 00:00:01 GMT"), new Date("Wed, 09 Aug 1995 00:00:00 GMT"), "<="));        

        global.ok(!comparer.compute("a", null, ">"));
        global.ok(!comparer.compute(null, "a", ">"));
        global.ok(!comparer.compute("a", null, "<"));
        global.ok(!comparer.compute(null, "a", "<"));

        global.raises(function() {
            comparer.compute({}, {}, ">");
        }, function(err) {
            return err === "Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.";
        });
    });

}(window, LogicalExpressionsAnalyser));
