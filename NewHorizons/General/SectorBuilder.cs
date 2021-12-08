using NewHorizons.External;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Body
{
    static class MakeSector
    {
        public static Sector Make(GameObject body, OWRigidbody rigidbody, IPlanetConfig config)
        {
            GameObject sectorGO = new GameObject("Sector");
            sectorGO.SetActive(false);
            sectorGO.transform.parent = body.transform;

            SphereShape SS = sectorGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = config.AtmoEndSize + 10;
            SS.center = Vector3.zero;

            sectorGO.AddComponent<OWTriggerVolume>();

            Sector S = sectorGO.AddComponent<Sector>();
            S.SetValue("_name", Sector.Name.Unnamed);
            S.SetValue("__attachedOWRigidbody", rigidbody);
            S.SetValue("_subsectors", new List<Sector>());

            sectorGO.SetActive(true);

            Logger.Log("Finished building sector", Logger.LogType.Log);
            return S;
        }
    }
}
