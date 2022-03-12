import re
from pathlib import Path

from lib.Content.AbstractTemplatedItem import AbstractTemplatedItem


class HTMLPage(AbstractTemplatedItem):

    def load_metadata(self):
        with self.in_path.open(mode='r', encoding='utf-8') as file:
            content = file.read()
        for match in re.findall(r"{#~(.*?)~#}", content, re.MULTILINE):
            seperated = match.strip().split(':')
            print(match.strip())
            self.meta[seperated[0].lower().replace('-', '_')] = seperated[1]
        super(HTMLPage, self).load_metadata()

    root_dir = Path("pages/")
    extensions = ('html', 'jinja2', 'jinja')
