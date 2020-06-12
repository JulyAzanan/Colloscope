using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Popups;
using Windows.Storage.AccessCache;
/* using iText.Layout.Splitting;
using iText.IO.Font.Otf; */

//On se donne une class Elève qui contient toutes les infos de l'élève
namespace Colloscope
{
    public class Eleves
    {
        public string Nom;
        public string Prénom;
        public bool cinqdemi;
        public List<string> options;
        public List<string> indisponibilités;
        public Eleves(string Nom, string Prénom, bool cinqdemi, List<string> options, List<string> indisponibilités)
        {
            this.Nom = Nom;
            this.Prénom = Prénom;
            this.cinqdemi = cinqdemi;
            this.options = options;
            this.indisponibilités = indisponibilités;
        }
        public bool Has_horaire(string horaire_colleur, List<string> indisp)
        {
            bool sortie = false;
            for (int i = 0; i < indisp.Count; i++)
            {
                sortie = sortie || (horaire_colleur == indisp[i]);
            }
            return sortie;
        }
    }

    //On se donne une classe Colleur qui contient toutes les infos d'un colleur 
    public class Colleur
    {
        public string Nom;
        public string Matière;
        public string Créneau;
        public string Salle;
        public Colleur(string Nom, string Matière, string Créneau, string Salle)
        {
            this.Nom = Nom;
            this.Matière = Matière;
            this.Créneau = Créneau;
            this.Salle = Salle;
        }

    }

    //On se donne une class groupes qui contient toutes les infos d'un groupe
    public class Groupes
    {
        public int numéro;
        public string spécificités;
        public List<Eleves> Membres;
        public List<string> indisponibilités_colles;
        public Groupes(int numéro, string spécificités, List<Eleves> Membres, List<string> indisponibilités_colles)
        {
            this.numéro = numéro;
            this.spécificités = spécificités;
            this.Membres = Membres;
            this.indisponibilités_colles = indisponibilités_colles;
        }
        public int Get_count()
        {
            return Membres.Count;
        }
        public Groupes Clone()
        {
            return new Groupes(this.numéro, this.spécificités, this.Membres, this.indisponibilités_colles);
        }
    }

