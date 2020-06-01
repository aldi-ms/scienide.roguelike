using UnityEngine;

namespace SCiENiDE.Core
{
    public static class CellularAutomataExtensions
    {
        public static void RunCARuleset(this BaseGrid<MapNode> map)
        {
            MoveDifficulty[,] modifiedMap = new MoveDifficulty[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var node = map[x, y];
                    var result = Ruleset(node.NeighbourNodes, node.Terrain);
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
            //if (node.Terrain.Difficulty != MoveDifficulty.NotWalkable)
            //    node.Terrain = Ruleset(node.NeighbourNodes, node.Terrain);
        }

        private static MoveDifficulty Ruleset(MapNode[] neigbours, NodeTerrain currentTerrain)
        {
            // A node can have 8 neighbours at max
            if (neigbours.Length >= 7)
            {
                return MoveDifficulty.Easy;
            }

            if (neigbours.Length >= 4)
            {
                return MoveDifficulty.Medium;
            }

            if (neigbours.Length >= 2)
            {
                return MoveDifficulty.Hard;
            }

            if (neigbours.Length == 0)
            {
                Debug.Log("Setting field to not walkable, neighbours might need to be recalculated.");
                return MoveDifficulty.NotWalkable;
            }

            return currentTerrain.Difficulty;
        }
    }
}
