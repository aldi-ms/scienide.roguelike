using System.Collections.Generic;

namespace SCiENiDE.Core
{
    public class PathNode<T> : IPathNode<T> where T: IPathNode<T>
    {
        private int _x;
        private int _y;
        private NodeTerrain _terrain;
        private BaseGrid<T> _gridMap;

        public PathNode(BaseGrid<T> gridMap, int x, int y, MoveDifficulty moveDifficulty = MoveDifficulty.Medium)
        {
            _x = x;
            _y = y;
            _gridMap = gridMap;
            _terrain = new NodeTerrain
            {
                Difficulty = moveDifficulty
            };
        }

        public T[] NeighbourNodes
        {
            get
            {
                List<T> tempNeighbourNodes = new List<T>();
                for (int dX = -1; dX <= 1; dX++)
                {
                    for (int dY = -1; dY <= 1; dY++)
                    {
                        if (dX == 0 && dY == 0)
                        {
                            continue;
                        }

                        T node = _gridMap.GetGridCell(_x + dX, _y + dY);
                        if (node != null && node.Terrain.Difficulty != MoveDifficulty.NotWalkable)
                        {
                            tempNeighbourNodes.Add(node);
                        }
                    }
                }

                return tempNeighbourNodes.ToArray();
            }
        }
        public NodeTerrain Terrain
        {
            get { return _terrain; }
            set { _terrain = value; }
        }
        public bool Visited { get; set; }
        public bool IsPath { get; set; }
        public int x
        {
            get { return _x; }
            protected set { _x = value; }
        }
        public int y
        {
            get { return _y; }
            protected set { _y = value; }
        }
        public float fScore { get; set; }

        public override string ToString()
        {
            return $"{_x}:{_y}{(Visited ? "*" : string.Empty)}\r\n{(int)_terrain.Difficulty}/{fScore.ToString("F1")}";
        }
    }
}
