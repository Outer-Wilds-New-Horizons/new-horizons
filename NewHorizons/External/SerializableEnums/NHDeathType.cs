using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    /// <summary>
    /// Some special death types are:
    /// 
    /// Supernova: Special death type used when the supernova hits you. You will not wake up if in the Dreamworld. 
    /// 
    /// Digestion: Death type used by anglerfish (and cut-content ghosts and water monster)
    /// 
    /// Big bang: Special death type used at the end of the game
    /// 
    /// Meditation: Special death type used when skipping to the next loop. You will not wake up if in the Dreamworld. 
    /// 
    /// Timeloop: Special death type used when the time loop ends. You will not wake up if in the Dreamworld. 
    /// 
    /// Blackhole: Special death type used by the ATP blackhole (and custom NH blackholes without whitehole destinations)
    /// 
    /// Dream: Special DLC death type used to kill a player in the real world while in the Dreamworld (i.e., you will loop not wake up)
    /// 
    /// DreamExplosion: Special DLC death type used by the prototype artifact to kill a player in the real world while in the Dreamworld (i.e., you will loop not wake up)
    /// 
    /// CrushedByElevator: Similar to the Crushed death type, but much faster
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHDeathType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"impact")] Impact,
        [EnumMember(Value = @"asphyxiation")] Asphyxiation,
        [EnumMember(Value = @"energy")] Energy,
        [EnumMember(Value = @"supernova")] Supernova,
        [EnumMember(Value = @"digestion")] Digestion,
        [EnumMember(Value = @"bigBang")] BigBang,
        [EnumMember(Value = @"crushed")] Crushed,
        [EnumMember(Value = @"meditation")] Meditation,
        [EnumMember(Value = @"timeLoop")] TimeLoop,
        [EnumMember(Value = @"lava")] Lava,
        [EnumMember(Value = @"blackHole")] BlackHole,
        [EnumMember(Value = @"dream")] Dream,
        [EnumMember(Value = @"dreamExplosion")] DreamExplosion,
        [EnumMember(Value = @"crushedByElevator")] CrushedByElevator
    }
}
