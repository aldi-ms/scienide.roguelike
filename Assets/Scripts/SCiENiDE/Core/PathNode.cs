using System.Collections.Generic;

namespace SCiENiDE.Core
{
    public class PathNode
    {
        public enum MoveDifficulty
        {
            None = 0,
            Easy = 10,
            Medium = 50,
            Hard = 75,
            NotWalkable = 100
        };

        private int _x;
        private int _y;
        private MoveDifficulty _moveDifficulty;
        private BaseGrid<PathNode> _gridMap;
        private PathNode[] _neighbourNodes;


        public PathNode(BaseGrid<PathNode> gridMap, int x, int y, MoveDifficulty moveDifficulty = MoveDifficulty.Medium)
        {
            _x = x;
            _y = y;
            _gridMap = gridMap;
            _moveDifficulty = moveDifficulty;
        }

        public PathNode[] NeighbourNodes
        {
            get
            {
                if (_neighbourNodes != null)
                {
                    return _neighbourNodes;
                }

                List<PathNode> tempNeighbourNodes = new List<PathNode>();
                for (int dX = -1; dX <= 1; dX++)
                {
                    for (int dY = -1; dY <= 1; dY++)
                    {
                        if (dX == 0 && dY == 0)
                        {
                            continue;
                        }

                        PathNode node = _gridMap.GetGridCell(_x + dX, _y + dY);
                        if (node != null && node._moveDifficulty != MoveDifficulty.NotWalkable)
                        {
                            tempNeighbourNodes.Add(node);
                        }
                    }
                }

                _neighbourNodes = tempNeighbourNodes.ToArray();
                return _neighbourNodes;
            }
        }
        public MoveDifficulty NodeMoveDifficulty
        {
            get { return _moveDifficulty; }
        }
        public bool Visited { get; set; }
        public bool IsPath { get; set; }
        public int x { get { return _x; } }
        public int y { get { return _y; } }

        public override string ToString()
        {
            return $"{_x}:{_y}{(Visited ? "*" : string.Empty)}\r\n{(int)_moveDifficulty}";
        }
    }
}
