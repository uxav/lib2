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
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.CrestronThread;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.Sockets
{
    public abstract class TCPServerSocketBase
    {
        private readonly int _portNumber;
        private readonly int _numberOfConnections;
        private readonly int _bufferSize;
        private TCPServer _socket;
        private bool _started;
        private Dictionary<uint, SocketStatus> _connections;
        private Dictionary<uint, Thread> _threads;
        private Thread _waitForConnectThread;
        private bool _programStopping;

        protected TCPServerSocketBase(int portNumber, int numberOfConnections, int bufferSize)
        {
            _portNumber = portNumber;
            _numberOfConnections = numberOfConnections;
            _bufferSize = bufferSize;
            CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironmentOnProgramStatusEventHandler;
        }

        public event TCPServerSocketClientConnectedEventHandler ClientConnected;

        public IEnumerable<string> CurrentConnections
        {
            get
            {
                return (from connection in _connections
                    where connection.Value == SocketStatus.SOCKET_STATUS_CONNECTED
                    select _socket.GetAddressServerAcceptedConnectionFromForSpecificClient(connection.Key));
            }
        }

        public void Start()
        {
            if (_started)
            {
                CloudLog.Warn("{0} instance on TCP port {1}, already started", GetType().Name, _socket.PortNumber);
                return;
            }

            _started = true;
            
            try
            {
                CloudLog.Notice("Starting {0} instance on TCP port {1}", GetType().Name, _portNumber);

                if (_socket == null || _socket.ServerSocketStatus == SocketStatus.SOCKET_STATUS_SOCKET_NOT_EXIST)
                {
                    CloudLog.Info("Created new socket for {0} on port {1}", GetType().Name, _portNumber);
                    _socket = new TCPServer(IPAddress.Any.ToString(), _portNumber, _bufferSize,
                        EthernetAdapterType.EthernetUnknownAdapter, _numberOfConnections);
                    _socket.SocketSendOrReceiveTimeOutInMs = 60000;
                    _socket.SocketStatusChange += SocketOnSocketStatusChange;
                    if (_waitForConnectThread == null || _waitForConnectThread.ThreadState != Thread.eThreadStates.ThreadRunning)
                    {
                        _waitForConnectThread = new Thread(ListenForConnections, null)
                        {
                            Name = string.Format("{0} Connection Handler", GetType().Name),
                            Priority = Thread.eThreadPriority.MediumPriority
                        };
                    }
                    CloudLog.Info("{0} on port {1}\r\nStatus: {2}\r\nMax Connections: {3}", GetType().Name,
                        _socket.PortNumber, _socket.State, _socket.MaxNumberOfClientSupported);
                }
                else
                {
                    CloudLog.Warn("TCP Server. Could not start. Current Socket Status:{0}", _socket.ServerSocketStatus);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Could not start socket");
            }
        }

        private object ListenForConnections(object userSpecific)
        {
            CloudLog.Notice("Started {0} connection handler", GetType().Name);

            while (!_programStopping)
            {
                CloudLog.Debug("{0} Waiting for connections...", GetType().Name);

                if (_socket.NumberOfClientsConnected < _socket.MaxNumberOfClientSupported)
                {
                    var result = _socket.WaitForConnection();
                    CloudLog.Debug("{0} Connection result: {1}", GetType().Name, result);
                }
                else
                {
                    CloudLog.Debug("{0} No more connections allowed", GetType().Name);
                    Thread.Sleep(10000);
                }
            }

            CloudLog.Notice("Exiting {0}", Thread.CurrentThread.Name);

            return null;
        }

        private void SocketOnSocketStatusChange(TCPServer myTcpServer, uint clientIndex, SocketStatus serverSocketStatus)
        {
            if (_connections == null)
            {
                _connections = new Dictionary<uint, SocketStatus>();
            }

            CloudLog.Debug("{0} Connection Status for ClientIndex {1}, {2}", GetType().Name, clientIndex, serverSocketStatus);

            _connections[clientIndex] = serverSocketStatus;

            if(serverSocketStatus != SocketStatus.SOCKET_STATUS_CONNECTED) return;

            if (_threads == null)
            {
                _threads = new Dictionary<uint, Thread>();
            }

            if (!_threads.ContainsKey(clientIndex) ||
                _threads[clientIndex].ThreadState != Thread.eThreadStates.ThreadRunning)
            {
                try
                {
                    _threads[clientIndex] = new Thread(ReceiveHandler, clientIndex)
                    {
                        Name = string.Format("{0} Rx Handler for client index {1}", GetType().Name, clientIndex),
                        Priority = Thread.eThreadPriority.HighPriority
                    };
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e, "Error starting RX Handler thread for client index {0}", clientIndex);
                    _socket.Disconnect(clientIndex);
                }
            }
        }

        private object ReceiveHandler(object userSpecific)
        {
            var clientIndex = (uint) userSpecific;

            CloudLog.Notice("{0} rx handler on TCP port {1}, is running for client index {2}", GetType().Name, _socket.PortNumber, clientIndex);

            try
            {
                OnClientConnect(clientIndex);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            while (true)
            {
                try
                {
                    var count = _socket.ReceiveData(clientIndex);

                    if (count <= 0)
                    {
                        CloudLog.Debug("{0} - Client Index {1} Received data count of {2}, Disconnected? - Exiting Thread",
                            GetType().Name, clientIndex, count);

                        try
                        {
                            OnClientDisconnect(clientIndex);
                        }
                        catch (Exception e)
                        {
                            CloudLog.Exception(e);
                        }

                        return null;
                    }

                    OnClientReceive(clientIndex, _socket.GetIncomingDataBufferForSpecificClient(clientIndex), count);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e,
                        "Error in {0} for client index {1}, closing connection and exiting rx handler thread",
                        Thread.CurrentThread.Name, clientIndex);
                    if (_connections[clientIndex] == SocketStatus.SOCKET_STATUS_CONNECTED)
                    {
                        _socket.Disconnect(clientIndex);
                        return null;
                    }
                }
            }
        }

        protected virtual void OnClientConnect(uint clientIndex)
        {
            if(ClientConnected == null) return;

            try
            {
                ClientConnected(this, clientIndex);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected abstract void OnClientReceive(uint clientIndex, byte[] dataBuffer, int count);

        protected abstract void OnClientDisconnect(uint clientIndex);

        public void SendToAll(byte[] bytes, int offset, int count)
        {
            foreach (var connection in _connections)
            {
                var clientIndex = connection.Key;
                if (connection.Value == SocketStatus.SOCKET_STATUS_CONNECTED)
                {
                    var result = _socket.SendData(clientIndex, bytes, offset, count);
                    if (result != SocketErrorCodes.SOCKET_OK)
                    {
                        CloudLog.Warn("{0} Sending data to client index {1} received error: {2}", GetType().Name,
                            clientIndex, result);
                    }
                }
            }
        }

        public SocketErrorCodes Send(uint clientIndex, byte[] bytes, int offset, int count)
        {
            if (!_connections.ContainsKey(clientIndex))
            {
                throw new Exception(string.Format("No client index exists for {0}", clientIndex));
            }

            return _socket.SendData(clientIndex, bytes, offset, count);
        }

        private void CrestronEnvironmentOnProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            _programStopping = programEventType == eProgramStatusEventType.Stopping;
            if (_programStopping)
            {
                Stop();
            }
        }

        public void Stop()
        {
            CloudLog.Notice("Stopping {0} instance on TCP port {1}", GetType().Name, _socket.PortNumber);
            if (_socket != null)
            {
                var message = Encoding.ASCII.GetBytes("Closing connections!\r\n");
                SendToAll(message, 0, message.Length);
                _socket.DisconnectAll();
            }
        }
    }

    public delegate void TCPServerSocketClientConnectedEventHandler(TCPServerSocketBase socket, uint clientId);
}