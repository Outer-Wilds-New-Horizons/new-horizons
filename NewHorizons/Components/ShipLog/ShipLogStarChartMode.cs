using Epic.OnlineServices.Stats;
using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.External.Configs;
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
using System.Security.AccessControl;
using System.Xml;
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

        public static readonly float comparisonRadius = 2000f;
        public static readonly float lowestScale = 0.375f;
        public static readonly float highestScale = 2f;
        public static readonly float starMinimum = 0.099f;
        public static readonly float singularityMinimum = 0.199f;
        public static readonly float focalRadiusMultiplier = 1.6f;
        public static readonly float focalChildRadiusMultiplier = focalRadiusMultiplier * focalRadiusMultiplier;
        public static readonly Color sunColor = new Color(2.302f, 0.8554f, 0.0562f, 1);

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
            _allStarsParent.parent = this.transform;
            _allStarsParent.SetAsFirstSibling();
            ResetTransforms(_allStarsParent);

            _genericParent = new GameObject("Generic").transform;
            _genericParent.parent = _allStarsParent;
            _genericParent.SetAsFirstSibling();
            ResetTransforms(_genericParent);

            _systemsParent = new GameObject("Systems").transform;
            _systemsParent.parent = _allStarsParent;
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
            cameraPivot.parent = this.transform;
            ResetTransforms(cameraPivot);
            cameraPivot.localEulerAngles = new Vector3(-5, 0, 0);

            foreach (Vector3 point in _galaxyStarPoints) AddGenericStar(point);

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
            highlightCursorObject.transform.parent = this.transform;
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

        private List<PlanetConfig> GetChildrenOf(string parentName, List<NewHorizonsBody> bodies)
        {
            return bodies
                .Where(b => GetStringID(b.Config.Orbit?.primaryBody) == GetStringID(parentName))
                .Select(b => b.Config)
                .ToList();
        }

        private float GetRenderableScale(PlanetConfig config)
        {
            if (IsRenderableStar(config))
                return Mathf.Clamp(config.Star.size / comparisonRadius, lowestScale, highestScale);

            if (IsRenderableSingularity(config))
                return Mathf.Clamp(config.Props.singularities.Max(s => s.horizonRadius) / comparisonRadius, lowestScale, highestScale);

            return 0;
        }

        private Color GetRenderableColor(PlanetConfig config)
        {
            if (IsRenderableStar(config))
                return StarTint(config.Star.tint);

            if (IsRenderableSingularity(config) && config.Props.singularities.First().type ==
                External.Modules.VariableSize.SingularityModule.SingularityType.BlackHole)
                return Color.black;

            return Color.white;
        }

        private float GetRenderableLifespan(PlanetConfig config)
        {
            return IsRenderableStar(config) ? config.Star.lifespan : 0;
        }

        private bool IsRenderableStar(PlanetConfig config)
        {
            return config.Star != null &&
                   Mathf.Clamp(config.Star.size / comparisonRadius, 0, highestScale) >= starMinimum;
        }

        private bool IsRenderableSingularity(PlanetConfig config)
        {
            var singularities = config.Props?.singularities;
            return singularities != null &&
                   singularities.Length > 0 &&
                   Mathf.Clamp(singularities.Max(s => s.horizonRadius) / comparisonRadius, 0, highestScale) >= singularityMinimum;
        }

        private bool IsRenderableStarOrSingularity(PlanetConfig config)
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
            childStar.transform.parent = parent;
            ResetTransforms(childStar.transform);
            childStar.transform.localPosition = offset;

            var image = childStar.AddComponent<RawImage>();
            image.texture = color == Color.black ? _blackHoleTexture : _starTexture;
            image.color = color == Color.black ? Color.white : color;

            childStar.AddComponent<CanvasRenderer>();

            newChildStar._starTimeLoopEnd = lifespan;
            newChildStar._starScale = scale;
            newChildStar.Initialize(this);

            return childStar;
        }

        private GameObject AddVisualChildStar(Transform parent)
        {
            return AddVisualChildStar(parent, Color.white, 0, Vector3.zero, 1f);
        }

        private GameObject AddVisualChildStar(Transform parent, PlanetConfig body, Vector3 offset)
        {
            if (!IsRenderableStarOrSingularity(body)) return null;

            float scale = GetRenderableScale(body);
            if (scale < (IsRenderableStar(body) ? starMinimum : singularityMinimum)) return null;

            Color color = GetRenderableColor(body);
            float lifespan = GetRenderableLifespan(body);

            var childStar = AddVisualChildStar(parent, color, lifespan, offset, scale);
            childStar.name = body.name;
            return childStar;
        }

        internal void AddStar(string customName)
        {
            AddStar(customName, Main.Instance.CurrentStarSystem == customName);
        }

        private static (Vector3 primaryOffset, Vector3 secondaryOffset) GetFocalOffsets(Vector3 origin, float distance = 10)
        {
            // Get perpendicular direction based on offset
            Vector2 raw = new Vector2(origin.x, origin.y);
            Vector2 dir = raw == Vector2.zero ? Vector2.down : raw.normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);

            Vector3 primaryOffset = (Vector3)(perp * -distance);
            Vector3 secondaryOffset = (Vector3)(perp * distance);

            return (primaryOffset, secondaryOffset);
        }

        private (Vector3 primaryOffset, Vector3 secondaryOffset) GetAdjustedFocalOffsets(Vector3 origin, PlanetConfig primary, PlanetConfig secondary)
        {
            const float maxDistance = 12f;   // distance for large stars
            const float baseDistance = 8f;   // distance for normal stars
            const float minDistance = 4f;    // distance for tiny stars

            const float largeScaleThreshold = 1.5f; // scale above this is "large"
            const float tinyScaleThreshold = 0.5f;  // scale below this is "tiny"

            float scale1 = GetRenderableScale(primary);
            float scale2 = GetRenderableScale(secondary);
            float avgScale = (scale1 + scale2) / 2f;

            float distance = minDistance;

            // Only adjust if both are renderable
            if (IsRenderableStarOrSingularity(primary) && IsRenderableStarOrSingularity(secondary))
            {
                if (avgScale < tinyScaleThreshold)
                {
                    // Tiny stars: interpolate between minDistance and baseDistance
                    float t = avgScale / tinyScaleThreshold;
                    distance = Mathf.Lerp(minDistance, baseDistance, Mathf.Clamp01(t));
                }
                else if (avgScale > largeScaleThreshold)
                {
                    // Large stars: interpolate between baseDistance and maxDistance
                    float t = (avgScale - largeScaleThreshold) / (highestScale - largeScaleThreshold);
                    distance = Mathf.Lerp(baseDistance, maxDistance, Mathf.Clamp01(t));
                }
                else
                {
                    // Normal range (between tiny and large)
                    distance = baseDistance;
                }
            }

            return GetFocalOffsets(origin, distance);
        }

        private static readonly PlanetConfig Sun = new PlanetConfig
        {
            name = "Sun",
            Base = new External.Modules.BaseModule
            {
                centerOfSolarSystem = true
            },
            Star = new External.Modules.VariableSize.StarModule
            {
                size = 2000,
                tint = MColor.FromColor(sunColor),
                lifespan = 22
            }
        };

        private Vector2 FlattenTo2D(Vector3 pos3D)
        {
            // Flatten Z (forward) → Y (vertical)
            // Blend in Y (up) to give the orbit inclination some effect in 2D

            float x = pos3D.x + (pos3D.y * 0.3f);
            float y = pos3D.z + (pos3D.y * 0.7f);

            return new Vector2(x, y);
        }

        private Vector3 GetOrbitVisualPosition(
            PlanetConfig config,
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

            var op = OrbitalParameters.FromTrueAnomaly(
                new Gravity(0, 2),
                new Gravity(0, 2),
                orbit.eccentricity,
                orbit.semiMajorAxis,
                orbit.inclination,
                orbit.argumentOfPeriapsis,
                orbit.longitudeOfAscendingNode,
                orbit.trueAnomaly
            );

            Vector3 flatOrbit = FlattenTo2D(op.InitialPosition);
            float dist3D = op.InitialPosition.magnitude;

            float tOrbit = Mathf.InverseLerp(minOrbitDist, maxOrbitDist, dist3D);
            float radiusOrbit = Mathf.Lerp(minRadius, maxRadius, tOrbit);

            return centerOffset + (Vector3)(flatOrbit.normalized * radiusOrbit);
        }

        private float GetOrbitDistance(PlanetConfig c)
        {
            var orbit = c.Orbit;
            if (orbit == null) return 0f;

            float semi = orbit.semiMajorAxis;
            float stat = (orbit.staticPosition?.Length() ?? 0);
            return Mathf.Max(semi, stat);
        }

        private void AddStar(string customName, bool isThisSystem)
        {
            var config = Main.SystemDict[customName].Config.StarChart;
            var bodies = Main.BodyDict[customName];

            // The seed for any default (random) fields of a custom star is based on the hash of their unique name (plus ten, bc why not).
            UnityEngine.Random.InitState(customName.GetHashCode() + 10);

            GameObject newStarObject = new GameObject("StarGroup");
            ShipLogStar newStar = newStarObject.AddComponent<ShipLogStar>();
            newStar.transform.parent = _systemsParent;

            RawImage starImage = newStarObject.AddComponent<RawImage>();
            starImage.texture = _starTexture;
            newStarObject.AddComponent<CanvasRenderer>();
            ResetTransforms(newStar.transform);

            newStar._starPosition = GetStarPosition(config);
            newStar._starScale = 0.6f;
            newStar._starName = customName;
            newStarObject.name = customName;

            bool hasColor = config?.color != null;
            bool hasTexture = config?.starTexturePath != null;

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

                // Assign this to something accessible if needed:
                newStar.visualGroup = visualGroup;

                var center = bodies.FirstOrDefault(b => b.Config.Base?.centerOfSolarSystem == true)?.Config ?? (customName == "SolarSystem" ? Sun : null);
                if (center == null)
                {
                    AddVisualChildStar(newStarObject.transform);
                    newStar._starTimeLoopEnd = 0;
                }
                else
                {
                    float maxLifespan = 0;

                    var staticRootless = bodies
                        .Where(b => b.Config.Base?.centerOfSolarSystem != true && b.Config.Orbit?.isStatic == true && string.IsNullOrEmpty(b.Config.Orbit.primaryBody))
                        .Select(b => b.Config)
                        .ToList();


                    // Collect all non-zero orbit distances
                    List<float> orbitDistances = bodies
                        .Select(b => GetOrbitDistance(b.Config))
                        .Where(d => d > 0)
                        .OrderBy(d => d)
                        .ToList();

                    float maxOrbitDist;

                    // Use percentile cutoff to avoid outliers
                    if (orbitDistances.Count == 0)
                    {
                        maxOrbitDist = 1f; // fallback
                    }
                    else
                    {
                        // Take 90th percentile to ignore extreme far-out stars
                        int index = Mathf.FloorToInt(orbitDistances.Count * 0.9f);
                        index = Mathf.Clamp(index, 0, orbitDistances.Count - 1);
                        maxOrbitDist = orbitDistances[index];
                    }


                    void TraverseFromCenter(PlanetConfig center)
                    {
                        Queue<(PlanetConfig config, Vector3 offset)> queue = new();
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
                                GameObject empty = new GameObject(current.name);
                                empty.transform.parent = visualGroup;
                                ResetTransforms(empty.transform);
                                empty.transform.localPosition = offset;
                            }
                            */

                            // Enqueue children for next layer
                            var children = GetChildrenOf(current.name, bodies);

                            if (current == center && staticRootless.Count > 0)
                                children = children.Concat(staticRootless).ToList();

                            // Handle focal point logic
                            var isFocal = current.FocalPoint != null;
                            if (isFocal)
                            {
                                // Remove primary and secondary from children list (they are added manually)
                                children = children
                                    .Where(c => GetStringID(c.name) != GetStringID(current.FocalPoint.primary) && GetStringID(c.name) != GetStringID(current.FocalPoint.secondary))
                                    .ToList();

                                var primary = bodies.Find(b => GetStringID(b.Config.name) == GetStringID(current.FocalPoint.primary))?.Config;
                                var secondary = bodies.Find(b => GetStringID(b.Config.name) == GetStringID(current.FocalPoint.secondary))?.Config;

                                if (primary != null && secondary != null)
                                {
                                    var (primaryOffset, secondaryOffset) = GetAdjustedFocalOffsets(offset, primary, secondary);
                                    queue.Enqueue((primary, offset + primaryOffset));
                                    queue.Enqueue((secondary, offset + secondaryOffset));
                                }
                            }

                            // Sort all remaining children by orbital distance
                            children = children.OrderBy(GetOrbitDistance).ToList();

                            foreach (var child in children.OrderBy(GetOrbitDistance))
                            {
                                bool isZero = GetOrbitDistance(child) == 0;

                                float scale = isFocal ? focalChildRadiusMultiplier : (child.FocalPoint != null ? focalRadiusMultiplier : 1f);
                                float minVisualRadius = 0;
                                float maxVisualRadius = 40 * scale;

                                var childOffset = isZero ? offset
                                    : GetOrbitVisualPosition(child, offset, minVisualRadius, maxVisualRadius, 0, maxOrbitDist);

                                queue.Enqueue((child, childOffset));
                            }
                        }
                    }

                    TraverseFromCenter(center);

                    // After placement: auto-rotate if visuals are too far upward
                    var childVisuals = visualGroupObj.GetComponentsInChildren<ShipLogChildStar>(true).ToList();

                    if (childVisuals.Count > 0)
                    {
                        float highestY = childVisuals.Max(r => r.transform.localPosition.y);
                        if (highestY >= 20)
                        {
                            float angleStep = 10;
                            float bestScore = float.MaxValue;
                            float bestAngle = 0;

                            for (float angle = 0; angle < 360; angle += angleStep)
                            {
                                Quaternion rot = Quaternion.Euler(0, 0, angle);
                                float score = childVisuals
                                    .Select(r => rot * r.transform.localPosition)
                                    .Max(pos => Mathf.Max(pos.y - 20, 0)); // penalize anything over 20

                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestAngle = angle;
                                }
                            }

                            // Apply best rotation to entire group
                            visualGroup.localRotation = Quaternion.Euler(0, 0, bestAngle);
                        }
                    }

                    // No valid children found: fallback
                    if (newStarObject.GetComponentsInChildren<ShipLogChildStar>(true).Length == 0)
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

            AddTextLabel(newStarObject.transform, UniqueIDToName(customName));
            newStar.enabled = true;
            newStar.Initialize(this);
        }

        public void AddGenericStar(Vector3 inputPosition)
        {
            GameObject newStarObject = new GameObject("Star");
            ShipLogStar newStar = newStarObject.AddComponent<ShipLogStar>();
            newStar.transform.parent = _genericParent;

            RawImage starImage = newStarObject.AddComponent<RawImage>();
            starImage.texture = _starTexture;
            newStarObject.AddComponent<CanvasRenderer>();
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

        private void AddTextLabel(Transform parent, string Text)
        {
            GameObject textObject = new GameObject("TextLabel");
            textObject.transform.parent = parent;
            ResetTransforms(textObject.transform);
            textObject.transform.localScale = Vector3.one * 2;

            Text text = textObject.AddComponent<Text>();
            RectTransform rect = textObject.GetAddComponent<RectTransform>();
            textObject.AddComponent<CanvasRenderer>();

            text.alignment = TextAnchor.UpperCenter;
            text.text = Text;
            text.font = _fontToUse;
            text.fontSize = 12;
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = 14;
            text.resizeTextMinSize = 10;
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
    }
}
