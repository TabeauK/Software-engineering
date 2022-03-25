using CommunicationUtils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Player.Test.Mocks
{
    public class BasicCommunicatorMock : ICommunicator
    {
        private Queue<Message> _msgQueue;
        public event MessageReceivedEventHandler MessageReceived;
        public event StateChangedEventHandler StateChanged;


        public void Dispose()
        {
            return;
        }

        public Message GetNextMessage()
        {
            throw new System.NotImplementedException();
        }

        public CommunicatorState GetState()
        {
            throw new System.NotImplementedException();
        }

        public bool HasMessage()
        {
            return _msgQueue.Count > 0;
        }

        public void SendMessage(Message m)
        {
            throw new System.NotImplementedException();
        }

        public void TrySendMessage(Message m)
        {
            
        }
        public void EnqueueMessage(Message m)
        {
            _msgQueue.Enqueue(m);
            MessageReceived.Invoke(this, new MessageReceivedEventArgs() { NewMessage = m });
        }
    }
}
