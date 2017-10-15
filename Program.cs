using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Aspirateur
{
    class Program
    {
        private static void AfficherCarte(int[] carte, Environnement env, Agent agent)
        {
            Console.SetCursorPosition(0,2);

            int i = 0;
            int pos = agent.GetPosition();
            foreach (int piece in carte){
                if ((i > 0) && (i % 10 == 0))
                {
                    Console.Write("\n");
                }
                if (i == pos)
                {
                    Console.Write("[X]");
                }
                else
                {
                    Console.Write("[" + piece + "]");
                }
                i++;
            }
            Console.Write("\nnombre de poussieres : " + carte.Count(j => j == 1) + "\n"
                                + "nombre de bijoux : " + carte.Count(j => j == 2) + "\n"
                                + "nombre de pousiere et bijoux : " + carte.Count(j => j == 3) + "\n"
                                + "mesure d'evaluation : " + env.GetMesurePerformance() + "\n"
                                + "nbAction : ");
            if (agent.GetNbAction() < 10) { Console.Write("0" + agent.GetNbAction()); }
            else { Console.Write(agent.GetNbAction()); }
            Console.SetCursorPosition(0, 20);
        }

        public static void Main(string[] args)
        {
            // Instanciation du programme
            Program prog = new Program();

            // Touche mettant fin au programme
            const ConsoleKey exitKey = ConsoleKey.Escape;
            
            // Fréquences de rafraichissement console et d'apparition des poussières et bijoux
            const int fps = 60;
            const double frequencePoussiereFrame = 0.12;
            const double frequenceBijouxFrame = 0.02;

            //choix de l'agloritme exploration : ASTAR | LARGEUR
            const AlgoExploration algoExp = AlgoExploration.ASTAR;

            //variables temporelles pour le rafraichissement (delta time = time - time2)
            long time = time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long time2;

            //variable pour stocker la grille a afficher
            int[] terrainOld;

            //variable seed aléatoire
            Random rand = new Random();

            //création de l'environnement et de son thread dédié
            Environnement env = new Environnement();
            Thread threadEnv = new Thread(env.Lancer);

            //création de l'agent dans l'environnement et de son thread dédié
            Agent agent = new Agent(env, algoExp);
            Thread threadAgent = new Thread(agent.Lancer);

            //démarrage du thread environnement et agent
            threadEnv.Start();
            Console.WriteLine("thread principal : démarrage du thread environnement");

            //attente du démarrage du thread environnement
            while (!threadEnv.IsAlive) ;

            threadAgent.Start();
            Console.WriteLine("thread principal : démarrage du thread agent");

            //attente du démarrage du thread agent
            while (!threadAgent.IsAlive) ;
            
            //on charge le terrain vide
            terrainOld = env.GetCarte();
            AfficherCarte(terrainOld,env,agent);
           
            //cache le curseur
            Console.CursorVisible = false;

            //boucle principale du main
            terrainOld = new int[100];
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == exitKey))
            {
                time2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if (time2 - time > (1000/fps))
                {
                    if (frequencePoussiereFrame > rand.NextDouble())
                    {
                        env.EventCreerPoussiere();
                    }
                    else if (frequenceBijouxFrame > rand.NextDouble())
                    {
                        env.EventCreerBijoux();
                    }

                    AfficherCarte(env.GetCarte(),env,agent);
                    time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
            }

            //arret du thread environnement de par lui même
            env.Arret();

            //attente de la fin du thread environnement
            threadEnv.Join();
            Console.WriteLine("thread principal : thread environnement c'est terminé");
            //arret du thread agent de par lui même
            agent.Arret();

            //attente de la fin du thread environnement
            threadEnv.Join();
            Console.WriteLine("thread principal : thread environnement c'est terminé");

            Console.ReadKey();
        }
    }
}
