using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Timers;
using System.Linq;

namespace ocpa.ro.api.Helpers
{
    public class Token
    {
        const int MaxTTL = 5;

        public string Credentials { get; set; }

        public string Seed { get; } = Guid.NewGuid().ToString();

        public DateTime Creation { get; } = DateTime.UtcNow;

        public bool IsExpired => (DateTime.UtcNow.Subtract(Creation).TotalMinutes > MaxTTL);
    }

    public interface ITokenUtility
    {
        public Token GetNewToken(string credentials);
        public bool CheckIfActive(string seed, string credentials);
    }

    public class TokenUtility : ITokenUtility
    {
        object _lock = new object();
        Dictionary<string, Token> _activeTokens = new Dictionary<string, Token>();
        Timer _tmrCheckTokens = null;

        public TokenUtility()
        {
            _tmrCheckTokens = new Timer(5000);
            _tmrCheckTokens.AutoReset = true;
            _tmrCheckTokens.Elapsed += (s, e) =>
            {
                List<string> seeds = new List<string>();
                lock (_lock)
                {
                    seeds.AddRange(_activeTokens.Keys);
                };

                foreach (string seed in seeds)
                    _ = CheckIfActive(seed, null);
            };
        }

        public bool CheckIfActive(string seed, string credentials)
        {
            lock (_lock)
            {
                if (_activeTokens.TryGetValue(seed, out Token token))
                {
                    if (token != null)
                    {
                        if (token.IsExpired)
                            _activeTokens.Remove(seed);
                        else
                            return
                                string.IsNullOrEmpty(credentials) ||
                                string.Equals(token.Credentials, credentials, StringComparison.Ordinal);

                    }
                }
            }

            return false;
        }

        public Token GetNewToken(string credentials)
        {
            if (string.IsNullOrEmpty(credentials))
                return null;

            lock (_lock)
            {
                var token = new Token();

                if (_activeTokens.ContainsKey(token.Seed))
                    token = _activeTokens[token.Seed];
                else
                    _activeTokens.Add(token.Seed, token);

                token.Credentials = credentials;

                return token;
            }
        }
    }
}
