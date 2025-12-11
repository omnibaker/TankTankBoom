using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sumfulla.TankTankBoom
{

    public class TerrainController : MonoBehaviour
    {
        public const int GUAGE_MAX_LENGTH = 100;
        public const int BUTTON_MAX_LENGTH_PLUS_BLEED = 250;
        public const int BOTTOM_UI_HEIGHT = 60;
        private const int DEF_RADIUS = 2;
        private const int BATTLEFIELD_WIDTH = 200;

        [Header("CREATION VALUES")]
        [SerializeField] private int _interval = 3;

        [Header("TILE COMPONENTS")]
        [SerializeField] private TileTerrain _fixedTileTerrain = TileTerrain.Random;
        [SerializeField] private TileBase _grassTile = null;
        [SerializeField] private TileBase _groundTile = null;
        [SerializeField] private Grid _grid = null;
        [SerializeField] private Tilemap _tilemap = null;

        public TankPositionsInWorld TankPositions { get; private set; }
        public static TerrainController Instance { get; set; }
        public bool TerrainReady { get; set; }

        private List<List<Vector3Int>> _hitColumns = new List<List<Vector3Int>>();
        private TileTerrain _currentTileTerrain = TileTerrain.Random;
        private TerrainGenerator.PlatformPositions _platforms;
        private TerrainProfile _terrain;
        private bool _alternatingExplosion;
        private bool[,] _mappedArray;
        private float _gridCellSize;

        private void Awake()
        {
            Instance = this;
            CheckForScriptableObjects();
        }

        private void Start()
        {
            _gridCellSize = _grid.cellSize.x;
            _terrain = GetTerrainProfile();
        }

        /// <summary>
        /// If ground/grass rule tiles hav enot been added yet, getch from assets
        /// </summary>
        private void CheckForScriptableObjects()
        {
            _groundTile = _groundTile == null
                ? Resources.Load<TileBase>(GameRef.Files.GAMESO_GROUND_TILE)
                : _groundTile;


            _grassTile = _grassTile == null
                ? Resources.Load<TileBase>(GameRef.Files.GAMESO_GRASS_TILE)
                : _grassTile;

            if (_groundTile == null || _grassTile == null)
            {
                GameLog.Warn($"Tile asset missing: Ground={_groundTile != null} Grass={_grassTile != null}");
            }
        }

        /// <summary> 
        /// Create list and updates with hardcoded values for camera offsets
        /// </summary> 
        private TerrainProfile GetTerrainProfile()
        {
            int widthInUnits = GUAGE_MAX_LENGTH + BATTLEFIELD_WIDTH + BUTTON_MAX_LENGTH_PLUS_BLEED;
            float cameraCentred = (GUAGE_MAX_LENGTH + BATTLEFIELD_WIDTH + GUAGE_MAX_LENGTH) / 2f;
            return new TerrainProfile(
                widthInUnits,       // Width
                BOTTOM_UI_HEIGHT,   // Height 
                cameraCentred,      // X Cam offset
                0);                 // Y Cam offset
        }

        /// <summary>
        /// Sets up new terrain generation when another play round commences
        /// </summary>
        public void CreateNewBattlefieldTerrain()
        {
            TerrainReady = false;
            GenerateTerrain();
            UpdateCameraOrientation();
            UpdateCameraPosition();
            UpdateMovePoints();
            TerrainReady = true;
        }

        /// <summary>
        /// Starts the process of programmatically generating terrain
        /// </summary>
        private void GenerateTerrain()
        {
            // TODO: Make random 
            _currentTileTerrain = GetTerrain(_fixedTileTerrain); 
            
            // Get new array with latest dimensions
            _mappedArray = CreateTerrain(
                _currentTileTerrain,
                GenerateCleanArray(_terrain.Width, _terrain.Height),
                GetSeed(_currentTileTerrain),
                _interval);

            // Render surface tiles using previously created map
            RenderMap(_mappedArray, _tilemap, _groundTile, _grassTile);
        }

        /// <summary>
        /// Checks if random selected, and if so returns a random type
        /// </summary>
        private TileTerrain GetTerrain(TileTerrain terrain)
        {
            if (terrain != TileTerrain.Random) return terrain;

            int index = Random.Range(1, System.Enum.GetNames(typeof(TileTerrain)).Length);

            return (TileTerrain)index;
        }

        /// <summary>
        /// Returns appropriate seed value for generation noise
        /// </summary>
        private float GetSeed(TileTerrain terrain)
        {
            switch (terrain)
            {
                case TileTerrain.HillBattle: return Random.Range(0.1f, 1f);
                case TileTerrain.JaggedRocks: return 0.5f;
                case TileTerrain.Roughlands: return Random.Range(0.1f, 1f);
                default: return Random.Range(0.1f, 1f);
            }
        }

        /// <summary>
        /// Repositions camera so tilegroup is centered at bottom of screen
        /// </summary>
        private void UpdateCameraPosition()
        {
            // Reset camera so it can be readjusted;
            Camera.main.transform.position = new Vector3(0, 0, -10f);

            Vector3 _bottomOfScreen = Camera.main.ScreenToWorldPoint(Vector3.zero);

            Camera.main.transform.position = new Vector3(
                _terrain.CamXOffset * _gridCellSize,
                _bottomOfScreen.y * -1f,
                Camera.main.transform.position.z);
        }

        /// <summary>
        /// Adjusts camera position to accomodate current CamOffsetter profile
        /// </summary>
        private void UpdateCameraOrientation()
        {
            float distanceBetweenTanks = _platforms.Enemy.x - _platforms.Player.x;
            float ratio = (float)Screen.width / Screen.height;
            float revRatio = 1f / ratio;
            Camera.main.orthographicSize = (int)(revRatio * distanceBetweenTanks / 15f); 
        }

        /// <summary>
        /// Adjusts player move positions
        /// </summary>
        private void UpdateMovePoints()
        {
            Vector3 player = _tilemap.CellToWorld(Vector3Int.RoundToInt(_platforms.Player));
            Vector3 enemy = _tilemap.CellToWorld(Vector3Int.RoundToInt(_platforms.Enemy));
            GameLog.Say($"PLAYER: {player}");
            GameLog.Say($"ENEMY: {enemy}");
            TankPositions = new TankPositionsInWorld(player, enemy);
        }

        /// <summary>
        /// Creates a terrain based on TileTerrain type and profile-based parameters
        /// </summary>
        private bool[,] CreateTerrain(TileTerrain type, bool[,] map, float seed, int variable)
        {
            _platforms = new TerrainGenerator.PlatformPositions();
            GameLog.Say($"LowerCutoff = {Mathf.RoundToInt(map.GetLength(1) * 0.6f)} | [{map.GetLength(0)} x {map.GetLength(1)}] | Type: {type} | seed={seed} | variable={variable}");
            switch (type)
            {
                case TileTerrain.HillBattle: return TerrainGenerator.RisingFinePerlin(map, seed, out _platforms, false);
                case TileTerrain.JaggedRocks: return TerrainGenerator.JaggedBluntPerlin(map, seed, out _platforms);
                case TileTerrain.Roughlands: return TerrainGenerator.RandomWalkTop(map, seed, out _platforms);
                default: return null;
            }
        }

        /// <summary>
        /// Creates and returns an array with all possible coordinates false
        /// </summary>
        public static bool[,] GenerateCleanArray(int width, int height)
        {
            bool[,] map = new bool[width, height];
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    map[x, y] = false;
                }
            }
            return map;
        }

        /// <summary>
        /// Goes through map array and if cooridnate value is true, fills in with tile
        /// </summary>
        public static void RenderMap(bool[,] map, Tilemap tilemap, TileBase groundTile, TileBase grassTile, float waitTime = 0.0f)
        {
            // Clear the map (ensures we dont overlap)
            tilemap.ClearAllTiles();
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            List<Vector3Int> positions = new List<Vector3Int>();
            List<TileBase> tiles = new List<TileBase>();

            // Loop through the width of the map
            for (int x = 0; x < width; x++)
            {
                bool grassMarked = false;

                // Loop through the height of the map
                for (int y = height - 1; y >= 0; y--)
                {
                    if (!map[x, y]) continue;

                    positions.Add(new Vector3Int(x, y, 0));
                    tiles.Add(grassMarked ? groundTile : grassTile);
                    grassMarked = true;
                }
            }

            tilemap.SetTiles(positions.ToArray(), tiles.ToArray());
        }

        /// <summary>
        /// //Takes in our map and tilemap, setting null tiles where needed
        /// </summary>
        public static void UpdateMap(bool[,] map, Tilemap tilemap)
        {
            for (int x = 0; x < map.GetUpperBound(0); x++)
            {
                for (int y = 0; y < map.GetUpperBound(1); y++)
                {
                    if (!map[x, y])
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }
        }

        /// <summary>
        /// Removes tiles in the surrounding radius of a given position
        /// </summary>
        public void DestroyTerrainSet(Vector3 explosionLocation, int radius = 0)
        {
            radius = radius == 0 ? DEF_RADIUS : radius;
            bool hitSomething = false;

            // Clear any previous destroyed references
            _hitColumns.Clear();
            for (int x = -radius; x <= radius; x++)
            {
                // Clear any previous destroyed references
                List<Vector3Int> destroyedTiles = new List<Vector3Int>();
                for (int y = -radius; y <= radius; y++)
                {
                    // Ignore far corner tiles to give explosion carveout a more curved feel
                    if (Mathf.Abs(x) != radius || Mathf.Abs(y) != radius) 
                    {

                        Vector3Int tilePos = _tilemap.WorldToCell(explosionLocation + new Vector3(x * _gridCellSize, y * _gridCellSize, 0));
                        if (_tilemap.GetTile(tilePos) != null)
                        {
                            destroyedTiles.Add(tilePos);
                            DestroyTile(tilePos);
                        }
                    }

                    if (destroyedTiles.Count > 0)
                    {
                        hitSomething = true;
                        _hitColumns.Add(destroyedTiles);
                    }
                }
            }

            if (hitSomething)
            {
                //Trigger ground explosion sound
                if (_alternatingExplosion)
                {
                    GameAudio.I.Play(SoundType.GroundExplode01);
                }
                else
                {
                    GameAudio.I.Play(SoundType.GroundExplode02);
                }
                _alternatingExplosion = !_alternatingExplosion;

                PlayManager.I.Score.AddPoints(GameRef.Points.DIRT_HIT);
                // ArtilleryManager.I.AddGamePlayPoints(ArtilleryEnemies.POINT_DIRT_HIT);

                //GameLog.Warn("TODO[TerrainController]: Make points appear for terrain hit");
                // ArtilleryManager.I.Points.CreateTextObject(ArtilleryEnemies.POINT_DIRT_HIT, explosionLocation, Color.yellow);
            }

            foreach (List<Vector3Int> v3int in _hitColumns)
            {
                if (v3int.Count > 0)
                {
                    DropAnyLooseTiles(v3int, v3int[0].x);
                }
            }
        }

        /// <summary>
        /// Any loose tiles (not directly connected ot main body) drop vertically to the closest tile on the main body
        /// </summary>
        private void DropAnyLooseTiles(List<Vector3Int> destroyedTileY, int x)
        {
            int lowestDestroyed = destroyedTileY.First().y;
            int highestDestroyed = destroyedTileY.Last().y;
            int blocksDestroyed = highestDestroyed - lowestDestroyed + 1;

            for (int y = lowestDestroyed; y < _terrain.Height; y++)
            {
                // Handle coordinates checked outside array limits first
                if (y + blocksDestroyed >= _terrain.Height)
                {
                    _tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    _mappedArray[x, y] = false;
                }
                else if (_mappedArray[x, y + blocksDestroyed])
                {
                    _tilemap.SetTile(new Vector3Int(x, y, 0), _groundTile);
                    _mappedArray[x, y] = true;
                }
                else
                {
                    _tilemap.SetTile(new Vector3Int(x, y, 0), null);
                    _mappedArray[x, y] = false;
                }
            }
        }

        /// <summary>
        /// Removes a tile from a given coordinate
        /// </summary>
        private void DestroyTile(Vector3Int tilePos)
        {
            _tilemap.SetTile(tilePos, null);
            _mappedArray[tilePos.x, tilePos.y] = false;
        }
    }

    public struct TerrainProfile
    {
        public int Height;
        public int Width;
        public float CamXOffset;
        public float CamYOFfset;

        public TerrainProfile(int width, int height, float offSetX, float offsetY)
        {
            Width = width;
            Height = height;
            CamXOffset = offSetX;
            CamYOFfset = offsetY;
        }
    }

}