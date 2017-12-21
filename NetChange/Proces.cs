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
        static public int MijnPoort;
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public List<int> V = new List<int>(); //List of all nodes

        static public Dictionary<int, int> D = new Dictionary<int, int>();  //estimated distance
        static public Dictionary<int, int> Nb = new Dictionary<int, int>(); //prefered Neigbour for v; <v, w>
        static public Dictionary<string, int> ndis = new Dictionary<string, int>(); //estimated distance w,u; <"w,v",d>

        static public int N; //size of network
        static public int udef = -1; //magicnumber to indicate impossible route

        public Proces()
        {
            N = 20;

            new Thread(() => initInput()).Start();
        }
        public void initInput()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("C"))
                {
                    int poort = int.Parse(input.Split()[1]);
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
                else if (input.StartsWith("B"))
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 3);
                    int poort = int.Parse(delen[1]);
                    if (!Buren.ContainsKey(poort))
                        Console.WriteLine("//Hier is al verbinding naar!");
                    else
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[2]);
                }
                else if (input.StartsWith("R"))
                {
                    printTable();
                }
                else if (input.StartsWith("P"))
                {
                    Console.WriteLine("List V :");
                    foreach (int v in V)
                    {
                        Console.WriteLine(v);
                    }
                    Console.WriteLine("List D :");
                    foreach (KeyValuePair<int,int> d in D)
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
                else
                {
                    string[] inp = input.Split();
                    tempsstart(inp);
                }
            }

        }

        public void tempsstart(string[] inp)
        {
            MijnPoort = int.Parse(inp[0]);
            V.Add(MijnPoort);
            new Server(MijnPoort);
            for (int i = 1; i < inp.Length; i++)
            {
                int poort = int.Parse(inp[i]);
                if (!V.Contains(poort))
                {
                    V.Add(poort);
                }
                if (Buren.ContainsKey(poort))
                    Console.WriteLine("//Hier is al verbinding naar!");
                else if(MijnPoort < poort)
                {
                    // Leg verbinding aan (als client)
                    tryConnect(poort);
                }
            }
            Initialize();
        }

        public void tryConnect(int poort)
        {
            try
            {
                Console.WriteLine("//Trying");
                Buren.Add(poort, new Connection(poort));
                Console.WriteLine("//Connected with " + poort);
            }
            catch
            {
                Console.WriteLine("//Trying to connect...");
                Thread.Sleep(5000);
                
                tryConnect(poort);
            }
            
        }

        public static void Recompute()
        {
            foreach (int v in V)
            {
                RecomputeV(v);
            }
        }

        public static void RecomputeV(int v)
        {
            int oldD = D[v];
            if(v == MijnPoort)
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
                    string tmp = "" + w.Key + "," + v;
                    if (ndis[tmp]  < tempdis)
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
                //send message to all neighbours
                foreach (KeyValuePair<int, Connection> w in Buren)
                {
                    w.Value.Write.WriteLine("mydist " + v + D[v]);
                }
            }
        }

        //initializeer de beginwaarden in routing table
        public void Initialize()
        {
            foreach (int v in V)
            {
                foreach (KeyValuePair<int, Connection> w in Buren)
                {
                    string tmp = "" + w.Key + "," + v;
                    ndis.Add(tmp, N);
                }
                D.Add(v, N);
                Nb.Add(v, udef);
            }
            D[MijnPoort] = 0;
            Nb[MijnPoort] = MijnPoort;
            //stuur bericht naar de buren dat afstand tot jezelf 0 is
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                w.Value.Write.WriteLine("mydist " + MijnPoort + " " + 0);
            }
        }

        public void printTable()
        {
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                Console.WriteLine(w);
            }
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
                else
                {
                    res2 = res1.ToString();
                }
                string res = String.Format("{0} {1} {2}", v, D[v], res2);
                Console.WriteLine(res);
            }
        }
        
    }
}
