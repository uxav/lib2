using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UX.Lib2.DeviceSupport.Relays
{
    public class UpDownRelays : IHoistController
    {
        private readonly BoolInputSig _upRelayClosedSig;
        private readonly BoolInputSig _downRelayClosedSig;
        private readonly Relay _upRelay;
        private readonly Relay _downRelay;
        public UpDownRelayModeType ModeType { get; protected set; }
        public UpDownRelayState State { get; protected set; }
        private CTimer _waitTimer;
        private long _pulseTime = 500;

        public UpDownRelays(Relay upRelay, Relay downRelay, UpDownRelayModeType modeType)
        {
            _upRelay = upRelay;
            _downRelay = downRelay;
            ModeType = modeType;
            State = UpDownRelayState.Unknown;
        }

        public UpDownRelays(BoolInputSig upRelayClosedSig, BoolInputSig downRelayClosedSig,
            UpDownRelayModeType modeType)
        {
            _upRelayClosedSig = upRelayClosedSig;
            _downRelayClosedSig = downRelayClosedSig;
            ModeType = modeType;
        }

        public UpDownRelays(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, UpDownRelayModeType modeType)
            : this(new Relay(upRelay), new Relay(downRelay), modeType) { }

        public void Up()
        {
            Up(_pulseTime);
        }

        public void Up(long pulseTime)
        {
            if (_waitTimer != null)
            {
                _waitTimer.Dispose();
                _waitTimer = null;
            }

            if (_downRelayClosedSig != null && _downRelayClosedSig.BoolValue)
            {
                _downRelayClosedSig.BoolValue = false;
                _waitTimer = new CTimer(RelaySet, _upRelayClosedSig, 500);
            }
            else if (_upRelayClosedSig != null)
            {
                RelaySet(_upRelayClosedSig);
            }
            else if (_downRelay != null && _downRelay.State)
            {
                _downRelay.Open();
                _waitTimer = new CTimer(RelaySet, _upRelay, 500);
            }
            else if(_upRelay != null)
            {
                RelaySet(_upRelay);
            }

            State = UpDownRelayState.Up;
        }

        public void Down()
        {
            Down(_pulseTime);
        }

        public void Down(long pulseTime)
        {
            if (_waitTimer != null)
            {
                _waitTimer.Dispose();
                _waitTimer = null;
            }

            if (_upRelayClosedSig != null && _upRelayClosedSig.BoolValue)
            {
                _upRelayClosedSig.BoolValue = false;
                _waitTimer = new CTimer(RelaySet, _downRelayClosedSig, 500);
            }
            else if (_downRelayClosedSig != null)
            {
                RelaySet(_downRelayClosedSig);
            }
            else if (_upRelay.State)
            {
                _upRelay.Open();
                _waitTimer = new CTimer(RelaySet, _downRelay, 500);
            }
            else
            {
                RelaySet(_downRelay);
            }

            State = UpDownRelayState.Down;
        }

        public void Stop()
        {
            if (_downRelayClosedSig != null)
            {
                _downRelayClosedSig.BoolValue = false;
                _upRelayClosedSig.BoolValue = false;
                State = UpDownRelayState.Unknown;
                return;
            }

            _upRelay.Open();
            _downRelay.Open();

            State = UpDownRelayState.Unknown;
        }

        public void StopUsingPulseBoth()
        {
            StopUsingPulseBoth(_pulseTime);
        }

        public void StopUsingPulseBoth(long pulseTime)
        {
            if (_waitTimer != null)
            {
                _waitTimer.Dispose();
                _waitTimer = null;
            }

            if (_downRelayClosedSig != null)
            {
                _downRelayClosedSig.BoolValue = true;
                _upRelayClosedSig.BoolValue = true;
            }
            else
            {
                _upRelay.Close();
                _downRelay.Close();
            }

            var timer = new CTimer(specific => Stop(), pulseTime);
        }

        void RelaySet(object obj)
        {
            var sig = obj as BoolInputSig;
            if (sig != null)
            {
                switch (ModeType)
                {
                    case UpDownRelayModeType.Momentary:
                        sig.Pulse((int) _pulseTime);
                        break;
                    case UpDownRelayModeType.Interlocked:
                        sig.BoolValue = true;
                        break;
                }
                return;
            }

            if (!(obj is Relay)) return;
            var relay = obj as Relay;
            switch (ModeType)
            {
                case UpDownRelayModeType.Momentary:
                    relay.Pulse((int) _pulseTime);
                    break;
                case UpDownRelayModeType.Interlocked:
                    relay.Close();
                    break;
            }
        }

        public void SetPulseTimeInMs(long ms)
        {
            _pulseTime = ms;
        }

        public void Register()
        {
            if(_upRelay == null) return;

            if (_upRelay.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.UpRelay with ID {0}", _upRelay.CrestronRelay.ID);
            if (_downRelay.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.DownRelay with ID {0}", _downRelay.CrestronRelay.ID);
        }
    }

    public enum UpDownRelayModeType
    {
        Momentary,
        Interlocked
    }

    public enum UpDownRelayState
    {
        Up,
        Down,
        Unknown
    }
}