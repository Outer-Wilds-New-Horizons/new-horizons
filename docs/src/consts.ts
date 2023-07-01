export const SITE = {
    title: 'New Horizons',
    description: 'Documentation on how to use the New Horizons planet creation tool for Outer Wilds.',
    defaultLanguage: 'en-us',
} as const;

export const OPEN_GRAPH = {
    image: {
        src: 'https://nh.outerwildsmods.com/public/home_logo.webp',
        alt:
            'The New Horizons Logo'
    }
};

export const KNOWN_LANGUAGES = {
    English: 'en',
} as const;
export const KNOWN_LANGUAGE_CODES = Object.values(KNOWN_LANGUAGES);

export const GITHUB_EDIT_URL = `https://github.com/Outer-Wilds-New-Horizons/new-horizons/tree/main/docs`;

export const COMMUNITY_INVITE_URL = `https://discord.gg/wusTQYbYTc`;

// See "Algolia" section of the README for more information.
export const ALGOLIA = {
    indexName: 'XXXXXXXXXX',
    appId: 'XXXXXXXXXX',
    apiKey: 'XXXXXXXXXX',
};

export type Sidebar = Record<
    (typeof KNOWN_LANGUAGE_CODES)[number],
    Record<string, { text: string; link: string }[]>
>;
export const SIDEBAR: Sidebar = {
    en: {
        'Intro': [
            { text: 'Introduction', link: 'en/introduction' },
        ],
        'Guides': [
            { text: 'Getting Started', link: 'en/getting-started' },
            { text: 'Creating An Addon', link: 'en/creating-addons' },
            { text: 'Updating Existing Planets', link: 'en/updating-planets' },
            { text: 'Creating New Planets', link: 'en/planet-generation' },
            { text: 'Detailing Planets', link: 'en/details' },
            { text: 'Custom Star Systems', link: 'en/star-systems' },
            { text: 'Adding Translations', link: 'en/translation' },
            { text: 'Understanding XML', link: 'en/xml' },
            { text: 'Ship Log', link: 'en/ship-log' },
            { text: 'Dialogue', link: 'en/dialogue' },
            { text: 'API', link: 'en/api' },
            { text: 'Extending Configs', link: 'en/extending-configs' },
            { text: 'Publishing Your Addon', link: 'en/publishing' },
        ],
        "Schemas": [
            { text: "Celestial Body Schema", link: "schemas/body-schema" }
        ]
    },
};
