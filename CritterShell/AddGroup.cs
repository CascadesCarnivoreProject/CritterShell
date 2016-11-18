using System.Collections.Generic;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Add, "Group")]
    public class AddGroup : Cmdlet
    {
        [Parameter(HelpMessage = "An existing collection of groups to add a new group to.")]
        public Dictionary<string, List<string>> Groups { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name of the group to add.")]
        public string Name { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Names of the stations in the new group.")]
        public List<string> Stations { get; set; }

        protected override void ProcessRecord()
        {
            if (this.Groups == null)
            {
                this.Groups = new Dictionary<string, List<string>>();
            }

            List<string> stationsInGroup = new List<string>();
            foreach (string station in this.Stations)
            {
                stationsInGroup.Add(station);
            }
            this.Groups.Add(this.Name, stationsInGroup);

            this.WriteObject(this.Groups);
        }
    }
}
