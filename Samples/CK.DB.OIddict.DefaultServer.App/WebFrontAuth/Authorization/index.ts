function renderAuthorizeView() {
    const applicationNameElement = document.getElementById('applicationName');
    const scopeElement = document.getElementById('scope');
    const form = document.querySelector('form');

    if (applicationNameElement && scopeElement && form) {
        const queryString = window.location.search.substr(1);
        const params = new URLSearchParams(queryString);

        applicationNameElement.textContent = params.get('applicationName');
        scopeElement.textContent = params.get('scope');

        const myCookie = getCookie("AntiForgeryCookie");
        params.append("__RequestVerificationToken", myCookie);

        for (const [key, value] of params) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = key;
            input.value = value;
            form.appendChild(input);
        }
    }
}

function getCookie(cookieName) {
    const name = cookieName + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const cookies = decodedCookie.split(';');

    for (let i = 0; i < cookies.length; i++) {
        let cookie = cookies[i];
        while (cookie.charAt(0) == ' ') {
            cookie = cookie.substring(1);
        }
        if (cookie.indexOf(name) == 0) {
            return cookie.substring(name.length, cookie.length);
        }
    }
    return "";
}

renderAuthorizeView();
