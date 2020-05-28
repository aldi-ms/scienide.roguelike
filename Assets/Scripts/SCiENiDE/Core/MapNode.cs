using UnityEngine;

namespace SCiENiDE.Core
{
    public class MapNode : PathNode<MapNode>
    {
        public MapNode(BaseGrid<MapNode> gridMap, int x, int y, MoveDifficulty moveDifficulty = MoveDifficulty.Medium)
            : base(gridMap, x, y, moveDifficulty)
        {
        }
    }
}
