using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility
{
    static class AddToUITable
    {
        public static int Add(string text)
        {
            TextTranslation.TranslationTable instance = GameObject.FindObjectOfType<TextTranslation>().GetValue<TextTranslation.TranslationTable>("m_table");

            try
            {
                KeyValuePair<int, string> pair = instance.theUITable.First(x => x.Value.Equals(text));
                if (pair.Equals(default(KeyValuePair<int, string>))) return pair.Key;
            }
            catch (Exception) { }
            instance.Insert_UI(instance.theUITable.Keys.Max() + 1, text);
            return instance.theUITable.Keys.Max();
        }
    }
}
