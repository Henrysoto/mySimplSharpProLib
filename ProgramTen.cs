using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace mySimplSharpProLib
{
    public class ProgramTen
    {
        public delegate void DigitalChangeEventHandler(uint deviceId, SigEventArgs args);
        public event DigitalChangeEventHandler DigitalChangeEvent;

        public delegate void AnalogChangeEventHandler(uint deviceId, SigEventArgs args);
        public event AnalogChangeEventHandler AnalogChangeEvent;

        public delegate void SerialChangeEventHandler(uint deviceId, SigEventArgs args);
        public event SerialChangeEventHandler SerialChangeEvent;
        
        public ProgramTen()
        {}

        public void Interlock(JoinNumbers.Digital setIndex, JoinNumbers.Digital startIndex,
            JoinNumbers.Digital endIndex, BasicTriList client)
        {
            ResetInterlock(startIndex, endIndex, client);
            client.BooleanInput[(ushort)setIndex].BoolValue = true;
        }

        public void ResetInterlock(JoinNumbers.Digital startIndex, JoinNumbers.Digital endIndex, BasicTriList client)
        {
            for (JoinNumbers.Digital index = startIndex; index <= endIndex; index++)
            {
                client.BooleanInput[(ushort)index].BoolValue = false;
            }
        }

        public void ClientSigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                {
                    DigitalChangeEvent(currentDevice.ID, args);
                    break;
                }
                case eSigType.UShort:
                {
                    AnalogChangeEvent(currentDevice.ID, args);
                    break;
                }
                case eSigType.String:
                {
                    SerialChangeEvent(currentDevice.ID, args);
                    break;
                }
            }
        }
    }
}