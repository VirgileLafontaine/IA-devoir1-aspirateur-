using System;
using System.Collections;
using System.Collections.Generic;

namespace Aspirateur
{
    /* Classe état contenant l'état associé à un noeud */
    /* Cet état contient
     * -  la position de l'agent,
     * -  l'état de la pièce,
     * -  la liste des cases contenant des bijoux,
     * -  et la liste des cases contenant des poussières
     */
    public class Etat
    {
        public int Position { get; set; }
        
        public ObjetCase EtatPiece { get; set; }

        public List<int> ListePoussiere;

        public List<int> ListeBijoux;

        public Etat(int pos, ObjetCase etat, List<int> poussieres, List<int> bijoux)
        {
            Position = pos;
            EtatPiece = etat;
            ListePoussiere = poussieres;
            ListeBijoux = bijoux;
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
        
        // Valeur heuristique
        public int Heuristique { get; set; }

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
            Heuristique = -1;
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
 
        public Noeud copie()
        {
            return new Noeud(EtatNoeud,Parent,ActionParent);
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
         *  - <(x, VIDE), HAUT> -> (x-10, ObjetCase[x-10])
         *  - <(x, VIDE), BAS> -> (x+10, ObjetCase[x+10])
         *  - <(x, VIDE), GAUCHE> -> (x-1, ObjetCase[x-1])
         *  - <(x, VIDE), DROITE> -> (x+1, ObjetCase[x+1])
         * Les autres possibilités sont illégales.
         */
        public List<Noeud> FonctionSuccession()
        {
            List<Noeud> successeurs = new List<Noeud>();
            Etat etat;
            Noeud noeud;
            switch (EtatNoeud.EtatPiece)
            {
                case ObjetCase.BIJOUX:
                {
                    List<int> listeBijoux = new List<int>(EtatNoeud.ListeBijoux);
                    listeBijoux.Remove(EtatNoeud.Position);
                    etat = new Etat(EtatNoeud.Position,
                        ObjetCase.VIDE,
                        EtatNoeud.ListePoussiere,
                        listeBijoux);
                    noeud = new Noeud(etat, this, Action.RAMASSER);
                    successeurs.Add(noeud);
                    break;
                }
                case ObjetCase.POUSSIEREBIJOUX:
                {
                    List<int> listeBijoux = new List<int>(EtatNoeud.ListeBijoux);
                    listeBijoux.Remove(EtatNoeud.Position);
                    etat = new Etat(EtatNoeud.Position,
                        ObjetCase.POUSSIERE,
                        EtatNoeud.ListePoussiere,
                        listeBijoux);
                    noeud = new Noeud(etat, this, Action.RAMASSER);
                    successeurs.Add(noeud);
                    break;
                }
                case ObjetCase.POUSSIERE:
                {
                    List<int> listePoussiere = new List<int>(EtatNoeud.ListePoussiere);
                    listePoussiere.Remove(EtatNoeud.Position);
                    etat = new Etat(EtatNoeud.Position,
                        ObjetCase.VIDE,
                        listePoussiere,
                        EtatNoeud.ListeBijoux);
                    noeud = new Noeud(etat, this, Action.ASPIRER);
                    successeurs.Add(noeud);
                    break;
                }
                case ObjetCase.VIDE:
                {
                    // Haut
                    if (EtatNoeud.Position >= 10)
                    {
                        int pos = EtatNoeud.Position - 10;
                        ObjetCase piece = ObjetCase.VIDE;
                        
                        if(EtatNoeud.ListePoussiere.Exists(x => x == pos) && EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.POUSSIEREBIJOUX;}
                        else if (EtatNoeud.ListePoussiere.Exists(x => x == pos)) {piece = ObjetCase.POUSSIERE;}
                        else if (EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.BIJOUX;}
                        
                        etat = new Etat(pos,
                            piece,
                            EtatNoeud.ListePoussiere,
                            EtatNoeud.ListeBijoux);
                        noeud = new Noeud(etat, this, Action.HAUT);
                        successeurs.Add(noeud);
                    }

                    // Bas
                    if (EtatNoeud.Position < 90)
                    {
                        int pos = EtatNoeud.Position + 10;
                        ObjetCase piece = ObjetCase.VIDE;
                        
                        if(EtatNoeud.ListePoussiere.Exists(x => x == pos) && EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.POUSSIEREBIJOUX;}
                        else if (EtatNoeud.ListePoussiere.Exists(x => x == pos)) {piece = ObjetCase.POUSSIERE;}
                        else if (EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.BIJOUX;}
                        
                        etat = new Etat(pos,
                            piece,
                            EtatNoeud.ListePoussiere,
                            EtatNoeud.ListeBijoux);
                        noeud = new Noeud(etat, this, Action.BAS);
                        successeurs.Add(noeud);
                    }

                    // Gauche
                    if ((EtatNoeud.Position % 10) != 0)
                    {
                        int pos = EtatNoeud.Position - 1;
                        ObjetCase piece = ObjetCase.VIDE;
                        
                        if(EtatNoeud.ListePoussiere.Exists(x => x == pos) && EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.POUSSIEREBIJOUX;}
                        else if (EtatNoeud.ListePoussiere.Exists(x => x == pos)) {piece = ObjetCase.POUSSIERE;}
                        else if (EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.BIJOUX;}
                        
                        etat = new Etat(pos,
                            piece,
                            EtatNoeud.ListePoussiere,
                            EtatNoeud.ListeBijoux);
                        noeud = new Noeud(etat, this, Action.GAUCHE);
                        successeurs.Add(noeud);
                    }

                    // Droite
                    if (((EtatNoeud.Position + 1) % 10) != 0)
                    {
                        int pos = EtatNoeud.Position + 1;
                        ObjetCase piece = ObjetCase.VIDE;
                        
                        if(EtatNoeud.ListePoussiere.Exists(x => x == pos) && EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.POUSSIEREBIJOUX;}
                        else if (EtatNoeud.ListePoussiere.Exists(x => x == pos)) {piece = ObjetCase.POUSSIERE;}
                        else if (EtatNoeud.ListeBijoux.Exists(x => x == pos)) {piece = ObjetCase.BIJOUX;}
                        
                        etat = new Etat(pos,
                            piece,
                            EtatNoeud.ListePoussiere,
                            EtatNoeud.ListeBijoux);
                        noeud = new Noeud(etat, this, Action.DROITE);
                        successeurs.Add(noeud);
                    }

                    break;
                }
            }
            return successeurs;
        }

        public override string ToString()
        {
            return "[noeud(" + EtatNoeud.EtatPiece + ", " + EtatNoeud.Position + ", " +
                   EtatNoeud.ListePoussiere +"), coût("+ CoutChemin +"), heuristique(" + 
                   Heuristique + ")]";
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