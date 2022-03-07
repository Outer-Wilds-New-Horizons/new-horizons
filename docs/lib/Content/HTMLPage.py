from pathlib import Path

from lib.Content.AbstractTemplatedItem import AbstractTemplatedItem


class HTMLPage(AbstractTemplatedItem):
    root_dir = Path("pages/")
    extensions = ('html', 'jinja2', 'jinja')
