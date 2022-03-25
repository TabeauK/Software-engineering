using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationUtils
{
    /* TUTORIAL KOMUNIKATORA
       1. Tworzysz obiekt typu Communicator (planujemy zrobić go za pomocą dependency injection)
       2. Łączysz się z serwerem przez Connect(string ip, int port), CS używa Connect(TcpClient)
       3. Czekasz aż komunikator się połączy (Connect tworzy nowy wątek, aby dowiedzieć się o pomyślnym połączeniu
            należy podłączyć się pod event MessageReceived lub wywoływać GetState())
       4. Wysyłasz wiadomości metodą SendMessage(Message m), obiekt typu Message tworzysz samemu wraz z IPayloadem 
            zależnym od danego typu wiadomości. W wiadomości nie musisz podawać jej id, właściwość sama się uzupełni
            podczas wysyłania. W przypadku odbioru wiadomości komunikator gwarantuje wypełniony payload,
            o ile wiadomość ma znany typ. W przeciwnym wypadku payload jest nullem (wiadomość nieznanego typu)
            Moduły nie muszą, i nie jest to zalecane, korzystać z właściwości messageID, a jedynie z payloadów
       4. Odbierasz wiadomości poprzez wywołanie GetNextMessage(). Metoda zwraca nulla jeżeli żadnej wiadomości
            nie było. Możesz też sprawdzić, czy jest wiadomość poprzez HasMessage(). Uwaga: GetNextMessage zwraca 
            wiadomość i usuwa ją z komunikatora, więc następne wywołanie GetNextMessage zwraca następną wiadomość.
            Zalecany sposób odbioru wiadomości:
            - podłącz się pod handler MessageReceived
            - w funkcji obsługującej zdarzenie wykonuj pętlę dopóki GetNextMessage nie zwraca nulla i 
            jednocześnie obsługuj zwracane przez tę metodę wiadomości. Korzystanie z HasMessage() powinno być
            uzasadnione i przemyślane, nie jest zalecane.
       5. Do monitorowania stanu komunikatora używa się eventów. Przy zmianie stanu komunikatora wywoływany jest
            StateChanged. Stany to: niezainicjalizowany, łączy się, połączony, rozłączony. Tak też wygląda cykl 
            życia komunikatora. Raz rozłączony komunikator nie może zostać użyty ponownie do wysyłania wiadomości. 
            Stany opisuje enum CommunicatorState. StateChanged w event argsach mówi, w jaki stan wszedł komunikator.
            Wysyłanie wiadomości, gdy komunikator nie jest w stanie Connected spowoduje zgłoszenie wyjątku!
            Można też odpytać komunikator o jego stan poprzez GetState()
       6. Dodatkowo można podpiąć się pod event MessageReceived, który jest wywoływany przy otrzymaniu przez 
            komunikator nowej wiadomości dostępnej w event argsach. Ważne, że odebranie wiadomości eventem nie oznacza,
            że znika ona z komunikatora. I tak trzeba ją wyjąć przez GetNextMessage(), ale nie ma gwarancji,
            że jest to pierwsza wiadomość jaka będzie otrzymana (jeżeli jakaś wiadomość przyszła wcześniej
            i nie była wyjęta).
       7. Zamykamy połączenie komunikatora metodą Dispose()

       Uwaga: Gdy komunikator nie dostanie wiadomości zgodnej ze specyfikacją (2 bajty wielkości i 
       potem wiadomość długości dokładnie takiej jak zadeklarowana), całkowicie ignoruje wszystkie zalegające
       bajty aż do uzyskania pustej kolejki
     */
    public class Communicator: ICommunicator
    {
        private string serverAddress = null;
        private int serverPort =-1;
        private CommunicatorState state = CommunicatorState.Unitialized;
        private TcpClient socket = null;
        private Task readingTask = null;
        private Queue<Message> incomingMessages = new Queue<Message>();


        public event MessageReceivedEventHandler MessageReceived;
        public event StateChangedEventHandler StateChanged;
        

        public void Connect(string serverIP, int serverPort)
        {
            if (state != CommunicatorState.Unitialized)
                throw new InvalidOperationException("Communicator was already initialized");
            if (serverIP == null)
                throw new ArgumentNullException("Server IP cannot be null");
            if (serverIP == "")
                throw new ArgumentException("Server IP cannot be empty");
            if (serverPort < IPEndPoint.MinPort || serverPort > IPEndPoint.MaxPort)
                throw new ArgumentException(String.Format("Server port must be an integer between {0} and {1}",IPEndPoint.MinPort, IPEndPoint.MaxPort));
            this.serverAddress = serverIP;
            this.serverPort = serverPort;
            ChangeState(CommunicatorState.Connecting);
            readingTask = Task.Run(()=>TryConnect());
        }

        public void Connect(TcpClient client)
        {
            if (state != CommunicatorState.Unitialized)
                throw new InvalidOperationException("Communicator was already initialized");
            if (client == null)
                throw new ArgumentNullException("Client cannot be null");
            if (!client.Connected)
                throw new ArgumentException("Client is not connected");
            this.serverAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            this.serverPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
            
            ChangeState(CommunicatorState.Connected);
            socket = client;
            readingTask = Task.Run(()=>WaitForMessages());
        }

        private void TryConnect()
        {
            try
            {
                socket = new TcpClient(this.serverAddress, this.serverPort);
                ChangeState(CommunicatorState.Connected);
            }
            catch (SocketException)
            {
                ChangeState(CommunicatorState.Disconnected);
                return;
            }
            WaitForMessages();
        }

        private void WaitForMessages()
        {
            try
            {
                while (state == CommunicatorState.Connected)
                {
                    byte[] nextMessageLengthData = new byte[2];
                    try
                    {
                        int bytesRead = socket.GetStream().Read(nextMessageLengthData, 0, 2);
                        if(bytesRead==0) //zamknięte połączenie
                        {
                            ChangeState(CommunicatorState.Disconnected);
                            socket.Close();
                            return;
                        }
                        if (bytesRead != 2)
                        {
                            //Nie mam lepszego pomysłu na opróżnienie danych przychodzących
                            while(socket.GetStream().DataAvailable)
                            {
                                socket.GetStream().Read(nextMessageLengthData, 0, 1);
                                continue;
                            }
                        }
                    }
                    catch(Exception e) when (e is SocketException|| e is IOException) //może wystąpić np. gdy zamknęliśmy klienta z jakiegoś powodu
                    {
                        socket.Close();
                        ChangeState(CommunicatorState.Disconnected);
                        return;
                    }
                    int messageSize = nextMessageLengthData[0] + 256 * nextMessageLengthData[1];
                    byte[] message = new byte[messageSize];
                    if (socket.GetStream().Read(message, 0, messageSize) < messageSize)
                    {
                        //Nie mam lepszego pomysłu na opróżnienie danych przychodzących
                        while (socket.GetStream().DataAvailable)
                        {
                            socket.GetStream().Read(nextMessageLengthData, 0, 1);
                            continue;
                        }
                    }
                    string json = Encoding.UTF8.GetString(message);
                    Message m = MessageParser.ParseJson(json);
                    incomingMessages.Enqueue(m);
                    OnMessageReceived(m);
                }
            }
            catch (SocketException)
            {
                ChangeState(CommunicatorState.Disconnected);
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
                throw new InvalidOperationException("Communicator is still connecting to the server");
            if (state == CommunicatorState.Disconnected)
                throw new InvalidOperationException("Connection has been already closed");
            if (m == null)
                throw new ArgumentNullException("Message cannot be null");
            string json = MessageParser.ParseMessage(m);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] socketMessage = new byte[bytes.Length + 2];
            Array.Copy(bytes, 0, socketMessage, 2, bytes.Length);
            socketMessage[0] = (byte) (bytes.Length % 256);
            socketMessage[1] = (byte) (bytes.Length / 256);
            try
            {
                socket.GetStream().Write(socketMessage, 0, socketMessage.Length);
            }
            catch(Exception)
            {
                ChangeState(CommunicatorState.Disconnected);
                throw new InvalidOperationException("Connection has been closed");
            }
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
            if(socket!=null)
                socket.Close();
            ChangeState(CommunicatorState.Disconnected);
            incomingMessages.Clear();
        }

        public CommunicatorState GetState()
        {
            return state;
        }

        private void ChangeState(CommunicatorState newstate)
        {
            if (state != newstate)
            {
                state = newstate;
                OnStateChanged(newstate);
            }
        }

        protected virtual void OnMessageReceived(Message m)
        {
            if (m == null)
                throw new ArgumentNullException("Message cannot be null");
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(){ NewMessage = m });
        }
        protected virtual void OnStateChanged(CommunicatorState state)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs(){NewState = state });
        }
    }
}
