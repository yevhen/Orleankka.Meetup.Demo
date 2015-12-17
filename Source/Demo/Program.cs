using System;
using System.Diagnostics;
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

            Run(system).Wait();
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
            public override async Task<object> OnReceive(object message)
            {
                return await Log(message, x => Sample(x, base.OnReceive));
            }

            async Task<object> Log(object message, Func<object, Task<object>> next)
            {
                WriteLine($"{Id} received {message.GetType()}");
                var result = await next(message);
                WriteLine($"{Id} processed {message.GetType()}");
                return result;
            }

            async Task<object> Sample(object message, Func<object, Task<object>> next)
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await next(message);
                WriteLine($"{Id} did {message.GetType()} in {sw.ElapsedMilliseconds} ms");

                return result;
            }
        }
    }
}
