using System.Security.Principal;

namespace PortAbuse2.Core.Common
{
    public static class Admin
    {
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
