﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspirateur
{
    class Environnement
    {
        //--------------variables privées de l'environnement-------------------//

        ///constantes des objets dans une piece, VIDE = 0, POUSSIERE = 1 ...
        private enum objetCase { VIDE, POUSSIERE, BIJOUX, POUSSIEREBIJOUX };

        ///carte de l'environnement et des objets présents dans les pieces de cet environnement
        private int[] carte = new int[100];

        ///Mesure de performance, 0 = meilleur
        private int mesurePerformance = 0;


        //---------------------Constructeurs----------------------------------//

        ///constructeur par défaut
        public Environnement()
        {

        }

        //-----------------------getters---------------------------------------//
        public int getMesurePerformance()
        {
            return this.mesurePerformance;
        }

    }
}
