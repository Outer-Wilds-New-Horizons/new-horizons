import os
from pathlib import Path
from shutil import copytree, rmtree

from jinja2 import Environment, select_autoescape, FileSystemLoader

from lib.Content.HTMLPage import HTMLPage
from lib.Content.JSONSchema import JSONSchema
from lib.Content.MDPage import MDPage
from lib.Content.MetaItem import MetaItem, MinifiedMetaItem
from lib.Content.XMLSchema import XMLSchema

print("Setup")

OUT_DIR = os.getenv("OUT_DIR", "/")
BASE_URL = os.getenv("BASE_URL", "")

env = Environment(
    loader=FileSystemLoader("content/"),
    autoescape=select_autoescape(['jinja2'])
)

scanners = (MDPage, HTMLPage)
schema_scanners = (JSONSchema, XMLSchema)
meta_files = (MinifiedMetaItem(Path('browserconfig.jinja2'), '.xml'), MinifiedMetaItem(Path('sitemap.jinja2'), '.xml'), MetaItem(Path('robots.jinja2'), '.txt'))

router = {}

env.filters.update({
    'upper_first': lambda x:   x[0].upper() + x[1:],
    'static': lambda path: str(Path(OUT_DIR, path).as_posix()),
    'route': lambda title:   router.get(title.lower(), "#"),
    'full_url': lambda relative: BASE_URL + (relative[1:] if relative[0] == "/" else relative)
})

MetaItem.initialize(env)
MinifiedMetaItem.initialize(env)

pages = []
schemas = []

print("Clearing Old Output")

if Path("out/").exists():
    rmtree("out/", ignore_errors=True)

copytree("content/static", "out")

print("Scanning For Files")

for scanner in scanners:
    new_pages = scanner.initialize(env)
    for page in new_pages:
        page.add_route(router, OUT_DIR)
    pages += new_pages

for scanner in schema_scanners:
    new_schemas = scanner.initialize(env)
    for schema in new_schemas:
        schema.add_route(router, OUT_DIR)
    schemas += new_schemas

content = pages + schemas

print("Generating Pages")

pages.sort(key=lambda p: p.sort_priority, reverse=True)
schemas.sort(key=lambda s: s.title)

for item in content:
    item.generate(pages=pages, schemas=schemas)

for meta_file in meta_files:
    meta_file.env = env
    meta_file.generate(content=content)

print("Done")
