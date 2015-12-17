using System;
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
            var foo = system.ActorOf<Foo>("world");
            WriteLine(await foo.Ask<string>(new Bar {Text = "Hello"}));
        }
        
        [Serializable] class Bar
        {
            public string Text;
        }

        class Foo : Actor
        {
            string On(Bar msg) => $"{msg.Text}, {Id}!";
        }
    }
}
