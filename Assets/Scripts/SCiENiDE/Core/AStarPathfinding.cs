using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class AStarPathfinding
    {
        private const float p = 1f / 1000f;

        public static PathNode[] Pathfind(BaseGrid<PathNode> map, int startX, int startY, int endX, int endY)
        {
            PathNode endNode = map.GetGridCell(endX, endY);
            if (endNode == null || endNode.NodeMoveDifficulty == PathNode.MoveDifficulty.NotWalkable)
            {
                return null;
            }

            Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
            Dictionary<PathNode, int> costSoFar = new Dictionary<PathNode, int>();
            PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>(
                Comparer<PathNode>.Create((x, y) =>
                {
                    if (x == null && y == null) return 0;
                    if (x == null && y != null) return 1;
                    if (x != null && y == null) return -1;

                    return (x.fScore != y.fScore)
                        ? x.fScore.CompareTo(y.fScore)
                        : x.NodeMoveDifficulty - y.NodeMoveDifficulty;
                }),
                map.Width * map.Height);

            PathNode startNode = map.GetGridCell(startX, startY);
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

            while (openSet.Peek() != null)
            {
                PathNode currentNode = openSet.Pop();

                if (currentNode.x == endX && currentNode.y == endY)
                {
                    break;
                }

                foreach (PathNode neighbourNode in currentNode.NeighbourNodes)
                {
                    int newCost = costSoFar[currentNode] + (int)neighbourNode.NodeMoveDifficulty;
                    if (!costSoFar.ContainsKey(neighbourNode)
                        || newCost < costSoFar[neighbourNode])
                    {
                        costSoFar[neighbourNode] = newCost;
                        float hScore = Heuristic(neighbourNode.x, neighbourNode.y, endX, endY) * (1f + p);
                        neighbourNode.fScore = newCost + hScore;
                        openSet.Push(neighbourNode);
                        cameFrom[neighbourNode] = currentNode;
                        neighbourNode.Visited = true;
                        map.TriggerOnGridCellChanged(neighbourNode.x, neighbourNode.y);
                    }
                }
            }

            return RecostructPath(map, cameFrom, startX, startY, endX, endY);
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
        private static PathNode[] RecostructPath(BaseGrid<PathNode> map, Dictionary<PathNode, PathNode> cameFrom, int startX, int startY, int endX, int endY)
        {
            PathNode current = map.GetGridCell(endX, endY);
            PathNode startNode = map.GetGridCell(startX, startY);

            List<PathNode> path = new List<PathNode>();

            while (current != startNode)
            {
                path.Add(current);
                current.IsPath = true;
                map.TriggerOnGridCellChanged(current.x, current.y);
                current = cameFrom[current];
            }

            path.Add(startNode);
            path.Reverse();

            Debug.Log($"Nodes visited: [{cameFrom.Count}].");
            Debug.Log($"Path nodes count: [{path.Count}].");
            Debug.Log($"Path score: [{path.Select(x => (int)x.NodeMoveDifficulty).Sum()}].");

            return path.ToArray();
        }
    }
}
