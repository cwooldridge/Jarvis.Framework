﻿using System;
using System.Configuration;
using Castle.Core.Logging;
using CommonDomain.Core;
using Jarvis.Framework.Kernel.Engine;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.TestHelpers;
using Jarvis.Framework.Tests.Support;
using Jarvis.NEventStoreEx.CommonDomainEx.Persistence.EventStore;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Jarvis.Framework.Tests.EngineTests
{
    [TestFixture]
    public class EngineSetupTests
    {
        private EventStoreFactory _factory;
        string _connectionString;
        private MongoDatabase _db;

        [SetUp]
        public void SetUp()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["eventstore"].ConnectionString;
            this._db = TestHelper.CreateNew(_connectionString);
            
            var loggerFactory = Substitute.For<ILoggerFactory>();
            loggerFactory.Create(Arg.Any<Type>()).Returns(NullLogger.Instance);
            _factory = new EventStoreFactory(loggerFactory);
        }

        [Test]
        public void setup_with_mongodb()
        {
            _factory.BuildEventStore(_connectionString);

            Assert.IsTrue(_db.Server.DatabaseExists(_db.Name));
            Assert.IsTrue(_db.CollectionExists("Commits"));
            Assert.IsTrue(_db.CollectionExists("Streams"));
        }

        [Test]
        public void save_stream()
        {
            var eventStore = _factory.BuildEventStore(_connectionString);
            var repo = new RepositoryEx(
                eventStore,
                new AggregateFactory(),
                new ConflictDetector(),
                new IdentityManager(new InMemoryCounterService())
            );

            var aggregate = TestAggregateFactory.Create<SampleAggregate, SampleAggregate.State>(new SampleAggregate.State(), new SampleAggregateId(1));
            aggregate.Create();
            repo.Save(
                aggregate,
                new Guid("{7A63A302-5575-4148-A86D-4421AAFF2019}"),
                null
            );
        }
    }
}
