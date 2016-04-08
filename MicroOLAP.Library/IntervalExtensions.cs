using System;
using System.Collections.Generic;

namespace MicroOLAP.Library
{
    /// <summary>
    /// Меторы для работы интервалами произвольного контекста
    /// </summary>
    public static class IntervalExtensions
    {
        /// <summary>
        /// Проверяет равны ли два интервала друг другу
        /// </summary>
        public static bool EqualsTo<TMeasure>(this Interval<TMeasure> instance, Interval<TMeasure> other)
            where TMeasure : IComparable<TMeasure>
        {
            return instance.Begin.CompareTo(other.Begin) == 0 && instance.End.CompareTo(other.End) == 0;
        }

        /// <summary>
        /// Проверяет пересекаются ли два интервала
        /// </summary>
        public static bool IntersectsWith<TMeasure>(this Interval<TMeasure> instance, Interval<TMeasure> other)
            where TMeasure : IComparable<TMeasure>
        {
            return instance.End.CompareTo(other.Begin) >= 0 && instance.Begin.CompareTo(other.End) <= 0;
        }

        /// <summary>
        /// Находит пересечение текущего и другого интервала
        /// </summary>
        public static Interval<TMeasure> Intersect<TMeasure>(this Interval<TMeasure> instance, Interval<TMeasure> other)
            where TMeasure : IComparable<TMeasure>
        {
            if (!instance.IntersectsWith(other)) return null;
            if (instance.EqualsTo(other)) return instance;

            var beginBorder = instance.Begin.CompareTo(other.Begin) > 0 ? instance.Begin : other.Begin;
            var endBorder = instance.End.CompareTo(other.End) < 0 ? instance.End : other.End;

            return new Interval<TMeasure>(beginBorder, endBorder);
        }

        /// <summary>
        /// Находит остатки от пересечения текущего и другого интервала (то есть остатки + пересесение = исходный интервал)
        /// </summary>
        public static List<Interval<TMeasure>> Difference<TMeasure>(this Interval<TMeasure> instance, Interval<TMeasure> other, Func<TMeasure, int, TMeasure> shifter)
            where TMeasure : IComparable<TMeasure>
        {
            if (!instance.IntersectsWith(other)) return new List<Interval<TMeasure>> { instance };

            var result = new List<Interval<TMeasure>>();
            if (instance.Begin.CompareTo(other.Begin) < 0)
            {
                result.Add(instance.End.CompareTo(other.Begin) < 0
                    ? instance
                    : new Interval<TMeasure>(instance.Begin, shifter(other.Begin, -1)));
            }
            if (instance.End.CompareTo(other.End) > 0)
            {
                result.Add(instance.Begin.CompareTo(other.End) > 0
                    ? instance
                    : new Interval<TMeasure>(shifter(other.End, +1), instance.End));
            }
            return result;
        }

        /// <summary>
        /// Объединяет два интервала
        /// </summary>
        public static Interval<TMeasure> Merge<TMeasure>(this Interval<TMeasure> instance, Interval<TMeasure> other, Func<TMeasure, int, TMeasure> shifter)
            where TMeasure : IComparable<TMeasure>
        {
            var extended = new Interval<TMeasure>(shifter(instance.Begin, -1), shifter(instance.End, +1));
            if (!extended.IntersectsWith(other)) return null;
            if (instance.EqualsTo(other)) return instance;

            var beginBorder = instance.Begin.CompareTo(other.Begin) <= 0 ? instance.Begin : other.Begin;
            var endBorder = instance.End.CompareTo(other.End) >= 0 ? instance.End : other.End;

            return new Interval<TMeasure>(beginBorder, endBorder);
        }
    }
}