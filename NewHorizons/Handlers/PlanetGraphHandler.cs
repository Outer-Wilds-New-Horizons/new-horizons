using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Handlers
{
    public class PlanetGraphHandler : IEnumerable<PlanetGraphHandler.PlanetNode>
    {
        public class PlanetNode
        {
            public NewHorizonsBody body;
            public PlanetNode parent;
            public IEnumerable<PlanetNode> children;
        }

        public class FocalPointNode : PlanetNode
        {
            public PlanetNode primary;
            public PlanetNode secondary;
        }

        private PlanetNode _rootNode;

        public PlanetGraphHandler(IEnumerable<NewHorizonsBody> iBodies)
        {
            var bodies = iBodies.ToArray();
            var centers = bodies.Where(b => b.Config.Base.centerOfSolarSystem).ToArray();
            if (centers.Length == 1)
            {
                _rootNode = ConstructGraph(centers[0], bodies);
            }
            else
            {
                if (centers.Length == 0 && Main.Instance.CurrentStarSystem == "SolarSystem")
                {
                    var SunConfig = new PlanetConfig();
                    SunConfig.name = "Sun";
                    _rootNode = ConstructGraph(new NewHorizonsBody(SunConfig, Main.Instance), bodies);
                }
                else
                {
                    Logger.LogError($"There must be one and only one centerOfSolarSystem! Found [{centers.Length}]");
                }
            }
        }

        public PlanetGraphHandler() { }

        public static List<PlanetGraphHandler> ConstructStockGraph(NewHorizonsBody[] bodies)
        {
            var astroObjects = bodies.Select(x => AstroObjectLocator.GetAstroObject(x.Config.name)).ToArray();
            var children = astroObjects.Select(x => AstroObjectLocator.GetChildren(x)).ToArray();
            var nodeDict = new Dictionary<NewHorizonsBody, PlanetNode>();

            //Logger.Log($"AstroObjects [{String.Join(", ", astroObjects.ToList())}]");

            var childBodyDict = new Dictionary<NewHorizonsBody, List<NewHorizonsBody>>();

            // Mmmm O(N^2) moment but this is limited to stock bodies
            for (int i = 0; i < bodies.Length; i++)
            {
                var ao = astroObjects[i];

                var childBodies = new List<NewHorizonsBody>();
                for (int j = 0; j < bodies.Length; j++)
                {
                    if (i == j) continue;
                    // If the list of children for current object (i) containes the ao for body at index j
                    if (children[i] != null && astroObjects[j]?.gameObject != null && children[i].Contains(astroObjects[j].gameObject))
                    {
                        childBodies.Add(bodies[j]);
                    }
                    // If uh the primary body straight up matches the name
                    else if (bodies[j].Config.Orbit.primaryBody == bodies[i].Config.name)
                    {
                        childBodies.Add(bodies[j]);
                    }
                    // If finding the astro object of the primary body matches the astro object but not null bc if its a new planet it'll always be null
                    else if (AstroObjectLocator.GetAstroObject(bodies[j].Config.Orbit.primaryBody) == astroObjects[i] && astroObjects[i] != null)
                    {
                        childBodies.Add(bodies[j]);
                    }
                }

                childBodyDict.Add(bodies[i], childBodies);

                //Logger.Log($"Children of [{ao}] : [{String.Join(", ", childBodyDict[bodies[i]].Select(x => x.Config.Name).ToList())}]");

                nodeDict.Add(bodies[i], new PlanetNode
                {
                    body = bodies[i]
                });
            }

            // Set their children/parents
            foreach (var body in bodies)
            {
                nodeDict[body].children = childBodyDict[body].Select(x => nodeDict[x]);
                foreach (var child in nodeDict[body].children)
                {
                    child.parent = nodeDict[body];
                }
            }

            // Verifying it worked
            foreach (var node in nodeDict.Values.ToList())
            {
                var childrenString = String.Join(", ", node.children.Select(x => x?.body?.Config?.name).ToList());
                Logger.Log($"NODE: [{node?.body?.Config?.name}], [{node?.parent?.body?.Config?.name}], [{childrenString}]");
            }

            // Return all tree roots (no parents)
            return nodeDict.Values.Where(x => x.parent == null).Select(x => new PlanetGraphHandler() { _rootNode = x }).ToList();
        }

        private static bool DetermineIfChildOfFocal(NewHorizonsBody body, FocalPointNode node)
        {
            var name = body.Config.name.ToLower();
            var primary = (body.Config.Orbit?.primaryBody ?? "").ToLower();
            var primaryName = node.primary.body.Config.name.ToLower();
            var secondaryName = node.secondary.body.Config.name.ToLower();
            return name != primaryName && name != secondaryName && (primary == node.body.Config.name.ToLower() || primary == primaryName || primary == secondaryName);
        }


        private static PlanetNode ConstructGraph(NewHorizonsBody body, NewHorizonsBody[] bodies)
        {
            if (body.Config.FocalPoint == null)
            {
                return new PlanetNode
                {
                    body = body,
                    children = bodies
                        .Where(b => string.Equals(b.Config.Orbit.primaryBody, body.Config.name, StringComparison.CurrentCultureIgnoreCase))
                        .Select(b => ConstructGraph(b, bodies))
                };
            }
            else
            {
                var newNode = new FocalPointNode
                {
                    body = body
                };
                foreach (var child in bodies)
                {
                    if (string.Equals(child.Config.name, body.Config.FocalPoint.primary, StringComparison.CurrentCultureIgnoreCase))
                    {
                        newNode.primary = new PlanetNode
                        {
                            body = child,
                            children = new List<PlanetNode>()
                        };
                    }
                    else if (string.Equals(child.Config.name, body.Config.FocalPoint.secondary, StringComparison.CurrentCultureIgnoreCase))
                    {
                        newNode.secondary = new PlanetNode
                        {
                            body = child,
                            children = new List<PlanetNode>()
                        };
                    }
                }
                newNode.children = bodies
                    .Where(b => DetermineIfChildOfFocal(b, newNode))
                    .Select(b => ConstructGraph(b, bodies));
                return newNode;
            }
        }

        public IEnumerator<PlanetNode> GetEnumerator()
        {
            yield return _rootNode;
            var queue = new Queue<PlanetNode>(_rootNode.children);
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                yield return node;
                foreach (var child in node.children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}