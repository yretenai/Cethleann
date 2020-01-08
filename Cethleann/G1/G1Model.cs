using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.G1.G1ModelSection;
using Cethleann.G1.G1ModelSection.G1MGSection;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Model;
using DragonLib;
using DragonLib.GLTF;
using DragonLib.Numerics;
using JetBrains.Annotations;

namespace Cethleann.G1
{
    /// <summary>
    ///     G1Model is the main model format
    /// </summary>
    [PublicAPI]
    public class G1Model : IKTGLSection
    {
        /// <summary>
        ///     Initialize with no data.
        /// </summary>
        public G1Model()
        {
            Section = new ResourceSectionHeader
            {
                Magic = DataType.Model,
                Size = -1,
                Version = SupportedVersion.ToVersionA()
            };
            SectionRoot = new PackedResource();
        }

        /// <summary>
        ///     Parse G1M from the provided data buffer
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreVersion"></param>
        /// <param name="parse"></param>
        public G1Model(Span<byte> data, bool ignoreVersion = true, bool parse = true)
        {
            if (!data.Matches(DataType.Model)) throw new InvalidOperationException("Not an G1M stream");

            Section = MemoryMarshal.Read<ResourceSectionHeader>(data);
            if (!ignoreVersion && Section.Version.ToVersion() != SupportedVersion) throw new NotSupportedException($"G1M version {Section.Version.ToVersion()} is not supported!");

            var header = MemoryMarshal.Read<ModelHeader>(data.Slice(0xC));
            SectionRoot = new PackedResource(data.Slice(header.HeaderSize, Section.Size - header.HeaderSize), header.SectionCount);
            if (!parse) return;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var block in SectionRoot.Sections)
            {
                var sectionHeader = MemoryMarshal.Read<ResourceSectionHeader>(block.Span);
                var dataBlock = block.Span.Slice(SizeHelper.SizeOf<ResourceSectionHeader>());
                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                var section = sectionHeader.Magic switch
                {
                    DataType.ModelSkeleton => (IKTGLSection) new G1MSkeleton(dataBlock, ignoreVersion, sectionHeader),
                    DataType.ModelFormat => new G1MFormat(dataBlock, ignoreVersion, sectionHeader),
                    DataType.ModelGeometry => new G1MGeometry(dataBlock, ignoreVersion, sectionHeader),
                    DataType.ModelMatrix => new G1MMatrix(dataBlock, ignoreVersion, sectionHeader),
                    DataType.ModelExtra => new G1Extra(dataBlock, ignoreVersion, sectionHeader),
                    DataType.ModelCollision => null,
                    DataType.ModelClothDriver => null,
                    DataType.ModelClothSurface => null,
                    DataType.ModelCloth => null,
                    DataType.ModelSoftbody => null,
                    DataType.ModelHair => null,
                    _ => throw new NotImplementedException($"Section {sectionHeader.Magic.ToFourCC(false)} not supported!")
                };

                Sections.Add(section);
            }
        }

        /// <summary>
        ///     Raw data for each section
        /// </summary>
        public PackedResource SectionRoot { get; set; }

        /// <summary>
        ///     Sections found in this model.
        ///     Look for <seealso cref="G1MGeometry" /> for Geometry.
        ///     Look for <seealso cref="G1MSkeleton" /> for Skeleton.
        /// </summary>
        public List<IKTGLSection> Sections { get; } = new List<IKTGLSection>();

        /// <inheritdoc />
        public int SupportedVersion { get; } = 37;


        /// <inheritdoc />
        public ResourceSectionHeader Section { get; }

