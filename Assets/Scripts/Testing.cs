using SCiENiDE.Core;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Testing : MonoBehaviour
{
    private List<MoveDifficulty> _moveDifficulties = new List<MoveDifficulty> {
            MoveDifficulty.Hard,
            MoveDifficulty.Medium,
            MoveDifficulty.None,
            MoveDifficulty.NotWalkable,
            MoveDifficulty.Easy
        };
    private List<int> CustomSeeds = new List<int> {
        23013203,
        122014906,
    };

    private void Start()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        MapGenerator generator = new MapGenerator(
            width: 64,
            height: 36,
            debugCallback: (g, tm) => BaseGridDebug(g, tm), fillPercent: 50);

        generator.Map.OnGridCellChanged += (object sender, BaseGrid<IPathNode>.OnGridCellChangedEventArgs args) =>
        {
            if (sender is BaseGrid<IPathNode> map)
            {
                var gridCell = map.GetGridCell(args.x, args.y);
                if (args.DebugTextMeshes == null)
                {
                    return;
                }

                var debugTextArray = args.DebugTextMeshes;
                debugTextArray[args.x, args.y].text = gridCell?.ToString();
                if (gridCell != null)
                {
                    if (gridCell.IsPath)
                    {
                        debugTextArray[args.x, args.y].color = Color.cyan;
                    }
                    else if (gridCell.Visited)
                    {
                        debugTextArray[args.x, args.y].color = Color.grey;
                    }
                    else
                    {
                        debugTextArray[args.x, args.y].color = GetColorFromNodeWalkDifficulty(gridCell.Terrain.Difficulty);
                    }
                }
            }
        };

        generator.GenerateMap(MapType.RandomFill, 5);
        
        sw.Stop();
        Debug.Log($"Time spent generating map: [{sw.ElapsedMilliseconds}]ms.");
    }

    public void BaseGridDebug(BaseGrid<IPathNode> map, TextMesh[,] debugTextArray)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                debugTextArray[x, y] = Utils.CreateWorldText(
                    $"{x}:{y}",
                    null,
                    map.GetWorldPosition(x, y) + new Vector3(map.CellSize, map.CellSize) * .5f,
                    12,
                    GetColorFromNodeWalkDifficulty(map.GetGridCell(x, y).Terrain.Difficulty),
                    TextAnchor.MiddleCenter);
            }
        }

        Debug.DrawLine(map.GetWorldPosition(0, map.Height), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);
        Debug.DrawLine(map.GetWorldPosition(map.Width, 0), map.GetWorldPosition(map.Width, map.Height), Color.white, 100f);
    }

    private Color GetColorFromNodeWalkDifficulty(MoveDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MoveDifficulty.None:
                return Color.white;
            case MoveDifficulty.Easy:
                return Color.green;
            case MoveDifficulty.Medium:
                return Color.yellow;
            case MoveDifficulty.Hard:
                return Color.red;
            case MoveDifficulty.NotWalkable:
                return Color.gray;
            default:
                return Color.white;
        }
    }
    private void BaseObjectToStringDebug<T>(BaseGrid<T> gridArray, TextMesh[,] debugTextArray)
    {
        for (int x = 0; x < gridArray.Width; x++)
        {
            for (int y = 0; y < gridArray.Height; y++)
            {
                debugTextArray[x, y] = Utils.CreateWorldText(
                    gridArray.GetGridCell(x, y)?.ToString(),
                    null,
                    gridArray.GetWorldPosition(x, y) + new Vector3(gridArray.CellSize, gridArray.CellSize) * .5f,
                    24,
                    Color.white,
                    TextAnchor.MiddleCenter);

                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(gridArray.GetWorldPosition(0, gridArray.Height), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);
        Debug.DrawLine(gridArray.GetWorldPosition(gridArray.Width, 0), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);

        gridArray.OnGridCellChanged += (object sender, BaseGrid<T>.OnGridCellChangedEventArgs args) =>
        {
            args.DebugTextMeshes[args.x, args.y].text = gridArray.GetGridCell(args.x, args.y)?.ToString();
        };
    }
}
