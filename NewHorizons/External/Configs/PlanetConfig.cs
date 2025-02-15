using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.External.Modules.Props.Dialogue;
using NewHorizons.External.Modules.Props.Quantum;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.Modules.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Describes a celestial body to generate
    /// </summary>
    [JsonObject(Title = "Celestial Body")]
    public class PlanetConfig
    {
        #region Fields
        /// <summary>
        /// Unique name of your planet. If not specified, the file name (without the extension) is used.
        /// </summary>
        public string name;

        /// <summary>
        /// Unique star system containing your planet. If you set this to be a custom solar system remember to add a Spawn module to one of the bodies, or else you can't get to the system.
        /// </summary>
        [DefaultValue("SolarSystem")] public string starSystem = "SolarSystem";

        /// <summary>
        /// Does this config describe a quantum state of a custom planet defined in another file?
        /// </summary>
        public bool isQuantumState;

        /// <summary>
        /// Does this config describe a stellar remnant of a custom star defined in another file?
        /// </summary>
        public bool isStellarRemnant;

        /// <summary>
        /// Should this planet ever be shown on the title screen?
        /// </summary>
        [DefaultValue(true)] public bool canShowOnTitle = true;

        /// <summary>
        /// `true` if you want to delete this planet
        /// </summary>
        public bool destroy;

        /// <summary>
        /// Do we track the position of this body when calculating the solar system radius?
        /// `true` if you want the map zoom speed, map panning distance/speed, map camera farclip plane,
        /// and autopilot-returning-to-solar-system to adjust to this planet's orbit
        /// </summary>
        [DefaultValue(true)] public bool trackForSolarSystemRadius = true;

        /// <summary>
        /// A list of paths to child GameObjects to destroy on this planet
        /// </summary>
        public string[] removeChildren;

        #endregion

        #region Modules
        /// <summary>
        /// Add ambient lights to this body
        /// </summary>
        public AmbientLightModule[] AmbientLights;

        /// <summary>
        /// Generate asteroids around this body
        /// </summary>
        public AsteroidBeltModule AsteroidBelt;

        /// <summary>
        /// Describes this Body's atmosphere
        /// </summary>
        public AtmosphereModule Atmosphere;

        /// <summary>
        /// Base Properties of this Body
        /// </summary>
        public BaseModule Base;

        /// <summary>
        /// Add bramble nodes to this planet and/or make this planet a bramble dimension
        /// </summary>
        public BrambleModule Bramble;

        /// <summary>
        /// Add a cloaking field to this planet
        /// </summary>
        public CloakModule Cloak;

        /// <summary>
        /// Make this planet part of the dream world
        /// </summary>
        public DreamModule Dream;

        /// <summary>
        /// Add features exclusive to the Eye of the Universe scene
        /// </summary>
        public EyeOfTheUniverseModule EyeOfTheUniverse;

        /// <summary>
        /// Make this body into a focal point (barycenter)
        /// </summary>
        public FocalPointModule FocalPoint;

        /// <summary>
        /// Add funnel from this planet to another
        /// </summary>
        public FunnelModule Funnel;

        /// <summary>
        /// Generate the surface of this planet using a heightmap
        /// </summary>
        public HeightMapModule HeightMap;

        /// <summary>
        /// Add lava to this planet
        /// </summary>
        public LavaModule Lava;

        /// <summary>
        /// Map marker properties of this body
        /// </summary>
        public MapMarkerModule MapMarker;

        /// <summary>
        /// Describes this Body's orbit (or lack there of)
        /// </summary>
        public OrbitModule Orbit;

        /// <summary>
        /// Procedural Generation
        /// </summary>
        public ProcGenModule ProcGen;

        /// <summary>
        /// Spawn various objects on this body
        /// </summary>
        public PropModule Props;

        /// <summary>
        /// Reference frame properties of this body
        /// </summary>
        public ReferenceFrameModule ReferenceFrame;

        /// <summary>
        /// Create rings around the planet
        /// </summary>
        public RingModule[] Rings;

        /// <summary>
        /// Add sand to this planet
        /// </summary>
        public SandModule Sand;

        /// <summary>
        /// Add ship log entries to this planet and describe how it looks in map mode
        /// </summary>
        public ShipLogModule ShipLog;

        /// <summary>
        /// Settings for shock effect on planet when the nearest star goes supernova
        /// </summary>
        public ShockEffectModule ShockEffect;

        /// <summary>
        /// Spawn the player at this planet
        /// </summary>
        public SpawnModule Spawn;

        /// <summary>
        /// Make this body a star
        /// </summary>
        public StarModule Star;

        /// <summary>
        /// Add water to this planet
        /// </summary>
        public WaterModule Water;

        /// <summary>
        /// Add particle effects in a field around the planet.
        /// Also known as Vection Fields.
        /// </summary>
        public ParticleFieldModule[] ParticleFields;

        /// <summary>
        /// Add various volumes on this body
        /// </summary>
        public VolumesModule Volumes;

        /// <summary>
        /// Add a comet tail to this body, like the Interloper
        /// </summary>
        public CometTailModule CometTail;

        /// <summary>
        /// Extra data that may be used by extension mods
        /// </summary>
        public object extras;

        #endregion

        #region Obsolete

        [Obsolete("ChildrenToDestroy is deprecated, please use RemoveChildren instead")]
        public string[] childrenToDestroy;

        [Obsolete("Singularity is deprecated, please use Props->singularities")]
        public SingularityModule Singularity;

        [Obsolete("Signal is deprecated, please use Props->signals")]
        public SignalModule Signal;

        [Obsolete("Ring is deprecated, please use Rings")]
        public RingModule Ring;

        #endregion Obsolete

        #region ctor validation and migration
        public PlanetConfig()
        {
            // Always have to have a base module
            if (Base == null) Base = new BaseModule();
            if (Orbit == null) Orbit = new OrbitModule();
            if (ReferenceFrame == null) ReferenceFrame = new ReferenceFrameModule();
            if (MapMarker == null) MapMarker = new MapMarkerModule();
        }

        public void Validate()
        {
            // If we can correct a part of the config, do it
            // If it cannot be solved, throw an exception
            if (Base.centerOfSolarSystem) Orbit.isStatic = true;
            if (Atmosphere?.clouds?.lightningGradient != null) Atmosphere.clouds.hasLightning = true;
            if (Bramble?.dimension != null && Orbit?.staticPosition == null) throw new Exception($"Dimension {name} must have Orbit.staticPosition defined.");
            if (Bramble?.dimension != null) canShowOnTitle = false;
            if (Orbit?.staticPosition != null) Orbit.isStatic = true;

            // Stars and focal points shouldnt be destroyed by stars
            if (Star != null || FocalPoint != null) Base.hasFluidDetector = false;

            // Disable map marker for dream dimensions
            if (Dream != null && Dream.inDreamWorld) MapMarker.enabled = false;

            // User error #983
            // This will not catch if they wrote the two names slightly differently but oh well don't be stupid
            // Ideally we should just check for loops in PlanetGraph
            if (Orbit.primaryBody == name && !string.IsNullOrEmpty(Orbit.primaryBody))
            {
                throw new Exception($"You set {name} to orbit itself, that is invalid. The planet will not load.");
            }
        }

        public void Migrate()
        {
            // Backwards compatibility
            // Should be the only place that obsolete things are referenced
#pragma warning disable 612, 618
            if (Base.waterSize != 0)
                Water = new WaterModule
                {
                    size = Base.waterSize,
                    tint = Base.waterTint
                };

            if (Base.lavaSize != 0)
                Lava = new LavaModule
                {
                    size = Base.lavaSize
                };

            if (Base.blackHoleSize != 0)
                Singularity = new SingularityModule
                {
                    type = SingularityModule.SingularityType.BlackHole,
                    size = Base.blackHoleSize
                };

            if (Base.isSatellite) Base.showMinimap = false;

            if (!Base.hasReferenceFrame) ReferenceFrame.enabled = false;

            if (Base.hasMapMarker) MapMarker.enabled = true;

            if (childrenToDestroy != null) removeChildren = childrenToDestroy;

            if (Base.cloakRadius != 0)
                Cloak = new CloakModule
                {
                    radius = Base.cloakRadius
                };

            if (Base.hasAmbientLight || Base.ambientLight != 0)
            {
                if (AmbientLights == null) AmbientLights = new AmbientLightModule[0];
                AmbientLights = AmbientLights.Append(new AmbientLightModule { intensity = Base.ambientLight != 0 ? Base.ambientLight : 0.5f }).ToArray();
            }

            if (Atmosphere != null)
            {
                if (!string.IsNullOrEmpty(Atmosphere.cloud))
                    Atmosphere.clouds = new AtmosphereModule.CloudInfo
                    {
                        outerCloudRadius = Atmosphere.size,
                        innerCloudRadius = Atmosphere.size * 0.9f,
                        tint = Atmosphere.cloudTint,
                        texturePath = Atmosphere.cloud,
                        capPath = Atmosphere.cloudCap,
                        rampPath = Atmosphere.cloudRamp,
                        fluidType = Atmosphere.fluidType,
                        useBasicCloudShader = Atmosphere.useBasicCloudShader,
                        unlit = !Atmosphere.shadowsOnClouds
                    };

                // Validate
                if (Atmosphere.clouds?.lightningGradient != null) Atmosphere.clouds.hasLightning = true;

                // Former is obsolete, latter is to validate
                if (Atmosphere.hasAtmosphere || Atmosphere.atmosphereTint != null)
                    Atmosphere.useAtmosphereShader = true;

                // useBasicCloudShader is obsolete
                if (Atmosphere.clouds != null && Atmosphere.clouds.useBasicCloudShader)
                    Atmosphere.clouds.cloudsPrefab = CloudPrefabType.Basic;

                if (Atmosphere.hasRain)
                {
                    if (ParticleFields == null) ParticleFields = new ParticleFieldModule[0];
                    ParticleFields = ParticleFields.Append(new ParticleFieldModule
                    {
                        type = ParticleFieldModule.ParticleFieldType.Rain,
                        rename = "RainEmitter"
                    }).ToArray();
                }

                if (Atmosphere.hasSnow)
                {
                    if (ParticleFields == null) ParticleFields = new ParticleFieldModule[0];
                    for (int i = 0; i < 5; i++)
                    {
                        ParticleFields = ParticleFields.Append(new ParticleFieldModule
                        {
                            type = ParticleFieldModule.ParticleFieldType.SnowflakesHeavy,
                            rename = "SnowEmitter"
                        }).ToArray();
                    }
                }
            }

            if (Props?.tornados != null)
                foreach (var tornado in Props.tornados)
                    if (tornado.downwards)
                        tornado.type = TornadoInfo.TornadoType.Downwards;

            if (Props?.audioVolumes != null)
            {
                if (Volumes == null) Volumes = new VolumesModule();
                if (Volumes.audioVolumes == null) Volumes.audioVolumes = new AudioVolumeInfo[0];
                Volumes.audioVolumes = Volumes.audioVolumes.Concat(Props.audioVolumes).ToArray();
            }

            if (Props?.reveal != null)
            {
                if (Volumes == null) Volumes = new VolumesModule();
                if (Volumes.revealVolumes == null) Volumes.revealVolumes = new RevealVolumeInfo[0];
                Volumes.revealVolumes = Volumes.revealVolumes.Concat(Props.reveal).ToArray();
            }

            if (Base.sphereOfInfluence != 0f) Base.soiOverride = Base.sphereOfInfluence;

            // Moved a bunch of stuff off of shiplog module to star system module because it didnt exist when we made this
            if (ShipLog != null)
            {
                Main.SystemDict.TryGetValue(starSystem, out var system);

                if (ShipLog.entryPositions != null)
                {
                    if (system.Config.entryPositions == null) system.Config.entryPositions = ShipLog.entryPositions;
                    else system.Config.entryPositions = system.Config.entryPositions.Concat(ShipLog.entryPositions).ToArray();
                }

                if (ShipLog.curiosities != null)
                {
                    if (system.Config.curiosities == null) system.Config.curiosities = ShipLog.curiosities;
                    else system.Config.curiosities = system.Config.curiosities.Concat(ShipLog.curiosities).ToArray();
                }

                if (ShipLog.initialReveal != null)
                {
                    if (system.Config.initialReveal == null) system.Config.initialReveal = ShipLog.initialReveal;
                    else system.Config.initialReveal = system.Config.initialReveal.Concat(ShipLog.initialReveal).ToArray();
                }
            }

            // Singularity is now a list in props so you can have many per planet
            if (Singularity != null)
            {
                if (Props == null) Props = new PropModule();
                if (Props.singularities == null) Props.singularities = new SingularityModule[0];
                Props.singularities = Props.singularities.Append(Singularity).ToArray();
            }

            // Old singularity size
            if (Props?.singularities != null)
            {
                foreach (var singularity in Props.singularities)
                {
                    if (singularity.size != 0f)
                    {
                        singularity.horizonRadius = singularity.size * 0.4f;
                        switch (singularity.type)
                        {
                            case SingularityModule.SingularityType.BlackHole:
                                singularity.distortRadius = singularity.size * 0.95f;
                                break;
                            case SingularityModule.SingularityType.WhiteHole:
                                singularity.distortRadius = singularity.size * 2.8f;
                                break;
                        }
                    }
                }
            }

            // Signals are now in props
            if (Signal?.signals != null)
            {
                if (Props == null) Props = new PropModule();
                if (Props.signals == null) Props.signals = new SignalInfo[0];
                Props.signals = Props.signals.Concat(Signal.signals).ToArray();
            }

            // Star
            if (Star != null)
            {
                if (!Star.goSupernova) Star.stellarDeathType = StellarDeathType.None;

                // Gave up on supporting pulsars
                if (Star.stellarRemnantType == StellarRemnantType.Pulsar) Star.stellarRemnantType = StellarRemnantType.NeutronStar;
            }

            // Signals no longer use two different variables for audio
            if (Props?.signals != null)
            {
                foreach (var signal in Props.signals)
                {
                    if (!string.IsNullOrEmpty(signal.audioClip)) signal.audio = signal.audioClip;
                    if (!string.IsNullOrEmpty(signal.audioFilePath)) signal.audio = signal.audioFilePath;
                }
            }

            // Cloak
            if (Cloak != null)
            {
                if (!string.IsNullOrEmpty(Cloak.audioClip)) Cloak.audio = Cloak.audioClip;
                if (!string.IsNullOrEmpty(Cloak.audioFilePath)) Cloak.audio = Cloak.audioFilePath;
            }

            // Ring is now a list so you can have many per planet
            if (Ring != null)
            {
                if (Rings == null) Rings = new RingModule[0];
                Rings = Rings.Append(Ring).ToArray();
            }

            // Rings are no longer variable size module
            if (Rings != null)
            {
                foreach (var ring in Rings)
                {
                    if (ring.curve != null) ring.scaleCurve = ring.curve;
                }
            }

            if (Base.zeroGravityRadius != 0f)
            {
                Volumes ??= new VolumesModule();
                Volumes.zeroGravityVolumes ??= new PriorityVolumeInfo[0];

                Volumes.zeroGravityVolumes = Volumes.zeroGravityVolumes.Append(new PriorityVolumeInfo()
                {
                    priority = 1,
                    rename = "ZeroGVolume",
                    radius = Base.zeroGravityRadius,
                    parentPath = "Volumes"
                }).ToArray();
            }

            // So that old mods still have shock effects
            if (ShockEffect == null && Star == null && name != "Sun" && name != "EyeOfTheUniverse" && FocalPoint == null)
            {
                ShockEffect = new ShockEffectModule() { hasSupernovaShockEffect = true };
            }

            // Spawn points reorganized to use GeneralPointPropInfo
            if (Spawn != null && Spawn.playerSpawn == null && Spawn.playerSpawnPoint != null)
            {
                Spawn.playerSpawn = new SpawnModule.PlayerSpawnPoint()
                {
                    position = Spawn.playerSpawnPoint,
                    rotation = Spawn.playerSpawnRotation,
                    startWithSuit = Spawn.startWithSuit,
                };
            }
            if (Spawn != null && Spawn.shipSpawn == null && Spawn.shipSpawnPoint != null)
            {
                Spawn.shipSpawn = new SpawnModule.ShipSpawnPoint()
                {
                    position = Spawn.shipSpawnPoint,
                    rotation = Spawn.shipSpawnRotation,
                };
            }

            // Spawn points are now a list
            if (Spawn != null && Spawn.playerSpawn != null && Spawn.playerSpawnPoints == null)
            {
                Spawn.playerSpawnPoints = new SpawnModule.PlayerSpawnPoint[] { Spawn.playerSpawn };
            }
            if (Spawn != null && Spawn.shipSpawn != null && Spawn.shipSpawnPoints == null)
            {
                Spawn.shipSpawnPoints = new SpawnModule.ShipSpawnPoint[] { Spawn.shipSpawn };
            }

            // Because these guys put TWO spawn points 
            if (starSystem == "2walker2.OogaBooga" && name == "The Campground")
            {
                Spawn.playerSpawnPoints[0].isDefault = true;
            }

            // Remote dialogue trigger reorganized to use GeneralPointPropInfo
            if (Props?.dialogue != null)
            {
                foreach (var dialogue in Props.dialogue)
                {
                    if (dialogue.remoteTrigger == null && (dialogue.remoteTriggerPosition != null || dialogue.remoteTriggerRadius != 0))
                    {
                        dialogue.remoteTrigger = new RemoteTriggerInfo
                        {
                            position = dialogue.remoteTriggerPosition,
                            radius = dialogue.remoteTriggerRadius,
                            prereqCondition = dialogue.remoteTriggerPrereqCondition,
                        };
                    }
                }
            }

            // alignRadial added to all props with rotation; default behavior varies
            if (Spawn?.playerSpawn != null && Spawn.playerSpawn.rotation == null && !Spawn.playerSpawn.alignRadial.HasValue)
            {
                Spawn.playerSpawn.alignRadial = true;
            }
            if (Spawn?.shipSpawn != null && Spawn.shipSpawn.rotation == null && !Spawn.shipSpawn.alignRadial.HasValue)
            {
                Spawn.shipSpawn.alignRadial = true;
            }
            if (Props?.details != null)
            {
                foreach (var detail in Props.details)
                {
                    if (!detail.alignRadial.HasValue)
                    {
                        detail.alignRadial = detail.alignToNormal;
                    }
                }
            }
            if (Props?.proxyDetails != null)
            {
                foreach (var detail in Props.proxyDetails)
                {
                    if (!detail.alignRadial.HasValue)
                    {
                        detail.alignRadial = detail.alignToNormal;
                    }
                }
            }
            if (Props?.geysers != null)
            {
                foreach (var geyser in Props.geysers)
                {
                    if (!geyser.alignRadial.HasValue && geyser.rotation == null)
                    {
                        geyser.alignRadial = true;
                    }
                }
            }
            if (Props?.tornados != null)
            {
                foreach (var tornado in Props.tornados)
                {
                    if (!tornado.alignRadial.HasValue && tornado.rotation == null)
                    {
                        tornado.alignRadial = true;
                    }
                }
            }
            if (Props?.volcanoes != null)
            {
                foreach (var volcano in Props.volcanoes)
                {
                    if (!volcano.alignRadial.HasValue && volcano.rotation == null)
                    {
                        volcano.alignRadial = true;
                    }
                }
            }
            if (Props?.nomaiText != null)
            {
                foreach (var nomaiText in Props.nomaiText)
                {
                    if (nomaiText.type == Modules.TranslatorText.NomaiTextType.Cairn)
                    {
                        nomaiText.type = Modules.TranslatorText.NomaiTextType.CairnBrittleHollow;
                    }
                    else if (nomaiText.type == Modules.TranslatorText.NomaiTextType.CairnVariant)
                    {
                        nomaiText.type = Modules.TranslatorText.NomaiTextType.CairnTimberHearth;
                    }
                }
            }
            if (Props?.translatorText != null)
            {
                foreach (var translatorText in Props.translatorText)
                {
                    if (translatorText.type == Modules.TranslatorText.NomaiTextType.Cairn)
                    {
                        translatorText.type = Modules.TranslatorText.NomaiTextType.CairnBrittleHollow;
                    }
                    else if (translatorText.type == Modules.TranslatorText.NomaiTextType.CairnVariant)
                    {
                        translatorText.type = Modules.TranslatorText.NomaiTextType.CairnTimberHearth;
                    }
                }
            }

            if (Base.hasCometTail)
            {
                CometTail ??= new();
                if (Base.cometTailRotation != null)
                {
                    CometTail.rotationOverride = Base.cometTailRotation;
                }
            }

            if (Volumes?.destructionVolumes != null)
            {
                foreach (var destructionVolume in Volumes.destructionVolumes)
                {
                    if (destructionVolume.onlyAffectsPlayerAndShip) destructionVolume.onlyAffectsPlayerRelatedBodies = true;
                }
            }

            if (Volumes?.creditsVolume != null)
            {
                foreach (var volume in Volumes.creditsVolume)
                {
                    if (!string.IsNullOrEmpty(volume.gameOverText))
                    {
                        if (volume.gameOver == null)
                        {
                            volume.gameOver = new();
                        }
                        volume.gameOver.text = volume.gameOverText;
                    }
                    if (volume.creditsType != null)
                    {
                        volume.gameOver.creditsType = (SerializableEnums.NHCreditsType)volume.creditsType;
                    }
                }
            }

            if (Base.invulnerableToSun)
            {
                Base.hasFluidDetector = false;
            }

            // OLD QUANTUM VALIDATION
            // For each quantum group, verify the following:
            //      this group's id should be unique
            //      if type == sockets, group.sockets should not be null or empty
            //      if type == sockets, count every prop that references this group. the number should be < group.sockets.Count
            //      if type == sockets, for each socket, if rotation == null, rotation = Vector3.zero
            //      if type == sockets, for each socket, position must not be null
            // For each detail prop,
            //      if detail.quantumGroupID != null, there exists a quantum group with that id
            if (Props?.quantumGroups != null && Props?.details != null)
            {
                Dictionary<string, QuantumGroupInfo> existingGroups = new Dictionary<string, QuantumGroupInfo>();
                foreach (var quantumGroup in Props.quantumGroups)
                {
                    if (existingGroups.ContainsKey(quantumGroup.id)) { NHLogger.LogWarning($"Duplicate quantumGroup id found: {quantumGroup.id}"); quantumGroup.type = QuantumGroupType.FailedValidation; }

                    existingGroups[quantumGroup.id] = quantumGroup;
                    if (quantumGroup.type == QuantumGroupType.Sockets)
                    {
                        if (quantumGroup.sockets?.Length == 0) { NHLogger.LogError($"quantumGroup {quantumGroup.id} is of type \"sockets\" but has no defined sockets."); quantumGroup.type = QuantumGroupType.FailedValidation; }
                        else
                        {
                            foreach (var socket in quantumGroup.sockets)
                            {
                                if (socket.rotation == null) socket.rotation = UnityEngine.Vector3.zero;
                                if (socket.position == null) { NHLogger.LogError($"quantumGroup {quantumGroup.id} has a socket without a position."); quantumGroup.type = QuantumGroupType.FailedValidation; }
                            }
                        }
                    }
                }

                var existingGroupsPropCounts = new Dictionary<string, int>();
                foreach (var prop in Props?.details)
                {
                    if (prop.quantumGroupID == null) continue;
                    if (!existingGroups.ContainsKey(prop.quantumGroupID)) NHLogger.LogWarning($"A prop wants to be a part of quantum group {prop.quantumGroupID}, but this group does not exist.");
                    else existingGroupsPropCounts[prop.quantumGroupID] = existingGroupsPropCounts.GetValueOrDefault(prop.quantumGroupID) + 1;
                }

                foreach (var quantumGroup in Props.quantumGroups)
                {
                    if (quantumGroup.type == QuantumGroupType.Sockets && existingGroupsPropCounts.GetValueOrDefault(quantumGroup.id) > quantumGroup.sockets?.Length)
                    {
                        NHLogger.LogError($"quantumGroup {quantumGroup.id} is of type \"sockets\" and has more props than sockets.");
                        quantumGroup.type = QuantumGroupType.FailedValidation;
                    }
                }
            }

            if (Props != null && Props.quantumGroups != null)
            {
                var socketQuantumGroups = Props.quantumGroups.Where(x => x.type == QuantumGroupType.Sockets).Select(x => new SocketQuantumGroupInfo()
                {
                    rename = "Quantum Sockets - " + x.id,
                    sockets = x.sockets,
                    details = Props.details.Where(y => y.quantumGroupID == x.id).Select(x => new QuantumDetailInfo(x)).ToArray()
                });
                if (socketQuantumGroups.Any())
                {
                    Props.socketQuantumGroups = socketQuantumGroups.ToArray();
                }
                var stateQuantumGroups = Props.quantumGroups.Where(x => x.type == QuantumGroupType.States).Select(x => new StateQuantumGroupInfo()
                {
                    rename = "Quantum States - " + x.id,
                    hasEmptyState = x.hasEmptyState,
                    loop = x.loop,
                    sequential = x.sequential,
                    details = Props.details.Where(y => y.quantumGroupID == x.id).Select(x => new QuantumDetailInfo(x)).ToArray()
                });
                if (stateQuantumGroups.Any())
                {
                    Props.stateQuantumGroups = stateQuantumGroups.ToArray();
                }

                Props.details = Props.details.Where(x => string.IsNullOrEmpty(x.quantumGroupID)).ToArray();
            }
        }
        #endregion
    }
}