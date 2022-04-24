using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TagStructEditor.Common;
using TagStructEditor.Helpers;

namespace TagStructEditor.Fields
{
    public class StructField : IField
    {
        private bool _isExpanded = true;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public IField Parent { get; set; }

        public ObservableCollection<IField> Fields { get; set; }

        public StructField()
        {
            Fields = new ObservableCollection<IField>();
            ExpandAllCommand = new DelegateCommand(ExpandChildren);
            CollapseAllCommand = new DelegateCommand(CollapseChildren);
        }

        public DelegateCommand ExpandAllCommand { get; set; }
        public DelegateCommand CollapseAllCommand { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
            }
        }

        public void AddChild(IField child)
        {
            child.Parent = this;
            Fields.Add(child);
        }

        public void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Populate(object owner, object value = null)
        {
            if (value == null)
                value = owner;
                
            foreach (var field in Fields)
                field.Populate(value);
        }

        private void ExpandChildren()
        {
            FieldExpander.Expand(this, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Expand);
        }

        private void CollapseChildren()
        {
            FieldExpander.Expand(this, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Collapse);
        }
    }
}
