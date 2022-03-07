from os import getenv
from pathlib import Path

from markdown import Extension
from markdown.treeprocessors import Treeprocessor

from lib.Content.ImageStaticItem import ImageStaticItem


class BootstrapExtension(Extension):
    def extendMarkdown(self, md, md_globals):
        md.registerExtension(self)
        self.processor = BootstrapTreeProcessor()
        self.processor.md = md
        self.processor.config = self.getConfigs()
        md.treeprocessors.add('bootstrap', self.processor, '_end')


classes = {
    'img': "img-fluid rounded mx-auto d-flex",
    'table': "table-striped"
}


def process(node):
    if node.tag in classes.keys():
        node.set("class", classes[node.tag])
    if node.tag == 'img' and "{{" not in node.get('src'):
        size = ImageStaticItem.get_size(str(Path(node.get('src')).relative_to(getenv('OUT_DIR')).as_posix()))
        node.set('width', str(size[0]))
        node.set('height', str(size[1]))
    for child in node:
        process(child)


class BootstrapTreeProcessor(Treeprocessor):

    def run(self, node):
        for child in node:
            process(child)
        return node
        