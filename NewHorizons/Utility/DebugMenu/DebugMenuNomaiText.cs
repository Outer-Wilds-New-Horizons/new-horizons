using NewHorizons.Builder.Props;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.DebugUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            public int parentID;

            public int pointOnParent = -1;
        }

        List<ConversationMetadata> conversations = new List<ConversationMetadata>();

        // menu stuff
        Vector2 conversationsScrollPosition = new Vector2();
        float dx = 0.1f;
        float dy = 0.1f;
        float dT = 1f;

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
                    
                    string regex = @"Arc \d+ - Child of (\d*)"; // $"Arc {i} - Child of {parentID}";
                    var parentID = (new Regex(regex)).Matches(metadata.spiralGo.name)[0].Groups[1].Value;
                    metadata.parentID = parentID == "" ? -1 : int.Parse(parentID);
                    
                    Logger.Log("Parent id for '" + metadata.spiralGo.name + "' : " + parentID);

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
                        if (GUILayout.Button(arrow + conversationMeta.planetConfig.name + " - " + i, menu._submenuStyle)) 
                        {
                            conversationMeta.collapsed = !conversationMeta.collapsed;
                            Logger.Log("BUTTON " + i);
                        }

                        if (!conversationMeta.collapsed)
                        {
                            GUILayout.Space(5);
                            // button to set this one to place with a raycast
                            GUILayout.Label(conversationMeta.conversation.type.ToString());
                            
                            // only show the button if this conversation is a wall text, do not show it if it is a scroll text or something
                            if (
                                //conversationMeta.conversation.type == PropModule.NomaiTextInfo.NomaiTextType.Wall &&
                                GUILayout.Button("Place conversation with G")
                            ) {
                                Logger.Log(conversationMeta.conversationGo+" 0");
                                DebugNomaiTextPlacer.active = true;
                                _dnp.onRaycast = (DebugRaycastData data) =>
                                {
                                    // I think this should be looking for AstroObject and then doing ._rootSector
                                    var sectorObject = data.hitBodyGameObject.GetComponentInChildren<Sector>()?.gameObject;
                                    if (sectorObject == null) sectorObject = data.hitBodyGameObject.GetComponentInParent<Sector>()?.gameObject;

                                    if (conversationMeta.conversation.type == NomaiTextInfo.NomaiTextType.Wall)
                                    {
                                        conversationMeta.conversation.position = data.pos;
                                        conversationMeta.conversation.normal   = data.norm;
                                        conversationMeta.conversation.rotation = null;
                                        UpdateConversationTransform(conversationMeta, sectorObject);
                                    }
                                    else
                                    {
                                        conversationMeta.conversationGo.transform.localPosition = data.pos;
                                        DebugPropPlacer.SetGameObjectRotation(conversationMeta.conversationGo, data, _dnp.gameObject.transform.position);
                                        
                                        conversationMeta.conversation.position = conversationMeta.conversationGo.transform.localPosition;
                                        conversationMeta.conversation.rotation = conversationMeta.conversationGo.transform.localEulerAngles;
                                    }

                                    DebugNomaiTextPlacer.active = false;
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
                                            if (GUILayout.Button("Reroll variation"))
                                            {
                                                spiralMeta.spiral.variation = UnityEngine.Random.Range(0, varietyCount);
                                            }
                                            GUI.enabled = true;
                                            GUILayout.EndHorizontal();
        
                                        var shouldBeInManual = spiralMeta.parentID < 0 || GUILayout.Toggle(spiralMeta.pointOnParent < 0, "Manual Positioning");
                                        if ( shouldBeInManual && spiralMeta.pointOnParent >= 0) spiralMeta.pointOnParent = -1;
                                        if (!shouldBeInManual && spiralMeta.pointOnParent < 0 ) spiralMeta.pointOnParent = 0;

                                        if (shouldBeInManual)
                                        {
                                            // x
                                            GUILayout.BeginHorizontal();
                                                var positionX = (spiralMeta.spiral.position?.x ?? 0);
                                                GUILayout.Label("x:     ", GUILayout.Width(50));
                                                float deltaX = float.Parse(GUILayout.TextField(positionX+"", GUILayout.Width(50))) - positionX;
                                                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaX += dx;
                                                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaX -= dx;
                                                dx = float.Parse(GUILayout.TextField(dx+"", GUILayout.Width(100)));
                                            GUILayout.EndHorizontal();
                                            // y
                                            GUILayout.BeginHorizontal();
                                                var positionY = (spiralMeta.spiral.position?.y ?? 0);
                                                GUILayout.Label("y:     ", GUILayout.Width(50));
                                                float deltaY = float.Parse(GUILayout.TextField(positionY+"", GUILayout.Width(50))) - positionY;
                                                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaY += dy;
                                                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaY -= dy;
                                                dy = float.Parse(GUILayout.TextField(dy+"", GUILayout.Width(100)));
                                            GUILayout.EndHorizontal();
                                            // theta
                                            GUILayout.BeginHorizontal();
                                                var theta = spiralMeta.spiral.zRotation;
                                                GUILayout.Label("theta: ", GUILayout.Width(50));
                                                float deltaTheta = float.Parse(GUILayout.TextField(theta+"", GUILayout.Width(50))) - theta;
                                                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaTheta += dT;
                                                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaTheta -= dT;
                                                dT = float.Parse(GUILayout.TextField(dT+"", GUILayout.Width(100)));
                                            GUILayout.EndHorizontal();

                                            if (deltaX != 0 || deltaY != 0 || deltaTheta != 0)
                                            {
                                                spiralMeta.spiral.position = (spiralMeta.spiral.position??Vector2.zero) + new Vector2(deltaX, deltaY);
                                                spiralMeta.spiral.zRotation += deltaTheta; 
        
                                                spiralMeta.spiralGo.transform.localPosition = new Vector3(spiralMeta.spiral.position.x, spiralMeta.spiral.position.y, 0);
                                                spiralMeta.spiralGo.transform.localRotation = Quaternion.Euler(0, 0, spiralMeta.spiral.zRotation);

                                                spiralMeta.pointOnParent = -1; // since tweaks have been made, we're setting this to manual mode
                                            
                                                UpdateChildrenLocations(spiralMeta);
                                            }
                                        }
                                        else
                                        {
                                            // var newPointOnParent = GUI.HorizontalSlider(); // stepped horizontal slider
                                            // if new value, set spiral GO positioning (and spiral's x/y/theta)
                                            // UpdateChildPositions()
                                        }
                                    GUILayout.EndVertical();
                                    GUILayout.Space(5);
                                GUILayout.EndHorizontal();
                
                                if (changed)
                                { 
                                    // cache required stuff, destroy spiralMeta.go, call NomaiTextBuilder.MakeArc using spiralMeta.spiral and cached stuff
                                    // PropModule.NomaiTextArcInfo arcInfo, GameObject conversationZone, GameObject parent, int textEntryID, int i
                                    var conversationZone = spiralMeta.spiralGo.transform.parent.gameObject;
                                    var textEntryId = spiralMeta.spiralGo.GetComponent<NomaiTextLine>()._entryID;
                                    GameObject.Destroy(spiralMeta.spiralGo);
                                    spiralMeta.spiralGo = NomaiTextBuilder.MakeArc(spiralMeta.spiral, conversationZone, null, textEntryId);
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

        private void UpdateChildrenLocations(SpiralMetadata parentSprialMetadata)
        {
            ConversationMetadata convoMeta = conversations.Where(m => m.conversation == parentSprialMetadata.conversation).First();
            convoMeta.spirals
                .Where(spiralMeta => spiralMeta.parentID == parentSprialMetadata.id)
                .ToList()
                .ForEach(spiralMeta => {
                    if (spiralMeta.pointOnParent == -1) return; // this spiral is positioned manually

                    // TODO: set x, y, and theta of spiralMeta based on spiralMeta.pointOnParent and parentSprialMetadata's x,y,theta
                    UpdateChildrenLocations(spiralMeta);
                });
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
