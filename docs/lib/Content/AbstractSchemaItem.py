from abc import ABC
from pathlib import Path

from jinja2 import Template, Environment

from lib.Content.AbstractTemplatedItem import AbstractTemplatedItem


class AbstractSchemaItem(AbstractTemplatedItem, ABC):
    root_dir = Path('schemas/')

    def __init__(self, in_path: Path):
        super().__init__(in_path)
        self.out_path = Path('out/schemas/', self.out_path.relative_to('out'))

    @classmethod
    def initialize(cls, env: Environment):
        new_pages = super(AbstractSchemaItem, cls).initialize(env)
        for page in new_pages:
            page.out_path = page.out_path.with_stem(page.title.replace(" ", "_").lower())
        return new_pages

    def render(self, template: Template, **context):
        raise NotImplementedError()
