import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

import rehypeExternalLinks from "rehype-external-links";

import makeSchemasPlugin from "./src/plugins/schema-plugin";

const url = "https://nh.outerwildsmods.com";

const schemas = [
    "body_schema.json",
    "star_system_schema.json",
    "translation_schema.json",
    "addon_manifest_schema.json",
    "dialogue_schema.xsd",
    "text_schema.xsd",
    "shiplog_schema.xsd"
];

const ogMeta = (name, val) => ({
    tag: "meta",
    attrs: {
        property: `og:${name}`,
        content: val
    }
});

const twMeta = (name, val) => ({
    tag: "meta",
    attrs: {
        name: `twitter:${name}`,
        content: val
    }
});

// https://astro.build/config
export default defineConfig({
    site: url,
    compressHTML: true,
    markdown: {
        rehypePlugins: [rehypeExternalLinks]
    },
    integrations: [
        starlight({
            title: "New Horizons",
            description:
                "Documentation on how to use the New Horizons planet creation tool for Outer Wilds.",
            defaultLocale: "en-us",
            favicon: "/favicon.png",
            plugins: [makeSchemasPlugin({ schemas })],
            components: {
                PageSidebar: "/src/components/ConditionalPageSidebar.astro"
            },
            customCss: ["/src/styles/custom.css"],
            logo: {
                src: "/src/assets/icon.webp",
                alt: "The New Horizons Logo"
            },
            social: {
                github: "https://github.com/Outer-Wilds-New-Horizons/new-horizons",
                discord: "https://discord.gg/wusTQYbYTc"
            },
            head: [
                ogMeta("image", `${url}/og_image.webp`),
                ogMeta("image:width", "1200"),
                ogMeta("image:height", "400"),
                twMeta("card", "summary"),
                twMeta("image", `${url}/og_image.webp`),
                { tag: "meta", attrs: { name: "theme-color", content: "#ffab8a" } }
            ],
            sidebar: [
                {
                    label: "Start Here",
                    autogenerate: {
                        directory: "start-here"
                    }
                },
                {
                    label: "Guides",
                    autogenerate: {
                        directory: "guides"
                    }
                },
                {
                    label: "Schemas",
                    items: [
                        { label: "Celestial Body Schema", link: "schemas/body-schema" },
                        { label: "Star System Schema", link: "schemas/star-system-schema" },
                        { label: "Translation Schema", link: "schemas/translation-schema" },
                        { label: "Addon Manifest Schema", link: "schemas/addon-manifest-schema" },
                        { label: "Dialogue Schema", link: "schemas/dialogue-schema" },
                        { label: "Text Schema", link: "schemas/text-schema" },
                        { label: "Title Screen Schema", link: "schemas/title-screen-schema" },
                        { label: "Ship Log Schema", link: "schemas/shiplog-schema" }
                    ]
                },
                {
                    label: "Reference",
                    autogenerate: {
                        directory: "reference"
                    }
                }
            ]
        })
    ],
    // Process images with sharp: https://docs.astro.build/en/guides/assets/#using-sharp
    image: {
        service: {
            entrypoint: "astro/assets/services/sharp"
        }
    }
});
