from rcssmin import cssmin

from lib.Content.StaticItem import MinifiedStaticItem


class CSSStaticItem(MinifiedStaticItem):

    extensions = ('css',)

    def minify(self, content):
        return cssmin(content)
