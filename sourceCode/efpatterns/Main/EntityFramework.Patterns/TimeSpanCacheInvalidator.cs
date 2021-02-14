using System;

namespace EntityFramework.Patterns
{
    public class TimeSpanCacheInvalidator : ICacheInvalidator
    {
        private readonly DateTime _additionTime;
        private readonly TimeSpan _duration;

        public TimeSpanCacheInvalidator() : this(TimeSpan.MaxValue) {}

        public TimeSpanCacheInvalidator(TimeSpan duration)
        {
            _additionTime = DateTime.Now;
            _duration = duration;
        }

        public bool IsValid
        {
            get { return DateTime.Now.Subtract(_additionTime) > _duration; }
        }
    }
}