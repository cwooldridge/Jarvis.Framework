using System;
using Jarvis.Framework.Shared.Messages;

namespace Jarvis.Framework.Tests.BusTests
{
    public class SampleMessage : IMessage
    {
        public SampleMessage(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
        public Guid MessageId { get { return Guid.Parse("E8ABD708-90C6-417C-87F0-4E317868E331"); } }
        public string Describe()
        {
            return "samplemessage";
        }
    }

    public class AnotherSampleMessage : IMessage
    {
        public AnotherSampleMessage(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }
        public Guid MessageId { get { return Guid.Parse("F8ABD708-90C6-417C-87F0-4E317868E331"); } }
        public string Describe()
        {
            return "samplemessage";
        }
    }
}