namespace NextGen.src.Services
{
    public static class ServiceProvider
    {
        public static IServiceProvider Current { get; private set; }

        public static void Configure(IServiceProvider serviceProvider)
        {
            Current = serviceProvider;
        }
    }
}
