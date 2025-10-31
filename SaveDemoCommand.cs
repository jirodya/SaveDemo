using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace SaveDemo
{
    public class ShowSaveDemoPanelCommand : Command
    {
        public override string EnglishName => "ShowSaveDemoPanel";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Panels.OpenPanel(typeof(SaveDemoPanel).GUID);
            RhinoApp.WriteLine("SaveDemo panel opened.");
            return Result.Success;
        }
    }
}