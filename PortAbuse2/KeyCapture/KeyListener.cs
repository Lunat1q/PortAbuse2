using System;
using System.Windows;
using GlobalHook.Event;

namespace PortAbuse2.KeyCapture
{
    public class KeyListener
    {
        private bool _onTop = Application.Current.MainWindow.Topmost;

        private void HandleEvent(KeyPressEvent eve)
        {
            //MessageBox.Show($"KPr, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}, k_char{eve.Key}");
            //Application.Current.MainWindow.Topmost = true;
        }

        private void HandleEvent(KeyDownEvent eve)
        {
            if (eve.Keys.Modifiers.HasFlag(KeyModifier.Ctrl) && eve.Keys.KeyPressed == 80)
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
    }
}