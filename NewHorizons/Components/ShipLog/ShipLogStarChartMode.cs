using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using static NewHorizons.Utility.Files.AssetBundleUtilities;

namespace NewHorizons.Components.ShipLog
{
    public class ShipLogStarChartMode : ShipLogMode
    {
        private OWAudioSource _oneShotSource;
        private Font _fontToUse;

        private ScreenPromptList _centerPromptList;

        private ScreenPrompt _targetSystemPrompt;
        private ScreenPrompt _warpPrompt = new ScreenPrompt(InputLibrary.autopilot, "<CMD> Warp to system");

        private ShipLogStar _target = null;
        private ShipLogStar _thisStar = null;
        private ShipLogStar _switchStar = null;
        private NotificationData _warpNotificationData = null;

        private List<ShipLogStar> shipLogStars = new List<ShipLogStar>();

        public Vector2 cameraPosition;
        public float cameraRotation = 0;
        public float cameraZoom = 8;
        public Transform cameraPivot;
        private Transform _allStarsParent;
        private Transform _genericParent;
        private Transform _systemsParent;
        private RectTransform highlightCursor;
        private RectTransform visualWarpLine;

        private Color _elementColor = new Color(245f / 255f, 158f / 255f, 44f / 255f);

        private AudioClip _onOpenClip;
        private AudioClip _onSelectClip;
        private AudioClip _onDeselectClip;
        private float _volumeScale = 0.45f;

        private Texture2D _starTexture;
        private Texture2D _blackHoleTexture;
        private Texture2D _cursorTexture;
        private ShipLogEntryCard _card;
        private GameObject _cardTemplate = null;

        Vector3[] _galaxyStarPoints;

        private float _startPanTime;
        private Vector2 _startPanPos;
        private float _panDuration = 0.25f;

        public static readonly float orbitDistPercentile = 0.8f; // 80th percentile
        public static readonly float comparisonRadius = 2000f;
        public static readonly float lowestScale = 0.375f;
        public static readonly float highestScale = 2f;
        public static readonly float starMinimum = 0.099f;
        public static readonly float singularityMinimum = 0.199f;
        public static readonly float minVisualRadius = 0;
        public static readonly float maxVisualRadius = 40;
        public static readonly Color sunColor = new Color(2.302f, 0.8554f, 0.0562f, 1);

        private static readonly PlanetConfig SunConfig = new PlanetConfig
        {
            name = "Sun",
            starSystem = "SolarSystem",
            Base = new BaseModule
            {
                surfaceSize = 2000,
                surfaceGravity = 100,
                centerOfSolarSystem = true
            },
            Star = new StarModule
            {
                size = 2000,
                tint = MColor.FromColor(sunColor),
                solarLuminosity = 1,
                lifespan = 22
            }
        };

        private static readonly PlanetConfig EyeOfTheUniverseConfig = new PlanetConfig
        {
            name = "Eye of the Universe",
            starSystem = "EyeOfTheUniverse",
            Base = new BaseModule
            {
                surfaceSize = 300,
                surfaceGravity = 30,
                centerOfSolarSystem = true
            }
        };

        private void SetCard(string uniqueID)
        {
            _card.transform.localScale = new Vector3(1, 0, 1);

            Texture texture = null;
            try
            {
                if (uniqueID.Equals("SolarSystem"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/hearthian system.png");
                }
                else if (uniqueID.Equals("EyeOfTheUniverse"))
                {
                    texture = ImageUtilities.GetTexture(Main.Instance, "Assets/eye symbol.png");
                }
                else
                {
                    IModBehaviour mod = Main.SystemDict[uniqueID].Mod;

                    var path = Path.Combine("systems", uniqueID + ".png");

                    // Else check the old location
                    if (!File.Exists(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, path)))
                    {
                        path = Path.Combine("planets", uniqueID + ".png");
                    }

                    texture = ImageUtilities.GetTexture(mod, path);
                }
            }
            catch (Exception) { }

            if (texture != null)
            {
                _card._questionMark.gameObject.SetActive(false);
                _card._photo.gameObject.SetActive(true);
                _card._photo.sprite = MakeSprite((Texture2D)texture);
            } else
            {
                _card._questionMark.gameObject.SetActive(true);
                _card._photo.gameObject.SetActive(false);
            }
            _card._name.text = UniqueIDToName(uniqueID);
        }


        private void CreateCard()
        {
            if (_cardTemplate == null)
            {
                var panRoot = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot");
                _cardTemplate = Instantiate(panRoot.GetComponentInChildren<ShipLogEntryCard>(true).gameObject);
                _cardTemplate.SetActive(false);
            }

            GameObject newCard = Instantiate(_cardTemplate, this.transform);
            newCard.name = "Card";
            newCard.SetActive(true);
            newCard.transform.localPosition = new Vector3(360, -150, 0);

            _card = newCard.GetAddComponent<ShipLogEntryCard>();
            _card._moreToExploreIcon.gameObject.SetActive(false);
            _card._unreadIcon.gameObject.SetActive(false);
        }

