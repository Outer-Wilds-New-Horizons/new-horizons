export const SITE = {
    title: "New Horizons",
    description:
        "Documentation on how to use the New Horizons planet creation tool for Outer Wilds.",
    defaultLanguage: "en-us"
} as const;

export const OPEN_GRAPH = {
    image: {
        src: "https://nh.outerwildsmods.com/public/home_logo.webp",
        alt: "The New Horizons Logo"
    }
};

export const KNOWN_LANGUAGES = {
    English: "en"
} as const;
export const KNOWN_LANGUAGE_CODES = Object.values(KNOWN_LANGUAGES);

export const GITHUB_EDIT_URL = `https://github.com/Outer-Wilds-New-Horizons/new-horizons/tree/main/docs`;

export const COMMUNITY_INVITE_URL = `https://discord.gg/wusTQYbYTc`;

// See "Algolia" section of the README for more information.
export const ALGOLIA = {
    indexName: "XXXXXXXXXX",
    appId: "XXXXXXXXXX",
    apiKey: "XXXXXXXXXX"
};

export type Sidebar = Record<
    (typeof KNOWN_LANGUAGE_CODES)[number],
    Record<string, { text: string; link: string }[]>
>;
export const SIDEBAR: Sidebar = {
    en: {
        Intro: [{ text: "Introduction", link: "" }],
        Guides: [
            { text: "Getting Started", link: "getting-started" },
            { text: "Creating An Addon", link: "creating-addons" },
            { text: "Updating Existing Planets", link: "updating-planets" },
            { text: "Creating New Planets", link: "planet-generation" },
            { text: "Detailing Planets", link: "details" },
            { text: "Custom Star Systems", link: "star-systems" },
            { text: "Adding Translations", link: "translation" },
            { text: "Understanding XML", link: "xml" },
            { text: "Ship Log", link: "ship-log" },
            { text: "Dialogue", link: "dialogue" },
            { text: "API", link: "api" },
            { text: "Extending Configs", link: "extending-configs" },
            { text: "Publishing Your Addon", link: "publishing" }
        ],
        Schemas: [
            { text: "Celestial Body Schema", link: "schemas/body-schema" },
            { text: "Star System Schema", link: "schemas/star-system-schema" },
            { text: "Translation Schema", link: "schemas/translation-schema" },
            { text: "Addon Manifest Schema", link: "schemas/addon-manifest-schema" },
            { text: "Dialogue Schema", link: "schemas/dialogue-schema" },
            { text: "Text Schema", link: "schemas/text-schema" },
            { text: "Ship Log Schema", link: "schemas/shiplog-schema" }
        ],
        Reference: [
            { text: "Bramble Colors", link: "reference/bramble-colors" },
            { text: "AudioClip Values", link: "reference/audio-enum" }
        ]
    }
};