    //Phase 1 : La lecture des fichiers csv des élèves et colleurs, et la création du colloscope sous forme de List<List<int>>.
    public class Principale
    {
        public async static void Debut()
        {
            List<Colleur> liste_colleurs = Lire_Colleurs(PublicSettings.colleur.DisplayName); //Construit la liste des colleurs
            List<Eleves> liste_élèves = Lire_Eleves(PublicSettings.eleve.DisplayName);  //Construit la liste des élèves
            Update_indisponibilités(liste_élèves);      //Met à jour la liste des indisponibilités (si cours à midi, alors peut pas avoir créneau à 13h)
            List<Groupes> liste_groupes = Construit_groupes(liste_élèves);  //Construit la liste des groupes et vide liste_élèves.
            List<List<int>> Tableau = new List<List<int>>();    //Déclare le tableau qui représente le colloscope
            List<string> matières = liste_matières(liste_colleurs);     //Construit la liste des matières en mattant les colleurs
            List<Colleur> colleurs_maths = Colleurs_par_matière(liste_colleurs, "Maths");   //Construit la liste des colleurs par matières
            List<Colleur> colleurs_physique = Colleurs_par_matière(liste_colleurs, "Physique");
            List<Colleur> colleurs_anglais = Colleurs_par_matière(liste_colleurs, "Anglais");
            List<Colleur> colleurs_allemand = Colleurs_par_matière(liste_colleurs, "Allemand");
            while (colleurs_maths.Count > liste_groupes.Count)  //Si trop de colleurs de maths, on rajoute des groupes fantômes pour combler
            {
                liste_groupes.Add(new Groupes(liste_groupes.Count, "Anglais", new List<Eleves>(), new List<string>()));
            }
            List<Groupes> groupes_anglais = groupe_par_option(liste_groupes, "Anglais"); //On crée la liste des groupes d'anglais
            List<Groupes> groupes_allemand = groupe_par_option(liste_groupes, "Allemand"); //On crée la liste des groupes d'allemand
            while (colleurs_maths.Count < liste_groupes.Count)  //Si pas assez de colleurs de maths, on explose des groupes
            {
                if (groupes_allemand.Count > groupes_anglais.Count)
                {
                    Explose_groupe_k(liste_groupes, groupes_allemand, 0);
                }
                else
                {
                    Explose_groupe_k(liste_groupes, groupes_anglais, 0);
                }
            }
            if (2 * colleurs_physique.Count < groupes_anglais.Count + groupes_allemand.Count) //Vérifie si il y a assez de colleurs de physique
            {
                PublicSettings.Callback = "Nombre de colleurs de physique insuffisant. Manquant : " + ((1 + groupes_anglais.Count + groupes_allemand.Count - 2 * colleurs_physique.Count) / 2).ToString();
                var messagedialog = new MessageDialog(PublicSettings.Callback);
                await messagedialog.ShowAsync();
                return;
            }
            if (2 * colleurs_anglais.Count < groupes_anglais.Count) //Vérifie s'il y a assez de colleurs d'anglais
            {
                PublicSettings.Callback = "Nombre de colleurs d'anglais insuffisant. Manquant : " + ((1 + groupes_anglais.Count - 2 * colleurs_anglais.Count) / 2).ToString();
                var messagedialog = new MessageDialog(PublicSettings.Callback);
                await messagedialog.ShowAsync();
                return;
            }
            if (2 * colleurs_allemand.Count < groupes_allemand.Count) //Vérifie s'il y a assez de colleurs d'anglais
            {
                PublicSettings.Callback = "Nombre de colleurs d'allemand insuffisant. Manquant : " + ((1 + groupes_allemand.Count - 2 * colleurs_allemand.Count) / 2).ToString();
                var messagedialog = new MessageDialog(PublicSettings.Callback);
                await messagedialog.ShowAsync();
                return;
            }
            renumérote_groupes(liste_groupes);
            groupes_anglais = shuffle_groupes(groupes_anglais);
            groupes_allemand = shuffle_groupes(groupes_allemand);
            int Nb_colles = 25;
            for (int j = 0; j < Nb_colles; j++)
            {
                reset_indisponibilités_éphémères(liste_groupes);
                reset_indisponibilités_éphémères(groupes_anglais);
                reset_indisponibilités_éphémères(groupes_allemand);
                List<int> semaine_k = new List<int>();
                int damnés_physique = j % 2; //Permet de savoir qui aura colle de physique cette semaine
                //Colles de maths de la semaine k :
                List<int> maths = Construit_semaine_k(colleurs_maths, shuffle_groupes(liste_groupes), "Maths");
                List<int> semainepassée = j > 0 ? Tableau[j - 1] : new List<int>(); //Permet de savoir si on a eu 2 semaine de suite le même colleur
                List<int> deuxcollesdesuite = compare_deux_semaines(maths, semainepassée); //Si oui, on essaie d'échanger la colle
                for (int compt = 0; compt < deuxcollesdesuite.Count; compt++) //deuxcollesdesuite = indice du groupe qui merde (le groupe associé au colleur k)
                {
                    Colleur colleur1 = colleurs_maths[deuxcollesdesuite[compt]];
                    for (int compt2 = 0; compt2 < maths.Count; compt2++)
                    {
                        if (semainepassée[deuxcollesdesuite[compt]] >= 0 && maths[compt2] != semainepassée[deuxcollesdesuite[compt]] && echange_colle(liste_groupes[semainepassée[deuxcollesdesuite[compt]]], liste_groupes[maths[compt2]], colleur1, colleurs_maths[maths[compt2]]))
                        {
                            int ancien = maths[compt2];
                            // liste_groupes[ancien].indisponibilités_colles[0] = colleurs_maths[deuxcollesdesuite[compt]].Créneau;
                            liste_groupes[ancien].indisponibilités_colles[0] = colleur1.Créneau;
                            maths[deuxcollesdesuite[compt]] = ancien;
                            maths[compt2] = liste_groupes[semainepassée[deuxcollesdesuite[compt]]].numéro;
                            liste_groupes[semainepassée[deuxcollesdesuite[compt]]].indisponibilités_colles[0] = colleurs_maths[compt2].Créneau;
                            Console.WriteLine("Echange de colle effectué");
                            break;
                        }
                    }
                }
                semaine_k = semaine_k.Concat(maths).ToList();
                Update_indisponibilités_éphémères(liste_groupes, groupes_allemand, groupes_anglais);
                //Colles de physique de la semaine k :
                int n_angl = (groupes_anglais.Count() / 2) * damnés_physique;
                int n_all = (groupes_allemand.Count() / 2) * damnés_physique;
                List<Groupes> ceux_physique = new List<Groupes>();
                ceux_physique = ceux_physique.Concat(groupes_anglais.GetRange(n_angl, ((groupes_anglais.Count() + damnés_physique) / 2))).ToList();
                ceux_physique = ceux_physique.Concat(groupes_allemand.GetRange(n_all, ((groupes_allemand.Count() + (1 - damnés_physique)) / 2))).ToList();
                List<int> physique = Construit_semaine_k(colleurs_physique, shuffle_groupes(ceux_physique), "Physique");
                semaine_k = semaine_k.Concat(physique).ToList();
                //Colles d'anglais de la semaine k :
                n_angl = (groupes_anglais.Count() / 2) * (1 - damnés_physique);
                List<int> anglais = Construit_semaine_k(colleurs_anglais, shuffle_groupes(groupes_anglais.GetRange(n_angl, ((groupes_anglais.Count() + (1 - damnés_physique)) / 2))), "Anglais");
                semaine_k = semaine_k.Concat(anglais).ToList();
                //Colles d'allemand de la semaine k :
                n_all = (groupes_allemand.Count() / 2) * (1 - damnés_physique);
                List<int> allemand = Construit_semaine_k(colleurs_allemand, shuffle_groupes(groupes_allemand.GetRange(n_all, ((groupes_allemand.Count() + damnés_physique) / 2))), "Allemand");
                semaine_k = semaine_k.Concat(allemand).ToList();
                Tableau.Add(semaine_k);
            }
            Console.WriteLine("Génération effectuée");
            List<List<Colleur>> tous_les_colleurs = new List<List<Colleur>>(); //On fait une liste de liste de colleurs
            tous_les_colleurs.Add(colleurs_maths);  //Comme ça on les a tous, mais triés par matières.
            tous_les_colleurs.Add(colleurs_physique);
            tous_les_colleurs.Add(colleurs_anglais);
            tous_les_colleurs.Add(colleurs_allemand);
            construit_pdf.Main(Tableau, tous_les_colleurs, liste_groupes); //Lance la génération du pdf.

        }

