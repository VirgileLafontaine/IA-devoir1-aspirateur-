using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

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
    // TO DO : je pense qu'il faudra remodeler les désirs pour les mettre sous forme de fonctions afin de les utiliser directement pour choisir un plan d'action parmi ceux possibles
            
    // 1. Maximiser le nombre de poussières aspirées
    public int NbPoussieres = 0;
            
    // 2. Ajouter une règle interdisant d'aspirer un bijou
    public int NbBijouxAspires = 0;
            
    // 3. Minimiser le coût du plan d'action MAIS impossible car en fait unplan d'action a un coût constant = nb d'actions (à mettre dans le rapport)

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
    public int[] ObserverCarte()
    {
        return new int[100];
    }
}

/* Effecteurs */
public class Effecteurs
{
    // Effecteur ASPIRER
    public void Aspirer(int position)
    {
        Console.WriteLine("Effecteur aspirer");
        // Notifier l'environnement qu'on aspire la pièce X
    }
    
    // Effecteur RAMASSER
    public void Ramasser(int position)
    {
        Console.WriteLine("Effecteur ramasser");
        // Notifier l'environnement qu'on ramasse un bijou dans la pièce X
    }
    
    //Effecteurs de déplacement : HAUT, BAS, DROITE, GAUCHE
    // Retour :        la nouvelle position de l'agent
    //                 position en cas d'erreur (mouvement impossible)
    public int Haut(int position)
    {
        Console.WriteLine("Effecteur haut");
        if (position >= 10) return position - 10;
        else return position;
    }
    
    public int Bas(int position)
    {
        Console.WriteLine("Effecteur bas");
        if (position < 90) return position + 10;
        else return position;
    }
    
    public int Gauche(int position)
    {
        Console.WriteLine("Effecteur gauche");
        if ((position % 10) != 0) return position - 1;
        else return position;
    }
    
    public int Droite(int position)
    {
        Console.WriteLine("Effecteur droite");
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
public enum Action {ASPIRER, RAMASSER, HAUT, BAS, DROITE, GAUCHE} ;

// NOTE : je pense qu'il faut mettre l'environnement en classe statique (une seule instance modifiée par des appels de méthode static)
namespace Aspirateur
{
    
    public class Agent
    {
        /* BDI */
        private Bdi _bdi= new Bdi();
        /* Capteur */
        private CapteurObservation _capteur = new CapteurObservation();
        /* Effecteurs */
        private Effecteurs _effecteurs = new Effecteurs();
        
        /* ------------------------------------------ */
        /*            Fonction principale             */
        /* ------------------------------------------ */
        public void Lancer()
        {
            while (JeSuisEnVie())
            {
                // Observer l'environnement avec mes capteurs
                int[] carteActuelle = _capteur.ObserverCarte();
                
                // Mettre à jour mon état (believes de mon BDI)
                MettreAJourBdi(carteActuelle);
                
                // Etablir le plan d'action
                EtablirPlanDAction();
                
                // Exécution du plan d'action
                ExecutionPlanDAction();

            }
        }
        
        /* ------------------------------------------ */
        /*            Fonctions internes              */
        /* ------------------------------------------ */
        private bool JeSuisEnVie()
        {
            return true; // Quand est ce qu'un agent meurt ?
        }

        private void MettreAJourBdi(int[] carteObservee)
        {
            // Actualiser la carte
            Array.Copy(carteObservee,_bdi.Carte,100);
            
            // Apprentissage : actualiser NbActions
        }

        private void EtablirPlanDAction()
        {
            _bdi.PlanDAction.Enqueue(Action.ASPIRER);
            _bdi.PlanDAction.Enqueue(Action.DROITE);
            _bdi.PlanDAction.Enqueue(Action.GAUCHE);
            _bdi.PlanDAction.Enqueue(Action.BAS);
            _bdi.PlanDAction.Enqueue(Action.HAUT);
            _bdi.PlanDAction.Enqueue(Action.RAMASSER);
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
                        _effecteurs.Aspirer(_bdi.Position);
                        break;
                    case (int) Action.RAMASSER:
                        _effecteurs.Ramasser(_bdi.Position);
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