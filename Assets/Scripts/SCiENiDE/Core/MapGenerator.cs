using Assets.Scripts.SCiENiDE.Core;
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
        private int _fillPercent;

        private RectInt[] _rooms;
        private Action<BaseGrid<MapNode>, TextMesh[,]> _debug;
        private BaseGrid<MapNode> _map;

        public MapGenerator(
            int width,
            int height,
            bool useRandomSeed = true,
            int seed = -1,
            Action<BaseGrid<MapNode>, TextMesh[,]> showDebugFunc = null,
            int fillPercent = 43)
        {
            _width = width;
            _height = height;
            _totalNodeCount = _width * _height;
            _debug = showDebugFunc;
            _seed = seed;
            _useRandomSeed = useRandomSeed;
            _fillPercent = fillPercent;

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

            int minRoomsRange = 8;
            int maxRoomsRange = 20;
            _rooms = new RectInt[Random.Range(minRoomsRange, maxRoomsRange)];

            _map = new BaseGrid<MapNode>(
                _width, _height, 3.5f, new Vector3(-110f, -60f),
                (grid, x, y) =>
                {
                    return new MapNode(grid, x, y, MoveDifficulty.NotWalkable);
                },
                _debug);
        }

        public BaseGrid<MapNode> GenerateMap(MapType mapType, int smoothing)
        {
            switch (mapType)
            {
                case MapType.Rooms:
                    {
                        GenerateRooms();
                        for (int i = 0; i < smoothing; i++)
                            _map.RunCARuleset(mapType);
                    }
                    break;

                case MapType.RandomFill:
                    {
                        RandomFillMap(_fillPercent);
                        for (int i = 0; i < smoothing; i++)
                            _map.RunCARuleset(mapType);

                    }
                    break;

                case MapType.SolidFill:
                    {
                    }
                    break;

                default:
                    break;
            }

            ProcessMap();

            return _map;
        }
        private void ProcessMap()
        {
            Dictionary<MoveDifficulty, List<Room>> roomRegions = GetMapRegions();
            Dictionary<MoveDifficulty, List<Room>> survivingRoomRegions = new Dictionary<MoveDifficulty, List<Room>>();
            foreach (MoveDifficulty moveDifficultyKey in roomRegions.Keys)
            {
                if (moveDifficultyKey == MoveDifficulty.NotWalkable) continue;

                foreach (Room room in roomRegions[moveDifficultyKey])
                {
                    if (room.Size < 6)
                    {
                        foreach (var node in room.Tiles)
                        {
                            node.Terrain.Difficulty = MoveDifficulty.NotWalkable;
                        }
                    }
                    else
                    {
                        if (!survivingRoomRegions.ContainsKey(moveDifficultyKey))
                        {
                            survivingRoomRegions[moveDifficultyKey] = new List<Room> { room };
                        }
                        else
                        {
                            survivingRoomRegions[moveDifficultyKey].Add(room);
                        }
                    }
                }
            }

            _map.TriggetAllGridCellsChanged();

            List<Room> flatRoomList = survivingRoomRegions
                .Select(x => x.Value)
                .SelectMany(x => x).ToList();
            flatRoomList.Sort();

            flatRoomList[0].IsMainRoom = true;
            flatRoomList[0].IsAccesibleFromMainRoom = true;

            ConnectRooms(flatRoomList);
        }
        private void ConnectRooms(List<Room> rooms, bool forceConnect = false)
        {
            if (rooms.Count <= 1)
            {
                return;
            }

            List<Room> roomListA = new List<Room>();
            List<Room> roomListB = new List<Room>();

            if (forceConnect)
            {
                foreach (Room room in rooms)
                {
                    if (room.IsAccesibleFromMainRoom)
                    {
                        roomListB.Add(room);
                    }
                    else
                    {
                        roomListA.Add(room);
                    }
                }
            }
            else
            {
                roomListA = rooms;
                roomListB = rooms;
            }

            int bestDistance = int.MaxValue;
            Room bestRoomA = null;
            Room bestRoomB = null;
            MapNode bestNodeA = null;
            MapNode bestNodeB = null;
            bool matchFound = false;
            foreach (Room roomA in roomListA)
            {
                if (!forceConnect)
                {
                    matchFound = false;
                    if (roomA.NeighbourRooms.Count > 0)
                    {
                        continue;
                    }
                }
                foreach (Room roomB in roomListB)
                {
                    if (roomA == roomB || roomA.IsConnectedTo(roomB))
                    {
                        continue;
                    }

                    for (int wallTileA = 0; wallTileA < roomA.EdgeTiles.Count; wallTileA++)
                    {
                        for (int wallTileB = 0; wallTileB < roomB.EdgeTiles.Count; wallTileB++)
                        {
                            int distanceBetweenRooms = (int)(Mathf.Pow(roomA.EdgeTiles[wallTileA].x - roomB.EdgeTiles[wallTileB].x, 2) +
                                Mathf.Pow(roomA.EdgeTiles[wallTileA].y - roomB.EdgeTiles[wallTileB].y, 2));

                            if (distanceBetweenRooms < bestDistance || !matchFound)
                            {
                                matchFound = true;
                                bestDistance = distanceBetweenRooms;
                                bestNodeA = roomA.EdgeTiles[wallTileA];
                                bestNodeB = roomB.EdgeTiles[wallTileB];
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }

                if (matchFound && !forceConnect)
                {
                    CreatePassage(bestRoomA, bestRoomB, bestNodeA, bestNodeB);
                }
            }

            if (matchFound && forceConnect)
            {
                CreatePassage(bestRoomA, bestRoomB, bestNodeA, bestNodeB);
                ConnectRooms(rooms, true);
            }
            if (!forceConnect)
            {
                ConnectRooms(rooms, true);
            }
        }
        private void CreatePassage(Room a, Room b, MapNode nodeA, MapNode nodeB)
        {
            Debug.Log($"Connecting room nodes [{nodeA}] => [{nodeB}]");
            Room.ConnectRooms(a, b);
            Debug.DrawLine(_map.GetWorldPosition(nodeA.x, nodeA.y, true), _map.GetWorldPosition(nodeB.x, nodeB.y, true), Color.red, 100);
        }
        private Vector3 CoordToWorldPoint(int x, int y)
        {
            return new Vector3(-_width / 2 + .5f + x, 2, -_height / 2 + .5f + y);
        }
        private Dictionary<MoveDifficulty, List<Room>> GetMapRegions()
        {
            Dictionary<MoveDifficulty, List<Room>> roomRegions = new Dictionary<MoveDifficulty, List<Room>>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    MapNode currentNode = _map.GetGridCell(x, y);
                    if (currentNode == null) continue;

                    MoveDifficulty currentTerrain = currentNode.Terrain.Difficulty;

                    if (!roomRegions.ContainsKey(currentTerrain)
                        || !roomRegions[currentTerrain].Any(r => r.Tiles.Contains(currentNode)))
                    {
                        List<MapNode> currentRegion = GetRegionTiles(x, y);
                        Room room = new Room(currentRegion, _map);
                        if (roomRegions.ContainsKey(currentTerrain))
                        {
                            roomRegions[currentTerrain].Add(room);
                        }
                        else
                        {
                            roomRegions[currentTerrain] = new List<Room> { room };
                        }
                    }
                }
            }

            return roomRegions;
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
        private void RandomFillMap(int wallPercent = 43)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _map[x, y].Terrain.Difficulty = Random.Range(0, 100) < wallPercent ? MoveDifficulty.NotWalkable : MoveDifficulty.Easy;
                }
            }
        }
        private List<MapNode> GetRegionTiles(int x, int y)
        {
            MapNode startNode = _map.GetGridCell(x, y);
            if (startNode == null)
            {
                return null;
            }

            Queue<MapNode> open = new Queue<MapNode>();
            List<MapNode> closed = new List<MapNode>();

            open.Enqueue(startNode);

            while (open.Count > 0)
            {
                MapNode current = open.Dequeue();
                closed.Add(current);
                foreach (MapNode neighbour in current.NeighbourNodes.Where(n => n.Terrain.Difficulty == startNode.Terrain.Difficulty))
                {
                    if (!closed.Contains(neighbour) && !open.Contains(neighbour))
                    {
                        open.Enqueue(neighbour);
                    }
                }
            }

            return closed;
        }
        private bool IsInMapRange(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }
    }
}
