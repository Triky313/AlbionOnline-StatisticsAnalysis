using System;
using PhotonPackageParser;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network;

public class AlbionPackageParser : PhotonParser
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly CountUpTimer _countUpTimer;

    public AlbionPackageParser(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        _countUpTimer = _trackingController?.CountUpTimer;
    }

    protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
    {
        var eventCode = ParseEventCode(parameters);
        //Debug.Print($"EventCode: {eventCode}");
        //if ((short)eventCode == 74)
        //{
        //    Debug.Print($"Event 74: {string.Join("", parameters)}");
        //}
        //Console.WriteLine("OnEvent");

        if (eventCode == EventCodes.Unused)
        {
            return;
        }

        Task.Run(async () =>
        {
            switch (eventCode)
            {
                case EventCodes.NewEquipmentItem:
                    await NewEquipmentItemEventHandlerAsync(parameters);
                    return;
                case EventCodes.OtherGrabbedLoot:
                    await OtherGrabbedLootEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.InventoryDeleteItem:
                    await InventoryDeleteItemEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.InventoryPutItem:
                    await InventoryPutItemEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.TakeSilver:
                    await TakeSilverEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.UpdateFame:
                    await UpdateFameEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.UpdateSilver:
                    await UpdateSilverEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.UpdateReSpecPoints:
                    await UpdateReSpecPointsEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.UpdateCurrency:
                    await UpdateCurrencyEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.Died:
                    await DiedEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.NewLootChest:
                    await NewLootChestEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.LootChestOpened:
                    await LootChestOpenedEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.InCombatStateUpdate:
                    await InCombatStateUpdateEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.NewShrine:
                    await NewShrineEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.HealthUpdate:
                    await HealthUpdateEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.PartyDisbanded:
                    await PartyDisbandedEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.PartyChangedOrder:
                    await PartyChangedOrderEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.NewCharacter:
                    await NewCharacterEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.SiegeCampClaimStart:
                    await SiegeCampClaimStartEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.NewMob:
                    await NewMobEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.CharacterEquipmentChanged:
                    await CharacterEquipmentChangedEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.ActiveSpellEffectsUpdate:
                    await ActiveSpellEffectsUpdateEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.UpdateFactionStanding:
                    await UpdateFactionStandingEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
                case EventCodes.ReceivedSeasonPoints:
                    await ReceivedSeasonPointsEventHandlerAsync(parameters).ConfigureAwait(false);
                    return;
            }
        });
    }

    protected override void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
    {
        var opCode = ParseOperationCode(parameters);

        Task.Run(async () =>
        {
            switch (opCode)
            {
                case OperationCodes.UseShrine:
                    await UseShrineRequestHandlerAsync(parameters);
                    return;
            }
        });
    }

    protected override void OnResponse(byte operationCode, short returnCode, string debugMessage, Dictionary<byte, object> parameters)
    {
        var opCode = ParseOperationCode(parameters);

        Task.Run(async () =>
        {
            switch (opCode)
            {
                case OperationCodes.ChangeCluster:
                    await ChangeClusterResponseHandlerAsync(parameters);
                    return;
                case OperationCodes.PartyMakeLeader:
                    await PartyMakeLeaderEventHandlerAsync(parameters);
                    return;
                case OperationCodes.Join:
                    await JoinResponseHandlerAsync(parameters);
                    return;
            }
        });
    }

    private static EventCodes ParseEventCode(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(252, out var value))
        {
            return EventCodes.Unused;
        }
        
        return (EventCodes)Enum.ToObject(typeof(EventCodes), value);
    }

    private OperationCodes ParseOperationCode(IReadOnlyDictionary<byte, object> parameters)
    {
        if (!parameters.TryGetValue(253, out var value))
        {
            return OperationCodes.Unused;
        }

        return (OperationCodes)Enum.ToObject(typeof(OperationCodes), value);
    }

    #region Handler

    #region Events

    private async Task NewEquipmentItemEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new NewEquipmentItemEvent(parameters);
        var updateFameEventHandler = new NewEquipmentItemEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task OtherGrabbedLootEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new OtherGrabbedLootEvent(parameters);
        var updateFameEventHandler = new OtherGrabbedLootEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task InventoryDeleteItemEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new InventoryDeleteItemEvent(parameters);
        var updateFameEventHandler = new InventoryDeleteItemEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task InventoryPutItemEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new InventoryPutItemEvent(parameters);
        var updateFameEventHandler = new InventoryPutItemEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task TakeSilverEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new TakeSilverEvent(parameters);

        var updateFameEventHandler = new TakeSilverEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task UpdateFameEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UpdateFameEvent(parameters);
        var updateFameEventHandler = new UpdateFameEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task UpdateSilverEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UpdateSilverEvent(parameters);
        var updateFameEventHandler = new UpdateSilverEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task UpdateReSpecPointsEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UpdateReSpecPointsEvent(parameters);
        var updateFameEventHandler = new UpdateReSpecPointsEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task UpdateCurrencyEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UpdateCurrencyEvent(parameters);
        var updateFameEventHandler = new UpdateCurrencyEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task DiedEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new DiedEvent(parameters);
        var updateFameEventHandler = new DiedEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task NewLootChestEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new NewLootChestEvent(parameters);
        var updateFameEventHandler = new NewLootChestEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task LootChestOpenedEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new LootChestOpenedEvent(parameters);
        var updateFameEventHandler = new LootChestOpenedEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task InCombatStateUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new InCombatStateUpdateEvent(parameters);
        var updateFameEventHandler = new InCombatStateUpdateEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task NewShrineEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new NewShrineEvent(parameters);
        var updateFameEventHandler = new NewShrineEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task HealthUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new HealthUpdateEvent(parameters);
        var updateFameEventHandler = new HealthUpdateEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task PartyDisbandedEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new PartyDisbandedEvent(parameters);
        var updateFameEventHandler = new PartyDisbandedEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task PartyChangedOrderEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new PartyChangedOrderEvent(parameters);
        var updateFameEventHandler = new PartyChangedOrderEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task NewCharacterEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new NewCharacterEvent(parameters);
        var updateFameEventHandler = new NewCharacterEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task SiegeCampClaimStartEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new SiegeCampClaimStartEvent(parameters);
        var updateFameEventHandler = new SiegeCampClaimStartEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task NewMobEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new NewMobEvent(parameters);
        var updateFameEventHandler = new NewMobEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task CharacterEquipmentChangedEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new CharacterEquipmentChangedEvent(parameters);
        var updateFameEventHandler = new CharacterEquipmentChangedEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task ActiveSpellEffectsUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new ActiveSpellEffectsUpdateEvent(parameters);
        var updateFameEventHandler = new ActiveSpellEffectsUpdateEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task UpdateFactionStandingEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UpdateFactionStandingEvent(parameters);
        var updateFameEventHandler = new UpdateFactionStandingEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task ReceivedSeasonPointsEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new ReceivedSeasonPointsEvent(parameters);
        var updateFameEventHandler = new ReceivedSeasonPointsEventHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    #endregion

    #region Requests

    private async Task UseShrineRequestHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new UseShrineRequest(parameters);
        var updateFameEventHandler = new UseShrineRequestHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    #endregion

    #region Response

    private async Task ChangeClusterResponseHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new ChangeClusterResponse(parameters);
        var updateFameEventHandler = new ChangeClusterResponseHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task PartyMakeLeaderEventHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new PartyMakeLeaderResponse(parameters);
        var updateFameEventHandler = new PartyMakeLeaderResponseHandler(_trackingController);
        await updateFameEventHandler.OnActionAsync(value);
    }

    private async Task JoinResponseHandlerAsync(Dictionary<byte, object> parameters)
    {
        var value = new JoinResponse(parameters);
        var updateFameEventHandler = new JoinResponseHandler(_trackingController, _mainWindowViewModel);
        await updateFameEventHandler.OnActionAsync(value);
    }

    #endregion

    #endregion
}