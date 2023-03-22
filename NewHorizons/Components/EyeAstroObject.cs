namespace NewHorizons.Components
{
    public class EyeAstroObject : AstroObject
    {
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
