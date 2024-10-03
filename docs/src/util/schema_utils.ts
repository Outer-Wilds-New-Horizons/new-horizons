/* eslint-disable no-case-declarations */
import type { JSONSchema } from "@apidevtools/json-schema-ref-parser/dist/lib/types";
import type { MarkdownHeading } from "astro";
import { readFileSync } from "fs";
import { xml2js } from "xml-js";
import type { Element } from "xml-js";

export type InternalSchema = { type: "JSON"; val: JSONSchema } | { type: "XML"; val: Element };

export interface Schema {
    internalSchema: InternalSchema;
    fileName: string;
    slug: string;
    rootSlug?: string;
    rootTitle?: string;
}

const getSchemaSlug = (rawName: string) => rawName.split(".")[0].replaceAll("_", "-");

const getElementsAsObject = (elements: Element[]) =>
    Object.fromEntries(elements.map((e) => [e.name ?? e.type ?? "??", e]));

export const SchemaTools = {
    readSchema: (file: string): Schema => {
        const contents = readFileSync(`../NewHorizons/Schemas/${file}`).toString();

        let internalSchema: InternalSchema | null = null;

        if (file.endsWith(".json")) {
            internalSchema = {
                type: "JSON",
                val: JSON.parse(contents) as JSONSchema
            };
        } else if (file.endsWith(".xsd")) {
            internalSchema = {
                type: "XML",
                val: xml2js(contents) as Element
            };
        }

        if (internalSchema) {
            return {
                fileName: file,
                slug: getSchemaSlug(file),
                rootSlug: getSchemaSlug(file),
                internalSchema
            };
        } else {
            throw Error(`Invalid Schema File: ${file}`);
        }
    },

    getTitle: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return schema.internalSchema.val.title;
            case "XML":
                const elem = schema.internalSchema.val;
                const title =
                    getElementsAsObject(elem.elements ?? [])["comment"]?.comment ??
                    elem.attributes?.["name"] ??
                    elem.name ??
                    elem.type ??
                    "??";
                return title;
        }
    },

    getDescription: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return schema.internalSchema.val.description;
            case "XML":
                const annotation = getElementsAsObject(schema.internalSchema.val.elements ?? [])[
                    "xs:annotation"
                ];
                if (annotation === undefined) {
                    return undefined;
                } else {
                    const documentation = getElementsAsObject(annotation.elements ?? [])[
                        "xs:documentation"
                    ];
                    if (documentation === undefined) {
                        return undefined;
                    } else {
                        return documentation.elements?.[0]?.text;
                    }
                }
        }
    },

    getRequired: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return schema.internalSchema.val.required ?? false;
            case "XML":
                const node = schema.internalSchema.val;
                return (node.attributes?.["minOccurs"] ?? 1).toString() !== "0";
        }
    },

    getType: (schema: Schema, stripXs?: boolean) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                const internalSchema = schema.internalSchema.val;
                if (internalSchema.$ref) {
                    return internalSchema.$ref?.split("/").at(-1);
                } else if (Array.isArray(internalSchema.type)) {
                    return internalSchema.type.join(" or ");
                } else {
                    return internalSchema.type;
                }
            case "XML":
                const node = schema.internalSchema.val;
                const type = node.attributes?.["type"] as string | undefined;
                if ((stripXs ?? true) && type?.startsWith("xs:")) {
                    return type.substring(3);
                } else {
                    return type === "empty" ? "Self-Closing" : type;
                }
        }
    },

    getAdditionalBadges: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                const internalSchema = schema.internalSchema.val;
                const badges = [];
                if (internalSchema.minimum !== undefined) {
                    badges.push(`Minimum: ${internalSchema.minimum}`);
                }
                if (internalSchema.maximum !== undefined) {
                    badges.push(`Maximum: ${internalSchema.maximum}`);
                }
                if (internalSchema.default !== undefined) {
                    badges.push(`Default: ${internalSchema.default}`);
                }
                return badges;
            case "XML":
                const node = schema.internalSchema.val;
                if (node.name === "xs:complexType") return [];
                const maxOccurs = node.attributes?.["maxOccurs"] as string | number | undefined;
                if (maxOccurs) {
                    if (maxOccurs === "unbounded") {
                        return ["Can Occur Unlimited Times"];
                    } else {
                        return [
                            `Can Occur ${maxOccurs.toString()} Time${maxOccurs === 1 ? "" : "s"}`
                        ];
                    }
                } else {
                    return ["Can Only Occur Once"];
                }
        }
    },

    getDefs: (schema: Schema): Schema[] => {
        switch (schema.internalSchema.type) {
            case "JSON":
                return Object.entries(schema.internalSchema.val.definitions ?? {}).map(
                    ([key, val]) =>
                        ({
                            fileName: schema.fileName,
                            slug: getSchemaSlug(key),
                            rootSlug: schema.slug,
                            rootTitle: SchemaTools.getTitle(schema),
                            internalSchema: {
                                type: "JSON",
                                val: { title: key, ...val }
                            }
                        }) as Schema
                );
            case "XML":
                let node = schema.internalSchema.val;
                const elements = getElementsAsObject(node.elements ?? []);
                if ("xs:schema" in elements) {
                    node = elements["xs:schema"];
                }
                const defNodes = node.elements?.filter((def) => def.name === "xs:complexType");
                if (defNodes) {
                    return defNodes
                        .filter((d) => d.attributes?.["name"] !== "empty")
                        .map(
                            (d) =>
                                ({
                                    fileName: schema.fileName,
                                    slug: getSchemaSlug(d.attributes!["name"]!.toString()),
                                    rootSlug: schema.slug,
                                    rootTitle: SchemaTools.getTitle(schema),
                                    internalSchema: {
                                        type: "XML",
                                        val: d
                                    }
                                }) as Schema
                        );
                } else {
                    return [];
                }
        }
    },

    getEnumValues: (schema: Schema) => {
        switch (schema.internalSchema.type) {
            case "JSON":
                const internalSchema = schema.internalSchema.val;
                return internalSchema.enum ?? [];
            case "XML":
                return [];
        }
    },

    getRefSlug: (schema: Schema) => {
        const type = SchemaTools.getType(schema, false);
        if (type) {
            switch (schema.internalSchema.type) {
                case "JSON":
                    return schema.internalSchema.val.$ref?.split("/").at(-1);
                case "XML":
                    if (!type.toString().startsWith("xs:") && type !== "Self-Closing") {
                        return getSchemaSlug(type as string);
                    }
            }
        }
    },

    getProps: (schema: Schema, level: number): [string, Schema][] => {
        switch (schema.internalSchema.type) {
            case "JSON":
                const internalSchema = schema.internalSchema.val;
                let requiredList: string[] = [];
                if (internalSchema.required && Array.isArray(internalSchema.required)) {
                    requiredList = internalSchema.required;
                }
                if (internalSchema.type === "object") {
                    return Object.entries(internalSchema.properties ?? {}).map((e) => [
                        e[0],
                        {
                            fileName: schema.fileName,
                            slug: `${level === 0 ? "" : `${schema.slug}-`}${getSchemaSlug(e[0])}`,
                            rootSlug: schema.rootSlug,
                            internalSchema: {
                                type: "JSON",
                                val: { title: e[0], required: requiredList.includes(e[0]), ...e[1] }
                            }
                        } as Schema
                    ]);
                } else if (internalSchema.type === "array" && internalSchema.items) {
                    return [
                        [
                            "Items",
                            {
                                fileName: schema.fileName,
                                slug: `${level === 0 ? "" : `${schema.slug}-`}items`,
                                rootSlug: schema.rootSlug,
                                internalSchema: {
                                    type: "JSON",
                                    val: { title: "Items", ...(internalSchema.items as object) }
                                }
                            } as Schema
                        ]
                    ];
                } else {
                    return [];
                }
            case "XML":
                let node = schema.internalSchema.val;
                let elements = getElementsAsObject(node.elements ?? []);
                if ("xs:schema" in elements) {
                    node = elements["xs:schema"];
                    elements = getElementsAsObject(node.elements ?? []);
                }
                if (node.name === "xs:complexType") {
                    node = elements["xs:sequence"];
                    elements = getElementsAsObject(node?.elements ?? []);
                }
                if (node.name === "xs:element") {
                    node = elements["xs:complexType"];
                    if (node === undefined) {
                        return [];
                    } else {
                        node = getElementsAsObject(node.elements ?? [])["xs:sequence"];
                        if (node === undefined) {
                            return [];
                        } else {
                            elements = getElementsAsObject(node.elements ?? []);
                        }
                    }
                }
                return (node.elements ?? [])
                    .filter((e) => e.name === "xs:element")
                    .map((e) => [
                        (e.attributes?.["name"] as string) ?? e.name ?? "??",
                        {
                            fileName: schema.fileName,
                            slug: `${schema.slug}-${getSchemaSlug(
                                e.attributes?.["name"] as string
                            )}`,
                            rootSlug: schema.rootSlug,
                            internalSchema: {
                                type: "XML",
                                val: e
                            }
                        } as Schema
                    ]);
        }
    },

    getHeaders: (schema: Schema, level?: number) => {
        let headers: MarkdownHeading[] = [];
        const props = SchemaTools.getProps(schema, level ?? 0);
        for (const prop of props) {
            headers.push({ depth: level ?? 2, slug: prop[1].slug, text: prop[0] });
            headers = headers.concat(SchemaTools.getHeaders(prop[1], (level ?? 2) + 1));
        }
        return headers;
    }
};
