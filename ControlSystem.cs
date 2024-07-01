using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets; // For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication; // For Generic Device Support

namespace mySimplSharpProLib
{
    public partial class ControlSystem : CrestronControlSystem
    {
        private EthernetIntersystemCommunications _programTen;
        private ProgramTen _tcpClient;

        private TCPServer _tcpServer;
        
        private const uint ProgramTenIpId = 0x03;
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(_ControllerEthernetEventHandler);
                
                // Interactions avec program10
                _programTen = new EthernetIntersystemCommunications(ProgramTenIpId, "127.0.0.2", this);
                _programTen.Description = "Program10 TCP Client";
                
                if (_programTen.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    ErrorLog.Error("Erreur lors de l'enregistrement de {0}, err {1}", _programTen.Description, _programTen.RegistrationFailureReason);
                    CrestronConsole.PrintLine("Erreur lors de l'enregistrement de {0}, err {1}", _programTen.Description, _programTen.RegistrationFailureReason);
                }

                _tcpClient = new ProgramTen();
                _tcpClient.DigitalChangeEvent += TcpClientOnDigitalChangeEvent;
                _tcpClient.AnalogChangeEvent += TcpClientOnAnalogChangeEvent;
                _tcpClient.SerialChangeEvent += TcpClientOnSerialChangeEvent;

                _programTen.SigChange += _tcpClient.ClientSigChange;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        private void TcpClientOnSerialChangeEvent(uint deviceid, SigEventArgs args)
        {
            switch (deviceid)
            {
                case ProgramTenIpId:
                {
                    if (args.Sig.Type == eSigType.String)
                    {
                        CrestronConsole.PrintLine("DEBUG Message from TCP: {0}", args.Sig.StringValue);
                    }
                    break;
                }
            }
        }

        private void TcpClientOnAnalogChangeEvent(uint deviceid, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void TcpClientOnDigitalChangeEvent(uint deviceid, SigEventArgs args)
        {
            switch (deviceid)
            {
                case ProgramTenIpId:
                {
                    TCPHandlerDigital(args);
                    break;
                }
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                // CrestronConsole.PrintLine("DEBUG: START ----------------");
                // var csv = new CSVReader();
                // csv.ReadCSV(@"\html\test\MOCK_DATA.csv");
                
                // Écoute d'un socket tcp
                _tcpServer = new TCPServer("0.0.0.0", 25000, 2048, EthernetAdapterType.EthernetCSAdapter);
                _tcpServer.WaitForConnectionAsync(OnConnect);
                _tcpServer.SocketStatusChange += TcpServerStatusChangeEvent;
                CrestronConsole.PrintLine("DEBUG TCP Server: {0} {1} {2} {3}", _tcpServer.State, _tcpServer.PortNumber, 
                    _tcpServer.ServerSocketStatus, _tcpServer.NumberOfClientsConnected);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
        
        private void OnConnect(TCPServer server, uint clientId)
        {
            CrestronConsole.PrintLine("DEBUG TCP Server: ClientID {0} connected !", clientId);
            byte[] testmsg = Encoding.ASCII.GetBytes("proutpacket");
            if (server.SendData(clientId, testmsg, testmsg.Length) != SocketErrorCodes.SOCKET_OK)
            {
                CrestronConsole.PrintLine("DEBUG TCP Server: ClientID {0} erreur !", clientId);    
            }
            // server.Disconnect(clientId);
            // CrestronConsole.PrintLine("DEBUG TCP Server: ClientID {0} disconnected !", clientId);
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// which Ethernet adapter this event belongs to.
        /// </param>
        void _ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void _ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void _ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}