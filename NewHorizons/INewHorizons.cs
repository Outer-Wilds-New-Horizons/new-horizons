using OWML.Common;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NewHorizons
{
    public interface INewHorizons
    {
        #region Obsolete
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        void Create(Dictionary<string, object> config);

        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        void Create(Dictionary<string, object> config, IModBehaviour mod);

        [Obsolete("SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignRadial) is deprecated, please use SpawnObject(IModBehaviour mod, GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignRadial) instead")]
        GameObject SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignWithNormal);
        #endregion

        /// <summary>
        /// Will load all configs in the regular folders (planets, systems, translations, etc) for this mod.
        /// The NH addon config template is just a single call to this API method.
        /// </summary>
        void LoadConfigs(IModBehaviour mod);

        /// <summary>
        /// Retrieve the root GameObject of a custom planet made by creating configs. 
        /// Will only work if the planet has been created (see GetStarSystemLoadedEvent)
        /// </summary>
        GameObject GetPlanet(string name);

        /// <summary>
        /// Returns the uniqueIDs of each installed NH addon.
        /// </summary>
        string[] GetInstalledAddons();

        #region Get/set/change star system
        /// <summary>
        /// The name of the current star system loaded.
        /// </summary>
        string GetCurrentStarSystem();

        /// <summary>
        /// Allows you to overwrite the default system. This is where the player is respawned after dying.
        /// </summary>
        bool SetDefaultSystem(string name);

        /// <summary>
        /// Allows you to instantly begin a warp to a new star system.
        /// Will return false if that system does not exist (cannot be warped to).
        /// </summary>
        bool ChangeCurrentStarSystem(string name);
        #endregion

        #region events
        /// <summary>
        /// An event invoked when the player begins loading the new star system, before the scene starts to load.
        /// Gives the name of the star system being switched to.
        /// </summary>
        UnityEvent<string> GetChangeStarSystemEvent();

        /// <summary>
        /// An event invoked when NH has finished generating all planets for a new star system.
        /// Gives the name of the star system that was just loaded.
        /// </summary>
        UnityEvent<string> GetStarSystemLoadedEvent();

        /// <summary>
        /// An event invoked when NH has finished a planet for a star system.
        /// Gives the name of the planet that was just loaded.
        /// </summary>
        UnityEvent<string> GetBodyLoadedEvent();
        #endregion

        #region Querying configs
        /// <summary>
        /// Uses JSONPath to query a body
        /// </summary>
        object QueryBody(Type outType, string bodyName, string path);

        ///<summary>
        /// Uses JSONPath to query a body
        /// </summary>
        T QueryBody<T>(string bodyName, string path);

        /// <summary>
        /// Uses JSONPath to query the current star system
        /// </summary>
        object QuerySystem(Type outType, string path);

        ///<summary>
        /// Uses JSONPath to query the current star system
        ///</summary>
        T QuerySystem<T>(string path);

        /// <summary>
        /// Register your own builder that will act on the given GameObject by reading the json string of its "extras" module
        /// </summary>
        void RegisterCustomBuilder(Action<GameObject, string> builder); 
        #endregion

        #region Spawn props
        /// <summary>
        /// Allows you to spawn a copy of a prop by specifying its path.
        /// This is the same as using Props->details in a config, but also returns the spawned gameObject to you.
        /// </summary>
        GameObject SpawnObject(IModBehaviour mod, GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, 
            float scale, bool alignWithNormal);

        /// <summary>
        /// Allows you to spawn an AudioSignal on a planet.
        /// This is the same as using Props->signals in a config, but also returns the spawned AudioSignal to you.
        /// This method will not set its position. You will have to do that with the returned object.
        /// </summary>
        AudioSignal SpawnSignal(IModBehaviour mod, GameObject root, string audio, string name, string frequency,
            float sourceRadius = 1f, float detectionRadius = 20f, float identificationRadius = 10f, bool insideCloak = false,
            bool onlyAudibleToScope = true, string reveals = "");

        /// <summary>
        /// Allows you to spawn character dialogue on a planet. Also returns the RemoteDialogueTrigger if remoteTriggerRadius is specified.
        /// This is the same as using Props->dialogue in a config, but also returns the spawned game objects to you.
        /// This method will not set the position of the dialogue or remote trigger. You will have to do that with the returned objects.
        /// </summary>
        (CharacterDialogueTree, RemoteDialogueTrigger) SpawnDialogue(IModBehaviour mod, GameObject root, string xmlFile, float radius = 1f,
            float range = 1f, string blockAfterPersistentCondition = null, float lookAtRadius = 1f, string pathToAnimController = null,
            float remoteTriggerRadius = 0f);
        #endregion

        #region Load json/xml directly
        /// <summary>
        /// Allows creation of a planet by directly passing the json contents as a string.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="mod"></param>
        void CreatePlanet(string config, IModBehaviour mod);

        /// <summary>
        /// Allows defining details of a star system by directly passing the json contents as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <param name="mod"></param>
        void DefineStarSystem(string name, string config, IModBehaviour mod);

        /// <summary>
        /// Allows creation of dialogue by directly passing the xml and dialogueInfo json contents as strings. 
        /// Must be called at least 2 frames before entering dialogue if you're using ReuseDialogueOptionsFrom
        /// </summary>
        /// <param name="textAssetID">TextAsset name used for compatibility with voice mod. Just has to be a unique identifier.</param>
        /// <param name="xml">The contents of the dialogue xml file as a string</param>
        /// <param name="dialogueInfo">The json dialogue info as a string. See the documentation/schema for what this can contain.</param>
        /// <param name="planetGO">The root planet rigidbody that this dialogue is attached to. Any paths in the dialogueInfo are relative to this body.</param>
        /// <returns></returns>
        (CharacterDialogueTree, RemoteDialogueTrigger) CreateDialogueFromXML(string textAssetID, string xml, string dialogueInfo, GameObject planetGO);

        /// <summary>
        /// Allows the creation of Nomai text by directly passing the xml and translatorTextInfo json contents as strings
        /// </summary>
        /// <param name="xml">The contents of the translator text file as a string</param>
        /// <param name="textInfo">The json translator text info as a string. See the documentation/schema for what this can contain.</param>
        /// <param name="planetGO">The root planet rigidbody that this text is attached to. Any paths in the translatorTextInfo are relative to this body.</param>
        /// <returns></returns>
        GameObject CreateNomaiText(string xml, string textInfo, GameObject planetGO);

        /// <summary>
        /// Directly add ship logs from XML. Call this method right before ShipLogManager awake.
        /// </summary>
        /// <param name="mod">The mod this method is being called from. This is required for loading files.</param>
        /// <param name="xml">The ship log xml contents</param>
        /// <param name="planetName">The planet that these ship logs correspond to in the map mode</param>
        /// <param name="imageFolder">The relative path from your mod manifest.json where the ship log images are located. The filename must be the same as the fact id. Optional.</param>
        /// <param name="entryPositions">A dictionary of each fact id to its 2D position in the ship log. Optional.</param>
        /// <param name="curiousityColours">A dictionary of each curiousity ID to its colour and highlight colour in the ship log. Optional.</param>
        void AddShipLogXML(IModBehaviour mod, XElement xml, string planetName, string imageFolder = null, Dictionary<string, Vector2> entryPositions = null, Dictionary<string, (Color colour, Color highlight)> curiousityColours = null);
        #endregion

        #region Translations
        /// <summary>
        /// Look up shiplog-related translated text for the given text key.
        /// Defaults to English if no translation in the current language is available, and just the key if no English translation is available.
        /// </summary>
        /// <param name="text">The text key to look up.</param>
        /// <returns></returns>
        string GetTranslationForShipLog(string text);
        /// <summary>
        /// Look up dialogue-related translated text for the given text key.
        /// Defaults to English if no translation in the current language is available, and just the key if no English translation is available.
        /// </summary>
        /// <param name="text">The text key to look up.</param>
        /// <returns></returns>
        string GetTranslationForDialogue(string text);
        /// <summary>
        /// Look up UI-related translated text for the given text key.
        /// Defaults to English if no translation in the current language is available, and just the key if no English translation is available.
        /// </summary>
        /// <param name="text">The text key to look up.</param>
        /// <returns></returns>
        string GetTranslationForUI(string text);
        /// <summary>
        /// Look up miscellaneous translated text for the given text key.
        /// Defaults to English if no translation in the current language is available, and just the key if no English translation is available.
        /// </summary>
        /// <param name="text">The text key to look up.</param>
        /// <returns></returns>
        string GetTranslationForOtherText(string text);
        #endregion

        /// <summary>
        /// Registers a subtitle for the main menu.
        /// Call this once before the main menu finishes loading
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="filePath"></param>
        void AddSubtitle(IModBehaviour mod, string filePath);

        /// <summary>
        /// Whatever system the player is warping to next, they will spawn at the spawn point with this ID
        /// Gets reset after warping. Is also overriden by entering a system-changing black hole or warp volume by their `spawnPointID`
        /// </summary>
        /// <param name="id"></param>
        void SetNextSpawnID(string id);
    }
}
