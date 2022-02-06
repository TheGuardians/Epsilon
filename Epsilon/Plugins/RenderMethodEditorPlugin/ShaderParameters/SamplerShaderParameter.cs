using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class SamplerShaderParameter : GenericShaderParameter
    {
        private string temp;

        public string Value
        {
            get => Property.TextureConstants[TemplateIndex].Bitmap.Name;
            set => temp = value;
        }

        public SamplerShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
