using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class MapGenerator
    {
        private bool _useRandomSeed;

        private int _width;
        private int _height;
        private int _totalNodeCount;
        private int _seed;

        private RectInt[] _rooms;
        private Action<BaseGrid<MapNode>, TextMesh[,]> _debug;
        private BaseGrid<MapNode> _map;
        private int _roomsSmoothing = 2;
        private int _randomFillSmoothing = 3;

        public MapGenerator(
            int width,
            int height,
            bool useRandomSeed = true,
            int seed = -1,
            Action<BaseGrid<MapNode>, TextMesh[,]> showDebugFunc = null)
        {
            _width = width;
            _height = height;
            _totalNodeCount = _width * _height;
            _debug = showDebugFunc;
            _seed = seed;
            _useRandomSeed = useRandomSeed;

            if (_useRandomSeed)
            {
                _seed = Environment.TickCount;
                Random.InitState(_seed);

                Debug.Log($"Init PRNG with new seed [{_seed}].");
            }
            else
            {
                Random.InitState(_seed);

                Debug.Log($"Init PRNG with seed [{_seed}].");
            }

            int minRoomsRange = Mathf.RoundToInt(_totalNodeCount * .012f);
            int maxRoomsRange = Mathf.RoundToInt(_totalNodeCount * .03f);
            _rooms = new RectInt[Random.Range(minRoomsRange, maxRoomsRange)];

            _map = new BaseGrid<MapNode>(
                _width, _height, 6.5f, new Vector3(-110f, -60f),
                (grid, x, y) =>
                {
                    return new MapNode(grid, x, y, MoveDifficulty.NotWalkable);
                },
                _debug);
        }

        public BaseGrid<MapNode> GenerateMap(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Rooms:
                    {
                        GenerateRooms();
                        for (int i = 0; i < _roomsSmoothing; i++)
                            _map.RunCARuleset(mapType);
                    }
                    break;

                case MapType.RandomFill:
                    {
                        RandomFillMap();
                        for (int i = 0; i < _randomFillSmoothing; i++)
                            _map.RunCARuleset(mapType);
                    }
                    break;

                default:
                    break;
            }

            //CheckNodeAvailability();
            return _map;
        }

        private void CheckNodeAvailability()
        {
            List<MapNode> nodesToPathfind = new List<MapNode>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    MapNode node = _map.GetGridCell(x, y);
                    if (node.Terrain.Difficulty != MoveDifficulty.Easy)
                    {
                        continue;
                    }

                    nodesToPathfind.Add(node);
                }
            }

            for (int x = 0; x < nodesToPathfind.Count; x++)
            {
                for (int y = 0; y < nodesToPathfind.Count; y++)
                {
                    if (x == y) continue;
                    MapNode[] path = AStarPathfinding.Pathfind(_map, nodesToPathfind[x].x, nodesToPathfind[x].y, nodesToPathfind[y].x, nodesToPathfind[y].y);

                    if (path == null)
                    {
                        Debug.Log($"No path found from [{nodesToPathfind[x]}] to [{nodesToPathfind[y]}]");
                    }
                }
            }
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
        private void RandomFillMap(int wallPercent = 44)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _map[x, y].Terrain.Difficulty = Random.Range(0, 100) < wallPercent ? MoveDifficulty.NotWalkable : MoveDifficulty.Easy;
                }
            }
        }
    }
}
