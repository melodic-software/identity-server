window.addEventListener("load", () => {
    const qrCodeData = document.getElementById("qrCodeData");
    const keyUri = qrCodeData.getAttribute('data-url');
    const qrCode = document.getElementById("qrCode");

    new QRCode(qrCode,
        {
            text: keyUri,
            width: 150,
            height: 150
        });
});