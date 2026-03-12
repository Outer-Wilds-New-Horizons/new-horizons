using NewHorizons.Components.ShipLog;
using NewHorizons.External;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NewHorizons.External.Modules.ShipLogModule;

namespace NewHorizons.Builder.ShipLog
{
    public static class MapModeBuilder
    {
        // Takes the game object because sometimes we change the AO to an NHAO and it breaks
        private static Dictionary<GameObject, ShipLogAstroObject> _astroObjectToShipLog = new();
        private static Dictionary<ShipLogAstroObject, MapModeInfo> _astroObjectToMapModeInfo = new();

        public static MapModeInfo GetMapModeInfoForAstroObject(ShipLogAstroObject slao)
        {
            if (_astroObjectToMapModeInfo.TryGetValue(slao, out var mapModeInfo))
            {
                return mapModeInfo;
            }
            else
            {
                return null;
            }
        }

        #region General
        public static ShipLogAstroObject[][] ConstructMapMode(string systemName, GameObject transformParent, ShipLogAstroObject[][] currentNav, int layer)
        {
            _astroObjectToShipLog = new();
            _astroObjectToMapModeInfo = new();

            // Add stock planets
            foreach (var shipLogAstroObject in currentNav.SelectMany(x => x))
            {
                var astroObject = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(shipLogAstroObject._id));
                if (astroObject == null)
                {
                    // Outsider compat
                    if (shipLogAstroObject._id == "POWER_STATION")
                    {
                        astroObject = GameObject.FindObjectsOfType<AstroObject>().FirstOrDefault(x => x._customName == "Power Station");
                        if (astroObject == null) continue;
                    }
                    else
                    {
                        NHLogger.LogError($"Couldn't find stock (?) astro object [{shipLogAstroObject?._id}]");
                        continue;
                    }
                }
                _astroObjectToShipLog[astroObject.gameObject] = shipLogAstroObject;
            }

            Material greyScaleMaterial = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/TimberHearth/Sprite").GetComponent<Image>().material;
            List<NewHorizonsBody> bodies = Main.BodyDict[systemName].Where(
                b => !(b.Config.ShipLog?.mapMode?.remove ?? false) && !b.Config.isQuantumState && !b.Config.destroy
            ).ToList();
            bool flagManualPositionUsed = systemName == "SolarSystem";
            bool flagAutoPositionUsed = false;
            foreach (NewHorizonsBody body in bodies.Where(b => ShipLogHandler.IsVanillaBody(b) == false))
            {
                if (body.Config.ShipLog == null) continue;

                if (body.Config.ShipLog.mapMode?.manualPosition == null)
                {
                    flagAutoPositionUsed = true;
                }
                else
                {
                    flagManualPositionUsed = true;
                    if (body.Config.ShipLog?.mapMode != null && body.Config.ShipLog.mapMode.manualNavigationPosition == null && body.Config.ShipLog.mapMode.selectable)
                    {
                        NHLogger.LogError("Navigation position is missing for: " + body.Config.name);
                        return null;
                    }
                }
            }

            // If they're both false, just default to auto (this means that no planets even have ship log info)
            if (!flagManualPositionUsed && !flagAutoPositionUsed)
            {
                flagAutoPositionUsed = true;
            }

            var isBaseSolarSystem = systemName == "SolarSystem";

            // Default to MANUAL in Base Solar System (we can't automatically fix them so it might just break, but AUTO breaks even more!)
            var useManual = (flagManualPositionUsed && !flagAutoPositionUsed) || (flagAutoPositionUsed && flagManualPositionUsed && isBaseSolarSystem);

            // Default to AUTO in other solar systems (since we can actually fix them)
            var useAuto = (flagAutoPositionUsed && !flagManualPositionUsed) || (flagAutoPositionUsed && flagManualPositionUsed && !isBaseSolarSystem);

            if (flagAutoPositionUsed && flagManualPositionUsed)
            {
                if (useAuto)
                {
                    NHLogger.LogWarning("Can't mix manual and automatic layout of ship log map mode, defaulting to AUTOMATIC");
                }
                else
                {
                    NHLogger.LogWarning("Can't mix manual and automatic layout of ship log map mode, defaulting to MANUAL");
                }
            }

