---
title: Translations
description: A guide to creating translations in New Horizons
---

There are 12 supported languages in Outer Wilds: english, spanish_la, german, french, italian, polish, portuguese_br, japanese, russian, chinese_simple, korean, and turkish.

All translations must go in a folder in the root directory called "translations".

In this folder you can put json files with the name of the language you want to translate for. Inside this file just follow the translation schema.

Here's an example, for `russian.json`:

```json title="russian.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/translation_schema.json",
    "DialogueDictionary": {
        "Fred": "Фред",
        "You looking at something?": "Ты что-то искал?",
        "Aren't you guys all supposed to be dead?": "А разве номаи не вымерли?",
        "OH MY GOD A LIVING NOMAI AHHH WHAT HOW?!": "ААААА, ЖИВАЯ НОМАИ?!"
    },
    "ShipLogDictionary": {
        "Unexpected guests": "Незванные гости",
        "Visitors": "Гости",
        "When I went to sleep by the campfire only Slate was here, who are these characters?": "Когда я ложился спать у костра здесь был только Сланец. Кто все остальные?",
        "I met a talking jellyfish. His name is Geswaldo!": "Я встретил говорящую медузу. Его зовут Гесвальдо!"
    }
}
```

## CLI Tool

Are you tired of manually translating JSON? Do you want an automatic translator? Well then the [nh-translation-helper](https://www.npmjs.com/package/nh-translation-helper) may be for you!

This tool has the following features:

-   Extract text from XML files and create english.json as the translation source.
-   Translate english.json to create a json file for another language.

This section outlines how to install and use the nh-translation-helper.

### Installation

To get started, head over to the [repo for the tool](https://github.com/96-38/nh-translation-helper) and prepare the requirements:

-   Install [Node.js](https://nodejs.org/) >= 12.0.0
    -   Install the LTS version.
-   Get [DeepL API](https://www.deepl.com/docs-api) Key (Free or Pro)
    -   Sign up [here](https://www.deepl.com/pro#developer)

When you are ready, execute the following command in a terminal or command prompt:

```bash
npm i -g nh-translation-helper
```

Now your installation is complete!

You can use the tool by executing the following command in a terminal or command prompt:

```bash
nh-translation-helper
```

### Generating a english.json from XML

Select `Generate english.json from XML files` and enter the path of your project folder.

You are done! a english.json has been generated in "_your_project_root_/translations/".

### Translating english.json to another language

Select `Translate JSON (DeepL API key required)` and enter the path of your project folder. ( Note: **Not** the path to the "translations" folder. )

Select the source and target languages.

You are done! a translated json file has been generated in "_your_project_root_/translations/".

Please enter the DeepL API key for the first time only. The API key will be saved on your PC.

### Note

-   Not supported extracting UIDictionary and AchievementTranslations

    -   It is difficult to parse these automatically, and the number of words is small that it would be better to add them by MOD developers manually for better results.
    -   Translating UIDictionary and AchievementTranslations is supported.

-   Not supported translation into Korean

    -   Translation is provided by the DeepL API, so it is not possible to translate into languages that are not supported by DeepL.

-   The generated translations are "**not**" perfect

    -   It is a machine translation though DeepL. The translations on DeepL are known to be too casual or to abbreviate some sentences.
    -   It will need to be manually corrected to make it a good translation. However, this tool allows you to prototype and is more efficient than starting from scratch. Also, the CDATA tag has been removed from the translated text and must be added manually.

-   Parsing errors may occur when trying to translate manually created JSON files
    -   In many cases, this is due to a specific comment in the JSON. Please remove the comments and try again.
    -   Most comments are processed normally, but errors may occur if the comment contains special symbols or if the comment is located at the end of a JSON object.
