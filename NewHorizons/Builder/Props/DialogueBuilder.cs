using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props.Dialogue;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace NewHorizons.Builder.Props
{
    public static class DialogueBuilder
    {
        // Returns the character dialogue tree and remote dialogue trigger, if applicable.
        public static (CharacterDialogueTree, RemoteDialogueTrigger) Make(GameObject go, Sector sector, DialogueInfo info, IModBehaviour mod)
        {
            var xml = File.ReadAllText(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, info.xmlFile));
            var dialogueName = Path.GetFileNameWithoutExtension(info.xmlFile);
            return Make(go, sector, info, xml, dialogueName);
        }

        // Create dialogue directly from xml string instead of loading it from a file
        public static (CharacterDialogueTree, RemoteDialogueTrigger) Make(GameObject go, Sector sector, DialogueInfo info, string xml, string dialogueName)
        {
            if (string.IsNullOrEmpty(info.pathToExistingDialogue))
            {
                return MakeNewDialogue(go, sector, info, xml, dialogueName);
            }
            else
            {
                return (AddToExistingDialogue(info, xml), null);
            }
        }

        private static (CharacterDialogueTree, RemoteDialogueTrigger) MakeNewDialogue(GameObject go, Sector sector, DialogueInfo info, string xml, string dialogueName)
        { 
            NHLogger.LogVerbose($"[DIALOGUE] Created a new character dialogue [{info.rename}] on [{info.parentPath}]");

            // In stock I think they disable dialogue stuff with conditions
            // Here we just don't make it at all
            if (!string.IsNullOrEmpty(info.blockAfterPersistentCondition) && PlayerData.GetPersistentCondition(info.blockAfterPersistentCondition))
            {
                NHLogger.LogVerbose($"[DIALOGUE] Persistent condition [{info.blockAfterPersistentCondition}] was met for [{info.rename}], aborting");
                return (null, null);
            }

            var dialogue = MakeConversationZone(go, sector, info, xml, dialogueName);
            
            RemoteDialogueTrigger remoteTrigger = null;
            if (info.remoteTrigger != null)
            {
                remoteTrigger = MakeRemoteDialogueTrigger(go, sector, info, dialogue);
            }

            // Make the character look at the player
            // Useful for dialogue replacement
            // Overrides parent path for dialogue
            if (!string.IsNullOrEmpty(info.pathToAnimController))
            {
                MakePlayerTrackingZone(go, dialogue, info);
            }

            return (dialogue, remoteTrigger);
        }

        private static CharacterDialogueTree AddToExistingDialogue(DialogueInfo info, string xml)
        {
            var existingDialogue = SearchUtilities.Find(info.pathToExistingDialogue)?.GetComponent<CharacterDialogueTree>();

            if (existingDialogue == null)
            {
                NHLogger.LogError($"Couldn't find dialogue at {info.pathToExistingDialogue}!");
                return null;
            }

            var existingText = existingDialogue._xmlCharacterDialogueAsset.text;

            var existingDialogueDoc = new XmlDocument();
            existingDialogueDoc.LoadXml(existingText);
            var existingDialogueTree = existingDialogueDoc.DocumentElement.SelectSingleNode("//DialogueTree");

            var existingDialogueNodesByName = new Dictionary<string, XmlNode>();
            foreach (XmlNode existingDialogueNode in existingDialogueTree.GetChildNodes("DialogueNode"))
            {
                var name = existingDialogueNode.GetChildNode("Name").InnerText;
                existingDialogueNodesByName[name] = existingDialogueNode;
            }

            var additionalDialogueDoc = new XmlDocument();
            additionalDialogueDoc.LoadXml(xml);
            var newDialogueNodes = additionalDialogueDoc.DocumentElement.SelectSingleNode("//DialogueTree").GetChildNodes("DialogueNode");

            foreach (XmlNode newDialogueNode in newDialogueNodes)
            {
                var name = newDialogueNode.GetChildNode("Name").InnerText;

                if (existingDialogueNodesByName.TryGetValue(name, out var existingNode))
                {
                    // We just have to merge the dialogue options
                    var dialogueOptions = newDialogueNode.GetChildNode("DialogueOptionsList").GetChildNodes("DialogueOption");
                    var existingDialogueOptionsList = existingNode.GetChildNode("DialogueOptionsList");
                    if (existingDialogueOptionsList == null)
                    {
                        existingDialogueOptionsList = existingDialogueDoc.CreateElement("DialogueOptionsList");
                        existingNode.AppendChild(existingDialogueOptionsList);
                    }
                    foreach (XmlNode node in dialogueOptions)
                    {
                        var importedNode = existingDialogueOptionsList.OwnerDocument.ImportNode(node, true);
                        // We add them to the start because normally the last option is to return to menu or exit
                        existingDialogueOptionsList.PrependChild(importedNode);
                    }
                }
                else
                {
                    // We add the new dialogue node to the existing dialogue
                    var importedNode = existingDialogueTree.OwnerDocument.ImportNode(newDialogueNode, true);
                    existingDialogueTree.AppendChild(importedNode);
                }
            }

            // Character name is required for adding translations, something to do with how OW prefixes its dialogue
            var characterName = existingDialogueTree.SelectSingleNode("NameField").InnerText;
            AddTranslation(additionalDialogueDoc.GetChildNode("DialogueTree"), characterName);

            DoDialogueOptionsListReplacement(existingDialogueTree);

            var newTextAsset = new TextAsset(existingDialogueDoc.OuterXml)
            {
                name = existingDialogue._xmlCharacterDialogueAsset.name
            };

            existingDialogue.SetTextXml(newTextAsset);

            return existingDialogue;
        }

        /// <summary>
        /// Always call this after adding translations, else it won't update them properly
        /// </summary>
        /// <param name="dialogueTree"></param>
        private static void DoDialogueOptionsListReplacement(XmlNode dialogueTree)
        {
            var optionsListsByName = new Dictionary<string, XmlNode>();
            var dialogueNodes = dialogueTree.GetChildNodes("DialogueNode");
            foreach (XmlNode dialogueNode in dialogueNodes)
            {
                var optionsList = dialogueNode.GetChildNode("DialogueOptionsList");
                if (optionsList != null)
                {
                    var name = dialogueNode.GetChildNode("Name").InnerText;
                    optionsListsByName[name] = optionsList;
                }
            }
            foreach (var (name, optionsList) in optionsListsByName)
            {
                var replacement = optionsList.GetChildNode("ReuseDialogueOptionsListFrom");
                if (replacement != null)
                {
                    var replacementName = replacement.InnerText;
                    if (optionsListsByName.TryGetValue(replacementName, out var replacementOptionsList))
                    {
                        if (replacementOptionsList.GetChildNode("ReuseDialogueOptionsListFrom") != null)
                        {
                            NHLogger.LogError($"Can not target a node with ReuseDialogueOptionsListFrom that also reuses options when making dialogue. Node {name} cannot reuse the list from {replacement.InnerText}");
                        }
                        var dialogueNode = optionsList.ParentNode;
                        dialogueNode.RemoveChild(optionsList);
                        dialogueNode.AppendChild(replacementOptionsList.Clone());

                        // Have to manually fix the translations here
                        var characterName = dialogueTree.SelectSingleNode("NameField").InnerText;

                        var xmlText = replacementOptionsList.SelectNodes("DialogueOption/Text");
                        foreach (object option in xmlText)
                        {
                            var optionData = (XmlNode)option;
                            var text = optionData.InnerText.Trim();
                            TranslationHandler.ReuseDialogueTranslation(text, new string[] { characterName, replacementName }, new string[] { characterName, name });
                        }
                    }
                    else
                    {
                        NHLogger.LogError($"Could not reuse dialogue options list from node with Name {replacement.InnerText} to node with Name {name}");
                    }
                }
            }
        }

        private static RemoteDialogueTrigger MakeRemoteDialogueTrigger(GameObject planetGO, Sector sector, DialogueInfo info, CharacterDialogueTree dialogue)
        {
            var conversationTrigger = GeneralPropBuilder.MakeNew("ConversationTrigger", planetGO, sector, info.remoteTrigger, defaultPosition: info.position, defaultParentPath: info.pathToAnimController);

            var remoteDialogueTrigger = conversationTrigger.AddComponent<RemoteDialogueTrigger>();
            var sphereCollider = conversationTrigger.AddComponent<SphereCollider>();
            conversationTrigger.AddComponent<OWCollider>();

            remoteDialogueTrigger._listDialogues = new RemoteDialogueTrigger.RemoteDialogueCondition[]
            {
                new RemoteDialogueTrigger.RemoteDialogueCondition()
                {
                    priority = 1,
                    dialogue = dialogue,
                    prereqConditionType = RemoteDialogueTrigger.MultiConditionType.AND,
                    // Base game never uses more than one condition anyone so we'll keep it simple
                    prereqConditions = string.IsNullOrEmpty(info.remoteTrigger.prereqCondition) ? new string[]{ } : new string[] { info.remoteTrigger.prereqCondition },
                    // Just set your enter conditions in XML instead of complicating it with this
                    onTriggerEnterConditions = new string[]{ }
                }
            };
            remoteDialogueTrigger._activatedDialogues = new bool[1];
            remoteDialogueTrigger._deactivateTriggerPostConversation = true;

            sphereCollider.radius = info.remoteTrigger.radius == 0 ? info.radius : info.remoteTrigger.radius;

            conversationTrigger.SetActive(true);

            return remoteDialogueTrigger;
        }

        private static CharacterDialogueTree MakeConversationZone(GameObject planetGO, Sector sector, DialogueInfo info, string xml, string dialogueName)
        {
            var conversationZone = GeneralPropBuilder.MakeNew("ConversationZone", planetGO, sector, info, defaultParentPath: info.pathToAnimController);

            conversationZone.layer = Layer.Interactible;

            var sphere = conversationZone.AddComponent<SphereCollider>();
            sphere.radius = info.radius;
            sphere.isTrigger = true;

            var owCollider = conversationZone.AddComponent<OWCollider>();
            var interact = conversationZone.AddComponent<InteractReceiver>();

            interact._interactRange = info.range;

            if (info.radius <= 0)
            {
                sphere.enabled = false;
                owCollider.enabled = false;
                interact.enabled = false;
            }

            var dialogueTree = conversationZone.AddComponent<NHCharacterDialogueTree>();

            var dialogueDoc = new XmlDocument();
            dialogueDoc.LoadXml(xml);
            var xmlNode = dialogueDoc.SelectSingleNode("DialogueTree");
            AddTranslation(xmlNode);
            DoDialogueOptionsListReplacement(xmlNode);
            xml = xmlNode.OuterXml;

            var text = new TextAsset(xml)
            {
                // Text assets need a name to be used with VoiceMod
                name = dialogueName
            };
            dialogueTree.SetTextXml(text);

            switch (info.flashlightToggle)
            {
                case FlashlightToggle.TurnOff:
                    dialogueTree._turnOffFlashlight = true;
                    dialogueTree._turnOnFlashlight = false;
                    break;
                case FlashlightToggle.TurnOffThenOn:
                    dialogueTree._turnOffFlashlight = true;
                    dialogueTree._turnOnFlashlight = true;
                    break;
                case FlashlightToggle.None:
                default:
                    dialogueTree._turnOffFlashlight = false;
                    dialogueTree._turnOnFlashlight = false;
                    break;
            }

            conversationZone.SetActive(true);

            return dialogueTree;
        }

        private static void MakePlayerTrackingZone(GameObject go, CharacterDialogueTree dialogue, DialogueInfo info)
        {
            var character = go.transform.Find(info.pathToAnimController);

            if (character == null)
            {
                NHLogger.LogError($"Couldn't find child of {go.transform.GetPath()} at {info.pathToAnimController}");
                return;
            }

            // At most one of these should ever not be null
            var nomaiController = character.GetComponent<SolanumAnimController>();
            var controller = character.GetComponent<CharacterAnimController>();
            var traveler = character.GetComponent<TravelerController>();
            var travelerEye = character.GetComponent<TravelerEyeController>();
            var hearthianRecorder = character.GetComponent<HearthianRecorderEffects>();

            var lookOnlyWhenTalking = info.lookAtRadius <= 0;

            // To have them look when you start talking
            if (controller != null)
            {
                if (controller._dialogueTree != null)
                {
                    controller._dialogueTree.OnStartConversation -= controller.OnStartConversation;
                    controller._dialogueTree.OnEndConversation -= controller.OnEndConversation;
                }

                controller._dialogueTree = dialogue;
                controller.lookOnlyWhenTalking = lookOnlyWhenTalking;
                controller._dialogueTree.OnStartConversation += controller.OnStartConversation;
                controller._dialogueTree.OnEndConversation += controller.OnEndConversation;
            }
            else if (traveler != null)
            {
                if (traveler._dialogueSystem != null)
                {
                    traveler._dialogueSystem.OnStartConversation -= traveler.OnStartConversation;
                    traveler._dialogueSystem.OnEndConversation -= traveler.OnEndConversation;
                }

                traveler._dialogueSystem = dialogue;
                traveler._dialogueSystem.OnStartConversation += traveler.OnStartConversation;
                traveler._dialogueSystem.OnEndConversation += traveler.OnEndConversation;
            }
            else if (travelerEye != null)
            {
                if (travelerEye._dialogueTree != null)
                {
                    travelerEye._dialogueTree.OnStartConversation -= travelerEye.OnStartConversation;
                    travelerEye._dialogueTree.OnEndConversation -= travelerEye.OnEndConversation;
                }

                travelerEye._dialogueTree = dialogue;
                travelerEye._dialogueTree.OnStartConversation += travelerEye.OnStartConversation;
                travelerEye._dialogueTree.OnEndConversation += travelerEye.OnEndConversation;
            }
            else if (nomaiController != null)
            {
                if (lookOnlyWhenTalking)
                {
                    dialogue.OnStartConversation += nomaiController.StartWatchingPlayer;
                    dialogue.OnEndConversation += nomaiController.StopWatchingPlayer;
                }
            }
            else if (hearthianRecorder != null)
            {
                Delay.FireOnNextUpdate(() =>
                {
                    // #520
                    if (hearthianRecorder._characterDialogueTree != null)
                    {
                        hearthianRecorder._characterDialogueTree.OnStartConversation -= hearthianRecorder.OnPlayRecorder;
                        hearthianRecorder._characterDialogueTree.OnEndConversation -= hearthianRecorder.OnStopRecorder;
                    }

                    // Recorder props have their own dialogue on them already
                    // Make sure to delete it when we're trying to connect new dialogue to it
                    var existingDialogue = hearthianRecorder.GetComponent<CharacterDialogueTree>();
                    if (existingDialogue != dialogue && existingDialogue != null)
                    {
                        // Can't delete the existing dialogue because its a required component but we can make it unable to select at least
                        GameObject.Destroy(hearthianRecorder.GetComponent<OWCollider>());
                        GameObject.Destroy(hearthianRecorder.GetComponent<SphereCollider>());
                        GameObject.Destroy(existingDialogue._interactVolume);
                        existingDialogue.enabled = false;
                    }

                    hearthianRecorder._characterDialogueTree = dialogue;
                    hearthianRecorder._characterDialogueTree.OnStartConversation += hearthianRecorder.OnPlayRecorder;
                    hearthianRecorder._characterDialogueTree.OnEndConversation += hearthianRecorder.OnStopRecorder;
                });
            }
            else
            {
                // If they have nothing else just put the face player when talking thing on them
                character.gameObject.GetAddComponent<FacePlayerWhenTalking>();
            }

            var facePlayerWhenTalking = character.GetComponent<FacePlayerWhenTalking>();
            if (facePlayerWhenTalking != null)
            {
                if (facePlayerWhenTalking._dialogueTree != null)
                {
                    facePlayerWhenTalking._dialogueTree.OnStartConversation -= facePlayerWhenTalking.OnStartConversation;
                    facePlayerWhenTalking._dialogueTree.OnEndConversation -= facePlayerWhenTalking.OnEndConversation;
                }

                facePlayerWhenTalking._dialogueTree = dialogue;
                facePlayerWhenTalking._dialogueTree.OnStartConversation += facePlayerWhenTalking.OnStartConversation;
                facePlayerWhenTalking._dialogueTree.OnEndConversation += facePlayerWhenTalking.OnEndConversation;
            }

            if (info.lookAtRadius > 0)
            {
                var playerTrackingZone = new GameObject("PlayerTrackingZone");
                playerTrackingZone.SetActive(false);

                playerTrackingZone.layer = Layer.BasicEffectVolume;
                playerTrackingZone.SetActive(false);

                var sphereCollider = playerTrackingZone.AddComponent<SphereCollider>();
                sphereCollider.radius = info.lookAtRadius;
                sphereCollider.isTrigger = true;

                playerTrackingZone.AddComponent<OWCollider>();

                var triggerVolume = playerTrackingZone.AddComponent<OWTriggerVolume>();

                if (controller)
                {
                    // Since the Awake method is CharacterAnimController was already called 
                    if (controller.playerTrackingZone)
                    {
                        controller.playerTrackingZone.OnEntry -= controller.OnZoneEntry;
                        controller.playerTrackingZone.OnExit -= controller.OnZoneExit;
                    }
                    // Set it to use the new zone
                    controller.playerTrackingZone = triggerVolume;
                    triggerVolume.OnEntry += controller.OnZoneEntry;
                    triggerVolume.OnExit += controller.OnZoneExit;
                }
                // Simpler for the Nomai
                else if (nomaiController)
                {
                    triggerVolume.OnEntry += (_) => nomaiController.StartWatchingPlayer();
                    triggerVolume.OnExit += (_) => nomaiController.StopWatchingPlayer();
                }
                // No controller
                else
                {
                    // TODO
                }

                playerTrackingZone.transform.parent = dialogue.gameObject.transform;
                playerTrackingZone.transform.localPosition = Vector3.zero;

                playerTrackingZone.SetActive(true);
            }
        }

        [Obsolete("Pass in the DialogueTree XmlNode instead, this is still here because Pikpik was using it in EOTP")]
        public static void AddTranslation(string xml, string characterName = null)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            var xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            AddTranslation(xmlNode, characterName);
        }

        public static void AddTranslation(XmlNode xmlNode, string characterName = null) 
        { 
            var xmlNodeList = xmlNode.SelectNodes("DialogueNode");

            // When adding dialogue to existing stuff, we have to pass in the character name
            // Otherwise we translate it if its from a new dialogue object
            if (characterName == null)
            {
                characterName = xmlNode.SelectSingleNode("NameField").InnerText;
                TranslationHandler.AddDialogue(characterName);
            }

            foreach (object obj in xmlNodeList)
            {
                var xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                var xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object page in xmlText)
                {
                    var pageData = (XmlNode)page;
                    var text = pageData.InnerText;
                    // The text is trimmed in DialogueText constructor (_listTextBlocks), so we also need to trim it for the key
                    TranslationHandler.AddDialogue(text, true, name);
                }

                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");
                foreach (object option in xmlText)
                {
                    var optionData = (XmlNode)option;
                    var text = optionData.InnerText;
                    // The text is trimmed in CharacterDialogueTree.LoadXml, so we also need to trim it for the key
                    TranslationHandler.AddDialogue(text, true, characterName, name);
                }
            }
        }

        public static void HandleUnityCreatedDialogue(CharacterDialogueTree dialogue)
        {
            var text = dialogue._xmlCharacterDialogueAsset.text;
            var dialogueDoc = new XmlDocument();
            dialogueDoc.LoadXml(text);
            var xmlNode = dialogueDoc.SelectSingleNode("DialogueTree");
            AddTranslation(xmlNode, null);
            DoDialogueOptionsListReplacement(xmlNode);
            var newTextAsset = new TextAsset(dialogueDoc.OuterXml)
            {
                name = dialogue._xmlCharacterDialogueAsset.name
            };
            dialogue.SetTextXml(newTextAsset);
        }
    }
}
