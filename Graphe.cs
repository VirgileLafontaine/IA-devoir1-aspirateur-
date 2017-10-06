using System;
using System.Collections;
using System.Collections.Generic;

namespace Aspirateur
{
    /* Classe état contenant l'état associé à un noeud */
    // Cet état contient la position de l'agent, si la case contient de la poussière et/ou un bijou et le nombre de poussières restantes
    public class Etat
    {
        public int Position { get; set; }
        
        public objetCase EtatPiece { get; set; }
        
        public int NbPoussiere { get; set; }

        public Etat(int pos, objetCase etat, int nbPoussiere)
        {
            Position = pos;
            EtatPiece = etat;
            NbPoussiere = nbPoussiere;
        }
        
    }
    
    /* Classe implémentant le noeud pour l'exploration */
    public class Noeud
    {
        // Etat du noeud
        public Etat EtatNoeud { get; set; }
        
        // Parent du noeud
        public Noeud Parent { get; set; }
        
        // Action effectuée par le noeud parent
        public Action ActionParent { get; set; }
 
        // Liste des enfants du noeud
        private List<Noeud> _listeEnfants;
        
        // Profondeur
        public int Profondeur { get; set; }
        
        // Coût de chemin
        public int CoutChemin { get; set; }

        /* Constructeur du noeud : correspond à MAKE-NODE(..) */
        // Le noeud parent est null pour le noeud racine
        // Parent : le noeud parent
        // Action : l'action effectuée par le parent
        public Noeud(Etat e, Noeud parent, Action action)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            EtatNoeud = e;
            Parent = parent;
            _listeEnfants = new List<Noeud>();
            ActionParent = action;
            if (parent != null)
            {
                Profondeur = Parent.Profondeur + 1;
                CoutChemin = Parent.CoutChemin + 1; // Une action = 1 unité d'énergie
            }
            // Cas de la racine
            else
            {
                Profondeur = 0;
                CoutChemin = 0;
            }
        }
 
        // Le nombre d'enfants du noeud
        public int NbEnfants
        {
            get
            {
                return _listeEnfants.Count;
            }
        }
 
        // Ajouter un enfant au noeud
        public void AjouterEnfant(Noeud enfant)
        {
            if (enfant == null)
            {
                throw new ArgumentNullException("enfant");
            }
            
            _listeEnfants.Add(enfant);
        }
 
        // Retourne l'enfant du noeud en fonction de son index
        public Noeud ObtenirEnfant(int index)
        {
            return _listeEnfants[index];
        }
        
        // Fonction de succession des états
        /*  En considérant le couple (position x, état de la pièce) : 
         *  - <(x, BIJOU), RAMASSER> -> (x, VIDE)
         *  - <(x, POUSSIEREBIJOU), RAMASSER> -> (x, POUSSIERE)
         *  - <(x, POUSSIERE), ASPIRER> -> (x, VIDE)
         *  - <(x, VIDE), HAUT> -> (x-10, objetCase[x-10])
         *  - <(x, VIDE), BAS> -> (x+10, objetCase[x+10])
         *  - <(x, VIDE), GAUCHE> -> (x-1, objetCase[x-1])
         *  - <(x, VIDE), DROITE> -> (x+1, objetCase[x+1])
         * Les autres possibilités sont illégales.
         */
        public List<Noeud> FonctionSuccession(int[] carte)
        {
            List<Noeud> successeurs = new List<Noeud>();
            Etat etat;
            Noeud noeud;
            switch (EtatNoeud.EtatPiece)
            {
                case objetCase.BIJOUX:
                {
                    etat = new Etat(EtatNoeud.Position,
                                 objetCase.VIDE,
                                 EtatNoeud.NbPoussiere);
                    noeud = new Noeud(etat, this, Action.RAMASSER);
                    successeurs.Add(noeud);
                    break;
                }
                case objetCase.POUSSIEREBIJOUX:
                {
                    etat = new Etat(EtatNoeud.Position,
                                 objetCase.POUSSIERE,
                                 EtatNoeud.NbPoussiere);
                    noeud = new Noeud(etat, this, Action.RAMASSER);
                    successeurs.Add(noeud);
                    break;
                }
                case objetCase.POUSSIERE:
                {
                    etat = new Etat(EtatNoeud.Position,
                                 objetCase.VIDE,
                                 EtatNoeud.NbPoussiere - 1);
                    noeud = new Noeud(etat, this, Action.ASPIRER);
                    successeurs.Add(noeud);
                    break;
                }
                case objetCase.VIDE:
                {
                    // Haut
                    if (EtatNoeud.Position >= 10)
                    {
                        etat = new Etat(EtatNoeud.Position - 10,
                                     (objetCase) carte[EtatNoeud.Position - 10],
                                     EtatNoeud.NbPoussiere);
                        noeud = new Noeud(etat, this, Action.HAUT);
                        successeurs.Add(noeud);
                    }

                    // Bas
                    if (EtatNoeud.Position < 90)
                    {
                        etat = new Etat(EtatNoeud.Position + 10,
                                     (objetCase) carte[EtatNoeud.Position + 10],
                                     EtatNoeud.NbPoussiere);
                        noeud = new Noeud(etat, this, Action.BAS);
                        successeurs.Add(noeud);
                    }

                    // Gauche
                    if ((EtatNoeud.Position % 10) != 0)
                    {
                        etat = new Etat(EtatNoeud.Position - 1,
                                     (objetCase) carte[EtatNoeud.Position - 1],
                                     EtatNoeud.NbPoussiere);
                        noeud = new Noeud(etat, this, Action.GAUCHE);
                        successeurs.Add(noeud);
                    }

                    // Droite
                    if (((EtatNoeud.Position + 1) % 10) != 0)
                    {
                        etat = new Etat(EtatNoeud.Position + 1,
                                     (objetCase) carte[EtatNoeud.Position + 1],
                                     EtatNoeud.NbPoussiere);
                        noeud = new Noeud(etat, this, Action.DROITE);
                        successeurs.Add(noeud);
                    }

                    break;
                }
            }
            return successeurs;
        }
    }
 
    // Graphe pour l'exploration
    public class Graphe
    {
        // Racine : noeud contenant l'état initial
        public Noeud Racine { get; set; }
 
        /* Constructeur du graphe */
        // Prend en paramètre l'état initial pour l'exploration et l'ensemble de la grille
        public Graphe(Etat etatInitial)
        {
            Racine = new Noeud(etatInitial,null, Action.RIEN);
        }
        
        /* Obtenir la séquence d'actions à partir d'un noeud */
        public Queue SequenceActions(Noeud n)
        {
            var actions = new Stack();

            while (n.Parent != null)
            {
                actions.Push(n.ActionParent);
                n = n.Parent;
            }
            
            var sequence = new Queue();
            foreach ( Action a in actions)
            {
                sequence.Enqueue(a);
            }
            
            return sequence;
        }
    }

}