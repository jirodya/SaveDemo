using System;
using Rhino.Geometry;

namespace SaveDemo
{
    [Serializable]
    public class PluginState
    {
        public double Radius { get; set; } = 5.0;
        public double Height { get; set; } = 10.0;

        public Sphere BuildGeometry()
        {
            var center = new Point3d(0, 0, Height);
            return new Sphere(center, Radius);
        }
    }
}