using System;
using System.Collections.Generic;
using System.Linq;

namespace SCiENiDE.Core
{
    public static class CellularAutomataExtensions
    {
        #region Rulesets
        private static Dictionary<MapType, Func<MapNode[], NodeTerrain, MoveDifficulty>> _rulesets
            = new Dictionary<MapType, Func<MapNode[], NodeTerrain, MoveDifficulty>>
            {
                {
                    MapType.Rooms,
                    (MapNode[] neigbours, NodeTerrain currentTerrain) =>
                    {
                        if (neigbours.Length >= 6) return MoveDifficulty.Easy;
                        if (neigbours.Length >= 4) return MoveDifficulty.Medium;
                        if (neigbours.Length >= 2) return MoveDifficulty.Hard;
                        if (neigbours.Length == 0) return MoveDifficulty.NotWalkable;
                        return currentTerrain.Difficulty;
                    }
                },
                {
                    MapType.RandomFill,
                    (MapNode[] neighbours, NodeTerrain currentTerrain) =>
                    {
                        //if (neighbours.Length < 8) return MoveDifficulty.NotWalkable;
                        int d = 8 - neighbours.Length;
                        int wallCount = neighbours.Count(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable) + d;
                        if (wallCount > 4) return MoveDifficulty.NotWalkable;
                        if (wallCount < 4) return MoveDifficulty.Easy;
                        return currentTerrain.Difficulty;
                    }
                }
            };
        #endregion

        public static void RunCARuleset(this BaseGrid<MapNode> map, MapType mapType)
        {
            MoveDifficulty[,] modifiedMap = new MoveDifficulty[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var node = map[x, y];
                    var result = _rulesets[mapType](node.NeighbourNodes, node.Terrain);
                    modifiedMap[x, y] = result;
                }
            }

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    map[x, y].Terrain.Difficulty = modifiedMap[x, y];
                    map.TriggerOnGridCellChanged(x, y);
                }
            }
        }
    }
}
