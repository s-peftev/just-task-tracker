window.jttTheme = {
    storageKey: "jtt.theme",
    dark: "dark",
    light: "light",

    getStoredTheme() {
        const stored = localStorage.getItem(this.storageKey);
        return stored === this.light ? this.light : this.dark;
    },

    applyTheme(theme) {
        const root = document.documentElement;
        const isLight = theme === this.light;

        root.classList.toggle("light", isLight);
        root.dataset.theme = isLight ? this.light : this.dark;
    },

    setTheme(theme) {
        const normalized = theme === this.light ? this.light : this.dark;
        localStorage.setItem(this.storageKey, normalized);
        this.applyTheme(normalized);
        return normalized;
    },

    init() {
        this.applyTheme(this.getStoredTheme());
    }
};
