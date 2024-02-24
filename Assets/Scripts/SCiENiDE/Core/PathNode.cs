using UnityEngine;

namespace SCiENiDE.Core
{
    public class PathNode : IPathNode
    {
        private readonly int _x;
        private readonly int _y;
        private NodeTerrain _terrain;

        public PathNode(int x, int y, MoveDifficulty moveDifficulty = MoveDifficulty.Medium)
        {
            _x = x;
            _y = y;
            Coords = new Vector2(x, y);
            _terrain = new NodeTerrain
            {
                Difficulty = moveDifficulty
            };

            Visited = false;
            IsPath = false;
            fScore = 0;
        }

        public NodeTerrain Terrain
        {
            get { return _terrain; }
            set { _terrain = value; }
        }

        public bool Visited { get; set; }

        public bool IsPath { get; set; }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public float fScore { get; set; }

        public Vector2 Coords { get; private set; }

        public string ToLongString()
        {
            return $"{_x}:{_y}{(Visited ? "*" : string.Empty)}\r\n{(int)_terrain.Difficulty}/{fScore.ToString("F1")}";
        }

        public override string ToString()
        {
            return $"{_x}:{_y}";
        }
    }
}
