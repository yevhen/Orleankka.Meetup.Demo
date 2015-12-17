using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using static System.Console;

using Orleankka;
using Orleankka.Playground;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Configure()
                .Playground()
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            try
            {
                Run(new ClusterGateway(system)).Wait();
            }
            catch (AggregateException e)
            {
                WriteLine(e.GetBaseException().Message);
            }

            ReadKey(true);

            system.Dispose();
        }

        static async Task Run(IActorSystem system)
        {
            var foo = system.TypedActorOf<Foo>("world");

            WriteLine(await foo.Ask(new Bar {Text = "Hello"}));

            await foo.Tell(new Baz {Text = "Hello"});
        }
        
        [Serializable] class Bar : ActorMessage<Foo, string>
        {
            public string Text;
        }

        [Serializable] class Baz : ActorMessage<Foo>
        {
            public string Text;
        }

        class Foo : MyActorBase
        {
            string On(Bar msg) => $"{msg.Text}, {Id}!";
            void On(Baz msg) => WriteLine(msg.Text + " again!");
        }

        class MyActorBase : Actor
        {
            public override Task<object> OnReceive(object message)
            {
                var envelope = (MessageEnvelope) message;

                var user = (string) envelope.Headers["User"];
                if (user != "admin")
                    throw new ApplicationException($"Access denied for {user}");

                return base.OnReceive(envelope.Body);
            }
        }

        [Serializable] class MessageEnvelope
        {
            public IDictionary<string, object> Headers;
            public object Body;
        }

        class ClusterGateway : IActorSystem
        {
            readonly IActorSystem system;

            public ClusterGateway(IActorSystem system)
            {
                this.system = system;
            }

            public ActorRef ActorOf(Type type, string id) => ActorOf(ActorPath.From(type, id));
            public ActorRef ActorOf(ActorPath path) => new SecuredActor(system.ActorOf(path));
            public StreamRef StreamOf(StreamPath path) => system.StreamOf(path);
            public void Dispose() => system.Dispose();
        }

        class SecuredActor : ActorRef
        {
            readonly ActorRef actor;

            public SecuredActor(ActorRef actor)
                : base(actor.Path)
            {
                this.actor = actor;
            }

            public override Task Tell(object message) => actor.Tell(Wrap(message));
            public override Task<TResult> Ask<TResult>(object message) => actor.Ask<TResult>(Wrap(message));
            public override void Notify(object message) => actor.Notify(Wrap(message));

            static MessageEnvelope Wrap(object message)
            {
                var headers = new Dictionary<string, object>();
                headers["User"] = "some"; // we might get it from HttpContext.Current.User
                return new MessageEnvelope {Headers = headers, Body = message};
            }
        }
    }
}
