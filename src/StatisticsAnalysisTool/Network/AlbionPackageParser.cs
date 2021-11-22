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

namespace StatisticsAnalysisTool.Network
{
    public class AlbionPackageParser : PhotonParser
    {
        private readonly NewEquipmentItemEventHandler NewEquipmentItemEventHandler;
        private readonly OtherGrabbedLootEventHandler OtherGrabbedLootEventHandler;
        private readonly InventoryDeleteItemEventHandler InventoryDeleteItemEventHandler;
        private readonly InventoryPutItemEventHandler InventoryPutItemEventHandler;
        private readonly TakeSilverEventHandler TakeSilverEventHandler;
        private readonly UpdateFameEventHandler UpdateFameEventHandler;
        private readonly UpdateSilverEventHandler UpdateSilverEventHandler;
        private readonly UpdateReSpecPointsEventHandler UpdateReSpecPointsEventHandler;
        private readonly UpdateCurrencyEventHandler UpdateCurrencyEventHandler;
        private readonly DiedEventHandler DiedEventHandler;
        private readonly NewLootChestEventHandler NewLootChestEventHandler;
        private readonly LootChestOpenedEventHandler LootChestOpenedEventHandler;
        private readonly InCombatStateUpdateEventHandler InCombatStateUpdateEventHandler;
        private readonly NewShrineEventHandler NewShrineEventHandler;
        private readonly HealthUpdateEventHandler HealthUpdateEventHandler;
        private readonly PartyDisbandedEventHandler PartyDisbandedEventHandler;
        private readonly PartyChangedOrderEventHandler PartyChangedOrderEventHandler;
        private readonly NewCharacterEventHandler NewCharacterEventHandler;
        private readonly SiegeCampClaimStartEventHandler SiegeCampClaimStartEventHandler;
        private readonly CharacterEquipmentChangedEventHandler CharacterEquipmentChangedEventHandler;
        private readonly NewMobEventHandler NewMobEventHandler;
        private readonly ActiveSpellEffectsUpdateEventHandler ActiveSpellEffectsUpdateEventHandler;
        private readonly UpdateFactionStandingEventHandler UpdateFactionStandingEventHandler;
        private readonly ReceivedSeasonPointsEventHandler ReceivedSeasonPointsEventHandler;

        private readonly UseShrineRequestHandler UseShrineRequestHandler;

        private readonly ChangeClusterResponseHandler ChangeClusterResponseHandler;
        private readonly PartyMakeLeaderResponseHandler PartyMakeLeaderResponseHandler;
        private readonly JoinResponseHandler JoinResponseHandler;

        public AlbionPackageParser(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            NewEquipmentItemEventHandler = new NewEquipmentItemEventHandler(trackingController);
            OtherGrabbedLootEventHandler = new OtherGrabbedLootEventHandler(trackingController);
            InventoryDeleteItemEventHandler = new InventoryDeleteItemEventHandler(trackingController);
            InventoryPutItemEventHandler = new InventoryPutItemEventHandler(trackingController);
            TakeSilverEventHandler = new TakeSilverEventHandler(trackingController);
            UpdateFameEventHandler = new UpdateFameEventHandler(trackingController);
            UpdateSilverEventHandler = new UpdateSilverEventHandler(trackingController);
            UpdateReSpecPointsEventHandler = new UpdateReSpecPointsEventHandler(trackingController);
            UpdateCurrencyEventHandler = new UpdateCurrencyEventHandler(trackingController);
            DiedEventHandler = new DiedEventHandler(trackingController);
            NewLootChestEventHandler = new NewLootChestEventHandler(trackingController);
            LootChestOpenedEventHandler = new LootChestOpenedEventHandler(trackingController);
            InCombatStateUpdateEventHandler = new InCombatStateUpdateEventHandler(trackingController);
            NewShrineEventHandler = new NewShrineEventHandler(trackingController);
            HealthUpdateEventHandler = new HealthUpdateEventHandler(trackingController);
            PartyDisbandedEventHandler = new PartyDisbandedEventHandler(trackingController);
            PartyChangedOrderEventHandler = new PartyChangedOrderEventHandler(trackingController);
            NewCharacterEventHandler = new NewCharacterEventHandler(trackingController);
            SiegeCampClaimStartEventHandler = new SiegeCampClaimStartEventHandler(trackingController);
            CharacterEquipmentChangedEventHandler = new CharacterEquipmentChangedEventHandler(trackingController);
            NewMobEventHandler = new NewMobEventHandler(trackingController);
            ActiveSpellEffectsUpdateEventHandler = new ActiveSpellEffectsUpdateEventHandler(trackingController);
            UpdateFactionStandingEventHandler = new UpdateFactionStandingEventHandler(trackingController);
            ReceivedSeasonPointsEventHandler = new ReceivedSeasonPointsEventHandler(trackingController);

            UseShrineRequestHandler = new UseShrineRequestHandler(trackingController);

            ChangeClusterResponseHandler = new ChangeClusterResponseHandler(trackingController);
            PartyMakeLeaderResponseHandler = new PartyMakeLeaderResponseHandler(trackingController);
            JoinResponseHandler = new JoinResponseHandler(trackingController, mainWindowViewModel);
        }

