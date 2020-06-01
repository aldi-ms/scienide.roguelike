using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class MapGenerator
    {
        private int _width;
        private int _height;
        private int _totalNodeCount;
        private Action<BaseGrid<MapNode>, TextMesh[,]> _debug;
        private RectInt[] _rooms;
        private BaseGrid<MapNode> _map;

        public MapGenerator(int width, int height, Action<BaseGrid<MapNode>, TextMesh[,]> showDebug = null)
        {
            _width = width;
            _height = height;
            _totalNodeCount = _width * _height;
            _debug = showDebug;

            int minRoomsRange = Mathf.RoundToInt(_totalNodeCount * (1.2f / 100));
            int maxRoomsRange = Mathf.RoundToInt(_totalNodeCount * (3f / 100));
            _rooms = new RectInt[Random.Range(minRoomsRange, maxRoomsRange)];

            _map = new BaseGrid<MapNode>(
                _width, _height, 6.5f, new Vector3(-110f, -60f),
                (grid, x, y) =>
                {
                    return new MapNode(grid, x, y, MoveDifficulty.NotWalkable);
                },
                _debug);
        }

        public BaseGrid<MapNode> GenerateMap()
        {
            GenerateRooms();
            _map.RunCARuleset();

            return _map;
        }

        private void GenerateRooms()
        {
            Debug.Log($"Generating [{_rooms.Length}] rooms.");

            for (int i = 0; i < _rooms.Length; i++)
            {
                Vector2Int roomSize = GetRandomRoomSize();
                RectInt tempRoom = GenerateRoom(roomSize.x, roomSize.y);

                if (!tempRoom.Equals(default))
                {
                    _rooms[i] = tempRoom;
                }
            }
        }

        private Vector2Int GetRandomRoomSize()
        {
            return new Vector2Int(Random.Range(3, 7), Random.Range(3, 6));
        }

        private RectInt GenerateRoom(
            int roomWidth,
            int roomHeight,
            MoveDifficulty moveDifficulty = MoveDifficulty.Easy,
            int maxTries = 15)
        {
            int mapX;
            int mapY;
            int tries = 0;

            RectInt createdRoom;
            do
            {
                mapX = Random.Range(0, _width - roomWidth);
                mapY = Random.Range(0, _height - roomHeight);
                createdRoom = new RectInt(mapX, mapY, roomWidth, roomHeight);

                tries++;
                if (tries > maxTries)
                {
                    Debug.Log($"MaxTries [{maxTries}] for {nameof(GenerateRoom)} reached.");
                    return default;
                }
            } while (_rooms.Any(x => x.Overlaps(createdRoom))); //mapX + width >= map.Width || mapY + height >= map.Height ||

            Debug.Log($"Creating a room at [{mapX}:{mapY}], w/h = {roomWidth}/{roomHeight}.");

            for (int x = mapX; x < mapX + roomWidth; x++)
            {
                for (int y = mapY; y < mapY + roomHeight; y++)
                {
                    _map[x, y].Terrain.Difficulty = moveDifficulty;
                }
            }

            return createdRoom;
        }

        private enum MapFeature
        {
            Rooms,
            Terrain,
            Unit,
            Object
        }
    }
}
