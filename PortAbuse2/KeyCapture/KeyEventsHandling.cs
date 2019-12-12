using System;
using System.Collections.Generic;
using GlobalHook;

namespace PortAbuse2.KeyCapture
{
    public class KeyEventsHandling
    {
        private readonly KeyboardHook _hook = new KeyboardHook();
        private readonly KeyListener _listener = new KeyListener();
        private readonly Dictionary<KeyActionType, List<Action>> _subscriptions = new Dictionary<KeyActionType, List<Action>>(); 
        public KeyEventsHandling()
        {
            this._hook.EventDispatcher.EventReceived += evt => this._listener.Listen(evt);
            this._listener.KeyAction += this.ListenerKeyAction;
            this._hook.Start();
        }

        private void ListenerKeyAction(KeyActionType actionType)
        {
            if (!this._subscriptions.ContainsKey(actionType)) return;

            foreach (var action in this._subscriptions[actionType])
            {
                action();
            }
        }

        public void SignForKeyAction(KeyActionType action, Action callBack)
        {
            if (this._subscriptions.ContainsKey(action))
            {
                this._subscriptions[action].Add(callBack);
            }
            else
            {
                this._subscriptions.Add(action, new List<Action> {callBack});
            }
        }

        public void Stop()
        {
            this._hook.Stop();
        }
    }
}
