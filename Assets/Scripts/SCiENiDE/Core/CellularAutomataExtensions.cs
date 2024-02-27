using SCiENiDE.Core.GameObjects;
using SCiENiDE.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCiENiDE.Core
{
    public static class CellularAutomataExtensions
    {
        #region Rulesets
        private static readonly Dictionary<MapType, Func<List<PathNode>, NodeTerrain, MoveDifficulty>> _rulesets
            = new()
            {
                {
                    MapType.Rooms,
                    (List<PathNode> neighbours, NodeTerrain currentTerrain) =>
                    {
                        int d = 8 - neighbours.Count;
                        int wallCount = 8 - neighbours.Count(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable) + d;
                        if (wallCount >= 6) return MoveDifficulty.Easy;
                        if (wallCount >= 4) return MoveDifficulty.Medium;
                        if (wallCount >= 2) return MoveDifficulty.Hard;
                        if (wallCount == 0) return MoveDifficulty.NotWalkable;
                        return currentTerrain.Difficulty;
                    }
                },
                {
                    MapType.RandomFill,
                    (List<PathNode> neighbours, NodeTerrain currentTerrain) =>
                    {
                        int d = 8 - neighbours.Count;
                        int wallCount = neighbours.Count(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable) + d;
                        if (wallCount > 4) return MoveDifficulty.NotWalkable;
                        if (wallCount < 4) return MoveDifficulty.Easy;
                        return currentTerrain.Difficulty;
                    }
                }

                // TODO: create a random fill with random difficulty patches
            };
        #endregion

        public static void RunCARuleset(this Grid map, MapType mapType)
        {
            MoveDifficulty[,] modifiedMap = new MoveDifficulty[map.Width, map.Height];

            map.ForEachCoordinate((x, y) =>
            {
                var node = map[x, y];
                var result = _rulesets[mapType](map.GetNeighbourNodesWithCache(node.Coords), node.Terrain);
                modifiedMap[x, y] = result;
            });

            map.ForEachCoordinate((x, y) =>
            {
                map[x, y].Terrain.Difficulty = modifiedMap[x, y];
                map.TriggerOnGridCellChanged(x, y);
            });
        }
    }
}
