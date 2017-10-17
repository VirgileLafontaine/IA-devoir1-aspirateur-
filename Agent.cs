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
        // Position initiale : 45 (au milieu de la carte)
        public int Position { get; set; }

        // Nombre d'actions à exécuter avant de faire une nouvelle exploration
        // Initialement, ce paramètre sera fixé à 10
        public int NbActions { get; set; }

        /*---------------------------------*/
        /*             Intentions          */
        /*---------------------------------*/
        // Plan d'action à réaliser
        public Queue PlanDAction;
            
        /*---------------------------------*/
        /*             Desires             */
        /*---------------------------------*/
        // Objectif : aspirer toutes les poussières
        // L'objectif est formulé sous la forme d'une fonction de test de but
        public bool TestBut(Etat etat,Etat etatInitial)
        {
            return (etat.ListePoussiere.Count() < etatInitial.ListePoussiere.Count()
                    || etat.ListeBijoux.Count() < etatInitial.ListeBijoux.Count()
                    || etat.ListePoussiere.Count() == 0);
        }
        
        // Constructeur du BDI
        public Bdi()
        {
            Carte = new int[100];
            Position = 45;
            NbActions = 10;
            PlanDAction = new Queue();
        }
    }
    
    /* Capteur */
    public class CapteurObservation
    {
        public int[] ObserverCarte(Environnement env)
        {
            return env.GetCarte();
        }
        public int ObserverPerformance(Environnement env)
        {
            return env.GetMesurePerformance();
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
            Environnement.FileAction.Enqueue(_message);
        }
    
        // Effecteur RAMASSER
        public void Ramasser(int position, Environnement env)
        {
            // Console.WriteLine("Effecteur ramasser");
            // Notifier l'environnement qu'on ramasse un bijou dans la pièce X
            _message = Tuple.Create(Action.RAMASSER, position);
            Environnement.FileAction.Enqueue(_message);
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
    
    // Algorithmes d'exploration disponibles
    public enum AlgoExploration {LARGEUR, ASTAR}

    public abstract class Exploration
    {
        protected List<Etat> DejaVisites = new List<Etat>();
        
        // Insère le noeud dans la frange, représentée sous forme de liste
        protected abstract List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud);
        
        // Fonction générique d'exploration
        // Retourne null en cas d'erreur
        public Queue Explorer(Etat etatInitial,int nbAction, Func<Etat,Etat, bool> testBut)
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
                frange.RemoveAt(0);
                if (testBut(noeud.EtatNoeud,etatInitial) || noeud.Profondeur == nbAction)
                    return arbreRecherche.SequenceActions(noeud);

                // Expansion du noeud
                DejaVisites.Add(noeud.EtatNoeud);
                foreach (Noeud n in noeud.FonctionSuccession())
                {
                    frange = InsererNoeud(frange, n);
                    noeud.AjouterEnfant(n);
                }
            }
        }
        
    }
    
    public class RechercheEnLargeur : Exploration
    {
        protected override List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud)
        {
            if (!DejaVisites.Exists(n => n.Equals(noeud.EtatNoeud))) {frange.Add(noeud);}
            return frange;
        }
    }
    
    public class Astar : Exploration {

        private int DistanceManhattan(int pos1, int pos2)
        {
            return Math.Abs(pos1 % 10 - pos2 % 10) + Math.Abs(pos1 / 10 - pos2 / 10);
        }
        
        private int CalculHeuristique(Noeud noeud)
        {
            int min = 200;
            
            foreach(int x in noeud.EtatNoeud.ListePoussiere)
            {
                int distance = DistanceManhattan(noeud.EtatNoeud.Position,x);
                if (distance < min)
                {
                    min = distance;
                }
            }
            if (noeud.ActionParent == Action.ASPIRER){
                min = 0;
            }
            
            
            return min + noeud.EtatNoeud.ListePoussiere.Count + noeud.EtatNoeud.ListeBijoux.Count;
        }
        
        private static int ComparaisonAStar(Noeud n1, Noeud n2)
        {
            int evaluationN1 = n1.Heuristique + n1.CoutChemin;
            int evaluationN2 = n2.Heuristique + n2.CoutChemin;
            return evaluationN1.CompareTo(evaluationN2);
        }
        
        protected override List<Noeud> InsererNoeud(List<Noeud> frange, Noeud noeud)
        {
            noeud.Heuristique = CalculHeuristique(noeud);
            frange.Add(noeud);
            frange.Sort(ComparaisonAStar);
            return frange;
        }
    }
    
    public class Agent
    { 
        
        /* Exploration */
        private readonly Exploration _exploration;
        /* BDI */
        private readonly Bdi _bdi= new Bdi();
        /* Capteur */
        private readonly CapteurObservation _capteur = new CapteurObservation();
        /* Effecteurs */
        private readonly Effecteurs _effecteurs = new Effecteurs();
        /* environnement lié*/
        private readonly Environnement _environnement;
        /*estEnVie */
        private volatile bool _enVie = true;
        /*temps par action*/
        private const int Vitesse = 40;

        /*variables apprentissage */
        private int _dernierPerf = 0;
        private readonly List<int> _deltaPerformances = new List<int>();
        private const int TailleListePerf = 100;
        private bool _deltaNbAction = false;
        private const double Alpha = 1.5; //facteur de non prise en compte des anciens deltaPerf

        private const double Seuil = 1; // seuil de variation pour déclencher une modification de nbaction

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

                //apprentissage
                MiseAJourNbAction();
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
        }

        private void EtablirPlanDAction()
        {
            /* Calcul du nombre de poussières */
            List<int> poussieres = new List<int>();
            List<int> bijoux = new List<int>();
            for(int i = 0; i < 100; i++)
            {
                if(_bdi.Carte[i] == (int) ObjetCase.POUSSIERE || _bdi.Carte[i] == (int) ObjetCase.POUSSIEREBIJOUX) {poussieres.Add(i);}
                if(_bdi.Carte[i] == (int) ObjetCase.BIJOUX || _bdi.Carte[i] == (int) ObjetCase.POUSSIEREBIJOUX) {bijoux.Add(i);}
            }
            
            Etat etatInitial = new Etat(_bdi.Position,(ObjetCase) _bdi.Carte[_bdi.Position], poussieres, bijoux);
            _bdi.PlanDAction = _exploration.Explorer(etatInitial, _bdi.NbActions, _bdi.TestBut);
        }
        
        
        private void ExecutionPlanDAction()
        {
            //booléen pour stopper le plan en cours
            bool stop = false;
            int cpt = 0;
            
            // Exécution des actions
            while (_bdi.PlanDAction.Count != 0 && !stop)
            {
                cpt++;
                if (cpt == _bdi.NbActions)
                {
                    stop = true;
                }
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
                System.Threading.Thread.Sleep(1000 / Vitesse);
            }
        }

        //fonction d'apprentissage
        private void MiseAJourNbAction()
        {
            if (_deltaPerformances.Count == TailleListePerf)
            {
                _deltaPerformances.RemoveAt(TailleListePerf-1);
            }
            int tempPerf = _capteur.ObserverPerformance(_environnement);
            _deltaPerformances.Insert(0, tempPerf-_dernierPerf);
            _dernierPerf = tempPerf;
            double somme = 0;
            for (int i = 0; i < _deltaPerformances.Count; i++)
            {
                somme += (double)_deltaPerformances[i] / (Alpha * Math.Exp((double)i));
            }
            if (_deltaPerformances.Count == 1) { _bdi.NbActions--; }
            else if (somme < 0 && Math.Abs(somme)>Seuil)
            {
                if (_deltaNbAction) { _bdi.NbActions++; }
                else {
                    if (_bdi.NbActions > 1) { _bdi.NbActions--; }
                    }
            }else if (somme > 0 && Math.Abs(somme) > Seuil)
            {
                if (_deltaNbAction)
                {
                    if (_bdi.NbActions > 1) { _bdi.NbActions--; _deltaNbAction = false; }
                }
                else { _bdi.NbActions++; _deltaNbAction = true; }
            }
        }
        /* ------------------------------------------ */
        /*             Fonctions publiques            */
        /* ------------------------------------------ */

        public int GetPosition()
        {
            return _bdi.Position;
        }
        public int GetNbAction()
        {
            return _bdi.NbActions;
        }
    }
}