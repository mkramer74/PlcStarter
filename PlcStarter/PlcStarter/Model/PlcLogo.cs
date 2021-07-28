﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace PlcStarter.Model
{
    public class PlcLogo : IPlc
    {
        public PlcProjekt PlcProjekte { get; set; }

        private readonly MainWindow _mainWindow;
        private readonly string _ordnerSource;
        private readonly string _ordnerDestination;

        public PlcLogo(MainWindow mainWindow, OrdnerDaten ordnerDaten)
        {
            _mainWindow = mainWindow;
            _ordnerSource = ordnerDaten.Source;
            _ordnerDestination = ordnerDaten.Destination;

            PlcProjekte =
                JsonConvert.DeserializeObject<PlcProjekt>(
                    File.ReadAllText(_ordnerSource + "/Logo8Projektliste.json"));
        }

        public void TabEigenschaftenHinzufuegen()
        {
            _mainWindow.AlleDaten.AlleTabEigenschaften.Add(
                new TabEigenschaften(PlcKategorie.Plc, Steuerungen.Logo, _mainWindow.WebLogoPlc, _mainWindow.StackPanelLogoPlc, _mainWindow.ButtonStartenLogoPlc));

            _mainWindow.AlleDaten.AlleTabEigenschaften.Add(
                new TabEigenschaften(PlcKategorie.Bug, Steuerungen.Logo, _mainWindow.WebLogoPlcBugs, _mainWindow.StackPanelLogoPlcBugs, _mainWindow.ButtonStartenLogoPlcBugs));
        }

        public void AnzeigeUpdaten(TabEigenschaften tabEigenschaften)
        {
            var fup = _mainWindow.CheckboxLogoFup?.IsChecked != null && (bool)_mainWindow.CheckboxLogoFup.IsChecked;
            var kop = _mainWindow.CheckboxLogoKop?.IsChecked != null && (bool)_mainWindow.CheckboxLogoKop.IsChecked;

            foreach (var plcProjekt in PlcProjekte.PlcProjektliste)
            {
                if (tabEigenschaften.PlcKategorie != plcProjekt.Kategorie) continue;

                if (!fup && plcProjekt.Sprache == PlcSprachen.Fup) continue;
                if (!kop && plcProjekt.Sprache == PlcSprachen.Kop) continue;

                plcProjekt.BrowserBezeichnung = tabEigenschaften.BrowserBezeichnung;
                plcProjekt.OrdnerSource = _ordnerSource;
                plcProjekt.OrdnerDestination = _ordnerDestination;

                var rdo = new RadioButton
                {
                    GroupName = "Logo",
                    Name = plcProjekt.Bezeichnung,
                    FontSize = 14,
                    Content = plcProjekt.Bezeichnung + " (" + plcProjekt.Kommentar + " / " + plcProjekt.Sprache + ")",
                    VerticalAlignment = VerticalAlignment.Top,
                    Tag = plcProjekt
                };

                rdo.Checked += _mainWindow.RadioButton_Checked;

                tabEigenschaften.StackPanelBezeichnung.Children.Add(rdo);
            }
        }

        public void ProjektStarten(ViewModel.ViewModel viewModel, Button btn)
        {
            switch (btn.Tag)
            {
                case PlcProjektdaten projektdaten:
                    foreach (var job in projektdaten.Jobs)
                    {
                        PlcJobAusfuehren(job, projektdaten, viewModel);
                    }
                    break;
            }
        }
        internal void PlcJobAusfuehren(PlcJobs job, PlcProjektdaten projektdaten, ViewModel.ViewModel viewModel)
        {
            switch (job)
            {
                case PlcJobs.None: break;
                case PlcJobs.SorceOrdnerErstellen:
                    AllePlcJobs.DestinationOrdnerErstellen(viewModel, projektdaten.OrdnerDestination, projektdaten.OrdnerProjekt);
                    break;
                case PlcJobs.ProjektKopieren:
                    AllePlcJobs.ProjektOrdnerKopieren(viewModel, projektdaten.OrdnerSource, projektdaten.OrdnerDestination, projektdaten.OrdnerProjekt);
                    break;
                case PlcJobs.CmdDateiProjektStarten:
                    AllePlcJobs.ProjektStarten(viewModel, projektdaten.OrdnerDestination, projektdaten.OrdnerProjekt);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(job), job, null);
            }
        }
    }
}