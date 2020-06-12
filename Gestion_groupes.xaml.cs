using System;
using System.Collections.Generic;
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
    public sealed partial class Gestion_groupes : Page
    {
        public Gestion_groupes()
        {
            this.InitializeComponent();

            StorageFile eleve = PublicSettings.eleve;
            if (eleve != null)
            {
                panel_eleve1.Visibility = Visibility.Visible;
                panel_eleve2.Visibility = Visibility.Visible;
                Charger_Contenu_eleve(eleve);
            }
            //Charger_Contenu(groupe);
        }

        public void Charger_Contenu_eleve(StorageFile eleve)
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
                        texte.Click += join_Click;
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
        }
        public void Charger_Contenu(StorageFile groupe)
        {
            int i = 0;
            if (groupe != null && File.Exists(groupe.Name))
            {
                using (StreamReader sr = new StreamReader(groupe.Name))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] temp = line.Split(';');
                        string Option = temp[0];
                        string Numéro = temp[1];
                        ToolTip info_groupe = new ToolTip();
                        info_groupe.Content = Fonctions_globales.contenu_popup_groupe(temp);
                        Button texte = new Button();
                        texte.Content = "Groupe " + Numéro + " (" + Option + ")";
                        texte.FontSize = 9;
                        texte.VerticalContentAlignment = VerticalAlignment.Top;
                        texte.Name = i.ToString();
                        texte.Width = 200;
                        texte.Height = 25;
                        //if (i.ToString() == PublicSettings.groupe_selected)
                        //{
                        //    Brush blue = new SolidColorBrush(Windows.UI.Colors.Blue);
                        //    texte.BorderBrush = blue;
                        //}
                        texte.HorizontalAlignment = HorizontalAlignment.Left;
                        texte.Click += selectionne_Click;
                        ToolTipService.SetToolTip(texte, info_groupe);
                        panel_creation.Children.Add(texte);
                        i++;
                        line = sr.ReadLine();
                    }
                    sr.Dispose();
                }
                Button ajoute = new Button();
                ajoute.FontFamily = new FontFamily("Segoe MDL2 Assets");
                ajoute.Content = "\uE710";
                ajoute.Width = 50;
                ajoute.Height = 50;
                ajoute.Click += ajoute_Click;
            }
        }
        private void ajoute_Click(object sender, RoutedEventArgs e)
        {

        }
        private async void join_Click(object sender, RoutedEventArgs e)
        {
            //if (PublicSettings.groupe_selected != "")
            if (true)
            {
                string numero_eleve = ((Button)sender).Name;
                List<string> Eleves = new List<string>();
                using (StreamReader sr = new StreamReader(PublicSettings.eleve.Name))
                {
                    string line = sr.ReadLine();
                    int j = 0;
                    while (line != null)
                    {
                        if (int.Parse(numero_eleve) == j)
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
                //A finir
                //Frame.Navigate(typeof(Gestion_eleves));
            }
        }
        private void selectionne_Click(object sender, RoutedEventArgs e)
        {
            string selected = ((Button)sender).Name;
            Brush blue = new SolidColorBrush(Windows.UI.Colors.Blue);
            Brush debase = new SolidColorBrush(Windows.UI.Colors.Gray);
            for (int i = 0; i < panel_creation.Children.Count - 1; i++)
            {
                ((Button)panel_creation.Children[i]).BorderBrush = debase;
            }
            ((Button)panel_creation.Children[int.Parse(selected)]).BorderBrush = blue;
            //PublicSettings.groupe_selected = selected;
        }

        private async void charger_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Nouveau Groupe";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                //await FileIO.WriteTextAsync(file, file.Name);
                // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    //PublicSettings.groupe = file;
                    Frame.Navigate(typeof(Gestion_groupes));
                }
            }
        }
    }
}