            ShipLogAstroObject[][] newNavMatrix = null;

            if (useAuto)
            {
                newNavMatrix = ConstructMapModeAuto(bodies, transformParent, greyScaleMaterial, layer);
            }
            else if (useManual)
            {
                newNavMatrix = ConstructMapModeManual(bodies, transformParent, greyScaleMaterial, currentNav, layer);
            }

            ReplaceExistingMapModeIcons();

            return newNavMatrix;
        }

        public static string GetAstroBodyShipLogName(string id)
        {
            return TranslationHandler.GetTranslation(ShipLogHandler.GetNameFromAstroID(id) ?? id, TranslationHandler.TextType.UI);
        }

        private static GameObject CreateImage(GameObject nodeGO, Texture2D texture, string name, int layer)
        {
            GameObject newImageGO = new GameObject(name);
            newImageGO.layer = layer;
            newImageGO.transform.SetParent(nodeGO.transform);

            RectTransform transform = newImageGO.AddComponent<RectTransform>();
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            Image newImage = newImageGO.AddComponent<Image>();

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(texture.width / 2, texture.height / 2);
            newImage.sprite = Sprite.Create(texture, rect, pivot, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);

            return newImageGO;
        }

        private static GameObject CreateMapModeGameObject(NewHorizonsBody body, GameObject parent, int layer, Vector2 position)
        {
            GameObject newGameObject = new GameObject(body.Config.name + "_ShipLog");
            newGameObject.layer = layer;
            newGameObject.transform.SetParent(parent.transform);

            RectTransform transform = newGameObject.AddComponent<RectTransform>();
            float scale = body.Config.ShipLog?.mapMode?.scale ?? 1f;
            transform.localPosition = position;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * scale;
            transform.SetAsFirstSibling();
            return newGameObject;
        }

        private static ShipLogAstroObject AddShipLogAstroObject(GameObject gameObject, NewHorizonsBody body, Material greyScaleMaterial, int layer)
        {
            if (body.Object == null)
            {
                NHLogger.LogError($"Tried to make ship logs for planet with null Object: [{body?.Config?.name}]");
                return null;
            }

            const float unviewedIconOffset = 15;

            NHLogger.LogVerbose($"Adding ship log astro object for {body.Config.name}");

            GameObject unviewedReference = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/TimberHearth/UnviewedIcon");

            ShipLogAstroObject astroObject = gameObject.AddComponent<ShipLogAstroObject>();
            astroObject._id = ShipLogHandler.GetAstroObjectId(body);
            _astroObjectToShipLog[body.Object] = astroObject;
            _astroObjectToMapModeInfo[astroObject] = body.Config.ShipLog?.mapMode;

            Texture2D image = null;
            Texture2D outline = null;

            string imagePath = body.Config.ShipLog?.mapMode?.revealedSprite;
            string outlinePath = body.Config.ShipLog?.mapMode?.outlineSprite;

            if (imagePath != null) image = ImageUtilities.GetTexture(body.Mod, imagePath);
            if (image == null) image = ImageUtilities.AutoGenerateMapModePicture(body);

            if (outlinePath != null) outline = ImageUtilities.GetTexture(body.Mod, outlinePath);
            if (outline == null) outline = ImageUtilities.GetCachedOutlineOrCreate(body, image, imagePath);

            astroObject._imageObj = CreateImage(gameObject, image, body.Config.name + " Revealed", layer);
            astroObject._outlineObj = CreateImage(gameObject, outline, body.Config.name + " Outline", layer);

            if (ShipLogHandler.BodyHasEntries(body))
            {
                Image revealedImage = astroObject._imageObj.GetComponent<Image>();
                astroObject._greyscaleMaterial = greyScaleMaterial;
                revealedImage.material = greyScaleMaterial;
                revealedImage.color = Color.white;
                astroObject._image = revealedImage;
            }

            astroObject._unviewedObj = UnityEngine.Object.Instantiate(unviewedReference, gameObject.transform, false);
            astroObject._invisibleWhenHidden = body.Config.ShipLog?.mapMode?.invisibleWhenHidden ?? false;

            Rect imageRect = astroObject._imageObj.GetComponent<RectTransform>().rect;
            astroObject._unviewedObj.transform.localPosition = new Vector3(imageRect.width / 2 + unviewedIconOffset, imageRect.height / 2 + unviewedIconOffset, 0);

            // Set all icons inactive, they will be conditionally activated when the map mode is opened for the first time
            astroObject._unviewedObj.SetActive(false);
            astroObject._imageObj.SetActive(false);
            astroObject._outlineObj.SetActive(false);

            return astroObject;
        }
        #endregion

