using Bright.Collections;
using Luban.Job.Cfg.RawDefs;
using Luban.Job.Common.Types;
using Luban.Job.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luban.Job.Cfg.Defs
{
    public class DefTable : CfgDefTypeBase
    {
        private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

        public DefTable(Table b)
        {
            Name = b.Name;
            Namespace = b.Namespace;
            Index = b.Index;
            ValueType = b.ValueType;
            Mode = b.Mode;
            InputFiles = b.InputFiles;
            Groups = b.Groups;
            _patchInputFiles = b.PatchInputFiles;
            Comment = b.Comment;
            Tags = DefUtil.ParseAttrs(b.Tags);
            _outputFile = b.OutputFile;
        }


        public string Index { get; private set; }

        public string ValueType { get; }

        public ETableMode Mode { get; }

        public bool IsMapTable => Mode == ETableMode.MAP;

        public bool IsOneValueTable => Mode == ETableMode.ONE;

        public bool IsSingletonTable => Mode == ETableMode.ONE;

        public bool IsListTable => Mode == ETableMode.LIST;

        public List<string> InputFiles { get; }

        private readonly Dictionary<string, List<string>> _patchInputFiles;

        private readonly string _outputFile;

        public List<string> Groups { get; }

        public TType KeyTType { get; private set; }

        public DefField IndexField { get; private set; }

        public int IndexFieldIdIndex { get; private set; }

        public TBean ValueTType { get; private set; }

        public TType Type { get; private set; }

        public bool IsUnionIndex { get; private set; }

        public List<(TType Type, DefField IndexField, int IndexFieldIdIndex)> IndexList { get; } = new();

        public bool NeedExport => Assembly.NeedExport(this.Groups);

        public string OutputDataFile => string.IsNullOrWhiteSpace(_outputFile) ? FullName.Replace('.', '_').ToLower() : _outputFile;

        public string InnerName => "_" + this.Name;

        public List<string> GetPatchInputFiles(string patchName)
        {
            return _patchInputFiles.GetValueOrDefault(patchName);
        }

        public override void Compile()
        {
            var ass = Assembly;

            foreach (var patchName in _patchInputFiles.Keys)
            {
                if (ass.GetPatch(patchName) == null)
                {
                    throw new Exception($"table:'{FullName}' patch_input patch:'{patchName}' 不存在");
                }
            }

            if ((ValueTType = (TBean)ass.CreateType(Namespace, ValueType)) == null)
            {
                throw new Exception($"table:'{FullName}' 的 value类型:'{ValueType}' 不存在");
            }

            switch (Mode)
            {
                case ETableMode.ONE:
                {
                    IsUnionIndex = false;
                    KeyTType = null;
                    Type = ValueTType;
                    break;
                }
                case ETableMode.MAP:
                {
                    IsUnionIndex = true;
                    if (!string.IsNullOrWhiteSpace(Index))
                    {
                        if (ValueTType.GetBeanAs<DefBean>().TryGetField(Index, out var f, out var i))
                        {
                            IndexField = f;
                            IndexFieldIdIndex = i;
                        }
                        else
                        {
                            throw new Exception($"table:'{FullName}' index:'{Index}' 字段不存在");
                        }
                    }
                    else if (ValueTType.Bean.HierarchyFields.Count == 0)
                    {
                        throw new Exception($"table:'{FullName}' 必须定义至少一个字段");
                    }
                    else
                    {
                        IndexField = (DefField)ValueTType.Bean.HierarchyFields[0];
                        Index = IndexField.Name;
                        IndexFieldIdIndex = 0;
                    }
                    KeyTType = IndexField.CType;
                    Type = TMap.Create(false, null, KeyTType, ValueTType, false);
                    break;
                }
                case ETableMode.LIST:
                {
                    var indexs = Index.Split('+', ',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
                    foreach (var idx in indexs)
                    {
                        if (ValueTType.GetBeanAs<DefBean>().TryGetField(idx, out var f, out var i))
                        {
                            IndexField = f;
                            IndexFieldIdIndex = i;
                            this.IndexList.Add((f.CType, f, i));
                        }
                        else
                        {
                            throw new Exception($"table:'{FullName}' index:'{idx}' 字段不存在");
                        }
                    }
                    // 如果不是 union index, 每个key必须唯一，否则 (key1,..,key n)唯一
                    IsUnionIndex = IndexList.Count > 1 && !Index.Contains(',');
                    break;
                }
                default: throw new Exception($"unknown mode:'{Mode}'");
            }
        }
    }
}
