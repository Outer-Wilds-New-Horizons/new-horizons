using NewHorizons.Components.SizeControllers;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SupernovaPlanetEffectController;

namespace NewHorizons.Components
{
    public class NHSupernovaPlanetEffectController : MonoBehaviour
    {
        public Light _ambientLight;
        public float _ambientLightOrigIntensity;
        public LODGroup _atmosphere;
        public Renderer _atmosphereRenderer;
        public PlanetaryFogController _fog;
        public Renderer _fogImpostor;
        public Color _fogOrigTint;
        [Space]
        public MeshRenderer _shockLayer;
        public static readonly Color _shockLayerDefaultColor = new Color(0.3569f, 0.7843f, 1f, 1f);
        [ColorUsage(true, true)]
        public Color _shockLayerColor = _shockLayerDefaultColor;
        public float _shockLayerStartRadius = 1000f;
        public float _shockLayerFullRadius = 10000f;
        public float _shockLayerTrailLength = 300f;
        public float _shockLayerTrailFlare = 100f;

        private LOD[] _atmosphereLODs;

        public StarEvolutionController _starEvolutionController;

        public SunController _sunController;

        public void Awake()
        {
            if (s_matPropBlock_Atmosphere == null)
            {
                s_matPropBlock_Atmosphere = new MaterialPropertyBlock();
                s_propID_SunIntensity = Shader.PropertyToID("_SunIntensity");
                s_propID_Tint = Shader.PropertyToID("_Tint");
                s_matPropBlock_ShockLayer = new MaterialPropertyBlock();
                s_propID_Color = Shader.PropertyToID("_Color");
                s_propID_WorldToLocalShockMatrix = Shader.PropertyToID("_WorldToShockLocalMatrix");
                s_propID_Dir = Shader.PropertyToID("_Dir");
                s_propID_Length = Shader.PropertyToID("_Length");
                s_propID_Flare = Shader.PropertyToID("_Flare");
                s_propID_TrailFade = Shader.PropertyToID("_TrailFade");
                s_propID_GradientLerp = Shader.PropertyToID("_GradientLerp");
                s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
            }
        }

        public void Start()
        {
            SupernovaEffectHandler.RegisterPlanetEffect(this);
            if (_atmosphere != null) _atmosphereLODs = _atmosphere.GetLODs();
            if (_fog != null)
            {
                _fogOrigTint = _fog.fogTint;
                _fogImpostor = _fog.fogImpostor;
            }
            else if (_fogImpostor != null)
            {
                _fogOrigTint = _fogImpostor.material.GetColor(s_propID_Tint);
            }
            if (_shockLayer != null) _shockLayer.enabled = false;
        }

        public void OnDestroy()
        {
            SupernovaEffectHandler.UnregisterPlanetEffect(this);
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
            if (_shockLayer != null) _shockLayer.enabled = false;
        }

