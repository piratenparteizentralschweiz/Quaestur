﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using BaseLibrary;
using SecurityServiceClient;

namespace Publicus
{
    public static class Global
    {
		private static Config _config;
		private static Logger _logger;
		private static Mailer _mailer;
        private static SessionManager _login;
        private static SecurityThrottle _throttle;
        private static SecurityService _security;
        private static Gpg _gpg;
        private static MailCounter _mailCounter;

        public static MailCounter MailCounter
        {
            get
            {
                if (_mailCounter == null)
                {
                    _mailCounter = new MailCounter(30, 3);
                }

                return _mailCounter;
            }
        }

        public static Gpg Gpg
        {
            get
            {
                if (_gpg == null)
                {
                    _gpg = new SecurityServiceGpg(Security);
                }

                return _gpg;
            }
        }

        public static SecurityService Security
        {
            get
            {
                if (_security == null)
                {
                    _security = new SecurityService(Config.SecurityServiceUrl, Config.SecurityServiceKey);
                }

                return _security;
            }
        }

        public static SecurityThrottle Throttle
        {
            get
            {
                if (_throttle == null)
                {
                    _throttle = new SecurityThrottle();
                }

                return _throttle;
            }
        }

        public static SessionManager Sessions
        {
            get 
            {
                if (_login == null)
                {
                    _login = new SessionManager(); 
                }

                return _login;
            } 
        }

        private static IEnumerable<string> ConfigPaths
        {
            get
            {
                yield return "/Security/Test/publicus.xml";
                yield return "config.xml";
            }
        }

        private static string FirstFileExists(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path; 
                }
            }

            return null;
        }

        public static Config Config
		{
			get
			{
				if (_config == null)
				{
					_config = new Config();
					_config.Load(FirstFileExists(ConfigPaths));
				}

				return _config;
			}
		}

        public static IDatabase CreateDatabase()
        {
            return new PostgresDatabase(Config); 
        }

		public static Logger Log
        {
            get
            {
				if (_logger == null)
                {
					_logger = new Logger();
                }

				return _logger;
            }
        }

		public static Mailer Mail
		{
            get
            {
				if (_mailer == null)
                {
					_mailer = new Mailer(Log, Config, Gpg);
                }

				return _mailer;
            }
        }
    }
}
