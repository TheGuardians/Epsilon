using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class Float2ShaderParameter : GenericShaderParameter
    {
        public float Value1
        {
            get => Property.RealConstants[TemplateIndex].Arg0;
            set => Property.RealConstants[TemplateIndex].Arg0 = value;
        }

        public float Value2
        {
            get => Property.RealConstants[TemplateIndex].Arg1;
            set => Property.RealConstants[TemplateIndex].Arg1 = value;
        }

        public Float2ShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
