using SCiENiDE.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCiENiDE.Core.GameObjects
{
    public class Room : IComparable<Room>
    {
        private List<PathNode> _tiles;
        private List<PathNode> _edgeTiles;
        private int _roomSize;
        private List<Room> _neighbourRooms;
        public bool IsAccesibleFromMainRoom { get; set; }
        public bool IsMainRoom { get; set; }
        
        public List<PathNode> Tiles { get { return _tiles; } }
        public List<PathNode> EdgeTiles { get { return _edgeTiles; } }
        public int Size { get { return _roomSize; } }
        public List<Room> NeighbourRooms { get { return _neighbourRooms; } }

        public Room(List<PathNode> roomTiles, Grid map)
        {
            _tiles = roomTiles;
            _roomSize = _tiles.Count;
            _edgeTiles = new List<PathNode>();
            _neighbourRooms = new List<Room>();

            foreach (var tile in roomTiles)
            {
                if (map.GetNeighbourNodesWithCache(tile.Coords).Any(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable))
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
            if (a.IsAccesibleFromMainRoom)
            {
                b.SetAccessibleFromMainRoom();
            }
            else if (b.IsAccesibleFromMainRoom)
            {
                a.SetAccessibleFromMainRoom();
            }

            a._neighbourRooms.Add(b);
            b._neighbourRooms.Add(a);
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!IsAccesibleFromMainRoom)
            {
                IsAccesibleFromMainRoom = true;
                foreach (Room room in _neighbourRooms)
                {
                    room.SetAccessibleFromMainRoom();
                }
            }
        }

        public int CompareTo(Room x)
        {
            return x.Size.CompareTo(this.Size);
        }
    }
}
