using SCiENiDE.Core;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.SCiENiDE.Core
{
    public class Room
    {
        private List<MapNode> _tiles;
        private List<MapNode> _edgeTiles;
        private int _roomSize;
        private List<Room> _neighbourRooms;

        public List<MapNode> Tiles
        {
            get
            {
                return _tiles;
            }
        }
        public int Size { get { return _roomSize; } }


        public Room(List<MapNode> roomTiles, BaseGrid<MapNode> map)
        {
            _tiles = roomTiles;
            _roomSize = _tiles.Count;
            _edgeTiles = new List<MapNode>();
            _neighbourRooms = new List<Room>();

            foreach (MapNode tile in roomTiles)
            {
                if (tile.NeighbourNodes.Any(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable))
                {
                    _edgeTiles.Add(tile);
                }
            }
        }

        public bool IsConnectedTo(Room x)
        {
            return _neighbourRooms.Contains(x);
        }

        public static void ConnectRooms(Room a, Room b)
        {
            a._neighbourRooms.Add(b);
            b._neighbourRooms.Add(a);
        }
    }
}
