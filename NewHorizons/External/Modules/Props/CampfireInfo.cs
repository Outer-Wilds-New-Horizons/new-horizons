using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class CampfireInfo : GeneralPropInfo
    {
        /// <summary>
        /// The initial state of the campfire.
        /// </summary>
        [DefaultValue("lit")] public State initialState = State.Lit;

        /// <summary>
        /// Whether the player to sleep at this campfire.
        /// </summary>
        [DefaultValue(true)] public bool canSleepHere = true;

        /// <summary>
        /// Whether the player should look up at the sky while sleeping at this campfire.
        /// </summary>
        [DefaultValue(false)] public bool lookUpWhileSleeping = false;

        public enum State
        {
            [EnumMember(Value = @"unlit")] Unlit = 0,

            [EnumMember(Value = @"smoldering")] Smoldering = 1,

            [EnumMember(Value = @"lit")] Lit = 2,
        }
    }
}
