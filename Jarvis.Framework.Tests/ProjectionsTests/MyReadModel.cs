using Jarvis.Framework.Shared.ReadModel;

namespace Jarvis.Framework.Tests.ProjectionsTests
{
    public class MyReadModel : AbstractReadModel<string>
    {
        public string Text { get; set; }
    }
}