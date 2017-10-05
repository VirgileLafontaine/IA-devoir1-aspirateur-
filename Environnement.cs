using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aspirateur
{

    public class Environnement
    {
        //--------------variables privées de l'environnement-------------//

        ///constantes des objets dans une piece, VIDE = 0, POUSSIERE = 1 ...
        private enum objetCase { VIDE, POUSSIERE, BIJOUX, POUSSIEREBIJOUX };
        
        ///carte de l'environnement et des objets présents dans les pieces de cet environnement
        private int[] carte = new int[100];

        /// file d'action que l'aspirateur réalise à effectuer par l'environnement
        public static volatile Queue fileAction = new Queue();

        ///Mesure de performance, 0 = meilleur, poussiere +1, bijoux +10, aspirer bijoux +100
        private int mesurePerformance = 0;
        private int malusApparitionPousiere = 1;
        private int malusApparitionBijoux = 10;
        private int malusAspirationBijoux = 100;

        ///variable d'arret du thread
        private volatile bool doisArreter = false;

        ///doit créer une poussière
        private volatile bool doitCreerPoussiere = false;

        ///doit créer un bijoux
        private volatile bool doitCreerBijoux = false;

        ///message reçu
        Tuple<Action, int> message;

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
                while (fileAction.Count != 0)
                {
                    message = (Tuple<Action, int>)fileAction.Dequeue();
                    switch (message.Item1)
                    {
                        case Action.ASPIRER:
                            aspirer(message.Item2);
                            break;
                        case Action.RAMASSER:
                            ramasser(message.Item2);
                            break;
                    }
                }
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
        
        private void aspirer(int position)
        {
            if (carte[position]== (int)objetCase.BIJOUX)
            {
                carte[position] = (int)objetCase.VIDE;
                mesurePerformance += malusAspirationBijoux;
            }
            else if (carte[position] == (int)objetCase.POUSSIERE)
            {
                carte[position] = (int)objetCase.VIDE;
                mesurePerformance -= malusApparitionPousiere;
            }
            else if (carte[position] == (int)objetCase.POUSSIEREBIJOUX)
            {
                carte[position] = (int)objetCase.VIDE;
                mesurePerformance -= malusApparitionPousiere;
                mesurePerformance += malusAspirationBijoux;
            }

        }

        private void ramasser(int position)
        {
            if (carte[position] == (int)objetCase.BIJOUX)
            {
                carte[position] = (int)objetCase.VIDE;
                mesurePerformance -= malusApparitionBijoux;
            }
            else if (carte[position] == (int)objetCase.POUSSIEREBIJOUX)
            {
                carte[position] = (int)objetCase.POUSSIERE;
                mesurePerformance -= malusApparitionBijoux;
            }
        }

    }
}
