from pathlib import Path

from PIL import Image

from lib.Content.StaticItem import StaticItem


class ImageStaticItem(StaticItem):

    extensions = ('png', 'jpg', 'jpeg', 'webp')

    sizes = {}

    @classmethod
    def get_size(cls, stem):
        return cls.sizes.get(stem, (0, 0))

    def __init__(self, path: Path):
        super().__init__(path)
        with Image.open(self.in_path) as img:
            self.sizes[str(self.out_path.relative_to('out/').as_posix())] = img.size
