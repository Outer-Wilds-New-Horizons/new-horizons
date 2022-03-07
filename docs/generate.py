import os
from datetime import datetime
from pathlib import Path
from shutil import rmtree

from jinja2 import Environment, select_autoescape, FileSystemLoader

from lib.Content.CSSStaticItem import CSSStaticItem
from lib.Content.HTMLPage import HTMLPage
from lib.Content.ImageStaticItem import ImageStaticItem
from lib.Content.JSONSchema import JSONSchema
from lib.Content.JSStaticItem import JSStaticItem
from lib.Content.MDPage import MDPage
from lib.Content.MetaItem import MetaItem, MinifiedMetaItem
from lib.Content.StaticItem import StaticItem
from lib.Content.XMLSchema import XMLSchema

print("Setup")

OUT_DIR = os.getenv("OUT_DIR", "/")
BASE_URL = os.getenv("BASE_URL", "")

GEN_TIME = datetime.now().strftime("%d/%m/%Y")

env = Environment(
    loader=FileSystemLoader("content/"),
    autoescape=select_autoescape(['jinja2'])
)

env.globals['OUT_DIR'] = OUT_DIR

static_scanners = (JSStaticItem, CSSStaticItem, ImageStaticItem)
static_extensions = {}
for scanner in static_scanners:
    for ext in scanner.extensions:
        static_extensions['.' + ext] = scanner
page_scanners = (MDPage, HTMLPage)
schema_scanners = (JSONSchema, XMLSchema)
meta_files = [MinifiedMetaItem(Path('browserconfig.jinja2'), '.xml'),
              MinifiedMetaItem(Path('sitemap.jinja2'), '.xml'),
              MetaItem(Path('robots.jinja2'), '.txt')]

router = {}

env.filters.update({
    'upper_first': lambda x:   x[0].upper() + x[1:],
    'static': lambda path: str(Path(OUT_DIR, path).as_posix()),
    'route': lambda title:   router.get(title.lower(), "#"),
    'full_url': lambda relative: BASE_URL + (relative[1:] if relative[0] == "/" else relative),
    'gen_time': lambda x: GEN_TIME
})

MetaItem.initialize(env)
MinifiedMetaItem.initialize(env)

pages = []
schemas = []
static_files = []

print("Clearing Old Output")

if Path("out/").exists():
    rmtree("out/", ignore_errors=True)

static_files = [static_extensions.get(f.suffix, StaticItem)(f) for f in Path('content/static/').glob("**/*.*")]

print("Scanning For Files")

for scanner in page_scanners:
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
hide_content = meta_files + static_files

print("Generating Pages")

pages.sort(key=lambda p: p.sort_priority, reverse=True)
schemas.sort(key=lambda s: s.title)

for item in content:
    item.generate(pages=pages, schemas=schemas)

for meta_file in hide_content:
    meta_file.env = env
    meta_file.generate(content=content)

print("Done")
