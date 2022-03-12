from abc import ABC
from itertools import chain
from pathlib import Path

from jinja2 import Template, Environment

from lib.Content.AbstractItem import AbstractItem, MinifyMixin


class AbstractTemplatedItem(MinifyMixin, AbstractItem, ABC):
    extensions: tuple[str]
    title: str = None
    description: str | None = None
    sort_priority: int = None
    render_toc: bool = False
    table_of_contents: dict = {}
    meta: dict = {
        'title': None,
        'description': None,
        'sort_priority': 10
    }

    def load_metadata(self):
        self.title = self.meta['title'] if self.meta['title'] is not None else self.in_path.stem
        self.description = self.meta['description']
        self.sort_priority = int(self.meta['sort_priority'])

    @classmethod
    def initialize(cls, env: Environment):
        pages = []
        file_paths = list(chain(*[Path('content/', cls.root_dir).glob(f'**/*.{ext}') for ext in cls.extensions]))
        for path in file_paths:
            new_page = cls(path)
            new_page.env = env
            new_page.load_metadata()
            pages.append(new_page)
        return pages

    def inner_render(self, template: Template, **context):
        return template.render(**context)

    def render(self, **context):
        container = self.env.get_template('base/page_template.jinja2')
        template = self.env.get_template(str(self.in_path.relative_to(Path('content/')).as_posix()))
        context.update({
            'page': self,
            'rendered': self.inner_render(template, **context),
            'render_toc': self.render_toc
        })
        return container.render(**context)

    def add_route(self, router, out_dir):
        router[self.title.lower()] = out_dir + str(self.out_path.relative_to('out/'))
