using NewHorizons.External.Configs;

namespace NewHorizons.Utility.DebugTools.Menu
{
    abstract class DebugSubmenu
    {
        internal abstract void OnAwake(DebugMenu menu);
        internal abstract void OnGUI(DebugMenu menu);
        internal abstract void PrintNewConfigSection(DebugMenu menu);
        internal abstract void PreSave(DebugMenu menu);
        internal abstract void OnInit(DebugMenu menu);
        internal abstract void LoadConfigFile(DebugMenu menu, PlanetConfig config);
        internal abstract void OnBeginLoadMod(DebugMenu debugMenu);

        internal abstract string SubmenuName();

        internal abstract void GainActive();
        internal abstract void LoseActive();

    }
}
