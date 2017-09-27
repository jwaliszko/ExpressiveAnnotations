/// <reference path="../extern/qunit-1.18.0.js" />
/// <reference path="../extern/jquery-1.8.2.js" />
/// <reference path="../extern/jquery.validate-1.10.0.js" />
/// <reference path="../extern/jquery.validate.unobtrusive-3.1.1.js" />

/// <reference path="./expressive.annotations.validate.js" />
/// <reference path="./form.tests.html" />

(function($, qunit, ea) {

    qunit.begin(function() {
        ea.addMethod('Whoami', function() {
            return 'root';
        });
        ea.addMethod('ArrayContains', function(value, array) {
            qunit.assert.equal(Object.prototype.toString.call(array), '[object Array]');

            return $.inArray(value, array) >= 0;
        });
        ea.addValueParser('ArrayParser', function(value, field) {
            qunit.assert.equal(field, 'ContactDetails.Letters');

            var array = value.split(',');
            return array.length === 0 ? null : array;
        });
    });

    qunit.module("html_based_full_computation_flow");

    qunit.test("verify_sample_field_validation_with_default_options", function(assert) { // relies on form defined in test harness file

        var validator = $('#basic_test_form').validate();

        var triggered = false;
        var element = $('#basic_test_form').find('[name="ContactDetails.Email"]');
        $(element).on('eavalid', function(e, type, valid, expr) {
            triggered = true;

            assert.equal(arguments.length, 4);

            assert.equal(e.currentTarget.name, 'ContactDetails.Email');
            assert.equal(type, 'assertthat');
            assert.equal(valid, false);
            assert.equal(expr, "Whoami() == 'root' && ArrayContains(LetterA, Letters) && PoliticalStabilityA == Stability.High && IsEmail(Email)");
        });
        var result = element.valid(); // trigger wait for result (all is synchronous)
        assert.ok(triggered);
        assert.ok(!result);
        assert.equal(validator.errorList[0].message, 'Provided email ea{at}home.com (yes ea{at}home.com) cannot be accepted {0}{1}.');
    });

    qunit.test("eavalid_event_optimization_is_respected", function(assert) { // relies on form defined in test harness file

        ea.settings.optimize = true; // default
        var triggered = 0;
        var element = $('#basic_test_form').find('[name="ContactDetails.Letters"]');
        $(element).on('eavalid', function(e, type, valid, expr, cond, idx) {
            ++triggered;

            assert.equal(arguments.length, 6);

            assert.equal(e.currentTarget.name, 'ContactDetails.Letters');
            assert.equal(type, 'requiredif');
            assert.equal(valid, true);
            assert.equal(expr, triggered === 1 ? "true" : "false");
            assert.equal(cond, undefined); // optimization on - because value always provided, no redundant expression evaluation is ever performed for requiredif
            assert.equal(idx, triggered - 1); // requiredif then requiredifa
        });
        var result = element.valid();
        $(element).off('eavalid');
        assert.equal(triggered, 2);
        assert.ok(result);

        ea.settings.optimize = false;
        triggered = 0;
        $(element).on('eavalid', function(e, type, valid, expr, cond, idx) {
            ++triggered;

            assert.equal(arguments.length, 6);

            assert.equal(e.currentTarget.name, 'ContactDetails.Letters');
            assert.equal(type, 'requiredif');
            assert.equal(valid, true);
            assert.equal(expr, triggered === 1 ? "true" : "false");
            assert.equal(cond, triggered === 1 ? true : false); // optimization off - despite the value being always provided, redundant expression evaluation is still performed for requiredif
            assert.equal(idx, triggered - 1);
        });
        result = element.valid();
        $(element).off('eavalid');
        assert.equal(triggered, 2);
        assert.ok(result);
    });

    qunit.test("enums_handling_behaves_as_expected", function(assert) {

        ea.settings.enumsAsNumbers = true; // default
        var element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStabilityA"]');
        var result = element.valid();
        assert.ok(result);

        ea.settings.enumsAsNumbers = false;
        element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStabilityB"]');
        result = element.valid();
        assert.ok(result);
    });

    qunit.test("dependency_trigger_initiates_dependent_field_validation_when_not_ignored", function(assert) {

        ea.settings.lazyDependencyValidation = false;

        var validator = $('#basic_test_form').validate();
        var element = $('#basic_test_form').find('[name="ContactDetails.Letters"]');

        var triggered = false;
        var dependant = $('#basic_test_form').find('[name="Hidden"]');
        $(dependant).on('eavalid', function() {
            triggered = true;
        });

        validator.settings.ignore = ':hidden'; // ignore ':hidden' fields when validating
        element.trigger('change');
        assert.ok(!triggered);

        validator.settings.ignore = ''; // do not ignore any fields when validating
        element.trigger('change');
        assert.ok(triggered);

        ea.settings.apply({ dependencyTriggers: '' }); // turn off dependency validation (apply for dependency triggers to be re-bound)
        triggered = false;
        element.trigger('change');
        assert.ok(!triggered);
    });

}($, QUnit, window.ea));
