using NewHorizons.OtherMods.MenuFramework;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OtherMods.CommonCameraUtility
{
    public static class CommonCameraHandler
    {
        private static ICommonCameraAPI _cameraAPI;

        static CommonCameraHandler()
        {
            _cameraAPI = Main.Instance.ModHelper.Interaction.TryGetModApi<ICommonCameraAPI>("xen.CommonCameraUtility");
        }

        public static void RegisterCustomCamera(OWCamera camera)
        {
            if (_cameraAPI != null)
            {
                _cameraAPI.RegisterCustomCamera(camera);
            }
            else
            {
                Logger.LogError("Tried to register custom camera but Common Camera Utility was missing.");
            }
        }
    }
}
