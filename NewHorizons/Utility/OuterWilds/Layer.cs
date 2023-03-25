using UnityEngine;

namespace NewHorizons.Utility.OuterWilds
{
    public static class Layer
    {
        public static int Default = LayerMask.NameToLayer(nameof(Default));
        public static int TransparentFX = LayerMask.NameToLayer(nameof(TransparentFX));
        public static int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static int Water = LayerMask.NameToLayer(nameof(Water));
        public static int UI = LayerMask.NameToLayer(nameof(UI));
        public static int PlayerSafetyCollider = LayerMask.NameToLayer(nameof(PlayerSafetyCollider));
        public static int Sun = LayerMask.NameToLayer(nameof(Sun));
        public static int ShipInterior = LayerMask.NameToLayer(nameof(ShipInterior));
        public static int HelmetUVPass = LayerMask.NameToLayer(nameof(HelmetUVPass));
        public static int AdvancedDetector = LayerMask.NameToLayer(nameof(AdvancedDetector));
        public static int Primitive = LayerMask.NameToLayer(nameof(Primitive));
        public static int IgnoreSun = LayerMask.NameToLayer(nameof(IgnoreSun));
        public static int AdvancedEffectVolume = LayerMask.NameToLayer(nameof(AdvancedEffectVolume));
        public static int BasicEffectVolume = LayerMask.NameToLayer(nameof(BasicEffectVolume));
        public static int ProxyPrimitive = LayerMask.NameToLayer(nameof(ProxyPrimitive));
        public static int ReferenceFrameVolume = LayerMask.NameToLayer(nameof(ReferenceFrameVolume));
        public static int BasicDetector = LayerMask.NameToLayer(nameof(BasicDetector));
        public static int Interactible = LayerMask.NameToLayer(nameof(Interactible));
        public static int VisibleToProbe = LayerMask.NameToLayer(nameof(VisibleToProbe));
        public static int HeadsUpDisplay = LayerMask.NameToLayer(nameof(HeadsUpDisplay));
        public static int CloseRangeRFVolume = LayerMask.NameToLayer(nameof(CloseRangeRFVolume));
        public static int ProxyPrimitive2 = LayerMask.NameToLayer(nameof(ProxyPrimitive2));
        public static int PhysicalDetector = LayerMask.NameToLayer(nameof(PhysicalDetector));
        public static int VisibleToPlayer = LayerMask.NameToLayer(nameof(VisibleToPlayer));
        public static int DreamSimulation = LayerMask.NameToLayer(nameof(DreamSimulation));
        public static int Skybox = LayerMask.NameToLayer(nameof(Skybox));
        public static int IgnoreOrbRaycast = LayerMask.NameToLayer(nameof(IgnoreOrbRaycast));
        public static int Flashback = LayerMask.NameToLayer(nameof(Flashback));
    }
}
