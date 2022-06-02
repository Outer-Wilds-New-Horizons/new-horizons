using NewHorizons.Builder.Props;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.DebugUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NewHorizons.External.Modules.PropModule;

namespace NewHorizons.Utility.DebugMenu
{

    /*
     * Strategy:
     * load all existing nomai text and allow the user to select them from a list
     * from there, allow them to edit the placement of the one selected
     */

    class DebugMenuNomaiText : DebugSubmenu
    {
        internal DebugRaycaster _drc;
        internal DebugNomaiTextPlacer _dnp;

        class ConversationMetadata
        {
            public NomaiTextInfo conversation;
            public GameObject conversationGo;
            public PlanetConfig planetConfig;

            public List<SpiralMetadata> spirals;

            public bool collapsed;
        }

        class SpiralMetadata
        {
            public NomaiTextArcInfo spiral;
            public GameObject spiralGo;

            public NomaiTextInfo conversation;
            public PlanetConfig planetConfig;

            public string planetName;
            public int id;
        }

        List<ConversationMetadata> conversations = new List<ConversationMetadata>();

        // menu stuff
        Vector2 conversationsScrollPosition = new Vector2();

        internal override string SubmenuName()
        {
            return "Text Placer";
        }

        internal override void OnInit(DebugMenu menu)
        {
            _drc = menu.GetComponent<DebugRaycaster>();
            _dnp = menu.GetComponent<DebugNomaiTextPlacer>();
        }

        internal override void OnAwake(DebugMenu menu)
        {
            _drc = menu.GetComponent<DebugRaycaster>();
            _dnp = menu.GetComponent<DebugNomaiTextPlacer>();
        }

        internal override void OnBeginLoadMod(DebugMenu debugMenu) {}

        internal override void GainActive() {} // intentionally blank. do not set `DebugNomaiTextPlacer.active = true;` here

        internal override void LoseActive()
        {
            DebugNomaiTextPlacer.active = false;
        }
        
        internal override void LoadConfigFile(DebugMenu menu, PlanetConfig config)
        {
            if (config?.Props?.nomaiText == null) return;

            foreach(NomaiTextInfo conversation in config.Props.nomaiText)
            {
                ConversationMetadata conversationMetadata = new ConversationMetadata()
                {
                    conversation = conversation,
                    conversationGo = NomaiTextBuilder.GetSpawnedGameObjectByNomaiTextInfo(conversation),
                    planetConfig = config,
                    spirals = new List<SpiralMetadata>(),
                    collapsed = true
                };

                Logger.Log("adding go " + conversationMetadata.conversationGo);

                conversations.Add(conversationMetadata);

                var numArcs = conversation.arcInfo == null
                    ? 0
                    : conversation.arcInfo.Length;
                for(int id = 0; id < numArcs; id++)
                {
                    NomaiTextArcInfo arcInfo = conversation.arcInfo[id];

                    SpiralMetadata metadata = new SpiralMetadata()
                    {
                        spiral = arcInfo,
                        spiralGo = NomaiTextBuilder.GetSpawnedGameObjectByNomaiTextArcInfo(arcInfo),
                        conversation = conversation,
                        planetConfig = config,
                        planetName = config.name,
                        id = id
                    };

                    conversationMetadata.spirals.Add(metadata);
                }
            }
        }


