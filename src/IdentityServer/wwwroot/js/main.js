$(function () {
    function toggleFloatLabel() {
        $('.form-control').each(function () {
            const $input = $(this);
            const $label = $input.siblings('.form-label');
            const hasFloatLabelClass = $label.hasClass('float-label');

            if (($input.val() || $input.is(':focus')) && !hasFloatLabelClass) {
                $label.addClass('float-label');
            } else if (!$input.val() && !$input.is(':focus') && hasFloatLabelClass) {
                $label.removeClass('float-label');
            }
        });
    }

    // Bind events to run toggleFloatLabel only when needed.
    $('.float-labels .form-control').on('focus blur input', toggleFloatLabel);

    // Ensure toggleFloatLabel runs when the page is shown from the cache (back/forward navigation).
    $(window).on('pageshow', function () {
        toggleFloatLabel();
    });

    // Run on initial page load.
    toggleFloatLabel();
});