(function($, qunit, ea) {
    qunit.module("html based full computation flow");

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

        var element = $('#basic_test_form').find('[name="ContactDetails.Email"]');
        var result = element.valid(); // trigger wait for result (all is synchronous)
        assert.ok(!result);
        assert.equal(validator.errorList[0].message, 'Provided email: ea@home.com (yes ea@home.com) cannot be accepted.');

        element = $('#basic_test_form').find('[name="ContactDetails.PoliticalStability"]');
        result = element.valid();
        assert.ok(!result);
        assert.equal(validator.errorList[0].message, '{0}{1}');
    });

}($, QUnit, window.ea));
