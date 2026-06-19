using TimeOn.UITests.Infrastructure;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TimeOn.UITests;

[CollectionDefinition(nameof(MauiUiTestCollection))]
public sealed class MauiUiTestCollection : ICollectionFixture<MauiUiTestHost>;
