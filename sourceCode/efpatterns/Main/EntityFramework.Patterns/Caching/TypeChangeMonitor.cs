using System;
using System.Runtime.Caching;

namespace EntityFramework.Patterns.Caching
{
    public class TypeChangeMonitor : ChangeMonitor
    {

        private readonly string _uniqueId;
        
        public TypeChangeMonitor()
        {
            _uniqueId = string.Format("{0}-{1}", GetType().Name, Guid.NewGuid());

            InitializationComplete();
        }

        protected override void Dispose(bool disposing)
        {
            Dispose();
        }

        public override string UniqueId
        {
            get { return _uniqueId; }
        }

        public void OnInvalidateCacheElement(object sender, EventArgs args)
        {
            OnChanged(null);
        }

    }
}