        #region Details
        private static void MakeDetail(ShipLogModule.ShipLogDetailInfo info, Transform parent, NewHorizonsBody body, Material greyScaleMaterial)
        {
            GameObject detailGameObject = new GameObject("Detail");
            detailGameObject.transform.SetParent(parent);
            detailGameObject.SetActive(false);

            RectTransform detailTransform = detailGameObject.AddComponent<RectTransform>();
            detailTransform.localPosition = (Vector2)(info.position ?? Vector2.zero);
            detailTransform.localRotation = Quaternion.Euler(0f, 0f, info.rotation);
            detailTransform.localScale = (Vector2)(info.scale ?? Vector2.zero);

            Texture2D image;
            Texture2D outline;

            string imagePath = info.revealedSprite;
            string outlinePath = info.outlineSprite;

            if (imagePath != null) image = ImageUtilities.GetTexture(body.Mod, imagePath);
            else image = ImageUtilities.AutoGenerateMapModePicture(body);

            if (outlinePath != null) outline = ImageUtilities.GetTexture(body.Mod, outlinePath);
            else outline = ImageUtilities.GetCachedOutlineOrCreate(body, image, imagePath);

            Image revealedImage = CreateImage(detailGameObject, image, "Detail Revealed", parent.gameObject.layer).GetComponent<Image>();
            Image outlineImage = CreateImage(detailGameObject, outline, "Detail Outline", parent.gameObject.layer).GetComponent<Image>();

            ShipLogDetail detail = detailGameObject.AddComponent<ShipLogDetail>();
            detail.Init(info, revealedImage, outlineImage, greyScaleMaterial);
            detailGameObject.SetActive(true);
        }

        private static void MakeDetails(NewHorizonsBody body, Transform parent, Material greyScaleMaterial)
        {
            if (body.Config.ShipLog?.mapMode?.details?.Length > 0)
            {
                GameObject detailsParent = new GameObject("Details");
                detailsParent.transform.SetParent(parent);
                detailsParent.SetActive(false);

                RectTransform detailsTransform = detailsParent.AddComponent<RectTransform>();
                detailsTransform.localPosition = Vector3.zero;
                detailsTransform.localRotation = Quaternion.identity;
                detailsTransform.localScale = Vector3.one;

                foreach (ShipLogModule.ShipLogDetailInfo detailInfo in body.Config.ShipLog.mapMode.details)
                {
                    MakeDetail(detailInfo, detailsTransform, body, greyScaleMaterial);
                }
                detailsParent.SetActive(true);
            }
        }
        #endregion

