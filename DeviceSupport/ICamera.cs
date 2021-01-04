using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UX.Lib2.DeviceSupport
{
    public interface ICamera
    {
        void TiltUp();
        void TiltDown();
        void TiltStop();
        void PanLeft();
        void PanRight();
        void PanStop();
        void ZoomIn();
        void ZoomOut();
        void ZoomStop();
    }
}