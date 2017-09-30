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
        static void Main(string[] args)
        {
            //création de l'environnement et de son thread dédié
            Environnement env = new Environnement();
            Thread threadEnv = new Thread(env.run);

            //démarrage du thread
            threadEnv.Start();
            Console.WriteLine("thread principal : démarrage du thread environnement");

            //attente du démarrage du thread environnement
            while (!threadEnv.IsAlive);

            // test de création de 10ms d'intervalle pour laisser travailler le thread environnement
            for (int i = 0; i < 20; i++)
            {
                env.EventCreerBijoux();
                env.EventCreerPoussiere();
                Thread.Sleep(10);

            }
            //arret du thread environnement de par lui même

            env.arret();

            //attente de la fin du thread environnement
            threadEnv.Join();
            Console.WriteLine("thread principal : thread environnement c'est terminé");
            Console.ReadKey();
        }
    }
}
