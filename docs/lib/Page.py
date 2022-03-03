from dataclasses import dataclass
from pathlib import Path

from jinja2 import Environment
from markdown import Markdown


@dataclass
class Page:
    sort_priority: int
    in_path: Path
    out_path: Path
    title: str
    env: Environment

    def __init__(self, path, environment, options):
        self.in_path = path
        self.env = environment
        md = Markdown(**options)
        with path.open() as file:
            md.convert(file.read())
        self.sort_priority = int(md.Meta.get('sort-priority', '20')[0])
        self.title = md.Meta.get('title', (path.stem,))[0]
        outfile: Path
        try:
            outfile = Path("out/", path.relative_to(Path("content/pages/")).parent,
                           md.Meta['out-file'][0] + '.html')
        except KeyError:
            outfile = Path("out/", path.relative_to(Path("content/pages/"))).with_suffix('.html')
        self.out_path = outfile

    def render(self, **options):
        template = self.env.get_template(str(self.in_path.relative_to(Path("content/")).as_posix()))
        page_template = self.env.get_template("base/page_template.jinja2")
        rendered_string = page_template.render(content=template.render(**options), **options)

        self.out_path.parent.mkdir(mode=511, parents=True, exist_ok=True)

        with self.out_path.open(mode='w+', encoding='utf-8') as file:
            file.write(rendered_string)
