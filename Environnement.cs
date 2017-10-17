using System;
using System.Collections;

namespace Aspirateur
{
    
    ///constantes des objets dans une piece, VIDE = 0, POUSSIERE = 1 ...
    public enum ObjetCase { VIDE, POUSSIERE, BIJOUX, POUSSIEREBIJOUX };

    public class Environnement
    {
        //--------------variables privées de l'environnement-------------//
        
        ///carte de l'environnement et des objets présents dans les pieces de cet environnement
        private readonly int[] _carte = new int[100];

        /// file d'action que l'aspirateur réalise à effectuer par l'environnement
        public static volatile Queue FileAction = new Queue();

        ///Mesure de performance, 0 = meilleur, poussiere +1, bijoux +1, aspirer bijoux +10
        private int _mesurePerformance = 0;

        private const int MalusApparitionPousiere = 1;
        private const int MalusApparitionBijoux = 1;
        private const int MalusAspirationBijoux = 10;

        ///variable d'arret du thread
        private volatile bool _doisArreter = false;

        ///doit créer une poussière
        private volatile bool _doitCreerPoussiere = false;

        ///doit créer un bijoux
        private volatile bool _doitCreerBijoux = false;

        ///message reçu
        private Tuple<Action, int> _message;

        //variable seed aléatoire
        private readonly Random _rand;
        //---------------------Constructeurs----------------------------//

        ///constructeur par défaut
        public Environnement()
        {
            _rand = new Random();
        }

        //-----------------------getters-------------------------------//
        public int GetMesurePerformance()
        {
            return _mesurePerformance;
        }

        public int[] GetCarte()
        {
            return _carte;
        }

        //---------------------methodes publiques--------------------//
        ///methode principale
        public void Lancer()
        {
            while (!_doisArreter)
            {
                if (_doitCreerPoussiere)
                {
                    CreerPoussiere();
                    _doitCreerPoussiere = false;
                }
                if (_doitCreerBijoux)
                {
                    CreerBijoux();
                    _doitCreerBijoux = false;
                }
                while (FileAction.Count != 0)
                {
                    _message = (Tuple<Action, int>)FileAction.Dequeue();
                    if (_message != null)
                    {
                        switch (_message.Item1)
                        {
                            case Action.ASPIRER:
                                Aspirer(_message.Item2);
                                break;
                            case Action.RAMASSER:
                                Ramasser(_message.Item2);
                                break;
                        }
                    }
                }
            }
            Console.WriteLine("thread env : arrêt");
        }

        ///methode d'arret
        public void Arret()
        {
            _doisArreter = true ;
        }

        ///event recu de création de poussiere
        public void EventCreerPoussiere()
        {
            _doitCreerPoussiere = true;
        }
        
        ///event recu de création de bijoux
        public void EventCreerBijoux()
        {
            _doitCreerBijoux = true;
        }

        
        //---------------------methodes internes-------------------//
        ///methode de création de poussiere
        private void CreerPoussiere()
        {
            int aleatoire = _rand.Next(100); // random 0-99
            if (_carte[aleatoire] ==(int)ObjetCase.VIDE)
            {
                _carte[aleatoire] = (int)ObjetCase.POUSSIERE;
                _mesurePerformance += MalusApparitionPousiere;
            }
            else if (_carte[aleatoire] == (int)ObjetCase.BIJOUX)
            {
                _carte[aleatoire] = (int)ObjetCase.POUSSIEREBIJOUX;
                _mesurePerformance += MalusApparitionPousiere;
            }
        }

        ///methode de création de bijoux
        private void CreerBijoux()
        {
            int aleatoire = _rand.Next(100); // random 0-99
            if (_carte[aleatoire] == (int)ObjetCase.VIDE)
            {
                _carte[aleatoire] = (int)ObjetCase.BIJOUX;
                _mesurePerformance += MalusApparitionBijoux;
            }
            else if (_carte[aleatoire] == (int)ObjetCase.POUSSIERE)
            {
                _carte[aleatoire] = (int)ObjetCase.POUSSIEREBIJOUX;
                _mesurePerformance += MalusApparitionBijoux;
            }

        }
        
        private void Aspirer(int position)
        {
            if (_carte[position]== (int)ObjetCase.BIJOUX)
            {
                _carte[position] = (int)ObjetCase.VIDE;
                _mesurePerformance += MalusAspirationBijoux;
            }
            else if (_carte[position] == (int)ObjetCase.POUSSIERE)
            {
                _carte[position] = (int)ObjetCase.VIDE;
                _mesurePerformance -= MalusApparitionPousiere;
            }
            else if (_carte[position] == (int)ObjetCase.POUSSIEREBIJOUX)
            {
                _carte[position] = (int)ObjetCase.VIDE;
                _mesurePerformance -= MalusApparitionPousiere;
                _mesurePerformance += MalusAspirationBijoux;
            }

        }

        private void Ramasser(int position)
        {
            if (_carte[position] == (int)ObjetCase.BIJOUX)
            {
                _carte[position] = (int)ObjetCase.VIDE;
                _mesurePerformance -= MalusApparitionBijoux;
            }
            else if (_carte[position] == (int)ObjetCase.POUSSIEREBIJOUX)
            {
                _carte[position] = (int)ObjetCase.POUSSIERE;
                _mesurePerformance -= MalusApparitionBijoux;
            }
        }

    }
}
