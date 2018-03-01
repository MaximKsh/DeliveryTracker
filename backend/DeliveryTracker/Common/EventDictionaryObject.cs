using System.Collections.Generic;

namespace DeliveryTracker.Common
{
    public class EventDictionaryObject : DictionaryObject
    {
        #region public
        
        public override void SetDictionary(
            IDictionary<string, object> dict)
        {
            this.BeforeSetDictionary(dict);
            base.SetDictionary(dict);
            this.AfterSetDictionary(dict);
        }

        public override IDictionary<string, object> GetDictionary()
        {
            this.BeforeGetDictionary();
            var dict = base.GetDictionary();
            this.AfterGetDictionary();
            return dict;
        }
        
        
        #endregion
        
        #region protected
        
        protected virtual void BeforeSetDictionary(IDictionary<string, object> dict)
        {   
        }

        protected virtual void AfterSetDictionary(IDictionary<string, object> dict)
        {
        }
        
        protected virtual void BeforeGetDictionary()
        {   
        }

        protected virtual void AfterGetDictionary()
        {   
        }
        
        protected virtual bool BeforeGet<T>(
            string key,
            T defaultValue, 
            out T substitutedValue)
        {
            substitutedValue = defaultValue;
            return false;
        }

        protected virtual bool AfterGet<T>(
            string key,
            T resultValue,
            T defaultValue, 
            out T substitutedValue)
        {
            substitutedValue = defaultValue;
            return false;
        }
        
        protected virtual bool BeforeGetObject<T>(
            string key,
            T defaultValue, 
            out T substitutedValue) where T : IDictionaryObject, new ()
        {
            substitutedValue = defaultValue;
            return false;
        }

        protected virtual bool AfterGetObject<T>(
            string key,
            T resultValue,
            T defaultValue, 
            out T substitutedValue) where T : IDictionaryObject, new ()
        {
            substitutedValue = defaultValue;
            return false;
        }
        
        protected virtual bool BeforeGetList<T>(
            string key,
            IList<T> defaultValue, 
            out IList<T> substitutedValue) where T : IDictionaryObject, new ()
        {
            substitutedValue = defaultValue;
            return false;
        }

        protected virtual bool AfterGetList<T>(
            string key,
            IList<T> resultValue,
            IList<T> defaultValue, 
            out IList<T> substitutedValue) where T : IDictionaryObject, new ()
        {
            substitutedValue = defaultValue;
            return false;
        }
        
        protected virtual bool BeforeSet<T>(
            string key,
            T value)
        {
            return false;
        }
        
        protected virtual void AfterSet<T>(
            string key,
            T value)
        {
        }
        
        protected override T Get<T>(
            string key,
            T defaultValue = default)
        {
            if (!this.BeforeGet(key, defaultValue, out var substitutedValue))
            {
                return substitutedValue;
            }

            var result = base.Get(key, defaultValue);

            if (!this.AfterGet(key, result, defaultValue, out substitutedValue))
            {
                return substitutedValue;
            }
            
            return result;
        }
        
        protected override T GetObject<T>(
            string key,
            T defaultValue = default)
        {
            if (!this.BeforeGetObject(key, defaultValue, out var substitutedValue))
            {
                return substitutedValue;
            }

            var result = base.GetObject(key, defaultValue);
            
            if (!this.AfterGetObject(key, result, defaultValue, out substitutedValue))
            {
                return substitutedValue;
            }
            
            return result;
        }
        
        protected override IList<T> GetList<T>(
            string key,
            IList<T> defaultValue = default)
        {
            if (!this.BeforeGetList(key, defaultValue, out var substitutedValue))
            {
                return substitutedValue;
            }

            var result = base.GetList(key, defaultValue);

            if (!this.AfterGetList(key, result, defaultValue, out substitutedValue))
            {
                return substitutedValue;
            }
            
            return result;
        }
        
        protected override void Set(
            string key,
            object value)
        {
            if (this.BeforeSet(key, value))
            {
                return;
            }

            base.Set(key, value);
            
            this.AfterSet(key, value);
        }

        
        #endregion
    }
}