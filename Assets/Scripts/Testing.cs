using SCiENiDE.Core;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private BaseGrid<PathNode> _grid;
    private void Start()
    {
        List<PathNode.MoveDifficulty> moveDifficulties = new List<PathNode.MoveDifficulty> {
            PathNode.MoveDifficulty.Hard,
            PathNode.MoveDifficulty.Medium,
            PathNode.MoveDifficulty.None,
            PathNode.MoveDifficulty.NotWalkable,
            PathNode.MoveDifficulty.Easy
        };

        _grid = new BaseGrid<PathNode>(24, 14, 8f, new Vector3(-100f, -50f),
            (BaseGrid<PathNode> g, int x, int y) => new PathNode(g, x, y), //, moveDifficulties[Random.Range(0, 5)]
            (BaseGrid<PathNode> g, TextMesh[,] tm) => PathNodeDebug(g, tm));

        var path = AStarPathfinding.Pathfind(_grid, 0, 0, 9, 7);
    }

    private void PathNodeDebug(BaseGrid<PathNode> gridArray, TextMesh[,] debugTextArray)
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
                    GetColorFromNodeWalkDifficulty(gridArray.GetGridCell(x, y).NodeMoveDifficulty),
                    TextAnchor.MiddleCenter);

                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(gridArray.GetWorldPosition(x, y), gridArray.GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }

        Debug.DrawLine(gridArray.GetWorldPosition(0, gridArray.Height), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);
        Debug.DrawLine(gridArray.GetWorldPosition(gridArray.Width, 0), gridArray.GetWorldPosition(gridArray.Width, gridArray.Height), Color.white, 100f);

        gridArray.OnGridCellChanged += (object sender, BaseGrid<PathNode>.OnGridCellChangedEventArgs args) =>
        {
            PathNode gridCell = gridArray.GetGridCell(args.x, args.y);
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
            }
        };
    }
    private Color GetColorFromNodeWalkDifficulty(PathNode.MoveDifficulty difficulty)
    {
        switch (difficulty)
        {
            case PathNode.MoveDifficulty.None:
                return Color.white;
            case PathNode.MoveDifficulty.Easy:
                return Color.green;
            case PathNode.MoveDifficulty.Medium:
                return Color.yellow;
            case PathNode.MoveDifficulty.Hard:
                return Color.red;
            case PathNode.MoveDifficulty.NotWalkable:
                return Color.black;
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
