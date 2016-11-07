﻿namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI
{
    public class ServerAddress
    {
        public ServerAddress(string name, string address, bool isDefault)
        {
            Name = name;
            Address = address;
            IsDefault = isDefault;
        }

        public string Name { get; set; }

        public string Address { get; set; }

        public bool IsDefault { get; set; }
    }
}