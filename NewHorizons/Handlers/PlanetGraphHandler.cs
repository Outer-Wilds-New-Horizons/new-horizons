using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NewHorizons.External.Configs;
using NewHorizons.Utility;

namespace NewHorizons.Handlers
{
    public class PlanetGraphHandler : IEnumerable<PlanetGraphHandler.PlanetNode>
    {
        public class PlanetNode
        {
            public NewHorizonsBody body;
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
            var centers = bodies.Where(b => b.Config.Base.CenterOfSolarSystem).ToArray();
            if (centers.Length == 1)
            {
                _rootNode = ConstructGraph(centers[0], bodies);
            }
            else
            {
                if (centers.Length == 0 && Main.Instance.CurrentStarSystem == "SolarSystem")
                {
                    var SunConfig = new Dictionary<string, object>
                    {
                        {"name", "Sun"}
                    };
                    _rootNode = ConstructGraph(new NewHorizonsBody(new PlanetConfig(SunConfig), Main.Instance), bodies);
                }
                else
                {
                    Logger.LogError("There must be one and only one centerOfSolarSystem!");
                }
            }
        }

        private static bool DetermineIfChildOfFocal(NewHorizonsBody body, FocalPointNode node)
        {
            var name = body.Config.Name.ToLower();
            var primary = (body.Config.Orbit?.PrimaryBody ?? "").ToLower();
            var primaryName = node.primary.body.Config.Name.ToLower();
            var secondaryName = node.secondary.body.Config.Name.ToLower();
            return name != primaryName && name != secondaryName && (primary == node.body.Config.Name.ToLower() || primary == primaryName || primary == secondaryName);
        }
        

        private static PlanetNode ConstructGraph(NewHorizonsBody body, NewHorizonsBody[] bodies)
        {
            if (body.Config.FocalPoint == null)
            {
                return new PlanetNode
                {
                    body = body,
                    children = bodies
                        .Where(b => string.Equals(b.Config.Orbit.PrimaryBody, body.Config.Name, StringComparison.CurrentCultureIgnoreCase))
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
                    if (string.Equals(child.Config.Name, body.Config.FocalPoint.Primary, StringComparison.CurrentCultureIgnoreCase))
                    {
                        newNode.primary = new PlanetNode
                        {
                            body = child,
                            children = new List<PlanetNode>()
                        };
                    }
                    else if (string.Equals(child.Config.Name, body.Config.FocalPoint.Secondary, StringComparison.CurrentCultureIgnoreCase))
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