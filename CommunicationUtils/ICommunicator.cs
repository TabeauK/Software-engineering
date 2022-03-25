using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using static CommunicationUtils.Communicator;

namespace CommunicationUtils
{
    public interface ICommunicator: IDisposable
    {
        //void Connect(string serverIP, int serverPort);
        //void Connect(TcpClient client);
        Message GetNextMessage();
        bool HasMessage();
        void SendMessage(Message m);
        void TrySendMessage(Message m);
        CommunicatorState GetState();

        event MessageReceivedEventHandler MessageReceived;
        event StateChangedEventHandler StateChanged;
    }


    public class MessageReceivedEventArgs
    {
        public Message NewMessage { get; set; }
    }

    public class StateChangedEventArgs
    {
        public CommunicatorState NewState { get; set; }
    }


    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    public delegate void StateChangedEventHandler(object sender, StateChangedEventArgs e);


    public enum CommunicatorState { Unitialized, Connecting, Connected, Disconnected }
}
