using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Sumfulla.TankTankBoom
{
    // SOURRCE: Based on code found at https://blog.unity.com/technology/procedural-patterns-you-can-use-with-tilemaps-part-i
    public class TerrainGenerator
    {
        enum GenPhase { START_FLAT, PRE_PLAYER, PLAYER, NO_MANS_LAND, ENEMY, POST_ENEMY, END_FLAT }
        private const int UB_X = 0;
        private const int UB_Y = 1;
        private const int TERRAIN_START = 50;
        private const int PLAYER_PLATFORM_LENGTH = 30;
        private const int ENEMY_PLATFORM_LENGTH = 20;
        private const float VERTICAL_WIGGLE_ROOM = 5f;
        private const float SLOPE_BOTTOM_PERCENTAGE = 10f;
        private const int FLAT_HEIGHT = 3;
        public const float MIN_HEIGHT_FACTOR = 7f; //8.5f;
        public static PlatformPositions Platform;
        private static int _platformY = 0;

        public struct PlatformPositions
        {
            public Vector3 Player;
            public Vector3 Enemy;
            
        }

        /// <summary>
        /// Returns minimum platform level a tank can be positioned based on current vertical bandwidth
        /// </summary>
        private static int GetMinimumTankLevel(int yLimit)
        {
            return Mathf.RoundToInt(yLimit * 0.40f);
        }

        /// <summary>
        /// Returns a perlin noise based height level that takes in a slope factor to allow for incremental ascent/descent
        /// </summary>
        private static int GetPerlinNoiseSlope(int x, float seed, float reduction, float slopeFactor, float fullSectionAtBottom)
        {
            // Get pure perlin for this vertical column
            float floatPNPure = Mathf.PerlinNoise(x, seed * reduction);
            float floatPNFluct = floatPNPure - 0.5f;
            float slopeChange = Mathf.CeilToInt(x * slopeFactor);
            float floatPN = floatPNFluct * VERTICAL_WIGGLE_ROOM + fullSectionAtBottom + slopeChange;
            int intPN = Mathf.FloorToInt(floatPN);
            return intPN;
        }

        /// <summary>
        /// Returns a perlin noise based height level that keeps spread clamped between a certain limit
        /// </summary>
        private static int GetPerlinNoiseClampedHeight(int x, float seed, float reduction, float fullSectionAtBottom)
        {
            float floatPNPure = Mathf.PerlinNoise(x, seed * reduction);
            float floatPNFluct = floatPNPure - 0.5f;
            float floatPN = floatPNFluct * 30f + fullSectionAtBottom;
            int intPN = Mathf.FloorToInt(floatPN);
            return intPN;
        }

        /// <summary>
        /// Creates struct for platforms with default values
        /// </summary>
        private static PlatformPositions MakeNewPositionObject()
        {
            return new PlatformPositions { Player = Vector3.zero, Enemy = Vector3.zero };
        }

        /// <summary>
        /// Generates a surface with fine perlin noise values at the top but on a hill gradient the gradually rises/drops
        /// </summary>
        public static bool[,] RisingFinePerlin(bool[,] map, float seed, out PlatformPositions platforms, bool reversed = false)
        {
            // Get distance/height
            int upperX = map.GetLength(UB_X);
            int upperY = map.GetLength(UB_Y);

            // Postions
            platforms = MakeNewPositionObject();
            int pos_prePlayer = TERRAIN_START;
            int pos_player = TerrainController.GUAGE_MAX_LENGTH;
            int pos_noMansLand = pos_player + PLAYER_PLATFORM_LENGTH;
            int pos_enemy = upperX - ENEMY_PLATFORM_LENGTH - TerrainController.BUTTON_MAX_LENGTH_PLUS_BLEED;
            int pos_postEnemy = pos_enemy + ENEMY_PLATFORM_LENGTH;
            int pos_endFlat = pos_postEnemy + TERRAIN_START;
            int minTankLevel = GetMinimumTankLevel(upperY);

            // Next column variables
            int height;
            GenPhase phase = GenPhase.START_FLAT;
            System.Random rand = new System.Random(seed.GetHashCode());

            // Actual height of noise on the surface (+/- range)
            float perlinWiggleRoom = VERTICAL_WIGGLE_ROOM * 0.5f;
            float fullSectionAtBottom = SLOPE_BOTTOM_PERCENTAGE * 0.01f * upperY; 
            float slopeSectionOnTop = upperY - perlinWiggleRoom - (fullSectionAtBottom /2f);
            int fieldDistance = pos_postEnemy - pos_prePlayer;
            float slopeFactor = slopeSectionOnTop / fieldDistance;

            // Create columns based on perlin noise generated heights
            for (int x = 0; x < upperX; x++)
            {
                // Randomise perlin range
                float reduction = (float)rand.NextDouble();

                // Work out slope position with offsets
                int startOffset = x - pos_prePlayer;
                int directionStep = reversed ? fieldDistance - startOffset : startOffset;
                int MakeHeight() => Mathf.Clamp(GetPerlinNoiseSlope(directionStep, seed, reduction, slopeFactor, fullSectionAtBottom), 0, upperY);

                switch (phase)
                {
                    case GenPhase.START_FLAT:
                        if (x < pos_prePlayer) height = FLAT_HEIGHT;
                        else
                        {
                            phase = GenPhase.PRE_PLAYER;
                            height = MakeHeight();
                        }
                        break;
                    case GenPhase.PRE_PLAYER:
                        if (x < pos_player) height = MakeHeight();
                        else
                        {
                            phase = GenPhase.PLAYER;
                            _platformY = Mathf.Clamp(MakeHeight(), minTankLevel, upperY);
                            platforms.Player = new Vector2(pos_player + PLAYER_PLATFORM_LENGTH * 0.5f, _platformY);
                            height = _platformY;
                        }
                        break;
                    case GenPhase.PLAYER:
                        if (x < pos_noMansLand) height = _platformY;
                        else
                        {
                            phase = GenPhase.NO_MANS_LAND;
                            height = MakeHeight();
                        }
                        break;
                    case GenPhase.NO_MANS_LAND:
                        if (x < pos_enemy) height = MakeHeight();
                        else
                        {
                            phase = GenPhase.ENEMY;
                            _platformY = Mathf.Clamp(MakeHeight(), minTankLevel, upperY);
                            height = _platformY;
                            platforms.Enemy = new Vector2(pos_enemy + ENEMY_PLATFORM_LENGTH / 2, _platformY);
                        }
                        break;
                    case GenPhase.ENEMY:
                        if (x < pos_postEnemy) height = _platformY;
                        else
                        {
                            phase = GenPhase.POST_ENEMY;
                            height = MakeHeight();
                        }
                        break;
                    case GenPhase.POST_ENEMY:
                        if (x < pos_endFlat) height = MakeHeight();
                        else
                        {
                            phase = GenPhase.END_FLAT;
                            height = FLAT_HEIGHT;
                        }
                        break;
                    default:
                        height = FLAT_HEIGHT;
                        break;
                }

                // Fill in from upper limit down to bottom
                for (int y = height - 1; y >= 0; y--)
                {
                    map[x, y] = true;
                }
            }

            return map;
        }

        /// <summary>
        /// Generates a surface based on perlin noise values but with heavy sharp changes giving jaffed, badland feel
        /// </summary>
        public static bool[,] JaggedBluntPerlin(bool[,] map, float seed, out PlatformPositions platforms)
        {
            // Get distance/height
            int upperX = map.GetLength(UB_X);
            int upperY = map.GetLength(UB_Y);

            // Postions
            platforms = MakeNewPositionObject();
            int pos_prePlayer = TERRAIN_START;
            int pos_player = TerrainController.GUAGE_MAX_LENGTH;
            int pos_noMansLand = pos_player + PLAYER_PLATFORM_LENGTH;
            int pos_enemy = upperX - ENEMY_PLATFORM_LENGTH - TerrainController.BUTTON_MAX_LENGTH_PLUS_BLEED;
            int pos_postEnemy = pos_enemy + ENEMY_PLATFORM_LENGTH;
            int pos_endFlat = pos_postEnemy + TERRAIN_START;
            int minTankLevel = GetMinimumTankLevel(upperY);

            // Next column variables
            GenPhase phase = GenPhase.START_FLAT;
            System.Random rand = new System.Random(seed.GetHashCode());
            
            // Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            // The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //int MakeHeight(int x) => Mathf.FloorToInt(Mathf.PerlinNoise(x, seed * reduction) * upperY);
            float fullSectionAtBottom = 30 * 0.01f * upperY;
            int MakeHeight(int x) => GetPerlinNoiseClampedHeight(x, seed, reduction, fullSectionAtBottom);
            int MakeRandomInterval() => rand.Next(3, 15);


            int interval = MakeRandomInterval();
            // Generate the noise
            for (int x = 0; x < upperX; x += interval)
            {
                int nextHeight;

                switch (phase)
                {
                    case GenPhase.START_FLAT:
                        if (x < pos_prePlayer) nextHeight = FLAT_HEIGHT;
                        else
                        {
                            phase = GenPhase.PRE_PLAYER;
                            nextHeight = MakeHeight(x);
                        }
                        break;
                    case GenPhase.PRE_PLAYER:
                        if (x < pos_player)
                        {
                            nextHeight = MakeHeight(x);
                            interval = MakeRandomInterval();
                        }
                        else
                        {
                            phase = GenPhase.PLAYER;
                            _platformY = Mathf.Clamp(MakeHeight(x), minTankLevel, upperY);
                            platforms.Player = new Vector2(pos_player + PLAYER_PLATFORM_LENGTH * 0.5f, _platformY);
                            nextHeight = _platformY;
                        }
                        break;
                    case GenPhase.PLAYER:
                        if (x < pos_noMansLand) { nextHeight = _platformY; }
                        else
                        {
                            phase = GenPhase.NO_MANS_LAND;
                            nextHeight = MakeHeight(x);
                        }
                        break;
                    case GenPhase.NO_MANS_LAND:
                        if (x < pos_enemy)
                        {
                            nextHeight = MakeHeight(x);
                            interval = MakeRandomInterval();
                        }
                        else
                        {
                            phase = GenPhase.ENEMY;
                            _platformY = Mathf.Clamp(MakeHeight(x), minTankLevel, upperY);
                            nextHeight = _platformY;
                            platforms.Enemy = new Vector2(pos_enemy + ENEMY_PLATFORM_LENGTH / 2, _platformY);
                        }
                        break;
                    case GenPhase.ENEMY:
                        if (x < pos_postEnemy) nextHeight = _platformY;
                        else
                        {
                            phase = GenPhase.POST_ENEMY;
                            nextHeight = MakeHeight(x);
                        }
                        break;
                    case GenPhase.POST_ENEMY:
                        if (x < pos_endFlat)
                        {
                            nextHeight = MakeHeight(x);
                            interval = MakeRandomInterval();
                        }
                        else
                        {
                            phase = GenPhase.END_FLAT;
                            nextHeight = FLAT_HEIGHT;
                        }
                        break;
                    default:
                        nextHeight = FLAT_HEIGHT;
                        break;
                }

                noiseX.Add(x);
                noiseY.Add(nextHeight);
            }

            // Smoothing process - start at 1 so we have a previous position already
            for (int i = 0; i < noiseY.Count; i++)
            {
                // Get comparable positions, and there difference
                Vector2Int current = new Vector2Int(noiseX[i], noiseY[i]);
                Vector2Int previous = i == 0 ? Vector2Int.zero : new Vector2Int(noiseX[i - 1], noiseY[i - 1]);
                Vector2 diff = current - previous;

                // Set up what the height change value will be
                float heightChange = diff.y / interval;

                // Determine the current height
                float height = previous.y;

                // Work our way through from the last x to the current x
                for (int x = previous.x; x < current.x; x++)
                {
                    for (int y = Mathf.FloorToInt(height); y >= 0; y--)
                    {
                        try { map[x, y] = true; }
                        catch (Exception e) { GameLog.Shout($"{x}, {y}  | {e.Message} "); }
                    }
                    height += heightChange;
                }
            }

            return map;
        }

        /// <summary>
        /// Randomly selects direction, avoids top/bottom limits
        /// </summary>
        private static int GetIncermentedAdjustedRandomWalkHeight(System.Random rand, int lastHeight, int limit)
        {
            // Flip a coin
            int nextMove = rand.Next(2);

            // If heads, and we aren't near the bottom, minus some height
            if (nextMove == 0 && lastHeight > 3)
            {
                return --lastHeight;
            }
            // If tails, and we aren't near the top, add some height
            else if (nextMove == 1 && lastHeight < limit - 3)
            {
                return ++lastHeight;
            }
            else
            {
                return lastHeight;
            }
        }

        /// <summary>
        /// Randomly and roughly draws up and down
        /// </summary>
        public static bool[,] RandomWalkTop(bool[,] map, float seed, out PlatformPositions platforms)
        {
            // Get distance/height
            int upperX = map.GetUpperBound(UB_X);
            int upperY = map.GetUpperBound(UB_Y);

            // Postions
            platforms = MakeNewPositionObject();
            int pos_prePlayer = TERRAIN_START;
            int pos_player = TerrainController.GUAGE_MAX_LENGTH;
            int pos_noMansLand = pos_player + PLAYER_PLATFORM_LENGTH;
            int pos_enemy = upperX - ENEMY_PLATFORM_LENGTH - TerrainController.BUTTON_MAX_LENGTH_PLUS_BLEED;
            int pos_postEnemy = pos_enemy + ENEMY_PLATFORM_LENGTH;
            int pos_endFlat = pos_postEnemy + TERRAIN_START;
            int minTankLevel = GetMinimumTankLevel(upperY);

            // Next column variables
            GenPhase phase = GenPhase.START_FLAT;
            System.Random rand = new System.Random(seed.GetHashCode());

            //Set our starting height
            int height = UnityEngine.Random.Range(0,upperY);

            // Preloaded height method
            int MakeHeight(int height) => GetIncermentedAdjustedRandomWalkHeight(rand, height, upperY);

            // Create columns based on perlin noise generated heights
            for (int x = 0; x < upperX; x++)
            {
                // 50/50
                int nextMove = rand.Next(2);

                // If near bottom, minus some height
                if (nextMove == 0 && height > 2)
                {
                    height--;
                }
                // If near top, add some height
                else if (nextMove == 1 && height < upperY - 2)
                {
                    height++;
                }

                switch (phase)
                {
                    case GenPhase.START_FLAT:
                        if (x < pos_prePlayer) height = FLAT_HEIGHT;
                        else
                        {
                            phase = GenPhase.PRE_PLAYER;
                            int initialHeight = UnityEngine.Random.Range(0, upperY);
                            height = MakeHeight(initialHeight);
                        }
                        break;
                    case GenPhase.PRE_PLAYER:
                        if (x < pos_player) height = MakeHeight(height);
                        else
                        {
                            phase = GenPhase.PLAYER;
                            _platformY = Mathf.Clamp(MakeHeight(height), minTankLevel, upperY);
                            platforms.Player = new Vector2(pos_player + PLAYER_PLATFORM_LENGTH * 0.5f, _platformY);
                            height = _platformY;
                        }
                        break;
                    case GenPhase.PLAYER:
                        if (x < pos_noMansLand) height = _platformY;
                        else
                        {
                            phase = GenPhase.NO_MANS_LAND;
                            height = MakeHeight(height);
                        }
                        break;
                    case GenPhase.NO_MANS_LAND:
                        if (x < pos_enemy) height = MakeHeight(height);
                        else
                        {
                            phase = GenPhase.ENEMY;
                            _platformY = Mathf.Clamp(MakeHeight(height), minTankLevel, upperY);
                            height = _platformY;
                            platforms.Enemy = new Vector2(pos_enemy + ENEMY_PLATFORM_LENGTH / 2, _platformY);
                        }
                        break;
                    case GenPhase.ENEMY:
                        if (x < pos_postEnemy) height = _platformY;
                        else
                        {
                            phase = GenPhase.POST_ENEMY;
                            height = MakeHeight(height);
                        }
                        break;
                    case GenPhase.POST_ENEMY:
                        if (x < pos_endFlat) height = MakeHeight(height);
                        else
                        {
                            phase = GenPhase.END_FLAT;
                            height = FLAT_HEIGHT;
                        }
                        break;
                    default:
                        height = FLAT_HEIGHT;
                        break;
                }

                // Fill in from upper limit down to bottom
                for (int y = height; y >= 0; y--)
                {
                    map[x, y] = true;
                }
            }

            //Return the map
            return map;
        }
    }
}