        public void InitializeStars()
        {
            _allStarsParent = new GameObject("Stars").transform;
            _allStarsParent.transform.SetParent(transform, false);
            _allStarsParent.SetAsFirstSibling();
            ResetTransforms(_allStarsParent);

            _genericParent = new GameObject("Generic").transform;
            _genericParent.transform.SetParent(_allStarsParent, false);
            _genericParent.SetAsFirstSibling();
            ResetTransforms(_genericParent);

            _systemsParent = new GameObject("Systems").transform;
            _systemsParent.transform.SetParent(_allStarsParent, false);
            _systemsParent.SetAsLastSibling();
            ResetTransforms(_systemsParent);

            RawImage highlightCursorImage = AddVisualIndicator();
            highlightCursorImage.texture = _cursorTexture;
            highlightCursor = highlightCursorImage.gameObject.GetAddComponent<RectTransform>();

            RawImage visualWarpLineImage = AddVisualIndicator();
            visualWarpLine = visualWarpLineImage.gameObject.GetAddComponent<RectTransform>();
            visualWarpLine.transform.SetAsFirstSibling();

            _fontToUse = FindObjectOfType<ShipLogController>().GetComponentInChildren<Text>().font;

            cameraPivot = new GameObject("CameraPivot").transform;
            cameraPivot.transform.SetParent(transform, false);
            ResetTransforms(cameraPivot);
            cameraPivot.localEulerAngles = new Vector3(-5, 0, 0);

            CreateGalaxyStars();

            CreateSystemStars();
        }

        private void CreateGalaxyStars()
        {
            foreach (Vector3 point in _galaxyStarPoints) AddGenericStar(point);
        }

        private void CreateSystemStars()
        {
            foreach (var starSystem in Main.SystemDict.Keys)
            {
                bool thisSystem = Main.Instance.CurrentStarSystem == starSystem;
                if (StarChartHandler.CanWarpToSystem(starSystem) || thisSystem)
                {
                    AddStar(starSystem, thisSystem);
                }
            }
        }

        private void LoadAssets()
        {
            _onOpenClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/open star map.ogg");
            _onSelectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/select star.ogg");
            _onDeselectClip = NHPrivateAssetBundle.LoadAsset<AudioClip>("Assets/StarChart/Audio/deselect star.ogg");

            _cursorTexture = NHPrivateAssetBundle.LoadAsset<Texture2D>("Assets/StarChart/arrow.png");
            _starTexture = NHPrivateAssetBundle.LoadAsset<Texture2D>("Assets/StarChart/star.png");
            _blackHoleTexture = ImageUtilities.GetTexture(Main.Instance, "Assets/BlackHole.png");
        }


        private void UpdateWarpDriveVisuals()
        {
            Main.Instance.ShipWarpController.UpdateWarpDriveVisuals();
        }

        private RawImage AddVisualIndicator()
        {
            GameObject highlightCursorObject = new GameObject("VisualIndicator");
            highlightCursorObject.transform.SetParent(transform, false);
            ResetTransforms(highlightCursorObject.transform);
            RawImage image = highlightCursorObject.AddComponent<RawImage>();
            image.color = _elementColor;
            return image;
        }

        public static string GetStringID(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            var stringID = name.ToUpperInvariant().Replace(" ", "_").Replace("'", "").Replace(" ", "");
            if (stringID.Equals("TIMBER_MOON")) stringID = "ATTLEROCK";
            if (stringID.Equals("VOLCANIC_MOON")) stringID = "HOLLOWS_LANTERN";
            if (stringID.Equals("TOWER_TWIN")) stringID = "ASH_TWIN";
            if (stringID.Equals("CAVE_TWIN")) stringID = "EMBER_TWIN";
            if (stringID.Equals("COMET")) stringID = "INTERLOPER";
            if (stringID.Equals("EYE") || stringID.Equals("EYEOFTHEUNIVERSE")) stringID = "EYE_OF_THE_UNIVERSE";
            if (stringID.Equals("MAPSATELLITE")) stringID = "MAP_SATELLITE";
            if (stringID.Equals("INVISIBLE_PLANET")) stringID = "RINGWORLD";

            return stringID;
        }

        private Dictionary<string, MergedPlanetData> GetMergedBodies(IEnumerable<PlanetConfig> configs)
        {
            Dictionary<string, MergedPlanetData> lookup = new();
            foreach (var config in configs)
            {
                string id = GetStringID(config.name);
                if (!lookup.ContainsKey(id))
                {
                    lookup.Add(id, new MergedPlanetData { ID = id });
                }
                lookup[id].Merge(config);
            }
            return lookup;
        }

        private void SetupParentChildRelationships(Dictionary<string, MergedPlanetData> mergedBodies)
        {
            foreach (var data in mergedBodies.Values)
            {
                var primary = GetStringID(data.Orbit?.primaryBody);
                if (!string.IsNullOrEmpty(primary) && mergedBodies.TryGetValue(primary, out var parent))
                {
                    data.SetupParentChildRelationship(parent);
                }
            }
        }

        private float GetStarScale(StarModule star) => GetStarScale(star, false);

        private float GetStarScale(StarModule star, bool unconstrainedLowest)
        {
            return Mathf.Clamp(star.size / comparisonRadius, unconstrainedLowest ? 0 : lowestScale, highestScale);
        }

        private float GetSingularityScale(SingularityModule singularity) => GetSingularityScale(singularity, false);

        private float GetSingularityScale(SingularityModule singularity, bool unconstrainedLowest)
        {
            return Mathf.Clamp(singularity.horizonRadius / comparisonRadius, unconstrainedLowest ? 0 : lowestScale, highestScale);
        }

        private float GetSingularitiesScale(SingularityModule[] singularities)
        {
            return singularities.Max(GetSingularityScale);
        }

        private float GetSingularitiesScale(SingularityModule[] singularities, bool unconstrainedLowest)
        {
            return singularities.Max(singularity => GetSingularityScale(singularity, unconstrainedLowest));
        }

        private float GetRenderableScale(MergedPlanetData config)
        {
            if (IsRenderableStar(config))
                return GetStarScale(config.Star);

            if (IsRenderableSingularity(config))
                return GetSingularitiesScale(config.Singularities);

            return 0;
        }

