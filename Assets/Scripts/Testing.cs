using SCiENiDE.Core;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Testing : MonoBehaviour
{
    [SerializeField]
    private int WidthInCells = 64;

    [SerializeField]
    private int HeightInCells = 36;

    private readonly static List<int> _customSeeds = new List<int> {
        23013203,
        122014906,
    };

    private void Start()
    {
        var sw = Stopwatch.StartNew();

        MapGenerator generator = new MapGenerator(
            WidthInCells,
            HeightInCells,
            displayCallback: (g, c) => InitializeVisualArray(g, ref c),
            fillPercent: 48,
            smoothing: 2);

        generator.Map.OnGridCellChanged += (object sender, Grid<IPathNode>.OnGridCellChangedEventArgs args) =>
        {
            if (sender is not Grid<IPathNode> map)
            {
                return;
            }

            var pathNode = map.GetPathNode(args.x, args.y);
            if (args.CellMap == null)
            {
                return;
            }

            var cell = args.CellMap[args.x, args.y];
            var sre = cell.GetComponent<SpriteRenderer>();
            var texture2d = Utils.GetSharedSingleColorTexture2D(Utils.GetPathNodeColor(map.GetPathNode(args.x, args.y)));
            var block = new MaterialPropertyBlock();
            block.SetTexture("_MainTex", texture2d);
            sre.SetPropertyBlock(block);
        };

        var map = generator.GenerateMap(MapType.RandomFill);

        sw.Stop();
        Debug.Log($"Time spent generating map: [{sw.ElapsedMilliseconds}]ms.");

        var path = AStarPathfinding.Pathfind(map, Random.Range(0, WidthInCells), Random.Range(0, HeightInCells), Random.Range(0, WidthInCells), Random.Range(0, HeightInCells));
    }

    private void InitializeVisualArray(Grid<IPathNode> map, ref Component[,] visualArray)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                visualArray[x, y] = Utils.CreateMapCell(
                    null,
                    map.CellSize,
                    Utils.GetPathNodeColor(map.GetPathNode(x, y)),
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
