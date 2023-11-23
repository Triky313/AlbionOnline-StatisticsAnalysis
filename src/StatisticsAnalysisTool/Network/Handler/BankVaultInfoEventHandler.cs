using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class BankVaultInfoEventHandler : EventPacketHandler<BankVaultInfoEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public BankVaultInfoEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.BankVaultInfo)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(BankVaultInfoEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.VaultController.SetCurrentVault(new VaultInfo(value.ObjectId, value.LocationGuidString, value.VaultGuidList, value.VaultNames, value.IconTags));
        }

        await Task.CompletedTask;
    }
}