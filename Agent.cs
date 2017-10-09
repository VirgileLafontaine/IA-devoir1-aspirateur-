using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aspirateur
{
    /* BDI */
    public class Bdi
    {
        /*---------------------------------*/
        /*       Believes - Croyances      */
        /*---------------------------------*/
        // Etat de la carte récupéré lors de la phase d'exploration
        public int[] Carte { get; set; }

        // Position de l'agent sur la carte
        public int Position { get; set; }

        // Nombre d'actions à exécuter avant de faire une nouvelle exploration
        // Initialement, ce paramètre sera fixé à 1
        public int NbActions { get; set; }

        /*---------------------------------*/
        /*             Intentions          */
        /*---------------------------------*/
        // Plan d'action à réaliser
        public Queue PlanDAction;
            
        // Coût total
        public int Cout { get; set; }
            
        /*---------------------------------*/
        /*             Desires             */
        /*---------------------------------*/
        // Objectif : aspirer toutes les poussières
        // L'objectif est formulé sous la forme d'une fonction de test de but
        public bool TestBut(Etat etat)
        {
            return etat.NbPoussiere == 0;
        }

        // Constructeur du BDI
        public Bdi()
        {
            Carte = new int[100];
            Position = 0;
            NbActions = 1;
            PlanDAction = new Queue();
        }
    }

    /* Capteur */
    public class CapteurObservation
    {
        public int[] ObserverCarte(Environnement env)
        {
            return env.getCarte();
        }
    }

    /* Effecteurs */
    public class Effecteurs
    {
        //tuple message
        Tuple<Action, int> _message;
        // Effecteur ASPIRER
        public void Aspirer(int position, Environnement env)
        {
            // Console.WriteLine("Effecteur aspirer");
            // Notifier l'environnement qu'on aspire la pièce X
            _message = Tuple.Create(Action.ASPIRER, position);
            Environnement.fileAction.Enqueue(_message);
        }
    
        // Effecteur RAMASSER
        public void Ramasser(int position, Environnement env)
        {
            // Console.WriteLine("Effecteur ramasser");
            // Notifier l'environnement qu'on ramasse un bijou dans la pièce X
            _message = Tuple.Create(Action.RAMASSER, position);
            Environnement.fileAction.Enqueue(_message);
        }
    
        //Effecteurs de déplacement : HAUT, BAS, DROITE, GAUCHE
        // Retour :        la nouvelle position de l'agent
        //                 position en cas d'erreur (mouvement impossible)
        public int Haut(int position)
        {
            // Console.WriteLine("Effecteur haut");
            if (position >= 10) return position - 10;
            else return position;
        }
    
        public int Bas(int position)
        {
            // Console.WriteLine("Effecteur bas");
            if (position < 90) return position + 10;
            else return position;
        }
    
        public int Gauche(int position)
        {
            // Console.WriteLine("Effecteur gauche");
            if ((position % 10) != 0) return position - 1;
            else return position;
        }
    
        public int Droite(int position)
        {
            // Console.WriteLine("Effecteur droite");
            if (((position + 1) % 10) != 0) return position + 1;
            else return position;
        }
    }
    
    // Actions possibles de l'agent
    // - ASPIRER : l'agent aspire la case dans laquelle il se trouve
    // - RAMASSER : l'agent ramasse un bijou dans la case où il se trouve
    // - HAUT : l'agent se déplace d'une case vers le haut
    // - BAS : l'agent se déplace d'une case vers le bas
    // - DROITE : l'agent se déplace d'une case vers la droite
    // - GAUCHE : l'agent se déplace d'une case vers la gauche
    public enum Action {ASPIRER, RAMASSER, HAUT, BAS, DROITE, GAUCHE, RIEN}

    // NOTE : je pense qu'il faut mettre l'environnement en classe statique (une seule instance modifiée par des appels de méthode static)
    
    // Algorithmes d'exploration disponibles
    public enum AlgoExploration {LARGEUR, ASTAR}

    public abstract class Exploration
    {
        // Insère le noeud dans la frange, représentée sous forme de liste
        protected abstract List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud, int[] carte);
        
        // Fonction générique d'exploration
        // Retourne null en cas d'erreur
        public Queue Explorer(Etat etatInitial, int[] carte,  Func<Etat, bool> testBut)
        {
            /* Création du graphe */
            Graphe arbreRecherche = new Graphe(etatInitial);
            
            /* Initialisation de la frange */
            List<Noeud> frange = new List<Noeud> {arbreRecherche.Racine};

            /* Boucle de construction de la frange */
            // En cas d'échec, retourne un plan d'action vide
            while (true)
            {
                // Cas d'échec
                if (frange.Count == 0) return null;
                
                // Test de but
                Noeud noeud = frange.First();
                frange.Remove(frange.First());
                if (testBut(noeud.EtatNoeud)) return arbreRecherche.SequenceActions(noeud);
                
                // Expansion du noeud
                foreach (Noeud n in noeud.FonctionSuccession(carte))
                {
                    frange = InsererNoeud(frange, n, carte);
                    noeud.AjouterEnfant(n);
                }
            }
        }
    }
    
    public class RechercheEnLargeur : Exploration
    {
        protected override List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud, int[] carte)
        {
            frange.Add(noeud);
            return frange;
        }
    }
    
    public class Astar : Exploration {

        private int DistanceManhattan(int pos1, int pos2)
        {
            return Math.Abs(pos1 % 10 - pos2 % 10) + Math.Abs(pos1 / 10 - pos2 / 10);
        }
        
        private int CalculHeuristique(int[] carte, Noeud noeud)
        {
            int max = 0;
            foreach (var x in carte)
            {
                if (x == (int) objetCase.POUSSIERE || x == (int) objetCase.POUSSIEREBIJOUX)
                {
                    int distance = DistanceManhattan(noeud.EtatNoeud.Position, x);
                    if (distance > max)
                    {
                        max = distance;
                    }
                }
            }
            return max + noeud.EtatNoeud.NbPoussiere;
        }
        
        private static int ComparaisonAStar(Noeud n1, Noeud n2)
        {
            int evaluationN1 = n1.Heuristique + n1.CoutChemin;
            int evaluationN2 = n2.Heuristique + n2.CoutChemin;
            return evaluationN1.CompareTo(evaluationN2);
        }
        
        protected override List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud, int[] carte)
        {
            noeud.Heuristique = CalculHeuristique(carte, noeud);
            frange.Add(noeud);
            frange.Sort(ComparaisonAStar);
            return frange;
        }
    }
    
    public class Agent
    { 
        /* Exploration */
        private Exploration _exploration;
        
        /* BDI */
        private Bdi _bdi= new Bdi();
        /* Capteur */
        private CapteurObservation _capteur = new CapteurObservation();
        /* Effecteurs */
        private Effecteurs _effecteurs = new Effecteurs();
        /* environnement lié*/
        Environnement _environnement;
        /*estEnVie */
        private volatile Boolean _enVie = true;

        /* Constructeur a utiliser pour placer un agent dans un environnement*/
        public Agent(Environnement env, AlgoExploration exploration)
        {
            _environnement = env;

            switch (exploration)
            {
                   case AlgoExploration.LARGEUR:
                       _exploration = new RechercheEnLargeur();
                       break;
                   case AlgoExploration.ASTAR:
                       _exploration = new Astar();
                       break;
            }
        }

        /* Dois s'arreter */
        public void Arret()
        {
            _enVie = false;
        }
        /* ------------------------------------------ */
        /*            Fonction principale             */
        /* ------------------------------------------ */
        public void Lancer()
        {
            Console.WriteLine("thread agent : démarrage");
            while (JeSuisEnVie())
            {
                // Observer l'environnement avec mes capteurs
                int[] carteActuelle = _capteur.ObserverCarte(_environnement);
                
                // Mettre à jour mon état (believes de mon BDI)
                MettreAJourBdi(carteActuelle);
                
                // Etablir le plan d'action
                EtablirPlanDAction();
                
                // Exécution du plan d'action
                ExecutionPlanDAction();

            }
            Console.WriteLine("thread agent : arrêt");
        }

        /* ------------------------------------------ */
        /*            Fonctions internes              */
        /* ------------------------------------------ */
        private bool JeSuisEnVie()
        {
            return _enVie; // Quand est ce qu'un agent meurt ?
        }

        private void MettreAJourBdi(int[] carteObservee)
        {
            // Actualiser la carte
            Array.Copy(carteObservee,_bdi.Carte,100);
            
            // Apprentissage : actualiser NbActions
        }

        private void EtablirPlanDAction()
        {
            /* Calcul du nombre de poussières */
            int nbPoussieres = 0;
            foreach (var x in _bdi.Carte)
            {
                if (x == (int) objetCase.POUSSIERE || x == (int) objetCase.POUSSIEREBIJOUX)
                    nbPoussieres++;
            }
            
            Etat etatInitial = new Etat(_bdi.Position,(objetCase) _bdi.Carte[_bdi.Position], nbPoussieres);
            _bdi.PlanDAction = _exploration.Explorer(etatInitial, _bdi.Carte, _bdi.TestBut);
        }

        private void ExecutionPlanDAction()
        {
            // Actualiser le coût
            _bdi.Cout+= _bdi.PlanDAction.Count;
            
            // Exécution des actions
            while (_bdi.PlanDAction.Count != 0)
            {
                switch ((int) _bdi.PlanDAction.Dequeue())
                {
                    case (int) Action.ASPIRER:
                        _effecteurs.Aspirer(_bdi.Position,_environnement);
                        break;
                    case (int) Action.RAMASSER:
                        _effecteurs.Ramasser(_bdi.Position,_environnement);
                        break;
                    case (int) Action.HAUT:
                        _bdi.Position = _effecteurs.Haut(_bdi.Position);
                        break;
                    case (int) Action.BAS:
                        _bdi.Position = _effecteurs.Bas(_bdi.Position);
                        break;
                    case (int) Action.GAUCHE:
                        _bdi.Position = _effecteurs.Gauche(_bdi.Position);
                        break;
                    case (int) Action.DROITE:
                        _bdi.Position = _effecteurs.Droite(_bdi.Position);
                        break;  
                }   
            }
        }
    }
}