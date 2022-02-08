using NewHorizons.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Builder.Handlers;

namespace NewHorizons.Builder.ShipLog
{
    public static class MapModeBuilder
    {
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
            public void Increment_width()
            {
                branch_width++;
                parent?.Increment_width();
            }
            public void Increment_height()
            {
                branch_height++;
                parent?.Increment_height();
            }
        }

        public static string GetAstroBodyShipLogName(string id)
        {
            return ShipLogHandler.GetConfigFromID(id)?.Config?.Name ?? id;
        }

        public static ShipLogAstroObject[][] ConstructMapMode(string systemName, GameObject transformParent, int layer)
        {
            MapModeObject rootObject = ConstructPrimaryNode(systemName);
            Material greyScaleMaterial = GameObject.Find(ShipLogHandler.PAN_ROOT_PATH + "/TimberHearth/Sprite").GetComponent<Image>().material;
            if (rootObject.mainBody != null)
            {
                MakeAllNodes(ref rootObject, transformParent, greyScaleMaterial, layer);
            }

            int maxAmount = Main.BodyDict[Main.Instance.CurrentStarSystem].Count;
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

        private static GameObject CreateImage(GameObject nodeGO, IModAssets assets, string imagePath, string name, int layer)
        {
            GameObject newImageGO = new GameObject(name);
            newImageGO.layer = layer;
            newImageGO.transform.SetParent(nodeGO.transform);

            RectTransform transform = newImageGO.AddComponent<RectTransform>();
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            Image newImage = newImageGO.AddComponent<Image>();
            if (imagePath == "DEFAULT")
            {
                newImage.sprite = Locator.GetShipLogManager()._shipLogLibrary.defaultEntrySprite;
            }
            else
            {
                Texture2D newTexture = assets.GetTexture(imagePath);
                Rect rect = new Rect(0, 0, newTexture.width, newTexture.height);
                Vector2 pivot = new Vector2(newTexture.width / 2, newTexture.height / 2);
                newImage.sprite = Sprite.Create(newTexture, rect, pivot);
            }
            return newImageGO;
        }

        private static void CreateShipLogAstroObject(GameObject nodeGO, ref MapModeObject node, GameObject referenceUnviewedSprite, Material greyScaleMaterial, int layer)
        {
            const float unviewedIconOffset = 15;
            ShipLogAstroObject astroObject = nodeGO.AddComponent<ShipLogAstroObject>();
            astroObject._id = ShipLogHandler.GetAstroObjectId(node.mainBody);

            string imagePath = node.mainBody.Config.ShipLog?.mapMode?.revealedSprite ?? "DEFAULT";
            string outlinePath = node.mainBody.Config.ShipLog?.mapMode?.outlineSprite ?? imagePath;
            astroObject._imageObj = CreateImage(nodeGO, node.mainBody.Mod.Assets, imagePath, node.mainBody.Config.Name + " Revealed", layer);
            if (ShipLogHandler.BodyHasEntries(node.mainBody))
            {
                Image revealedImage = astroObject._imageObj.GetComponent<Image>();
                astroObject._greyscaleMaterial = greyScaleMaterial;
                revealedImage.material = greyScaleMaterial;
                revealedImage.color = Color.white;
                astroObject._image = revealedImage;
            }
            astroObject._outlineObj = CreateImage(nodeGO, node.mainBody.Mod.Assets, outlinePath, node.mainBody.Config.Name + " Outline", layer);

            astroObject._unviewedObj = Object.Instantiate(referenceUnviewedSprite, nodeGO.transform, false);
            if (node.mainBody.Config.FocalPoint != null)
            {
                astroObject._imageObj.GetComponent<Image>().enabled = false;
                astroObject._outlineObj.GetComponent<Image>().enabled = false;
                astroObject._unviewedObj.GetComponent<Image>().enabled = false;
                astroObject.transform.localScale = node.lastSibling.astroObject.transform.localScale;
            }
            astroObject._invisibleWhenHidden = node.mainBody.Config.ShipLog?.mapMode?.invisibleWhenHidden ?? false;

            Rect imageRect = astroObject._imageObj.GetComponent<RectTransform>().rect;
            astroObject._unviewedObj.transform.localPosition = new Vector3(imageRect.width / 2 + unviewedIconOffset, imageRect.height / 2 + unviewedIconOffset, 0);
            node.astroObject = astroObject;
        }

        private static void ConnectNodeToLastSibling(MapModeObject node, Material greyScaleMaterial)
        {
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
            linkImage.color = new Color(0.28f, 0.28f, 0.5f, 0.28f);

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

        private static void MakeDetail(ShipLogModule.ShipLogDetailInfo info, Transform parent, IModAssets assets, Material greyScaleMaterial)
        {
            GameObject detailGameObject = new GameObject("Detail");
            detailGameObject.transform.SetParent(parent);
            detailGameObject.SetActive(false);

            RectTransform detailTransform = detailGameObject.AddComponent<RectTransform>();
            detailTransform.localPosition = (Vector2)(info.position ?? new MVector2(0, 0));
            detailTransform.localRotation = Quaternion.Euler(0f, 0f, info.rotation);
            detailTransform.localScale = (Vector2)(info.scale ?? new MVector2(0, 0));

            string revealedPath = info.revealedSprite ?? "DEFAULT";
            string outlinePath = info.outlineSprite ?? revealedPath;

            Image revealedImage = CreateImage(detailGameObject, assets, revealedPath, "Detail Revealed", parent.gameObject.layer).GetComponent<Image>();
            Image outlineImage = CreateImage(detailGameObject, assets, outlinePath, "Detail Outline", parent.gameObject.layer).GetComponent<Image>();

            ShipLogDetail detail = detailGameObject.AddComponent<ShipLogDetail>();
            detail.Init(info, revealedImage, outlineImage, greyScaleMaterial);
            detailGameObject.SetActive(true);
        }

        private static void MakeDetails(MapModeObject node, Material greyScaleMaterial)
        {
            if (node.mainBody.Config.ShipLog?.mapMode?.details?.Length > 0)
            {
                GameObject detailsParent = new GameObject("Details");
                detailsParent.transform.SetParent(node.astroObject.transform);
                detailsParent.SetActive(false);

                RectTransform detailsTransform = detailsParent.AddComponent<RectTransform>();
                detailsTransform.localPosition = Vector3.zero;
                detailsTransform.localRotation = Quaternion.identity;
                detailsTransform.localScale = Vector3.one;

                foreach (ShipLogModule.ShipLogDetailInfo detailInfo in node.mainBody.Config.ShipLog.mapMode.details)
                {
                    MakeDetail(detailInfo, detailsTransform, node.mainBody.Mod.Assets, greyScaleMaterial);
                }
                detailsParent.SetActive(true);
            }
        }

        private static void MakeNode(ref MapModeObject node, GameObject parent, Material greyScaleMaterial, int layer)
        {
            const float padding = 250f;

            GameObject newNodeGO = new GameObject(node.mainBody.Config.Name + "_ShipLog");
            newNodeGO.layer = layer;
            newNodeGO.transform.SetParent(parent.transform);

            RectTransform transform = newNodeGO.AddComponent<RectTransform>();
            float scale = node.mainBody.Config.ShipLog?.mapMode?.scale ?? 1f;
            Vector2 position = Vector2.zero;
            if (node.lastSibling != null)
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
            transform.localPosition = new Vector3(position.x, position.y, 0);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * scale;
            CreateShipLogAstroObject(newNodeGO, ref node, GameObject.Find(ShipLogHandler.PAN_ROOT_PATH + "/TimberHearth/UnviewedIcon"), greyScaleMaterial, layer);
            if (node.lastSibling != null) ConnectNodeToLastSibling(node, greyScaleMaterial);
            MakeDetails(node, greyScaleMaterial);
            transform.SetAsFirstSibling();
        }

        private static MapModeObject ConstructPrimaryNode(string systemName)
        {
            foreach (NewHorizonsBody body in Main.BodyDict[systemName].Where(b => b.Config.Base.CenterOfSolarSystem))
            {
                List<NewHorizonsBody> searchList = Main.BodyDict[systemName].Where(b => (b.Config.ShipLog?.mapMode?.remove ?? false) == false).ToList();
                searchList.Sort((b, o) => b.Config.Orbit.SemiMajorAxis.CompareTo(o.Config.Orbit.SemiMajorAxis));
                MapModeObject newNode = new MapModeObject
                {
                    mainBody = body,
                    level = 0,
                    x = 0,
                    y = 0
                };
                newNode.children = ConstructChildrenNodes(systemName, newNode, searchList);
                return newNode;
            }
            Logger.LogError("Couldn't find center of system!");
            return new MapModeObject();
        }

        private static List<MapModeObject> ConstructChildrenNodes(string systemName, MapModeObject parent, List<NewHorizonsBody> searchList)
        {
            List<MapModeObject> children = new List<MapModeObject>();
            int newX = parent.x;
            int newY = parent.y;
            int newLevel = parent.level + 1;
            MapModeObject lastSibling = parent;
            foreach (NewHorizonsBody body in searchList.Where(b => b.Config.Orbit.PrimaryBody == parent.mainBody.Config.Name))
            {
                if (body.Config.Orbit.PrimaryBody == parent.mainBody.Config.Name)
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
                    newNode.children = ConstructChildrenNodes(systemName, newNode, searchList);
                    if (even)
                    {
                        newY += newNode.branch_height;
                        parent.Increment_height();
                        newY += 1;
                    }
                    else
                    {
                        newX += newNode.branch_width;
                        parent.Increment_width();
                        newX += 1;
                    }
                    lastSibling = newNode;
                    children.Add(newNode);
                }
            }
            return children;
        }
    }

}
