from abc import ABC
from pathlib import Path

from htmlmin import minify
from jinja2 import Environment


class AbstractItem(ABC):

    output_ext: str = '.html'
    env: Environment
    in_path: Path
    out_path: Path
    root_dir: Path

    def __init__(self, in_path: Path, ext: str = None):
        self.env = None
        if ext is not None:
            self.output_ext = ext
        self.in_path = in_path
        self.out_path = Path('out/', in_path.name).with_suffix(self.output_ext)

    @classmethod
    def initialize(cls, env: Environment) -> list:
        raise NotImplementedError()

    def render(self, **context) -> str:
        raise NotImplementedError()

    def _save(self, rendered: str):
        self.out_path.parent.mkdir(mode=511, parents=True, exist_ok=True)
        with self.out_path.open(mode='w+', encoding='utf-8') as file:
            file.write(rendered)

    def generate(self, **context) -> None:
        print("Building:", self.in_path, "->", self.out_path)
        self._save(self.render(**context))

    def add_route(self, routes, out_dir):
        pass


class MinifyMixin(AbstractItem, ABC):

    MINIFY_SETTINGS = {
        'remove_empty_space': True,
        'keep_pre': True,
        'remove_optional_attribute_quotes': False
    }

    def _save(self, rendered: str):
        rendered = minify(rendered, **self.MINIFY_SETTINGS)
        super(MinifyMixin, self)._save(rendered)





