using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetChange
{
    class Program
    {
        static void Main(string[] args)
        {
            NetChange netchange = new NetChange();
        }
    }
    class NetChange
    {
        string commando;
        string bericht;
        int N; 


        public NetChange()
        {
            Console.WriteLine("How many nodes in the network");
            N = int.Parse(Console.ReadLine());


            string[] input = Console.ReadLine().Split();
            commando = input[0];    //commando dat uitgevoerd moet worden
            if (commando != "R") poortnummer = int.Parse(input[1]);
            if (commando == "B") bericht = input[2];

            switch (commando)
            {
                case "R":
                    routingTable();
                    break;
                case "B":
                    stuurBericht();
                    break;
                case "C":
                    maakVerbinding();
                    break;
                case "D":
                    verbreekVerbinding();
                    break;
                default:
                    break;
            }
        }
        public void routingTable()
        {
            foreach (string verbinding in verbindingen)
            {
                Console.WriteLine(verbinding);
            }
        }
        public void stuurBericht()
        {

        }
        public void maakVerbinding()
        {

        }
        public void verbreekVerbinding()
        {

        }
    }
}
