import os
from pathlib import Path
from shutil import copytree, rmtree

from jinja2 import Environment, select_autoescape, FileSystemLoader
from markupsafe import Markup
from json_schema_for_humans.generate import GenerationConfiguration
from markdown import Markdown

from lib.BootstrapExtension import BootstrapExtension
from lib.Schema import Schema
from lib.Page import Page

OUT_DIR = os.getenv("OUT_DIR", "/")
BASE_URL = os.getenv("BASE_URL", OUT_DIR)

if Path("out/").exists():
    rmtree("out/", ignore_errors=True)

copytree("content/static", "out")
os.makedirs("out/schemas", exist_ok=True)

env = Environment(
    loader=FileSystemLoader("content"),
    autoescape=select_autoescape(['jinja2'])
)

markdown_settings = {
    'extensions': ['extra', 'meta', BootstrapExtension()]
}

schema_settings = GenerationConfiguration(custom_template_path="content/base/schema_base.jinja2")
schema_settings.link_to_reused_ref = False

md = Markdown(**markdown_settings)

env.filters["upper_first"] = lambda x:   x[0].upper() + x[1:]
env.filters["markdown"] = lambda text:  Markup(md.convert(text))
env.filters["static"] = lambda path: str(Path(OUT_DIR, path).as_posix())

pages_paths = Path("content/pages").glob("**/*.md")
schemas_paths = Path("content/schemas").glob("**/*.json")

router = {}

env.filters['route'] = lambda title:   router.get(title.lower(), "#")
env.filters['full_url'] = lambda relative: BASE_URL + relative

pages = []
schemas = []

for page_path in pages_paths:
    new_page = Page(page_path, env, markdown_settings)
    router[new_page.title.lower()] = OUT_DIR + str(new_page.out_path.relative_to('out/'))
    pages.append(new_page)

for schema_path in schemas_paths:
    new_schema = Schema(schema_path, env, schema_settings)
    router[new_schema.title.lower()] = OUT_DIR + "schemas/" + str(new_schema.out_path.relative_to("out/schemas/"))
    schemas.append(new_schema)

content = pages + schemas

if OUT_DIR != "":
    router['home'] = OUT_DIR

pages.sort(key=lambda p: p.sort_priority, reverse=True)

def build_meta(in_path, out_path):
    meta_template = env.load_template(str(in_path.relative_to("content/")))
    with Path("out", out_path).open(mode="w+", encoding="utf-8") as file:
        file.write(meta_template.render(content=content))

print("Building Meta Files")
build_meta(Path("content/sitemap.jinja2"), Path("sitemap.xml"))
build_meta(Path("content/robots.jinja2"), Path("robots.txt"))

print ("Building Pages")
for item in content:
    print("Building:", item.in_path, "->", item.out_path)
    item.render(page=page, pages=pages, schemas=schemas)
