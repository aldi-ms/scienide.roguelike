using SCiENiDE.Core;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Testing : MonoBehaviour
{
    private List<int> CustomSeeds = new List<int> {
        23013203,
        122014906,
    };

    private void Start()
    {
        const int Width = 64;
        const int Height = 36;
        Stopwatch sw = Stopwatch.StartNew();

        MapGenerator generator = new MapGenerator(
            Width,
            Height,
            debugCallback: (g, c) => FillInDisplayMap(g, ref c), fillPercent: 16);

        generator.Map.OnGridCellChanged += (object sender, BaseGrid<IPathNode>.OnGridCellChangedEventArgs args) =>
        {
            if (sender is BaseGrid<IPathNode> map)
            {
                var pathNode = map.GetPathNode(args.x, args.y);
                if (args.CellMap == null)
                {
                    return;
                }

                if (args.CellMap[args.x, args.y] is TextMesh textMesh)
                {
                    textMesh.text = pathNode?.ToString();
                    if (pathNode != null)
                    {
                        if (pathNode.IsPath)
                        {
                            textMesh.color = Color.cyan;
                        }
                        else if (pathNode.Visited)
                        {
                            textMesh.color = Color.white;
                        }
                        else
                        {
                            textMesh.color = GetPathNodeColor(pathNode);
                        }
                    }
                }
            }
        };

        var map = generator.GenerateMap(MapType.RandomFill, 5);
        
        sw.Stop();
        Debug.Log($"Time spent generating map: [{sw.ElapsedMilliseconds}]ms.");

        var path = AStarPathfinding.Pathfind(map, Random.Range(0, Width), Random.Range(0, Height), Random.Range(0, Width), Random.Range(0, Height));
    }

    private void FillInDisplayMap(BaseGrid<IPathNode> map, ref Component[,] displayMap)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                displayMap[x, y] = Utils.CreateMapCell(
                    null,
                    map.CellSize, 
                    GetPathNodeColor(map.GetPathNode(x, y)), 
                    map.GetWorldPosition(x, y) + new Vector3(map.CellSize, map.CellSize) * .5f);

                //displayMap[x, y] = Utils.CreateWorldText(
                //    $"{x}:{y}",
                //    null,
                //    map.GetWorldPosition(x, y) + new Vector3(map.CellSize, map.CellSize) * .5f,
                //    12,
                //    GetPathNodeColor(map.GetPathNode(x, y)),
                //    TextAnchor.MiddleCenter);
            }
        }

        Debug.DrawLine(map.GetWorldPosition(0, map.Height), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);
        Debug.DrawLine(map.GetWorldPosition(map.Width, 0), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);
    }

    private static Color GetPathNodeColor(IPathNode node)
    {
        switch (node.Terrain.Difficulty)
        {
            case MoveDifficulty.Easy:
                return Color.green;
            case MoveDifficulty.Medium:
                return Color.yellow;
            case MoveDifficulty.Hard:
                return Color.red;
            case MoveDifficulty.NotWalkable:
                return Color.gray;

            case MoveDifficulty.None:
            default:
                return Color.white;
        }
    }

    //private void BaseObjectToStringDebug<T>(BaseGrid<T> map, TextMesh[,] debugTextArray)
    //{
    //    for (int x = 0; x < map.Width; x++)
    //    {
    //        for (int y = 0; y < map.Height; y++)
    //        {
    //            debugTextArray[x, y] = Utils.CreateWorldText(
    //                map.GetGridCell(x, y)?.ToString(),
    //                null,
    //                map.GetWorldPosition(x, y) + new Vector3(map.CellSize, map.CellSize) * .5f,
    //                24,
    //                Color.white,
    //                TextAnchor.MiddleCenter);

    //            Debug.DrawLine(map.GetWorldPosition(x, y), map.GetWorldPosition(x, y + 1), Color.white, 100f);
    //            Debug.DrawLine(map.GetWorldPosition(x, y), map.GetWorldPosition(x + 1, y), Color.white, 100f);
    //        }
    //    }

    //    Debug.DrawLine(map.GetWorldPosition(0, map.Height), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);
    //    Debug.DrawLine(map.GetWorldPosition(map.Width, 0), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);

    //    map.OnGridCellChanged += (object sender, BaseGrid<T>.OnGridCellChangedEventArgs args) =>
    //    {
    //        (args.DebugTextMeshes[args.x, args.y] as TextMesh).text = map.GetGridCell(args.x, args.y)?.ToString();
    //    };
    //}
}
