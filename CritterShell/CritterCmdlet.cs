using System;
using System.IO;
using System.Management.Automation;

namespace CritterShell
{
    public abstract class CritterCmdlet : PSCmdlet
    {
        protected string CanonicalizePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            if (this.SessionState != null)
            {
                return Path.GetFullPath(Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path, path));
            }
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
        }
    }
}
