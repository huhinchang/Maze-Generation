using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;


public class MapGenerator : MonoBehaviour {

    [SerializeField] private bool debugMazeGen = false;
    [SerializeField] private CinemachineVirtualCamera cam = null;
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private Vector2 playerDefaultPos;

    [Header("Tiles")]
    [SerializeField] private TileBase wallTile = null;
    [SerializeField] private TileBase floorTile = null;
    [SerializeField] private TileBase goalTile = null;

    [Header("Map")]
    [SerializeField] private Vector2Int mapSize;
    [SerializeField] private Tilemap baseTileMap = null;
    [SerializeField] private Tilemap goalTileMap = null;

    [System.Flags]
    enum Direction { N = 1, E = 2, S = 4, W = 8 };
    Direction[,] roomOpenings;

    Vector2Int[] DIR_TO_MOVE = new Vector2Int[9];
    Direction[] OPPOSITE = new Direction[9];

    void Start() {
        RegenerateMap();
    }

    public void SetMapSizeX(float n) {
        mapSize.x = (int) n;
    }
    public void SetMapSizeY(float n) {
        mapSize.y = (int) n;
    }
    [ContextMenu("Generate Map")]
    public void RegenerateMap() {
        GenerateMap();
        UpdateTilemap();
        playerTransform.position = playerDefaultPos;
        // make sure maze is always in view
        cam.transform.position = new Vector3(mapSize.x * 3 / 2, mapSize.y * 3 / 2, -10);
        float mapRatio = mapSize.x / mapSize.y;
        if (mapRatio > 1) {
            // map is wider than tall
            float screenRatio = cam.m_Lens.Aspect;
            cam.m_Lens.OrthographicSize = mapSize.x * screenRatio;
        } else {
            // map is taller than wide
            cam.m_Lens.OrthographicSize = mapSize.y * 2;
        }
    }
    public static void Shuffle<T>(T[] items) {
        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < items.Length - 1; i++) {
            int j = Random.Range(i, items.Length);
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
    }

    void GenerateMap() {
        //recursive backtracker 
        //http://weblog.jamisbuck.org/2010/12/27/maze-generation-recursive-backtracking
        //https://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/
        //initialize
        // storing it in arrays is faster than dictionary lookup probably
        OPPOSITE[(int)Direction.N] = Direction.S;
        OPPOSITE[(int)Direction.E] = Direction.W;
        OPPOSITE[(int)Direction.S] = Direction.N;
        OPPOSITE[(int)Direction.W] = Direction.E;

        DIR_TO_MOVE[(int)Direction.N] = Vector2Int.up;
        DIR_TO_MOVE[(int)Direction.E] = Vector2Int.right;
        DIR_TO_MOVE[(int)Direction.S] = Vector2Int.down;
        DIR_TO_MOVE[(int)Direction.W] = Vector2Int.left;

        roomOpenings = new Direction[mapSize.x, mapSize.y];

        //choose spawnRoom
        Vector2Int spawnRoom = new Vector2Int(0, 0);
        /*new Vector2Int {
            x = Random.Range(0, mapSize.x),
            y = Random.Range(0, mapSize.y),
        };
        */
        if (debugMazeGen) Debug.Log($"Spawn Room = {spawnRoom}");

        CarvePath(spawnRoom);
        if (debugMazeGen) PrintRooms(Vector2Int.zero);
    }

    void CarvePath(Vector2Int startPos) {
        //shuffle the direction array;
        Direction[] directions = { Direction.N, Direction.E, Direction.S, Direction.W };
        Shuffle(directions);

        // iterates through all directions then test if the cell in that direction is valid and
        // within the bounds of the maze
        foreach (Direction direction in directions) {
            //Debug.Log($"Checking{(Direction)direction}");
            Vector2Int delta = DIR_TO_MOVE[(int)direction];

            Vector2Int neighborPos = startPos + delta;
            //check if within grid
            if (neighborPos.x >= 0 && neighborPos.x < mapSize.x && neighborPos.y >= 0 && neighborPos.y < mapSize.y) {
                //check if unvisited grid
                if (roomOpenings[neighborPos.x, neighborPos.y] == 0) {
                    //Debug.Log($"Moving to {newPos}");
                    roomOpenings[startPos.x, startPos.y] |= direction;
                    roomOpenings[neighborPos.x, neighborPos.y] |= OPPOSITE[(int)direction];

                    if (debugMazeGen) PrintRooms(neighborPos);
                    CarvePath(neighborPos);
                    if (debugMazeGen) PrintRooms(startPos);
                }
            }
        }

    }
    void PrintRooms(Vector2Int currentRoom) {
        string stringToPrint = "";
        for (int y = mapSize.y - 1; y >= 0; y--)//reverse order
        {
            for (int x = 0; x < mapSize.x; x++) {
                //up connections
                if ((roomOpenings[x, y] & Direction.N) != 0) {
                    stringToPrint += "<color=white>─</color>│<color=white>─</color>";
                } else {
                    stringToPrint += "<color=white>─</color><color=white>─</color><color=white>─</color>";
                }
            }
            stringToPrint += "\n";
            for (int x = 0; x < mapSize.x; x++) {
                bool isLinkedEast = (roomOpenings[x, y] & Direction.E) != 0;
                bool isLinkedWest = (roomOpenings[x, y] & Direction.W) != 0;

                //horizontal connections
                if (isLinkedWest) {
                    stringToPrint += "─";
                } else {
                    stringToPrint += "<color=white>─</color>";
                }

                if (currentRoom.x == x && currentRoom.y == y) {

                    stringToPrint += "╬";
                } else {
                    stringToPrint += "┼";
                }

                if (isLinkedEast) {
                    stringToPrint += "─";
                } else {
                    stringToPrint += "<color=white>─</color>";
                }

            }
            stringToPrint += "\n";
            for (int x = 0; x < mapSize.x; x++) {
                //down connections
                if ((roomOpenings[x, y] & Direction.S) != 0) {
                    stringToPrint += "<color=white>─</color>│<color=white>─</color>";
                } else {
                    stringToPrint += "<color=white>─</color><color=white>─</color><color=white>─</color>";
                }
            }
            stringToPrint += "\n";
        }

        Debug.Log(stringToPrint);
    }

    private void UpdateTilemap() {
        baseTileMap.ClearAllTiles();
        goalTileMap.ClearAllTiles();
        goalTileMap.SetTile(new Vector3Int(mapSize.x * 3 - 2, mapSize.y * 3 - 2, 0), goalTile);
        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        baseTileMap.SetTile(new Vector3Int(x * 3 + i, y * 3 + j, 0), wallTile);
                    }
                }
                //old code for wire path
                Vector2Int roomCenter = new Vector2Int(x * 3 + 1, y * 3 + 1);
                baseTileMap.SetTile((Vector3Int)roomCenter, floorTile);


                Direction[] directions = { Direction.N, Direction.E, Direction.S, Direction.W };
                foreach (Direction direction in directions) {
                    //Debug.Log($"Checking{(Direction)direction}");

                    if (roomOpenings[x, y].HasFlag(direction)) {
                        Vector2Int delta = DIR_TO_MOVE[(int)direction];
                        baseTileMap.SetTile((Vector3Int)(roomCenter + delta), floorTile);
                    }
                }

            }
        }
    }
}
