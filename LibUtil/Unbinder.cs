namespace LibUtil
{
    public class Unbinder(Action OnUnbind) : IDisposable
    {
        private readonly Action mOnUnbind = OnUnbind;

        public void Dispose() =>
            mOnUnbind();
    }
}
