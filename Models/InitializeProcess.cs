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

namespace UX.Lib2.Models
{
    public class InitializeProcess
    {
        private readonly InitializeDelegate _initializeDelegate;
        private readonly string _processDescription;
        private readonly TimeSpan _timeToWaitBeforeRunningProcess;
        private readonly CheckCompleteDelegate _checkCompleteCallback;

        #region Fields
        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public InitializeProcess(InitializeDelegate initializeDelegate, string processDescription)
            : this(initializeDelegate, processDescription, TimeSpan.Zero, null)
        {

        }

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public InitializeProcess(InitializeDelegate initializeDelegate, string processDescription,
            TimeSpan timeToWaitBeforeRunningProcess)
            : this(initializeDelegate, processDescription, timeToWaitBeforeRunningProcess, null)
        {

        }

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public InitializeProcess(InitializeDelegate initializeDelegate, string processDescription,
            TimeSpan timeToWaitBeforeRunningProcess, CheckCompleteDelegate checkCompleteCallback)
        {
            _initializeDelegate = initializeDelegate;
            _processDescription = processDescription;
            _timeToWaitBeforeRunningProcess = timeToWaitBeforeRunningProcess;
            _checkCompleteCallback = checkCompleteCallback;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public InitializeDelegate InitializeDelegate
        {
            get { return _initializeDelegate; }
        }

        public string ProcessDescription
        {
            get { return _processDescription; }
        }

        public TimeSpan TimeToWaitBeforeRunningProcess
        {
            get { return _timeToWaitBeforeRunningProcess; }
        }

        public CheckCompleteDelegate CheckCompleteCallback
        {
            get { return _checkCompleteCallback; }
        }

        #endregion

        #region Methods

        #endregion
    }

    public delegate void InitializeDelegate();

    public delegate bool CheckCompleteDelegate();
}