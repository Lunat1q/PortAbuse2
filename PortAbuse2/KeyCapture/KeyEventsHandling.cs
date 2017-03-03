using GlobalHook;

namespace PortAbuse2.KeyCapture
{
    public class KeyEventsHandling
    {
        private readonly KeyboardHook _hook = new KeyboardHook();
        private readonly KeyListener _listener = new KeyListener();
        public KeyEventsHandling()
        {
            _hook.EventDispatcher.EventReceived += evt => _listener.Listen(evt);
            _hook.Start();
        }

        public void Stop()
        {
            _hook.Stop();
        }
    }
}
