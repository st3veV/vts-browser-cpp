/**
 * Copyright (c) 2017 Melown Technologies SE
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * *  Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 *
 * *  Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace vts
{
    public enum GpuType
    {
        // compatible with OpenGL
        Byte = 0x1400,
        UnsignedByte = 0x1401,
        Short = 0x1402, // two bytes
        UnsignedShort = 0x1403,
        Int = 0x1404, // four bytes
        UnsignedInt = 0x1405,
        Float = 0x1406, // four bytes
    };

    public enum FilterMode
    {
        // compatible with OpenGL
        Nearest = 0x2600,
        Linear = 0x2601,
        NearestMipmapNearest = 0x2700,
        LinearMipmapNearest = 0x2701,
        NearestMipmapLinear = 0x2702,
        LinearMipmapLinear = 0x2703,
    };

    public enum WrapMode
    {
        // compatible with OpenGL
        Repeat = 0x2901,
        ClampToEdge = 0x812F,
        ClampToBorder = 0x812D,
        MirroredRepeat = 0x8370,
        MirrorClampToEdge = 0x8743,
    };

    public enum FaceMode
    {
        // compatible with OpenGL
        Points = 0x0000,
        Lines = 0x0001,
        LineStrip = 0x0003,
        Triangles = 0x0004,
        TriangleStrip = 0x0005,
        TriangleFan = 0x0006,
    };

    public class Texture
    {
        public uint width;
        public uint height;
        public uint components;
        public GpuType type;
        public FilterMode filterMode;
        public WrapMode wrapMode;
        public byte[] data;
        public string id;

        public void Load(IntPtr handle)
        {
            id = Util.CheckString(BrowserInterop.vtsResourceGetId(handle));
            BrowserInterop.vtsTextureGetResolution(handle, ref width, ref height, ref components);
            Util.CheckInterop();
            type = (GpuType)BrowserInterop.vtsTextureGetType(handle);
            Util.CheckInterop();
            filterMode = (FilterMode)BrowserInterop.vtsTextureGetFilterMode(handle);
            Util.CheckInterop();
            wrapMode = (WrapMode)BrowserInterop.vtsTextureGetWrapMode(handle);
            Util.CheckInterop();
            IntPtr bufPtr = IntPtr.Zero;
            uint bufSize = 0;
            BrowserInterop.vtsTextureGetBuffer(handle, ref bufPtr, ref bufSize);
            Util.CheckInterop();
            data = new byte[bufSize];
            Marshal.Copy(bufPtr, data, 0, (int)bufSize);
        }
    }

    public struct VertexAttribute
    {
        public uint offset; // in bytes
        public uint stride; // in bytes
        public uint components; // 1, 2, 3 or 4
        public GpuType type;
        public bool enable;
        public bool normalized;
    };

    public class Mesh
    {
        public FaceMode faceMode;
        public List<VertexAttribute> attributes;
        public uint verticesCount;
        public uint indicesCount;
        public byte[] vertices;
        public ushort[] indices;
        public string id;

        public void Load(IntPtr handle)
        {
            id = Util.CheckString(BrowserInterop.vtsResourceGetId(handle));
            faceMode = (FaceMode)BrowserInterop.vtsMeshGetFaceMode(handle);
            Util.CheckInterop();
            IntPtr bufPtr = IntPtr.Zero;
            uint bufSize = 0;
            BrowserInterop.vtsMeshGetIndices(handle, ref bufPtr, ref bufSize, ref indicesCount);
            Util.CheckInterop();
            if (indicesCount > 0)
            {
                short[] tmp = new short[indicesCount];
                Marshal.Copy(bufPtr, tmp, 0, (int)indicesCount);
                indices = new ushort[indicesCount];
                Buffer.BlockCopy(tmp, 0, indices, 0, (int)indicesCount * 2);
            }
            BrowserInterop.vtsMeshGetVertices(handle, ref bufPtr, ref bufSize, ref verticesCount);
            Util.CheckInterop();
            vertices = new byte[bufSize];
            Marshal.Copy(bufPtr, vertices, 0, (int)bufSize);
            attributes = new List<VertexAttribute>(4);
            for (uint i = 0; i < 4; i++)
            {
                VertexAttribute a;
                a.offset = 0;
                a.stride = 0;
                a.components = 0;
                a.enable = false;
                a.normalized = false;
                uint type = 0;
                BrowserInterop.vtsMeshGetAttribute(handle, i, ref a.offset, ref a.stride, ref a.components, ref type, ref a.enable, ref a.normalized);
                Util.CheckInterop();
                a.type = (GpuType)type;
                attributes.Add(a);
            }
        }
    }

    public enum GeodataType
    {
        Invalid = 0,
        Triangles = 1,
        LineFlat = 2,
        PointFlat = 3,
        IconFlat = 4,
        LabelFlat = 5,
        LineScreen = 6,
        PointScreen = 7,
        IconScreen = 8,
        LabelScreen = 9,
    }

    public class Point3D
    {
        public double x;
        public double y;
        public double z;

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }
        public double[] ToArray3()
        {
            return new double[] { x, y, z };
        }
        public double[] ToArray4()
        {
            return new double[] { x, y, z, 1 };
        }
    }

    public class Geodata
    {

        public string id;
        public GeodataType type;
        public string[] texts;
        public Point3D[][] positions;
        public double[] model;
        public Dictionary<string, string>[] properties;

        public void Load(IntPtr handle)
        {
            id = Util.CheckString(BrowserInterop.vtsResourceGetId(handle));

            type = (GeodataType)BrowserInterop.vtsGeodataGetType(handle);
            Util.CheckInterop();
            LoadModel(handle);
            LoadTexts(handle);
            LoadPositions(handle);
            LoadProperties(handle);
        }

        private void LoadModel(IntPtr handle)
        {
            model = new double[16];
            IntPtr bufPtr = IntPtr.Zero;
            BrowserInterop.vtsGeodataGetModel(handle, ref bufPtr);
            Util.CheckInterop();
            Marshal.Copy(bufPtr, model, 0, 16);
        }

        private void LoadPositions(IntPtr handle)
        {
            IntPtr bufPtr = IntPtr.Zero;
            IntPtr sizesBufPtr = IntPtr.Zero;
            uint bufSize = 0;
            BrowserInterop.vtsGeodataGetPositions(handle, ref bufPtr, ref bufSize, ref sizesBufPtr);
            Util.CheckInterop();

            if (bufSize == 0)
            {
                positions = new Point3D[0][];
                return;
            }

            int[] sizes = new int[bufSize];
            Marshal.Copy(sizesBufPtr, sizes, 0, (int)bufSize);

            positions = new Point3D[bufSize][];

            for (int i = 0; i < bufSize; i++)
            {
                uint size = (uint)sizes[i];
                List<Point3D> subVector = new((int)size);
                int numPoints = 3;
                for (int j = 0; j < size; j++)
                {
                    float[] points = new float[numPoints];
                    Marshal.Copy(bufPtr + j * numPoints, points, 0, numPoints);
                    Point3D vec = new(points[0], points[1], points[2]);
                    subVector.Add(vec);
                }
                positions[i] = subVector.ToArray();
                bufPtr += (int)size * numPoints;
            }
        }

        private void LoadTexts(IntPtr handle)
        {
            IntPtr bufPtr = IntPtr.Zero;
            uint bufSize = 0;
            BrowserInterop.vtsGeodataGetTexts(handle, ref bufPtr, ref bufSize);
            Util.CheckInterop();

            if (bufSize == 0)
            {
                texts = new string[0];
                return;
            }

            string[] stringArray = new string[bufSize];
            IntPtr[] stringPointers = new IntPtr[bufSize];
            Marshal.Copy(bufPtr, stringPointers, 0, (int)bufSize);
            for (int i = 0; i < bufSize; i++)
            {
                if (stringPointers[i] == IntPtr.Zero)
                {
                    stringArray[i] = "";
                    continue;
                }
                stringArray[i] = Marshal.PtrToStringAnsi(stringPointers[i]);
            }

            texts = stringArray;
        }

        private void LoadProperties(IntPtr handle)
        {
            IntPtr bufPtr = IntPtr.Zero;
            uint bufSize = 0;
            BrowserInterop.vtsGeodataGetProperties(handle, ref bufPtr, ref bufSize);
            Util.CheckInterop();

            if (bufSize == 0)
            {
                return;
            }

            // Deserialize the properties
            List<Dictionary<string, string>> properties = new();
            byte[] data = new byte[bufSize];
            Marshal.Copy(bufPtr, data, 0, (int)bufSize);

            int dictCount = BitConverter.ToInt32(data, 0);
            int position = sizeof(int);

            for (int i = 0; i < dictCount; i++)
            {
                int dictSize = BitConverter.ToInt32(data, position);
                position += sizeof(int);

                var map = new Dictionary<string, string>();

                for (int j = 0; j < dictSize; j++)
                {
                    int keySize = BitConverter.ToInt32(data, position);
                    position += sizeof(int);
                    int valueSize = BitConverter.ToInt32(data, position);
                    position += sizeof(int);

                    string key = Encoding.ASCII.GetString(data, position, keySize);
                    position += keySize;

                    string value = Encoding.ASCII.GetString(data, position, valueSize);
                    position += valueSize;

                    map[key] = value;
                }

                properties.Add(map);
            }

            this.properties = properties.ToArray();
        }
    }

    public class Font
    {
        public string id;

        public void Load(IntPtr handle)
        {
            id = Util.CheckString(BrowserInterop.vtsResourceGetId(handle));
        }
    }

}
