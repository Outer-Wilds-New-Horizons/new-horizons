import { defineCollection, z } from "astro:content";
import { docsSchema } from "@astrojs/starlight/schema";

export const collections = {
    docs: defineCollection({
        schema: docsSchema({
            extend: z.object({
                schemaFile: z.string().optional(),
                defName: z.string().optional()
            })
        })
    })
};
