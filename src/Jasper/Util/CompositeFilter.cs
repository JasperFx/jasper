using System;
using System.Collections.Generic;
using System.Linq;

namespace Jasper.Util
{
    public class CompositeFilter<T>
    {
        private readonly CompositePredicate<T> _excludes = new CompositePredicate<T>();
        private readonly CompositePredicate<T> _includes = new CompositePredicate<T>();

        public CompositePredicate<T> Includes
        {
            get { return _includes; }
            set { }
        }

        public CompositePredicate<T> Excludes
        {
            get { return _excludes; }
            set { }
        }

        public bool Matches(T target)
        {
            return Includes.MatchesAny(target) && Excludes.DoesNotMatcheAny(target);
        }
    }

    public class CompositePredicate<T>
    {
        private readonly List<Func<T, bool>> _list = new List<Func<T, bool>>();
        private Func<T, bool> _matchesAll = x => true;
        private Func<T, bool> _matchesAny = x => true;
        private Func<T, bool> _matchesNone = x => false;

        public void Add(Func<T, bool> filter)
        {
            _matchesAll = x => _list.All(predicate => predicate(x));
            _matchesAny = x => _list.Any(predicate => predicate(x));
            _matchesNone = x => !MatchesAny(x);

            _list.Add(filter);
        }

        public static CompositePredicate<T> operator +(CompositePredicate<T> invokes, Func<T, bool> filter)
        {
            invokes.Add(filter);
            return invokes;
        }

        public bool MatchesAll(T target)
        {
            return _matchesAll(target);
        }

        public bool MatchesAny(T target)
        {
            return _matchesAny(target);
        }

        public bool MatchesNone(T target)
        {
            return _matchesNone(target);
        }

        public bool DoesNotMatcheAny(T target)
        {
            return _list.Count == 0 ? true : !MatchesAny(target);
        }
    }
}
