using System;
using System.IO;
using System.Linq;
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
        Console.WriteLine("Outputting Body Schema");
        var bodySchema = new Schema<PlanetConfig>("Celestial Body Schema", $"{folderName}/body_schema", settings);
        bodySchema.Output();
        Console.WriteLine("Outputting Star System Schema");
        var systemSchema =
            new Schema<StarSystemConfig>("Star System Schema", $"{folderName}/star_system_schema", settings);
        systemSchema.Output();
        Console.WriteLine("Outputting Translation Schema");
        var translationSchema =
            new Schema<TranslationConfig>("Translation Schema", $"{folderName}/translation_schema", settings);
        translationSchema.Output();
        Console.WriteLine("Done!");
    }

    private readonly struct Schema<T>
    {
        private readonly JsonSchemaGeneratorSettings _generatorSettings;
        private readonly string _title;
        private readonly string _outFileName;

        public Schema(string schemaTitle, string fileName, JsonSchemaGeneratorSettings settings)
        {
            _title = schemaTitle;
            _outFileName = fileName;
            _generatorSettings = settings;
        }

        public void Output()
        {
            File.WriteAllText($"{_outFileName}.json", ToString());
        }

        public override string ToString()
        {
            return GetJsonSchema().ToJson();
        }

        private static void FixOneOf(JsonSchema schema)
        {
            if (schema.OneOf.Count != 0)
            {
                schema.Reference = schema.OneOf.First();
                schema.OneOf.Clear();
                foreach (var property in schema.Reference.Properties.Values) FixOneOf(property);
            }
            foreach (var property in schema.Properties.Values) FixOneOf(property);
        }

        private JsonSchema GetJsonSchema()
        {
            var schema = JsonSchema.FromType<T>(_generatorSettings);
            schema.Title = _title;
            // FixOneOf(schema);
            return schema;
        }
    }
}