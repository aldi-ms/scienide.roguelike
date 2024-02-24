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
        private static Dictionary<MapType, Func<IEnumerable<PathNode>, NodeTerrain, MoveDifficulty>> _rulesets
            = new Dictionary<MapType, Func<IEnumerable<PathNode>, NodeTerrain, MoveDifficulty>>
            {
                {
                    MapType.Rooms,
                    (IEnumerable<PathNode> neighbours, NodeTerrain currentTerrain) =>
                    {
                        int d = 8 - neighbours.Count(); // TODO: IEnumerable maybe is not the best here since Count is not O(1) operation, but it'll do for now
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
                    (IEnumerable<PathNode> neighbours, NodeTerrain currentTerrain) =>
                    {
                        int d = 8 - neighbours.Count();
                        int wallCount = neighbours.Count(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable) + d;
                        if (wallCount > 4) return MoveDifficulty.NotWalkable;
                        if (wallCount < 4) return MoveDifficulty.Easy;
                        return currentTerrain.Difficulty;
                    }
                }
            };
        #endregion

        public static void RunCARuleset(this Grid map, MapType mapType)
        {
            MoveDifficulty[,] modifiedMap = new MoveDifficulty[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var node = map[x, y];
                    var result = _rulesets[mapType](map.GetNeighbourNodesCached(node.Coords), node.Terrain);
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