        //Fonction permettant la lecture du fichier des colleurs et la création de la liste des colleurs
        public static List<Colleur> Lire_Colleurs(string path)
        {
            path += ".csv";
            List<Colleur> colleurs = new List<Colleur>();
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();
                    if (line == null)       //S'il est vide, on renvoit un message d'erreur et on stop là.
                    {
                        Console.WriteLine("Fichier vide");
                        return colleurs;
                    }
                    while (line != null)
                    {
                        string[] temp = line.Split(';');
                        string Nom = temp[0];
                        string Matière = temp[1];
                        string heures = temp[2];
                        string Salle = temp[3];
                        Colleur khôlleur = new Colleur(Nom, Matière, heures, Salle);
                        colleurs.Add(khôlleur);
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
                return colleurs;
            }
            else
            {
                Console.WriteLine("Fichier non trouvé");
                return colleurs;
            }
        }
        //Fonction permettant la lecture du fichier de liste d'élève, et la génération de la liste des élèves
        public static List<Eleves> Lire_Eleves(string path)
        {
            int ChercheSéparateur(string[] a)      //Fonction auxiliaire qui cherche le séparature nécessaire entre la liste des options et celle des indisponibilités
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i].Last() == '=')
                    {
                        return i;
                    }
                }
                return -1;
            }
            path += ".csv";
            List<Eleves> élèves = new List<Eleves>();
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();
                    if (line == null)       //S'il est vide, on renvoit un message d'erreur et on stop là.
                    {
                        Console.WriteLine("Fichier vide");
                        return élèves;
                    }
                    while (line != null)
                    {
                        string[] temp = line.Split(';');
                        string Nom = temp[0];
                        string Prénom = temp[1];
                        bool cinqdemi = bool.Parse(temp[2]);
                        List<string> options = temp.ToList();
                        List<string> indisponibilités = temp.ToList();
                        int i = ChercheSéparateur(temp);          //Fonction annexe donnant l'indice du dernier élément d'option (là où est le séparateur)
                        indisponibilités.RemoveRange(0, i + 1);     //On dispose ici de la liste des indisponibilités de l'élève
                        options.RemoveRange(i + 1, options.Count - i - 1);
                        options.RemoveRange(0, 3);
                        options[options.Count - 1] = options[options.Count - 1].Remove(options[options.Count - 1].Length - 1, 1);
                        Eleves eleves = new Eleves(Nom, Prénom, cinqdemi, options, indisponibilités);
                        élèves.Add(eleves);
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
                return élèves;
            }
            else
            {
                Console.WriteLine("Fichier non trouvé");
                return élèves;
            }
        }
        //Fonction qui update les indisponibilités genre pour le midi chef

        public static void Update_indisponibilités(List<Eleves> liste_élèves)
        {
            for (int j = 0; j < liste_élèves.Count; j++)
            {
                List<string> indisponibilités = liste_élèves[j].indisponibilités;
                int sup = indisponibilités.Count;
                for (int i = 0; i < sup; i++)
                {
                    string jour = indisponibilités[i];
                    if (jour == "Lundi 12h" || jour == "Lundi 13h")
                    {
                        indisponibilités[i] = "Lundi 12h";
                        indisponibilités.Add("Lundi 13h");
                    }
                    else if (jour == "Mardi 12h" || jour == "Mardi 13h")
                    {
                        indisponibilités[i] = "Mardi 12h";
                        indisponibilités.Add("Mardi 13h");
                    }
                    else if (jour == "Mercredi 12h" || jour == "Mercredi 13h")
                    {
                        indisponibilités[i] = "Mercredi 12h";
                        indisponibilités.Add("Mercredi 13h");
                    }
                    else if (jour == "Jeudi 12h" || jour == "Jeudi 13h")
                    {
                        indisponibilités[i] = "Jeudi 12h";
                        indisponibilités.Add("Jeudi 13h");
                    }
                    else if (jour == "Vendredi 12h" || jour == "Vendredi 13h")
                    {
                        indisponibilités[i] = "Vendredi 12h";
                        indisponibilités.Add("Vendredi 13h");
                    }
                }
                liste_élèves[j].indisponibilités = indisponibilités;
            }
        }
        //Fonction construisant la liste des groupes
        public static List<Groupes> Construit_groupes(List<Eleves> liste_élèves)
        {
            List<Groupes> liste_groupes = new List<Groupes>();
            int n = liste_élèves.Count;
            int i = 0;
            while (liste_élèves.Count > 0)
            {
                Groupes groupe;
                (groupe, liste_élèves) = Construit_groupe_k_option(i, liste_élèves);
                liste_groupes.Add(groupe);
                i++;
            }
            return liste_groupes;
        }

        //Fonction qui construit le groupe d'indice k en triant selon les options. A tester.
        public static (Groupes, List<Eleves>) Construit_groupe_k_option(int k, List<Eleves> liste_élèves)
        {
            (int, List<int>) nb_occurence(List<Eleves> liste, string elem)
            {
                int s = 0;
                List<int> indice = new List<int>();
                for (int i = 0; i < liste.Count; i++)
                {
                    if (liste[i].options[0] == elem)
                    {
                        s++;
                        indice.Add(i);
                    }
                }
                return (s, indice);
            }
            int nb = 0;
            string OptionRetenue = liste_élèves[0].options[0];
            List<int> indices = new List<int>();
            (nb, indices) = nb_occurence(liste_élèves, OptionRetenue);
            int capacité; int surplus;
            (capacité, surplus) = nb % 3 == 0 ? (3, 0) : (nb / 3, nb % 3);
            if (surplus != 0 && capacité <= 3)
            {
                switch (nb)
                {
                    case 4:
                        capacité = 4;
                        surplus = 0;
                        break;
                    case 5:
                        capacité = 2;
                        surplus = 3;
                        break;
                    case 7:
                        capacité = 4;
                        break;
                    case 8:
                        capacité = 4;
                        surplus = 4;
                        break;
                    default:
                        break;
                }
            }
            else if (capacité == 0)
            {
                capacité = surplus;
            }
            else
            {
                capacité = 3;
            }
            List<Eleves> members = new List<Eleves>();
            members.Add(liste_élèves[0]);
            for (int i = 1; i < capacité; i++)
            {
                members.Add(liste_élèves[indices[i]]);
            }
            List<Eleves> àsuppr = new List<Eleves>();
            àsuppr.Add(liste_élèves[0]);
            for (int i = 0; i < capacité; i++)
            {
                àsuppr.Add(liste_élèves[indices[i]]);
            }
            for (int i = 0; i < àsuppr.Count; i++)
            {
                liste_élèves.Remove(àsuppr[i]);
            }
            Groupes groupe = new Groupes(k, OptionRetenue, members, new List<string>());
            return (groupe, liste_élèves);
        }
        //Fonction qui construit la liste des colleurs de la matière spécifiée
        public static List<Colleur> Colleurs_par_matière(List<Colleur> liste_colleurs, string matière)
        {
            List<Colleur> résultat = new List<Colleur>();
            for (int i = 0; i < liste_colleurs.Count; i++)
            {
                if (liste_colleurs[i].Matière == matière)
                {
                    résultat.Add(liste_colleurs[i]);
                }
            }
            return résultat;
        }
        //Fonction qui permet l'explosion d'un groupe et sa répartition dans les autres groupes de la même LV1
        public static void Explose_groupe_k(List<Groupes> liste_groupes, List<Groupes> groupe_langue, int k)
        {
            int nb_membres = groupe_langue[k].Get_count();
            int nb_groupes_langues = groupe_langue.Count - 1;   //on ne compte pas le groupe à dissoudre
            Groupes temp = groupe_langue[k];
            groupe_langue.RemoveAt(k);
            int i = 0;
            while (i < nb_membres && temp.Get_count() > 0)
            {
                groupe_langue[i].Membres.Add(temp.Membres[0]);
                temp.Membres.RemoveAt(0);
                i++;
                i = i == nb_groupes_langues ? i = 0 : i;
            }
            liste_groupes.Remove(temp);
        }
        //Fonction qui renumérote les groupes (utile s'il y a eu explosion de groupes)
        public static void renumérote_groupes(List<Groupes> liste_groupes)
        {
            for (int i = 0; i < liste_groupes.Count; i++)
            {
                liste_groupes[i].numéro = i;
            }
        }
        //Fonction qui construit la liste des matières à partir de celles données par la liste des colleurs
        public static List<string> liste_matières(List<Colleur> colleurs)
        {
            List<string> matières = new List<string>();
            for (int i = 0; i < colleurs.Count; i++)
            {
                if (!matières.Contains(colleurs[i].Matière))
                {
                    matières.Add(colleurs[i].Matière);
                }
            }
            return matières;
        }
        //Fonction qui mélange le groupe donné en argumant
        public static List<Groupes> shuffle_groupes(List<Groupes> groupes)
        {
            int n = groupes.Count;
            List<Groupes> shuffled = groupes.Select(book => book.Clone()).ToList();
            while (n > 1)
            {
                n--;
                Random rng = new Random();
                int k = rng.Next(n + 1);
                Groupes value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }
            return shuffled;
        }
        //Fonction qui crée la liste des groupes d'une même option
        public static List<Groupes> groupe_par_option(List<Groupes> groupes, string option)
        {
            List<Groupes> groupe_option = new List<Groupes>();
            foreach (Groupes groupe in groupes)
            {
                if (groupe.spécificités == option)
                {
                    groupe_option.Add(groupe);
                }
            }
            return groupe_option;
        }
        //Fonction qui indique si une colle est possible pour le groupe spécifié avec le colleur spécifié
        public static bool Colle_possible(Groupes groupe, Colleur colleur)
        {
            foreach (Eleves membre in groupe.Membres)
            {
                if (membre.Has_horaire(colleur.Créneau, membre.indisponibilités))
                {
                    return false;
                }
            }
            // return true;
            return !(groupe.indisponibilités_colles.Contains(colleur.Créneau));
        }

        //Le tableau de la semaine k contient à l'indice i le groupe qui collera avec le colleur i
        public static List<int> Construit_semaine_k(List<Colleur> colleurs, List<Groupes> groupes, string Matière)
        {
            List<int> semaineK = new List<int>();
            Groupes[] temp = new Groupes[groupes.Count];
            Groupes[] temp2 = new Groupes[groupes.Count];
            groupes.CopyTo(temp);
            groupes.CopyTo(temp2);
            List<Groupes> groupe_copy = temp.ToList();
            List<Groupes> groupes2 = temp2.ToList();
            List<(Colleur, int)> Colleurs_unmatched = new List<(Colleur, int)>();
            for (int k = 0; k < colleurs.Count; k++)
            {
                bool matched = false;
                if (groupe_copy.Count == 0)
                {
                    break;
                }
                for (int i = 0; i < groupe_copy.Count; i++)
                {
                    bool faisable = Colle_possible(groupe_copy[i], colleurs[k]);
                    if (faisable && !semaineK.Contains(groupe_copy[i].numéro))
                    {
                        semaineK.Add(groupe_copy[i].numéro);
                        groupes[i].indisponibilités_colles.Add(colleurs[k].Créneau);
                        groupes.RemoveAt(i);
                        groupe_copy.RemoveAt(i);
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    Colleurs_unmatched.Add((colleurs[k], k));
                    semaineK.Add(-1);
                }
            }
            for (int i = 0; i < groupe_copy.Count; i++)
            {
                (Colleur colleur, int k) = Colleurs_unmatched[i];
                for (int j = 0; j < semaineK.Count; j++)
                {
                    if (semaineK[j] >= 0 && j < groupes2.Count && echange_colle(groupe_copy[i], groupes2[j], colleur, colleurs[j]))
                    {
                        int ancien = semaineK[j];
                        groupes2[j].indisponibilités_colles.Add(colleur.Créneau);
                        semaineK[k] = ancien;
                        semaineK[j] = groupe_copy[i].numéro;
                        groupe_copy[i].indisponibilités_colles.Add(colleurs[j].Créneau);
                        break;
                    }
                }
            }
            while (semaineK.Count < colleurs.Count)
            {
                semaineK.Add(-1);
            }
            return semaineK;

        }
        //Fonction permettant de savoir s'il est possible déchanger deux colles
        public static bool echange_colle(Groupes groupe1, Groupes groupe2, Colleur colleur1, Colleur colleur2) //Groupe 1 et colleur 1 sont les défaillants
        {               //On doit donc tester les combis groupe2 colleur1 et groupe1 colleur2
            return (Colle_possible(groupe2, colleur1) && Colle_possible(groupe1, colleur2));
        }
        //Fonction qui donne la liste des colles qui sont avec les même colleurs 2 semaines de suite
        public static List<int> compare_deux_semaines(List<int> semaine1, List<int> semaine2)
        {
            List<int> résultat = new List<int>();
            if (semaine2.Count == 0)
            {
                return résultat;
            }
            for (int i = 0; i < semaine1.Count; i++)
            {
                if (semaine1[i] == semaine2[i])
                {
                    résultat.Add(i);
                }
            }
            return résultat;
        }
        //Fonction permettant de faire en sorte qu'un groupe ne puisse pas coller avec 2 colleurs au même moments, ni 2 fois entre midi
        public static void Update_indisponibilités_éphémères(List<Groupes> groupes, List<Groupes> allemands, List<Groupes> anglais)
        {
            void modifie_dans_langues(Groupes groupe, List<Groupes> langues)
            {
                for (int i = 0; i < langues.Count; i++)
                {
                    if (groupe.numéro == langues[i].numéro)
                    {
                        langues[i].indisponibilités_colles = groupe.indisponibilités_colles;
                        break;
                    }
                }
            }
            for (int j = 0; j < groupes.Count; j++)
            {
                List<string> indisponibilités = groupes[j].indisponibilités_colles;
                int sup = indisponibilités.Count;
                for (int i = 0; i < sup; i++)
                {
                    string jour = indisponibilités[i];
                    if (jour == "Lundi 12h" || jour == "Lundi 13h")
                    {
                        indisponibilités[i] = "Lundi 12h";
                        indisponibilités.Add("Lundi 13h");
                    }
                    else if (jour == "Mardi 12h" || jour == "Mardi 13h")
                    {
                        indisponibilités[i] = "Mardi 12h";
                        indisponibilités.Add("Mardi 13h");
                    }
                    else if (jour == "Mercredi 12h" || jour == "Mercredi 13h")
                    {
                        indisponibilités[i] = "Mercredi 12h";
                        indisponibilités.Add("Mercredi 13h");
                    }
                    else if (jour == "Jeudi 12h" || jour == "Jeudi 13h")
                    {
                        indisponibilités[i] = "Jeudi 12h";
                        indisponibilités.Add("Jeudi 13h");
                    }
                    else if (jour == "Vendredi 12h" || jour == "Vendredi 13h")
                    {
                        indisponibilités[i] = "Vendredi 12h";
                        indisponibilités.Add("Vendredi 13h");
                    }
                }
                groupes[j].indisponibilités_colles = indisponibilités;
                List<Groupes> langue = groupes[j].spécificités == "Anglais" ? anglais : allemands;
                modifie_dans_langues(groupes[j], langue);
            }
        }
        //fonction permettant de remettre à 0 les indisponibilités hebdomadaires
        public static void reset_indisponibilités_éphémères(List<Groupes> groupes)
        {
            for (int i = 0; i < groupes.Count; i++)
            {
                groupes[i].indisponibilités_colles = new List<string>();
            }
        }
    }

    //Phase 2 : La construction du PDF.
    public class construit_pdf
    {
        public static readonly string dest = "Collometre.pdf";
        public async static void Main(List<List<int>> Tableau, List<List<Colleur>> colleurs, List<Groupes> groupes)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await ApplicationData.Current.LocalFolder.GetFileAsync(dest) != null)
            {
                StorageFile fichier = await ApplicationData.Current.LocalFolder.GetFileAsync(dest);
                await fichier.DeleteAsync();
            }
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(dest);
            PublicSettings.collometre = file;
            

             new construit_pdf().Manipulate_Pdf(dest, Tableau, colleurs, groupes);
        }

        private async void Manipulate_Pdf(string dest, List<List<int>> Tableau, List<List<Colleur>> colleurs, List<Groupes> groupes)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(PublicSettings.collometre.Path));
            Document doc = new Document(pdfDoc);
            doc.SetMargins(0, 0, 0, 0);
            Table Global = new Table(new float[2]);

            int n = 0;
            for (int i = 0; i < colleurs.Count; i++)
            {
                n += colleurs[i].Count;
            }
            Table table = new Table(new float[n + 1]);
            table.SetMarginTop(10f);
            table.SetMarginLeft(10f);
            table.SetMarginBottom(0);

            table.AddCell(new Cell().SetWidth(30f).Add(new Paragraph("Collomètre")));
            for (int i = 0; i < colleurs.Count; i++)
            {
                Cell cell = new Cell(1, colleurs[i].Count).Add(new Paragraph(colleurs[i][0].Matière));
                cell.SetTextAlignment(TextAlignment.CENTER);
                table.AddCell(cell);
            }

            table.AddCell(new Cell().SetBorderBottom(Border.NO_BORDER));
            for (int i = 0; i < colleurs.Count; i++)
            {
                for (int j = 0; j < colleurs[i].Count; j++)
                {
                    Cell cell = new Cell().Add(new Paragraph(colleurs[i][j].Créneau));
                    cell.SetTextAlignment(TextAlignment.RIGHT);
                    cell.SetVerticalAlignment(VerticalAlignment.BOTTOM);
                    cell.SetRotationAngle(Math.PI / 2);
                    cell.SetFontSize(6f);
                    table.AddCell(cell);
                }
            }
            table.AddCell(new Cell().SetBorderBottom(Border.NO_BORDER).SetBorderTop(Border.NO_BORDER));
            for (int i = 0; i < colleurs.Count; i++)
            {
                for (int j = 0; j < colleurs[i].Count; j++)
                {
                    Cell cell = new Cell().Add(new Paragraph(colleurs[i][j].Nom));
                    cell.SetTextAlignment(TextAlignment.RIGHT);
                    cell.SetVerticalAlignment(VerticalAlignment.BOTTOM);
                    cell.SetRotationAngle(Math.PI / 2);
                    cell.SetFontSize(6f);
                    table.AddCell(cell);
                }
            }
            table.AddCell(new Cell().SetBorderTop(Border.NO_BORDER));
            for (int i = 0; i < colleurs.Count; i++)
            {
                for (int j = 0; j < colleurs[i].Count; j++)
                {
                    Cell cell = new Cell().Add(new Paragraph(colleurs[i][j].Salle));
                    cell.SetTextAlignment(TextAlignment.CENTER);
                    cell.SetVerticalAlignment(VerticalAlignment.BOTTOM);
                    cell.SetRotationAngle(Math.PI / 2);
                    cell.SetFontSize(6f);
                    table.AddCell(cell);
                }
            }

            for (int i = 0; i < Tableau.Count; i++)
            {
                table.AddCell("Semaine " + (i + 1).ToString()).SetFontSize(6f);
                for (int j = 0; j < Tableau[i].Count; j++)
                {
                    string spot = Tableau[i][j] == -1 ? " " : groupes[Tableau[i][j]].Get_count() == 0 ? "F" : (Tableau[i][j] + 1).ToString();
                    Cell cell = new Cell().Add(new Paragraph(spot));
                    cell.SetTextAlignment(TextAlignment.CENTER);
                    cell.SetFontSize(6f);
                    table.AddCell(cell);
                }
            }

            Global.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table));

            Table table2 = new Table(new float[1]).SetFontSize(5f).SetMarginRight(20f).SetMarginLeft(10f).SetMarginTop(10f).SetMarginBottom(30f);
            for (int i = 0; i < groupes.Count; i++)
            {
                table2.AddCell(new Cell().Add(new Paragraph("Groupe" + "\u00A0" + (groupes[i].numéro + 1).ToString())).SetTextAlignment(TextAlignment.CENTER).SetBold());
                for (int j = 0; j < groupes[i].Get_count(); j++)
                {
                    table2.AddCell(groupes[i].Membres[j].Nom + "\u00A0" + groupes[i].Membres[j].Prénom);
                }
                if (groupes[i].Get_count() == 0)
                {
                    table2.AddCell("Fantôme");
                }
            }
            Global.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(table2));
            doc.Add(Global);

            doc.Close();

            PublicSettings.Callback = "Génération effectuée avec succès !";
            var messagedialog = new MessageDialog(PublicSettings.Callback);
            await messagedialog.ShowAsync();
            await Windows.System.Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }
    }
}