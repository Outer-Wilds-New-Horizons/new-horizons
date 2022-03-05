import os

import xmlschema
from xmlschema.extras.codegen import AbstractGenerator, filter_method

__all__ = ('XMLSchema',)

from lib.Content.AbstractSchemaItem import AbstractSchemaItem


def children(group):
    child = [child for child in group if child.__class__.__name__ == "XsdElement"]
    for child_list in [children(inner_group) for inner_group in group if inner_group.__class__.__name__ == "XsdGroup"]:
        child += child_list
    return child


def ancestry(element):
    if element.parent is None:
        print(element.name)
        return [element.name]
    else:
        if element.name is None:
            return ancestry(element.parent)
        else:
            return [element.name] + ancestry(element.parent)


class HTMLConverter(AbstractGenerator):
    formal_language = "html"
    searchpaths = [os.getcwd() + "/content/"]

    @staticmethod
    @filter_method
    def children(group):
        return children(group)

    @staticmethod
    @filter_method
    def id_path(element):
        return '-'.join(reversed(ancestry(element)))

    @staticmethod
    @filter_method
    def split(string, delim):
        return string.split(delim)

    @staticmethod
    @filter_method
    def occurs_text(occurs):
        words = {
            0: "Zero",
            1: "One",
            None: "Many"
        }
        return "Appears " + words[occurs[0]] + " To " + words[occurs[1]] + " " + ("Time" if occurs[1] == 1 else "Times")

    def update_filters(self, filters):
        self._env.filters.update(filters)


class XMLSchema(AbstractSchemaItem):
    extensions = ('xsd', 'xml')

    def load_metadata(self):
        with self.in_path.open(mode='r', encoding='utf-8') as file:
            file.readline()
            line = file.readline()
        if len(line.strip()) != 0 and '<!--' in line and '-->' in line:
            self.title = line.replace('<!--', '').replace('-->', '').strip()
        super(XMLSchema, self).load_metadata()

    def render(self, **context):
        context.update({
            'page': self
        })
        with self.in_path.open(mode='r', encoding='utf-8') as file:
            schema = xmlschema.XMLSchema(file)
        converter = HTMLConverter(schema)
        converter.update_filters(self.env.filters)
        return converter.render('base/schema/xml/schema_base.jinja2', global_vars=context)[0]




