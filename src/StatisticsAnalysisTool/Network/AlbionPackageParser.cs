using System;
using PhotonPackageParser;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Operations.Requests;

namespace StatisticsAnalysisTool.Network
{
    public class AlbionPackageParser : PhotonParser
    {
        private readonly NewEquipmentItemEventHandler _newEquipmentItemEventHandler;
        private readonly OtherGrabbedLootEventHandler _grabbedLootEventHandler;
        private readonly InventoryDeleteItemEventHandler _inventoryDeleteItemEventHandler;
        private readonly InventoryPutItemEventHandler _inventoryPutItemEventHandler;
        private readonly TakeSilverEventHandler _takeSilverEventHandler;
        private readonly UpdateFameEventHandler _updateFameEventHandler;
        private readonly UpdateSilverEventHandler _updateSilverEventHandler;
        private readonly UpdateReSpecPointsEventHandler _updateReSpecPointsEventHandler;
        private readonly UpdateCurrencyEventHandler _updateCurrencyEventHandler;
        private readonly DiedEventHandler _diedEventHandler;
        private readonly NewLootChestEventHandler _newLootChestEventHandler;
        private readonly LootChestOpenedEventHandler _lootChestOpenedEventHandler;
        private readonly InCombatStateUpdateEventHandler _inCombatStateUpdateEventHandler;
        private readonly NewShrineEventHandler _newShrineEventHandler;
        private readonly HealthUpdateEventHandler _healthUpdateEventHandler;
        private readonly PartyPlayerJoinedEventHandler _partyPlayerJoinedHandler;
        private readonly PartyChangedOrderEventHandler _partyChangedOrderEventHandler;
        private readonly NewCharacterEventHandler _newCharacterEventHandler;
        private readonly SiegeCampClaimStartEventHandler _siegeCampClaimStartEventHandler;
        private readonly CharacterEquipmentChangedEventHandler _characterEquipmentChangedEventHandler;
        private readonly NewMobEventHandler _newMobEventHandler;
        private readonly ActiveSpellEffectsUpdateEventHandler _activeSpellEffectsUpdateEventHandler;
        private readonly UpdateFactionStandingEventHandler _updateFactionStandingEventHandler;
        private readonly ReceivedSeasonPointsEventHandler _receivedSeasonPointsEventHandler;
        private readonly MightFavorPointsEventHandler _mightFavorPointsEventHandler;

        private readonly UseShrineRequestHandler _useShrineRequestHandler;

        private readonly ChangeClusterResponseHandler _changeClusterResponseHandler;
        private readonly PartyMakeLeaderResponseHandler _partyMakeLeaderResponseHandler;
        private readonly JoinResponseHandler _joinResponseHandler;
        private readonly GetMailInfosResponseHandler _getMailInfosResponseHandler;
        private readonly ReadMailResponseHandler _readMailResponseHandler;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public AlbionPackageParser(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _newEquipmentItemEventHandler = new NewEquipmentItemEventHandler(trackingController);
            _grabbedLootEventHandler = new OtherGrabbedLootEventHandler(trackingController);
            _inventoryDeleteItemEventHandler = new InventoryDeleteItemEventHandler(trackingController);
            _inventoryPutItemEventHandler = new InventoryPutItemEventHandler(trackingController);
            _takeSilverEventHandler = new TakeSilverEventHandler(trackingController);
            _updateFameEventHandler = new UpdateFameEventHandler(trackingController);
            _updateSilverEventHandler = new UpdateSilverEventHandler(trackingController);
            _updateReSpecPointsEventHandler = new UpdateReSpecPointsEventHandler(trackingController);
            _updateCurrencyEventHandler = new UpdateCurrencyEventHandler(trackingController);
            _diedEventHandler = new DiedEventHandler(trackingController);
            _newLootChestEventHandler = new NewLootChestEventHandler(trackingController);
            _lootChestOpenedEventHandler = new LootChestOpenedEventHandler(trackingController);
            _inCombatStateUpdateEventHandler = new InCombatStateUpdateEventHandler(trackingController);
            _newShrineEventHandler = new NewShrineEventHandler(trackingController);
            _healthUpdateEventHandler = new HealthUpdateEventHandler(trackingController);
            _partyPlayerJoinedHandler = new PartyPlayerJoinedEventHandler(trackingController);
            _partyChangedOrderEventHandler = new PartyChangedOrderEventHandler(trackingController);
            _newCharacterEventHandler = new NewCharacterEventHandler(trackingController);
            _siegeCampClaimStartEventHandler = new SiegeCampClaimStartEventHandler(trackingController);
            _characterEquipmentChangedEventHandler = new CharacterEquipmentChangedEventHandler(trackingController);
            _newMobEventHandler = new NewMobEventHandler(trackingController);
            _activeSpellEffectsUpdateEventHandler = new ActiveSpellEffectsUpdateEventHandler(trackingController);
            _updateFactionStandingEventHandler = new UpdateFactionStandingEventHandler(trackingController);
            _receivedSeasonPointsEventHandler = new ReceivedSeasonPointsEventHandler(trackingController);
            _mightFavorPointsEventHandler = new MightFavorPointsEventHandler(trackingController);

            _useShrineRequestHandler = new UseShrineRequestHandler(trackingController);

            _changeClusterResponseHandler = new ChangeClusterResponseHandler(trackingController);
            _partyMakeLeaderResponseHandler = new PartyMakeLeaderResponseHandler(trackingController);
            _joinResponseHandler = new JoinResponseHandler(trackingController, mainWindowViewModel);
            _getMailInfosResponseHandler = new GetMailInfosResponseHandler(trackingController);
            _readMailResponseHandler = new ReadMailResponseHandler(trackingController);
        }

