$(function () {
    let $externalTab = $('#externalTab')

    const isExternalLogin = $externalTab ?
        $('#registerForm').data('is-external-login') === true : false;

    if (isExternalLogin && $externalTab) {
        $externalTab.trigger("click");
    }
});