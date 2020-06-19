using SCiENiDE.Core;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private List<MoveDifficulty> _moveDifficulties = new List<MoveDifficulty> {
            MoveDifficulty.Hard,
            MoveDifficulty.Medium,
            MoveDifficulty.None,
            MoveDifficulty.NotWalkable,
            MoveDifficulty.Easy
        };

    private void Start()
    {
        MapGenerator mapGeneration = new MapGenerator(34, 18, showDebugFunc: (g, tm) => BaseGridDebug(g, tm), fillPercent: 47);
        var map = mapGeneration.GenerateMap(MapType.RandomFill, 5);
    }

    public void BaseGridDebug<T>(BaseGrid<T> gridArray, TextMesh[,] debugTextArray)
        where T : IPathNode<T>
    {
        for (int x = 0; x < gridArray.Width; x++)
        {
            for (int y = 0; y < gridArray.Height; y++)
            {
                debugTextArray[x, y] = Utils.CreateWorldText(
                    gridArray.GetGridCell(x, y)?.ToString(),
                    null,
                    gridArray.GetWorldPosition(x, y) + new Vector3(gridArray.CellSize, gridArray.CellSize) * .5f,
                    18,
                    GetColorFromNodeWalkDifficulty(gridArray.GetGridCell(x, y).Terrain.Difficulty),
                    TextAnchor.MiddleCenter);

                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(gridArray.GetWorldPosition(0, gridArray.Height), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);
        Debug.DrawLine(gridArray.GetWorldPosition(gridArray.Width, 0), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);

        gridArray.OnGridCellChanged += (object sender, BaseGrid<T>.OnGridCellChangedEventArgs args) =>
        {
            T gridCell = gridArray.GetGridCell(args.x, args.y);
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
        };
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
                return Color.blue;
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
            debugTextArray[args.x, args.y].text = gridArray.GetGridCell(args.x, args.y)?.ToString();
        };
    }
}
