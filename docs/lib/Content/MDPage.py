from pathlib import Path

from markdown import Markdown

from lib.Content.AbstractTemplatedItem import AbstractTemplatedItem
from lib.BootstrapExtension import BootstrapExtension


class MDPage(AbstractTemplatedItem):
    root_dir = Path("pages/")
    extensions = ('md', 'markdown')

    MARKDOWN_SETTINGS = {
        'extensions': ['extra', 'toc', 'meta', BootstrapExtension()]
    }

    def load_metadata(self):
        md = self.__get_md()
        with self.in_path.open(mode='r', encoding='utf-8') as file:
            md.convert(file.read())

        self.title = md.Meta.get('title')[0]
        self.description = md.Meta.get('description', [None])[0]
        raw_priority = md.Meta.get('sort-priority')
        if raw_priority is not None:
            self.sort_priority = int(raw_priority[0])
        out_name = md.Meta.get('out-file', None)
        if out_name is not None:
            self.out_path = self.out_path.with_stem(out_name[0])
        super(MDPage, self).load_metadata()

    def __get_md(self):
        return Markdown(**self.MARKDOWN_SETTINGS)

    def inner_render(self, template, **context):
        rendered_markdown = super(MDPage, self).inner_render(template, **context)
        md = self.__get_md()
        return md.convert(rendered_markdown)

