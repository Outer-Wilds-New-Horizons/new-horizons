using NewHorizons.Builder.Props;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.External.Modules.Props.Dialogue;
using NewHorizons.External.Modules.TranslatorText;
using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using static NewHorizons.External.Modules.ShipLogModule;

namespace NewHorizons
{
    public class NewHorizonsApi : INewHorizons
    {
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config)
        {
            Create(config, null);
        }

        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config, IModBehaviour mod)
        {
            try
            {
                var name = (string)config["Name"];

                NHLogger.LogWarning($"Recieved API request to create planet [{name}]");

                if (name == null) return;

                var relativePath = $"temp/{name}.json";
                var fullPath = Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, relativePath);
                if (!Directory.Exists(Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "temp")))
                {
                    Directory.CreateDirectory(Path.Combine(Main.Instance.ModHelper.Manifest.ModFolderPath, "temp"));
                }
                JsonHelper.SaveJsonObject(fullPath, config);
                var body = Main.Instance.LoadConfig(Main.Instance, relativePath);
                File.Delete(fullPath);

                // Update it to point to their mod for textures and stuff
                body.Mod = mod ?? Main.Instance;

                if (!Main.BodyDict.ContainsKey(body.Config.starSystem)) Main.BodyDict.Add(body.Config.starSystem, new List<NewHorizonsBody>());
                Main.BodyDict[body.Config.starSystem].Add(body);
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Error in Create API:\n{ex}");
            }
        }

        [Obsolete("SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignRadial) is deprecated, please use SpawnObject(IModBehaviour mod, GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignRadial) instead")]
        public GameObject SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles,
            float scale, bool alignRadial)
        {
            return SpawnObject(null, planet, sector, propToCopyPath, position, eulerAngles, scale, alignRadial);
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyDict.Values.SelectMany(x => x)?.ToList()?.FirstOrDefault(x => x.Config.name == name)?.Object;
        }

        public string GetCurrentStarSystem() => Main.Instance.CurrentStarSystem;
        public UnityEvent<string> GetChangeStarSystemEvent() => Main.Instance.OnChangeStarSystem;
        public UnityEvent<string> GetStarSystemLoadedEvent() => Main.Instance.OnStarSystemLoaded;
        public UnityEvent<string> GetBodyLoadedEvent() => Main.Instance.OnPlanetLoaded;
        public UnityEvent<string> GetTitleScreenLoadedEvent() => Main.Instance.OnTitleScreenLoaded;
        public UnityEvent GetAllTitleScreensLoadedEvent() => Main.Instance.OnAllTitleScreensLoaded;

        public bool SetDefaultSystem(string name)
        {
            if (!Main.SystemDict.ContainsKey(name)) return false;

            Main.Instance.SetDefaultSystem(name);
            return true;
        }

        public bool ChangeCurrentStarSystem(string name)
        {
            if (!Main.SystemDict.ContainsKey(name)) return false;

            Main.Instance.ChangeCurrentStarSystem(name);
            return true;
        }

        public string[] GetInstalledAddons()
        {
            try
            {
                return Main.MountedAddons.Select(x => x?.ModHelper?.Manifest?.UniqueName).ToArray();
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Couldn't get installed addons:\n{ex}");
                return new string[] { };
            }
        }

        private static object QueryJson(Type outType, string filePath, string jsonPath)
        {
            if (filePath == "") return null;
            try
            {
                var jsonText = File.ReadAllText(filePath);
                var jsonData = JObject.Parse(jsonText);
                return jsonData.SelectToken(jsonPath)?.ToObject(outType);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (JsonException e)
            {
                NHLogger.LogError(e.ToString());
                return null;
            }
        }

        public object QueryBody(Type outType, string bodyName, string jsonPath)
        {
            var planet = Main.BodyDict[Main.Instance.CurrentStarSystem].Find((b) => b.Config.name == bodyName);
            if (planet == null){
                NHLogger.LogError($"Could not find planet with body name {bodyName}.");
				return null;
			}
			return QueryJson(outType, Path.Combine(planet.Mod.ModHelper.Manifest.ModFolderPath, planet.RelativePath), jsonPath);
        }

        public T QueryBody<T>(string bodyName, string jsonPath)
        {
            var data = QueryBody(typeof(T), bodyName, jsonPath);
            if (data is T result)
            {
                return result;
            }
            return default;
        }

        public object QuerySystem(Type outType, string jsonPath)
        {
            var system = Main.SystemDict[Main.Instance.CurrentStarSystem];
            return system == null
                ? null
                : QueryJson(outType, Path.Combine(system.Mod.ModHelper.Manifest.ModFolderPath, system.RelativePath), jsonPath);
        }

        public T QuerySystem<T>(string jsonPath)
        {
            var data = QuerySystem(typeof(T), jsonPath);
            if (data is T result)
            {
                return result;
            }
            return default;
        }

        public object QueryTitleScreen(Type outType, IModBehaviour mod, string jsonPath)
        {
            var titleScreenConfig = Main.TitleScreenConfigs[mod];
            return titleScreenConfig == null
                ? null
                : QueryJson(outType, Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "title-screen.json"), jsonPath);
        }

        public T QueryTitleScreen<T>(IModBehaviour mod, string jsonPath)
        {
            var data = QueryTitleScreen(typeof(T), mod, jsonPath);
            if (data is T result)
            {
                return result;
            }
            return default;
        }

        public GameObject SpawnObject(IModBehaviour mod, GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles,
            float scale, bool alignRadial)
        {
            var prefab = SearchUtilities.Find(propToCopyPath);
            var detailInfo = new DetailInfo()
            {
                position = position,
                rotation = eulerAngles,
                scale = scale,
                alignRadial = alignRadial
            };
            return DetailBuilder.Make(planet, sector, mod, prefab, detailInfo);
        }

        public AudioSignal SpawnSignal(IModBehaviour mod, GameObject root, string audio, string name, string frequency,
            float sourceRadius = 1f, float detectionRadius = 20f, float identificationRadius = 10f, bool insideCloak = false,
            bool onlyAudibleToScope = true, string reveals = "")
        {
            var info = new SignalInfo()
            {
                audio = audio,
                detectionRadius = detectionRadius,
                frequency = frequency,
                identificationRadius = identificationRadius,
                insideCloak = insideCloak,
                name = name,
                onlyAudibleToScope = onlyAudibleToScope,
                position = Vector3.zero,
                reveals = reveals,
                sourceRadius = sourceRadius
            };

            return SignalBuilder.Make(root, null, info, mod).GetComponent<AudioSignal>();
        }

        public (CharacterDialogueTree, RemoteDialogueTrigger) SpawnDialogue(IModBehaviour mod, GameObject root, string xmlFile, float radius = 1f,
            float range = 1f, string blockAfterPersistentCondition = null, float lookAtRadius = 1f, string pathToAnimController = null,
            float remoteTriggerRadius = 0f)
        {
            var info = new DialogueInfo()
            {
                blockAfterPersistentCondition = blockAfterPersistentCondition,
                lookAtRadius = lookAtRadius,
                pathToAnimController = pathToAnimController,
                position = Vector3.zero,
                radius = radius,
                range = range,
                xmlFile = xmlFile,
                remoteTrigger = remoteTriggerRadius > 0f ? new RemoteTriggerInfo()
                {
                    position = null,
                    radius = remoteTriggerRadius,
                } : null,
            };

            return DialogueBuilder.Make(root, null, info, mod);
        }

        public void CreatePlanet(string config, IModBehaviour mod)
        {
            try
            {
                var planet = JsonConvert.DeserializeObject<PlanetConfig>(config);
                if (planet == null)
                {
                    NHLogger.LogError($"Couldn't load planet via API. Is your Json formatted correctly? {config}");
                    return;
                }

                var body = Main.Instance.RegisterPlanetConfig(planet, mod, null);

                if (!Main.BodyDict.ContainsKey(body.Config.starSystem)) Main.BodyDict.Add(body.Config.starSystem, new List<NewHorizonsBody>());
                Main.BodyDict[body.Config.starSystem].Add(body);
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Error in CreatePlanet API:\n{ex}");
            }
        }

        public void DefineStarSystem(string name, string config, IModBehaviour mod)
        {
            var starSystemConfig = JsonConvert.DeserializeObject<StarSystemConfig>(config);
            starSystemConfig.name = name;
            Main.Instance.LoadStarSystemConfig(starSystemConfig, null, mod);
        }

        public (CharacterDialogueTree, RemoteDialogueTrigger) CreateDialogueFromXML(string textAssetID, string xml, string dialogueInfo, GameObject planetGO)
        {
            var info = JsonConvert.DeserializeObject<DialogueInfo>(dialogueInfo);
            return DialogueBuilder.Make(planetGO, null, info, xml, textAssetID);
        }

        public GameObject CreateNomaiText(string xml, string textInfo, GameObject planetGO)
        {
            var info = JsonConvert.DeserializeObject<TranslatorTextInfo>(textInfo);
            return TranslatorTextBuilder.Make(planetGO, null, info, null, xml);
        }

        public void AddShipLogXML(IModBehaviour mod, XElement xml, string planetName, string imageFolder, Dictionary<string, Vector2> entryPositions, Dictionary<string, (Color colour, Color highlight)> curiousityColours)
        {
            // This method has to be called each time the ship log manager is created, i.e. each time a system loads so it will only ever be relevant to the current one.
            var starSystem = Main.Instance.CurrentStarSystem;

            var body = new NewHorizonsBody(new PlanetConfig()
            {
                name = planetName,
                starSystem = starSystem,
                ShipLog = new ShipLogModule()
                {
                    spriteFolder = imageFolder
                }
            }, mod);

            if (!Main.BodyDict.ContainsKey(starSystem))
            {
                Main.BodyDict.Add(starSystem, new List<NewHorizonsBody>());
                Main.BodyDict[starSystem].Add(body);
            }
            else
            {
                var existingBody = Main.BodyDict[starSystem]
                    .FirstOrDefault(x => x.Config.name == planetName && x.Mod.ModHelper.Manifest.UniqueName == mod.ModHelper.Manifest.UniqueName);
                if (existingBody != null)
                {
                    body = existingBody;
                }
                else
                {
                    Main.BodyDict[starSystem].Add(body);
                }
            }

            var system = new StarSystemConfig()
            {
                name = starSystem,
                entryPositions = entryPositions?
                    .Select((pair) => new EntryPositionInfo() { id = pair.Key, position = pair.Value })
                    .ToArray(),
                curiosities = curiousityColours?
                    .Select((pair) => new CuriosityColorInfo() { id = pair.Key, color = MColor.FromColor(pair.Value.colour), highlightColor = MColor.FromColor(pair.Value.highlight) })
                    .ToArray()
            };

            Main.Instance.LoadStarSystemConfig(system, null, mod);

            RumorModeBuilder.AddShipLogXML(GameObject.FindObjectOfType<ShipLogManager>(), xml, body);
        }

        /// <summary>
        /// Register your own builder that will act on the given GameObject by reading its raw json string
        /// </summary>
        /// <param name="builder"></param>
        public void RegisterCustomBuilder(Action<GameObject, string> builder) => PlanetCreationHandler.CustomBuilders.Add(builder);

        public string GetTranslationForShipLog(string text) => TranslationHandler.GetTranslation(text, TranslationHandler.TextType.SHIPLOG);

        public string GetTranslationForDialogue(string text) => TranslationHandler.GetTranslation(text, TranslationHandler.TextType.DIALOGUE);

        public string GetTranslationForUI(string text) => TranslationHandler.GetTranslation(text, TranslationHandler.TextType.UI);

        public string GetTranslationForOtherText(string text) => TranslationHandler.GetTranslation(text, TranslationHandler.TextType.OTHER);

        public void AddSubtitle(IModBehaviour mod, string filePath) => SubtitlesHandler.RegisterAdditionalSubtitle(mod, filePath);

        public void SetNextSpawnID(string id) => PlayerSpawnHandler.TargetSpawnID = id;

        public void RegisterTitleScreenBuilder(IModBehaviour mod, Action<GameObject> builder, bool disableNHPlanets = true, bool shareTitleScreen = false, string conditionRequired = null, string factRequired = null)
             => TitleSceneHandler.RegisterBuilder(mod, builder, disableNHPlanets, shareTitleScreen, conditionRequired, factRequired);
    }
}
