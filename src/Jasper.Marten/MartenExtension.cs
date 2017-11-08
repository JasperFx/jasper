using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Jasper.Marten;
using Jasper.Marten.Codegen;
using Marten;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;

// SAMPLE: MartenExtension
[assembly:JasperModule(typeof(MartenExtension))]

namespace Jasper.Marten
{
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<StoreOptions>();

            registry.Services.AddSingleton<SessionCommitListener>();

            registry.Services.AddSingleton<IDocumentStore>(x =>
            {
                var storeOptions = x.GetService<StoreOptions>();
                var documentStore = new DocumentStore(storeOptions);
                storeOptions.Listeners.Add(x.GetService<SessionCommitListener>());
                return documentStore;
            });

            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().OpenSession());
            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().QuerySession());

            registry.Generation.Sources.Add(new SessionVariableSource());

        }
    }

    /// <summary>
    /// This class is used by the MartenOutboxBus class to react to the commit of sessions it attaches to.
    /// </summary>
    public class SessionCommitListener : DocumentSessionListenerBase
    {
        private readonly ConcurrentDictionary<IDocumentSession, List<Action>> _commitCallbacks
            = new ConcurrentDictionary<IDocumentSession, List<Action>>();

        public Action RegisterCallbackAfterCommit(IDocumentSession session, Action callback)
        {
            var list = _commitCallbacks.GetOrAdd(session, s => new List<Action>());
            // Note: we have one list per IDocumentSession and expect any given IDocumentSession instance
            // to be used by only one thread at a time, so we're not synchronizing access to the list, just
            // the dictionary that's shared by all sessions.
            list.Add(callback);

            // this is the unregistration function
            void UnregisterCallback()
            {
                // Again, we're not concerned about thread-safety: the unregistration shouldn't be
                // happening simultaneously with any RegisterCallback, or even AfterCommit of the same session.
                list.Remove(callback);
                if (list.Count == 0)
                {
                    _commitCallbacks.TryRemove(session, out var theOldList);
                }
            }

            return UnregisterCallback;
        }

        public override void AfterCommit(IDocumentSession session, IChangeSet commit)
        {
            if (_commitCallbacks.TryGetValue(session, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback.Invoke();
                }
            }
        }

        public override Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
        {
            if (_commitCallbacks.TryGetValue(session, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback.Invoke();
                }
            }
            return Task.CompletedTask;
        }

    }

}
// ENDSAMPLE
