﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace BaseLibrary
{
    public class ConfigSectionSecurityServiceClient : ConfigSection
    {
        public string SecurityServiceUrl { get; set; }
        public byte[] SecurityServiceKey { get; set; }

        public override IEnumerable<ConfigItem> ConfigItems
        {
            get
            {
                yield return new ConfigItemString("SecurityServiceUrl", v => SecurityServiceUrl = v);
                yield return new ConfigItemBytes("SecurityServiceKey", v => SecurityServiceKey = v);
            }
        }
    }

    public class ConfigSectionOauth2Client : ConfigSection
    {
        public string OAuth2AuthorizationUrl { get; set; }
        public string OAuth2TokenUrl { get; set; }
        public string OAuth2ApiUrl { get; set; }
        public string OAuth2ClientId { get; set; }
        public string OAuth2ClientSecret { get; set; }

        public override IEnumerable<ConfigItem> ConfigItems
        {
            get
            {
                yield return new ConfigItemString("OAuth2AuthorizationUrl", v => OAuth2AuthorizationUrl = v);
                yield return new ConfigItemString("OAuth2TokenUrl", v => OAuth2TokenUrl = v);
                yield return new ConfigItemString("OAuth2ApiUrl", v => OAuth2ApiUrl = v);
                yield return new ConfigItemString("OAuth2ClientId", v => OAuth2ClientId = v);
                yield return new ConfigItemString("OAuth2ClientSecret", v => OAuth2ClientSecret = v);
            }
        }
    }

    public class ConfigSectionMail : ConfigSection
    {
        public string MailServerHost { get; set; }
        public int MailServerPort { get; set; }
        public string MailAccountName { get; set; }
        public string MailAccountPassword { get; set; }
        public string AdminMailAddress { get; private set; }
        public string SystemMailAddress { get; private set; }

        public override IEnumerable<ConfigItem> ConfigItems
        {
            get
            {
                yield return new ConfigItemString("MailServerHost", v => MailServerHost = v);
                yield return new ConfigItemInt32("MailServerPort", v => MailServerPort = v);
                yield return new ConfigItemString("MailAccountName", v => MailAccountName = v);
                yield return new ConfigItemString("MailAccountPassword", v => MailAccountPassword = v);
                yield return new ConfigItemString("AdminMailAddress", v => AdminMailAddress = v);
                yield return new ConfigItemString("SystemMailAddress", v => SystemMailAddress = v);
            } 
        }
    }

    public class ConfigSectionDatabase : ConfigSection 
    {
        public string DatabaseServer { get; set; }
        public int DatabasePort { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }

        public override IEnumerable<ConfigItem> ConfigItems
        {
            get 
            {
                yield return new ConfigItemString("DatabaseServer", v => DatabaseServer = v);
                yield return new ConfigItemInt32("DatabasePort", v => DatabasePort = v);
                yield return new ConfigItemString("DatabaseName", v => DatabaseName = v);
                yield return new ConfigItemString("DatabaseUsername", v => DatabaseUsername = v);
                yield return new ConfigItemString("DatabasePassword", v => DatabasePassword = v);
            } 
        }
    }

    public abstract class Config : ConfigSection
    {
        public abstract IEnumerable<ConfigSection> ConfigSections { get; }

        public override void Load(string filename)
        {
            foreach (var configSection in ConfigSections)
            {
                configSection.Load(filename);
            }

            base.Load(filename);
        }
    }

    public abstract class ConfigSection
    {
        public abstract IEnumerable<ConfigItem> ConfigItems { get; }

        public virtual void Load(string filename)
        {
            var document = XDocument.Load(filename);
            var root = document.Root;

            foreach (var configItem in ConfigItems)
            {
                configItem.Load(root);
            }
        }
    }

    public abstract class ConfigItem
    {
        public abstract void Load(XElement root);
    }

    public abstract class ConfigItem<T> : ConfigItem
    {
        protected string Tag { get; private set; }
        private Action<T> _assign;

        public ConfigItem(string tag, Action<T> assign)
        {
            Tag = tag;
            _assign = assign;
        }

        protected abstract T Convert(string value);

        public override void Load(XElement root)
        {
            var elements = root.Elements(Tag);

            if (!elements.Any())
            {
                throw new XmlException("Config node " + Tag + " not found");
            }
            else if (elements.Count() >= 2)
            {
                throw new XmlException("Config node " + Tag + " ambigous");
            }

            _assign(Convert(elements.Single().Value));
        }
    }

    public class ConfigItemString : ConfigItem<string>
    {
        public ConfigItemString(string tag, Action<string> assign)
            : base(tag, assign)
        {
        }

        protected override string Convert(string value)
        {
            return value;
        }
    }

    public class ConfigItemInt32 : ConfigItem<int>
    {
        public ConfigItemInt32(string tag, Action<int> assign) 
            : base(tag, assign)
        {
        }

        protected override int Convert(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            else
            {
                throw new XmlException("Cannot convert value of config node " + Tag + " to integer"); 
            }
        }
    }

    public class ConfigItemBytes : ConfigItem<byte[]>
    {
        public ConfigItemBytes(string tag, Action<byte[]> assign) 
            : base(tag, assign)
        {
        }

        protected override byte[] Convert(string value)
        {
            var bytes = value.TryParseHexBytes();

            if (bytes != null)
            {
                return bytes;
            }
            else
            {
                throw new XmlException("Cannot convert value of config node " + Tag + " to bytes"); 
            }
        }
    }
}