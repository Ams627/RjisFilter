using System;
using System.Diagnostics;

namespace RjisFilter.Model
{
    class RjisFlow
    {
        public RjisFlow(string flow)
        {
            Debug.Assert(flow.Length == 8);
            FlowKey = flow;
        }
        public string FlowKey { get; private set; }

        public string GetOrigin => FlowKey.Substring(0, 4);
        public string GetDestination => FlowKey.Substring(4, 4);

        public RjisFlow GetReversedFlow() => new RjisFlow(FlowKey.Substring(4, 4) + FlowKey.Substring(0, 4));
    }
}
