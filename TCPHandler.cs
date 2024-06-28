using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace mySimplSharpProLib
{
    public partial class ControlSystem
    {
        private void TCPHandlerDigital(SigEventArgs args)
        {
            if (args.Sig.BoolValue)
            {
                switch ((JoinNumbers.Digital)args.Sig.Number)
                {
                    case JoinNumbers.Digital.Button1:
                    {
                        _tcpClient.Interlock(JoinNumbers.Digital.Button1, JoinNumbers.Digital.Button1, JoinNumbers.Digital.Button3, _programTen);
                        CrestronConsole.PrintLine("Appui digital 1");
                        break;
                    }
                    case JoinNumbers.Digital.Button2:
                    {
                        _tcpClient.Interlock(JoinNumbers.Digital.Button2, JoinNumbers.Digital.Button1, JoinNumbers.Digital.Button3, _programTen);
                        CrestronConsole.PrintLine("Appui digital 2");
                        break;
                    }
                    case JoinNumbers.Digital.Button3:
                    {
                        _tcpClient.Interlock(JoinNumbers.Digital.Button3, JoinNumbers.Digital.Button1, JoinNumbers.Digital.Button3, _programTen);
                        CrestronConsole.PrintLine("Appui digital 3");
                        break;
                    }
                }
            }
        }
    }
}