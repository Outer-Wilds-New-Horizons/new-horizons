using NewHorizons.External.Modules.Volumes.VolumeInfos;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class VolumesModule
    {
        /// <summary>
        /// Add audio volumes to this planet.
        /// </summary>
        public AudioVolumeInfo[] audioVolumes;

        /// <summary>
        /// Add condition trigger volumes to this planet. Sets a condition when the player, scout, or ship enters this volume.
        /// </summary>
        public ConditionTriggerVolumeInfo[] conditionTriggerVolumes;

        /// <summary>
        /// Add day night audio volumes to this planet. These volumes play a different clip depending on the time of day.
        /// </summary>
        public DayNightAudioVolumeInfo[] dayNightAudioVolumes;

        /// <summary>
        /// Add destruction volumes to this planet.
        /// Destroys bodies if they enter this volume. Can kill the player and recall the scout probe.
        /// </summary>
        public DestructionVolumeInfo[] destructionVolumes;

        /// <summary>
        /// Add fluid volumes to this planet.
        /// </summary>
        public FluidVolumeInfo[] fluidVolumes;

        /// <summary>
        /// Add force volumes to this planet.
        /// </summary>
        public ForceModule forces;

        /// <summary>
        /// Add hazard volumes to this planet.
        /// Causes damage to player when inside this volume.
        /// </summary>
        public HazardVolumeInfo[] hazardVolumes;

        /// <summary>
        /// Add interaction volumes to this planet.
        /// They can be interacted with by the player to trigger various effects.
        /// </summary>
        public InteractionVolumeInfo[] interactionVolumes;

        /// <summary>
        /// Add interference volumes to this planet.
        /// Hides HUD markers of ship scout/probe and prevents scout photos if you are not inside the volume together with ship or scout probe.
        /// </summary>
        public VolumeInfo[] interferenceVolumes;

        /// <summary>
        /// Add insulating volumes to this planet.
        /// These will stop electricty hazard volumes from affecting you (just like the jellyfish).
        /// </summary>
        public VolumeInfo[] insulatingVolumes;

        /// <summary>
        /// Add light source volumes to this planet.
        /// These will activate rafts and other light detectors.
        /// </summary>
        public VolumeInfo[] lightSourceVolumes;

        /// <summary>
        /// Add map restriction volumes to this planet.
        /// The map will be disabled when inside this volume.
        /// </summary>
        public VolumeInfo[] mapRestrictionVolumes;

        /// <summary>
        /// Add notification volumes to this planet.
        /// Sends a notification to the player just like ghost matter does when you get too close
        /// and also to the ship just like when you damage a component on the ship.
        /// </summary>
        public NotificationVolumeInfo[] notificationVolumes;

        /// <summary>
        /// Add oxygen volumes to this planet.
        /// </summary>
        public OxygenVolumeInfo[] oxygenVolumes;

        /// <summary>
        /// Add probe-specific volumes to this planet.
        /// </summary>
        public ProbeModule probe;

        /// <summary>
        /// Add reference frame blocker volumes to this planet.
        /// These will stop the player from seeing/targeting any reference frames.
        /// </summary>
        public VolumeInfo[] referenceFrameBlockerVolumes;

        /// <summary>
        /// Add triggers that reveal parts of the ship log on this planet.
        /// </summary>
        public RevealVolumeInfo[] revealVolumes;

        /// <summary>
        /// Add reverb volumes to this planet. Great for echoes in caves.
        /// </summary>
        public VolumeInfo[] reverbVolumes;

        /// <summary>
        /// Add ruleset volumes to this planet.
        /// </summary>
        public RulesetModule rulesets;

        /// <summary>
        /// Add speed trap volumes to this planet.
        /// Slows down the player when they enter this volume.
        /// </summary>
        public SpeedTrapVolumeInfo[] speedTrapVolumes;

        /// <summary>
        /// Add speed limiter volumes to this planet.
        /// Slows down the player, ship, and probe when they enter this volume.
        /// Used on the Stranger in DLC.
        /// </summary>
        public SpeedLimiterVolumeInfo[] speedLimiterVolumes;

        /// <summary>
        /// Add visor effect volumes to this planet.
        /// </summary>
        public VisorEffectModule visorEffects;

        /// <summary>
        /// Add zero-gravity volumes to this planet. 
        /// Good for surrounding planets which are using a static position to stop the player being pulled away.
        /// </summary>
        public PriorityVolumeInfo[] zeroGravityVolumes;

        /// <summary>
        /// Entering this volume will load a new solar system.
        /// </summary>
        public ChangeStarSystemVolumeInfo[] solarSystemVolume;

        /// <summary>
        /// Enter this volume to be sent to the end credits scene
        /// </summary>
        public LoadCreditsVolumeInfo[] creditsVolume;
    }
}
