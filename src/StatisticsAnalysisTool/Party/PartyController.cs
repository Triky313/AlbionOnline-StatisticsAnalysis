using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Party;

public class PartyController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public PartyController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    #region Death Alert

    public void PlayerHasDied(string diedPlayerName)
    {
        var party = _mainWindowViewModel.PartyBindings.Party;

        if (party.Any(x => x.IsDeathAlertActive && x.Username == diedPlayerName))
        {
            SoundController.PlayAlertSound(SoundController.GetCurrentSoundPath(SettingsController.CurrentSettings.SelectedDeathAlertSound));
        }
    }

    #endregion

    #region Party Builder

    public async Task UpdatePartyAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var currentParty = _trackingController?.EntityController?.GetAllEntities(true);
            var bindingsParty = _mainWindowViewModel.PartyBindings.Party;

            foreach (var item in currentParty?.ToList() ?? new List<KeyValuePair<Guid, PlayerGameObject>>())
            {
                int index = bindingsParty.ToList().FindIndex(x => x.Guid == item.Value.UserGuid);
                if (index >= 0)
                {
                    UpdatePartyPlannerPlayer(item.Value, bindingsParty[index]);
                }
                else
                {
                    bindingsParty.Add(CreatePartyPlannerPlayer(item.Value));
                }
            }

            for (int i = bindingsParty.Count - 1; i >= 0; i--)
            {
                var partyPlannerPlayer = bindingsParty[i];
                int? index = currentParty?.FindIndex(x => x.Value.UserGuid == partyPlannerPlayer.Guid);
                if (index is < 0)
                {
                    bindingsParty.RemoveAt(i);
                }
            }

            _mainWindowViewModel?.PartyBindings?.UpdatePartyBuilderPlayerConditions();
            _mainWindowViewModel?.PartyBindings?.UpdateAveragePartyIp(null, null);
        });
    }

    private PartyPlayer CreatePartyPlannerPlayer(PlayerGameObject playerGameObject)
    {
        return new PartyPlayer()
        {
            Guid = playerGameObject.UserGuid,
            Username = playerGameObject.Name,
            IsLocalPlayer = _trackingController?.EntityController?.LocalUserData?.Guid == playerGameObject.UserGuid,
            AverageItemPower = new PartyBuilderItemPower { ItemPower = playerGameObject.ItemPower },
            MainHand = playerGameObject.CharacterEquipment?.GetMainHand(),
            OffHand = playerGameObject.CharacterEquipment?.GetOffHand(),
            Head = playerGameObject.CharacterEquipment?.GetHead(),
            Chest = playerGameObject.CharacterEquipment?.GetChest(),
            Shoes = playerGameObject.CharacterEquipment?.GetShoes(),
            Bag = playerGameObject.CharacterEquipment?.GetBag(),
            Cape = playerGameObject.CharacterEquipment?.GetCape(),
            Mount = playerGameObject.CharacterEquipment?.GetMount(),
            Potion = playerGameObject.CharacterEquipment?.GetPotion(),
            BuffFood = playerGameObject.CharacterEquipment?.GetBuffFood(),
            MainHandSpells = playerGameObject.CharacterEquipment?.GetMainHandSpells() ?? new List<Spell>(),
            OffHandSpells = playerGameObject.CharacterEquipment?.GetOffHandSpells() ?? new List<Spell>(),
            HeadSpells = playerGameObject.CharacterEquipment?.GetHeadSpells() ?? new List<Spell>(),
            ChestSpells = playerGameObject.CharacterEquipment?.GetChestSpells() ?? new List<Spell>(),
            ShoesSpells = playerGameObject.CharacterEquipment?.GetShoesSpells() ?? new List<Spell>(),
            MountSpells = playerGameObject.CharacterEquipment?.GetMountSpells() ?? new List<Spell>(),
            PotionSpells = playerGameObject.CharacterEquipment?.GetPotionSpells() ?? new List<Spell>(),
            FoodSpells = playerGameObject.CharacterEquipment?.GetFoodSpells() ?? new List<Spell>()
        };
    }

    private void UpdatePartyPlannerPlayer(PlayerGameObject playerGameObject, PartyPlayer partyPlayer)
    {
        if (partyPlayer == null || playerGameObject == null)
        {
            return;
        }

        partyPlayer.AverageItemPower.ItemPower = playerGameObject.ItemPower;
        partyPlayer.MainHand = playerGameObject.CharacterEquipment?.GetMainHand();
        partyPlayer.OffHand = playerGameObject.CharacterEquipment?.GetOffHand();
        partyPlayer.Head = playerGameObject.CharacterEquipment?.GetHead();
        partyPlayer.Chest = playerGameObject.CharacterEquipment?.GetChest();
        partyPlayer.Shoes = playerGameObject.CharacterEquipment?.GetShoes();
        partyPlayer.Bag = playerGameObject.CharacterEquipment?.GetBag();
        partyPlayer.Cape = playerGameObject.CharacterEquipment?.GetCape();
        partyPlayer.Mount = playerGameObject.CharacterEquipment?.GetMount();
        partyPlayer.Potion = playerGameObject.CharacterEquipment?.GetPotion();
        partyPlayer.BuffFood = playerGameObject.CharacterEquipment?.GetBuffFood();
        partyPlayer.MainHandSpells = playerGameObject.CharacterEquipment?.GetMainHandSpells() ?? new List<Spell>();
        partyPlayer.OffHandSpells = playerGameObject.CharacterEquipment?.GetOffHandSpells() ?? new List<Spell>();
        partyPlayer.HeadSpells = playerGameObject.CharacterEquipment?.GetHeadSpells() ?? new List<Spell>();
        partyPlayer.ChestSpells = playerGameObject.CharacterEquipment?.GetChestSpells() ?? new List<Spell>();
        partyPlayer.ShoesSpells = playerGameObject.CharacterEquipment?.GetShoesSpells() ?? new List<Spell>();
        partyPlayer.MountSpells = playerGameObject.CharacterEquipment?.GetMountSpells() ?? new List<Spell>();
        partyPlayer.PotionSpells = playerGameObject.CharacterEquipment?.GetPotionSpells() ?? new List<Spell>();
        partyPlayer.FoodSpells = playerGameObject.CharacterEquipment?.GetFoodSpells() ?? new List<Spell>();

        _mainWindowViewModel.PartyBindings.UpdatePartyBuilderPlayerConditions();
    }

    public void UpdateIsPlayerInspectedToFalse()
    {
        var bindingsParty = _mainWindowViewModel.PartyBindings.Party;
        foreach (PartyPlayer partyBuilderPlayer in bindingsParty)
        {
            partyBuilderPlayer.IsPlayerInspected = false;
        }
    }

    public void UpdateInspectedPlayer(Guid guid, InternalCharacterEquipment characterEquipment, double itemPower)
    {
        var bindingsParty = _mainWindowViewModel.PartyBindings.Party.FirstOrDefault(x => x.Guid == guid);
        if (bindingsParty == null)
        {
            return;
        }

        bindingsParty.AverageItemPower.ItemPower = itemPower;
        bindingsParty.MainHand = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.MainHand);
        bindingsParty.OffHand = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.OffHand);
        bindingsParty.Head = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Head);
        bindingsParty.Chest = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Chest);
        bindingsParty.Shoes = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Shoes);
        bindingsParty.Bag = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Bag);
        bindingsParty.Cape = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Cape);
        bindingsParty.Mount = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Mount);
        bindingsParty.Potion = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.Potion);
        bindingsParty.BuffFood = _trackingController.LootController.GetItemFromDiscoveredLoot(characterEquipment.BuffFood);

        bindingsParty.IsPlayerInspected = true;
        _mainWindowViewModel.PartyBindings.UpdatePartyBuilderPlayerConditions();
        _mainWindowViewModel.PartyBindings.UpdateAveragePartyIp(null, null);
    }

    #endregion
}