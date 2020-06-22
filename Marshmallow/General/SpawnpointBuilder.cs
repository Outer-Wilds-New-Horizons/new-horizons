using UnityEngine;

namespace Marshmallow.General
{
    static class SpawnpointBuilder
    {
        public static SpawnPoint Make(GameObject body, Vector3 position)
        {
            GameObject spawnGO = new GameObject();
            spawnGO.transform.parent = body.transform;
            spawnGO.layer = 8;

            spawnGO.transform.localPosition = position;

            //Logger.Log("Made spawnpoint on [" + body.name + "] at " + position);

            var SS = spawnGO.AddComponent<SpawnPoint>();

            return SS;
        }
    }
}