        /*
           Conversations:                                                
           +---------------------------+                            
           |1) comment                 |
           |   |o Visible| |Place|     | // replace is greyed out if the user is currently replacing this plane (replacing is done after the user presses G
           |2) ...                     |                                        
           +---------------------------+
                                                                    
           Spirals:                                
           +---------------------------+                     
           | v Planet - Comment        |
           | +----------------------+  | 
           | | v ID                 |  |                            
           | |                      |  |                            
           | | | > Surface 2 |      |  |                            
           | | x: 5  ---o---        |  |                            
           | | y: 2  ---o---        |  |                            
           | | theta: 45 ---o---    |  |                            
           | |                      |  |                            
           | | o Child o Adult o ...|  |                            
           | | variation: o1 o2 o3..|  |                            
           | +----------------------+  |                            
           |                           |                            
           | +----------------------+  |                            
           | | > ID                 |  |                            
           | +----------------------+  |                            
           |                           |                            
           | ...                       |                            
           +---------------------------+ 
        
           +---------------------------+                                                        
           | > Planet - Comment        |                            
           +---------------------------+   
           ...
                                                                    
         */
        internal override void OnGUI(DebugMenu menu)
        {
            conversationsScrollPosition = GUILayout.BeginScrollView(conversationsScrollPosition);
            
            for(int i = 0; i < conversations.Count(); i++)
            {
                ConversationMetadata conversationMeta = conversations[i];

                GUILayout.BeginHorizontal();
                    GUILayout.Space(5);        

                    GUILayout.BeginVertical(menu._editorMenuStyle);
                        
                        var arrow = conversationMeta.collapsed ? " > " : " v ";
                        if (GUILayout.Button(arrow + conversationMeta.planetConfig.name + " - " + i)) 
                        {
                            conversationMeta.collapsed = !conversationMeta.collapsed;
                            Logger.Log("BUTTON " + i);
                        }

                        if (!conversationMeta.collapsed)
                        {
                            GUILayout.Space(5);
                            // button to set this one to place with a raycast
                            GUILayout.Label("Conversation");
                            
                            // only show the button if this conversation is a wall text, do not show it if it is a scroll text or something
                            if (
                                conversationMeta.conversation.type == PropModule.NomaiTextInfo.NomaiTextType.Wall &&
                                GUILayout.Button("Place conversation with G")
                            ) {
                                Logger.Log(conversationMeta.conversationGo+" 0");
                                DebugNomaiTextPlacer.active = true;
                                _dnp.onRaycast = (DebugRaycastData data) =>
                                {
                                    var sectorObject = data.hitBodyGameObject.GetComponentInChildren<Sector>()?.gameObject;
                                    if (sectorObject == null) sectorObject = data.hitBodyGameObject.GetComponentInParent<Sector>()?.gameObject;

                                    conversationMeta.conversation.position = data.pos;
                                    conversationMeta.conversation.normal   = data.norm;
                                    conversationMeta.conversation.rotation = null;

                                    DebugNomaiTextPlacer.active = false;
                                    UpdateConversationTransform(conversationMeta, sectorObject);
                                };
                            }

                            //
                            // spirals
                            //

                            for(int j = 0; j < conversationMeta.spirals.Count(); j++)
                            {
                                var spiralMeta = conversationMeta.spirals[j];
                                bool changed = false;
                                GUILayout.BeginHorizontal();
                                    GUILayout.Space(5);        
                                    GUILayout.BeginVertical(menu._submenuStyle);

                                        // spiral controls
                                        GUILayout.Label("Spiral");
                                        GUILayout.Label("Type");
                                            GUILayout.BeginHorizontal();
                                            GUI.enabled = spiralMeta.spiral.type != NomaiTextArcInfo.NomaiTextArcType.Adult;
                                            if (GUILayout.Button("Adult")) { spiralMeta.spiral.type = NomaiTextArcInfo.NomaiTextArcType.Adult; changed = true; }
                                            GUI.enabled = spiralMeta.spiral.type != NomaiTextArcInfo.NomaiTextArcType.Child;
                                            if (GUILayout.Button("Child")) { spiralMeta.spiral.type = NomaiTextArcInfo.NomaiTextArcType.Child; changed = true; }
                                            GUI.enabled = spiralMeta.spiral.type != NomaiTextArcInfo.NomaiTextArcType.Stranger;
                                            if (GUILayout.Button("Stranger")) { spiralMeta.spiral.type = NomaiTextArcInfo.NomaiTextArcType.Stranger; changed = true; }
                                            GUI.enabled = true;
                                            GUILayout.EndHorizontal();
                                        
                                        GUILayout.Label("Variation");
                                            GUILayout.BeginHorizontal();
                                            var varietyCount = GetVarietyCountForType(spiralMeta.spiral.type);
                                            for (int k = 0; k < varietyCount; k++)
                                            {
                                                GUI.enabled = spiralMeta.spiral.variation != k;
                                                if (GUILayout.Button(k+""))
                                                {
                                                    spiralMeta.spiral.variation = k;
                                                    changed = true;
                                                }
                                            }
                                            GUI.enabled = true;
                                            GUILayout.EndHorizontal();

                                    GUILayout.EndVertical();
                                    GUILayout.Space(5);
                                GUILayout.EndHorizontal();
                
                                if (changed)
                                { 
                                    // cache required stuff, destroy spiralMeta.go, call NomaiTextBuilder.MakeArc using spiralMeta.spiral and cached stuff
                                }

                                GUILayout.Space(10);
                            }
                        }

                    GUILayout.EndVertical();

                    GUILayout.Space(5);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
        }

        private int GetVarietyCountForType(NomaiTextArcInfo.NomaiTextArcType type)
        {
            switch(type)
            {
                case NomaiTextArcInfo.NomaiTextArcType.Stranger: return NomaiTextBuilder._ghostArcPrefabs.Count();
                case NomaiTextArcInfo.NomaiTextArcType.Child: return NomaiTextBuilder._childArcPrefabs.Count();
                default:
                case NomaiTextArcInfo.NomaiTextArcType.Adult: return NomaiTextBuilder._arcPrefabs.Count();
            }
            return 0;
        }

        void UpdateConversationTransform(ConversationMetadata conversationMetadata, GameObject sectorParent)
        {
            var nomaiWallTextObj = conversationMetadata.conversationGo;
            var planetGO = sectorParent;
            var info = conversationMetadata.conversation;
        
            Logger.Log(nomaiWallTextObj + " 1");
            Logger.Log(nomaiWallTextObj?.transform + " 2");
            Logger.Log(planetGO + " 3");
            Logger.Log(planetGO?.transform + " 4");
            Logger.Log(info + " 5");
            Logger.Log(info?.position + " 6");
            nomaiWallTextObj.transform.position = planetGO.transform.TransformPoint(info.position);
            if (info.normal != null)
            {
                // In global coordinates (normal was in local coordinates)
                var up = (nomaiWallTextObj.transform.position - planetGO.transform.position).normalized;
                var forward = planetGO.transform.TransformDirection(info.normal).normalized;

                nomaiWallTextObj.transform.up = up;
                nomaiWallTextObj.transform.forward = forward;
            }
            if (info.rotation != null)
            {
                nomaiWallTextObj.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(info.rotation));
            }
        }

        internal override void PreSave(DebugMenu menu)
        {
            conversations.ForEach(metadata =>
            {
                metadata.conversation.position = metadata.conversationGo.transform.localPosition;
                metadata.conversation.rotation = metadata.conversationGo.transform.localEulerAngles;           
            });

            // Spirals' configs do not need to be updated. They are always up to date
            //spirals.ForEach(metadata =>
            //{
            //    metadata.spiral.position = metadata.spiral.position;
            //    metadata.spiral.zRotation = metadata.spiral.zRotation;
            //});
        }
    }
}