        public void Update()
        {
            SupernovaEffectHandler.GetNearestStarSupernova(this);
            if (_starEvolutionController != null)
            {
                if (_starEvolutionController.HasSupernovaStarted())
                {
                    if (_shockLayer != null)
                    {
                        if (!_shockLayer.enabled) _shockLayer.enabled = true;
                        Vector3 dir = Vector3.Normalize(transform.position - _starEvolutionController.transform.position);
                        s_matPropBlock_ShockLayer.SetColor(s_propID_Color, _starEvolutionController.SupernovaColour != null ? _starEvolutionController.SupernovaColour.ToColor() : _shockLayerColor);
                        s_matPropBlock_ShockLayer.SetMatrix(s_propID_WorldToLocalShockMatrix, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(dir, Vector3.up), Vector3.one).inverse);
                        s_matPropBlock_ShockLayer.SetVector(s_propID_Dir, dir);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_Length, _shockLayerTrailLength);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_Flare, _shockLayerTrailFlare);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_TrailFade, 1f - Mathf.InverseLerp(_shockLayerStartRadius, _shockLayerFullRadius, _starEvolutionController.GetSupernovaRadius()));
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_GradientLerp, 0);
                        s_matPropBlock_ShockLayer.SetVector(s_propID_MainTex_ST, _shockLayer.sharedMaterial.GetVector(s_propID_MainTex_ST) with { w = -Time.timeSinceLevelLoad });
                        _shockLayer.SetPropertyBlock(s_matPropBlock_ShockLayer);
                    }
                }
                if (_starEvolutionController.IsCollapsing())
                {
                    float collapseProgress = _starEvolutionController.GetCollapseProgress();

                    if (_ambientLight != null) _ambientLight.intensity = _ambientLightOrigIntensity * (1f - collapseProgress);

                    if (_atmosphere != null)
                    {
                        s_matPropBlock_Atmosphere.SetFloat(s_propID_SunIntensity, 1f - collapseProgress);

                        foreach (var lod in _atmosphereLODs)
                            foreach (var renderer in lod.renderers)
                                renderer.SetPropertyBlock(s_matPropBlock_Atmosphere);
                    }

                    if (_atmosphereRenderer != null)
                    {
                        s_matPropBlock_Atmosphere.SetFloat(s_propID_SunIntensity, 1f - collapseProgress);

                        _atmosphereRenderer.SetPropertyBlock(s_matPropBlock_Atmosphere);
                    }

                    if (_fog != null) _fog.fogTint = Color.Lerp(_fogOrigTint, Color.black, collapseProgress);

                    if (_fogImpostor != null) _fogImpostor.material.SetColor(s_propID_Tint, Color.Lerp(_fogOrigTint, Color.black, collapseProgress));
                }
                else
                {
                    if (_shockLayer != null) _shockLayer.enabled = false;
                }
            }
            else if (_sunController != null)
            {
                if (_sunController.HasSupernovaStarted())
                {
                    if (_shockLayer != null)
                    {
                        if (!_shockLayer.enabled) _shockLayer.enabled = true;
                        Vector3 dir = Vector3.Normalize(transform.position - _sunController.transform.position);
                        s_matPropBlock_ShockLayer.SetColor(s_propID_Color, _shockLayerColor);
                        s_matPropBlock_ShockLayer.SetMatrix(s_propID_WorldToLocalShockMatrix, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(dir, Vector3.up), Vector3.one).inverse);
                        s_matPropBlock_ShockLayer.SetVector(s_propID_Dir, dir);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_Length, _shockLayerTrailLength);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_Flare, _shockLayerTrailFlare);
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_TrailFade, 1f - Mathf.InverseLerp(_shockLayerStartRadius, _shockLayerFullRadius, _sunController.GetSupernovaRadius()));
                        s_matPropBlock_ShockLayer.SetFloat(s_propID_GradientLerp, 0);
                        s_matPropBlock_ShockLayer.SetVector(s_propID_MainTex_ST, _shockLayer.sharedMaterial.GetVector(s_propID_MainTex_ST) with { w = -Time.timeSinceLevelLoad });
                        _shockLayer.SetPropertyBlock(s_matPropBlock_ShockLayer);
                    }
                }
                else if (_sunController._collapseStarted)
                {
                    float collapseProgress = _sunController.GetCollapseProgress();

                    if (_ambientLight != null) _ambientLight.intensity = _ambientLightOrigIntensity * (1f - collapseProgress);

                    if (_atmosphere != null)
                    {
                        s_matPropBlock_Atmosphere.SetFloat(s_propID_SunIntensity, 1f - collapseProgress);

                        foreach (var lod in _atmosphereLODs)
                            foreach (var renderer in lod.renderers)
                                renderer.SetPropertyBlock(s_matPropBlock_Atmosphere);
                    }

                    if (_atmosphereRenderer != null)
                    {
                        s_matPropBlock_Atmosphere.SetFloat(s_propID_SunIntensity, 1f - collapseProgress);

                        _atmosphereRenderer.SetPropertyBlock(s_matPropBlock_Atmosphere);
                    }

                    if (_fog != null) _fog.fogTint = Color.Lerp(_fogOrigTint, Color.black, collapseProgress);

                    if (_fogImpostor != null) _fogImpostor.material.SetColor(s_propID_Tint, Color.Lerp(_fogOrigTint, Color.black, collapseProgress));
                }
                else
                {
                    if (_shockLayer != null) _shockLayer.enabled = false;
                }
            }
            else
            {
                if (_shockLayer != null) _shockLayer.enabled = false;
            }
        }
    }
}
