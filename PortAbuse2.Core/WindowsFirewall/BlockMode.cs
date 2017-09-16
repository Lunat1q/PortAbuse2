using PortAbuse2.Core.Common;

namespace PortAbuse2.Core.WindowsFirewall
{
    public enum BlockMode
    {
        [LabelName("Block all direction")]
        BlockAll = 0,
        [LabelName("Block input packets")]
        BlockInput = 2,
        [LabelName("Block output packets")]
        BlockOutput = 4
    }
}
