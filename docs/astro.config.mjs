import { defineConfig } from "astro/config";
import preact from "@astrojs/preact";
import react from "@astrojs/react";
import mdx from "@astrojs/mdx";

import sitemap from "@astrojs/sitemap";

const site = `https://nh.outerwildsmods.com`;

// https://astro.build/config
export default defineConfig({
    compressHTML: true,
    integrations: [
        // Enable Preact to support Preact JSX components.
        preact(),
        // Enable React for the Algolia search component.
        react(),
        mdx(),
        sitemap({
            filter: (page) =>
                page !== `${site}/404.html` && page !== `${site}/reference/audio-enum/`
        })
    ],
    site
});
