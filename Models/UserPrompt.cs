/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2019 UX Digital Systems Ltd
 *
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.Models
{
    public class UserPrompt
    {
        #region Fields
        
        private PromptState _state;
        private CTimer _timeOutTimer;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal UserPrompt()
        {
            Response = new PromptResponse();
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event PromptStateChangedEventHandler StateChanged;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public RoomBase Room { get; internal set; }
        public string Title { get; internal set; }
        public string SubTitle { get; internal set; }
        public uint TimeOutInSeconds { get; internal set; }
        public uint SecondsRemaining { get; private set; }
        public object UserDefinedObject { get; internal set; }
        public List<PromptAction> Actions { get; internal set; }
        public PromptUsersResponse CallBack { get; internal set; }
        public uint CustomSubPageJoin { get; internal set; }
        public PromptResponse Response { get; private set; }

        public PromptState State    
        {
            get { return _state; }
            internal set
            {
                if(_state == value) return;

                _state = value;

                if (_state == PromptState.Shown)
                {
                    SecondsRemaining = TimeOutInSeconds;

                    if (SecondsRemaining > 0)
                    {
                        _timeOutTimer = new CTimer(TimeOutTimerStep, null, 1000, 1000);
                    }
                }
                else
                {
                    SecondsRemaining = 0;
                }

                if (_state == PromptState.TimedOut)
                {
                    _timeOutTimer.Stop();
                    _timeOutTimer.Dispose();
                }

                if (StateChanged == null) return;
                try
                {
                    StateChanged(this, value);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
        }

        private void TimeOutTimerStep (object userSpecific)
        {
            if(SecondsRemaining == 0) return;

            if (SecondsRemaining > 0)
                SecondsRemaining --;

            if (SecondsRemaining == 0)
            {
                CallBack(this);
                State = PromptState.TimedOut;
            }
        }

        #endregion

        #region Methods

        public void Respond(PromptAction action)
        {
            if (State != PromptState.Shown) return;
            
            try
            {
                Response = new PromptResponse(action);
                CallBack(this);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            State = PromptState.Actioned;
        }

        public void Cancel()
        {
            State = PromptState.Cancelled;
        }

        #endregion
    }

    public class PromptAction
    {
        public string ActionName { get; set; }
        public string IconName { get; set; }
        public PromptActionType ActionType { get; set; }
    }

    public class PromptResponse
    {
        internal PromptResponse(PromptAction action)
        {
            Action = action;
            Responded = true;
        }

        internal PromptResponse()
        {
            Responded = false;
            Action = new PromptAction()
            {
                ActionName = string.Empty,
                IconName = string.Empty,
                ActionType = PromptActionType.Cancel
            };
        }

        public bool Responded { get; private set; }
        public PromptAction Action { get; private set; }
    }

    public enum PromptActionType
    {
        Acknowledge,
        Cancel,
        Answer,
        Reject
    }

    public enum PromptState
    {
        Queued,
        Shown,
        Actioned,
        TimedOut,
        Cancelled
    }

    /// <summary>
    /// A response callback delegate
    /// </summary>
    /// <param name="prompt">The prompt instance</param>
    public delegate void PromptUsersResponse(UserPrompt prompt);

    public delegate void PromptStateChangedEventHandler(UserPrompt prompt, PromptState state);
}