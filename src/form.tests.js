(function($, qunit, ea) {
    qunit.module("html_based_full_computation_flow");

    qunit.test("verify_sample_form_validation", function(assert) { // relies on form defined in test harness file

        ea.addMethod('Whoami', function() {
            return 'root';
        });
        ea.addMethod('ArrayContains', function(value, array) {
            return $.inArray(value, array) >= 0;
        });
        ea.addValueParser('ArrayParser', function(value, field) {
            assert.equal(field, 'ContactDetails.Letters');

            var array = value.split(',');
            return array.length === 0 ? null : array;
        });

        var validator = $('#basic_test_form').validate();

        var triggered = false;
        var element = $('#basic_test_form').find('[name="ContactDetails.Email"]');
        $(element).on('eavalid', function(e, type, valid, expr, cond) {
            triggered = true;

            assert.equal(e.currentTarget.name, 'ContactDetails.Email');
            assert.equal(type, 'assertthat');
            assert.equal(valid, false);
            assert.equal(expr, "Whoami() == 'root' && ArrayContains(LetterA, Letters) && PoliticalStabilityA == Stability.High && IsEmail(Email)");
            assert.equal(cond, undefined);
        });
        var result = element.valid(); // trigger wait for result (all is synchronous)
        assert.ok(triggered);
        assert.ok(!result);
        assert.equal(validator.errorList[0].message, 'Provided email ea{at}home.com (yes ea{at}home.com) cannot be accepted {0}{1}.');

        triggered = false;
        element = $('#basic_test_form').find('[name="ContactDetails.Letters"]');
        $(element).on('eavalid', function(e, type, valid, expr, cond) {
            triggered = true;

            assert.equal(e.currentTarget.name, 'ContactDetails.Letters');
            assert.equal(type, 'requiredif');
            assert.equal(valid, true);
            assert.equal(expr, "true");
            assert.equal(cond, undefined); // optimization on - no redundant expression evaluation is performed
        });
        result = element.valid();
        assert.ok(triggered);
        assert.ok(result);

        triggered = false;
        $(element).off('eavalid');
        ea.settings.optimize = false;
        $(element).on('eavalid', function(e, type, valid, expr, cond) {
            triggered = true;

            assert.equal(e.currentTarget.name, 'ContactDetails.Letters');
            assert.equal(type, 'requiredif');
            assert.equal(valid, true);
            assert.equal(expr, "true");
            assert.equal(cond, true);
        });
        result = element.valid();
        assert.ok(triggered);
        assert.ok(result);

        element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStabilityA"]');
        result = element.valid();
        assert.ok(result);

        ea.settings.enumsAsNumbers = false;

        element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStabilityB"]');
        result = element.valid();
        assert.ok(result);
    });

}($, QUnit, window.ea));
