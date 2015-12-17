using System;
using System.Reflection;
using System.Threading.Tasks;

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
            Console.ReadKey(true);

            system.Dispose();
        }

        static async Task Run(IActorSystem system)
        {
            var foo = system.ActorOf<Foo>("world");
            Console.WriteLine(await foo.Ask<string>("Hello"));
        }

        class Foo : Actor
        {
            string On(string msg) => $"{msg}, {Id}!";
        }
    }
}
