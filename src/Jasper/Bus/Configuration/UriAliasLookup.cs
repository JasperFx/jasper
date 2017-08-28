using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Bus.Configuration
{
    public class UriAliasLookup
    {
        private readonly IDictionary<string, IUriLookup> _lookups = new Dictionary<string, IUriLookup>();
        private readonly IDictionary<Uri, Uri> _aliases = new Dictionary<Uri, Uri>();

        public UriAliasLookup(IUriLookup[] lookups)
        {
            foreach (var lookup in lookups)
            {
                _lookups.SmartAdd(lookup.Protocol, lookup);
            }
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
}
