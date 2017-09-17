using TiQWpfUtils.AbstractClasses.Attributes;
using TiQWpfUtils.Controls.Extensions.DataGrid;

namespace PortAbuse2.Core.Common
{
    public class LabelNameAttribute : LabelNameAttributeBase
    {
        public LabelNameAttribute(string label) : base(label)
        {
        }

        public override string GetProperText()
        {
            return Label;
        }
    }
}