        /// <summary>
        ///     Write to G1M span using just SectionsRoot
        /// </summary>
        /// <returns></returns>
        public Span<byte> WriteFromRoot()
        {
            var sectionHeaderSize = SizeHelper.SizeOf<ResourceSectionHeader>();
            var size = SizeHelper.SizeOf<ModelHeader>() + sectionHeaderSize + SectionRoot.Sections.Sum(x => x.Length);
            var buffer = new Span<byte>(new byte[size]);
            var modelSectionHeader = Section;
            modelSectionHeader.Size = size;
            MemoryMarshal.Write(buffer, ref modelSectionHeader);
            var offset = sectionHeaderSize;
            var modelHeader = new ModelHeader
            {
                HeaderSize = sectionHeaderSize + SizeHelper.SizeOf<ModelHeader>(),
                Reserved = 0,
                SectionCount = SectionRoot.Sections.Count
            };
            MemoryMarshal.Write(buffer.Slice(offset), ref modelHeader);
            offset = modelHeader.HeaderSize;

            foreach (var data in SectionRoot.Sections)
            {
                data.Span.CopyTo(buffer.Slice(offset));
                offset += data.Length;
            }

            return buffer;
        }

        /// <summary>
        ///     Gets a specific section from the G1M model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSection<T>() where T : class, IKTGLSection
        {
            return GetSections<T>().FirstOrDefault();
        }

        /// <summary>
        ///     Gets a specific section from the G1M model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetSections<T>() where T : class, IKTGLSection
        {
            return Sections.OfType<T>();
        }

