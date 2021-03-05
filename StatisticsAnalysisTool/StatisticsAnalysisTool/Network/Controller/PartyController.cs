using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class PartyController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<string> PartyUsers = new List<string>();

        public void AddUserToParty(string username)
        {
            if (!IsUsernameInParty(username))
            {
                PartyUsers.Add(username);
            }
        }

        public void SetParty(List<string> partyUsers)
        {
            PartyUsers = partyUsers;
        }

        public void ResetParty()
        {
            PartyUsers.Clear();
        }

        public bool IsUsernameInParty(string username)
        {
            return PartyUsers.Any(x => x.Equals(username));
        }
    }
}