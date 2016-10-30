﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Preferences;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Utils;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow
{
    public class SavedAddressesViewModel
    {
        private readonly ObservableCollection<AddressSetting> _savedAddresses = new ObservableCollection<AddressSetting>();
        private readonly ISavedAddressStore _savedAddressStore;

        public SavedAddressesViewModel(ISavedAddressStore savedAddressStore)
        {
            _savedAddressStore = savedAddressStore;

            foreach (var savedAddress in _savedAddressStore.LoadFromStore())
            {
                _savedAddresses.Add(savedAddress);
            }

            NewAddressCommand = new DelegateCommand(OnNewAddress);
            RemoveSelectedCommand = new DelegateCommand(OnRemoveSelected);
            SaveCommand = new DelegateCommand(OnSave);
        }

        public ObservableCollection<AddressSetting> SavedAddresses => _savedAddresses;

        public string NewName { get; set; }

        public string NewAddress { get; set; }

        public ICommand NewAddressCommand { get; }

        public ICommand SaveCommand { get; set; }

        public ICommand RemoveSelectedCommand { get; set; }

        public AddressSetting SelectedItem { get; set; }

        public AddressSetting DefaultAddress
        {
            get
            {
                var defaultAddress = _savedAddresses.FirstOrDefault(x => x.IsDefault);
                if (defaultAddress == null && _savedAddresses.Count > 0)
                {
                    defaultAddress = _savedAddresses.First();
                }
                return defaultAddress;
            }
        }

        private void OnNewAddress()
        {
            _savedAddresses.Add(new AddressSetting(NewName, NewAddress, false));
        }

        private void OnRemoveSelected()
        {
            if (SelectedItem == null)
            {
                return;
            }

            _savedAddresses.Remove(SelectedItem);
        }

        private void OnSave()
        {
            _savedAddressStore.SaveToStore(_savedAddresses);
        }
    }
}