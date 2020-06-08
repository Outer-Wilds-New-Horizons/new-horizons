using Marshmallow.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.External
{
    public interface IPlanetConfig
    {
        string name { get; }
        MVector3 position { get; }
        int orbitAngle { get; }
        string primaryBody { get; }
        bool hasSpawnPoint { get; }
        bool hasClouds { get; }
        float topCloudSize { get; }
        float bottomCloudSize { get; }
        MColor32 topCloudTint { get; }
        MColor32 bottomCloudTint { get; }
        bool hasWater { get; }
        float waterSize { get; }
        bool hasRain { get; }
        bool hasGravity { get; }
        float surfaceAcceleration { get; }
        bool hasMapMarker { get; }
        bool hasFog { get; }
        MColor32 fogTint { get; }
        float fogDensity { get; }
        float groundSize { get; }
    }
}
