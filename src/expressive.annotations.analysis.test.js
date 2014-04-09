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
    });

    test("Verify_infix_to_postfix_conversion", function() {
        var converter = new analyser.InfixToPostfixConverter();

        global.equal(converter.convert("()"), "");
        global.equal(converter.convert("( true && (true) ) || false"), "true true && false ||");
        global.equal(converter.convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
            "true true false true || || || true true false false true true true false || && && || || && && false && ||");
        global.equal(converter.convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");
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
    });

    test("Verify_complex_expression_evaluation", function() {
        var evaluator = new analyser.Evaluator();

        global.ok(evaluator.compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
        global.ok(evaluator.compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));
    });

    test("Verify_utils_array_contains", function () {
        global.ok(analyser.Utils.Array.contains(["a"], "a"));
        global.ok(analyser.Utils.Array.contains(["a", "b"], "a"));
        global.ok(analyser.Utils.Array.contains(["a", "b"], "b"));
        global.ok(!analyser.Utils.Array.contains(["a", "b"], "c"));
    });

    test("Verify_utils_array_sanatize", function () {
        var array = ["a"];
        analyser.Utils.Array.sanatize(["a"], "");
        global.deepEqual(["a"], array);

        array = ["a", "a"];
        analyser.Utils.Array.sanatize(array, "a");
        global.deepEqual([], array);

        array = ["a", "b"];
        analyser.Utils.Array.sanatize(array, "");
        global.deepEqual(["a", "b"], array);

        array = ["a", "b", "c", "a", "b"];
        analyser.Utils.Array.sanatize(array, "b");
        global.deepEqual(["a", "c", "a"], array);
    });

    test("Verify_utils_string_format", function () {
        global.equal(analyser.Utils.String.format("{0}", "a"), "a");
        global.equal(analyser.Utils.String.format("{0}{1}", "a", "b"), "ab");
        global.equal(analyser.Utils.String.format("{0}{0}", "a", "b"), "aa");
        global.equal(analyser.Utils.String.format("{0}{0}", "a"), "aa");

        global.equal(analyser.Utils.String.format("{0}", ["a"]), "a");
        global.equal(analyser.Utils.String.format("{0}{1}", ["a", "b"]), "ab");
        global.equal(analyser.Utils.String.format("{0}{0}", ["a", "b"]), "aa");
        global.equal(analyser.Utils.String.format("{0}{0}", ["a"]), "aa");
    });

    test("Verify_utils_bool_tryparse", function () {
        global.equal(analyser.Utils.Bool.tryParse(false), false);
        global.equal(analyser.Utils.Bool.tryParse("false"), false);
        global.equal(analyser.Utils.Bool.tryParse("False"), false);
        global.equal(analyser.Utils.Bool.tryParse(true), true);
        global.equal(analyser.Utils.Bool.tryParse("true"), true);
        global.equal(analyser.Utils.Bool.tryParse("True"), true);
        
        var result = analyser.Utils.Bool.tryParse("asd");
        global.equal(result.error, true);
        global.equal(result.msg, "Parsing error. Given value has no boolean meaning.");
    });

}(window, BooleanExpressionsAnalyser));
