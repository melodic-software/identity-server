$(function () {
    let seconds = $('#redirectDelayInSeconds').val();
    const countdownElem = document.getElementById('countdown');

    if (countdownElem) {
        let interval = setInterval(function () {
            seconds--;
            countdownElem.textContent = seconds;
            if (seconds <= 0) {
                clearInterval(interval);
                document.getElementById('redirectForm').submit();
            }
        }, 1000);
    }

    document.getElementById('resendConfirmationLink')?.addEventListener('click', function () {
        document.getElementById('confirmEmailForm').submit();
    });

    document.getElementById('redirectLink')?.addEventListener('click', function () {
        document.getElementById('redirectForm').submit();
    });
});
