using System;
using System.Threading.Tasks;
using NUnit.Framework;

using Orleankka.TestKit;

namespace Demo.Tests
{
    [TestFixture]
    public class TestKitFixture
    {
        ActorRuntimeMock runtime;
        ActorRefMock bar;
        Foo foo;

        [SetUp]
        public void SetUp()
        {
            runtime = new ActorRuntimeMock();
            bar = runtime.System.MockActorOf<Bar>("#");
            foo = new Foo("id", runtime).Define();
        }

        [Test]
        public async Task When_request_processed_succesfully()
        {
            bar.ExpectAsk<Request>(x => x.Text == "test")
               .Return("some");

            Assert.That(await foo.Dispatch("test"), Is.EqualTo("some"));
        }

        [Test]
        public async Task When_request_failed()
        {
            bar.ExpectAsk<Request>()
               .Throw(new ApplicationException());

            Assert.That(await foo.Dispatch("test"), Is.EqualTo("default"));
        } 
    }
}
