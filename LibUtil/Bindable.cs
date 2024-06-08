namespace LibUtil
{
    public abstract class Bindable
    {
        public Unbinder Bind() =>
            new(BindInternal());

        protected abstract Action BindInternal();
    }

    public abstract class Bindable<T>
        where T : struct
    {
        protected abstract T BindDefault { get; }

        public Unbinder Bind(T? arg = null) =>
            new(BindInternal(arg ?? BindDefault));

        protected abstract Action BindInternal(T arg);
    }
}
