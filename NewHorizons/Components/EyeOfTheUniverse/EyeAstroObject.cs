using NewHorizons.Components.Orbital;

namespace NewHorizons.Components.EyeOfTheUniverse
{
    public class EyeAstroObject : NHAstroObject
    {
        public EyeAstroObject()
        {
            isVanilla = true;
            modUniqueName = Main.Instance.ModHelper.Manifest.UniqueName;
        }

        public new void Awake()
        {
            _owRigidbody = GetComponent<OWRigidbody>();
        }

        public void Register()
        {
            Locator.RegisterAstroObject(this);
        }
    }
}
