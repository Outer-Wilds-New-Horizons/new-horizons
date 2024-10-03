import { generateSchema } from "../util/schema_generator";
import type { StarlightPlugin } from "@astrojs/starlight/types";

export interface SchemaPluginOptions {
    schemas: string[];
}

type Context = Parameters<StarlightPlugin["hooks"]["setup"]>[0];

const makePlugin = (options: SchemaPluginOptions): StarlightPlugin => {
    const setup = ({ logger }: Context) => {
        logger.debug("Generating schema docs");
        for (const schema of options.schemas) {
            logger.info(`Generating schema docs for ${schema}`);
            generateSchema(schema);
        }
        logger.debug("Finished generating schema docs");
    };

    return {
        name: "astro-plugin-schema",
        hooks: {
            setup
        }
    };
};

export default makePlugin;
