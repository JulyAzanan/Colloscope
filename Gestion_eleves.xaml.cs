using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Colloscope
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class Gestion_eleves : Page
    {
        ObservableCollection<string> creneaux = new ObservableCollection<string>();
        public Gestion_eleves()
        {
            this.InitializeComponent();

            List<string> jours = new List<string> { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi" };
            foreach (string jour in jours)
            {
                for (int i = 8; i < 23; i++)
                {
                    creneaux.Add(jour + " " + i.ToString() + "h");
                }
            }
            Box_Indisponibilites.ItemsSource = creneaux;
            StorageFile eleve = PublicSettings.eleve;
            if (eleve != null)
            {
                charger.Content = "Fichier chargé : " + eleve.DisplayName;
                charger.IsEnabled = false;
                panel_eleve1.Visibility = Visibility.Visible;
                panel_eleve2.Visibility = Visibility.Visible;
            }
            Charger_Contenu(eleve);
        }

        private async void charger_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".csv" });

            savePicker.SuggestedFileName = "Nouveau Eleve";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                
                CachedFileManager.DeferUpdates(file);
                
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    PublicSettings.eleve = file;
                    Frame.Navigate(typeof(Gestion_eleves));
                }
            }
        }
        public void Charger_Contenu(StorageFile eleve)
        {
            int i = 0;
            if (eleve != null && File.Exists(eleve.Name))
            {
                using (StreamReader sr = new StreamReader(eleve.Name))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] temp = line.Split(';');
                        string Nom = temp[0];
                        string Prénom = temp[1];
                        ToolTip info_eleve = new ToolTip();
                        info_eleve.Content = Fonctions_globales.contenu_popup_eleve(temp);
                        Button texte = new Button();
                        texte.Content = Nom + " " + Prénom;
                        texte.FontSize = 9;
                        texte.VerticalContentAlignment = VerticalAlignment.Top;
                        texte.Name = i.ToString();
                        texte.Width = 300;
                        texte.Height = 25;
                        texte.HorizontalAlignment = HorizontalAlignment.Left;
                        texte.Click += suppr_Click;
                        ToolTipService.SetToolTip(texte, info_eleve);
                        if (i > 25)
                        {
                            panel_eleve2.Children.Add(texte);
                        }
                        else
                        {
                            panel_eleve1.Children.Add(texte);
                        }
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
            if (i > 25)
            {
                panel_eleve2.Children.Add(ajoute);
            }
            else
            {
                panel_eleve1.Children.Add(ajoute);
            }
        }
        private void ajoute_Click(object sender, RoutedEventArgs e)
        {
            panel_creation.Visibility = Visibility.Visible;
        }
        private async void suppr_Click(object sender, RoutedEventArgs e)
        {
            int i = int.Parse(((Button)sender).Name);
            List<string> Eleves = new List<string>();
            using (StreamReader sr = new StreamReader(PublicSettings.eleve.Name))
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
                    Eleves.Add(line);
                    j++;
                    line = sr.ReadLine();
                }
                sr.Dispose();
            }
            string contenu = Eleves[0];
            for (int k = 1; k < Eleves.Count; k++)
            {
                contenu += "\n" + Eleves[k];
            }
            await Windows.Storage.FileIO.WriteTextAsync(PublicSettings.eleve, contenu, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            Frame.Navigate(typeof(Gestion_eleves));
        }

        private async void btn_Confirmer_Click(object sender, RoutedEventArgs e)
        {
            string list_to_string(List<string> liste)
            {
                string sortie = liste[0];
                for (int i = 1; i < liste.Count; i++)
                {
                    sortie += ";" + liste[i];
                }
                return sortie;
            }
            bool okay = true;
            string Nom = Box_Nom.Text;
            Brush red = new SolidColorBrush(Windows.UI.Colors.Red);
            Brush debase = new SolidColorBrush(Windows.UI.Colors.Gray);
            if (Nom == "")
            {
                Box_Nom.BorderBrush = red;
                okay = false;
            }
            string Prénom = Box_Prenom.Text;
            if (Prénom == "")
            {
                Box_Prenom.BorderBrush = red;
                okay = false;
            }
            string cinqde = Bouton_cinqde.SelectedIndex == 0 ? "true" : "false";
            List<string> Options = new List<string>();
            if (Box_Options.SelectedItems == null)
            {
                Box_Options.BorderBrush = red;
                okay = false;
            }
            else
            {
                foreach (string item in Box_Options.SelectedItems)
                {
                    Options.Add(item);
                }
            }
            List<string> Indisponibilités = new List<string>();
            if (Box_Indisponibilites.SelectedItems == null)
            {
                Box_Indisponibilites.BorderBrush = red;
                okay = false;
            }
            else
            {
                foreach (string item in Box_Indisponibilites.SelectedItems)
                {
                    Indisponibilités.Add(item);
                }
            }
            if (okay)
            {
                Box_Nom.Text = "";
                Box_Nom.BorderBrush = debase;
                Box_Indisponibilites.SelectedValue = -1;
                Box_Indisponibilites.BorderBrush = debase;
                Box_Prenom.Text = "";
                Box_Prenom.BorderBrush = debase;
                Bouton_cinqde.SelectedValue = -1;
                Box_Options.SelectedValue = -1;
                Box_Options.BorderBrush = debase;
                string ligne = await Windows.Storage.FileIO.ReadTextAsync(PublicSettings.eleve);
                string options = list_to_string(Options) + "=";
                string indisponibilités = list_to_string(Indisponibilités);
                string contenu = ligne == "" ? Nom + ";" + Prénom + ";" + cinqde + ";" + options + ";" + indisponibilités : "\n" + Nom + ";" + Prénom + ";" + cinqde + ";" + options + ";" + indisponibilités;
                await Windows.Storage.FileIO.AppendTextAsync(PublicSettings.eleve, contenu, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                Frame.Navigate(typeof(Gestion_eleves));
            }
        }
    }
}
