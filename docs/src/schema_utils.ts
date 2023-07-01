/* eslint-disable no-case-declarations */
import type { JSONSchema } from "@apidevtools/json-schema-ref-parser/dist/lib/types";
import type { MarkdownHeading } from "astro";
import { readFileSync } from "fs";
import { ElementCompact, xml2js } from "xml-js";

export type InternalSchema = { type: "JSON", val: JSONSchema } | { type: "XML", val: ElementCompact };

export interface Schema {
    internalSchema: InternalSchema;
    fileName: string;
    slug: string;
}

const getSchemaSlug = (rawName: string) => rawName.split(".")[0].replaceAll("_", "-");

export const SchemaTools = {
    readSchema: (file: string): Schema => {
        const contents = readFileSync(`../NewHorizons/Schemas/${file}`).toString();

        let internalSchema: InternalSchema | null = null;

        if (file.endsWith(".json")) {
            internalSchema = {
                type: "JSON",
                val: JSON.parse(contents) as JSONSchema
            }
        } else if (file.endsWith(".xsd")) {
            internalSchema = {
                type: "XML",
                val: xml2js(contents, { compact: true }) as ElementCompact
            }
        }

        if (internalSchema) {
            return {
                fileName: file,
                slug: getSchemaSlug(file),
                internalSchema
            }
        } else {
            throw Error(`Invalid Schema File: ${file}`);
        }
    },

    getTitle: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return schema.internalSchema.val.title;
            case "XML":
                return `${schema.internalSchema.val._comment ?? "??"} Schema`;
        }
    },

    getProps: (schema: Schema): [string, Schema][] => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return Object.entries(schema.internalSchema.val).map(e => [
                    e[0],
                    {
                        fileName: schema.fileName,
                        slug: schema.slug,
                        internalSchema: {
                            type: "JSON",
                            val: e[1]
                        }
                    }
                ]);
            case "XML":
                let node = schema.internalSchema.val;
                if ("xs:schema" in node) {
                    node = node["xs:schema"];
                }
                console.debug(node);
                return Object.entries(node).map(e => [
                    e[0],
                    {
                        fileName: schema.fileName,
                        slug: schema.slug,
                        internalSchema: {
                            type: "XML",
                            val: e[1]
                        }
                    }
                ]);
        }
    }
}



export const getHeaders = (schema: JSONSchema) => {
    if (schema.type === "object") {
        return Object.keys(schema.properties as Record<string, unknown>).map((h) => ({
            depth: 2,
            slug: h,
            text: h
        } as MarkdownHeading))
    } else {
        return [];
    }
};
