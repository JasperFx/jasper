using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Transports;
using Jasper.Util;

namespace Jasper.Bus.Configuration
{
    /// <summary>
    /// Internal service used to fetch and resolve Uri aliases
    /// </summary>
    public class UriAliasLookup
    {
        private readonly IDictionary<string, IUriLookup> _lookups = new Dictionary<string, IUriLookup>();
        private readonly IDictionary<Uri, Uri> _aliases = new Dictionary<Uri, Uri>();

        public UriAliasLookup(IEnumerable<IUriLookup> lookups)
        {
            // The only single reason this exists is to not shatter a bunch
            // of existing integration tests after we eliminated the separate
            // durable transport
            _lookups.Add(TransportConstants.Durable, new DurableUriLookup());

            foreach (var lookup in lookups)
            {
                _lookups.SmartAdd(lookup.Protocol, lookup);
            }
        }

        public void SetAlias(Uri alias, Uri real)
        {
            if (_aliases.ContainsKey(alias))
            {
                _aliases[alias] = real;
            }
            else
            {
                _aliases.Add(alias, real);
            }
        }

        public void SetAlias(string aliasUriString, string realUriString)
        {
            SetAlias(aliasUriString.ToUri(), realUriString.ToUri());
        }

        public async Task ReadAliases(Uri[] raw)
        {
            var unknown = raw
                .Where(x => x != null && !_aliases.ContainsKey(x))
                .GroupBy(x => x.Scheme)
                .Where(x => _lookups.ContainsKey(x.Key));

            // I just don't wanna bother w/ concurrency here,
            // so one at a time
            foreach (var group in unknown)
            {
                var incoming = @group.ToArray();
                var resolved = await _lookups[group.Key].Lookup(incoming);

                for (int i = 0; i < incoming.Length; i++)
                {
                    _aliases.Add(incoming[i], resolved[i]);
                }
            }
        }

        public Uri Resolve(Uri raw)
        {
            return _aliases.ContainsKey(raw) ? _aliases[raw] : raw;
        }


    }

    public class DurableUriLookup : IUriLookup
    {
        public string Protocol { get; } = TransportConstants.Durable;

        public Task<Uri[]> Lookup(Uri[] originals)
        {
            var values = originals.Select(x => x.ToCanonicalTcpUri()).ToArray();
            return Task.FromResult(values);
        }
    }
}
