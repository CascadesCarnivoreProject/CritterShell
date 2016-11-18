using CritterShell.Critters;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "DielActivity")]
    public class GetDielActivity : ActivityCmdlet<CritterDielActivity>
    {
        public GetDielActivity()
        {
            this.OutputWorksheet = "diel activity";
        }
    }
}
