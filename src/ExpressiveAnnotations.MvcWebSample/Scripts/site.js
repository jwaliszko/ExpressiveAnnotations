$(document).ready(function () {
    $('.code').hide();
    $('.action').click(function () {
        $(this).hide();
        $(this).parent().find('.code').toggle('slow');
    });
});