        #region Actions

        protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
            var eventCode = ParseEventCode(parameters);

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
                        await PartyMakeLeaderEventHandlerAsync(parameters);
                        return;
                    case OperationCodes.Join:
                        await JoinResponseHandlerAsync(parameters);
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
            await NewEquipmentItemEventHandler.OnActionAsync(value);
        }

        private async Task OtherGrabbedLootEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new OtherGrabbedLootEvent(parameters);
            await OtherGrabbedLootEventHandler.OnActionAsync(value);
        }

        private async Task InventoryDeleteItemEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InventoryDeleteItemEvent(parameters);
            await InventoryDeleteItemEventHandler.OnActionAsync(value);
        }

        private async Task InventoryPutItemEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InventoryPutItemEvent(parameters);
            await InventoryPutItemEventHandler.OnActionAsync(value);
        }

        private async Task TakeSilverEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new TakeSilverEvent(parameters);
            await TakeSilverEventHandler.OnActionAsync(value);
        }

        private async Task UpdateFameEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateFameEvent(parameters);
            await UpdateFameEventHandler.OnActionAsync(value);
        }

        private async Task UpdateSilverEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateSilverEvent(parameters);
            await UpdateSilverEventHandler.OnActionAsync(value);
        }

        private async Task UpdateReSpecPointsEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateReSpecPointsEvent(parameters);
            await UpdateReSpecPointsEventHandler.OnActionAsync(value);
        }

        private async Task UpdateCurrencyEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateCurrencyEvent(parameters);
            await UpdateCurrencyEventHandler.OnActionAsync(value);
        }

        private async Task DiedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new DiedEvent(parameters);
            await DiedEventHandler.OnActionAsync(value);
        }

        private async Task NewLootChestEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewLootChestEvent(parameters);
            await NewLootChestEventHandler.OnActionAsync(value);
        }

        private async Task LootChestOpenedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new LootChestOpenedEvent(parameters);
            await LootChestOpenedEventHandler.OnActionAsync(value);
        }

        private async Task InCombatStateUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new InCombatStateUpdateEvent(parameters);
            await InCombatStateUpdateEventHandler.OnActionAsync(value);
        }

        private async Task NewShrineEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewShrineEvent(parameters);
            await NewShrineEventHandler.OnActionAsync(value);
        }

        private async Task HealthUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new HealthUpdateEvent(parameters);
            await HealthUpdateEventHandler.OnActionAsync(value);
        }

        private async Task PartyDisbandedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyDisbandedEvent(parameters);
            await PartyDisbandedEventHandler.OnActionAsync(value);
        }

        private async Task PartyChangedOrderEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyChangedOrderEvent(parameters);
            await PartyChangedOrderEventHandler.OnActionAsync(value);
        }

        private async Task NewCharacterEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewCharacterEvent(parameters);
            await NewCharacterEventHandler.OnActionAsync(value);
        }

        private async Task SiegeCampClaimStartEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new SiegeCampClaimStartEvent(parameters);
            await SiegeCampClaimStartEventHandler.OnActionAsync(value);
        }

        private async Task NewMobEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new NewMobEvent(parameters);
            await NewMobEventHandler.OnActionAsync(value);
        }

        private async Task CharacterEquipmentChangedEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new CharacterEquipmentChangedEvent(parameters);
            await CharacterEquipmentChangedEventHandler.OnActionAsync(value);
        }

        private async Task ActiveSpellEffectsUpdateEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ActiveSpellEffectsUpdateEvent(parameters);
            await ActiveSpellEffectsUpdateEventHandler.OnActionAsync(value);
        }

        private async Task UpdateFactionStandingEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UpdateFactionStandingEvent(parameters);
            await UpdateFactionStandingEventHandler.OnActionAsync(value);
        }

        private async Task ReceivedSeasonPointsEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ReceivedSeasonPointsEvent(parameters);
            await ReceivedSeasonPointsEventHandler.OnActionAsync(value);
        }

        #endregion

        #region Requests

        private async Task UseShrineRequestHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new UseShrineRequest(parameters);
            await UseShrineRequestHandler.OnActionAsync(value);
        }

        #endregion

        #region Response

        private async Task ChangeClusterResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new ChangeClusterResponse(parameters);
            await ChangeClusterResponseHandler.OnActionAsync(value);
        }

        private async Task PartyMakeLeaderEventHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new PartyMakeLeaderResponse(parameters);
            await PartyMakeLeaderResponseHandler.OnActionAsync(value);
        }

        private async Task JoinResponseHandlerAsync(Dictionary<byte, object> parameters)
        {
            var value = new JoinResponse(parameters);
            await JoinResponseHandler.OnActionAsync(value);
        }

        #endregion

        #endregion
    }
}