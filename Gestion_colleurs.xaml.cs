using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Collections.ObjectModel;
using Windows.Devices.PointOfService;
using System.Text;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Colloscope
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class Gestion_colleurs : Page
    {
        ObservableCollection<string> creneaux = new ObservableCollection<string>();
        public Gestion_colleurs()
        {
            this.InitializeComponent();

            List<string> jours = new List<string> {"Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi" };
            foreach (string jour in jours)
            {
                for (int i = 8; i < 23; i++)
                {
                    creneaux.Add(jour + " " + i.ToString() + "h");
                }
            }
            Box_Horaires.ItemsSource = creneaux;
            StorageFile colleur = PublicSettings.colleur;
            if (colleur != null)
            {
                charger.Content = "Fichier chargé : " + colleur.DisplayName;
                charger.IsEnabled = false;
                panel_colleur.Visibility = Visibility.Visible;
            }
            Charger_Contenu(colleur);
        }

        private async void charger_Click(object sender, RoutedEventArgs e)
        {

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Nouveau Colleur";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    PublicSettings.colleur = file;
                    Frame.Navigate(typeof(Gestion_colleurs));
                }
            }
        }

        public void Charger_Contenu(StorageFile colleur)
        {
            if (colleur != null && File.Exists(colleur.Name))
            {
                using (StreamReader sr = new StreamReader(colleur.Name))
                {
                    string line = sr.ReadLine();
                    int i = 0;
                    while (line != null)
                    {
                        string[] temp = line.Split(';');
                        string Nom = temp[0];
                        string Matière = temp[1];
                        string heures = temp[2];
                        string Salle = temp[3];
                        Button texte = new Button();
                        texte.Content = Nom + " " + Matière + " " + heures + " " + Salle;
                        texte.Name = i.ToString();
                        texte.FontSize = 9;
                        texte.VerticalContentAlignment = VerticalAlignment.Top;
                        texte.Width = 300;
                        texte.Height = 25;
                        texte.HorizontalAlignment = HorizontalAlignment.Left;
                        texte.Click += suppr_Click;
                        panel_colleur.Children.Add(texte);
                        i++;
                        line = sr.ReadLine();
                    }
                    sr.Dispose();
                }
            }
            Button ajoute = new Button();
            ajoute.FontFamily = new FontFamily("Segoe MDL2 Assets");
            ajoute.Content = "\uE710";
            ajoute.Width = 50;
            ajoute.Height = 50;
            ajoute.Click += ajoute_Click;
            panel_colleur.Children.Add(ajoute);
        }

        private void ajoute_Click(object sender, RoutedEventArgs e)
        {
            panel_creation.Visibility = Visibility.Visible;
        }

        private async void btn_Confirmer_Click(object sender, RoutedEventArgs e)
        {
            bool okay = true;
            string Nom = Box_Nom.Text;
            Brush red = new SolidColorBrush(Windows.UI.Colors.Red);
            Brush debase = new SolidColorBrush(Windows.UI.Colors.Gray);
            if (Nom == "")
            {
                Box_Nom.BorderBrush = red;
                okay = false;
            }
            string Matière = "";
            if (Box_Matieres.SelectedItem == null)
            {
                Box_Matieres.BorderBrush = red;
                okay = false;
            }
            else
            {
                Matière = Box_Matieres.SelectedItem.ToString();
            }
            string Horaire = "";
            if (Box_Horaires.SelectedItem == null)
            {
                Box_Horaires.BorderBrush = red;
                okay = false;
            }
            else
            {
                Horaire = Box_Horaires.SelectedItem.ToString();
            }
            string Salle = Box_Salle.Text;
            if (Salle == "")
            {
                Box_Salle.BorderBrush = red;
                okay = false;
            }
            if (okay)
            {
                Box_Salle.Text = "";
                Box_Salle.BorderBrush = debase;
                Box_Horaires.SelectedValue = -1;
                Box_Horaires.BorderBrush = debase;
                Box_Nom.Text = "";
                Box_Nom.BorderBrush = debase;
                Box_Matieres.SelectedValue = -1;
                Box_Matieres.BorderBrush = debase;
                string ligne = await Windows.Storage.FileIO.ReadTextAsync(PublicSettings.colleur);
                string contenu = ligne == "" ? Nom + ";" + Matière + ";" + Horaire + ";" + Salle : "\n" + Nom + ";" + Matière + ";" + Horaire + ";" + Salle;
                await Windows.Storage.FileIO.AppendTextAsync(PublicSettings.colleur, contenu, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                Frame.Navigate(typeof(Gestion_colleurs));
            }
        }
        private async void suppr_Click(object sender, RoutedEventArgs e)
        {
            int i = int.Parse(((Button)sender).Name);
            List<string> Colleurs = new List<string>();
            using (StreamReader sr = new StreamReader(PublicSettings.colleur.Name))
            {
                string line = sr.ReadLine();
                int j = 0;
                while (line != null)
                {
                    if (i == j)
                    {
                        line = sr.ReadLine();
                        j++;
                        continue;
                    }
                    Colleurs.Add(line);
                    j++;
                    line = sr.ReadLine();
                }
                sr.Dispose();
            }
            string contenu = Colleurs[0];
            for (int k = 1; k < Colleurs.Count; k++)
            {
                contenu += "\n" + Colleurs[k];
            }
            await Windows.Storage.FileIO.WriteTextAsync(PublicSettings.colleur, contenu, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            Frame.Navigate(typeof(Gestion_colleurs));
        }
    }
}
