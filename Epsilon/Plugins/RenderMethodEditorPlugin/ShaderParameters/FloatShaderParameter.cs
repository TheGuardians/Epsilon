using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class FloatShaderParameter : GenericShaderParameter
    {
        public float Value
        {
            get => Property.RealConstants[TemplateIndex].Arg0;
            set => Property.RealConstants[TemplateIndex].Arg0 = value;
        }

        public FloatShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
