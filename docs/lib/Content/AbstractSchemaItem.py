from abc import ABC
from pathlib import Path

from jinja2 import Template

from lib.Content.AbstractTemplatedItem import AbstractTemplatedItem


class AbstractSchemaItem(AbstractTemplatedItem, ABC):
    root_dir = Path('schemas/')

    def __init__(self, in_path: Path):
        super().__init__(in_path)
        self.out_path = Path('out/schemas/', self.out_path.relative_to('out'))

    def render(self, template: Template, **context):
        raise NotImplementedError()
