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
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.Sockets
{
    public abstract class TCPClientSocketBase
    {

        #region Fields

        private readonly string _address;
        private readonly TCPClient _client;
        private Thread _thread;
        private bool _remainConnected;
        private EthernetAdapterType _adapterType;

        #endregion

        #region Constructors

        protected TCPClientSocketBase(string address, int port, int bufferSize)
        {
            _address = address;
            _client = new TCPClient(address, port, bufferSize) {Nagle = true};
            _client.SocketStatusChange += OnSocketStatusChange;
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping) Disconnect();
            };
            CrestronEnvironment.EthernetEventHandler += args =>
            {
                if (args.EthernetAdapter != _adapterType) return;
                switch (args.EthernetEventType)
                {
                    case eEthernetEventType.LinkDown:
                        _client.HandleLinkLoss();
                        break;
                    case eEthernetEventType.LinkUp:
                        _client.HandleLinkUp();
                        break;
                }
            };
        }

        private void OnSocketStatusChange(TCPClient myTcpClient, SocketStatus clientSocketStatus)
        {
            CloudLog.Debug("{0} Status = {1}", GetType(), clientSocketStatus);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event TCPClientSocketStatusEventHandler StatusChanged;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public bool Connected
        {
            get { return _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED; }
        }

        public string HostAddress
        {
            get { return _address; }
        }

        protected TCPClient Client
        {
            get { return _client; }
        }

        #endregion

        #region Methods

        public void Connect()
        {
            if (_thread != null && _thread.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                CloudLog.Warn("{0}.Connect() already called and is {1}", GetType(),
                    Connected ? "Connected" : "Connecting");
                return;
            }
            
            _remainConnected = true;

            _thread = new Thread(ConnectionThreadProcess, null)
            {
                Priority = Thread.eThreadPriority.UberPriority,
                Name = string.Format("{0} Handler Thread", GetType().Name)
            };
        }

        public void Disconnect()
        {
            _remainConnected = false;
            if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                _client.DisconnectFromServer();
            else if(_thread != null && _thread.ThreadState == Thread.eThreadStates.ThreadRunning)
                _thread.Abort();
        }

        protected SocketErrorCodes Send(byte[] bytes, int index, int count)
        {
            var result = _client.SendData(bytes, index, count);
            if (result != SocketErrorCodes.SOCKET_OK)
            {
                CloudLog.Warn("Could not send data in {0}, {1}", GetType().Name, result);
            }
            return result;
        }

        protected void Send(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            Send(bytes, 0, bytes.Length);
        }

        protected abstract void OnConnect();

        protected abstract void OnDisconnect();

        protected abstract void OnReceive(byte[] buffer, int count);

        protected virtual void OnStatusChanged(SocketStatusEventType eventtype)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, eventtype);
        }

        private object ConnectionThreadProcess(object o)
        {
            try
            {
                while (true)
                {
                    var connectCount = 0;
                    while (_remainConnected && !Connected)
                    {
                        if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_LINK_LOST)
                        {
                            Thread.Sleep(5000);
                            continue;
                        }

                        connectCount++;
                        
                        var result = _client.ConnectToServer();
                        if (result == SocketErrorCodes.SOCKET_OK)
                        {
                            CrestronConsole.PrintLine("{0} connected to {1}", GetType().Name,
                                _client.AddressClientConnectedTo);

                            try
                            {
                                OnConnect();
                                OnStatusChanged(SocketStatusEventType.Connected);
                            }
                            catch (Exception e)
                            {
                                CloudLog.Exception(e);
                            }
                            break;
                        }

                        if (connectCount <= 2 || connectCount > 5) continue;
                        if (connectCount == 5)
                            CloudLog.Error("{0} failed to connect to address: {1}, will keep trying in background",
                                GetType().Name, _client.AddressClientConnectedTo);
                        CrestronEnvironment.AllowOtherAppsToRun();
                    }

                    _adapterType = _client.EthernetAdapter;

                    while (true)
                    {
                        var dataCount = _client.ReceiveData();

                        if (dataCount <= 0)
                        {
                            CrestronConsole.PrintLine("{0} Disconnected!", GetType().Name);

                            try
                            {
                                OnStatusChanged(SocketStatusEventType.Disconnected);
                                OnDisconnect();
                            }
                            catch (Exception e)
                            {
                                CloudLog.Exception(e);
                            }

                            if (_remainConnected)
                            {
                                Thread.Sleep(2000);

                                if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_LINK_LOST)
                                {
                                    CloudLog.Warn("{0} Ethernet Link Lost! - sleeping thread until back up", GetType().Name);
                                }
                                break;
                            }

                            CrestronConsole.PrintLine("Exiting {0}", Thread.CurrentThread.Name);
                            return null;
                        }

                        //CrestronConsole.PrintLine("{0} {1} bytes in buffer", GetType().Name, dataCount);

                        try
                        {
                            OnReceive(_client.IncomingDataBuffer, dataCount);
                        }
                        catch (Exception e)
                        {
                            CloudLog.Exception(e);
                        }

                        CrestronEnvironment.AllowOtherAppsToRun();
                        Thread.Sleep(0);
                    }
                }
            }
            catch (Exception e)
            {
                if (_remainConnected)
                {
                    CloudLog.Exception("Error in handler thread while connected / connecting", e);
                }
                return null;
            }
        }

        #endregion
    }

    public delegate void TCPClientSocketStatusEventHandler(TCPClientSocketBase socket, SocketStatusEventType eventType);

    public enum SocketStatusEventType
    {
        Disconnected,
        Connected
    }
}