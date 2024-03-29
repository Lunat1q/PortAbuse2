﻿using TiqUtils.Wpf.AbstractClasses.Attributes;

namespace PortAbuse2.Core.Common
{
    public class LabelNameAttribute : LabelNameAttributeBase
    {
        public LabelNameAttribute(string label) : base(label)
        {
        }

        public override string GetProperText()
        {
            return this.Label;
        }
    }
}