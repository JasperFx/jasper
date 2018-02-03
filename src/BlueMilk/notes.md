# BlueMilk

* Frame --|> IFrame -- pull out the interface and use that from now on. Allows you to combine
  Variable & IFrame in one class

* ServiceRegistry --|> ServiceCollection
  * For<T>().Use<T>()
  * For<T>().Use<T>().Singleton()
  * ForSingleton<T>().Use<T>()

* Import the type scanning from SM. Hang off of ServiceRegistry.Scan()
* ServiceRegistry.Combine(IServiceCollection[]) : Task -- concat the service descriptions, finish all known
  scanners, apply service registrations

* Variable.CanBeReused : bool -- true by default

* Find the default service descriptor for a type
* Pick a constructor function based on the greediest constructor
* `ObjectBuilder` - wraps an `IServiceProvider`, uses reflection to build an object w/
  the right constructor args taken from the provider. If "*Settings" and there is no
  known registration, just build it directly

* ServiceRegistry.For<T>().BuildWith<TBuilder>(Expression<TBuilder, T>) -- try to use this
  later as an IVariableSource

* Build out the "build plan graph" for a type
  * Determine if it can be codegen'd, i.e. nothing that uses Func<IServiceProvider, object>
  * Use as an IVariableSource

* ConstructorCall --|> Frame, should do an IDisposable?
* AsyncFrame --|> Frame
* SyncFrame --|> Frame
* TryFinallyDisposeFrame --|> Frame
* InlineConstructorVariable --|> Variable, maybe it uses a "nullo" frame?


## Use Cases

1. Build a concrete type with only singleton arguments
1. Build a concrete type with transient dependencies, one deep
1. Build a concrete type with transient dependencies, two deep
1. Build a concrete type with container scoped dependencies, one deep
1. Build a concrete type with container scoped dependencies, two deep
1. Build a concrete type with transient, disposable dependency
1. Build a concrete type with container scoped, disposable dependency
1. Use variables within the generated method, i.e., singleton, injected fields, HttpContext kinda things
1. Deal with IEnumerable<T>, T[], IReadOnlyList<T>, IList<T>, List<T> dependencies



