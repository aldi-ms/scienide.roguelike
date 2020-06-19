using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class AStarPathfinding
    {
        private const float p = 1f / 1000f;

        public static T[] Pathfind<T>(BaseGrid<T> map, int startX, int startY, int endX, int endY) where T : class, IPathNode<T>
        {
            T endNode = map.GetGridCell(endX, endY);
            if (endNode == null || endNode.Terrain.Difficulty == MoveDifficulty.NotWalkable)
            {
                return null;
            }

            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            Dictionary<T, int> costSoFar = new Dictionary<T, int>();
            PriorityQueue<T> openSet = new PriorityQueue<T>(
                Comparer<T>.Create((x, y) =>
                {
                    if (x == null && y == null) return 0;
                    if (x == null && y != null) return 1;
                    if (x != null && y == null) return -1;

                    return (x.fScore != y.fScore)
                        ? x.fScore.CompareTo(y.fScore)
                        : x.Terrain.Difficulty - y.Terrain.Difficulty;
                }),
                map.Width * map.Height);

            T startNode = map.GetGridCell(startX, startY);
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
                T currentNode = openSet.Pop();

                if (currentNode.x == endX && currentNode.y == endY)
                {
                    pathFound = true;
                    break;
                }

                foreach (T neighbourNode in currentNode.NeighbourNodes.Where(x => x.Terrain.Difficulty != MoveDifficulty.NotWalkable))
                {
                    int newCost = costSoFar[currentNode] + (int)neighbourNode.Terrain.Difficulty;
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
        private static T[] RecostructPath<T>(BaseGrid<T> map, Dictionary<T, T> cameFrom, int startX, int startY, int endX, int endY) where T : class, IPathNode<T>
        {
            T current = map.GetGridCell(endX, endY);
            T startNode = map.GetGridCell(startX, startY);

            List<T> path = new List<T>();

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
            Debug.Log($"Path score: [{path.Select(x => (int)x.Terrain.Difficulty).Sum()}].");

            return path.ToArray();
        }
    }
}
