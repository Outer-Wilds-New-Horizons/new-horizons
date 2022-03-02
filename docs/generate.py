import os
from dataclasses import dataclass
from pathlib import Path
from shutil import copytree, rmtree

from jinja2 import Environment, PackageLoader, select_autoescape
from json_schema_for_humans.schema.schema_importer import get_schemas_to_render
from json_schema_for_humans.generate import generate_from_filename, GenerationConfiguration, generate_schemas_doc, \
    copy_additional_files_to_target
from json_schema_for_humans.template_renderer import TemplateRenderer, _minify

env = Environment(
    loader=PackageLoader('docs_templates', 'templates'),
    autoescape=select_autoescape(['jinja2'])
)

env.filters["lowerfirst"] = lambda x:   x[0].lower() + x[1:]

home = env.get_template("base.jinja2")


@dataclass
class Page:
    in_name: str
    out_name: str
    title: str

    def render(self, **options):
        template = env.get_template(self.in_name + ".jinja2")
        options.update({'page': self})
        rendered_string = template.render(**options)
        with open("out/" + self.out_name + ".html", 'w+', encoding="utf-8") as file:
            file.write(rendered_string)


config = GenerationConfiguration(custom_template_path="docs_templates/templates/schema_base.jinja2")
config.link_to_reused_ref = False


@dataclass
class Schema(Page):

    def render(self, **options):
        schemas = get_schemas_to_render("schemas/" + self.in_name + ".json", Path("out/schemas/" + self.out_name + ".html"), ".html")
        template_renderer = TemplateRenderer(config)
        template_renderer.render = lambda inter:    template_override(template_renderer, inter, self)
        generate_schemas_doc(schemas, template_renderer)
        copy_additional_files_to_target(schemas, config)


pages = (
    Page(in_name="home", out_name="index", title="Home"),
    Schema(in_name="schema", out_name="body_schema", title="Body"),
    Schema(in_name="star_system_schema", out_name="star_system_schema", title="Star System"),
    Schema(in_name="translation_schema", out_name="translation_schema", title="Translation")
)

schemas = [s for s in pages if s.__class__.__name__ == "Schema"]


def template_override(template, intermediate_schema, inter_page):
    template.template.environment.filters["lowerfirst"] = env.filters["lowerfirst"]
    rendered = template.template.render(schema=intermediate_schema, config=config, schemas=schemas, page=inter_page, title=inter_page.title + " Schema")

    if template.config.minify:
        rendered = _minify(rendered, template.config.template_is_markdown, template.config.template_is_html)

    return rendered


print("Initializing")


if os.path.exists("out"):
    rmtree("out")

print("Copying Static")

copytree("static", "out")

os.makedirs("out/schemas")

print("Rendering Pages")

for page in pages:
    page.render(schemas=schemas)

print("Done")
