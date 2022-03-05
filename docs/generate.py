import os
from pathlib import Path
from shutil import copytree, rmtree

from jinja2 import Environment, select_autoescape, FileSystemLoader

from lib.Content.HTMLPage import HTMLPage
from lib.Content.JSONSchema import JSONSchema
from lib.Content.MDPage import MDPage

OUT_DIR = os.getenv("OUT_DIR", "/")
BASE_URL = os.getenv("BASE_URL", "")

env = Environment(
    loader=FileSystemLoader("content/"),
    autoescape=select_autoescape(['jinja2'])
)

scanners = (MDPage, HTMLPage)
schema_scanners = (JSONSchema,)

router = {}

env.filters.update({
    'upper_first': lambda x:   x[0].upper() + x[1:],
    'static': lambda path: str(Path(OUT_DIR, path).as_posix()),
    'route': lambda title:   router.get(title.lower(), "#"),
    'full_url': lambda relative: BASE_URL + (relative[1:] if relative[0] == "/" else relative)
})

pages = []
schemas = []

if Path("out/").exists():
    rmtree("out/", ignore_errors=True)

copytree("content/static", "out")

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

pages.sort(key=lambda p: p.sort_priority, reverse=True)
schemas.sort(key=lambda s: s.title)

for page in pages:
    page.generate(pages=pages, schemas=schemas)

for schema in schemas:
    schema.generate(pages=pages, schemas=schemas)

#

#
#
# markdown_settings = {
#     'extensions': ['extra', 'toc', 'meta', BootstrapExtension()]
# }
#
#
# minify_settings = {
#     'remove_empty_space': True,
#     'keep_pre': True,
#     'remove_optional_attribute_quotes': False
# }
#
# env.minify_settings = minify_settings
#
# md = Markdown(**markdown_settings)
#
#
# pages_paths = Path("content/pages").glob("**/*.md")
# schemas_paths = Path("content/schemas").glob("**/*.json")
#
# router = {}
#

#
# pages = []
# schemas = []
#
# for page_path in pages_paths:
#     new_page = Page(page_path, env, markdown_settings)
#     pages.append(new_page)
#
# for schema_path in schemas_paths:
#     new_schema = Schema(schema_path, env, schema_settings)
#     router[new_schema.title.lower()] = OUT_DIR + "schemas/" + str(new_schema.out_path.relative_to("out/schemas/"))
#     schemas.append(new_schema)
#
# content = pages + schemas
#
# if OUT_DIR != "":
#     router['home'] = OUT_DIR
#
# pages.sort(key=lambda p: p.sort_priority, reverse=True)
# schemas.sort(key=lambda s: s.title)
#
#
# def log_build(in_path, out_path):
#     print("Building:", str(in_path), "->", str(out_path))
#
#
# def build_meta(in_path, out_path, do_minify=False):
#     log_build(in_path, out_path)
#     meta_template = env.get_template(str(in_path.relative_to("content/")))
#     with Path("out/", out_path).open(mode="w+", encoding="utf-8") as file:
#         out = meta_template.render(content=content)
#         if do_minify:
#             out = minify(out, **minify_settings)
#         file.write(out)
#
#
# print("Building Meta Files")
# build_meta(Path("content/sitemap.jinja2"), Path("sitemap.xml"), do_minify=True)
# build_meta(Path("content/robots.jinja2"), Path("robots.txt"))
# build_meta(Path("content/browserconfig.jinja2"), Path("fav/browserconfig.xml"), do_minify=True)
#
# print("Building Pages")
# for item in content:
#     log_build(item.in_path, item.out_path)
#     item.render(page=item, pages=pages, schemas=schemas)
