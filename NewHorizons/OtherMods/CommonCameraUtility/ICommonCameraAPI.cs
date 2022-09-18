using UnityEngine;
using UnityEngine.Events;

namespace NewHorizons.OtherMods.CommonCameraUtility
{
    public interface ICommonCameraAPI
    {
        void RegisterCustomCamera(OWCamera OWCamera);
        (OWCamera, Camera) CreateCustomCamera(string name);
        UnityEvent<PlayerTool> EquipTool();
        UnityEvent<PlayerTool> UnequipTool();
    }
}
