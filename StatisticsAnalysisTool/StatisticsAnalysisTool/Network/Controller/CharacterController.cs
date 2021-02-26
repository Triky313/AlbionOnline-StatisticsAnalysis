using log4net;
using StatisticsAnalysisTool.Models.NetworkModel;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class CharacterController
    {
        private readonly TrackingController _trackingController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<NewCharacterObject> NewCharacterList { get; } = new List<NewCharacterObject>();

        public CharacterController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public void ResetCharacters(NewCharacterObject newCharacterObject)
        {
            NewCharacterList.Clear();
        }

        public void AddNewCharacter(NewCharacterObject newCharacterObject)
        {
            NewCharacterList.Add(newCharacterObject);
        }
    }
}