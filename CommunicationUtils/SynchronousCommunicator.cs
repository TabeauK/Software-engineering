using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationUtils
{
    public class SynchronousCommunicator : ICommunicator
    {
        CommunicatorState state = CommunicatorState.Unitialized;

        public event MessageReceivedEventHandler MessageReceived;
        public event StateChangedEventHandler StateChanged;

        private SynchronousCommunicator partner;
        private Queue<Message> incomingMessages = new Queue<Message>();

        public void Connect(SynchronousCommunicator communicator)
        {
            if (communicator == null)
                throw new ArgumentNullException();
            if(GetState()==CommunicatorState.Unitialized)
            {
                ChangeState(CommunicatorState.Connecting);
                partner = communicator;
                if (partner.GetConnectionFrom(this))
                {
                    ChangeState(CommunicatorState.Connected);
                }
                else
                {
                    ChangeState(CommunicatorState.Disconnected);
                }
            }
            else
                throw new InvalidOperationException("Communicator was already initialized");
        }

        private bool GetConnectionFrom(SynchronousCommunicator communicator)
        {
            if (GetState() == CommunicatorState.Unitialized)
            {
                ChangeState(CommunicatorState.Connecting);
                partner = communicator;
                ChangeState(CommunicatorState.Connected);
                return true;
            }
            else
                return false;
        }

        private void GetMessage(SynchronousCommunicator src, Message m)
        {
            if(GetState()==CommunicatorState.Connected && partner==src)
            {
                incomingMessages.Enqueue(m);
                OnMessageReceived(m);
            }
        }

        public Message GetNextMessage()
        {
            if (HasMessage())
                return incomingMessages.Dequeue();
            else
                return null;
        }

        public bool HasMessage()
        {
            return incomingMessages.Count > 0;
        }

        public void SendMessage(Message m)
        {
            if (state == CommunicatorState.Unitialized)
                throw new InvalidOperationException("Communicator hasn't been connected yet");
            if (state == CommunicatorState.Connecting)
                throw new InvalidOperationException("Communicator is still connecting");
            if (state == CommunicatorState.Disconnected)
                throw new InvalidOperationException("Connection has been already closed");
            if (m == null)
                throw new ArgumentNullException("Message cannot be null");
            partner.GetMessage(this, m);
        }
        public void TrySendMessage(Message m)
        {
            try
            {
                SendMessage(m);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            if (state != CommunicatorState.Disconnected)
            {
                if(partner!=null)
                    partner.GetDisconnection(this);
                ChangeState(CommunicatorState.Disconnected);
            }
        }

        private void GetDisconnection(SynchronousCommunicator src)
        {
            if(state!=CommunicatorState.Disconnected && src==partner)
            {
                ChangeState(CommunicatorState.Disconnected);
            }
        }

        public CommunicatorState GetState()
        {
            return state;
        }

        private void ChangeState(CommunicatorState newstate)
        {
            state = newstate;
            OnStateChanged(newstate);
        }

        protected virtual void OnMessageReceived(Message m)
        {
            if (m == null)
                throw new ArgumentNullException("Message cannot be null");
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs() { NewMessage = m });
        }
        protected virtual void OnStateChanged(CommunicatorState state)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs() { NewState = state });
        }
    }
}
