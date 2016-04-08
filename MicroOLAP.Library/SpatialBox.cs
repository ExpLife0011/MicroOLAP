using System;
using System.Collections.Generic;

namespace MicroOLAP.Library
{
    /// <summary>
    /// Многомерный объект хранящий произвольный контекст
    /// </summary>
    public class SpatialBox<TContext> where TContext : class
    {
        /// <summary>
        /// Временной интервал предложения
        /// </summary>
        public Interval<DateTime> OfferInterval { get; }

        /// <summary>
        /// Временной интервал ночей проживания
        /// </summary>
        public Interval<short> NightInterval { get; }

        /// <summary>
        /// Временной интервал сезона
        /// </summary>
        public Interval<DateTime> SeasonInterval { get; }

        /// <summary>
        /// Контекст
        /// </summary>
        public TContext Context { get; }

        public SpatialBox(Interval<DateTime> offerInterval, Interval<short> nightInterval, Interval<DateTime> seasonInterval, TContext context)
        {
            OfferInterval = offerInterval;
            NightInterval = nightInterval;
            SeasonInterval = seasonInterval;
            Context = context;
        }

        public SpatialBox<TContextOut> Copy<TContextOut>(Func<TContext, TContextOut> transformer)
            where TContextOut : class
        {
            return new SpatialBox<TContextOut>(OfferInterval, NightInterval, SeasonInterval, transformer.Invoke(Context));
        }

        public SpatialBox<TContext> Copy(
            Interval<DateTime> offerInterval = null, Interval<short> nightInterval = null, Interval<DateTime> seasonInterval = null)
        {
            return new SpatialBox<TContext>(offerInterval ?? OfferInterval, nightInterval ?? NightInterval, seasonInterval ?? SeasonInterval, Context);
        }

        public List<SpatialBox<TContext>> ToList()
        {
            return new List<SpatialBox<TContext>> { this };
        }

        /// <summary>
        /// Визуализация для дебага
        /// </summary>
        public override string ToString()
        {
            return $"{OfferInterval}, {NightInterval}, {SeasonInterval}, {Context}";
        }
    }
}