﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace NetChange
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
        int conP; //connecting poort

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            conP = port;
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Proces.MijnPoort);

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int port)
        {
            conP = port;
            Read = read; Write = write;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            //first check if all connections are set and initialized
            while (!Proces.Ready)
            {
                Thread.Sleep(1000);
            }
            try
            {
                while (true)
                {
                    HandleMessage();
                }
            }
            catch // Verbinding is kennelijk verbroken
            {
                lock (Proces.myLock)
                {
                    Proces.Disconnect(conP);
                }
            }
        }

        //HANDLE MESSAGE
        public void HandleMessage()
        {
            string input = Read.ReadLine();
            if (input.StartsWith("mydist"))
            {
                //update distance
                string[] inp = input.Split();
                string k = "" + conP + "," + inp[1];
                int v = int.Parse(inp[1]);
                int dist = int.Parse(inp[2]);
                //if you don't know v, add it to your list
                lock (Proces.myLock)
                {
                    if (!Proces.V.Contains(v))
                    {
                        Proces.V.Add(v);
                        Proces.InitValue(v);
                    }
                    Proces.ndis[k] = dist;
                    Proces.RecomputeV(v);
                }
            }
            else if (input.StartsWith("bericht"))
            {
                string[] delen = input.Split(new char[] { ' ' }, 3);

                if (delen[1] == Proces.MijnPoort.ToString())
                    Console.WriteLine(delen[2]);
                else
                {
                    lock (Proces.myLock)
                    {
                        int sendto = Proces.Nb[int.Parse(delen[1])];
                        Console.WriteLine("Bericht voor {0} doorgestuurd naar {1}", delen[1], sendto);
                        Proces.Buren[sendto].Write.WriteLine(input);
                    }
                }
            }
            else if (input.StartsWith("reqTab"))
            {
                lock (Proces.myLock)
                {
                    Proces.messageMyTable(conP);
                }
            }
            else if (input.StartsWith("disconnect"))
            {
                throw new Exception();
            }
        }
    }
}

