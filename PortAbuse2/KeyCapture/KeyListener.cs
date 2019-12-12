using System;
using System.Diagnostics;
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
            if (eve.Keys != null)
            {
                Debug.WriteLine($"KPr, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}, k_char{eve.Key}");
            }

            //Application.Current.MainWindow.Topmost = true;
        }

        private void HandleEvent(KeyDownEvent eve)
        {
            if (eve.Keys.Modifiers.HasFlag(KeyModifier.Ctrl))
            {
                if (eve.Keys.KeyPressed == 80)
                {
                    this._onTop = !this._onTop;
                    //MessageBox.Show($"TopMost {Application.Current.MainWindow.Topmost}");
                    Application.Current.MainWindow.Topmost = this._onTop;
                    if (!this._onTop)
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
                    this.OnKeyAction(KeyActionType.BlockAllToggle);
                }
            }
            //MessageBox.Show($"KDw, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}");
        }

        private void HandleEvent(KeyUpEvent eve)
        {
            Debug.Write($"KUp, mod:{eve.Keys.Modifiers}, k: {eve.Keys.KeyPressed}");
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
            this.KeyAction?.Invoke(actiontype);
        }
    }

    public enum KeyActionType
    {
        BlockAllToggle
    }
}