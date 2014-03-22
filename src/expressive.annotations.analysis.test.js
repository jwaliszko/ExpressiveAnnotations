///<reference path="./expressive.annotations.analysis.js"/>

(function(global, analyser) { //scoping function (top-level, usually anonymous, function that prevents global namespace pollution)

    global.module("expressive.annotations.analysis.test");

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

}(window, BooleanExpressionsAnalyser));
