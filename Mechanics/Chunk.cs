﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using craftinggame.Graphics;
using OpenTK;

namespace craftinggame.Mechanics
{
    class Chunk
    {
        public static Random rand = new Random();
        public static OpenSimplexNoise noise = new OpenSimplexNoise(rand.Next());
        public static OpenSimplexNoise biomenoise = new OpenSimplexNoise(rand.Next());
        public static OpenSimplexNoise oceanfloornoise = new OpenSimplexNoise(rand.Next());

        public bool decorated = false;

        public Chunk((int x, int z) pos)
        {
            position = pos;
        }

        public void DecorateChunk()
        {
            decorated = true;
        }

        public void GenChunk()
        {
            blocks = new byte[16, 256, 16];
            Array.Clear(blocks, 0, blocks.Length);
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    int high;
                    double noisenum = noise.Evaluate((x + position.x * 16) / 300f, (z + position.z * 16) / 300f);
                    if (noisenum < 0.4 && noisenum > -0.3)
                    {
                        high = CalculateForestNoise(x + position.x * 16, z + position.z * 16);
                        if (noisenum > 0.3)
                        {
                            double delta = (0.4 - noisenum) * 5;
                            high = (int)((0.5 + delta) * high +  (0.5 - delta) * CalculateDesertNoise(x + position.x * 16, z + position.z * 16));
                        }
                        if (noisenum < -0.2)
                        {
                            double delta = Math.Abs(-0.3 - noisenum) * 10;
                            high = (int)((delta) * high + (1 - delta) * CalculateWaterNoise(x + position.x * 16, z + position.z * 16)) + 1;
                        }
                        if (rand.Next(0, 10) > 8)
                        {
                            blocks[x, high, z] = Block.GRASS;
                        }
                        else if (rand.Next(0, 40) > 38 && x > 4 && x < 12 && z > 4 && z < 12)
                        {
                            //Main trunk
                            blocks[x, high, z] = Block.OAK_LOG;
                            blocks[x, high + 1, z] = Block.OAK_LOG;
                            blocks[x, high + 2, z] = Block.OAK_LOG;
                            blocks[x, high + 3, z] = Block.OAK_LOG;
                            blocks[x, high + 4, z] = Block.OAK_LOG;
                            blocks[x, high + 5, z] = Block.OAK_LOG;
                            blocks[x, high + 6, z] = Block.OAK_LOG;
                            blocks[x, high + 7, z] = Block.OAK_LOG;
                            blocks[x, high + 8, z] = Block.OAK_LOG;
                            blocks[x, high + 9, z] = Block.OAK_LOG;
                            //+X trunk
                            blocks[x + 1, high + 6, z] = Block.OAK_LOG;
                            blocks[x + 2, high + 6, z] = Block.OAK_LOG;
                            blocks[x + 3, high + 6, z] = Block.OAK_LOG;
                            blocks[x + 3, high + 7, z] = Block.OAK_LOG;
                            blocks[x + 3, high + 8, z] = Block.OAK_LOG;
                            //-X trunk
                            blocks[x - 1, high + 6, z] = Block.OAK_LOG;
                            blocks[x - 1, high + 7, z] = Block.OAK_LOG;
                            //+Z trunk
                            blocks[x, high + 7, z + 1] = Block.OAK_LOG;
                            blocks[x, high + 7, z + 2] = Block.OAK_LOG;
                            blocks[x, high + 8, z + 2] = Block.OAK_LOG;
                            //-Z trunk
                            blocks[x, high + 7, z - 1] = Block.OAK_LOG;
                            blocks[x, high + 7, z - 2] = Block.OAK_LOG;

                            for (int lx = x - 4; lx <= x + 4; lx++)
                            {
                                for (int ly = high + 4; ly <= high + 10; ly++)
                                {
                                    for (int lz = z - 3; lz <= z + 3; lz++)
                                    {
                                        if (blocks[lx, ly, lz] == Block.AIR &&
                                           ((lx > 0 && blocks[lx - 1, ly, lz] == Block.OAK_LOG) ||
                                           (lx < 15 && blocks[lx + 1, ly, lz] == Block.OAK_LOG) ||
                                           (ly > 0 && blocks[lx, ly - 1, lz] == Block.OAK_LOG) ||
                                           (ly < 255 && blocks[lx, ly + 1, lz] == Block.OAK_LOG) ||
                                           (lz > 0 && blocks[lx, ly, lz - 1] == Block.OAK_LOG) ||
                                           (lz < 15 && blocks[lx, ly, lz + 1] == Block.OAK_LOG) ||
                                           (lx > 0 && lz > 0 && blocks[lx - 1, ly, lz - 1] == Block.OAK_LOG) ||
                                           (lx < 15 && lz < 15 && blocks[lx + 1, ly, lz + 1] == Block.OAK_LOG) ||
                                           (lz > 0 && lx < 15 && blocks[lx + 1, ly, lz - 1] == Block.OAK_LOG) ||
                                           (lz < 15 && lz > 0 && blocks[lx - 1, ly, lz + 1] == Block.OAK_LOG))) {
                                            blocks[lx, ly, lz] = Block.BIRCH_LEAVES;
                                        }
                                    }
                                }
                            }
                        }
                        for (int y = 0; y < high; y++)
                        {
                            byte value = Block.GRASS_BLOCK;
                            if (y < high - 1)
                            {
                                value = Block.DIRT;
                            }
                            blocks[x, y, z] = value;
                        }
                    }
                    else if (noisenum < -0.3 || noisenum > 0.8)
                    {
                        high = CalculateWaterNoise(x + position.x * 16, z + position.z * 16);
                        bool blocktype = CalculateWaterFloorBlockNoise(x + position.x * 16, z + position.z * 16);
                        int low = CalculateWaterFloorNoise(x + position.x * 16, z + position.z * 16);
                        if (noisenum > -0.4 && noisenum < -0.3)
                        {
                            double delta = Math.Abs(noisenum + 0.3) * 10;
                            low = (int)((delta) * low + (1 - delta) * CalculateForestNoise(x + position.x * 16, z + position.z * 16));
                        }
                        for (int y = 0; y < high; y++)
                        {
                            byte value = Block.WATER;
                            
                            if (y < low)
                            {
                                if (blocktype)
                                {
                                    value = Block.STONE;
                                } else
                                {
                                    value = Block.DIRT;
                                }
                            }
                            blocks[x, y, z] = value;
                        }
                    }
                    else
                    {
                        high = CalculateDesertNoise(x + position.x * 16, z + position.z * 16);
                        if (noisenum < 0.5)
                        {
                            double delta = (noisenum - 0.4) * 5;
                            high = (int)((0.5 + delta) * high + (0.5 - delta) * CalculateForestNoise(x + position.x * 16, z + position.z * 16));
                        }
                        if (rand.Next(0, 80) > 78)
                        {
                            blocks[x, high, z] = Block.DEAD_BUSH;
                        }
                        else if (rand.Next(0, 1000) > 998)
                        {
                            blocks[x, high, z] = Block.CACTUS;
                            blocks[x, high + 1, z] = Block.CACTUS;
                            blocks[x, high + 2, z] = Block.CACTUS;
                        }
                        for (int y = 0; y < high; y++)
                        {
                            byte value = Block.SAND;
                            blocks[x, y, z] = value;
                        }
                    }
                }
            }
        }

        public static int CalculateForestNoise(int x, int z)
        {
            return (int)
                (40 * Math.Pow(noise.Evaluate(x / 50f, z / 50f), 3) +
                20 * Math.Pow(noise.Evaluate(x / 25f, z / 25f), 3) +
                10 * Math.Pow(noise.Evaluate(x / 14f, z / 14f), 3) +
                100);
        }
        public static int CalculateDesertNoise(int x, int z)
        {
            return (int)
                (30 * Math.Pow(noise.Evaluate(x / 100f, z / 100f), 3) +
                100);
        }
        public static int CalculateWaterNoise(int x, int z)
        {
            return (int)
                (//30 * Math.Pow(noise.Evaluate(x / 150f, z / 150f), 3) +
                100);
        }

        public static int CalculateWaterFloorNoise(int x, int z)
        {
            return (int)
                (30 * Math.Pow(noise.Evaluate(x / 100f, z / 100f), 3) +
                80);
        }
        public static bool CalculateWaterFloorBlockNoise(int x, int z)
        {
            return oceanfloornoise.Evaluate(x / 50f, z / 50f) > 0;
        }

        public (int x, int z) position;
        public ChunkMesh mesh = null;
        public ChunkMesh waterMesh = null;
        public bool needsRemesh = false;

        public byte[,,] blocks = null;
        public float[] verts = null;
        public float[] waterVerts = null;

        public static (int x, int z) PosToChunkPos(float x, float z)
        {
            int ox = x < 0 ? ((int)x + 1) / 16 - 1 : (int)x / 16;
            int oz = z < 0 ? ((int)z + 1) / 16 - 1 : (int)z / 16;
            return (ox, oz);
        }

        public static (int x, int z) PosToChunkOffset(float x, float z)
        {
            int ox = (int)x % 16;
            if (ox < 0) ox += 16;
            int oz = (int)z % 16;
            if (oz < 0) oz += 16;
            return (ox, oz);
        }

        public void KillMesh()
        {
            if (mesh == null) return;
            mesh.Cleanup();
            mesh = null;
            if (waterMesh == null) return;
            waterMesh.Cleanup();
            waterMesh = null;
        }

        public void GenVerts()
        {
            var pospx = (position.x + 1, position.z);
            Chunk chunkpx = Craft.theCraft.chunks.ContainsKey(pospx) ? Craft.theCraft.chunks[pospx] : null;
            var posnx = (position.x - 1, position.z);
            Chunk chunknx = Craft.theCraft.chunks.ContainsKey(posnx) ? Craft.theCraft.chunks[posnx] : null;
            var pospz = (position.x, position.z + 1);
            Chunk chunkpz = Craft.theCraft.chunks.ContainsKey(pospz) ? Craft.theCraft.chunks[pospz] : null;
            var posnz = (position.x, position.z - 1);
            Chunk chunknz = Craft.theCraft.chunks.ContainsKey(posnz) ? Craft.theCraft.chunks[posnz] : null;

            List<float> outVerts = new List<float>();
            List<float> outWater = new List<float>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (blocks[x, y, z] != 0)
                        {
                            if (Block.GetBlockType(blocks[x, y, z]) == Block.Type.Leaves)
                            {
                                Matrix4 rotation = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rand.Next(0, 360)));
                                rotation *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rand.Next(0, 360)));
                                rotation *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rand.Next(0, 360)));

                                Vector4 pos1 = new Vector4(-.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos1 *= rotation;
                                Vector4 pos2 = new Vector4(1.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos2 *= rotation;
                                Vector4 pos3 = new Vector4(1.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos3 *= rotation;
                                Vector4 pos4 = new Vector4(-.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos4 *= rotation;
                                Vector4 pos5 = new Vector4(1.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos5 *= rotation;
                                Vector4 pos6 = new Vector4(-.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos6 *= rotation;

                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] _front =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, 0f, 0f, -1f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 0f, uv, 0f, 0f, -1f, // bottom left
                                };
                                outVerts.AddRange(_front);

                                pos1 = new Vector4(1.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos1 *= rotation;
                                pos2 = new Vector4(1.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos2 *= rotation;
                                pos3 = new Vector4(-.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos3 *= rotation;
                                pos4 = new Vector4(-.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos4 *= rotation;
                                pos5 = new Vector4(1.25f - 0.5f, -.25f - 0.5f, 0, 1f);
                                pos5 *= rotation;
                                pos6 = new Vector4(-.25f - 0.5f, 1.25f - 0.5f, 0, 1f);
                                pos6 *= rotation;

                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] _back =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, 0f, 0f, 1f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 0f, uv, 0f, 0f, 1f, // bottom left
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                };
                                outVerts.AddRange(_back);

                                pos1 = new Vector4(0, 1.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos1 *= rotation;
                                pos2 = new Vector4(0, 1.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos2 *= rotation;
                                pos3 = new Vector4(0, -.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos3 *= rotation;
                                pos4 = new Vector4(0, 1.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos4 *= rotation;
                                pos5 = new Vector4(0, -.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos5 *= rotation;
                                pos6 = new Vector4(0, -.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos6 *= rotation;

                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Left);
                                float[] _left =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, -1f, 0f, 0f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left 
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 0f, uv, -1f, 0f, 0f, // bottom left
                                };
                                outVerts.AddRange(_left);

                                pos1 = new Vector4(0, -.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos1 *= rotation;
                                pos2 = new Vector4(0, 1.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos2 *= rotation;
                                pos3 = new Vector4(0, 1.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos3 *= rotation;
                                pos4 = new Vector4(0, -.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos4 *= rotation;
                                pos5 = new Vector4(0, -.25f - 0.5f, -.25f - 0.5f, 1f);
                                pos5 *= rotation;
                                pos6 = new Vector4(0, 1.25f - 0.5f, 1.25f - 0.5f, 1f);
                                pos6 *= rotation;

                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Right);
                                float[] _right =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, 1f, 0f, 0f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 0f, uv, 1f, 0f, 0f, // bottom left
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                };
                                outVerts.AddRange(_right);

                                pos1 = new Vector4(1.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos1 *= rotation;
                                pos2 = new Vector4(-.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos2 *= rotation;
                                pos3 = new Vector4(-.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos3 *= rotation;
                                pos4 = new Vector4(1.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos4 *= rotation;
                                pos5 = new Vector4(1.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos5 *= rotation;
                                pos6 = new Vector4(-.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos6 *= rotation;

                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Top);
                                float[] _top =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, 0f, 1f, 0f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 0f, uv, 0f, 1f, 0f, // bottom left
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                };
                                outVerts.AddRange(_top);

                                pos1 = new Vector4(-.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos1 *= rotation;
                                pos2 = new Vector4(-.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos2 *= rotation;
                                pos3 = new Vector4(1.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos3 *= rotation;
                                pos4 = new Vector4(-.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos4 *= rotation;
                                pos5 = new Vector4(1.25f - 0.5f, 0, -.25f - 0.5f, 1f);
                                pos5 *= rotation;
                                pos6 = new Vector4(1.25f - 0.5f, 0, 1.25f - 0.5f, 1f);
                                pos6 *= rotation;

                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Bottom);
                                float[] _bottom =
                                {
                                    pos1.X + 0.5f + x, pos1.Y + 0.5f + y, pos1.Z + 0.5f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                    pos2.X + 0.5f + x, pos2.Y + 0.5f + y, pos2.Z + 0.5f + z, 1f, 1f, uv, 0f, -1f, 0f, // top right
                                    pos3.X + 0.5f + x, pos3.Y + 0.5f + y, pos3.Z + 0.5f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom 
                                    pos4.X + 0.5f + x, pos4.Y + 0.5f + y, pos4.Z + 0.5f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                    pos5.X + 0.5f + x, pos5.Y + 0.5f + y, pos5.Z + 0.5f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom right
                                    pos6.X + 0.5f + x, pos6.Y + 0.5f + y, pos6.Z + 0.5f + z, 0f, 0f, uv, 0f, -1f, 0f, // bottom left
                                };
                                outVerts.AddRange(_bottom);
                                continue;
                            }
                            if (Block.GetBlockType(blocks[x, y, z]) == Block.Type.Grass)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] left_front =
                                {
                                    //Position               UV      Layer Normal
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0, 1f, // top left 
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, 1f, 0, 1f, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0, 1f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0, 1f, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0, 1f, // bottom right
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, 1f, 0, 1f, // bottom left
                                };
                                outVerts.AddRange(left_front);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] left_back =
                                {
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0, -1f, // bottom right
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, -1f, 0, -1f, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0, -1f, // top left 
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, -1f, 0, -1f, // bottom left
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0, -1f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0, -1f, // top left 
                                };
                                outVerts.AddRange(left_back);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] right_front =
                                {
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0, 1f, // top left 
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, -1f, 0, 1f, // top right
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0, 1f, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0, 1f, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0, 1f, // bottom right
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, -1f, 0, 1f, // bottom left
                                };
                                outVerts.AddRange(right_front);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] right_back =
                                {
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0, -1f, // bottom right
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, 1f, 0, -1f, // top right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0, -1f, // top left 
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, 1f, 0, -1f, // bottom left
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0, -1f, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0, -1f, // top left 
                                };
                                outVerts.AddRange(right_back);
                                continue;
                            }
                            if (Block.GetBlockType(blocks[x, y, z]) == Block.Type.Liquid)
                            {
                                if ((z == 0 && chunknz != null && chunknz.blocks != null && Block.GetBlockOpacity(chunknz.blocks[x, y, 15]) == Block.Opacity.Transparent && Block.GetBlockType(chunknz.blocks[x, y, 15]) != Block.Type.Liquid) || (z != 0 && Block.GetBlockOpacity(blocks[x, y, z - 1]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y, z - 1]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                    float[] _front =
                                    {
                                        0f + x,  1f + y, 0f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                        1f + x,  1f + y, 0f + z, 1f, 1f, uv, 0f, 0f, -1f, // top right
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                        0f + x,  1f + y, 0f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                        0f + x,  0f + y, 0f + z, 0f, 0f, uv, 0f, 0f, -1f, // bottom left
                                    };
                                    outWater.AddRange(_front);
                                }
                                if ((z == 15 && chunkpz != null && chunkpz.blocks != null && Block.GetBlockOpacity(chunkpz.blocks[x, y, 0]) == Block.Opacity.Transparent && Block.GetBlockType(chunkpz.blocks[x, y, 0]) != Block.Type.Liquid) || (z != 15 && Block.GetBlockOpacity(blocks[x, y, z + 1]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y, z + 1]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                    float[] _back =
                                    {
                                        1f + x,  0f + y, 1f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                        1f + x,  1f + y, 1f + z, 1f, 1f, uv, 0f, 0f, 1f, // top right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                        0f + x,  0f + y, 1f + z, 0f, 0f, uv, 0f, 0f, 1f, // bottom left
                                        1f + x,  0f + y, 1f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                    };
                                    outWater.AddRange(_back);
                                }
                                if ((x == 0 && chunknx != null && chunknx.blocks != null && Block.GetBlockOpacity(chunknx.blocks[15, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(chunknx.blocks[15, y, z]) != Block.Type.Liquid) || (x != 0 && Block.GetBlockOpacity(blocks[x - 1, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x - 1, y, z]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Left);
                                    float[] _left =
                                    {
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left
                                        0f + x,  1f + y, 0f + z, 1f, 1f, uv, -1f, 0f, 0f, // top right
                                        0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left 
                                        0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                        0f + x,  0f + y, 1f + z, 0f, 0f, uv, -1f, 0f, 0f, // bottom left
                                    };
                                    outWater.AddRange(_left);
                                }
                                if ((x == 15 && chunkpx != null && chunkpx.blocks != null && Block.GetBlockOpacity(chunkpx.blocks[0, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(chunkpx.blocks[0, y, z]) != Block.Type.Liquid) || (x != 15 && Block.GetBlockOpacity(blocks[x + 1, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x + 1, y, z]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Right);
                                    float[] _right =
                                    {
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                        1f + x,  1f + y, 0f + z, 1f, 1f, uv, 1f, 0f, 0f, // top right
                                        1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                        1f + x,  0f + y, 1f + z, 0f, 0f, uv, 1f, 0f, 0f, // bottom left
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                        1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                    };
                                    outWater.AddRange(_right);
                                }
                                if (y == 255 || Block.GetBlockOpacity(blocks[x, y + 1, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y + 1, z]) != Block.Type.Liquid)
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Top);
                                    float[] _top =
                                    {
                                        1f + x,  1f + y, 0f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                        0f + x,  1f + y, 0f + z, 1f, 1f, uv, 0f, 1f, 0f, // top right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                        1f + x,  1f + y, 1f + z, 0f, 0f, uv, 0f, 1f, 0f, // bottom left
                                        1f + x,  1f + y, 0f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                    };
                                    outWater.AddRange(_top);
                                }
                                if (y == 0 || Block.GetBlockOpacity(blocks[x, y - 1, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y - 1, z]) != Block.Type.Liquid)
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Bottom);
                                    float[] _bottom =
                                    {
                                        0f + x,  0f + y, 1f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                        0f + x,  0f + y, 0f + z, 1f, 1f, uv, 0f, -1f, 0f, // top right
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom 
                                        0f + x,  0f + y, 1f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom right
                                        1f + x,  0f + y, 1f + z, 0f, 0f, uv, 0f, -1f, 0f, // bottom left
                                    };
                                    outWater.AddRange(_bottom);
                                }
                                continue;
                            }
                            if ((z == 0 && chunknz != null && chunknz.blocks != null && Block.GetBlockOpacity(chunknz.blocks[x, y, 15]) == Block.Opacity.Transparent) || (z != 0 && Block.GetBlockOpacity(blocks[x, y, z - 1]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] _front =
                                {
                                    0f + x,  1f + y, 0f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, 0f, 0f, -1f, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                    0f + x,  1f + y, 0f + z, 0f, 1f, uv, 0f, 0f, -1f, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, 0f, -1f, // bottom right
                                    0f + x,  0f + y, 0f + z, 0f, 0f, uv, 0f, 0f, -1f, // bottom left
                                };
                                outVerts.AddRange(_front);
                            }
                            if ((z == 15 && chunkpz != null && chunkpz.blocks != null && Block.GetBlockOpacity(chunkpz.blocks[x, y, 0]) == Block.Opacity.Transparent) || (z != 15 && Block.GetBlockOpacity(blocks[x, y, z + 1]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] _back =
                                {
                                    1f + x,  0f + y, 1f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                    1f + x,  1f + y, 1f + z, 1f, 1f, uv, 0f, 0f, 1f, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, 0f, 0f, 1f, // bottom left
                                    1f + x,  0f + y, 1f + z, 1f, 0f, uv, 0f, 0f, 1f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 0f, 1f, // top left 
                                };
                                outVerts.AddRange(_back);
                            }
                            if ((x == 0 && chunknx != null && chunknx.blocks != null && Block.GetBlockOpacity(chunknx.blocks[15, y, z]) == Block.Opacity.Transparent) || (x != 0 && Block.GetBlockOpacity(blocks[x - 1, y, z]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Left);
                                float[] _left =
                                {
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, -1f, 0f, 0f, // top right
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, -1f, 0f, 0f, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, -1f, 0f, 0f, // bottom right
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, -1f, 0f, 0f, // bottom left
                                };
                                outVerts.AddRange(_left);
                            }
                            if ((x == 15 && chunkpx != null && chunkpx.blocks != null && Block.GetBlockOpacity(chunkpx.blocks[0, y, z]) == Block.Opacity.Transparent) || (x != 15 && Block.GetBlockOpacity(blocks[x + 1, y, z]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Right);
                                float[] _right =
                                {
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, 1f, 0f, 0f, // top right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, 1f, 0f, 0f, // bottom left
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 1f, 0f, 0f, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, 1f, 0f, 0f, // top left 
                                };
                                outVerts.AddRange(_right);
                            }
                            if (y == 255 || Block.GetBlockOpacity(blocks[x, y + 1, z]) == Block.Opacity.Transparent)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Top);
                                float[] _top =
                                {
                                    1f + x,  1f + y, 0f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, 0f, 1f, 0f, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                    1f + x,  1f + y, 1f + z, 0f, 0f, uv, 0f, 1f, 0f, // bottom left
                                    1f + x,  1f + y, 0f + z, 1f, 0f, uv, 0f, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, 0f, 1f, 0f, // top left 
                                };
                                outVerts.AddRange(_top);
                            }
                            if (y == 0 || Block.GetBlockOpacity(blocks[x, y - 1, z]) == Block.Opacity.Transparent)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Bottom);
                                float[] _bottom =
                                {
                                    0f + x,  0f + y, 1f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 1f, uv, 0f, -1f, 0f, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom 
                                    0f + x,  0f + y, 1f + z, 0f, 1f, uv, 0f, -1f, 0f, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, 0f, -1f, 0f, // bottom right
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, 0f, -1f, 0f, // bottom left
                                };
                                outVerts.AddRange(_bottom);
                            }
                        }
                    }
                }
            }
            
            verts = outVerts.ToArray();
            waterVerts = outWater.ToArray();
            needsRemesh = true;
        }
    }
}
