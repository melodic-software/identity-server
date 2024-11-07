$(function () {
    const $hdnButton = $('#hdnButton');
    const $registerForm = $('#registerForm');
    const $formSteps = $('.form-step');
    const $progressBar = $('#registrationProgress');

    let currentStep = 0;

    const validator = $registerForm.validate({
        onkeyup: true,
        onfocusout: false,
    });

    function initializeForm() {
        showStep(currentStep);
    }

    function updateProgressBar(progress) {
        $progressBar.css('width', `${progress}%`);
        $progressBar.attr('aria-valuenow', progress);
    }

    function getTotalSteps() {
        return $formSteps.length;
    }

    function stepIsValid(step) {
        const $currentStep = $($formSteps[step]);
        let isValid = true;

        $currentStep.find('input').each(function () {
            const $input = $(this);
            if ($input.data('touched')) {
                if (!validator.element(this)) {
                    isValid = false;
                }
            }
        });

        return isValid;
    }

    function validateForm() {
        const invalidInputs = [];
        const invalidSteps = {};

        $registerForm.find('input').each(function (index, input) {
            if (input?.name) {
                if (!validator.element(input)) {
                    invalidInputs.push(input);
                    const stepIndex = $(input).closest('.form-step').index();
                    if (!invalidSteps[stepIndex]) {
                        invalidSteps[stepIndex] = [];
                    }
                    invalidSteps[stepIndex].push(input);
                }
            }
        });

        return {
            isValid: invalidInputs.length === 0,
            invalidInputs: invalidInputs,
            invalidSteps: invalidSteps
        };
    }

    function showStep(step) {
        $formSteps.removeClass('active').attr('aria-hidden', 'true');
        $($formSteps[step]).addClass('active').attr('aria-hidden', 'false');
        updateProgressBar(((step + 1) / getTotalSteps()) * 100);
        $($formSteps[step]).find('input:visible:first').trigger("focus");
        updateNextButtonState();
    }

    function moveToNextStep() {
        if (stepIsValid(currentStep) && currentStep < getTotalSteps() - 1) {
            currentStep++;
            showStep(currentStep);
        } else {
            $($formSteps[currentStep]).find('input').each(function () {
                $(this).data('touched', true);
                $(this).valid();
            });
        }

        updateNextButtonState();
    }

    function moveToPreviousStep() {
        if (currentStep > 0) {
            currentStep--;
            showStep(currentStep);
        }
    }

    function updateNextButtonState() {
        $('.next-step').prop('disabled', !stepIsValid(currentStep));
    }

    $formSteps.each(function () {
        const $step = $(this);

        $step.find('input').on('blur input', function () {
            const $input = $(this);
            $input.data('touched', true);
            $input.valid();
            updateNextButtonState();
        });

        $step.find('input').on('keydown', function (e) {
            if (e.key === 'Tab' && !e.shiftKey) {
                const inputs = $step.find('input');
                const currentIndex = inputs.index(this);
                if (currentIndex === inputs.length - 1 && stepIsValid(currentStep)) {
                    e.preventDefault();
                    moveToNextStep();
                }
            } else if (e.shiftKey && e.key === 'Tab') {
                const inputs = $step.find('input');
                const currentIndex = inputs.index(this);
                if (currentIndex === 0) {
                    e.preventDefault();
                    moveToPreviousStep();
                } else {
                    const previousInput = inputs.eq(currentIndex - 1);
                    previousInput.trigger("focus");
                    e.preventDefault();
                }
            } else if (e.key === 'Enter') {
                const inputs = $step.find('input');
                const currentIndex = inputs.index(this);
                if (currentIndex === inputs.length - 1) {
                    e.preventDefault();
                    if (currentStep === getTotalSteps() - 1) {
                        $registerForm.trigger("submit");
                    } else if (stepIsValid(currentStep)) {
                        moveToNextStep();
                    }
                }
            }
        });

        $step.find('input').on('input', updateNextButtonState);
    });

    $formSteps.last().find('input').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            // Check if the register button is enabled (i.e., the form is valid)
            if (!$('#btnRegister').prop('disabled')) {
                $('#btnRegister').trigger('click');
            }
        }
    });

    $('.next-step').on('click', moveToNextStep);
    $('.prev-step').on('click', moveToPreviousStep);

    $(document).on('keydown', function (e) {
        if (!$(':focus').is('input')) {
            if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
                e.preventDefault();
                moveToNextStep();
            } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
                e.preventDefault();
                moveToPreviousStep();
            }
        }
    });

    $formSteps.last().find('input').on('keydown', function (e) {
        if (e.key === 'Tab' && !e.shiftKey) {
            const inputs = $formSteps.last().find('input');
            const currentIndex = inputs.index(this);
            if (currentIndex === inputs.length - 1) {
                e.preventDefault();
                $('#btnRegister').trigger("focus");
            }
        }
    });

    $('#btnRegister').on('keydown', function (e) {
        if (e.shiftKey && e.key === 'Tab') {
            e.preventDefault();
            $formSteps.last().find('input:last').trigger("focus");
        }
    });

    $('#btnRegister').on('click', function (e) {
        e.preventDefault();
        $hdnButton.val('register');
        $registerForm.submit();
    });

    $registerForm.on('submit', function (e) {
        e.preventDefault();

        const formValidationResult = validateForm();

        if (formValidationResult.isValid) {
            if (currentStep === getTotalSteps() - 1) {
                this.submit();
            } else {
                moveToNextStep();
            }
        } else {
            // Navigate to the first invalid step
            const firstInvalidStepIndex = Object.keys(formValidationResult.invalidSteps)[0];
            showStep(parseInt(firstInvalidStepIndex));
            // Focus on the first invalid input in this step
            $(formValidationResult.invalidSteps[firstInvalidStepIndex][0]).trigger("focus");
        }
    });

    $registerForm.on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            if (currentStep < getTotalSteps() - 1) {
                moveToNextStep();
            } else {
                $registerForm.trigger("submit");
            }
        }
    });

    $('#btnCancel').on('click', function (e) {
        e.preventDefault();

        $hdnButton.val('cancel');

        // Disable form validation on cancel.
        $registerForm.off('submit');

        // Submit the form directly.
        $registerForm.trigger("submit");
    });

    initializeForm();
});