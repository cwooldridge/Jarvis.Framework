﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Jarvis.Framework.Kernel.Events;
using Jarvis.Framework.Kernel.ProjectionEngine;
using Jarvis.Framework.Shared.IdentitySupport;
using Jarvis.Framework.Shared.Messages;
using Jarvis.Framework.Shared.ReadModel;
using Jarvis.Framework.TestHelpers;
using Jarvis.Framework.Tests.EngineTests;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace Jarvis.Framework.Tests.ProjectionEngineTests.V2
{
    [TestFixture]
    public class ProjectionEngineTestsCheckpoints : AbstractV2ProjectionEngineTests
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
        }
          
        protected override void RegisterIdentities(IdentityManager identityConverter)
        {
            identityConverter.RegisterIdentitiesFromAssembly(typeof(SampleAggregateId).Assembly);
        }

        protected override string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["engine"].ConnectionString;
        }

        protected override IEnumerable<IProjection> BuildProjections()
        {
            var writer = new CollectionWrapper<SampleReadModel, string>(StorageFactory,new NotifyToNobody());
            yield return new Projection(writer);
            var writer3 = new CollectionWrapper<SampleReadModel3, string>(StorageFactory, new NotifyToNobody());
            if (returnProjection3) yield return new Projection3(writer3);
        }
        private Boolean returnProjection3 = true;

        [Test]
        public async void run_poller()
        {

            var projected = _statusChecker.IsCheckpointProjectedByAllProjection("2");
            if (projected)
            {
                var allProjected = _checkpoints.FindAll().ToList();
                foreach (var checkpoint in allProjected)
                {
                    Console.WriteLine("Projection {0} at checkpoint {1}",
                        checkpoint.Signature, checkpoint.Current);
                }
            }
            Console.WriteLine("FIRST CHECK DONE");
            var aggregate = TestAggregateFactory.Create<SampleAggregate, SampleAggregate.State>(new SampleAggregateId(1));
            aggregate.Create();
            Repository.Save(aggregate, Guid.NewGuid(), h => { });

            aggregate = TestAggregateFactory.Create<SampleAggregate, SampleAggregate.State>(new SampleAggregateId(2));
            aggregate.Create();
            Repository.Save(aggregate, Guid.NewGuid(), h => { });

            var stream = _eventStore.Advanced.GetFrom("0");
            var lastCommit = stream.Last();

            //need to wait for at least one checkpoint written to database.
            DateTime startTime = DateTime.Now;
            while (!_checkpoints.Find(Query.Null).Any() &&
                DateTime.Now.Subtract(startTime).TotalMilliseconds < 2000) //2 seconds timeout is fine
            {
                Thread.Sleep(100);
            }

            projected = _statusChecker.IsCheckpointProjectedByAllProjection(lastCommit.CheckpointToken);
            if (projected)
            {
                var allProjected = _checkpoints.FindAll().ToList();
                foreach (var checkpoint in allProjected)
                {
                    Console.WriteLine("Projection {0} at checkpoint {1}", 
                        checkpoint.Signature, checkpoint.Current);
                }
            }
            Assert.That(projected, Is.False);
             
            await Engine.UpdateAndWait();
            Assert.That(_statusChecker.IsCheckpointProjectedByAllProjection(lastCommit.CheckpointToken), Is.True);
        }

        
    }
}