using Cethleann.Structure.Resource.Model;
using DragonLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    public class G1MGShaderParam : IG1MGSection
    {
        public ModelGeometryType Type => ModelGeometryType.ShaderParam;
        public ModelGeometrySection Section { get; }
        public List<List<(ModelGeometryShaderParam param, string name, Array values)>> ParamGroups { get; } = new List<List<(ModelGeometryShaderParam param, string name, Array values)>>();

        public G1MGShaderParam(Span<byte> data, ModelGeometrySection sectionHeader)
        {
            Section = sectionHeader;

            var offset = 0;
            for (var i = 0; i < sectionHeader.Count; ++i)
            {
                var count = MemoryMarshal.Read<int>(data.Slice(offset));
                offset += sizeof(int);
                var @params = new List<(ModelGeometryShaderParam, string, Array)>();
                for (var j = 0; j < count; ++j)
                {
                    var blockHeader = MemoryMarshal.Read<ModelGeometryShaderParam>(data.Slice(offset));
                    var localOffset = SizeHelper.SizeOf<ModelGeometryShaderParam>();
                    try
                    {
                        var block = data.Slice(offset + localOffset, blockHeader.Size - localOffset);
                        var name = block.ReadString();
                        localOffset = (name.Length + 1).Align(4);
                        var paramsBlock = block.Slice(localOffset);
                        var paramData = blockHeader.Type switch
                        {
                            ShaderType.Float32 => (MemoryMarshal.Cast<byte, float>(paramsBlock).ToArray()),
                            ShaderType.Matrix4x4x2 => (Array)MemoryMarshal.Cast<byte, Matrix4x4>(paramsBlock).ToArray(),
                            _ => throw new NotSupportedException($"Can't handle ShaderParam {blockHeader.Type:F}"),
                        };
                        @params.Add((blockHeader, name, paramData));
                    }
                    finally
                    {
                        offset += blockHeader.Size;
                    }
                }
                ParamGroups.Add(@params);
            }
        }
    }
}