        private float[] ReadStrideEntryFloat(VertexDataType dataType, Span<byte> slice)
        {
            return dataType switch
            {
                VertexDataType.Vector4Byte => MemoryMarshal.Cast<byte, byte>(slice.Slice(0, 4)).ToArray().Select(x => x / (float) byte.MaxValue).ToArray(),
                VertexDataType.Vector2Single => MemoryMarshal.Cast<byte, float>(slice.Slice(0, 4 * 2)).ToArray(),
                VertexDataType.Vector3Single => MemoryMarshal.Cast<byte, float>(slice.Slice(0, 4 * 3)).ToArray(),
                VertexDataType.Vector4Single => MemoryMarshal.Cast<byte, float>(slice.Slice(0, 4 * 4)).ToArray(),
                VertexDataType.Vector4ByteNormalized => MemoryMarshal.Cast<byte, byte>(slice.Slice(0, 4)).ToArray().Select(x => x / (float) byte.MaxValue).ToArray(),
                VertexDataType.Vector2Half => MemoryMarshal.Cast<byte, ushort>(slice.Slice(0, 2 * 2)).ToArray().Select(x => (float) Half.ToHalf(x)).ToArray(),
                VertexDataType.Vector4Half => MemoryMarshal.Cast<byte, ushort>(slice.Slice(0, 2 * 4)).ToArray().Select(x => (float) Half.ToHalf(x)).ToArray(),
                VertexDataType.Vector4HalfNormalized => MemoryMarshal.Cast<byte, ushort>(slice.Slice(0, 2 * 4)).ToArray().Select(x => x / (float) ushort.MaxValue).ToArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        private int[] ReadStrideEntryInt(VertexDataType dataType, Span<byte> slice)
        {
            return dataType switch
            {
                VertexDataType.Vector4Byte => MemoryMarshal.Cast<byte, byte>(slice.Slice(0, 4)).ToArray().Select(x => (int) x).ToArray(),
                VertexDataType.Vector2Single => MemoryMarshal.Cast<byte, int>(slice.Slice(0, 4 * 2)).ToArray(),
                VertexDataType.Vector3Single => MemoryMarshal.Cast<byte, int>(slice.Slice(0, 4 * 3)).ToArray(),
                VertexDataType.Vector4Single => MemoryMarshal.Cast<byte, int>(slice.Slice(0, 4 * 4)).ToArray(),
                VertexDataType.Vector4ByteNormalized => MemoryMarshal.Cast<byte, byte>(slice.Slice(0, 4)).ToArray().Select(x => (int) x).ToArray(),
                VertexDataType.Vector2Half => MemoryMarshal.Cast<byte, short>(slice.Slice(0, 2 * 2)).ToArray().Select(x => (int) x).ToArray(),
                VertexDataType.Vector4Half => MemoryMarshal.Cast<byte, short>(slice.Slice(0, 2 * 4)).ToArray().Select(x => (int) x).ToArray(),
                VertexDataType.Vector4HalfNormalized => MemoryMarshal.Cast<byte, short>(slice.Slice(0, 2 * 4)).ToArray().Select(x => (int) x).ToArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
            };
        }

        /// <summary>
        ///     Exports meshes to GLTF
        /// </summary>
        /// <param name="bufferPath"></param>
        /// <param name="bufferPathRoot"></param>
        /// <param name="lod"></param>
        /// <param name="group"></param>
        /// <param name="texBase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GLTFRoot ExportMeshes(string bufferPath, string bufferPathRoot, int lod = -1, int group = -1, string texBase = "")
        {
            var root = new GLTFRoot
            {
                Asset = new GLTFAsset
                {
                    Version = "2.0",
                    Generator = "KTGL",
                    Copyright = "2019 (c) Koei"
                }
            };

            var scene = new GLTFScene();
            root.Scenes.Add(scene);
            root.Scene = 0;

            var skeleton = GetSection<G1MSkeleton>();

            var geom = GetSection<G1MGeometry>();

            var bones = geom.GetSection<G1MGBone>();
            var ibos = geom.GetSection<G1MGIndexBuffer>();
            var vbos = geom.GetSection<G1MGVertexBuffer>();
            var attributes = geom.GetSection<G1MGVertexAttribute>();
            var subMeshes = geom.GetSection<G1MGSubMesh>();
            var meshGroups = geom.GetSection<G1MGMesh>();
            var materials = geom.GetSection<G1MGMaterial>();

            var samplerId = root.Samplers.Count;
            root.Samplers.Add(new GLTFSampler
            {
                WrapS = GLTFWrapMode.Repeat,
                WrapT = GLTFWrapMode.Repeat,
                MagFilter = GLTFMagFilterMode.Linear,
                MinFilter = GLTFMinFilterMode.Linear
            });

            var materialIds = new List<int>();
            var textureIds = new Dictionary<int, int>();
            foreach (var (_, textureSet) in materials.Materials)
            {
                materialIds.Add(root.Materials.Count);
                var gltfMaterial = new GLTFMaterial
                {
                    Name = $"Material_{materialIds.Count}",
                    DoubleSided = false,
                    PbrMetallicRoughness = new GLTFPBRMetallicRoughness()
                };
                if (GetTexture(texBase, root, textureSet, textureIds, TextureKind.Color, samplerId, out var colorId, out var colorIndex))
                    gltfMaterial.PbrMetallicRoughness.BaseColorTexture = new GLTFTextureInfo
                    {
                        Index = colorId,
                        TexCoord = colorIndex.TexCoord
                    };

                if (GetTexture(texBase, root, textureSet, textureIds, TextureKind.Normal, samplerId, out var normalId, out var normalIndex))
                    gltfMaterial.NormalTexture = new GLTFNormalTextureInfo
                    {
                        Index = normalId,
                        TexCoord = normalIndex.TexCoord
                    };

                root.Materials.Add(gltfMaterial);
            }

            var gltfBuffer = new GLTFBuffer
            {
                Uri = bufferPathRoot
            };
            var gltfBufferId = root.Buffers.Count;
            root.Buffers.Add(gltfBuffer);
            var bufferChunks = new Span<byte>(new byte[] { 0x44, 0x52, 0x47, 0x4E });
            var vboDeconstructed = new List<Dictionary<string, int>>();
            var semanticToComponentType = new Dictionary<string, GLTFComponentType>
            {
                { "POSITION", GLTFComponentType.Float },
                { "NORMAL", GLTFComponentType.Float },
                { "TANGENT", GLTFComponentType.Float },
                { "TEXCOORD_0", GLTFComponentType.Float },
                { "TEXCOORD_1", GLTFComponentType.Float },
                { "COLOR_0", GLTFComponentType.Float },
                { "JOINTS_0", GLTFComponentType.UnsignedShort },
                { "WEIGHTS_0", GLTFComponentType.Float }
            };
            var semanticToAttributeType = new Dictionary<string, GLTFAccessorAttributeType>
            {
                { "POSITION", GLTFAccessorAttributeType.VEC3 },
                { "NORMAL", GLTFAccessorAttributeType.VEC3 },
                { "TANGENT", GLTFAccessorAttributeType.VEC4 },
                { "TEXCOORD_0", GLTFAccessorAttributeType.VEC2 },
                { "TEXCOORD_1", GLTFAccessorAttributeType.VEC2 },
                { "COLOR_0", GLTFAccessorAttributeType.VEC4 },
                { "JOINTS_0", GLTFAccessorAttributeType.VEC4 },
                { "WEIGHTS_0", GLTFAccessorAttributeType.VEC4 }
            };
            var jointsData = new List<List<Vector4Short>>();
            for (var i = 0; i < vbos.Buffers.Count; ++i)
            {
                var (vboInfo, vbo) = vbos.Buffers.ElementAt(i);
                var (_, attrs) = attributes.Attributes.ElementAt(i);
                var chunks = new Dictionary<string, object>
                {
                    { "POSITION", new List<Vector3>() },
                    { "NORMAL", new List<Vector3>() },
                    { "TANGENT", new List<Vector4>() },
                    { "TEXCOORD_0", new List<Vector2>() },
                    { "TEXCOORD_1", new List<Vector2>() },
                    { "COLOR_0", new List<Vector4>() },
                    { "JOINTS_0", new List<Vector4Short>() },
                    { "WEIGHTS_0", new List<Vector4>() }
                };
                for (var j = 0; j < vbo.Length; j += vboInfo.Stride)
                {
                    var vboSlice = vbo.Slice(j);
                    foreach (var attr in attrs)
                    {
                        var slice = vboSlice.Slice(attr.Offset).Span;
                        switch (attr.Semantic)
                        {
                            case VertexSemantic.Position:
                            {
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks["POSITION"] as List<Vector3>).Add(new Vector3
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2)
                                });
                                break;
                            }
                            case VertexSemantic.Normal:
                            {
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks["NORMAL"] as List<Vector3>).Add(new Vector3
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2)
                                });
                                break;
                            }
                            case VertexSemantic.UV:
                            {
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks[$"TEXCOORD_{(attr.Layer == 0 ? 0 : 1)}"] as List<Vector2>).Add(new Vector2
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1)
                                });
                                break;
                            }
                            case VertexSemantic.Tangent:
                            {
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks["TANGENT"] as List<Vector4>).Add(new Vector4
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2),
                                    W = pos.ElementAtOrDefault(3)
                                });
                                break;
                            }
                            case VertexSemantic.Color:
                            {
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks["COLOR_0"] as List<Vector4>).Add(new Vector4
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2),
                                    W = pos.ElementAtOrDefault(3)
                                });
                                break;
                            }
                            case VertexSemantic.Weight:
                            {
                                if (skeleton == null) continue;
                                var pos = ReadStrideEntryFloat(attr.DataType, slice);
                                (chunks["WEIGHTS_0"] as List<Vector4>).Add(new Vector4
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2),
                                    W = pos.ElementAtOrDefault(3)
                                });

                                break;
                            }
                            case VertexSemantic.Bone:
                            {
                                if (skeleton == null) continue;
                                var pos = ReadStrideEntryInt(attr.DataType, slice).Select(x => (short) x).ToArray();
                                (chunks["JOINTS_0"] as List<Vector4Short>).Add(new Vector4Short
                                {
                                    X = pos.ElementAtOrDefault(0),
                                    Y = pos.ElementAtOrDefault(1),
                                    Z = pos.ElementAtOrDefault(2),
                                    W = pos.ElementAtOrDefault(3)
                                });
                                break;
                            }
                            case VertexSemantic.BiNormal:
                            case VertexSemantic.Fog:
                            case VertexSemantic.PointSize:
                            case VertexSemantic.TesselationFactor:
                            case VertexSemantic.PositionTransform:
                            case VertexSemantic.Depth:
                            case VertexSemantic.Sample:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                if (skeleton != null && (chunks["WEIGHTS_0"] as List<Vector4>).Count == 0)
                {
                    var size = (chunks["POSITION"] as List<Vector3>).Count;
                    for (var j = 0; j < size; ++j) (chunks["WEIGHTS_0"] as List<Vector4>).Add(new Vector4(1, 0, 0, 0));
                }

                var vboBuffer = new Dictionary<string, int>();
                foreach (var (semantic, data) in chunks)
                {
                    switch (semantic)
                    {
                        case "JOINTS_0":
                            jointsData.Add(data as List<Vector4Short>);
                            continue;
                    }

                    if ((data as IList)?.Count == 0) continue;
                    var bytes = data switch
                    {
                        List<Vector2> v2Data => MemoryMarshal.Cast<Vector2, byte>(v2Data.ToArray()),
                        List<Vector3> v3Data => MemoryMarshal.Cast<Vector3, byte>(v3Data.ToArray()),
                        List<Vector4> v4Data => MemoryMarshal.Cast<Vector4, byte>(v4Data.ToArray()),
                        List<Vector4Short> shData => MemoryMarshal.Cast<Vector4Short, byte>(shData.ToArray()),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var bufferView = new GLTFBufferView
                    {
                        Buffer = gltfBufferId,
                        ByteOffset = (uint) bufferChunks.Length,
                        ByteLength = (uint) bytes.Length,
                        ByteStride = (uint) (bytes.Length / (data as IList).Count)
                    };
                    var temp = new Span<byte>(new byte[(bufferChunks.Length + bytes.Length).Align(4)]);
                    bufferChunks.CopyTo(temp);
                    bytes.CopyTo(temp.Slice(bufferChunks.Length));
                    bufferChunks = temp;
                    var bufferViewId = root.BufferViews.Count;
                    var accessorId = root.Accessors.Count;
                    var accessor = new GLTFAccessor
                    {
                        BufferView = bufferViewId,
                        ComponentType = semanticToComponentType[semantic],
                        Type = semanticToAttributeType[semantic],
                        Count = (uint) (data as IList)?.Count
                    };
                    if (semantic == "POSITION")
                    {
                        accessor.Max = new List<double>
                        {
                            geom.Header.BoundingBoxTopLeft.X,
                            geom.Header.BoundingBoxTopLeft.Y,
                            geom.Header.BoundingBoxTopLeft.Z
                        };
                        accessor.Min = new List<double>
                        {
                            geom.Header.BoundingBoxBottomRight.X,
                            geom.Header.BoundingBoxBottomRight.Y,
                            geom.Header.BoundingBoxBottomRight.Z
                        };
                    }

                    root.Accessors.Add(accessor);
                    vboBuffer[semantic] = accessorId;
                    root.BufferViews.Add(bufferView);
                }

                vboDeconstructed.Add(vboBuffer);
            }

            var iboDeconstructed = new List<int>();
            for (var i = 0; i < ibos.Buffers.Count; ++i)
            {
                var bytes = MemoryMarshal.Cast<ushort, byte>(ibos.Buffers[i].buffer);
                var bufferView = new GLTFBufferView
                {
                    Buffer = gltfBufferId,
                    ByteOffset = (uint) bufferChunks.Length,
                    ByteLength = (uint) bytes.Length
                };
                var temp = new Span<byte>(new byte[(bufferChunks.Length + bytes.Length).Align(4)]);
                bufferChunks.CopyTo(temp);
                bytes.CopyTo(temp.Slice(bufferChunks.Length));
                bufferChunks = temp;
                var bufferViewId = root.BufferViews.Count;
                root.BufferViews.Add(bufferView);
                iboDeconstructed.Add(bufferViewId);
            }

            var skinId = default(int?);
            scene.Nodes.Add(root.Nodes.Count);
            var skinNode = new GLTFNode
            {
                Name = "CethleannExport"
            };
            root.Nodes.Add(skinNode);
            var rootBones = new List<int>();
            if (skeleton != null)
            {
                skinId = root.Skins.Count;
                var joints = new List<int>();
                var jointsActual = new List<(GLTFNode, int, ModelSkeletonBone)>();
                var matrices = new List<Matrix4x4>();
                for (var index = 0; index < skeleton.Bones.Length; index++)
                {
                    var bone = skeleton.Bones[index];
                    var worldBone = skeleton.WorldBones[index];
                    var pos = bone.Position;
                    var rot = bone.Rotation;
                    var scale = bone.Scale;
                    var joint = new GLTFNode
                    {
                        Translation = pos,
                        Scale = scale,
                        Rotation = rot,
                        Name = $"Bone_{index:x}"
                    };
                    var jointId = root.Nodes.Count;
                    joints.Add(jointId);
                    root.Nodes.Add(joint);
                    jointsActual.Add((joint, jointId, bone));
                    if (!bone.HasParent()) rootBones.Add(jointId);
                    matrices.Add(worldBone.GetMatrix());
                }

                foreach (var (_, jointId, bone) in jointsActual)
                {
                    if (bone.HasParent())
                        jointsActual[bone.Parent].Item1.Children.Add(jointId);
                }

                var bytes = MemoryMarshal.Cast<Matrix4x4, byte>(matrices.ToArray());
                var bufferView = new GLTFBufferView
                {
                    Buffer = gltfBufferId,
                    ByteOffset = (uint) bufferChunks.Length,
                    ByteLength = (uint) bytes.Length
                };
                var temp = new Span<byte>(new byte[(bufferChunks.Length + bytes.Length).Align(4)]);
                bufferChunks.CopyTo(temp);
                bytes.CopyTo(temp.Slice(bufferChunks.Length));
                bufferChunks = temp;
                var bufferViewId = root.BufferViews.Count;
                root.BufferViews.Add(bufferView);
                var matrixId = root.Accessors.Count;
                root.Accessors.Add(new GLTFAccessor
                {
                    BufferView = bufferViewId,
                    ComponentType = GLTFComponentType.Float,
                    Type = GLTFAccessorAttributeType.MAT4,
                    Count = (uint) matrices.Count
                });

                var skin = new GLTFSkin
                {
                    Name = "CethleannExportedSkeleton",
                    InverseBindMatrices = matrixId,
                    Joints = joints
                };
                root.Skins.Add(skin);
            }

            foreach (var (meshGroup, meshes) in meshGroups.Meshes)
            {
                if (lod > -1 && meshGroup.LOD != lod) continue;
                if (group > -1 && meshGroup.Group != group) continue;

                foreach (var (name, _, indexList) in meshes)
                {
                    skinNode.Children.Add(root.Nodes.Count);
                    var fullname = $"{name}_Level{meshGroup.LOD}_Group{meshGroup.Group}";
                    root.Nodes.Add(new GLTFNode
                    {
                        Mesh = root.Meshes.Count,
                        Skin = skinId,
                        Name = fullname
                    });
                    var mesh = new GLTFMesh
                    {
                        Name = fullname
                    };
                    root.Meshes.Add(mesh);
                    foreach (var submeshIndex in indexList)
                    {
                        var submesh = subMeshes.SubMeshes.ElementAt(submeshIndex);
                        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                        var format = submesh.Format switch
                        {
                            SubMeshFormat.Triangle => GLTFDrawMode.Triangles,
                            SubMeshFormat.Strip => GLTFDrawMode.TriangleStrip,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        var ibo = iboDeconstructed[submesh.BufferIndex];
                        var vbo = vboDeconstructed[submesh.BufferIndex];
                        var vboAttributes = vbo.ToDictionary(x => x.Key, y => y.Value);
                        if (skeleton != null)
                        {
                            var joints = jointsData[submesh.BufferIndex];
                            var boneList = bones.Bones[submesh.BoneTableIndex];
                            var newJoints = new List<Vector4Short>();
                            var (vboInfo, _) = vbos.Buffers[submesh.BufferIndex];
                            for (var index = 0; index < vboInfo.Count; index++)
                            {
                                if (index < submesh.VertexOffset || index - submesh.VertexOffset > submesh.VertexCount)
                                {
                                    newJoints.Add(new Vector4Short
                                    {
                                        X = 0,
                                        Y = 0,
                                        Z = 0,
                                        W = 0
                                    });
                                    continue;
                                }

                                if (joints.Count > index)
                                {
                                    var origJoint = joints[index];
                                    var joint = new Vector4Short
                                    {
                                        X = origJoint.X,
                                        Y = origJoint.Y,
                                        Z = origJoint.Z,
                                        W = origJoint.W
                                    };

                                    joint.X = joint.X > -1 ? boneList[joint.X / 3 % boneList.Length].Bone : (short) 0;
                                    joint.Y = joint.Y > -1 ? boneList[joint.Y / 3 % boneList.Length].Bone : (short) 0;
                                    joint.Z = joint.Z > -1 ? boneList[joint.Z / 3 % boneList.Length].Bone : (short) 0;
                                    joint.W = joint.W > -1 ? boneList[joint.W / 3 % boneList.Length].Bone : (short) 0;
                                    newJoints.Add(joint);
                                }
                                else
                                {
                                    newJoints.Add(new Vector4Short
                                    {
                                        X = boneList[0].Bone
                                    });
                                }
                            }

                            var bytes = MemoryMarshal.Cast<Vector4Short, byte>(newJoints.ToArray());

                            var bufferView = new GLTFBufferView
                            {
                                Buffer = gltfBufferId,
                                ByteOffset = (uint) bufferChunks.Length,
                                ByteLength = (uint) bytes.Length,
                                ByteStride = (uint) (bytes.Length / newJoints.Count)
                            };
                            var temp = new Span<byte>(new byte[(bufferChunks.Length + bytes.Length).Align(4)]);
                            bufferChunks.CopyTo(temp);
                            bytes.CopyTo(temp.Slice(bufferChunks.Length));
                            bufferChunks = temp;
                            var bufferViewId = root.BufferViews.Count;

                            var accessorId = root.Accessors.Count;
                            var accessor = new GLTFAccessor
                            {
                                BufferView = bufferViewId,
                                ComponentType = semanticToComponentType["JOINTS_0"],
                                Type = semanticToAttributeType["JOINTS_0"],
                                Count = (uint) newJoints.Count
                            };

                            root.Accessors.Add(accessor);
                            root.BufferViews.Add(bufferView);
                            vboAttributes["JOINTS_0"] = accessorId;
                        }

                        var iboAccessor = root.Accessors.Count;
                        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                        var width = submesh.Format switch
                        {
                            SubMeshFormat.Strip => 2,
                            SubMeshFormat.Triangle => 3,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        root.Accessors.Add(new GLTFAccessor
                        {
                            BufferView = ibo,
                            ComponentType = GLTFComponentType.UnsignedShort,
                            Type = GLTFAccessorAttributeType.SCALAR,
                            ByteOffset = (uint) (submesh.FaceOffset * width),
                            Count = (uint) submesh.FaceCount
                        });
                        var primitive = new GLTFMeshPrimitive
                        {
                            Attributes = vboAttributes,
                            Indices = iboAccessor,
                            Mode = format,
                            Material = materialIds[submesh.MaterialIndex]
                        };
                        mesh.Primitives.Add(primitive);
                    }
                }
            }

            if (skeleton != null) skinNode.Children.AddRange(rootBones);


            using var file = File.OpenWrite(bufferPath);
            file.SetLength(0);
            file.Write(bufferChunks);
            gltfBuffer.ByteLength = (uint) bufferChunks.Length;

            return root;
        }

        private static bool GetTexture(string texBase, GLTFRoot root, ModelGeometryTextureSet[] textureSet, Dictionary<int, int> textureIds, TextureKind kind, int samplerId, out int texture, out ModelGeometryTextureSet set)
        {
            if (textureSet.Any(x => x.Kind == kind))
            {
                var index = textureSet.First(x => x.Kind == kind);
                if (!textureIds.TryGetValue(index.Index, out var textureId))
                {
                    textureId = root.Textures.Count;
                    root.Textures.Add(new GLTFTexture
                    {
                        Sampler = samplerId,
                        Name = index.Index.ToString("X4"),
                        Source = root.Images.Count
                    });
                    root.Images.Add(new GLTFImage
                    {
                        Uri = $"{texBase}/{index.Index:X4}.tif"
                    });
                    textureIds[index.Index] = textureId;
                }

                texture = textureId;
                set = index;
                return true;
            }

            texture = -1;
            set = default;
            return false;
        }
    }
}
