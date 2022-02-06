using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using HaloShaderGenerator.Generator;
using RenderMethodEditorPlugin.ShaderMethods;
using RenderMethodEditorPlugin.ShaderMethods.Particle;
using RenderMethodEditorPlugin.ShaderMethods.Shader;
using RenderMethodEditorPlugin.ShaderMethods.Halogram;
using RenderMethodEditorPlugin.ShaderMethods.Decal;
using RenderMethodEditorPlugin.ShaderMethods.Screen;
using RenderMethodEditorPlugin.ShaderParameters;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;
using System.ComponentModel;

namespace RenderMethodEditorPlugin
{
    class RenderMethodEditorViewModel : TagEditorPluginBase
    {
        private RenderMethod _renderMethod;
        private GameCache _cache;
        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethod.RenderMethodPostprocessBlock _shaderProperty;

        public ObservableCollection<BooleanConstant> BooleanConstants { get; private set;  } = new ObservableCollection<BooleanConstant>();
        public ObservableCollection<Method> ShaderMethods { get; private set; } = new ObservableCollection<Method>();
        public ObservableCollection<GenericShaderParameter> ShaderParameters { get; private set; } = new ObservableCollection<GenericShaderParameter>();

        public RenderMethodEditorViewModel(GameCache cache, RenderMethod renderMethod)
        {
            _cache = cache;
            Load(cache, renderMethod);
        }

        private void Load(GameCache cache, RenderMethod renderMethod)
        {
            _renderMethod = renderMethod;
            _shaderProperty = _renderMethod.ShaderProperties[0];
            var templateName = _shaderProperty.Template.Name;

            if (templateName == null)
                return;

            var templateTypeSplitIndex = 1;
            if (templateName.Contains("ms30"))
                templateTypeSplitIndex = 2;

            string templateType = templateName.Split('\\')[templateTypeSplitIndex];

            MethodParser methodParser;
            IShaderGenerator generator;

            using (var stream = _cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);
            }

            switch (templateType)
            {
                case "shader_templates":
                    methodParser = new ShaderMethod();
                    break;
                case "particle_templates":
                    methodParser = new ParticleMethod();
                    break;
                case "halogram_templates":
                    methodParser = new HalogramMethod();
                    break;
                case "decal_templates":
                    methodParser = new DecalMethod();
                    break;
                case "screen_templates":
                    methodParser = new ScreenMethod();
                    break;
                default:
                    return;
            }

            generator = methodParser.BuildShaderGenerator(_renderMethod.Options);

            ShaderMethods = new ObservableCollection<Method>();
            for (int i = 0; i < _renderMethod.Options.Count; i++)
            {
                var optionIndex = _renderMethod.Options[i].OptionIndex;

                var methodInfo = methodParser.ParseMethod(i, optionIndex, generator);
                if (methodInfo != null)
                    ShaderMethods.Add(methodInfo);
            }

            bool useRotation = false;
            if (generator is HaloShaderGenerator.Shader.ShaderGenerator && _renderMethod.Options[9].OptionIndex == 3)
                useRotation = true;
            var parameters = generator.GetPixelShaderParameters().Parameters;
            parameters.AddRange(generator.GetVertexShaderParameters().Parameters);
            ShaderParameters = ShaderParameterFactory.BuildShaderParameters(cache, parameters, _shaderProperty, _renderMethodTemplate, useRotation);

            this.NotifyOfPropertyChange(nameof(ShaderMethods));
            this.NotifyOfPropertyChange(nameof(ShaderParameters));
        }

        // get boolean values from existing tag data

        private void ParseBooleanArguments()
        {
            for(int i = 0; i < _renderMethodTemplate.BooleanParameterNames.Count; i++)
            {
                string name = _cache.StringTable.GetString(_renderMethodTemplate.BooleanParameterNames[i].Name);
                BooleanConstants.Add(new BooleanConstant(_shaderProperty, name, FindDescriptionFromName(name), i));
            }
        }

        private string FindDescriptionFromName(string shaderArgName)
        {
            if(ShaderArgumentsDescription.ArgsDescription.ContainsKey(shaderArgName))
                return ShaderArgumentsDescription.ArgsDescription[shaderArgName];
            else
                return "Missing description";
        }

        public void Test()
        {
            PostMessage(this, new DefinitionDataChangedEvent(_renderMethod));
        }

        protected override void OnMessage(object sender, object message)
        {
            if (message is DefinitionDataChangedEvent e)
            {
               Load(_cache, (RenderMethod)e.NewData);
            }
        }
    }

    class ShaderConstant
    {
        public RenderMethod.RenderMethodPostprocessBlock Property;
        public string Name { get; set; }
        public string Description { get; set; }
        public int TemplateIndex;
        public ShaderConstant(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex)
        {
            Name = ShaderStringConverter.ToPrettyFormat(name);
            TemplateIndex = templateIndex;
            Property = property;
            Description = desc;
        }
    }

    class BooleanConstant : ShaderConstant
    {
        public bool Value
        {
            get => (((int)(Property.BooleanConstants) >> TemplateIndex) & 1) == 1;
            set
            {
                if(value == true)
                    Property.BooleanConstants = (uint)((int)Property.BooleanConstants | (1 << TemplateIndex));
                else
                    Property.BooleanConstants = (uint)((int)Property.BooleanConstants & ~(1 << TemplateIndex));
            }
        }

        public BooleanConstant(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
