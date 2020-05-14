using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class AStarPathfinding
    {
        public static PathNode[] Pathfind(BaseGrid<PathNode> map, int startX, int startY, int endX, int endY)
        {
            PriorityQueue<PathNode> openSet = new PriorityQueue<PathNode>(
                Comparer<PathNode>.Create((x, y) =>
                {
                    if (x == null && y == null) return 0;
                    if (x == null && y != null) return 1;
                    if (x != null && y == null) return -1;

                    return x.NodeMoveDifficulty - y.NodeMoveDifficulty;
                }),
                map.Width * map.Height);

            Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();
            PathNode startNode = map.GetGridCell(startX, startY);
            if (startNode == null)
            {
                return null;
            }

            openSet.Push(startNode);
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
                    if (!cameFrom.ContainsKey(neighbourNode))
                    {
                        openSet.Push(neighbourNode);
                        cameFrom.Add(neighbourNode, currentNode);
                        neighbourNode.Visited = true;
                        map.TriggerOnGridCellChanged(neighbourNode.x, neighbourNode.y);
                    }
                }
            }

            Debug.Log($"Nodes visited: [{cameFrom.Count}].");

            return RecostructPath(map, cameFrom, startX, startY, endX, endY);
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

            Debug.Log($"Path nodes count: [{path.Count}].");
            Debug.Log($"Path score: [{path.Select(x => (int)x.NodeMoveDifficulty).Sum()}].");

            return path.ToArray();
        }
    }
}