        #region Actions

        protected override async void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
            var eventCode = ParseEventCode(parameters);

            if (eventCode == EventCodes.Unused)
            {
                return;
            }

            try
            {
                switch (eventCode)
                {
                    case EventCodes.NewEquipmentItem:
                        await NewEquipmentItemEventHandlerAsync(parameters);
                        return;
                    case EventCodes.GrabbedLoot:
                        await GrabbedLootEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.InventoryDeleteItem:
                        await InventoryDeleteItemEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.InventoryPutItem:
                        await InventoryPutItemEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.TakeSilver:
                        await TakeSilverEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.UpdateFame:
                        await UpdateFameEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.UpdateSilver:
                        await UpdateSilverEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.UpdateReSpecPoints:
                        await UpdateReSpecPointsEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.UpdateCurrency:
                        await UpdateCurrencyEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.Died:
                        await DiedEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.NewLootChest:
                        await NewLootChestEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.LootChestOpened:
                        await LootChestOpenedEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.InCombatStateUpdate:
                        await InCombatStateUpdateEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.NewShrine:
                        await NewShrineEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.HealthUpdate:
                        await HealthUpdateEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.PartyPlayerJoined:
                        await PartyPlayerJoinedHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.PartyChangedOrder:
                        await PartyChangedOrderEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.NewCharacter:
                        await NewCharacterEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.SiegeCampClaimStart:
                        await SiegeCampClaimStartEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.NewMob:
                        await NewMobEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.CharacterEquipmentChanged:
                        await CharacterEquipmentChangedEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.ActiveSpellEffectsUpdate:
                        await ActiveSpellEffectsUpdateEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.UpdateFactionStanding:
                        await UpdateFactionStandingEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.ReceivedSeasonPoints:
                        await ReceivedSeasonPointsEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                    case EventCodes.MightFavorPoints:
                        await MightFavorPointsEventHandlerAsync(parameters).ConfigureAwait(true);
                        return;
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                Log.Error(nameof(OnEvent), ex);
            }
        }

        protected override void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            var opCode = ParseOperationCode(parameters);

            if (opCode == OperationCodes.Unused)
            {
                return;
            }

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

            if (opCode == OperationCodes.Unused)
            {
                return;
            }

            Task.Run(async () =>
            {
                switch (opCode)
                {
                    case OperationCodes.ChangeCluster:
                        await ChangeClusterResponseHandlerAsync(parameters);
                        return;
                    case OperationCodes.PartyMakeLeader:
                        await PartyMakeLeaderResponseHandlerAsync(parameters);
                        return;
                    case OperationCodes.Join:
                        await JoinResponseHandlerAsync(parameters);
                        return;
                    case OperationCodes.GetMailInfos:
                        await GetMailInfosResponseHandlerAsync(parameters);
                        return;
                    case OperationCodes.ReadMail:
                        await ReadMailResponseHandlerAsync(parameters);
                        return;
                }
            });
        }

        #endregion

        #region Code Parser

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

        #endregion

        #region Handler

        #region Events

