using UnityEngine;

namespace NewHorizons.Components;

public class ConditionalNomaiTextTranslatable : MonoBehaviour
{
    public string legiblePersistentCondition;

    public bool IsIllegible() => !(string.IsNullOrEmpty(legiblePersistentCondition) || PlayerData.GetPersistentCondition(legiblePersistentCondition));
}
