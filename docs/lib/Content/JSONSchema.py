import json
from pathlib import Path

from json_schema_for_humans.generate import generate_schemas_doc
from json_schema_for_humans.generation_configuration import GenerationConfiguration
from json_schema_for_humans.schema.schema_to_render import SchemaToRender
from json_schema_for_humans.template_renderer import TemplateRenderer

from lib.Content.AbstractSchemaItem import AbstractSchemaItem

SCHEMA_SETTINGS = GenerationConfiguration()
SCHEMA_SETTINGS.link_to_reused_ref = False
SCHEMA_SETTINGS.minify = False


class JSONSchema(AbstractSchemaItem):
    extensions = ('json',)

    def load_metadata(self):
        self.sort_priority = 10
        with self.in_path.open(mode='r', encoding='utf-8') as file:
            self.title = json.load(file).get('title', self.in_path.stem)
        self.description = "Schema for a " + self.title + " in New Horizons"
        super(JSONSchema, self).load_metadata()

    def render(self, **context):
        context.update({
            'page': self
        })
        dumb_renderer = TemplateRenderer(SCHEMA_SETTINGS)
        self.env.filters.update(dumb_renderer.template.environment.filters)
        self.env.tests.update(dumb_renderer.template.environment.tests)
        self.env.globals.update(dumb_renderer.template.environment.globals)
        schemas = [SchemaToRender(self.in_path, None, None)]
        schema_template = self.env.get_template(str(Path('base/schema/json/schema_base.jinja2').as_posix()))
        template_renderer = TemplateRenderer(SCHEMA_SETTINGS, schema_template)
        template_renderer.render = lambda inter: self.template_override(template_renderer, inter, **context)
        rendered = generate_schemas_doc(schemas, template_renderer)
        return rendered[str(self.in_path.name)]

    def template_override(self, template: TemplateRenderer, intermediate_schema, **context):
        template.template.environment.loader = self.env.loader
        rendered = template.template.render(schema=intermediate_schema, config=SCHEMA_SETTINGS,
                                            title=self.title + " Schema", **context)
        return rendered