        private async Task NewEquipmentItemEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewEquipmentItemEvent(parameters);
            await _newEquipmentItemEventHandler.OnActionAsync(value);
        }

        private async Task GrabbedLootEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new GrabbedLootEvent(parameters);
            await _grabbedLootEventHandler.OnActionAsync(value);
        }

        private async Task InventoryDeleteItemEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InventoryDeleteItemEvent(parameters);
            await _inventoryDeleteItemEventHandler.OnActionAsync(value);
        }

        private async Task InventoryPutItemEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InventoryPutItemEvent(parameters);
            await _inventoryPutItemEventHandler.OnActionAsync(value);
        }

        private async Task TakeSilverEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new TakeSilverEvent(parameters);
            await _takeSilverEventHandler.OnActionAsync(value);
        }

        private async Task UpdateFameEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateFameEvent(parameters);
            await _updateFameEventHandler.OnActionAsync(value);
        }

        private async Task UpdateSilverEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateSilverEvent(parameters);
            await _updateSilverEventHandler.OnActionAsync(value);
        }

        private async Task UpdateReSpecPointsEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateReSpecPointsEvent(parameters);
            await _updateReSpecPointsEventHandler.OnActionAsync(value);
        }

        private async Task UpdateCurrencyEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateCurrencyEvent(parameters);
            await _updateCurrencyEventHandler.OnActionAsync(value);
        }

        private async Task DiedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new DiedEvent(parameters);
            await _diedEventHandler.OnActionAsync(value);
        }

        private async Task NewLootChestEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewLootChestEvent(parameters);
            await _newLootChestEventHandler.OnActionAsync(value);
        }

        private async Task LootChestOpenedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new LootChestOpenedEvent(parameters);
            await _lootChestOpenedEventHandler.OnActionAsync(value);
        }

        private async Task InCombatStateUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InCombatStateUpdateEvent(parameters);
            await _inCombatStateUpdateEventHandler.OnActionAsync(value);
        }

        private async Task NewShrineEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewShrineEvent(parameters);
            await _newShrineEventHandler.OnActionAsync(value);
        }

        private async Task HealthUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new HealthUpdateEvent(parameters);
            await _healthUpdateEventHandler.OnActionAsync(value);
        }

        private async Task PartyPlayerJoinedHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyPlayerJoinedEvent(parameters);
            await _partyPlayerJoinedHandler.OnActionAsync(value);
        }

        private async Task PartyChangedOrderEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyChangedOrderEvent(parameters);
            await _partyChangedOrderEventHandler.OnActionAsync(value);
        }

        private async Task NewCharacterEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewCharacterEvent(parameters);
            await _newCharacterEventHandler.OnActionAsync(value);
        }

        private async Task SiegeCampClaimStartEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new SiegeCampClaimStartEvent(parameters);
            await _siegeCampClaimStartEventHandler.OnActionAsync(value);
        }

        private async Task NewMobEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewMobEvent(parameters);
            await _newMobEventHandler.OnActionAsync(value);
        }

        private async Task CharacterEquipmentChangedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new CharacterEquipmentChangedEvent(parameters);
            await _characterEquipmentChangedEventHandler.OnActionAsync(value);
        }

        private async Task ActiveSpellEffectsUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ActiveSpellEffectsUpdateEvent(parameters);
            await _activeSpellEffectsUpdateEventHandler.OnActionAsync(value);
        }

        private async Task UpdateFactionStandingEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateFactionStandingEvent(parameters);
            await _updateFactionStandingEventHandler.OnActionAsync(value);
        }

        private async Task ReceivedSeasonPointsEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ReceivedSeasonPointsEvent(parameters);
            await _receivedSeasonPointsEventHandler.OnActionAsync(value);
        }

        private async Task MightFavorPointsEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new MightFavorPointsEvent(parameters);
            await _mightFavorPointsEventHandler.OnActionAsync(value);
        }

        #endregion

        #region Requests

        private async Task UseShrineRequestHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UseShrineRequest(parameters);
            await _useShrineRequestHandler.OnActionAsync(value);
        }

        #endregion

        #region Response

        private async Task ChangeClusterResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ChangeClusterResponse(parameters);
            await _changeClusterResponseHandler.OnActionAsync(value);
        }

        private async Task PartyMakeLeaderResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyMakeLeaderResponse(parameters);
            await _partyMakeLeaderResponseHandler.OnActionAsync(value);
        }

        private async Task JoinResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new JoinResponse(parameters);
            await _joinResponseHandler.OnActionAsync(value);
        }

        private async Task GetMailInfosResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new GetMailInfosResponse(parameters);
            await _getMailInfosResponseHandler.OnActionAsync(value);
        }

        private async Task ReadMailResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ReadMailResponse(parameters);
            await _readMailResponseHandler.OnActionAsync(value);
        }

        #endregion

        #endregion
    }
}