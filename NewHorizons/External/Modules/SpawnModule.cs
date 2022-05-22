using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class SpawnModule
    {
        public MVector3 PlayerSpawnPoint { get; set; }
        public MVector3 PlayerSpawnRotation { get; set; }
        public MVector3 ShipSpawnPoint { get; set; }
        public MVector3 ShipSpawnRotation { get; set; }
        public bool StartWithSuit { get; set; }
    }
}
