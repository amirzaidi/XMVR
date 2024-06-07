namespace LibUtil
{
    public abstract class Bindable
    {
        public Unbinder Bind() =>
            new(BindInternal());

        protected abstract Action BindInternal();
    }

    public abstract class Bindable<T>
    {
        public Unbinder Bind(T arg) =>
            new(BindInternal(arg));

        protected abstract Action BindInternal(T arg);
    }
}
