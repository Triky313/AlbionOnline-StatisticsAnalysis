using log4net;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Data;
using FontAwesome5;

namespace StatisticsAnalysisTool.ViewModels
{
    public class ItemSearchViewModel : INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private ICollectionView _itemsView;
        private string _searchText;
        private Category _selectedItemCategories;
        private ParentCategory _selectedItemParentCategories;
        private ItemLevel _selectedItemLevel;
        private ItemTier _selectedItemTier;
        private Visibility _itemCategoriesVisibility;
        private Dictionary<Category, string> _itemCategories;
        private bool _isFullItemInfoSearchActive;
        private Visibility _itemLevelsVisibility;
        private Visibility _itemTiersVisibility;
        private Visibility _itemParentCategoriesVisibility;
        private bool _isShowOnlyItemsWithAlertOnActive;
        private bool _isShowOnlyFavoritesActive;
        private int _localImageCounter;
        private string _itemCounterString;
        private bool _isTxtSearchEnabled;
        private bool _isItemSearchCheckboxesEnabled;
        private bool _isFilterResetEnabled;
        private Visibility _loadIconVisibility;
        private Visibility _gridTryToLoadTheItemListAgainVisibility;
        private Visibility _loadFullItemInfoButtonVisibility;
        private int _loadFullItemInfoProBarMin;
        private bool _isLoadFullItemInfoButtonEnabled;
        private Visibility _loadFullItemInfoProBarGridVisibility;
        private int _loadFullItemInfoProBarValue;
        private int _loadFullItemInfoProBarMax;
        private string _loadFullItemInfoProBarCounter;
        public AlertController AlertManager;
        private bool _isFullItemInfoLoading;
        private readonly ItemSearchControl _viewModel;

        public ItemSearchViewModel(ItemSearchControl viewModel)
        {
            _viewModel = viewModel;
        }

