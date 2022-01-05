using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Game.State;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    public static class HeavenlyBodyBuilder
    {
        private static readonly Dictionary<string, HeavenlyBody> _bodyMap = new Dictionary<string, HeavenlyBody>();

        public static void Make(GameObject body, IPlanetConfig config, float SOI, GravityVolume bodyGravity, InitialMotion initialMotion)
        {
            var size = new Position.Size(config.Base.SurfaceSize, SOI);
            var gravity = GetGravity(bodyGravity, config.Orbit.IsStatic);
            var parent = config.Orbit.PrimaryBody != null ? GetBody(config.Orbit.PrimaryBody) : HeavenlyBody.None;

            if (config.Orbit.PrimaryBody != null && parent == HeavenlyBody.None)
                Logger.LogWarning($"Could not find [{config.Orbit.PrimaryBody}] parent of [{config.Name}]");

            var orbit = OrbitalHelper.KeplerCoordinatesFromOrbitModule(config.Orbit);

            var hb = GetBody(config.Name);
            if (hb == null) hb = AddHeavenlyBody(config.Name, config.FocalPoint != null);

            Planet.Plantoid planetoid;
            
            if (!config.Orbit.IsStatic) planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, initialMotion._initAngularSpeed, parent, orbit);
            else planetoid = new Planet.Plantoid(size, gravity, body.transform.rotation, 0f, HeavenlyBody.None, body.transform.position, Vector3.zero);

            var mapping = Planet.defaultMapping;
            mapping[hb] = planetoid;

            // Fix for binary focal points
            if(parent != HeavenlyBody.None)
            {
                var focalPoint = Position.AstroLookup[parent].Invoke()?.gameObject.GetComponent<BinaryFocalPoint>();
                if (focalPoint != null && mapping.ContainsKey(parent))
                {
                    GameObject primary = null;
                    GameObject secondary = null;

                    Gravity primaryGravity = null;
                    Gravity secondaryGravity = null;

                    // One of them is null if it's the one being loaded
                    if(config.Name.Equals(focalPoint.PrimaryName))
                    {
                        primary = body;
                        primaryGravity = GetGravity(bodyGravity, false);
                        secondary = Position.getBody(GetBody(focalPoint.SecondaryName))?.gameObject;
                        var secondaryGV = Position.getBody(GetBody(focalPoint.SecondaryName))?.GetAttachedGravityVolume();
                        if (secondaryGV != null) secondaryGravity = GetGravity(secondaryGV, false);
                    }
                    else if (config.Name.Equals(focalPoint.SecondaryName))
                    {
                        secondary = body;
                        secondaryGravity = GetGravity(bodyGravity, false);
                        primary = Position.getBody(GetBody(focalPoint.PrimaryName))?.gameObject;
                        var primaryGV = Position.getBody(GetBody(focalPoint.PrimaryName))?.GetAttachedGravityVolume();
                        if (primaryGV != null) primaryGravity = GetGravity(primaryGV, false);
                    }

                    if (primaryGravity != null && secondaryGravity != null)
                    {

                        // Also have to fix the children
                        var primaryHB = GetBody(focalPoint.PrimaryName);
                        var secondaryHB = GetBody(focalPoint.SecondaryName);

                        var r = primary.transform.position - secondary.transform.position;

                        var m1 = primaryGravity.mass;
                        var m2 = secondaryGravity.mass;

                        float r1 = r.magnitude * m2 / (m1 + m2);
                        float r2 = r.magnitude * m1 / (m1 + m2);

                        ParameterizedAstroObject primaryAO = Position.AstroLookup[primaryHB].Invoke() as ParameterizedAstroObject;
                        ParameterizedAstroObject secondaryAO = Position.AstroLookup[secondaryHB].Invoke() as ParameterizedAstroObject;

                        float ecc = primaryAO.Eccentricity;
                        float i = primaryAO.Inclination;
                        float l = primaryAO.LongitudeOfAscendingNode;
                        float p = primaryAO.ArgumentOfPeriapsis;

                        var primaryKeplerCoordinates = KeplerCoordinates.fromEccentricAnomaly(ecc, r1 / (1 - ecc), i, p, l, 0);
                        var secondaryKeplerCoordinates = KeplerCoordinates.fromEccentricAnomaly(ecc, r2 / (1 - ecc), i, p, l, 180);

                        var totalMass = m1 + m2;

                        var exponent = (primaryGravity.exponent + secondaryGravity.exponent) / 2f;
                        var primaryCartesianState = OrbitHelper.toCartesian(Gravity.of(exponent, totalMass), 0, primaryKeplerCoordinates);
                        var secondaryCartesianState = OrbitHelper.toCartesian(Gravity.of(exponent, totalMass), 0, secondaryKeplerCoordinates);

                        var point = Position.AstroLookup[parent].Invoke();

                        primary.transform.position = point.transform.position + primaryCartesianState.Item1;
                        secondary.transform.position = point.transform.position + secondaryCartesianState.Item1;

                        var primaryOriginal = mapping[primaryHB];
                        var secondaryOriginal = mapping[secondaryHB];

                        // TODO: Idk if this works at all... probably not
                        var primaryRotation = 0f;
                        try
                        {
                            primaryRotation = primaryOriginal.state.relative.angularVelocity.magnitude;
                        }
                        catch (Exception) { };

                        var secondaryRotation = 0f; 
                        try
                        {
                            secondaryRotation = secondaryOriginal.state.relative.angularVelocity.magnitude;
                        }
                        catch (Exception) { };

                        mapping[primaryHB] = new Planet.Plantoid(
                            primaryOriginal.size,
                            primaryOriginal.gravity,
                            primary.transform.rotation,
                            primaryRotation,
                            parent,
                            primaryKeplerCoordinates);

                        mapping[secondaryHB] = new Planet.Plantoid(
                            secondaryOriginal.size,
                            secondaryOriginal.gravity,
                            secondary.transform.rotation,
                            secondaryRotation,
                            parent,
                            secondaryKeplerCoordinates);

                        var period = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(r.magnitude, exponent + 1) / (GravityVolume.GRAVITATIONAL_CONSTANT * totalMass));

                        var trackingOrbitPrimary = primary.GetComponentInChildren<TrackingOrbitLine>();
                        if (trackingOrbitPrimary != null)
                        {
                            trackingOrbitPrimary.TrailTime = period;
                        }

                        var trackingOrbitSecondary = secondary.GetComponentInChildren<TrackingOrbitLine>();
                        if (trackingOrbitSecondary != null)
                        {
                            trackingOrbitSecondary.TrailTime = period;
                        }
                    }
                }
            }

            Planet.defaultMapping = mapping;
            Planet.mapping = mapping;
        }

        public static void Remove(AstroObject obj)
        {
            var astro = Position.find(obj);
            var mapping = Planet.defaultMapping;
            mapping.Remove(astro);
            Planet.defaultMapping = mapping;
            Planet.mapping = mapping;
        }

        private static Gravity GetGravity(GravityVolume volume, bool isStatic)
        {
            if (volume == null)
            {
                return Gravity.of(2, 0, isStatic);
            }

            var exponent = volume._falloffType != GravityVolume.FalloffType.linear ? 2f : 1f;
            var mass = (volume._surfaceAcceleration * Mathf.Pow(volume._upperSurfaceRadius, exponent)) / GravityVolume.GRAVITATIONAL_CONSTANT;

            return Gravity.of(exponent, mass, isStatic);
        }

        private static HeavenlyBody AddHeavenlyBody(string name, bool isFocalPoint)
        {
            var hb = new HeavenlyBody(name, isFocalPoint);
            _bodyMap.Add(name, hb);

            var astroLookup = Position.AstroLookup;
            var bodyLookup = Position.BodyLookup;

            astroLookup.Add(hb, () => GetAstroObject(name));
            bodyLookup.Add(hb, () => GetOWRigidbody(name));

            Position.AstroLookup = astroLookup;
            Position.BodyLookup = bodyLookup;

            return hb;
        }

        private static HeavenlyBody GetBody(string name)
        {
            if (_bodyMap.ContainsKey(name))
            {
                return _bodyMap[name];
            }

            var hb = Position.find(AstroObjectLocator.GetAstroObject(name));
            if (hb != null)
            {
                _bodyMap.Add(name, hb);
            }
            return hb;
        }

        public static void Reset()
        {
            Planet.defaultMapping = Planet.standardMapping;
        }

        private static AstroObject GetAstroObject(string name)
        {
            var astroBody = AstroObjectLocator.GetAstroObject(name);
            if (astroBody == null
                || astroBody.gameObject == null)
            {
                return null;
            }

            return astroBody;
        }

        private static OWRigidbody GetOWRigidbody(string name)
        {
            var astroBody = GetAstroObject(name);
            return astroBody?.GetOWRigidbody();
        }
    }
}
