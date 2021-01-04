using System;
using System.Collections;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public class UITabBar : IEnumerable<IButton>
    {
        private readonly UIControllerWithSmartObjects _uiController;
        private readonly SmartObject _smartObject;
        internal readonly Dictionary<uint, IButton> Buttons = new Dictionary<uint, IButton>();

        public UITabBar(UIControllerWithSmartObjects uiController, SmartObject smartObject)
        {
            _uiController = uiController;
            _smartObject = smartObject;

            CloudLog.Debug("{0}.ctor for SmartObject ID: {1}", GetType(), smartObject.ID);
            try
            {
                var count = 1U;
                while (true)
                {
                    var name = string.Format("Tab Button {0} Press", count);
                    if (_smartObject.BooleanOutput.Contains(name))
                    {
                        Buttons[count] = new UIButton(_smartObject, name, string.Format("Tab Button {0} Select", count));
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                CloudLog.Debug("{0} for SmartObject ID: {1} contains {2} items", GetType(), smartObject.ID,
                    Buttons.Count);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error in {0}.ctor, {1}", GetType().Name, e.Message);
            }
        }

        public IButton this[uint index]
        {
            get { return Buttons[index]; }
        }

        public UIControllerWithSmartObjects UIController
        {
            get { return _uiController; }
        }

        public SmartObject SmartObject
        {
            get { return _smartObject; }
        }

        public IEnumerator<IButton> GetEnumerator()
        {
            return Buttons.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}