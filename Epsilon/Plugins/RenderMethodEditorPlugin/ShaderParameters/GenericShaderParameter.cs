using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class GenericShaderParameter
    {
        public RenderMethod.RenderMethodPostprocessBlock Property;
        public string Name { get; set; }
        public string Description { get; set; }
        public int TemplateIndex;

        public GenericShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex)
        {
            Name = ShaderStringConverter.ToPrettyFormat(name);
            TemplateIndex = templateIndex;
            Property = property;
            Description = desc;
        }
    }
}
