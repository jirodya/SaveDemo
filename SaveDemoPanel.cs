﻿using System;
using System.Runtime.InteropServices;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Geometry;

namespace SaveDemo
{
    [Guid("902cd691-1e27-4efd-a730-367778c772f7")]
    public class SaveDemoPanel : Form
    {
        private readonly Slider _slider;
        private readonly NumericStepper _numeric;
        private readonly Label _label;
        private bool _isInitialized;

        public SaveDemoPanel()
        {
            Title = "Save Demo Panel";
            ClientSize = new Size(320, 120);
            Resizable = true;
            ShowInTaskbar = false;
            Topmost = true; // stay above Rhino, non-modal
            WindowStyle = WindowStyle.Default;

            // --- Center on screen safely (convert float → int) ---
            Application.Instance.AsyncInvoke(() =>
            {
                try
                {
                    var screen = Screen.PrimaryScreen.Bounds;
                    Location = new Eto.Drawing.Point(
                        (int)((screen.Width - ClientSize.Width) / 2) - 500,
                        (int)((screen.Height - ClientSize.Height) / 2)
                    );
                }
                catch
                {
                    // fallback: safe default
                    Location = new Eto.Drawing.Point(200, 200);
                }
            });

            // --- UI components ---
            _label = new Label { Text = "Scale", VerticalAlignment = VerticalAlignment.Center };

            _slider = new Slider
            {
                MinValue = 1,
                MaxValue = 100,
                Width = 200
            };

            _numeric = new NumericStepper
            {
                DecimalPlaces = 1,
                Increment = 0.1,
                MinValue = _slider.MinValue,
                MaxValue = _slider.MaxValue,
                Width = 80
            };

            // --- Event hookups ---
            _slider.ValueChanged += (s, e) =>
            {
                if (_isInitialized)
                {
                    double val = _slider.Value;
                    _numeric.Value = val;
                    UpdateState(val);
                }
            };

            _numeric.ValueChanged += (s, e) =>
            {
                if (_isInitialized)
                {
                    double val = _numeric.Value;
                    _slider.Value = (int)val;
                    UpdateState(val);
                }
            };

            // --- Layout (slider resizes, label + numeric fixed) ---
            var layout = new DynamicLayout { Padding = 10, Spacing = new Size(6, 6) };

            layout.AddRow(new Label
            {
                Text = "Adjust Sphere Scale",
                Font = new Font(SystemFont.Bold, 12)
            });

            layout.BeginHorizontal();
            layout.Add(_label, xscale: false);   // fixed
            layout.Add(_slider, xscale: true);   // stretches with window
            layout.Add(_numeric, xscale: false); // fixed
            layout.EndHorizontal();

            layout.Add(null); // spacer for resizing

            Content = layout;

            // Delay init to ensure Rhino and plugin are ready
            Application.Instance.AsyncInvoke(InitializeStateSafe);
        }

        private void InitializeStateSafe()
        {
            var plugin = SaveDemoPlugin.Instance;
            if (plugin == null)
            {
                Application.Instance.AsyncInvoke(InitializeStateSafe);
                return;
            }

            var state = plugin.State;
            _slider.Value = (int)state.Radius;
            _numeric.Value = state.Radius;
            _isInitialized = true;

            RhinoApp.WriteLine($"SaveDemoPanel initialized with Radius={state.Radius:F1}");
        }

        private void UpdateState(double value)
        {
            var plugin = SaveDemoPlugin.Instance;
            if (plugin == null) return;

            var state = plugin.State;
            state.Radius = value;

            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;

            doc.Objects.Clear();
            doc.Objects.AddSphere(state.BuildGeometry());
            doc.Views.Redraw();

            plugin.PersistStateToDocument();
            // RhinoApp.WriteLine($"SaveDemo: Radius updated -> {value:F1} (and persisted)");
        }

        public void RefreshUI()
        {
            var plugin = SaveDemoPlugin.Instance;
            if (plugin == null) return;

            var radius = plugin.State.Radius;
            _slider.Value = (int)radius;
            _numeric.Value = radius;

            RhinoApp.WriteLine($"SaveDemoPanel refreshed with Radius={radius:F1}");
        }

        public static void ShowFloating()
        {
            Application.Instance.AsyncInvoke(() =>
            {
                var window = new SaveDemoPanel();
                window.Show();
                window.BringToFront();
            });
        }
    }
}
