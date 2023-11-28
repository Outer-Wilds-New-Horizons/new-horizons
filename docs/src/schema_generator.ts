import { Schema, SchemaTools } from "./schema_utils";
import * as fs from "node:fs";

const addFrontmatter = (content: string, frontmatter: Record<string, boolean | string>) => {
    const entries = Object.entries(frontmatter).map(([key, value]) => `${key}: ${value}`);

    if (entries.length === 0) {
        return content;
    }

    return `---\n${entries.join("\n")}\n---\n\n${content}`;
};

export const generateSchema = (fileName: string) => {
    const schema = SchemaTools.readSchema(fileName);

    const dir = `src/content/docs/schemas/${schema.slug}`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir);
    }

    const frontMatter = {
        title: SchemaTools.getTitle(schema) as string,
        description: SchemaTools.getDescription(schema) as string,
        editUrl: false
    };

    const content = `import Schema from "/src/components/Schemas/Schema.astro";\n\n<Schema fileName="${schema.fileName}" />\n`;

    fs.writeFileSync(`${dir}/index.mdx`, addFrontmatter(content, frontMatter));
};
