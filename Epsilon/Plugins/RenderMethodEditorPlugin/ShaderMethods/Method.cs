using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Shader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TagTool.Tags.Definitions.RenderMethod;

namespace RenderMethodEditorPlugin.ShaderMethods
{
    class Method
    {
        public string MethodName { get; set; }
        public string MethodOption { get; set; }
        public string MethodDescription { get; set; }
        public int MethodIndex { get; set; }
        public int OptionIndex { get; set; }


        public Method(string name, string option, string desc, int methodIndex, int optionIndex)
        {
            MethodName = ShaderStringConverter.ToPrettyFormat(name);
            MethodOption = ShaderStringConverter.ToPrettyFormat(option);
            MethodDescription = desc;
            MethodIndex = methodIndex;
            OptionIndex = optionIndex;
        }
    }

    abstract class MethodParser
    {
        public Method ParseMethod(int methodIndex, int optionIndex, IShaderGenerator generator)
        {
            if (methodIndex >= 0 && methodIndex < generator.GetMethodCount())
            {
                if (optionIndex >= 0 && optionIndex < generator.GetMethodOptionCount(methodIndex))
                {
                    return new Method(GetMethodName(methodIndex), GetOptionName(methodIndex, optionIndex), GetOptionDescription(methodIndex, optionIndex), methodIndex, optionIndex);
                }
            }
            return null;
        }

        public abstract string GetMethodName(int methodIndex);
        public abstract string GetOptionName(int methodIndex, int optionIndex);
        public abstract string GetOptionDescription(int methodIndex, int optionIndex);
        public abstract IShaderGenerator BuildShaderGenerator(List<RenderMethodOptionIndex> shaderOptions);
    }
}
