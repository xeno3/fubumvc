using FubuMVC.Core.Registration.Nodes;

namespace FubuMVC.Core.Registration.Diagnostics
{
    public abstract class NodeEvent
    {
        public object Subject { get; set; }

        public ActionLog Source { get; set; }
    }
}