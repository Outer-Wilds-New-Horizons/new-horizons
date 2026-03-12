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

        public override bool TryGetTranslatedCustomName(out string translatedCustomName)
        {
            if (GetAstroObjectName() == AstroObject.Name.Eye)
            {
                translatedCustomName = UITextLibrary.GetString(UITextType.LocationEye);
                return true;
            }
            return base.TryGetTranslatedCustomName(out translatedCustomName);
        }
    }
}
