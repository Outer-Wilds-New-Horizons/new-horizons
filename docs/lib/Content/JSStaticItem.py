from rjsmin import jsmin

from lib.Content.StaticItem import MinifiedStaticItem


class JSStaticItem(MinifiedStaticItem):

    extensions = ('js',)

    def minify(self, content):
        return jsmin(content)


