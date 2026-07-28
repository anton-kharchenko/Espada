namespace Espada.Tests.Application.TestData
{
    public static class BuildContextPathTestData
    {
        public static TheoryData<string> InvalidRepositoryRelativePaths =>
        [
            "../secrets",
            "src/./app",
            "C:\\repository\\src",
            "/repository/src"
        ];
    }
}