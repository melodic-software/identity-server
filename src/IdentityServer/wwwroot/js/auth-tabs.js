$(function () {
    const $localTab = $('#localTab');
    const $externalTab = $('#externalTab');
    const $localContent = $('#localContent');
    const $externalContent = $('#externalContent');
    const $localLoginForm = $('#localLoginForm');  // Login form reference
    const $registerForm = $('#registerForm');  // Register form reference (if applicable)

    function resetValidationState($form) {
        // Reset validation states for inputs in the form
        $form.find('.text-danger').text('');  // Clear validation error messages
        $form.find('input').removeClass('input-validation-error');  // Remove error styling
    }

    function switchTab($clickedTab) {
        if ($clickedTab.hasClass('disabled-tab') || $clickedTab.hasClass('active')) {
            return;
        }

        // Deactivate all tabs and set aria-selected to false
        $('.tab').removeClass('active').attr('aria-selected', 'false').attr('tabindex', '-1');

        // Activate the clicked tab and set aria-selected to true
        $clickedTab.addClass('active').attr('aria-selected', 'true').attr('tabindex', '0');

        const isLocalTab = $clickedTab.attr('id') === 'localTab';

        // Show/Hide content based on selected tab
        $localContent.toggleClass('active', isLocalTab)
            .attr('aria-hidden', !isLocalTab)
            .attr('tabindex', isLocalTab ? '2' : '-1');

        $externalContent.toggleClass('active', !isLocalTab)
            .attr('aria-hidden', isLocalTab)
            .attr('tabindex', isLocalTab ? '-1' : '4');

        // Trigger reflow by accessing offsetWidth (explicit reflow)
        $localContent[0].offsetWidth;
        $externalContent[0].offsetWidth;

        // Move focus to the first input in the active content section
        if (isLocalTab) {
            $localContent.find('input:visible:first').focus();
            resetValidationState($localLoginForm);  // Reset validation for local login form
        } else {
            $externalContent.find('input:visible:first').focus();
            resetValidationState($registerForm);  // Reset validation for external register form
        }
    }

    // Click event handlers for tabs
    $localTab.on('click', function () {
        switchTab($(this));
    });

    $externalTab.on('click', function () {
        switchTab($(this));
    });

    // Optionally, add keyboard navigation support (Arrow Left/Right)
    $(document).on('keydown', function (e) {
        if (e.key === 'ArrowRight') {
            switchTab($externalTab);
        } else if (e.key === 'ArrowLeft') {
            switchTab($localTab);
        }
    });
});