        public async Task InitItemListAsync()
        {
            IsTxtSearchEnabled = false;
            IsItemSearchCheckboxesEnabled = false;
            IsFilterResetEnabled = false;
            LoadIconVisibility = Visibility.Visible;
            GridTryToLoadTheItemListAgainVisibility = Visibility.Collapsed;

            var isItemListLoaded = await ItemController.GetItemListFromJsonAsync().ConfigureAwait(true);
            if (!isItemListLoaded)
            {
                SetErrorBar(Visibility.Visible, LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"));
                GridTryToLoadTheItemListAgainVisibility = Visibility.Visible;

                return;
            }

            await ItemController.SetFavoriteItemsFromLocalFileAsync();
            await ItemController.GetItemInformationListFromLocalAsync();
            IsFullItemInformationCompleteCheck();

            ItemsView = new ListCollectionView(ItemController.Items);
            InitAlerts();

            LoadIconVisibility = Visibility.Hidden;
            IsFilterResetEnabled = true;
            IsItemSearchCheckboxesEnabled = true;
            IsTxtSearchEnabled = true;

            _viewModel.Dispatcher?.Invoke(() => { _ = _viewModel.TxtSearch.Focus(); });
        }

        private void InitAlerts()
        {
            SoundController.InitializeSoundFilesFromDirectory();
            //AlertManager = new AlertController(_viewModel, ItemsView);
        }

        #region Item View Filters

        private void ItemsViewFilter()
        {
            if (ItemsView == null)
            {
                return;
            }

            if (IsFullItemInfoSearchActive)
                ItemsView.Filter = i =>
                {
                    var item = i as Item;
                    if (IsShowOnlyItemsWithAlertOnActive)
                    {
                        return item?.FullItemInformation != null &&
                               item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                               && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                                   SelectedItemParentCategory == ParentCategory.Unknown)
                               && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                               && ((ItemTier)item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                               && ((ItemLevel)item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                               && item.IsAlertActive;
                    }

                    if (IsShowOnlyFavoritesActive)
                    {
                        return item?.FullItemInformation != null &&
                               item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                               && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                                   SelectedItemParentCategory == ParentCategory.Unknown)
                               && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                               && ((ItemTier)item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                               && ((ItemLevel)item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                               && item.IsFavorite;
                    }

                    return item?.FullItemInformation != null &&
                           item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                           && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                               SelectedItemParentCategory == ParentCategory.Unknown)
                           && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                           && ((ItemTier)item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                           && ((ItemLevel)item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown);
                };
            else
                ItemsView.Filter = i =>
                {
                    var item = i as Item;

                    if (IsShowOnlyItemsWithAlertOnActive)
                    {
                        return (item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false) && item.IsAlertActive;
                    }

                    if (IsShowOnlyFavoritesActive)
                    {
                        return (item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false) && item.IsFavorite;
                    }

                    return item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false;
                };

            SetItemCounterAsync();
        }

        private async void SetItemCounterAsync()
        {
            try
            {
                LocalImageCounter = await ImageController.LocalImagesCounterAsync();
                ItemCounterString = $"{((ListCollectionView)ItemsView)?.Count ?? 0}/{ItemController.Items?.Count ?? 0}";
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public void ItemFilterReset()
        {
            SearchText = string.Empty;
            SelectedItemCategory = Category.Unknown;
            SelectedItemParentCategory = ParentCategory.Unknown;
            SelectedItemLevel = ItemLevel.Unknown;
            SelectedItemTier = ItemTier.Unknown;
        }

        #endregion

        #region Full Item Information

        public void IsFullItemInformationCompleteCheck()
        {
            if (ItemController.IsFullItemInformationComplete)
            {
                LoadFullItemInfoButtonVisibility = Visibility.Hidden;
                IsLoadFullItemInfoButtonEnabled = false;
                LoadFullItemInfoProBarGridVisibility = Visibility.Hidden;
            }
            else
            {
                LoadFullItemInfoButtonVisibility = Visibility.Visible;
                IsLoadFullItemInfoButtonEnabled = true;
            }
        }

        public async void LoadAllFullItemInformationFromWeb()
        {
            IsLoadFullItemInfoButtonEnabled = false;
            LoadFullItemInfoButtonVisibility = Visibility.Hidden;
            LoadFullItemInfoProBarGridVisibility = Visibility.Visible;

            LoadFullItemInfoProBarMin = 0;
            LoadFullItemInfoProBarValue = 0;
            LoadFullItemInfoProBarMax = ItemController.Items.Count;
            IsFullItemInfoLoading = true;

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5,
                BoundedCapacity = 50
            };

            var block = new ActionBlock<Item>(async item =>
            {
                item.FullItemInformation = await ItemController.GetFullItemInformationAsync(item);
            }, options);

            await foreach (var item in ItemController.Items.ToAsyncEnumerable())
            {
                if (!IsFullItemInfoLoading)
                {
                    break;
                }

                if (await block.SendAsync(item))
                {
                    LoadFullItemInfoProBarValue++;
                }
            }

            block.Complete();
            await block.Completion;

            LoadFullItemInfoProBarGridVisibility = Visibility.Hidden;

            if (ItemController.IsFullItemInformationComplete)
            {
                LoadFullItemInfoButtonVisibility = Visibility.Hidden;
                IsLoadFullItemInfoButtonEnabled = false;
            }
            else
            {
                LoadFullItemInfoButtonVisibility = Visibility.Visible;
                IsLoadFullItemInfoButtonEnabled = true;
            }
        }

        #endregion

        #region Alert

        public void ToggleAlertSender(object sender)
        {
            if (sender == null)
            {
                return;
            }

            try
            {
                var imageAwesome = (ImageAwesome)sender;
                var item = (Item)imageAwesome.DataContext;

                if (item.AlertModeMinSellPriceIsUndercutPrice <= 0)
                {
                    return;
                }

                item.IsAlertActive = AlertManager.ToggleAlert(ref item);
                ItemsView.Refresh();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        #endregion

        #region Error bar

        public void SetErrorBar(Visibility visibility, string errorMessage)
        {
            //ErrorBarText = errorMessage;
            //ErrorBarVisibility = visibility;
        }

        #endregion

        #region Bindings

        public ICollectionView ItemsView
        {
            get => _itemsView;
            set
            {
                _itemsView = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                ItemsViewFilter();
                ItemsView?.Refresh();

                OnPropertyChanged();
            }
        }

        public Category SelectedItemCategory
        {
            get => _selectedItemCategories;
            set
            {
                _selectedItemCategories = value;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public ParentCategory SelectedItemParentCategory
        {
            get => _selectedItemParentCategories;
            set
            {
                _selectedItemParentCategories = value;
                ItemCategories = CategoryController.GetCategoriesByParentCategory(SelectedItemParentCategory);
                SelectedItemCategory = Category.Unknown;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public ItemLevel SelectedItemLevel
        {
            get => _selectedItemLevel;
            set
            {
                _selectedItemLevel = value;
                ItemsView?.Refresh();
                SetItemCounterAsync();
                OnPropertyChanged();
            }
        }

        public ItemTier SelectedItemTier
        {
            get => _selectedItemTier;
            set
            {
                _selectedItemTier = value;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public Visibility ItemCategoriesVisibility
        {
            get => _itemCategoriesVisibility;
            set
            {
                _itemCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<Category, string> ItemCategories
        {
            get => _itemCategories;
            set
            {
                var categories = value;
                categories = new Dictionary<Category, string> { { Category.Unknown, string.Empty } }.Concat(categories)
                    .ToDictionary(k => k.Key, v => v.Value);
                _itemCategories = categories;
                OnPropertyChanged();
            }
        }

        public bool IsFullItemInfoSearchActive
        {
            get => _isFullItemInfoSearchActive;
            set
            {
                _isFullItemInfoSearchActive = value;

                if (_isFullItemInfoSearchActive)
                {
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Visible;
                }
                else
                {
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Hidden;
                }

                ItemsViewFilter();
                ItemsView?.Refresh();

                Settings.Default.IsFullItemInfoSearchActive = _isFullItemInfoSearchActive;
                OnPropertyChanged();
            }
        }

        public Visibility ItemLevelsVisibility
        {
            get => _itemLevelsVisibility;
            set
            {
                _itemLevelsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemTiersVisibility
        {
            get => _itemTiersVisibility;
            set
            {
                _itemTiersVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemParentCategoriesVisibility
        {
            get => _itemParentCategoriesVisibility;
            set
            {
                _itemParentCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsShowOnlyItemsWithAlertOnActive
        {
            get => _isShowOnlyItemsWithAlertOnActive;
            set
            {
                _isShowOnlyItemsWithAlertOnActive = value;

                if (value)
                {
                    IsShowOnlyFavoritesActive = false;
                }

                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public bool IsShowOnlyFavoritesActive
        {
            get => _isShowOnlyFavoritesActive;
            set
            {
                _isShowOnlyFavoritesActive = value;

                if (value)
                {
                    IsShowOnlyItemsWithAlertOnActive = false;
                }

                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public int LocalImageCounter
        {
            get => _localImageCounter;
            set
            {
                _localImageCounter = value;
                OnPropertyChanged();
            }
        }

        public string ItemCounterString
        {
            get => _itemCounterString;
            set
            {
                _itemCounterString = value;
                OnPropertyChanged();
            }
        }

        public bool IsTxtSearchEnabled
        {
            get => _isTxtSearchEnabled;
            set
            {
                _isTxtSearchEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsItemSearchCheckboxesEnabled
        {
            get => _isItemSearchCheckboxesEnabled;
            set
            {
                _isItemSearchCheckboxesEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsFilterResetEnabled
        {
            get => _isFilterResetEnabled;
            set
            {
                _isFilterResetEnabled = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadIconVisibility
        {
            get => _loadIconVisibility;
            set
            {
                _loadIconVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility GridTryToLoadTheItemListAgainVisibility
        {
            get => _gridTryToLoadTheItemListAgainVisibility;
            set
            {
                _gridTryToLoadTheItemListAgainVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadFullItemInfoButtonVisibility
        {
            get => _loadFullItemInfoButtonVisibility;
            set
            {
                _loadFullItemInfoButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoadFullItemInfoButtonEnabled
        {
            get => _isLoadFullItemInfoButtonEnabled;
            set
            {
                _isLoadFullItemInfoButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadFullItemInfoProBarGridVisibility
        {
            get => _loadFullItemInfoProBarGridVisibility;
            set
            {
                _loadFullItemInfoProBarGridVisibility = value;
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarValue
        {
            get => _loadFullItemInfoProBarValue;
            set
            {
                _loadFullItemInfoProBarValue = value;
                LoadFullItemInfoProBarCounter = $"{_loadFullItemInfoProBarValue}/{LoadFullItemInfoProBarMax}";
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarMax
        {
            get => _loadFullItemInfoProBarMax;
            set
            {
                _loadFullItemInfoProBarMax = value;
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarMin
        {
            get => _loadFullItemInfoProBarMin;
            set
            {
                _loadFullItemInfoProBarMin = value;
                OnPropertyChanged();
            }
        }

        public string LoadFullItemInfoProBarCounter
        {
            get => _loadFullItemInfoProBarCounter;
            set
            {
                _loadFullItemInfoProBarCounter = value;
                OnPropertyChanged();
            }
        }

        public bool IsFullItemInfoLoading
        {
            get => _isFullItemInfoLoading;
            set
            {
                _isFullItemInfoLoading = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}