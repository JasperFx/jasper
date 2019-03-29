using System;
using System.Collections.Concurrent;
using LamarCodeGeneration;
using LamarCompiler;

namespace Jasper.Messaging.Sagas
{
    public class InMemorySagaPersistor
    {
        private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

        public static string ToKey(Type documentType, object id)
        {
            return documentType.FullName + "/" + id;
        }


        public T Load<T>(object id) where T : class
        {
            var key = ToKey(typeof(T), id);


            if (_data.TryGetValue(key, out var value)) return value as T;


            return null;
        }

        public void Store<T>(T document)
        {
            var id = typeof(T).GetProperty("Id")?.GetValue(document);
            if (id == null)
                throw new InvalidOperationException(
                    $"Type {typeof(T).FullNameInCode()} does not have a public Id property");

            var key = ToKey(typeof(T), id);
            _data[key] = document;
        }

        public void Delete<T>(object id)
        {
            var key = ToKey(typeof(T), id);
            _data.TryRemove(key, out var doc);
        }
    }
}
