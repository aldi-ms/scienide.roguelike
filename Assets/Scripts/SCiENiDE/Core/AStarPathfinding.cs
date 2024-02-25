using SCiENiDE.Core.GameObjects;
using SCiENiDE.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SCiENiDE.Core
{
    public class AStarPathfinding
    {
        private const float p = 1f / 1000f;
        private const float MainMoveCost = 1f;
        private readonly static float DiagonalMoveCost = Mathf.Sqrt(2);

        public static PathNode[] Pathfind(Grid map, int startX, int startY, int endX, int endY)
        {
            var startNode = map.GetPathNode(startX, startY);
            var endNode = map.GetPathNode(endX, endY);

            return Pathfind(map, startNode, endNode);
        }
        public static PathNode[] Pathfind(Grid map, PathNode startNode, PathNode endNode)
        {
            if (endNode == null || endNode.Terrain.Difficulty == MoveDifficulty.NotWalkable)
            {
                Debug.Log($"Pathfind: End is not walkable! Stopping.");
                return null;
            }

            Debug.Log($"Pathfind: Looking for valid path between [{startNode}] and [{endNode}].");

            var sw = Stopwatch.StartNew();

            var cameFrom = new Dictionary<PathNode, PathNode>();
            var costSoFar = new Dictionary<Vector2, int>();
            var openSet = new PriorityQueue<PathNode>(
                Comparer<PathNode>.Create((x, y) =>
                {
                    if (x == null && y == null) return 0;
                    if (x == null && y != null) return 1;
                    if (x != null && y == null) return -1;

                    return (x.fScore != y.fScore)
                        ? x.fScore.CompareTo(y.fScore)
                        : x.Terrain.Difficulty - y.Terrain.Difficulty;
                }),
                map.Width * map.Height);

            if (startNode == null)
            {
                return null;
            }

            openSet.Push(startNode);
            costSoFar.Add(startNode.Coords, 0);
            cameFrom.Add(startNode, null);
            startNode.Visited = true;
            startNode.IsPath = true;
            map.TriggerOnGridCellChanged(startNode.X, startNode.Y);
            bool pathFound = false;

            while (openSet.Peek() != null)
            {
                var currentNode = openSet.Pop();

                if (currentNode.X == endNode.X && currentNode.Y == endNode.Y)
                {
                    pathFound = true;
                    break;
                }

                var walkableNeighbourNodes = map.GetNeighbourNodesWithCache(currentNode.Coords)
                    .Where(x => x.Terrain.Difficulty != MoveDifficulty.NotWalkable);
                foreach (var node in walkableNeighbourNodes)
                {
                    int newCost = costSoFar[currentNode.Coords] + (int)node.Terrain.Difficulty;
                    if (!costSoFar.ContainsKey(node.Coords)
                        || newCost < costSoFar[node.Coords])
                    {
                        costSoFar[node.Coords] = newCost;
                        float hScore = Heuristic(node.X, node.Y, endNode.X, endNode.Y) * (1f + p);
                        node.fScore = newCost + hScore;
                        openSet.Push(node);
                        cameFrom[node] = currentNode;
                        node.Visited = true;
                        map.TriggerOnGridCellChanged(node.X, node.Y);
                    }
                }
            }

            if (pathFound)
            {
                var path = ReconstructPath(map, cameFrom, startNode.X, startNode.Y, endNode.X, endNode.Y);
                sw.Stop();
                
                Debug.Log($"Pathfind finished in [{sw.ElapsedMilliseconds}]ms.");

                return path;
            }
            else
            {
                Debug.Log($"Could not find valid path from [{startNode.X}:{startNode.Y}] to [{endNode.X}:{endNode.Y}].");
                return null;
            }
        }

        private static float Heuristic(int startX, int startY, int endX, int endY)
        {
            int dx = Mathf.Abs(startX - endX);
            int dy = Mathf.Abs(startY - endY);

            /* Euclidean distance */
            // return Mathf.Sqrt(dx * dy + dy * dy);

            /* Manhattan distance */
            //return 2 * dx + dy;

            /* Diagonal distance */
            return MainMoveCost * (dx + dy) + (DiagonalMoveCost - 2 * MainMoveCost) * Mathf.Min(dx, dy);
        }

        private static PathNode[] ReconstructPath(Grid map, Dictionary<PathNode, PathNode> cameFrom, int startX, int startY, int endX, int endY)
        {
            var current = map.GetPathNode(endX, endY);
            var startNode = map.GetPathNode(startX, startY);

            var path = new List<PathNode>();

            while (current != startNode)
            {
                path.Add(current);
                current.IsPath = true;
                map.TriggerOnGridCellChanged(current.X, current.Y);
                current = cameFrom[current];
            }

            path.Add(startNode);
            path.Reverse();

            Debug.Log($"Nodes visited: [{cameFrom.Count}].");
            Debug.Log($"Path nodes count: [{path.Count}].");
            Debug.Log($"Path score: [{path.Select(x => (int)x.Terrain.Difficulty).Sum()}].");

            return path.ToArray();
        }
    }
}
