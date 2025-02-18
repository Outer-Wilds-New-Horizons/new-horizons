using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Conditionals
{
    [JsonObject]
    public class ConditionalCheckInfo
    {
        /// <summary>
        /// The conditions that must be met for the check to pass
        /// </summary>
        public ConditionalCheckConditionsInfo check;
        /// <summary>
        /// The effects of the check if it passes
        /// </summary>
        public ConditionalCheckEffectsInfo then;
    }

    [JsonObject]
    public class ConditionalCheckConditionsInfo
    {
        /// <summary>
        /// The check will only pass if all of these dialogue conditions are set
        /// </summary>
        public string[] allConditionsSet;
        /// <summary>
        /// The check will only pass if any of these dialogue conditions are set
        /// </summary>
        public string[] anyConditionsSet;
        /// <summary>
        /// The check will only pass if all of these persistent conditions are set
        /// </summary>
        public string[] allPersistentConditionsSet;
        /// <summary>
        /// The check will only pass if any of these persistent conditions are set
        /// </summary>
        public string[] anyPersistentConditionsSet;
        /// <summary>
        /// The check will only pass if all of these ship log facts are revealed
        /// </summary>
        public string[] allFactsRevealed;
        /// <summary>
        /// The check will only pass if any of these ship log facts are revealed
        /// </summary>
        public string[] anyFactsRevealed;

        /// <summary>
        /// If the check should pass only if the conditions are not met
        /// </summary>
        public bool invert;
    }

    [JsonObject]
    public class ConditionalCheckEffectsInfo
    {
        /// <summary>
        /// The check will set these dialogue conditions if it passes
        /// </summary>
        public string[] setConditions;
        /// <summary>
        /// The check will unset these dialogue conditions if it passes
        /// </summary>
        public string[] unsetConditions;
        /// <summary>
        /// The check will set these persistent conditions if it passes
        /// </summary>
        public string[] setPersistentConditions;
        /// <summary>
        /// The check will unset these persistent conditions if it passes
        /// </summary>
        public string[] unsetPersistentConditions;
        /// <summary>
        /// The check will reveal these ship log facts if it passes
        /// </summary>
        public string[] revealFacts;

        /// <summary>
        /// If the check should undo its effects if the conditions are not met anymore (unset the set conditions, etc.). Note: ship log facts cannot currently be unrevealed.
        /// </summary>
        public bool reversible;
    }
}
