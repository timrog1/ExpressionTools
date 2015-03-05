    public static class EnumerableExtensions
    {
        public static IEnumerable<IGrouping<TKey, TElement>> SequenceGroup<TKey, TElement>(
            this IEnumerable<TElement> source,
            Func<TElement, TKey> keySelector)
        {
            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var group = new Group<TKey, TElement>(enumerator, keySelector);
            while (group.HasElements)
            {
                yield return group;
                group = new Group<TKey, TElement>(enumerator, keySelector);
            }
        }

        private class Group<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly IEnumerator<TElement> enumerator;

            private readonly Func<TElement, TKey> keySelector;

            public Group(IEnumerator<TElement> enumerator, Func<TElement, TKey> keySelector)
            {
                this.enumerator = enumerator;
                this.keySelector = keySelector;
                HasElements = enumerator.MoveNext();
                Key = keySelector(enumerator.Current);
            }

            public TKey Key { get; private set; }

            public bool HasElements { get; private set; }

            public IEnumerator<TElement> GetEnumerator()
            {
                do
                {
                    if (!Key.Equals(keySelector(enumerator.Current)))
                    {
                        yield break;
                    }

                    yield return enumerator.Current;
                }
                while (enumerator.MoveNext());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
