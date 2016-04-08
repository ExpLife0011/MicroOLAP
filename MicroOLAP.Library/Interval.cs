namespace MicroOLAP.Library
{
    /// <summary>
    /// Интервал проивольного типа
    /// </summary>
    public class Interval<TMeasure>
    {
        /// <summary>
        /// Начало интервала
        /// </summary>
        public TMeasure Begin { get; }

        /// <summary>
        /// Конец интервала
        /// </summary>
        public TMeasure End { get; }

        public Interval(TMeasure begin, TMeasure end)
        {
            Begin = begin;
            End = end;
        }

        /// <summary>
        /// Визуалиция для дебага
        /// </summary>
        public override string ToString()
        {
            return $"{Begin} - {End}";
        }
    }
}