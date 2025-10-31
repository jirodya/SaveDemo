﻿using System;
using System.Text.Json;
using Eto.Forms;
using Rhino;
using Rhino.PlugIns;

namespace SaveDemo
{
    public class SaveDemoPlugin : PlugIn
    {
        public static SaveDemoPlugin Instance { get; private set; }

        public PluginState State { get; private set; } = new PluginState();

        public SaveDemoPanel Window { get; private set; }

        private bool _windowClosed = true;

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

        private const string KeyName = "SaveDemoState";

        public SaveDemoPlugin()
        {
            Instance = this;

            RhinoDoc.NewDocument += OnNewDocument;
            RhinoDoc.CloseDocument += OnCloseDocument;
            RhinoDoc.EndOpenDocument += OnEndOpenDocument;
        }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                RhinoApp.Idle -= handler;
                ShowWindow();
                RhinoApp.WriteLine("SaveDemo floating window opened.");
            };

            RhinoApp.Idle += handler;
            return LoadReturnCode.Success;
        }

        // =====================================================
        // Window management
        // =====================================================
        public void ShowWindow()
        {
            Application.Instance.AsyncInvoke(() =>
            {
                // Recreate window if none or last was closed
                if (Window == null || _windowClosed)
                {
                    Window = new SaveDemoPanel();
                    _windowClosed = false;

                    Window.Closed += (s, e) =>
                    {
                        _windowClosed = true;
                        RhinoApp.WriteLine("SaveDemo: window closed.");
                    };
                }

                Window.Show();
                Window.BringToFront();
            });
        }

        // =====================================================
        // Document events
        // =====================================================
        private void OnNewDocument(object sender, DocumentEventArgs e)
        {
            State = new PluginState();
            RhinoApp.WriteLine("SaveDemo: new document, state reset.");
            ShowWindow();
        }

        private void OnCloseDocument(object sender, DocumentEventArgs e)
        {
            State = new PluginState();
            RhinoApp.WriteLine("SaveDemo: document closed, state cleared.");
        }

        private void OnEndOpenDocument(object sender, DocumentOpenEventArgs e)
        {
            RhinoApp.WriteLine("SaveDemo: document fully opened, refreshing UI.");
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;

            try
            {
                string json = doc.Strings.GetValue(Id.ToString(), KeyName);
                if (!string.IsNullOrEmpty(json))
                {
                    State = JsonSerializer.Deserialize<PluginState>(json) ?? new PluginState();
                    RhinoApp.WriteLine($"SaveDemo: state loaded (Radius={State.Radius:F1})");
                }
                else
                {
                    State = new PluginState();
                    RhinoApp.WriteLine("SaveDemo: no saved state found, using defaults.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"SaveDemo: failed to load state: {ex.Message}");
                State = new PluginState();
            }

            // Recreate geometry
            doc.Objects.Clear();
            doc.Objects.AddSphere(State.BuildGeometry());
            doc.Views.Redraw();

            // Reopen floating window safely
            Application.Instance.AsyncInvoke(() =>
            {
                ShowWindow();
                Window?.RefreshUI();
            });
        }

        // =====================================================
        // Save state manually
        // =====================================================
        public void PersistStateToDocument()
        {
            try
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null) return;

                string json = JsonSerializer.Serialize(State);
                doc.Strings.SetString(Id.ToString(), KeyName, json);
                // RhinoApp.WriteLine($"SaveDemo: state stored in active document (Radius={State.Radius:F1})");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"SaveDemo: failed to store state: {ex.Message}");
            }
        }
    }
}