        private Color GetRenderableColor(MergedPlanetData config)
        {
            if (IsRenderableStar(config))
                return StarTint(config.Star.tint);

            if (IsRenderableSingularity(config) && config.Singularities.First().type ==
                SingularityModule.SingularityType.BlackHole)
                return Color.black;

            return Color.white;
        }

        private float GetRenderableLifespan(MergedPlanetData config)
        {
            return IsRenderableStar(config) ? config.Star.lifespan : 0;
        }

        private bool IsRenderableStar(MergedPlanetData config)
        {
            return config.Star != null &&
                   GetStarScale(config.Star, true) >= starMinimum;
        }

        private bool IsRenderableSingularity(MergedPlanetData config)
        {
            var singularities = config.Singularities;
            return singularities != null &&
                   singularities.Length > 0 &&
                   GetSingularitiesScale(singularities, true) >= singularityMinimum;
        }

        private bool IsRenderableStarOrSingularity(MergedPlanetData config)
        {
            return IsRenderableStar(config) || IsRenderableSingularity(config);
        }

        private Vector3 GetStarPosition(StarSystemConfig.StarChartModule config)
        {
            return config?.position != null
                ? new Vector3(config.position.x, config.position.y, 0)
                : new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f), 0);
        }

        private void TryAddTextureFromPath(string customName, string texturePath, RawImage image)
        {
            var mod = Main.SystemDict[customName].Mod;
            string path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, texturePath);
            if (File.Exists(path))
                image.texture = ImageUtilities.GetTexture(mod, path);
        }

        private Color StarTint(MColor tint)
        {
            if (tint == null) return sunColor;
            var color = tint.ToColor();
            color.a = 1f;
            return color;
        }

        private GameObject AddVisualChildStar(Transform parent, Color color, float lifespan, Vector3 offset, float scale)
        {
            GameObject childStar = new GameObject("ChildStar");
            ShipLogChildStar newChildStar = childStar.AddComponent<ShipLogChildStar>();
            childStar.transform.SetParent(parent, false);
            ResetTransforms(childStar.transform);
            childStar.transform.localPosition = offset;

            childStar.AddComponent<CanvasRenderer>();
            var image = childStar.AddComponent<RawImage>();
            image.texture = color == Color.black ? _blackHoleTexture : _starTexture;
            image.color = color == Color.black ? Color.white : color;

            newChildStar._starTimeLoopEnd = lifespan;
            newChildStar._starScale = scale;
            newChildStar.Initialize(this);

            return childStar;
        }

        private GameObject AddVisualChildStar(Transform parent)
        {
            return AddVisualChildStar(parent, Color.white, 0, Vector3.zero, 1f);
        }

        private GameObject AddVisualChildStar(Transform parent, MergedPlanetData body, Vector3 offset)
        {
            if (!IsRenderableStarOrSingularity(body)) return null;

            float scale = GetRenderableScale(body);
            if (scale < (IsRenderableStar(body) ? starMinimum : singularityMinimum)) return null;

            Color color = GetRenderableColor(body);
            float lifespan = GetRenderableLifespan(body);

            var childStar = AddVisualChildStar(parent, color, lifespan, offset, scale);
            childStar.name = body.Name;
            return childStar;
        }

        internal void AddStar(string customName)
        {
            AddStar(customName, Main.Instance.CurrentStarSystem == customName);
        }

        private Vector2 FlattenTo2D(Vector3 pos3D)
        {
            // Flatten Z (forward) → Y (vertical)
            // Blend in Y (up) to give the orbit inclination some effect in 2D

            float x = pos3D.x + (pos3D.y * 0.3f);
            float y = pos3D.z + (pos3D.y * 0.7f);

            return new Vector2(x, y);
        }

        private static OrbitalParameters GetOrbitalParametersFromConfig(MergedPlanetData config)
        {
            if (config == null || config.Orbit == null) return null;

            var orbit = config.Orbit;

            var gravity = new Gravity(config.Base);
            var parentGravity = new Gravity(0, 2);

            if (config.Parent != null)
            {
                parentGravity = new Gravity(config.Parent.Base);
                if (config.Parent.FocalPoint != null)
                {
                    var focal = config.Parent.FocalPoint;
                    var primaryID = ShipLogStarChartMode.GetStringID(focal.primary);
                    var secondaryID = ShipLogStarChartMode.GetStringID(focal.secondary);

                    // Check if we're the primary or secondary body
                    bool isPrimary = config.ID == primaryID;
                    bool isSecondary = config.ID == secondaryID;

                    if (isPrimary || isSecondary)
                    {
                        // Grab both bodies
                        var primaryBody = config.Parent.Children.FirstOrDefault(b => b.ID == primaryID);
                        var secondaryBody = config.Parent.Children.FirstOrDefault(b => b.ID == secondaryID);

                        if (primaryBody != null && secondaryBody != null)
                        {
                            var primaryGravity = new Gravity(primaryBody.Base);
                            var secondaryGravity = new Gravity(secondaryBody.Base);

                            float m1 = primaryGravity.Mass;
                            float m2 = secondaryGravity.Mass;
                            float distance = primaryBody.Orbit.semiMajorAxis + secondaryBody.Orbit.semiMajorAxis;

                            float r1 = distance * m2 / (m1 + m2);
                            float r2 = distance * m1 / (m1 + m2);

                            float focalSemiMajorAxis = isPrimary ? r1 : r2;

                            var secondaryOrbit = secondaryBody.Orbit;

                            return OrbitalParameters.FromTrueAnomaly(
                                isPrimary ? primaryGravity : secondaryGravity,
                                isPrimary ? secondaryGravity : primaryGravity,
                                secondaryOrbit.eccentricity,
                                focalSemiMajorAxis,
                                secondaryOrbit.inclination,
                                secondaryOrbit.argumentOfPeriapsis + (isPrimary ? -180 : 0),
                                secondaryOrbit.longitudeOfAscendingNode,
                                secondaryOrbit.trueAnomaly
                            );
                        }
                    }
                }
            }

            return OrbitalParameters.FromTrueAnomaly(
                parentGravity,
                gravity,
                orbit.eccentricity,
                orbit.semiMajorAxis,
                orbit.inclination,
                orbit.argumentOfPeriapsis,
                orbit.longitudeOfAscendingNode,
                orbit.trueAnomaly
            );
        }

        private Vector3 GetOrbitVisualPosition(
            MergedPlanetData config,
            Vector3 centerOffset,
            float minRadius,
            float maxRadius,
            float minOrbitDist,
            float maxOrbitDist)
        {
            var orbit = config.Orbit;
            if (orbit == null) return centerOffset;

            // If static, just use position directly
            if (orbit.isStatic && orbit.staticPosition != null)
            {
                Vector3 staticPos = (Vector3)orbit.staticPosition;
                Vector3 flat = FlattenTo2D(staticPos);

                float dist = staticPos.magnitude;
                float t = Mathf.InverseLerp(minOrbitDist, maxOrbitDist, dist);
                float radius = Mathf.Lerp(minRadius, maxRadius, t);

                return centerOffset + flat.normalized * radius;
            }

            if (orbit.semiMajorAxis == 0f)
                return centerOffset;

            var op = GetOrbitalParametersFromConfig(config);

            Vector3 flatOrbit = FlattenTo2D(op.InitialPosition);
            float dist3D = op.InitialPosition.magnitude;

            float tOrbit = Mathf.InverseLerp(minOrbitDist, maxOrbitDist, dist3D);
            float radiusOrbit = Mathf.Lerp(minRadius, maxRadius, tOrbit);

            return centerOffset + (Vector3)(flatOrbit.normalized * radiusOrbit);
        }

        private Vector3 GetOrbitPosition(MergedPlanetData config)
        {
            var orbit = config.Orbit;
            if (orbit == null) return Vector3.zero;

            // Static position override
            if (orbit.isStatic && orbit.staticPosition != null && orbit.staticPosition.Length() > 0)
                return orbit.staticPosition;

            if (orbit.semiMajorAxis == 0)
                return Vector3.zero;

            // Use orbital parameters for dynamic position
            var op = GetOrbitalParametersFromConfig(config);
            return op.InitialPosition;
        }

        private float GetOrbitDistance(MergedPlanetData config)
        {
            return GetOrbitPosition(config).magnitude;
        }

        private Vector3 GetTotalOrbitPosition(MergedPlanetData body)
        {
            if (body == null) return Vector3.zero;

            Vector3 currentPosition = GetOrbitPosition(body);
            if (body.Parent == null) return currentPosition;

            Vector3 parentPosition = GetTotalOrbitPosition(body.Parent);
            return parentPosition + currentPosition;
        }

        private float GetTotalOrbitDistance(MergedPlanetData body)
        {
            return GetTotalOrbitPosition(body).magnitude;
        }

        private float GetMaxOrbitDistanceFrom(MergedPlanetData body)
        {
            List<float> distances = new();

            void CollectDistances(MergedPlanetData current)
            {
                float dist = GetTotalOrbitDistance(current);
                if (dist > 0) distances.Add(dist);

                foreach (var child in current.Children)
                {
                    CollectDistances(child);
                }
            }

            CollectDistances(body);

            if (distances.Count == 0) return 0f;

            distances.Sort();

            int index = Mathf.FloorToInt(distances.Count * orbitDistPercentile);
            index = Mathf.Clamp(index, 0, distances.Count - 1);

            // If it's the last and there’s another one, use second-to-last
            if (index == distances.Count - 1 && distances.Count > 2)
            {
                index = distances.Count - 2;
            }

            return distances[index];
        }

        private void AddStar(string customName, bool isThisSystem)
        {
            try
            {
            var config = Main.SystemDict[customName].Config.StarChart;
            var bodies = Main.BodyDict[customName].Select(b => b.Config);
            if (customName == "SolarSystem") bodies = bodies.Prepend(SunConfig);
            else if (customName == "EyeOfTheUniverse") bodies = bodies.Prepend(EyeOfTheUniverseConfig);

            Dictionary<string, MergedPlanetData> mergedBodies = GetMergedBodies(bodies);
            var mergedList = mergedBodies.Values.ToList();

            // The seed for any default (random) fields of a custom star is based on the hash of their unique name (plus ten, bc why not).
            UnityEngine.Random.InitState(customName.GetHashCode() + 10);

            GameObject newStarObject = new GameObject("StarGroup");
            ShipLogStar newStar = newStarObject.AddComponent<ShipLogStar>();
            newStar.transform.SetParent(_systemsParent, false);

            newStarObject.AddComponent<CanvasRenderer>();
            RawImage starImage = newStarObject.AddComponent<RawImage>();
            starImage.texture = _starTexture;
            ResetTransforms(newStar.transform);

            newStar._starPosition = GetStarPosition(config);
            newStar._starScale = 0.6f;
            newStar._starName = customName;
            newStar._isWarpSystem = true;
            newStarObject.name = customName;

            bool hasColor = config?.color != null;
            bool hasTexture = config?.starTexturePath != null;

            float labelYOffset = 0;

            // Use manual color/texture if provided
            if (hasColor || hasTexture)
            {
                starImage.color = hasColor ? config.color.ToColor() : Color.white;

                if (hasTexture)
                {
                    TryAddTextureFromPath(customName, config.starTexturePath, starImage);
                }

                newStar._starTimeLoopEnd = config?.disappearanceTime ?? 0;
            }
            else
            {
                // No explicit texture/color: infer from center and children
                starImage.enabled = false; // No root visual

                GameObject visualGroupObj = new GameObject("VisualGroup");
                Transform visualGroup = visualGroupObj.transform;
                visualGroup.SetParent(newStarObject.transform);
                ResetTransforms(visualGroup);

                var center = mergedList.FirstOrDefault(b => b.IsCenter);
                if (center == null)
                {
                    AddVisualChildStar(newStarObject.transform);
                    newStar._starTimeLoopEnd = 0;
                }
                else
                {
                    float maxLifespan = 0;

                    SetupParentChildRelationships(mergedBodies);

                    var staticRootless = mergedList
                        .Where(b => !b.IsCenter && b.Orbit?.isStatic == true && string.IsNullOrEmpty(b.Orbit.primaryBody))
                        .ToList();

                    foreach (var rootlessBody in staticRootless)
                    {
                        rootlessBody.SetupParentChildRelationship(center);
                    }

                    float maxOrbitDist = Mathf.Max(
                        GetMaxOrbitDistanceFrom(center),
                        comparisonRadius
                    );

                    void TraverseFromCenter(MergedPlanetData center)
                    {
                        Queue<(MergedPlanetData config, Vector3 offset)> queue = new();
                        queue.Enqueue((center, Vector3.zero));

                        while (queue.Count > 0)
                        {
                            var (current, offset) = queue.Dequeue();

                            // Determine if this is a valid star or singularity
                            bool isStar = IsRenderableStar(current);
                            bool isSingularity = IsRenderableSingularity(current);

                            if (isStar && maxLifespan >= 0)
                            {
                                maxLifespan = Mathf.Max(maxLifespan, current.Star.lifespan);
                            }

                            if (isSingularity)
                            {
                                maxLifespan = -1;
                            }

                            // Place visual
                            if (isStar || isSingularity)
                            {
                                AddVisualChildStar(visualGroup, current, offset);
                            }
                            /*
                            else
                            {
                                GameObject empty = new GameObject(current.Name);
                                empty.transform.SetParent(visualGroup, false);
                                ResetTransforms(empty.transform);
                                empty.transform.localPosition = offset;
                            }
                            */

                            // Enqueue children for next layer
                            var children = current.Children;

                            // Handle focal point logic
                            var isFocal = current.FocalPoint != null;
                            if (isFocal)
                            {
                                // Remove primary and secondary from children list (they are added manually)
                                children = children
                                    .Where(c => c.ID != GetStringID(current.FocalPoint.primary) && c.ID != GetStringID(current.FocalPoint.secondary))
                                    .ToList();

                                if (mergedBodies.TryGetValue(GetStringID(current.FocalPoint.primary), out MergedPlanetData primary) && 
                                    mergedBodies.TryGetValue(GetStringID(current.FocalPoint.secondary), out MergedPlanetData secondary))
                                {
                                    Vector3 primaryOffset = GetOrbitVisualPosition(
                                        primary, offset,
                                        minVisualRadius, maxVisualRadius,
                                        0, maxOrbitDist);

                                    Vector3 secondaryOffset = GetOrbitVisualPosition(
                                        secondary, offset,
                                        minVisualRadius, maxVisualRadius,
                                        0, maxOrbitDist);

                                    queue.Enqueue((primary, primaryOffset));
                                    queue.Enqueue((secondary, secondaryOffset));
                                }
                            }

                            // Sort all remaining children by orbital distance
                            children = children.OrderBy(GetOrbitDistance).ToList();

                            foreach (var child in children)
                            {
                                bool isZero = GetOrbitDistance(child) == 0;

                                var childOffset = isZero ? offset
                                    : GetOrbitVisualPosition(child, offset, minVisualRadius, maxVisualRadius, 0, maxOrbitDist);

                                queue.Enqueue((child, childOffset));
                            }
                        }
                    }

                    TraverseFromCenter(center);

                    var childVisuals = visualGroupObj.GetComponentsInChildren<ShipLogChildStar>(true).ToList();

                    // After placement: Shift text label higher based on how far above 20 the highest visual goes
                    if (childVisuals.Count > 0)
                    {
                        float highestY = childVisuals.Max(r => r.transform.localPosition.y);
                        if (highestY >= 20)
                        {
                            labelYOffset = highestY - 20;
                        }
                    }

                    // No valid children found: fallback
                    if (childVisuals.Count == 0)
                    {
                        AddVisualChildStar(visualGroup);
                        newStar._starTimeLoopEnd = 0;
                    }
                    else
                    {
                        newStar._starTimeLoopEnd = Mathf.Max(maxLifespan, 0);
                    }
                }
            }

            if (!isThisSystem) shipLogStars.Add(newStar);
            else
            {
                _thisStar = newStar;
                cameraPosition = new Vector2(newStar._starPosition.x, newStar._starPosition.y);
            }

            var textLabel = AddTextLabel(newStarObject.transform, UniqueIDToName(customName));
            if (labelYOffset > 0 && textLabel != null)
            {
                textLabel.transform.localPosition += new Vector3(0, labelYOffset, 0);
            }

            newStar.enabled = true;
            newStar.Initialize(this);
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Failed to add system [{customName}] to Ship Log Star Chart: {e}");
            }
        }

        public void AddGenericStar(Vector3 inputPosition)
        {
            GameObject newStarObject = new GameObject("Star");
            ShipLogStar newStar = newStarObject.AddComponent<ShipLogStar>();
            newStar.transform.SetParent(_genericParent, false);

            newStarObject.AddComponent<CanvasRenderer>();
            RawImage starImage = newStarObject.AddComponent<RawImage>();
            starImage.texture = _starTexture;
            ResetTransforms(newStar.transform);

            starImage.color = RandomStarColor();
            newStar._starTimeLoopEnd = UnityEngine.Random.Range(0f, 23f);
            newStar._starScale = UnityEngine.Random.Range(0.03f, 0.2f);
            newStar._starPosition = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-5f, 5f));

            newStar._starPosition = inputPosition;

            newStar.enabled = true;

            newStar.Initialize(this);
        }

        private Color RandomStarColor()
        {
            Color blueYellowColor = Color.Lerp(Color.blue, Color.yellow, UnityEngine.Random.Range(0f, 1f));
            Color darkLightColor = Color.Lerp(blueYellowColor, Color.white, UnityEngine.Random.Range(0.8f, 1f));
            return darkLightColor;
        }

        private Text AddTextLabel(Transform parent, string Text)
        {
            GameObject textObject = new GameObject("TextLabel");
            textObject.transform.SetParent(parent, false);
            ResetTransforms(textObject.transform);
            textObject.transform.localScale = Vector3.one * 2;

            textObject.AddComponent<CanvasRenderer>();
            Text text = textObject.AddComponent<Text>();
            RectTransform rect = textObject.GetAddComponent<RectTransform>();

            text.alignment = TextAnchor.UpperCenter;
            text.text = Text;
            text.font = _fontToUse;
            text.fontSize = 12;
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = 14;
            text.resizeTextMinSize = 10;

            return text;
        }

        private void ResetTransforms(Transform t)
        {
            t.transform.localPosition = Vector3.zero;
            t.transform.localScale = Vector3.one;
            t.transform.localRotation = Quaternion.identity;
        }


        private void Awake()
        {
            // Prompts
            Locator.GetPromptManager().AddScreenPrompt(_warpPrompt, PromptPosition.UpperLeft, false);
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            _galaxyStarPoints = CreateGalaxy();
            CreateCard();
            LoadAssets();
            InitializeStars();
            _oneShotSource = oneShotSource;
            _centerPromptList = centerPromptList;
            _targetSystemPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, TranslationHandler.GetTranslation("LOCK_AUTOPILOT_WARP", TranslationHandler.TextType.UI), 0, ScreenPrompt.DisplayState.Normal, false);
            GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));
        }

        public void OnDestroy()
        {
            GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", new Callback<ReferenceFrame>(OnTargetReferenceFrame));

            Locator.GetPromptManager()?.RemoveScreenPrompt(_warpPrompt, PromptPosition.UpperLeft);
        }

        public override bool AllowCancelInput()
        {
            return true;
        }

        public override bool AllowModeSwap()
        {
            return true;
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            gameObject.SetActive(true);
            _oneShotSource.PlayOneShot(_onOpenClip, _volumeScale);
            Locator.GetPromptManager().AddScreenPrompt(_targetSystemPrompt, _centerPromptList, TextAnchor.MiddleCenter, -1, false);
        }

        public override void ExitMode()
        {
            cameraPosition = new Vector2(_thisStar._starPosition.x, _thisStar._starPosition.y);
            cameraRotation = 0;
            cameraZoom = 8;
            cameraPivot.localEulerAngles = new Vector3(-5, 0, 0);
            cameraPivot.localScale = Vector3.zero;
            Locator.GetPromptManager().RemoveScreenPrompt(_targetSystemPrompt);
            gameObject.SetActive(false);
        }

        public override string GetFocusedEntryID()
        {
            return "";
        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public override void UpdateMode()
        {
            UpdateMapCamera();
            UpdateSelection();
            UpdatePrompts();
        }

        private void UpdateMapCamera()
        {
            if (shipLogStars.Count == 0)
            {
                NHLogger.LogWarning("Showing star chart mode when there are no available systems");
                return;
            }

            cameraPivot.transform.localScale = Vector3.Lerp(cameraPivot.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 2);
            if (_target != null && _card != null)
            {
                _card.transform.localScale = Vector3.Lerp(_card.transform.localScale, Vector3.one * 1.25f, Time.unscaledDeltaTime * 20);
            } else
            {
                _card.transform.localScale = Vector3.Lerp(_card.transform.localScale, Vector3.zero, Time.unscaledDeltaTime * 20);
            }

            float zoom = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
            cameraZoom *= 1 + (zoom * Time.unscaledDeltaTime * 2);
            cameraZoom = Mathf.Clamp(cameraZoom, 1f, 10f);

            float deltaTime = Time.unscaledDeltaTime;
            float adjustedDeltaTime = InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? (1f / 60f) : deltaTime;

            Vector2 look = OWInput.GetAxisValue(InputLibrary.look, InputMode.All);
            float rotationSensitivity = OWInput.UsingGamepad() ? PlayerCameraController.GAMEPAD_LOOK_RATE_Y : PlayerCameraController.LOOK_RATE;
            float delta = look.x * -rotationSensitivity * adjustedDeltaTime;
            cameraRotation = ((cameraRotation + delta) % 360f + 360f) % 360f;

            float rad = cameraRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            float x = -5f * cos;
            float y = -5f * sin;
            cameraPivot.localRotation = Quaternion.Euler(x, y, cameraRotation);
            
            Vector2 moveInput = OWInput.GetAxisValue(InputLibrary.moveXZ);
            Vector2 rotatedInput = new Vector2(
                moveInput.x * cos - moveInput.y * sin,
                moveInput.x * sin + moveInput.y * cos
            );
            cameraPosition += (rotatedInput * Time.unscaledDeltaTime * 500) / cameraZoom;

            if (_target != null && Time.unscaledTime < _startPanTime + _panDuration)
            {
                float pan = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_startPanTime, _startPanTime + _panDuration, Time.unscaledTime));
                cameraPosition = Vector2.Lerp(_startPanPos, _target._starPosition, pan);
            }

            _card._nameBackground.color = _elementColor;
            _card._border.color = _elementColor;
            _card._hudMarkerIcon.gameObject.SetActive(true);
        }

        private void UpdateSelection()
        {
            float minimumDistance = Mathf.Infinity;
            ShipLogStar highlightedStar = null;
            foreach(ShipLogStar s in shipLogStars)
            {
                float distance = Vector3.Distance(s.transform.localPosition, Vector3.zero);
                if (distance < minimumDistance && distance < 200 && s.gameObject.activeSelf)
                {
                    minimumDistance = distance;
                    highlightedStar = s;
                }
            }

            if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD, InputMode.All))
            {
                if (highlightedStar != null && _target != highlightedStar)
                {
                    SetWarpTarget(highlightedStar);
                } else
                {
                    RemoveWarpTarget(true);
                }
            }


            if (highlightCursor != null)
            {
                if (highlightedStar != null && _target != highlightedStar)
                {
                    _targetSystemPrompt.SetVisibility(true);
                    highlightCursor.localPosition = highlightedStar.transform.localPosition + (Vector3.down * 30);
                    highlightCursor.localScale = Vector3.one * 0.25f;

                    if (_switchStar != highlightedStar) _oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);

                    _switchStar = highlightedStar;
                } else
                {
                    _targetSystemPrompt.SetVisibility(false);
                    highlightCursor.localScale = Vector3.zero;
                    _switchStar = null;
                }
            } else
            {
                _switchStar = null;
            }

            if (visualWarpLine != null)
            {
                if (_target != null)
                {
                    SetWarpLinePositions(_thisStar.transform.localPosition, _target.transform.localPosition);
                } else
                {
                    SetWarpLinePositions(_thisStar.transform.localPosition, Vector3.zero);
                }
            }

            if (_target != null && !_target.gameObject.activeSelf) RemoveWarpTarget(false);
        }

        private void SetWarpLinePositions(Vector3 pos1, Vector3 pos2)
        {
            visualWarpLine.localPosition = Vector3.Lerp(pos1, pos2, 0.5f);
            visualWarpLine.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(pos1.x - pos2.x, pos1.y - pos2.y) * (180 / Mathf.PI) * -1);
            visualWarpLine.localScale = new Vector3(0.01f, Vector3.Distance(pos1, pos2) * 0.01f, 1f);
        }

        private void UpdatePrompts()
        {
        }

        public static string UniqueIDToName(string uniqueID)
        {
            var name = TranslationHandler.GetTranslation(uniqueID, TranslationHandler.TextType.UI);

            // If it can't find a translation it just returns the key
            if (!name.Equals(uniqueID)) return name;

            // Else we return a default name
            if (uniqueID.Equals("SolarSystem")) return TranslationHandler.GetTranslation("The Outer Wilds", TranslationHandler.TextType.UI);
            if (uniqueID.Equals("EyeOfTheUniverse")) return UITextLibrary.GetString(UITextType.LocationEye);

            var splitString = uniqueID.Split('.');
            if (splitString.Length > 1) splitString = splitString.Skip(1).ToArray();
            name = string.Join("", splitString).SplitCamelCase();
            return name;
        }

        private int Posmod(int a, int b)
        {
            return (a % b + b) % b;
        }

        private Sprite MakeSprite(Texture2D texture)
        {
            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(texture.width / 2, texture.height / 2);
            return Sprite.Create(texture, rect, pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        }

        private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
        {
            RemoveWarpTarget();
        }

        private void SetWarpTarget(ShipLogStar starTarget)
        {
            RemoveWarpTarget(false);
            if (starTarget != null)
            {
                SetCard(starTarget._starName);
            }
            _oneShotSource.PlayOneShot(_onSelectClip, _volumeScale);
            _target = starTarget;
            _startPanTime = Time.unscaledTime;
            _startPanPos = cameraPosition;
            Locator._rfTracker.UntargetReferenceFrame();
            GlobalMessenger.FireEvent("UntargetReferenceFrame");

            var name = UniqueIDToName(starTarget.name);

            var warpNotificationDataText = TranslationHandler.GetTranslation("WARP_LOCKED", TranslationHandler.TextType.UI).Replace("{0}", name.ToUpperFixed());
            _warpNotificationData = new NotificationData(warpNotificationDataText);
            NotificationManager.SharedInstance.PostNotification(_warpNotificationData, true);

            var warpPromptText = "<CMD> " + TranslationHandler.GetTranslation("ENGAGE_WARP_PROMPT", TranslationHandler.TextType.UI).Replace("{0}", name);
            _warpPrompt.SetText(warpPromptText);
            UpdateWarpDriveVisuals();
        }

        private void RemoveWarpTarget(bool playSound = false)
        {
            if (_warpNotificationData != null) NotificationManager.SharedInstance.UnpinNotification(_warpNotificationData);
            if (_target == null) return;
            if (playSound) _oneShotSource.PlayOneShot(_onDeselectClip, _volumeScale);
            _target = null;
            _startPanTime = 0;
            UpdateWarpDriveVisuals();
        }

        public string GetTargetStarSystem()
        {
            return _target?.name;
        }

        private bool IsWarpDriveAvailable()
        {
            return OWInput.IsInputMode(InputMode.ShipCockpit) && _target != null;
        }

        public void UpdateWarpPromptVisibility()
        {
            _warpPrompt.SetVisibility(IsWarpDriveAvailable());
        }




        // Galaxy Visuals
        private Vector3[] CreateGalaxy()
        {
            List<Vector3> galaxyStarPoints = new List<Vector3>();

            UnityEngine.Random.InitState(1);
            int arms = 7;
            for (int i = 0; i < arms; i++)
            {
               CreateGalaxyArm(
                   galaxyStarPoints,
                   360*((float)i/(float)arms),
                   12, 1,
                   60, 200,
                   UnityEngine.Random.Range(2f, 3f),
                   24
                   );
            }

            return galaxyStarPoints.ToArray();
        }

        private void CreateGalaxyArm(List<Vector3> list, float startingAngle, float startDensity, float endDensity, float startScatter, float endScatter, float armLength, int bunchCount)
        {
            Vector3 moveDirection = Quaternion.AngleAxis(startingAngle, Vector3.forward) * Vector3.up;
            Vector3 point = moveDirection * 5;
            float RotateDirection = 10f;
            for (int i = 0; i < bunchCount; i++)
            {
                moveDirection = Quaternion.AngleAxis(RotateDirection, Vector3.forward) * moveDirection;

                float t = (float)i / (float)bunchCount;
                float scatter = Mathf.Lerp(startScatter, endScatter, t);
                float density = Mathf.Lerp(startDensity, endDensity, t);

                for (int s = 0; s < (int)density; s++)
                {
                    Vector3 scatterOffset = RandomStarOffsetVector(scatter);
                    list.Add(point + scatterOffset);
                }

                point += moveDirection * 30 * armLength;
            }
        }
        private Vector3 RandomStarOffsetVector(float Range)
        {
            return new Vector3(RandomStarRange(Range), RandomStarRange(Range), RandomStarRange(Range) * 0.03f);
        }
        private float RandomStarRange(float Range)
        {
            return UnityEngine.Random.Range(Range * -1, Range);
        }

        private class MergedPlanetData
        {
            public string ID;
            public bool IsCenter;
            public PlanetConfig PrimaryConfig;
            public List<PlanetConfig> AllConfigs = new();

            private BaseModule _base;
            private OrbitModule _orbit;
            private FocalPointModule _focalPoint;
            private StarModule _star;
            private SingularityModule[] _singularities;

            public string Name => PrimaryConfig.name;
            public BaseModule Base => _base;
            public OrbitModule Orbit => _orbit;
            public FocalPointModule FocalPoint => _focalPoint;
            public StarModule Star => _star;
            public SingularityModule[] Singularities => _singularities;

            public MergedPlanetData Parent { get; internal set; }
            public List<MergedPlanetData> Children { get; } = new();

            public void SetupParentChildRelationship(MergedPlanetData parent)
            {
                this.Parent = parent;
                parent.Children.SafeAdd(this);
            }

            public bool ValidBase => HasValidBase(Base);
            public bool ValidOrbit => HasValidOrbit(Orbit);

            public void Merge(PlanetConfig config)
            {
                AllConfigs.Add(config);

                if (PrimaryConfig == null || config.Base?.centerOfSolarSystem == true)
                {
                    PrimaryConfig = config;
                }

                if (config.Base?.centerOfSolarSystem == true)
                {
                    IsCenter = true;
                }

                RefreshResolvedValues();
            }

            private void RefreshResolvedValues()
            {
                _base = null;
                _orbit = null;
                _focalPoint = null;
                _star = null;
                _singularities = null;

                // Base: prefer last with valid base data
                _base = AllConfigs.LastOrDefault(HasValidBase)?.Base
                    ?? PrimaryConfig?.Base;

                // Orbit: prefer last with valid orbit data
                _orbit = AllConfigs.LastOrDefault(HasValidOrbit)?.Orbit
                    ?? PrimaryConfig?.Orbit;

                // FocalPoint: prefer primary, then fallback
                _focalPoint = PrimaryConfig?.FocalPoint
                    ?? AllConfigs.Select(c => c.FocalPoint).FirstOrDefault(fp => fp != null);

                // Star: prefer primary, then fallback
                _star = PrimaryConfig?.Star
                    ?? AllConfigs.Select(c => c.Star).FirstOrDefault(s => s != null);

                // Singularities: merge all from configs
                _singularities = AllConfigs
                    .Where(c => c.Props?.singularities != null)
                    .SelectMany(c => c.Props.singularities)
                    .ToArray();
            }

            private static bool HasValidBase(PlanetConfig config)
            {
                var baseModule = config.Base;
                return HasValidBase(baseModule);
            }

            private static bool HasValidBase(BaseModule baseModule)
            {
                return baseModule != null &&
                       baseModule.surfaceGravity > 0 &&
                       baseModule.surfaceSize > 0;
            }

            private static bool HasValidOrbit(PlanetConfig config)
            {
                var orbit = config.Orbit;
                return HasValidOrbit(orbit);
            }

            private static bool HasValidOrbit(OrbitModule orbit)
            {
                return orbit != null && (
                    orbit.semiMajorAxis > 0f ||
                    (orbit.isStatic && orbit.staticPosition != null)
                );
            }
        }
    }
}
