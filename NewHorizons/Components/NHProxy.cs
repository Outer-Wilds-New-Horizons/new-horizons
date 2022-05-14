using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHProxy : ProxyPlanet
    {
        public string astroName;
        public Renderer[] renderers;
        public TessellatedRenderer[] tessellatedRenderers;

        public override void Initialize()
        {
            AstroObject astroObject = AstroObjectLocator.GetAstroObject(astroName);
            _realObjectTransform = astroObject.transform;
            _hasAtmosphere = _atmosphere != null;
            if (_hasAtmosphere)
            {
                _atmosphereMaterial = new Material(_atmosphere.sharedMaterial);
                _baseAtmoMatShellInnerRadius = _atmosphereMaterial.GetFloat(propID_AtmoInnerRadius);
                _baseAtmoMatShellOuterRadius = _atmosphereMaterial.GetFloat(propID_AtmoOuterRadius);
                _atmosphere.sharedMaterial = _atmosphereMaterial;
            }
            if (_fog != null)
            {
                _hasFog = true;
                _fogMaterial = new Material(_fog.sharedMaterial);
                _fogMaterial.SetFloat(propID_LODFade, 1f);
                _fog.sharedMaterial = _fogMaterial;
            }
        }

        public override void ToggleRendering(bool on)
        {
            base.ToggleRendering(on);
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(on);
            }
        }
    }
}