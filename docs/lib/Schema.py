import json
from dataclasses import dataclass
from pathlib import Path

from json_schema_for_humans.generate import generate_schemas_doc, copy_additional_files_to_target
from json_schema_for_humans.schema.schema_importer import get_schemas_to_render
from json_schema_for_humans.template_renderer import TemplateRenderer, _minify
from json_schema_for_humans.generation_configuration import GenerationConfiguration

# noinspection PyUnresolvedReferences
from lib.Page import Page


@dataclass
class Schema(Page):

    config: GenerationConfiguration

    def __init__(self, path, env, options):
        self.sort_priority = 10
        self.in_path = path
        self.config = options
        self.env = env
        with path.open() as file:
            self.title = json.load(file).get('title', path.stem)
        self.description = "Schema for a " + self.title + " in New Horizons"
        self.out_path = Path('out/schemas/', self.in_path.relative_to(Path("content/schemas/")).with_name(self.title.replace(" ", "_").lower()).with_suffix(".html"))

    def render(self, **options):
        schemas = get_schemas_to_render(self.in_path, self.out_path, ".html")
        template_renderer = TemplateRenderer(self.config)
        template_renderer.render = lambda inter:  self.template_override(template_renderer, inter, **options)
        generate_schemas_doc(schemas, template_renderer)
        copy_additional_files_to_target(schemas, self.config)

    def template_override(self, template, intermediate_schema, **options):
        template.template.environment.filters.update(self.env.filters)
        rendered = template.template.render(schema=intermediate_schema, dumb=True, config=self.config, title=self.title + " Schema", **options)

        if template.config.minify:
            rendered = _minify(rendered, template.config.template_is_markdown, template.config.template_is_html)

        return rendered
