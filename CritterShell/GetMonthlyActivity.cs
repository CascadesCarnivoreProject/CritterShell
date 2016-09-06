using CritterShell.Critters;
using System.Management.Automation;

namespace CritterShell
{
    [Cmdlet(VerbsCommon.Get, "MonthlyActivity")]
    public class GetMonthlyActivity : ActivityCmdlet<CritterMonthlyActivity>
    {
    }
}
