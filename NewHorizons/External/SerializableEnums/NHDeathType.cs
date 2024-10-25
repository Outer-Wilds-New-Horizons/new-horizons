using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHDeathType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"impact")] Impact,
        [EnumMember(Value = @"asphyxiation")] Asphyxiation,
        [EnumMember(Value = @"energy")] Energy,

        /// <summary>
        /// Special death type used when the supernova hits you. You will not wake up if in the Dreamworld. 
        /// </summary>
        [EnumMember(Value = @"supernova")] Supernova,

        /// <summary>
        /// Death type used by anglerfish (and cut-content ghosts and water monster)
        /// </summary>
        [EnumMember(Value = @"digestion")] Digestion,

        /// <summary>
        /// Special death type used at the end of the game
        /// </summary>
        [EnumMember(Value = @"bigBang")] BigBang,
        [EnumMember(Value = @"crushed")] Crushed,

        /// <summary>
        /// Special death type used when skipping to the next loop. You will not wake up if in the Dreamworld. 
        /// </summary>
        [EnumMember(Value = @"meditation")] Meditation,

        /// <summary>
        /// Special death type used when the time loop ends. You will not wake up if in the Dreamworld. 
        /// </summary>
        [EnumMember(Value = @"timeLoop")] TimeLoop,

        [EnumMember(Value = @"lava")] Lava,

        /// <summary>
        /// Special death type used by the ATP blackhole (and custom NH blackholes without whitehole destinations)
        /// </summary>
        [EnumMember(Value = @"blackHole")] BlackHole,

        /// <summary>
        /// Special DLC death type used to kill a player in the real world while in the Dreamworld (i.e., you will loop not wake up)
        /// </summary>
        [EnumMember(Value = @"dream")] Dream,

        /// <summary>
        /// Special DLC death type used to kill a player in the real world while in the Dreamworld (i.e., you will loop not wake up)
        /// </summary>
        [EnumMember(Value = @"dreamExplosion")] DreamExplosion,

        /// <summary>
        /// Similar to the Crushed death type, but much faster
        /// </summary>
        [EnumMember(Value = @"crushedByElevator")] CrushedByElevator
    }
}
