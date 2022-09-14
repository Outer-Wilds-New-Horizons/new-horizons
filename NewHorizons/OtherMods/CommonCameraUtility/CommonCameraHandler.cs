using NewHorizons.OtherMods.MenuFramework;

namespace NewHorizons.OtherMods.CommonCameraUtility
{
    public static class CommonCameraHandler
    {
        private static ICommonCameraAPI _cameraAPI;

        static CommonCameraHandler()
        {
            _cameraAPI = Main.Instance.ModHelper.Interaction.TryGetModApi<ICommonCameraAPI>("xen.CommonCameraUtility");
        }

        public static void RegisterCustomCamera(OWCamera camera) => _cameraAPI.RegisterCustomCamera(camera);
    }
}
