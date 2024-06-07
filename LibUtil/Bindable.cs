namespace LibUtil
{
    public abstract class Bindable : IDisposable
    {
        public abstract void Dispose();

        public class AutoUnbind(Action OnUnbind) : IDisposable
        {
            private readonly Action mOnUnbind = OnUnbind;

            public void Dispose() =>
                mOnUnbind();
        }
    }
}
