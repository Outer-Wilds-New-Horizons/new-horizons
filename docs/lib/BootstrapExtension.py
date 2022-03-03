from markdown import Extension
from markdown.treeprocessors import Treeprocessor

class BootstrapExtension(Extension):
    def extendMarkdown(self, md, md_globals):
        md.registerExtension(self)
        self.processor = BootstrapTreeprocessor()
        self.processor.md = md
        self.processor.config = self.getConfigs()
        md.treeprocessors.add('bootstrap', self.processor, '_end')


classes = {
    'img': "img-fluid",
    'table': "table-striped"
}


def process(node):
    if node.tag in classes.keys:
        node.set("class", classes[node.tag])
    else:
        for child in node.getiterator():
            process(child)



class BootstrapTreeProcessor(Treeprocessor):

    def run(self, node):
        for child in node.getiterator():
            process(child)
        return node
        