using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NetChange
{
    class Proces
    {
        static public Object myLock = new object();
        static public int MijnPoort;
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public List<int> V = new List<int>(); //List of all nodes

        static public Dictionary<int, int> D = new Dictionary<int, int>();  //estimated distance
        static public Dictionary<int, int> Nb = new Dictionary<int, int>(); //prefered Neigbour for v; <v, w>
        static public Dictionary<string, int> ndis = new Dictionary<string, int>(); //estimated distance w,u; <"w,v",d>

        static public int N; //size of network
        static public int udef = -1; //magicnumber to indicate impossible route

        static public bool Ready = false;

        static void Main(string[] args)
        {
            N = 20;
            initConnect(args);

            Console.Title = "Netchange " + MijnPoort;
            new Thread(() => initInput()).Start();
        }

        public static void initInput()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("C"))
                {
                    string[] inp = input.Split();
                    int poort = int.Parse(inp[1]);
                    lock (myLock)
                    {
                        makeConnection(poort);
                        RecomputeAll();
                    }
                }
                else if (input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 3);
                    int poort = int.Parse(delen[1]);
                    if (!V.Contains(poort))
                        Console.WriteLine("Poort " + poort + " is niet bekend");
                    else
                    {
                        lock (myLock)
                        {
                            int sendto = Nb[int.Parse(delen[1])];
                            Buren[sendto].Write.WriteLine("bericht" + " " + poort + " " + delen[2]);
                        }
                    }
                }
                else if (input.StartsWith("R"))
                {
                    lock (myLock)
                    {
                        printTable();
                    }
                }
                else if (input.StartsWith("P"))
                {
                    testprint();
                }
                else if (input.StartsWith("D"))
                {
                    string[] inp = input.Split();
                    lock (myLock)
                    {
                        if (!V.Contains(int.Parse(inp[1])))
                            Console.WriteLine("Poort " + inp[1] + " is niet bekend");
                        else
                        {
                            int poort = int.Parse(inp[1]);
                            //verbreek verbinding
                            Buren[poort].Write.WriteLine("disconnect");
                            Disconnect(poort);
                        }
                    }
                }
                else if (input.StartsWith("X"))
                {
                    string[] inp = input.Split();
                    int pnm = int.Parse(inp[1]);
                    int val = int.Parse(inp[2]);
                    D[pnm] = val;
                }
            }
        }

        //print alle informtaie voor debuggen
        public static void testprint()
        {
            Console.WriteLine("List Buren : ");
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                Console.WriteLine(w);
            }
            Console.WriteLine("List V :");
            foreach (int v in V)
            {
                Console.WriteLine(v);
            }
            Console.WriteLine("List D :");
            foreach (KeyValuePair<int, int> d in D)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("List Nb :");
            foreach (KeyValuePair<int, int> nb in Nb)
            {
                Console.WriteLine(nb);
            }
            Console.WriteLine("List ndis :");
            foreach (KeyValuePair<string, int> ndis in ndis)
            {
                Console.WriteLine(ndis);
            }
        }

        //maakt een niewe verbinding aan
        public static void makeConnection(int poort)
        {
            // Leg verbinding aan (als client)
            tryConnect(poort);
            //Update je tabel
            D[poort] = 1;
            Nb[poort] = poort;
            //update de informatie
            messageMyTable(poort);
            requestTable(poort);
        }
        //verbreek verbinding
        public static void Disconnect(int poort)
        {
            Console.WriteLine("//disconnecting...");
            Buren.Remove(poort);
            Console.WriteLine("Verbroken: " + poort);
            D[poort] = N;
            Nb[poort] = udef;
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                myDistMessage(poort, w.Key, D[poort]);
            }
            RecomputeAll();
        }

        //bericht naar proces met jouw afstand tot v
        public static void myDistMessage(int distanceTo, int sendMto, int dist)
        {
            string message = "mydist " + distanceTo + " " + dist;
            //Console.WriteLine("//   me to " + sendMto + ": " + message);
            Buren[sendMto].Write.WriteLine(message);
        }
        //vraag tabel informatie van een buur
        public static void requestTable(int sendMto)
        {
            string message = "reqTab";
            Buren[sendMto].Write.WriteLine(message);
        }
        //verzend jouw tabel informatie naar buur die opvraagt
        public static void messageMyTable(int newPoort)
        {
            //laat al je buren weten van je nieuwe connectie
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                myDistMessage(newPoort, w.Key, 1);
            }
            //Laat de nieuwe connectie van jouw routing table weten 
            foreach (int v in V)
            {
                myDistMessage(v, newPoort, D[v]);
            }
        }

        //maak verbinding met een poort, en wacht tot het lukt
        public static void initInitConnect(string poortstring)
        {
            int poort = int.Parse(poortstring);
            if (!V.Contains(poort))
            {
                V.Add(poort);
            }
            if (Buren.ContainsKey(poort))
                Console.WriteLine("//Hier is al verbinding naar!");
            else if (MijnPoort < poort)
            {
                // Leg verbinding aan (als client)
                tryConnect(poort);
            }
            else
            {
                waitConnect(poort);
            }
        }
        //maak server en connecties met buren
        public static void initConnect(string[] inp)
        {
            MijnPoort = int.Parse(inp[0]);
            new Server(MijnPoort);
            V.Add(MijnPoort);
            for (int i = 1; i < inp.Length; i++)
            {
                initInitConnect(inp[i]);
            }
            Initialize();
        }
        //try connecting, sleep and try again
        public static void tryConnect(int poort)
        {
            try
            {
                Buren.Add(poort, new Connection(poort));
                Console.WriteLine("Verbonden: " + poort);
            }
            catch
            {
                Thread.Sleep(1000);
                tryConnect(poort);
            }
        }
        //wait till connection is started
        public static void waitConnect(int poort)
        {
            while (!Buren.ContainsKey(poort))
            {
                Thread.Sleep(1000);
            }
        }

        public static void RecomputeAll()
        {
            lock (myLock)
            {
                foreach (int v in V)
                {
                    RecomputeV(v);
                }
            }
        }
        //recompute closest distance, if it is changed send message to neighs
        public static void RecomputeV(int v)
        {
            N = V.Count();
            int oldD = D[v];
            if (v == MijnPoort)
            {
                D[v] = 0;
                Nb[v] = MijnPoort;
            }
            else
            {
                //estimate distance to v
                int tempdis = N;
                int tempNeigh = udef;
                foreach (KeyValuePair<int, Connection> w in Buren) //get the closest neighbour to v
                {
                    string tmp = w.Key + "," + v;
                    if (!ndis.ContainsKey(tmp))
                    {
                        continue;
                    }
                    else if (ndis[tmp] < tempdis)
                    {
                        tempdis = ndis[tmp];
                        tempNeigh = w.Key;
                    }
                }
                int d = 1 + tempdis;
                if (d < N)
                {
                    D[v] = d;
                    Nb[v] = tempNeigh;
                }
                else
                {
                    D[v] = N;
                    Nb[v] = udef;
                }
            }
            if (D[v] != oldD)
            {
                if (D[v] == N)
                {
                    Console.WriteLine("Onbereikbaar: " + v);
                }
                else
                {
                    Console.WriteLine("Afstand naar {0} is nu {1} via {2}", v, D[v], Nb[v]);
                }
                //send message to all neighbours
                foreach (KeyValuePair<int, Connection> w in Buren)
                {
                    string message = "mydist " + v + " " + D[v];
                    w.Value.Write.WriteLine(message);
                }
            }
        }
        //initializeer de beginwaarden in routing table
        public static void Initialize()
        {
            foreach (int v in V)
            {
                InitValue(v);
            }
            D[MijnPoort] = 0;
            Nb[MijnPoort] = MijnPoort;
            //stuur bericht naar de buren dat afstand tot jezelf 0 is
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                string message = "mydist " + MijnPoort + " " + 0;
                w.Value.Write.WriteLine(message);
            }
            Ready = true;
        }
        //sets the initial values for v in the dictionaries
        public static void InitValue(int v)
        {
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                string tmp = "" + w.Key + "," + v;
                ndis[tmp] = N;
            }
            D[v] = N;
            Nb[v] = udef;
        }

        //prints routing table
        public static void printTable()
        {
            foreach (int v in V)
            {
                int res1 = Nb[v];
                string res2;
                if (res1 == udef)
                {
                    continue;
                }
                else if (res1 == MijnPoort)
                {
                    res2 = "local";
                }
                else if (D[v] == N)
                {
                    continue;
                }
                else
                {
                    res2 = res1.ToString();
                }
                Console.WriteLine("{0} {1} {2}", v, D[v], res2);
            }
        }
    }
}
