using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Colloscope
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class Page_Accueil : Page
    {
        public Page_Accueil()
        {
            this.InitializeComponent();

            Afficher_infos_accueil();
        }
        public void Afficher_infos_accueil()
        {
            StorageFile colleur = PublicSettings.colleur;
            if (colleur != null)
            {
                if (File.Exists(colleur.Name))
                {
                    using (StreamReader sr = new StreamReader(colleur.Name))
                    {
                        string line = sr.ReadLine();
                        while (line != null)
                        {
                            string[] temp = line.Split(';');
                            string Nom = temp[0];
                            string Matière = temp[1];
                            string heures = temp[2];
                            string Salle = temp[3];
                            TextBlock texte = new TextBlock();
                            texte.Text = Nom + " " + Matière + " " + heures + " " + Salle;
                            texte.Width = 300;
                            texte.Height = 23;
                            texte.HorizontalAlignment = HorizontalAlignment.Left;
                            stack.Children.Add(texte);
                            line = sr.ReadLine();
                        }
                        sr.Dispose();
                    }
                    btnColleurs.Content = colleur.DisplayName;
                }
            }
            StorageFile eleve = PublicSettings.eleve;
            if (eleve != null)
            {
                if (File.Exists(eleve.Name))
                {
                    using (StreamReader sr = new StreamReader(eleve.Name))
                    {
                        string line = sr.ReadLine();
                        int i = 0;
                        while (line != null)
                        {
                            string[] temp = line.Split(';');
                            string Nom = temp[0];
                            string Prénom = temp[1];
                            TextBlock texte = new TextBlock();
                            texte.Text = Nom + " " + Prénom;
                            texte.Width = 200;
                            texte.Height = 23;
                            texte.HorizontalAlignment = HorizontalAlignment.Left;
                            ToolTip info_eleve = new ToolTip();
                            info_eleve.Content = Fonctions_globales.contenu_popup_eleve(temp);
                            ToolTipService.SetToolTip(texte, info_eleve);
                            if (i > 30)
                            {
                                stack_eleves2.Children.Add(texte);
                            }
                            else
                            {
                                stack_eleves1.Children.Add(texte);
                            }
                            line = sr.ReadLine();
                            i++;
                        }
                        sr.Dispose();
                    }
                    btnEleves.Content = eleve.DisplayName;
                    btnEleves.IsEnabled = true;
                }
            }
        }

        private async void btnColleurs_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".csv");


            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PublicSettings.colleur = file;
            }
            Frame.Navigate(typeof(Page_Accueil));
        }

        private async void btnEleves_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".csv");


            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PublicSettings.eleve = file;
            }
            Frame.Navigate(typeof(Page_Accueil));
        }
    }
}

