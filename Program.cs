﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Aspirateur
{
    class Program
    {
        private void afficherCarte(int[] carte)
        {
            Console.SetCursorPosition(0,2);

            int i = 0;
            foreach (int piece in carte){
                if ((i > 0) && (i % 10 == 0))
                {
                    Console.Write("\n");
                }
                Console.Write("[" + piece + "]");
                i++;
            }
            Console.WriteLine(  "\n nombre de poussieres : " + carte.Count(j => j == 1) + "\n"
                                + "nombre de bijoux : " + carte.Count(j => j == 2)+ "\n"
                                + "nombre de pousiere et bijoux : " + carte.Count(j => j == 3));
        }

        static void Main(string[] args)
        {
            //instanciation du programme
            Program prog = new Program();

            //touche pour fermer le programme
            ConsoleKey exitKey = ConsoleKey.Escape;
            
            //frequence rafraichissement console
            int fps = 10;

            //variables temporelles pour le rafraichissement (delta time = time - time2)
            long time = time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            long time2;

            //variable pour stocker la grille a afficher
            int[] terrainOld;

            //variable seed aléatoire
            Random rand = new Random();

            //création de l'environnement et de son thread dédié
            Environnement env = new Environnement();
            Thread threadEnv = new Thread(env.run);

            //création de l'agent dans l'environnement et de son thread dédié
            Agent agent = new Agent(env, AlgoExploration.LARGEUR);
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
            terrainOld = env.getCarte();
            prog.afficherCarte(terrainOld);
           
            //cache le curseur
            Console.CursorVisible = false;

            //boucle principale du main
            terrainOld = new int[100];
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == exitKey))
            {
                time2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if (time2 - time > (1000/fps))
                {
                    if (rand.Next(101)%100 == 1)
                    {
                        env.EventCreerBijoux();
                        
                    }
                    else if (rand.Next(101) %10  == 0)
                    {
                        env.EventCreerPoussiere();
                    }

                    prog.afficherCarte(env.getCarte());
                    time = time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
            }

            //arret du thread environnement de par lui même
            env.arret();

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
