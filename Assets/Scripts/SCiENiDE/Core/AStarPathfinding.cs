using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class AStarPathfinding
    {
        private const float p = 1f / 1000f;

        public static IPathNode[] Pathfind(BaseGrid<IPathNode> map, int startX, int startY, int endX, int endY)
        {
            var endNode = map.GetGridCell(endX, endY);
            if (endNode == null || endNode.Terrain.Difficulty == MoveDifficulty.NotWalkable)
            {
                return null;
            }

            var cameFrom = new Dictionary<IPathNode, IPathNode>();
            var costSoFar = new Dictionary<IPathNode, int>();
            var openSet = new PriorityQueue<IPathNode>(
                Comparer<IPathNode>.Create((x, y) =>
                {
                    //if (x == null && y == null) return 0;
                    //if (x == null && y != null) return 1;
                    //if (x != null && y == null) return -1;

                    return (x.fScore != y.fScore)
                        ? x.fScore.CompareTo(y.fScore)
                        : x.Terrain.Difficulty - y.Terrain.Difficulty;
                }),
                map.Width * map.Height);

            var startNode = map.GetGridCell(startX, startY);
            if (startNode == null)
            {
                return null;
            }

            openSet.Push(startNode);
            costSoFar.Add(startNode, 0);
            cameFrom.Add(startNode, null);
            startNode.Visited = true;
            startNode.IsPath = true;
            map.TriggerOnGridCellChanged(startX, startY);
            bool pathFound = false;

            while (openSet.Peek() != null)
            {
                var currentNode = openSet.Pop();

                if (currentNode.X == endX && currentNode.Y == endY)
                {
                    pathFound = true;
                    break;
                }

                var walkableNeighbourNodes = map.GetNeighbourNodesCached(currentNode.Coords).Where(x => x.Terrain.Difficulty != MoveDifficulty.NotWalkable);
                foreach (var node in walkableNeighbourNodes)
                {
                    int newCost = costSoFar[currentNode] + (int)node.Terrain.Difficulty;
                    if (!costSoFar.ContainsKey(node)
                        || newCost < costSoFar[node])
                    {
                        costSoFar[node] = newCost;
                        float hScore = Heuristic(node.X, node.Y, endX, endY) * (1f + p);
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
                return RecostructPath(map, cameFrom, startX, startY, endX, endY);
            }
            else
            {
                Debug.Log($"Could not find valid path from [{startX}:{startY}] to [{endX}:{endY}].");
                return null;
            }
        }

        private const float MainMoveCost = 1f;
        private readonly static float DiagonalMoveCost = Mathf.Sqrt(2);
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
        private static IPathNode[] RecostructPath(BaseGrid<IPathNode> map, Dictionary<IPathNode, IPathNode> cameFrom, int startX, int startY, int endX, int endY)
        {
            var current = map.GetGridCell(endX, endY);
            var startNode = map.GetGridCell(startX, startY);

            var path = new List<IPathNode>();

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
