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
        static public int local = -2; //magicnumber to indicate yourself

        public Proces()
        {
            N = 20; //for now
            string[] inp = Console.ReadLine().Split();
            MijnPoort = int.Parse(inp[0]);
            V.Add(MijnPoort);
            new Server(MijnPoort);
            
            //loop over buren
            for (int i = 1; i < inp.Length; i++)
            {
                int poort = int.Parse(inp[i]);
                V.Add(poort); //build V

                //handle sockets
                if (MijnPoort < poort) //so you only open it once
                {
                    //open socket
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
            }

            Initialize();

            new Thread(() => HandleUserInput()).Start();
        }

        public void Recompute(int v)
        {
            if(v == MijnPoort)
            {
                D[v] = 0;
                Nb[v] = local;
            }
            ///////////////////////////32fvwf nbsrbes   gf
        }
        //initializeer de beginwaarden
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
            D.Add(MijnPoort, 0);
            Nb.Add(MijnPoort, MijnPoort);
            //stuur bericht naar de buren dat afstand tot jezelf 0 is
            foreach (KeyValuePair<int, Connection> w in Buren)
            {
                w.Value.Write.WriteLine("mydist " + MijnPoort + " " + 0);
            }
        }

        public void HandleUserInput()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("verbind"))
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
                else
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int poort = int.Parse(delen[0]);
                    if (!Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                }
            }
        }
    }
}
