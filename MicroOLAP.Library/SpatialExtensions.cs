using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroOLAP.Library
{
    /// <summary>
    /// Меторы для работы пространственными объектами произвольного контекста
    /// </summary>
    public static class SpatialExtensions
    {
        #region Intersection

        /// <summary>
        /// Находит пересечение текущего куба и постороннего куба
        /// </summary>
        public static SpatialBox<TContext> Intersect<TContext>(this SpatialBox<TContext> instance, SpatialBox<TContext> other, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            var offerIntersect = instance.OfferInterval.Intersect(other.OfferInterval);
            var nightIntersect = instance.NightInterval.Intersect(other.NightInterval);
            var seasonIntersect = instance.SeasonInterval.Intersect(other.SeasonInterval);

            if (offerIntersect == null || nightIntersect == null || seasonIntersect == null) return null;

            return new SpatialBox<TContext>(offerIntersect, nightIntersect, seasonIntersect, combiner(instance.Context, other.Context));
        }

        /// <summary>
        /// Находит пересечение текущего списка различающихся кубов с посторонним кубом
        /// </summary>
        public static List<SpatialBox<TContext>> Intersect<TContext>(this List<SpatialBox<TContext>> instances, SpatialBox<TContext> other, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            return instances.Select(x => x.Intersect(other, combiner)).Where(x => x != null).ToList();
        }

        /// <summary>
        /// Находит пересечение текущего куба со списком посторонних различающихся кубов
        /// </summary>
        public static List<SpatialBox<TContext>> Intersect<TContext>(this SpatialBox<TContext> instance, List<SpatialBox<TContext>> others, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            return others.Intersect(instance, combiner);
        }

        /// <summary>
        /// Находит пересечение текущего списка различающихся кубов с посторонним списком различающихся кубов
        /// </summary>
        public static List<SpatialBox<TContext>> Intersect<TContext>(this List<SpatialBox<TContext>> instances, List<SpatialBox<TContext>> others, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            return others.Aggregate(instances, (x, y) => x.Intersect(y, combiner)).ToList();
        }

        #endregion

        #region Difference

        /// <summary>
        /// Находит остатки от пересечения текущего куба с посторонним кубом
        /// </summary>
        public static List<SpatialBox<TContext>> Difference<TContext>(this SpatialBox<TContext> instance, SpatialBox<TContext> other)
            where TContext : class
        {
            var intersection = instance.Intersect(other, (x, y) => null); // we do not need context of intersection
            if (intersection == null) return new List<SpatialBox<TContext>> { instance };

            var result = new List<SpatialBox<TContext>>();
            result.AddRange(
                instance.OfferInterval.Difference(intersection.OfferInterval, (x, i) => x.AddDays(i))
                .Select(x => instance.Copy(offerInterval: x)).ToList());
            result.AddRange(
                instance.NightInterval.Difference(intersection.NightInterval, (x, i) => (short)(x + i))
                .Select(x => instance.Copy(offerInterval: intersection.OfferInterval, nightInterval: x)).ToList());
            result.AddRange(
                instance.SeasonInterval.Difference(intersection.SeasonInterval, (x, i) => x.AddDays(i))
                .Select(x => instance.Copy(offerInterval: intersection.OfferInterval, nightInterval: intersection.NightInterval, seasonInterval: x)).ToList());

            return result;
        }

        /// <summary>
        /// Находит остатки от пересечения текущего списка непересекающихся кубов с посторонним кубом
        /// </summary>
        public static List<SpatialBox<TContext>> Difference<TContext>(this List<SpatialBox<TContext>> instances, SpatialBox<TContext> other)
            where TContext : class
        {
            return instances.SelectMany(x => x.Difference(other)).Where(x => x != null).ToList();
        }

        /// <summary>
        /// Находит остатки от пересечения текущего кубоа с посторонним списком непересекающихся кубов
        /// </summary>
        public static List<SpatialBox<TContext>> Difference<TContext>(this SpatialBox<TContext> instance, List<SpatialBox<TContext>> others)
            where TContext : class
        {
            return others.Aggregate(instance.ToList(), (x, y) => x.Difference(y)).ToList();
        }

        /// <summary>
        /// Находит остатки от пересечения текущего списка непересекающихся кубов с посторонним списком непересекающихся кубов
        /// </summary>
        public static List<SpatialBox<TContext>> Difference<TContext>(this List<SpatialBox<TContext>> instances, List<SpatialBox<TContext>> others)
            where TContext : class
        {
            return others.Aggregate(instances, (x, y) => x.Difference(y)).ToList();
        }

        #endregion

        #region Merge

        /// <summary>
        /// Объединяет параллелепипеды, если возможно
        /// </summary>
        public static SpatialBox<TContext> Merge<TContext>(this SpatialBox<TContext> instance, SpatialBox<TContext> other)
            where TContext : class
        {
            var areOffersEqual = instance.OfferInterval.EqualsTo(other.OfferInterval);
            var areNightsEqual = instance.NightInterval.EqualsTo(other.NightInterval);
            var areSeasonsEqual = instance.SeasonInterval.EqualsTo(other.SeasonInterval);

            var offerMerged = instance.OfferInterval.Merge(other.OfferInterval, (x, i) => x.AddDays(i));
            var nightMerged = instance.NightInterval.Merge(other.NightInterval, (x, i) => (short)(x + i));
            var seasonMerged = instance.SeasonInterval.Merge(other.SeasonInterval, (x, i) => x.AddDays(i));

            if (offerMerged != null && areNightsEqual && areSeasonsEqual) return instance.Copy(offerInterval: offerMerged);
            if (areOffersEqual && nightMerged != null && areSeasonsEqual) return instance.Copy(nightInterval: nightMerged);
            if (areOffersEqual && areNightsEqual && seasonMerged != null) return instance.Copy(seasonInterval: seasonMerged);

            return null;
        }

        /// <summary>
        /// Склеивает параллелепипеды с одинаковым контекстом, если возможно
        /// </summary>
        public static List<SpatialBox<TContext>> Merge<TContext>(this List<SpatialBox<TContext>> instances, Func<TContext, int> identifier)
            where TContext : class
        {
            return Merge(instances, new ItemComparer<TContext>(identifier));
        }

        /// <summary>
        /// Склеивает параллелепипеды с одинаковым контекстом, если возможно
        /// </summary>
        public static List<SpatialBox<List<TContext>>> Merge<TContext>(this List<SpatialBox<List<TContext>>> instances, Func<TContext, int> identifier)
            where TContext : class
        {
            return Merge(instances, new ListComparer<TContext>(identifier));
        }

        class ItemComparer<TContext> : IEqualityComparer<TContext>
        {
            readonly Func<TContext, int> _identifier;
            public ItemComparer(Func<TContext, int> identifier) { _identifier = identifier; }
            public bool Equals(TContext first, TContext second) { return _identifier(first) == _identifier(second); }
            public int GetHashCode(TContext instance) { return _identifier(instance); }
        }

        class ListComparer<TContext> : IEqualityComparer<List<TContext>>
        {
            readonly Func<TContext, int> _identifier;
            public ListComparer(Func<TContext, int> identifier) { _identifier = identifier; }
            public bool Equals(List<TContext> first, List<TContext> second)
            {
                return !first.Select(x => _identifier(x)).Except(second.Select(x => _identifier(x))).Any();
            }
            public int GetHashCode(List<TContext> list)
            {
                return list.Select(x => _identifier(x)).OrderBy(x => x).Aggregate(list.Count, (x, y) => unchecked(x * 314159 + y));
            }
        }

        static List<SpatialBox<TContext>> Merge<TContext>(this List<SpatialBox<TContext>> instances, IEqualityComparer<TContext> comparer)
            where TContext : class
        {
            var groups = instances.GroupBy(x => x.Context, comparer).ToList();

            var result = new List<SpatialBox<TContext>>();
            foreach (var group in groups)
            {
                var resultGroup = new List<SpatialBox<TContext>>(group);
                while (true)
                {
                    Tuple<SpatialBox<TContext>, SpatialBox<TContext>, SpatialBox<TContext>> foundMerge = null;
                    for (var i = 0; i < resultGroup.Count; i++)
                    {
                        for (var j = i + 1; j < resultGroup.Count; j++)
                        {
                            var merge = resultGroup[i].Merge(resultGroup[j]);
                            if (merge == null) continue;
                            foundMerge = new Tuple<SpatialBox<TContext>, SpatialBox<TContext>, SpatialBox<TContext>>(resultGroup[i], resultGroup[j], merge);
                            break;
                        }
                        if (foundMerge != null) break;
                    }
                    if (foundMerge == null) break;

                    resultGroup.Remove(foundMerge.Item1);
                    resultGroup.Remove(foundMerge.Item2);
                    resultGroup.Add(foundMerge.Item3);
                }
                result.AddRange(resultGroup);
            }
            return result;
        }

        #endregion

        #region Other Work with SpatialBoxes

        /// <summary>
        /// Объединяет непересекающиеся параллелепипеды
        /// </summary>
        public static List<SpatialBox<TContext>> Join<TContext>(this List<SpatialBox<TContext>> instances, SpatialBox<TContext> other, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            var differences = instances.Difference(other);
            var intersections = instances.Intersect(other, combiner);

            var otherParts = new List<SpatialBox<TContext>> { other };
            foreach (var intersection in intersections)
            {
                otherParts = otherParts.Difference(intersection);
            }

            var result = differences.Concat(intersections).Concat(otherParts).ToList();

            return result;
        }

        /// <summary>
        /// Объединяет непересекающиеся параллелепипеды
        /// </summary>
        public static List<SpatialBox<TContext>> Distinct<TContext>(this List<SpatialBox<TContext>> instances, Func<TContext, TContext, TContext> combiner)
            where TContext : class
        {
            var spatialBoxes = new List<SpatialBox<TContext>>();
            foreach (var instance in instances)
            {
                spatialBoxes = spatialBoxes.Join(instance, combiner);
            }
            return spatialBoxes;
        }

        /// <summary>
        /// Трансформирует контекст внутри каждого из параллелепипедов
        /// </summary>
        public static List<SpatialBox<TContextOut>> Transform<TContextIn, TContextOut>(this List<SpatialBox<TContextIn>> instances, Func<TContextIn, TContextOut> transformer)
            where TContextIn : class where TContextOut : class
        {
            return instances.Select(x => x.Copy(transformer)).ToList();
        }

        #endregion
    }
}