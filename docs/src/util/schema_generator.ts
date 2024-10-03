import { SchemaTools, type Schema } from "./schema_utils";
import * as fs from "node:fs";

const addFrontmatter = (
    content: string,
    frontmatter: Record<string, boolean | string | object>
) => {
    const entries = Object.entries(frontmatter).map(([key, value]) => `${key}: ${value}`);

    if (entries.length === 0) {
        return content;
    }

    return `---\n${entries.join("\n")}\n---\n\n${content}`;
};

const generateDef = (def: Schema) => {
    const title = SchemaTools.getTitle(def) as string;
    const dir = `src/content/docs/schemas/${def.rootSlug!}/defs/${title}`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }

    let description = SchemaTools.getDescription(def) as string | undefined;

    if (description === undefined || (description as string).trim() === "") {
        description = `Definition of ${title}`;
    }

    const frontMatter = {
        title,
        description,
        editUrl: false,
        schemaFile: def.fileName,
        defName: title
    };

    const content = `import SchemaDef from "/src/components/Schemas/SchemaDef.astro";\n\n<SchemaDef fileName="${def.fileName}" def="${title}" />\n`;

    fs.writeFileSync(`${dir}/index.mdx`, addFrontmatter(content, frontMatter));
};

const generateDefList = (schema: Schema) => {
    const dir = `src/content/docs/schemas/${schema.slug}/defs`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }

    const frontMatter = {
        title: `${SchemaTools.getTitle(schema)} definitions`,
        description: "List of all definitions in the ${SchemaTools.getTitle(schema)} schema",
        editUrl: false,
        schemaFile: schema.fileName
    };

    const content = `import DefinitionList from "/src/components/Schemas/DefinitionList.astro";\n\n<DefinitionList fileName="${schema.fileName}" />\n`;

    fs.writeFileSync(`${dir}/index.mdx`, addFrontmatter(content, frontMatter));
};

export const generateSchema = (fileName: string) => {
    const schema = SchemaTools.readSchema(fileName);

    const dir = `src/content/docs/schemas/${schema.slug}`;

    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }

    const frontMatter = {
        title: SchemaTools.getTitle(schema) as string,
        description: SchemaTools.getDescription(schema) as string,
        editUrl: false,
        schemaFile: schema.fileName
    };

    const content = `import Schema from "/src/components/Schemas/Schema.astro";\n\n<Schema fileName="${schema.fileName}" />\n`;

    fs.writeFileSync(`${dir}/index.mdx`, addFrontmatter(content, frontMatter));

    generateDefList(schema);

    for (const def of SchemaTools.getDefs(schema)) {
        generateDef(def);
    }
};
