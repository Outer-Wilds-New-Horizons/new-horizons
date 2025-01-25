using System;
using System.Collections.Generic;
using System.IO;
using NewHorizons.External.Configs;
using NJsonSchema;
using NJsonSchema.Generation;

namespace SchemaExporter;

public static class SchemaExporter
{
    public static void Main(string[] args)
    {
        const string folderName = "NewHorizons/Schemas";

        Directory.CreateDirectory(folderName);
        Console.WriteLine("Schema Generator: We're winning!");
        var settings = new JsonSchemaGeneratorSettings
        {
            IgnoreObsoleteProperties = true,
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            FlattenInheritanceHierarchy = true,
            AllowReferencesWithProperties = true
        };
        var bodySchema = new Schema<PlanetConfig>("Celestial Body Schema", "Schema for a celestial body in New Horizons", $"{folderName}/body_schema", settings);
        bodySchema.Output();
        var systemSchema =
            new Schema<StarSystemConfig>("Star System Schema", "Schema for a star system in New Horizons", $"{folderName}/star_system_schema", settings);
        systemSchema.Output();
        var addonSchema = new Schema<AddonConfig>("Addon Manifest Schema",
            "Schema for an addon manifest in New Horizons", $"{folderName}/addon_manifest_schema", settings);
        addonSchema.Output();
        var translationSchema =
            new Schema<TranslationConfig>("Translation Schema", "Schema for a translation file in New Horizons", $"{folderName}/translation_schema", settings);
        translationSchema.Output();
        var titleScreenSchema = new Schema<TitleScreenConfig>("Title Screen Schema",
            "Schema for the title screen config in New Horizons", $"{folderName}/title_screen_schema", settings);
        titleScreenSchema.Output();
        Console.WriteLine("Done!");
    }

    private readonly struct Schema<T>
    {
        private readonly JsonSchemaGeneratorSettings _generatorSettings;
        private readonly string _title, _description;
        private readonly string _outFileName;

        public Schema(string schemaTitle, string schemaDescription, string fileName, JsonSchemaGeneratorSettings settings)
        {
            _title = schemaTitle;
            _description = schemaDescription;
            _outFileName = fileName;
            _generatorSettings = settings;
        }

        public void Output()
        {
            Console.WriteLine($"Outputting {_title}");
            File.WriteAllText($"{_outFileName}.json", ToString());
        }

        public override string ToString()
        {
            return GetJsonSchema().ToJson();
        }

        private JsonSchema GetJsonSchema()
        {
            var schema = JsonSchema.FromType<T>(_generatorSettings);
            schema.Title = _title;
            var schemaLinkProp = new JsonSchemaProperty
            {
                Type = JsonObjectType.String,
                Description = "The schema to validate with"
            };
            schema.Properties.Add("$schema", schemaLinkProp);
            schema.ExtensionData ??= new Dictionary<string, object>();
            schema.ExtensionData.Add("$docs", new Dictionary<string, object>
            {
                {"title", _title},
                {"description", _description}
            });

            switch (_title)
            {
                case "Celestial Body Schema":
                    schema.Definitions["NomaiTextType"].Enumeration.Remove("cairn");
                    schema.Definitions["NomaiTextType"].EnumerationNames.Remove("Cairn");
                    schema.Definitions["NomaiTextType"].Enumeration.Remove("cairnVariant");
                    schema.Definitions["NomaiTextType"].EnumerationNames.Remove("CairnVariant");
                    schema.Definitions["StellarRemnantType"].Enumeration.Remove("Pulsar");
                    schema.Definitions["StellarRemnantType"].EnumerationNames.Remove("Pulsar");
                    break;
                case "Star System Schema":
                    schema.Definitions["NomaiCoordinates"].Properties["x"].UniqueItems = true;
                    schema.Definitions["NomaiCoordinates"].Properties["y"].UniqueItems = true;
                    schema.Definitions["NomaiCoordinates"].Properties["z"].UniqueItems = true;
                    break;
            }

            if (_title is "Star System Schema" or "Celestial Body Schema")
            {
                schema.Properties["extras"] = new JsonSchemaProperty {
                    Type = JsonObjectType.Object,
                    Description = "Extra data that may be used by extension mods",
                    AllowAdditionalProperties = true,
                    AdditionalPropertiesSchema = new JsonSchema
                    {
                        Type = JsonObjectType.Object
                    }
                };
            }

            return schema;
        }
    }
}