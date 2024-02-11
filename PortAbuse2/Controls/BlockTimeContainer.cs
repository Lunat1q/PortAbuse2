using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortAbuse2.Controls
{
    public class BlockTimeContainer
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged = delegate { };
        private static int _curTime;

        public static void RaiseStaticPropertyChanged([CallerMemberName] string? propName = null)
        {
            StaticPropertyChanged(null, new PropertyChangedEventArgs(propName));
        }

        public static void RaiseStaticPropertyChangedByName(string? propName = null)
        {
            StaticPropertyChanged(null, new PropertyChangedEventArgs(propName));
        }

        public static int CurrentBlockTime
        {
            get => _curTime;
            set
            {
                if (_curTime == value) return;
                _curTime = value;
                RaiseStaticPropertyChanged();
            }
        }
    }
}
