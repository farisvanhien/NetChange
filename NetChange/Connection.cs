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
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;

            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    //HANDLE MESSAGE
                    string[] input = Read.ReadLine().Split();
                    if (input[0] == "mydist")
                    {
                        //update distance
                        string k = "" + conP + "," + input[1];
                        int v = int.Parse(input[2]);
                        //if you don't know v, add it to your list
                        if (!Proces.V.Contains(v))
                        {
                            Proces.V.Add(v);
                            Proces.Recompute(v);
                        }
                        if (Proces.ndis.ContainsKey(k))
                        {
                            Proces.ndis[k] = v; //step to neighbour + neighbour's cost
                        }
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
    }
}
