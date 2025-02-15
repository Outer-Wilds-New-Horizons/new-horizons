using NewHorizons.Components.Orbital;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class OrbitModule : IOrbitalParameters
    {
        /// <summary>
        /// Specify this if you want the body to remain stationary at a given location (ie not orbit its parent). Required for Bramble dimensions
        /// </summary>
        public MVector3 staticPosition;

        /// <summary>
        /// The name of the body this one will orbit around
        /// </summary>
        public string primaryBody;

        /// <summary>
        /// Is this the moon of a planet? Used for determining when its name is shown on the map.
        /// </summary>
        public bool isMoon;

        /// <summary>
        /// The angle between the normal to the orbital plane and its axis of rotation.
        /// </summary>
        public float axialTilt;

        /// <summary>
        /// Rotation period in minutes.
        /// </summary>
        public float siderealPeriod;

        /// <summary>
        /// Offsets the planet's starting sidereal rotation. In degrees.
        /// </summary>
        [Range(0f, 360f)]
        public float initialRotation;

        /// <summary>
        /// Should the body always have one side facing its primary?
        /// </summary>
        public bool isTidallyLocked;

        /// <summary>
        /// Is the body meant to stay in one place without moving? If staticPosition is not set, the initial position
        /// will be determined using its orbital parameters.
        /// </summary>
        public bool isStatic;

        /// <summary>
        /// If it is tidally locked, this direction will face towards the primary. Ex: Interloper uses `0, -1, 0`. Most planets
        /// will want something like `-1, 0, 0`.
        /// </summary>
        public MVector3 alignmentAxis;

        /// <summary>
        /// Referring to the orbit line in the map screen.
        /// </summary>
        [DefaultValue(true)]
        public bool showOrbitLine = true;

        /// <summary>
        /// Should the orbit line be dotted?
        /// </summary>
        public bool dottedOrbitLine;

        /// <summary>
        /// Colour of the orbit-line in the map view.
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Should we just draw a line behind its orbit instead of the entire circle/ellipse?
        /// </summary>
        public bool trackingOrbitLine;

        /// <summary>
        /// If the camera is farther than this distance the orbit line will fade out. Leave empty to not have it fade out.
        /// </summary>
        public float orbitLineFadeEndDistance = -1f;

        /// <summary>
        /// If the camera is closer than this distance the orbit line will fade out. Leave empty to not have it fade out.
        /// </summary>
        public float orbitLineFadeStartDistance = -1f;

        /// <summary>
        /// The semi-major axis of the ellipse that is the body's orbit. For a circular orbit this is the radius.
        /// </summary>
        [Range(0f, double.MaxValue)]
        public float semiMajorAxis { get; set; }

        /// <summary>
        /// The angle (in degrees) between the body's orbit and the plane of the star system
        /// </summary>
        public float inclination { get; set; }

        /// <summary>
        /// An angle (in degrees) defining the point where the orbit of the body rises above the orbital plane if it has
        /// nonzero inclination.
        /// </summary>
        public float longitudeOfAscendingNode { get; set; }

        /// <summary>
        /// At 0 the orbit is a circle. The closer to 1 it is, the more oval-shaped the orbit is.
        /// </summary>
        [Range(0f, 0.9999999999f)]
        public float eccentricity { get; set; }

        /// <summary>
        /// An angle (in degrees) defining the location of the periapsis (the closest distance to it's primary body) if it has
        /// nonzero eccentricity.
        /// </summary>
        public float argumentOfPeriapsis { get; set; }

        /// <summary>
        /// Where the planet should start off in its orbit in terms of the central angle.
        /// </summary>
        public float trueAnomaly { get; set; }

        public OrbitalParameters GetOrbitalParameters(Gravity primaryGravity, Gravity secondaryGravity)
        {
            return OrbitalParameters.FromTrueAnomaly(primaryGravity, secondaryGravity, eccentricity, semiMajorAxis,
                inclination, argumentOfPeriapsis, longitudeOfAscendingNode, trueAnomaly);
        }
    }
}