from abc import ABC
from pathlib import Path


class StaticItem:

    extensions = ('',)

    def __init__(self, path: Path):
        self.in_path = path
        self.out_path = Path('out/', path.relative_to('content/static/'))
        self.out_path.parent.mkdir(parents=True, exist_ok=True)

    def generate(self, **kwargs):
        with self.in_path.open(mode='rb') as file:
            content = file.read()
        with self.out_path.open(mode='wb+') as file:
            file.write(content)


class MinifiedStaticItem(StaticItem, ABC):

    def __init__(self, path: Path):
        super().__init__(path)
        self.out_path = self.out_path.with_stem(self.out_path.stem + '.min')

    def minify(self, content):
        raise NotImplementedError()

    def generate(self, **kwargs):
        with self.in_path.open(mode='r') as file:
            content = file.read()
        with self.out_path.open(mode='w+') as file:
            file.write(self.minify(content))
