using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskForge.Domain.Enums
{
    public enum InvitationStatus
    {
        Pending = 0,    // Invitation is pending (not yet accepted or declined)
        Accepted = 1,   // Invitation has been accepted
        Declined = 2,   // Invitation has been declined
        Expired = 3     // Optional: if the invitation expires after a certain period
    }
}
