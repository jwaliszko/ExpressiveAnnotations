/*!
	jquery.expressive.annotations
	jQuery front-end part for ExpresiveAnnotations, annotation-based conditional validation library
	(c) 2014 Jaroslaw Waliszko - https://github.com/JaroslawWaliszko
	license: http://www.opensource.org/licenses/mit-license.php
*/

///<reference path="./expressive.annotations.analysis.js"/>

test("Verify_infix_lexer_logic", function () {
    //debugger;
    var expression = "( true && (true) ) || false";
    var lexer = new InfixLexer();

    var tokens = lexer.analyze(expression, false);
    equal(tokens.length, 15);
    equal(tokens[0], "(");
    equal(tokens[1], " ");
    equal(tokens[2], "true");
    equal(tokens[3], " ");
    equal(tokens[4], "&&");
    equal(tokens[5], " ");
    equal(tokens[6], "(");
    equal(tokens[7], "true");
    equal(tokens[8], ")");
    equal(tokens[9], " ");
    equal(tokens[10], ")");
    equal(tokens[11], " ");
    equal(tokens[12], "||");
    equal(tokens[13], " ");
    equal(tokens[14], "false");

    tokens = lexer.analyze(expression, true);
    equal(tokens.length, 9);
    equal(tokens[0], "(");
    equal(tokens[1], "true");
    equal(tokens[2], "&&");
    equal(tokens[3], "(");
    equal(tokens[4], "true");
    equal(tokens[5], ")");
    equal(tokens[6], ")");
    equal(tokens[7], "||");
    equal(tokens[8], "false");
});

test("Verify_postfix_lexer_logic", function () {

    var expression = "true true && false ||";
    var lexer = new PostfixLexer();

    var tokens = lexer.analyze(expression, false);
    equal(tokens.length, 9);
    equal(tokens[0], "true");
    equal(tokens[1], " ");
    equal(tokens[2], "true");
    equal(tokens[3], " ");
    equal(tokens[4], "&&");
    equal(tokens[5], " ");
    equal(tokens[6], "false");
    equal(tokens[7], " ");
    equal(tokens[8], "||");

    tokens = lexer.analyze(expression, true);
    equal(tokens.length, 5);
    equal(tokens[0], "true");
    equal(tokens[1], "true");
    equal(tokens[2], "&&");
    equal(tokens[3], "false");
    equal(tokens[4], "||");
});

test("Verify_infix_to_postfix_conversion", function () {

    var converter = new InfixToPostfixConverter();

    equal(converter.convert("()"), "");
    equal(converter.convert("( true && (true) ) || false"), "true true && false ||");
    equal(converter.convert(
        "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
        "true true false true || || || true true false false true true true false || && && || || && && false && ||");
    equal(converter.convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");
});

test("Verify_postfix_parser", function () {

    var parser = new PostfixParser();    
    
    ok(parser.evaluate("true"));
    ok(!parser.evaluate("false"));

    ok(parser.evaluate("true true &&"));
    ok(!parser.evaluate("true false &&"));
    ok(!parser.evaluate("false true &&"));
    ok(!parser.evaluate("false false &&"));

    ok(parser.evaluate("true true ||"));
    ok(parser.evaluate("true false ||"));
    ok(parser.evaluate("false true ||"));
    ok(!parser.evaluate("false false ||"));

    ok(parser.evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));
});

test("Verify_complex_expression_evaluation", function () {

    var evaluator = new Evaluator();

    ok(evaluator.compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
    ok(evaluator.compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));
});
