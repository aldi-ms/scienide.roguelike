using SCiENiDE.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.SCiENiDE.Core
{
    public class Room : IComparable<Room>
    {
        private List<IPathNode> _tiles;
        private List<IPathNode> _edgeTiles;
        private int _roomSize;
        private List<Room> _neighbourRooms;
        public bool IsAccesibleFromMainRoom { get; set; }
        public bool IsMainRoom { get; set; }

        public List<IPathNode> Tiles { get { return _tiles; } }
        public List<IPathNode> EdgeTiles { get { return _edgeTiles; } }
        public int Size { get { return _roomSize; } }
        public List<Room> NeighbourRooms { get { return _neighbourRooms; } }

        public Room(List<IPathNode> roomTiles, Grid<IPathNode> map)
        {
            _tiles = roomTiles;
            _roomSize = _tiles.Count;
            _edgeTiles = new List<IPathNode>();
            _neighbourRooms = new List<Room>();

            foreach (var tile in roomTiles)
            {
                if (map.GetNeighbourNodesCached(tile.Coords).Any(x => x.Terrain.Difficulty == MoveDifficulty.NotWalkable))
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
