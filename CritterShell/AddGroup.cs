using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Add, "Group")]
    public class AddGroup : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public Dictionary<string, List<string>> Groups { get; set; }

        [Parameter(Mandatory = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public List<string> Stations { get; set; }

        protected override void ProcessRecord()
        {
            List<string> stationsInGroup = new List<string>();
            foreach (string station in this.Stations)
            {
                stationsInGroup.Add(station);
            }
            this.Groups.Add(this.Name, stationsInGroup);
        }
    }
}
