using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using static Colloscope.Program;
using Windows.UI.Popups;


// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Colloscope
{

    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    class PublicSettings
    {
        private static StorageFile _eleve;
        private static StorageFile _colleur;
        private static StorageFile _collometre;
        private static string _callback;
        public static StorageFile eleve
        {
            get
            {
                return _eleve;
            }
            set
            {
                _eleve = value;
            }
        }
        public static StorageFile colleur
        {
            get
            {
                return _colleur;
            }
            set
            {
                _colleur = value;
            }
        }
        public static StorageFile collometre
        {
            get
            {
                return _collometre;
            }
            set
            {
                _collometre = value;
            }
        }
        public static string Callback
        {
            get
            {
                return _callback;
            }
            set
            {
                _callback = value;
            }
        }
    }
    class Fonctions_globales
    {
        public static string contenu_popup_groupe(string[] infos)
        {
            string contenu = "Groupe " + infos[1] + " (" + infos[0] + ")";
            for (int i = 2; i < infos.Length; i++)
            {
                contenu += "\n" + infos[i];
            }
            return contenu;
        }

        public static string contenu_popup_eleve(string[] infos)
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
            bool cinqdemi;
            string cinqde = bool.TryParse(infos[2], out cinqdemi) ? (cinqdemi ? "5/2" : "3/2") : "";
            string contenu = infos[0] + " " + infos[1] + " (" + cinqde + ")";
            int maxi = ChercheSéparateur(infos);
            contenu += "\n" + "Options:";
            for (int i = 3; i <= maxi; i++)
            {
                infos[i] = i == maxi ? infos[i].Substring(0, infos[i].Length - 1) : infos[i];
                contenu += "\n" + infos[i];
            }
            contenu += "\n" + "Indisponibilités:";
            for (int i = maxi + 1; i < infos.Length; i++)
            {
                contenu += "\n" + infos[i];
            }
            return contenu;
        }
    }
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            frame.Navigate(typeof(Page_Aide));
        }

        private void btnShowPane_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
            btnShowPane.Content = MySplitView.IsPaneOpen ? "\uE00E" : "\uE00F";
        }

        private void Accueil_Menu_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(Page_Accueil));
        }

        private void Colleur_Menu_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(Gestion_colleurs));
        }

        private void Eleve_Menu_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(Gestion_eleves));
        }

        private async void Generer_Click(object sender, RoutedEventArgs e)
        {
            if (PublicSettings.colleur != null && PublicSettings.eleve != null)
            {
                Colloscope.Principale.Debut();
            }
        }

        private void Aide_Menu_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(Page_Aide));
        }
    }
}
