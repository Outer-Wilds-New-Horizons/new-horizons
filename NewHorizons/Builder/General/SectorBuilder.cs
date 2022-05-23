#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.General
{
    public static class MakeSector
    {
        public static Sector Make(GameObject planetBody, OWRigidbody owRigidBody, float sphereOfInfluence)
        {
            var sectorGO = new GameObject("Sector");
            sectorGO.SetActive(false);
            sectorGO.transform.parent = planetBody.transform;
            sectorGO.transform.localPosition = Vector3.zero;

            var SS = sectorGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = sphereOfInfluence + 10;
            SS.center = Vector3.zero;

            sectorGO.AddComponent<OWTriggerVolume>();

            var S = sectorGO.AddComponent<Sector>();
            S._name = (Sector.Name) 24;
            S._attachedOWRigidbody = owRigidBody;
            S._subsectors = new List<Sector>();

            sectorGO.SetActive(true);

            return S;
        }
    }
}