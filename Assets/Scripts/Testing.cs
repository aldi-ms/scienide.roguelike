using SCiENiDE.Core;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private GridBase<int> _grid;
    private void Start()
    {
        _grid = new GridBase<int>(20, 10, 8f, new Vector3(-70f, -40f), (GridBase<int> grid, int x, int y) => 0);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _grid.SetGridCell(Utils.GetMouseWorldPosition(), 7);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(_grid.GetGridCell(Utils.GetMouseWorldPosition()));
        }
    }
}
