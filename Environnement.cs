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

        ///Mesure de performance, 0 = meilleur, poussiere +1, bijoux +10, aspirer bijoux +100
        private int mesurePerformance = 0;
        private int malusApparitionPousiere = 1;
        private int malusApparitionBijoux = 10;
        private int malusAspirationBijoux = 100;

        ///variable d'arret du thread
        private volatile bool doisArreter = false;

        ///doit créer une poussière
        private bool doitCreerPoussiere = false;

        ///doir créer un bijoux
        private bool doitCreerBijoux = false;
        
        //variable seed aléatoire
        private Random rand;
        //---------------------Constructeurs----------------------------//

        ///constructeur par défaut
        public Environnement()
        {
            rand = new Random();
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

        //---------------------methodes publiques--------------------//
        ///methode principale
        public void run()
        {
            while (!doisArreter)
            {
                if (doitCreerPoussiere)
                {
                    creerPoussiere();
                    doitCreerPoussiere = false;
                }
                if (doitCreerBijoux)
                {
                    creerBijoux();
                    doitCreerBijoux = false;
                }
                Console.WriteLine("thread env : execution...");
            }
            Console.WriteLine("thread env : arrêt");
        }

        ///methode d'arret
        public void arret()
        {
            doisArreter = true ;
        }

        ///event recu de création de poussiere
        public void EventCreerPoussiere()
        {
            doitCreerPoussiere = true;
        }
        
        ///event recu de création de bijoux
        public void EventCreerBijoux()
        {
            doitCreerBijoux = true;
        }


        ///methode d'action de l'aspirateur
        public void actionAspirateur(String action, int position)
        {
            switch (action)
            {
                case "aspirer":
                    if (carte[position] == (int)objetCase.POUSSIERE) { mesurePerformance -= malusApparitionPousiere; }
                    else if (carte[position] == (int)objetCase.BIJOUX) { mesurePerformance += malusAspirationBijoux; }
                    else if (carte[position] == (int)objetCase.POUSSIEREBIJOUX) { mesurePerformance += ( malusAspirationBijoux - malusApparitionPousiere ); }
                    carte[position] = (int)objetCase.VIDE;
                    break;

                case "prendre":
                    if (carte[position] == (int)objetCase.BIJOUX)
                    {
                        mesurePerformance -= malusApparitionBijoux;
                        carte[position] = (int)objetCase.VIDE;
                    }
                    else if (carte[position] == (int)objetCase.POUSSIEREBIJOUX) {
                        mesurePerformance -= malusApparitionBijoux;
                        carte[position] = (int)objetCase.POUSSIERE;
                    }
                    break;

                default:
                    break;
            }
        }

        //---------------------methodes internes-------------------//
        ///methode de création de poussiere
        private void creerPoussiere()
        {
            int aleatoire = rand.Next(100); // random 0-99
            if (carte[aleatoire] ==(int)objetCase.VIDE)
            {
                carte[aleatoire] = (int)objetCase.POUSSIERE;
                mesurePerformance += malusApparitionPousiere;
            }
            else if (carte[aleatoire] == (int)objetCase.BIJOUX)
            {
                carte[aleatoire] = (int)objetCase.POUSSIEREBIJOUX;
                mesurePerformance += malusApparitionPousiere;
            }
        }

        ///methode de création de bijoux
        private void creerBijoux()
        {
            int aleatoire = rand.Next(100); // random 0-99
            if (carte[aleatoire] == (int)objetCase.VIDE)
            {
                carte[aleatoire] = (int)objetCase.BIJOUX;
                mesurePerformance += malusApparitionBijoux;
            }
            else if (carte[aleatoire] == (int)objetCase.POUSSIERE)
            {
                carte[aleatoire] = (int)objetCase.POUSSIEREBIJOUX;
                mesurePerformance += malusApparitionBijoux;
            }
        }

    }
}
