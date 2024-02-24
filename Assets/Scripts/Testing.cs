using SCiENiDE.Core;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Grid = SCiENiDE.Core.Grid;

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

    private static Grid _map;

    private void Start()
    {
        var sw = Stopwatch.StartNew();

        var generator = new MapGenerator(WidthInCells, HeightInCells, fillPercent: 48, smoothing: 2, seed: 903168484);

        generator.Map.OnGridCellChanged += (object sender, Grid.OnGridCellChangedEventArgs args) =>
        {
            if (sender is not Grid map)
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

        _map = generator.GenerateMap(MapType.RandomFill);

        sw.Stop();
        Debug.Log($"Time spent generating map: [{sw.ElapsedMilliseconds}]ms.");

        // Play around with pathfinding
        var path = AStarPathfinding.Pathfind(
            _map,
            _map.GetPathNode(42, 3),//map.GetRandomAvailablePathNode(),
            _map.GetPathNode(3, 2));//map.GetRandomAvailablePathNode());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var node = _map.GetMousePositionInGrid();
            Debug.Log(node.ToLongString());
        }
    }
}
