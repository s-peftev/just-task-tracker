window.jttAuth = {
    localSignOut: async function () {
        const prefixes = ["msal.", "Microsoft.Authentication.WebAssembly.Msal"];

        const auth = window.AuthenticationService;
        const instance = auth?.instance;
        if (instance) {
            instance._account = null;

            if (typeof instance.purgeState === "function") {
                instance.purgeState();
            }

            const msal = instance._msalApplication;
            if (msal && typeof msal.clearCache === "function") {
                await msal.clearCache();
            }
        }

        const keysToRemove = [];
        for (let i = 0; i < sessionStorage.length; i++) {
            const key = sessionStorage.key(i);
            if (!key) {
                continue;
            }

            if (prefixes.some((prefix) => key.startsWith(prefix))) {
                keysToRemove.push(key);
            }
        }

        keysToRemove.forEach((key) => sessionStorage.removeItem(key));
    }
};
