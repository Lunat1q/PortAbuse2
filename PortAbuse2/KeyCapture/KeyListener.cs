using System;
using System.Windows;
using GlobalHook.Event;

namespace PortAbuse2.KeyCapture
{
    public class KeyListener
    {
        private bool _onTop = Application.Current.MainWindow.Topmost;

        internal delegate void KeyActionHandler(KeyActionType actionType);

        internal event KeyActionHandler KeyAction;

        private void HandleEvent(KeyPressEvent eve)
        {
            //MessageBox.Show($"KPr, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}, k_char{eve.Key}");
            //Application.Current.MainWindow.Topmost = true;
        }

        private void HandleEvent(KeyDownEvent eve)
        {
            if (eve.Keys.Modifiers.HasFlag(KeyModifier.Ctrl))
            {
                if (eve.Keys.KeyPressed == 80)
                {
                    _onTop = !_onTop;
                    //MessageBox.Show($"TopMost {Application.Current.MainWindow.Topmost}");
                    Application.Current.MainWindow.Topmost = _onTop;
                    if (!_onTop)
                    {
                        WindowHandler.SendWpfWindowBack(Application.Current.MainWindow);
                    }
                    else
                    {
                        Application.Current.MainWindow.Focus();
                    }
                }
                else if (eve.Keys.KeyPressed == 66)
                {
                    OnKeyAction(KeyActionType.BlockAllToggle);
                }
            }
            //MessageBox.Show($"KDw, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}");
        }

        private void HandleEvent(KeyUpEvent eve)
        {
            //MessageBox.Show($"KUp, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}");
        }

        public void Listen(IEvent evt)
        {
            try
            {
                dynamic eve = evt;
                HandleEvent(eve);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected virtual void OnKeyAction(KeyActionType actiontype)
        {
            KeyAction?.Invoke(actiontype);
        }
    }

    public enum KeyActionType
    {
        BlockAllToggle
    }
}