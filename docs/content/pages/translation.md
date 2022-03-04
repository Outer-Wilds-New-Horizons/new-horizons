Title: Translations
Sort-Priority: 100

There are 12 supported languages in Outer Wilds: english, spanish_la, german, french, italian, polish, portuguese_br, japanese, russian, chinese_sinple, korean, and turkish.

All translations must go in a folder in the root directory called "translations".

In this folder you can put json files with the name of the language you want to translate for. Inside this file just follow the translation schema.

Here's an example, for `russian.json`:

```json
{
    "DialogueDictionary" :
    {
        "Fred" : "Фред",
        "You looking at something?" : "Ты что-то искал?",
        "Aren't you guys all supposed to be dead?" : "А разве номаи не вымерли?",
        "OH MY GOD A LIVING NOMAI AHHH WHAT HOW?!" : "ААААА, ЖИВАЯ НОМАИ?!"
    },
    "ShipLogDictionary" :
    {
        "Unexpected guests" : "Незванные гости",
        "Visitors" : "Гости",
        "When I went to sleep by the campfire only Slate was here, who are these characters?" : "Когда я ложился спать у костра здесь был только Сланец. Кто все остальные?",
        "I met a talking jellyfish. His name is Geswaldo!" : "Я встретил говорящую медузу. Его зовут Гесвальдо!"
    }
}
```