        #region Manual Map Mode
        private static ShipLogAstroObject[][] ConstructMapModeManual(List<NewHorizonsBody> bodies, GameObject transformParent, Material greyScaleMaterial, ShipLogAstroObject[][] currentNav, int layer)
        {
            int maxAmount = bodies.Count + 20;
            ShipLogAstroObject[][] navMatrix = new ShipLogAstroObject[maxAmount][];
            for (int i = 0; i < maxAmount; i++)
            {
                navMatrix[i] = new ShipLogAstroObject[maxAmount];
            }

            Dictionary<string, int[]> astroIdToNavIndex = new Dictionary<string, int[]>();

            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {

                for (int y = 0; y < currentNav.Length; y++)
                {
                    for (int x = 0; x < currentNav[y].Length; x++)
                    {
                        navMatrix[y][x] = currentNav[y][x];
                        astroIdToNavIndex.Add(currentNav[y][x].GetID(), new[] { y, x });
                    }
                }
            }

            foreach (NewHorizonsBody body in bodies)
            {
                if (body.Config.ShipLog?.mapMode?.manualNavigationPosition == null && body.Config.ShipLog?.mapMode?.details == null)
                {
                    if (body.Config.ShipLog?.mapMode?.manualPosition != null)
                    {
                        NHLogger.LogError($"Missing ship log map mode manualNavigationPosition for [{body.Config.name}]");
                    }
                    continue;
                }

                // Sometimes they got other names idk
                var name = body.Config.name.Replace(" ", "");
                var existingBody = AstroObjectLocator.GetAstroObject(body.Config.name);
                if (existingBody != null)
                {
                    var astroName = existingBody.GetAstroObjectName();
                    if (astroName == AstroObject.Name.RingWorld) name = "InvisiblePlanet";
                    else if (astroName != AstroObject.Name.CustomString) name = astroName.ToString();
                }
                // Should probably also just fix the IsVanilla method
                var isVanilla = ShipLogHandler.IsVanillaBody(body);

                if (!isVanilla)
                {
                    NHLogger.LogVerbose($"Making map mode object for [{body.Config.name}]");
                    GameObject newMapModeGO = CreateMapModeGameObject(body, transformParent, layer, body.Config.ShipLog?.mapMode?.manualPosition);
                    ShipLogAstroObject newAstroObject = AddShipLogAstroObject(newMapModeGO, body, greyScaleMaterial, layer);
                    if (body.Config.FocalPoint != null)
                    {
                        newAstroObject._imageObj.GetComponent<Image>().enabled = false;
                        newAstroObject._outlineObj.GetComponent<Image>().enabled = false;
                        newAstroObject._unviewedObj.GetComponent<Image>().enabled = false;
                    }
                    MakeDetails(body, newMapModeGO.transform, greyScaleMaterial);
                    Vector2 navigationPosition = body.Config.ShipLog?.mapMode?.manualNavigationPosition;
                   if (navigationPosition.y < 0 || navigationPosition.y < 0)
                    {
                        NHLogger.LogError("Map Mode navigation positions cannot be in the negatives!");
                        continue;
                    }
                    navMatrix[(int)navigationPosition.y][(int)navigationPosition.x] = newAstroObject;
                }
                else if (Main.Instance.CurrentStarSystem == "SolarSystem")
                {
                    GameObject gameObject = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + name);
                    if (body.Config.destroy || (body.Config.ShipLog?.mapMode?.remove ?? false))
                    {
                        ShipLogAstroObject astroObject = gameObject.GetComponent<ShipLogAstroObject>();
                        if (astroObject != null)
                        {
                            int[] navIndex = astroIdToNavIndex[astroObject.GetID()];
                            navMatrix[navIndex[0]][navIndex[1]] = null;
                            if (astroObject.GetID() == "CAVE_TWIN" || astroObject.GetID() == "TOWER_TWIN")
                            {
                                SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + "SandFunnel").SetActive(false);
                            }
                        }
                        else if (name == "SandFunnel")
                        {
                            SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + "SandFunnel").SetActive(false);
                        }
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        if (body.Config.ShipLog?.mapMode?.manualPosition != null)
                        {
                            gameObject.transform.localPosition = (Vector2)body.Config.ShipLog.mapMode.manualPosition;
                        }
                        if (body.Config.ShipLog?.mapMode?.manualNavigationPosition != null)
                        {
                            Vector2 navigationPosition = body.Config.ShipLog?.mapMode?.manualNavigationPosition;
                            navMatrix[(int)navigationPosition.y][(int)navigationPosition.x] = gameObject.GetComponent<ShipLogAstroObject>();
                        }
                        if (body.Config.ShipLog?.mapMode?.scale != null)
                        {
                            gameObject.transform.localScale = Vector3.one * body.Config.ShipLog.mapMode.scale;
                        }
                        MakeDetails(body, gameObject.transform, greyScaleMaterial);
                    }
                }
            }

            navMatrix = navMatrix.Where(a => a.Count(c => c != null && c.gameObject != null) > 0).Prepend(new ShipLogAstroObject[1]).ToArray();
            for (var index = 0; index < navMatrix.Length; index++)
            {
                navMatrix[index] = navMatrix[index].Where(a => a != null && a.gameObject != null).ToArray();
            }

            return navMatrix;
        }
        #endregion

        #region Automatic Map Mode
        private class MapModeObject
        {
            public int x;
            public int y;
            public int branch_width;
            public int branch_height;
            public int level;
            public NewHorizonsBody mainBody;
            public ShipLogAstroObject astroObject;
            public List<MapModeObject> children;
            public MapModeObject parent;
            public MapModeObject lastSibling;
            public void IncrementWidth()
            {
                branch_width++;
                parent?.IncrementWidth();
            }
            public void IncrementHeight()
            {
                branch_height++;
                parent?.IncrementHeight();
            }
        }

        private static ShipLogAstroObject[][] ConstructMapModeAuto(List<NewHorizonsBody> bodies, GameObject transformParent, Material greyScaleMaterial, int layer)
        {
            MapModeObject rootObject = ConstructPrimaryNode(bodies);
            if (rootObject.mainBody != null)
            {
                MakeAllNodes(ref rootObject, transformParent, greyScaleMaterial, layer);
            }

            int maxAmount = bodies.Count;
            ShipLogAstroObject[][] navMatrix = new ShipLogAstroObject[maxAmount][];
            for (int i = 0; i < maxAmount; i++)
            {
                navMatrix[i] = new ShipLogAstroObject[maxAmount];
            }

            CreateNavigationMatrix(rootObject, ref navMatrix);
            navMatrix = navMatrix.Where(a => a.Count(c => c != null) > 0).Prepend(new ShipLogAstroObject[1]).ToArray();
            for (var index = 0; index < navMatrix.Length; index++)
            {
                navMatrix[index] = navMatrix[index].Where(a => a != null).ToArray();
            }
            return navMatrix;
        }

        private static void CreateNavigationMatrix(MapModeObject root, ref ShipLogAstroObject[][] navMatrix)
        {
            if (root.astroObject != null)
            {
                navMatrix[root.y][root.x] = root.astroObject;
            }
            foreach (MapModeObject child in root.children)
            {
                CreateNavigationMatrix(child, ref navMatrix);
            }
        }

        private static void MakeAllNodes(ref MapModeObject parentNode, GameObject parent, Material greyScaleMaterial, int layer)
        {
            MakeNode(ref parentNode, parent, greyScaleMaterial, layer);
            for (var i = 0; i < parentNode.children.Count; i++)
            {
                MapModeObject child = parentNode.children[i];
                MakeAllNodes(ref child, parent, greyScaleMaterial, layer);
                parentNode.children[i] = child;
            }
        }

        private static MapModeObject ConstructPrimaryNode(List<NewHorizonsBody> bodies)
        {
            foreach (NewHorizonsBody body in bodies.Where(b => b.Config.Base.centerOfSolarSystem))
            {
                bodies.Sort((b, o) => b.Config.Orbit.semiMajorAxis.CompareTo(o.Config.Orbit.semiMajorAxis));
                MapModeObject newNode = new MapModeObject
                {
                    mainBody = body,
                    level = 0,
                    x = 0,
                    y = 0
                };
                newNode.children = ConstructChildrenNodes(newNode, bodies);
                return newNode;
            }
            NHLogger.LogError("Couldn't find center of system!");
            return new MapModeObject();
        }

        private static List<MapModeObject> ConstructChildrenNodes(MapModeObject parent, List<NewHorizonsBody> searchList, string secondaryName = "", string focalPointName = "")
        {
            List<MapModeObject> children = new List<MapModeObject>();
            int newX = parent.x;
            int newY = parent.y;
            int newLevel = parent.level + 1;
            MapModeObject lastSibling = parent;

            foreach (NewHorizonsBody body in searchList.Where(b => b.Config.Orbit.primaryBody == parent.mainBody.Config.name || (b.Config.Orbit.primaryBody == focalPointName && b.Config.name != parent.mainBody.Config.name) || b.Config.name == secondaryName))
            {
                bool even = newLevel % 2 == 0;
                newX = even ? newX : newX + 1;
                newY = even ? newY + 1 : newY;
                MapModeObject newNode = new MapModeObject()
                {
                    mainBody = body,
                    level = newLevel,
                    x = newX,
                    y = newY,
                    parent = parent,
                    lastSibling = lastSibling
                };
                string newSecondaryName = "";
                string newFocalPointName = "";
                if (body.Config.FocalPoint != null)
                {
                    newFocalPointName = body.Config.name;
                    newNode.mainBody = searchList.Find(b => b.Config.name == body.Config.FocalPoint.primary);
                    newSecondaryName = searchList.Find(b => b.Config.name == body.Config.FocalPoint.secondary).Config.name;
                }

                newNode.children = ConstructChildrenNodes(newNode, searchList, newSecondaryName, newFocalPointName);
                if (even)
                {
                    newY += newNode.branch_height;
                    parent.IncrementHeight();
                }
                else
                {
                    newX += newNode.branch_width;
                    parent.IncrementWidth();
                }

                lastSibling = newNode;
                children.Add(newNode);
            }
            return children;
        }

        private static void ConnectNodeToLastSibling(MapModeObject node, Material greyScaleMaterial)
        {
            if (node.astroObject == null)
            {
                NHLogger.LogError($"Failed to connect node to last sibling because ShipLogAstroObject is null");
                return;
            }

            if (node.lastSibling == null || node.lastSibling.astroObject == null)
            {
                NHLogger.LogError($"Failed to connect node {node.astroObject.GetID()} to last sibling because lastSibling is null");
                return;
            }
            
            NHLogger.LogVerbose($"Connecting node {node.astroObject.GetID()} to last sibling {node.lastSibling.astroObject.GetID()}");

            Vector2 fromPosition = node.astroObject.transform.localPosition;
            Vector2 toPosition = node.lastSibling.astroObject.transform.localPosition;

            GameObject newLink = new GameObject("Line_ShipLog");
            newLink.layer = node.astroObject.gameObject.layer;
            newLink.SetActive(false);

            RectTransform transform = newLink.AddComponent<RectTransform>();
            transform.SetParent(node.astroObject.transform.parent);
            Vector2 center = toPosition + (fromPosition - toPosition) / 2;
            transform.localPosition = new Vector3(center.x, center.y, -1);
            transform.localRotation = Quaternion.identity;
            transform.localScale = node.level % 2 == 0 ? new Vector3(node.astroObject.transform.localScale.x / 5f, Mathf.Abs(fromPosition.y - toPosition.y) / 100f, 1) : new Vector3(Mathf.Abs(fromPosition.x - toPosition.x) / 100f, node.astroObject.transform.localScale.y / 5f, 1);
            Image linkImage = newLink.AddComponent<Image>();
            linkImage.color = new Color(0.28f, 0.28f, 0.5f, 0.12f);

            ShipLogModule.ShipLogDetailInfo linkDetailInfo = new ShipLogModule.ShipLogDetailInfo()
            {
                invisibleWhenHidden = node.mainBody.Config.ShipLog?.mapMode?.invisibleWhenHidden ?? false
            };

            ShipLogDetail linkDetail = newLink.AddComponent<ShipLogDetail>();
            linkDetail.Init(linkDetailInfo, linkImage, linkImage, greyScaleMaterial);

            transform.SetParent(node.astroObject.transform);
            transform.SetAsFirstSibling();
            newLink.SetActive(true);
        }

        private static void MakeNode(ref MapModeObject node, GameObject parent, Material greyScaleMaterial, int layer)
        {
            NHLogger.LogVerbose($"Making node for [{node.mainBody?.Config?.name}]");

            // Space between this node and the previous node
            // Take whatever scale will prevent overlap
            var lastSiblingScale = node.lastSibling?.mainBody?.Config?.ShipLog?.mapMode?.scale ?? 1f;
            var scale = node.mainBody?.Config?.ShipLog?.mapMode?.scale ?? 1f;
            float padding = 100f * (scale + lastSiblingScale) / 2f;

            Vector2 position = Vector2.zero;
            if (node.lastSibling != null)
            {
                if (node.lastSibling.astroObject != null)
                {
                    ShipLogAstroObject lastAstroObject = node.lastSibling.astroObject;
                    Vector3 lastPosition = lastAstroObject.transform.localPosition;
                    position = lastPosition;
                    float extraDistance = (node.mainBody.Config.ShipLog?.mapMode?.offset ?? 0f) * 100;

                    if (node.level % 2 == 0)
                    {
                        position.y += padding * (node.y - node.lastSibling.y) + extraDistance;
                    }
                    else
                    {
                        position.x += padding * (node.x - node.lastSibling.x) + extraDistance;
                    }
                }
                else
                {
                    NHLogger.LogError($"Last sibling's ShipLogAstroObject is null");
                }
            }

            GameObject newNodeGO = CreateMapModeGameObject(node.mainBody, parent, layer, position);
            ShipLogAstroObject astroObject = AddShipLogAstroObject(newNodeGO, node.mainBody, greyScaleMaterial, layer);
            if (node.mainBody.Config.FocalPoint != null)
            {
                astroObject._imageObj.GetComponent<Image>().enabled = false;
                astroObject._outlineObj.GetComponent<Image>().enabled = false;
                astroObject._unviewedObj.GetComponent<Image>().enabled = false;
            }
            node.astroObject = astroObject;
            if (NewHorizons.Main.Debug)
            {
                if (node.lastSibling != null) ConnectNodeToLastSibling(node, greyScaleMaterial);
            }
            MakeDetails(node.mainBody, newNodeGO.transform, greyScaleMaterial);
        }
        #endregion

        #region Replacement
        private static List<(NewHorizonsBody, ModBehaviour, MapModeInfo)> _mapModIconsToUpdate = new();
        public static void TryReplaceExistingMapModeIcon(NewHorizonsBody body, ModBehaviour mod, MapModeInfo info)
        {
            if (!string.IsNullOrEmpty(info.revealedSprite) || !string.IsNullOrEmpty(info.outlineSprite))
            {
                _mapModIconsToUpdate.Add((body, mod, info));
            }
        }

        private static void ReplaceExistingMapModeIcons()
        {
            foreach (var (body, mod, info) in _mapModIconsToUpdate)
            {
                ReplaceExistingMapModeIcon(body, mod, info);
            }
            _mapModIconsToUpdate.Clear();
        }

        private static void ReplaceExistingMapModeIcon(NewHorizonsBody body, ModBehaviour mod, MapModeInfo info)
        {
            var astroObject = _astroObjectToShipLog[body.Object];
            var gameObject = astroObject.gameObject;
            var layer = gameObject.layer;

            if (!string.IsNullOrEmpty(info.revealedSprite))
            {
                var revealedTexture = ImageUtilities.GetTexture(body.Mod, info.revealedSprite);
                if (revealedTexture == null)
                {
                    NHLogger.LogError($"Couldn't load replacement revealed texture {info.revealedSprite}");
                }
                else
                {
                    GameObject.Destroy(astroObject._imageObj);
                    if (ShipLogHandler.IsVanillaBody(body) || ShipLogHandler.BodyHasEntries(body))
                    {
                        Image revealedImage = astroObject._imageObj.GetComponent<Image>();
                        revealedImage.material = astroObject._greyscaleMaterial;
                        revealedImage.color = Color.white;
                        astroObject._image = revealedImage;
                    }
                    astroObject._imageObj = CreateImage(gameObject, revealedTexture, body.Config.name + " Revealed", layer);
                }
            }
            if (!string.IsNullOrEmpty(info.outlineSprite))
            {
                var outlineTexture = ImageUtilities.GetTexture(body.Mod, info.outlineSprite);
                if (outlineTexture == null)
                {
                    NHLogger.LogError($"Couldn't load replacement outline texture {info.outlineSprite}");

                }
                else
                {
                    GameObject.Destroy(astroObject._outlineObj);
                    astroObject._outlineObj = CreateImage(gameObject, outlineTexture, body.Config.name + " Outline", layer);
                }
            }
            astroObject._invisibleWhenHidden = info.invisibleWhenHidden;
        }
        #endregion
    }
}
