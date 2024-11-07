$(function () {
    // Check if the form is for external login
    const isExternalLogin = $('#registerForm').data('is-external-login');

    if (isExternalLogin) {
        return;
    }

    // Get password requirements from hidden fields
    const requiredLength = parseInt($('#requiredLength').val());
    const requiredUniqueChars = parseInt($('#requiredUniqueChars').val());
    const requireNonAlphanumeric = $('#requireNonAlphanumeric').val() === 'true';
    const requireLowercase = $('#requireLowercase').val() === 'true';
    const requireUppercase = $('#requireUppercase').val() === 'true';
    const requireDigit = $('#requireDigit').val() === 'true';

    // Get form elements
    const $passwordInput = $('input[name="Input.Password"]');
    const $confirmPasswordInput = $('input[name="Input.ConfirmPassword"]');
    const $passwordStrengthMeter = $('#password-strength-meter');
    const $passwordFeedback = $('#password-feedback');

    // Initialize zxcvbn-ts with options for English and common packages
    const options = {
        translations: zxcvbnts['language-en'].translations,
        graphs: zxcvbnts['language-common'].adjacencyGraphs,
        dictionary: {
            ...zxcvbnts['language-common'].dictionary,
            ...zxcvbnts['language-en'].dictionary,
        },
    };

    zxcvbnts.core.zxcvbnOptions.setOptions(options);

    function updatePasswordStrength() {
        const password = $passwordInput.val();
        if (password.length === 0) {
            $passwordStrengthMeter.css('width', '0%').removeClass('weak fair good strong');
            $passwordFeedback.html(''); // Clear feedback
            return;
        }

        // Analyze password strength using zxcvbn-ts
        const result = zxcvbnts.core.zxcvbn(password);
        const score = result.score;  // Score is 0 (weakest) to 4 (strongest)

        // Update strength meter UI
        const strength = (score + 1) * 20; // Scale the score to a percentage
        $passwordStrengthMeter.css('width', `${strength}%`);

        // Add classes based on strength
        if (score === 0) {
            $passwordStrengthMeter.removeClass('fair good strong').addClass('weak');
        } else if (score === 1) {
            $passwordStrengthMeter.removeClass('weak good strong').addClass('fair');
        } else if (score === 2 || score === 3) {
            $passwordStrengthMeter.removeClass('weak fair strong').addClass('good');
        } else {
            $passwordStrengthMeter.removeClass('weak fair good').addClass('strong');
        }
    }

    function updatePasswordFeedback() {
        const password = $passwordInput.val();
        const result = zxcvbnts.core.zxcvbn(password);

        const feedback = result.feedback.suggestions;
        const warning = result.feedback.warning;

        if (feedback.length > 0 || warning.length > 0) {
            let feedbackHtml = '';

            // Include the warning if present
            if (warning.length > 0) {
                feedbackHtml += `<strong>Warning:</strong> ${warning} `;
            }

            // Display feedback suggestions
            feedbackHtml += feedback.join(' ');
            $passwordFeedback.html(feedbackHtml); // Show feedback to the user
        } else {
            $passwordFeedback.html(''); // Clear feedback if password is strong enough
        }
    }

    // Custom password validation method
    $.validator.addMethod('customPassword', function (value, element) {
        let valid = true;
        let messages = [];
        if (value.length < requiredLength) {
            valid = false;
            messages.push(`Password must be at least ${requiredLength} characters long.`);
        }
        if ((new Set(value)).size < requiredUniqueChars) {
            valid = false;
            messages.push(`Password must contain at least ${requiredUniqueChars} unique characters.`);
        }
        if (requireNonAlphanumeric && !/[!@#$%^&*(),.?":{}|<>]/.test(value)) {
            valid = false;
            messages.push('Password must contain at least one non-alphanumeric character.');
        }
        if (requireLowercase && !/[a-z]/.test(value)) {
            valid = false;
            messages.push('Password must contain at least one lowercase letter.');
        }
        if (requireUppercase && !/[A-Z]/.test(value)) {
            valid = false;
            messages.push('Password must contain at least one uppercase letter.');
        }

        // Use concise character class syntax \d instead
        if (requireDigit && !/\d/.test(value)) {
            valid = false;
            messages.push('Password must contain at least one digit.');
        }

        $.validator.messages.customPassword = messages.join(' ');

        return this.optional(element) || valid;
    });

    // Add validation rules to the password input
    $passwordInput.rules('add', {
        customPassword: true,
        required: true,
        messages: {
            required: 'Please enter a password.'
        }
    });

    // Add validation rules to the confirm password input
    $confirmPasswordInput.rules('add', {
        equalTo: '#Input_Password',
        required: true,
        messages: {
            equalTo: 'Passwords do not match.',
            required: 'Please confirm your password.'
        }
    });

    // Event listener for password input to update strength meter and feedback
    $passwordInput.on('input', function () {
        updatePasswordStrength();
        updatePasswordFeedback();
    });

    updatePasswordStrength();
    updatePasswordFeedback();
});