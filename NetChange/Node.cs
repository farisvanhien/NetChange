using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetChange
{
    
    class Node
    {
        string poortnummer;
        int NodeNum;
        HashSet<Node> Neigh;   //list of neighbours
        string[] poortnums;//list of portnumbers
        string[] Nb;      //preferred neigbour
        int[] D;        //estimated distance
        int[,] nDis;     //estimates d(w,v)
        Node[] V;
        int N; //number of nodes
        public Node(string[] _poortnums, HashSet<Node> _Neigh)
        {
            initialization();
            N = V.Length;
            poortnums = _poortnums;
            Neigh = _Neigh;
            D = new int[N];
            Nb = new string[N];
            nDis = new int[N, N];

        }
        public int getIndex(string poort)
        {
            for (int i = 0; i < poortnums.Length; i++)
            {
                if (poortnums[i] == poort)
                {
                    return i;
                }
            }
            return -1;
        }
        //estimate the distance between two nodes
        public void d(int u, int v)
        {
          
        }
        public void initialization()
        {
            foreach (Node v in V)
            {
                int vi = getIndex(v.poortnummer);
                foreach (Node w in Neigh)
                {
                    int wi = getIndex(w.poortnummer);
                    nDis[wi, vi] = N;
                }
                D[vi] = N;
                Nb[vi] = "udef";
            }
            int ui = getIndex(poortnummer);
            D[ui] = 0;
            Nb[ui] = "local";
            foreach (Node w in Neigh)
            {
                w.receiveMessage("mydist", poortnummer, poortnummer, 0);
            }


        }
        public void recompute(Node v)
        {
           
        }
        public void receiveMessage(string s, string poortVan, string poortNaar, int dist)
        {
            if (isNeigh(poortVan)) //maybe delete
            {
                int wi = getIndex(poortVan);
                int vi = getIndex(poortNaar);
                nDis[wi, vi] = dist;
            }
        }
        public bool isNeigh(string poort)
        {
            foreach (Node w in Neigh)
            {
                if (w.poortnummer == poort) return true;
            }
            return false;
        }
    }
}
