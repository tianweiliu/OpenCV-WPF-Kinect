using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using OSC.NET;

namespace OSC.NET
{
    public class OSCListener
    {

        private bool connected = false;
        public int port = 7000;
        private OSCReceiver receiver;
        private Thread update;
        private Thread thread;

        private List<OSCMessage> processQueue = new List<OSCMessage>();
        public delegate void OSCMessageReceivedHandler(OSCMessage msg);
        public static event OSCMessageReceivedHandler OSCMessageReceived;

        public OSCListener()
        {
            connect();
        }

        public OSCListener(int port)
        {
            this.port = port;
            connect();
        }

        ~OSCListener()
        {
            disconnect();
        }

        public void connect()
        {
            connected = true;

            //Main update thread
            try
            {
                update = new Thread(new ThreadStart(Update));
                update.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                connected = false;
                return;
            }

            //OscReceiver thread
            try
            {
                receiver = new OSCReceiver(port);
                thread = new Thread(new ThreadStart(listen));
                thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                connected = false;
                return;
            }
        }

        /**
         * Call update every frame in order to dispatch all messages that have come
         * in on the listener thread
         */
        public void Update()
        {
            while (connected)
            {
                //processMessages has to be called on the main thread
                //so we used a shared proccessQueue full of OSC Messages
                lock (processQueue)
                {
                    foreach (OSCMessage message in processQueue)
                    {
                        if (OSCMessageReceived != null)
                        {
                            OSCMessageReceived(message); //uses events/delegates for speed, as opposed to BroadcastMessage. Clients should subscribe to this event.
                        }
                    }
                    processQueue.Clear();
                }
            }
        }

        public void disconnect()
        {
            if (receiver != null)
            {
                receiver.Close();
            }

            receiver = null;
            connected = false;
        }

        public bool isConnected() { return connected; }

        private void listen()
        {
            while (connected)
            {
                try
                {
                    OSCPacket packet = receiver.Receive();
                    if (packet != null)
                    {
                        lock (processQueue)
                        {

                            //Debug.Log( "adding  packets " + processQueue.Count );
                            if (packet.IsBundle())
                            {
                                ArrayList messages = packet.Values;
                                for (int i = 0; i < messages.Count; i++)
                                {
                                    processQueue.Add((OSCMessage)messages[i]);
                                }
                            }
                            else
                            {
                                processQueue.Add((OSCMessage)packet);
                            }
                        }
                    }
                    else Console.WriteLine("null packet");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
