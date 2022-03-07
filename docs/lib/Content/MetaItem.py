from jinja2 import Environment

from lib.Content.AbstractItem import AbstractItem, MinifyMixin


class MetaItem(AbstractItem):

    @classmethod
    def initialize(cls, env: Environment) -> list:
        cls.env = env

    def render(self, **context) -> str:
        template = self.env.get_template(str(self.in_path.as_posix()))
        return template.render(**context)


class MinifiedMetaItem(MinifyMixin, MetaItem):
    pass
