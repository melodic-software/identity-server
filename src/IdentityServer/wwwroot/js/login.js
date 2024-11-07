$(function () {
    const $localLoginForm = $('#localLoginForm');
    const $cancelButton = $localLoginForm.find('button[value="cancel"]');
    const $emailInput = $('#Input_Email');
    const $deviceId = $('#hdnDeviceId');

    const tenantId = $('#authsignalTenantId').val();
    const baseUrl = $('#authsignalBaseUrl').val();
    const loginCallbackUri = $('#loginCallbackUri').val();

    const client = new window.authsignal.Authsignal({
        tenantId: tenantId,
        baseUrl: baseUrl,
    });

    $cancelButton.on("click", function (event) {
        $localLoginForm.validate().cancelSubmit = true;
    });

    $.validator.setDefaults({
        submitHandler: function (form, event) {
            const $form = $(form);
            if ($form.validate().cancelSubmit) {
                $form.validate().cancelSubmit = false;
                $form.trigger("submit");
            } else {
                $.validator.defaults.submitHandler.call(this, $form, event);
            }
        }
    });

    function initPasskeyAutofill() {
        // This is configured for a specific domain / relying party.
        // If this ever changes, this will need to be updated.
        // TODO: Make this configurable, we can probably use a hidden input with a value generated server side.
        if (window.location.hostname.endsWith("melodicsoftware.com")) {
            // This will result in a 400 IF the passkey authenticator is not enabled.
            // https://docs.authsignal.com/integrations/duende-identityserver
            client.passkey.signIn({ autofill: true }).then((token) => {
                if (token) {
                    const returnUrl = $('#Input_ReturnUrl').val();
                    const encodedReturnUrl = btoa(returnUrl);
                    window.location = `${loginCallbackUri}?returnUrl=${encodedReturnUrl}&token=${token}`;
                }
            }).catch((error) => {
                console.error("Passkey sign-in error: ", error);
            });
        } else {
            console.warn("Passkey authentication is not supported on this domain: " + window.location.hostname);
        }
    }

    initPasskeyAutofill();

    // Set the device ID (requires an Authsignal client).
    // https://docs.authsignal.com/sdks/client/web#anonymousid
    $deviceId.val(client.anonymousId);

    // Ensure the input is given focus.
    if ($emailInput.val()?.trim() !== "") {
        $emailInput.trigger("focus");
    }
});
