using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Colloscope
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class Page_Aide : Page
    {
        public static string premiere_utilisation = "Bienvenue sur ce logiciel de création de colloscope, conçu par Benhaim Samuel, ancien élève en MP2 au lycée Kléber. Si c'est votre première utilisation, vous pouvez observer dans la barre de menu sur la gauche plusieurs menus. Le premier permet d'élargir le menu et de voir la description de chaque icône. Le deuxième permet d'accéder au menu de création et d'édition des colleurs. Pour plus de détails, veuillez lire la rubrique correspondante. Le 3e menu est celui de création et gestion des élèves. Le 4e est celui menant à la page d'accueil, le 5e celui menant à ce menu, et enfin le dernier est le bouton de lancement de l'algorithme de génération du colloscope.";
        public static string menu_accueil = "Le menu d'accueil vous permet de charger des données d'un fichier colleur, ou d'un fichier élève créé lors d'une autre utilisation, et d'en afficher les données. Vous pourrez seulement voir ces données. Pour les modifier, il vous faudra vous rendre dans le menu correspondant (élève ou colleur).";
        public static string menu_colleurs = "Le menu colleur vous permet de créer un nouveau fichier colleur si aucun n'est actuellement chargé, sinon vous permet d'ajouter des colleurs en sélectionnant sa matière, son horaire, et en indiquant son nom et sa salle. Vous pouvez aussi supprimer un colleur existant en cliquant sur son nom.";
        public static string menu_eleves = "Le menu élève vous permet de créer un nouveau fichier élève si aucun n'est actuellement chargé, sinon vous permet d'ajouter des élèves en sélectionnant ses options, s'il est 5/2 ou non, indiquer son prénom et son nom, et indiquer les heures où il ne peut pas avoir de colles (par exemple les heures où il a cours de LV2, LV1, CamL, SI, ...). Vous pouvez aussi supprimer un élève existant en cliquant sur son nom.";
        public static string lancer_generation = "Ce bouton vous permet de lancer la génération d'un colloscope, si vous estimez avoir entré tous les détails. S'il manque des colleurs d'anglais, de physique, ou d'allemand, le logiciel vous l'indiquera et ne lancera pas la génération. Cependant, s'il manque des colleurs de maths, alors certains groupes seront simplement plus remplis que 3 élèves. S'il y a trop de colleurs de maths, alors le logiciel comblera avec des groupes fantômes. Si la génération s'effectue avec succès, un message vous l'indiquera, et une fenêtre s'ouvrira dans le répertoire où se trouve le colloscope généré. Il est conseillé de copier le pdf dans un autre dossier si celui-ci vous convient avant de fermer le dossier qui s'est ouvert.";
        public static string licence = "Copyright 2020 JulyAzanan" + "\n" + "\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the 'Software'), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" + "\n" + "\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." + "\n" + "\nTHE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

        public Page_Aide()
        {
            this.InitializeComponent();

            Zone.Text = premiere_utilisation;
        }

        private void Choix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Zone != null)
            {
                switch (Choix.SelectedIndex)
                {
                    case 0:
                        Zone.Text = premiere_utilisation;
                        break;
                    case 1:
                        Zone.Text = menu_accueil;
                        break;
                    case 2:
                        Zone.Text = menu_colleurs;
                        break;
                    case 3:
                        Zone.Text = menu_eleves;
                        break;
                    case 4:
                        Zone.Text = lancer_generation;
                        break;
                    case 5:
                        Zone.Text = licence;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
