using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Builder.General
{
    public static class SectorBuilder
    {
        public static Sector Make(GameObject planetBody, OWRigidbody owRigidBody, float sphereOfInfluence)
        {
            GameObject sectorGO = new GameObject("Sector");
            sectorGO.SetActive(false);
            sectorGO.transform.parent = planetBody.transform;
            sectorGO.transform.localPosition = Vector3.zero;

            SphereShape SS = sectorGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = sphereOfInfluence + 10;
            SS.center = Vector3.zero;

            sectorGO.AddComponent<OWTriggerVolume>();

            Sector S = sectorGO.AddComponent<Sector>();
            S._name = (Sector.Name)24;
            S._attachedOWRigidbody = owRigidBody;
            S._subsectors = new List<Sector>();

            sectorGO.SetActive(true);
            S.enabled = true;

            return S;
        }

        public static Sector Make(GameObject planetBody, OWRigidbody owRigidBody, Sector parent)
        {
            if (parent == null) return null;

            GameObject sectorGO = new GameObject("Sector");
            sectorGO.SetActive(false);
            sectorGO.transform.parent = planetBody.transform;
            sectorGO.transform.localPosition = Vector3.zero;

            Sector S = sectorGO.AddComponent<Sector>();
            S._idString = parent._idString;
            S._name = parent._name;
            S._attachedOWRigidbody = owRigidBody;
            S._subsectors = new List<Sector>();
            S._triggerRoot = parent._triggerRoot;
            S._proximityTrigger = parent._proximityTrigger;
            S._volumeExcluder = parent._volumeExcluder;
            S._owTriggerVolume = parent._owTriggerVolume;
            S._frameCounter = parent._frameCounter;
            S.SetParentSector(parent);

            sectorGO.SetActive(true);
            S.enabled = true;

            return S;
        }
    }
}
