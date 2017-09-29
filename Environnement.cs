using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aspirateur
{
    class Environnement
    {
        //--------------variables privées de l'environnement-------------//

        ///constantes des objets dans une piece, VIDE = 0, POUSSIERE = 1 ...
        private enum objetCase { VIDE, POUSSIERE, BIJOUX, POUSSIEREBIJOUX };

        ///carte de l'environnement et des objets présents dans les pieces de cet environnement
        private int[] carte = new int[100];

        ///Mesure de performance, 0 = meilleur
        private int mesurePerformance = 0;

        ///variable d'arret du thread
        private volatile bool doisArreter;

        //---------------------Constructeurs----------------------------//

        ///constructeur par défaut
        public Environnement()
        {

        }

        //-----------------------getters-------------------------------//
        public int getMesurePerformance()
        {
            return this.mesurePerformance;
        }

        public int[] getCarte()
        {
            return this.carte;
        }

        //---------------------methodes--------------------//
        ///fonction principale
        public void run()
        {
            while (!doisArreter)
            {
                Console.WriteLine("thread env : execution...");
            }
            Console.WriteLine("thread env : arrêt");
        }

        ///fonction d'arret
        public void arret()
        {
            doisArreter = true ;
        }
    }
}
