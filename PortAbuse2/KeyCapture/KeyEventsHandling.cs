using System;
using System.Collections;
using System.Collections.Generic;
using GlobalHook;

namespace PortAbuse2.KeyCapture
{
    public class KeyEventsHandling
    {
        private readonly KeyboardHook _hook = new KeyboardHook();
        private readonly KeyListener _listener = new KeyListener();
        private Dictionary<KeyActionType, List<Action>> _subscriptions = new Dictionary<KeyActionType, List<Action>>(); 
        public KeyEventsHandling()
        {
            _hook.EventDispatcher.EventReceived += evt => _listener.Listen(evt);
            _listener.KeyAction += _listener_KeyAction;
            _hook.Start();
        }

        private void _listener_KeyAction(KeyActionType actionType)
        {
            if (!_subscriptions.ContainsKey(actionType)) return;

            foreach (var action in _subscriptions[actionType])
            {
                action();
            }
        }

        public void SignForKeyAction(KeyActionType action, Action callBack)
        {
            if (_subscriptions.ContainsKey(action))
            {
                _subscriptions[action].Add(callBack);
            }
            else
            {
                _subscriptions.Add(action, new List<Action> {callBack});
            }
        }

        public void Stop()
        {
            _hook.Stop();
        }
    }
}
