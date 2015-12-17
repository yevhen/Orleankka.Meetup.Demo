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
            var foo = system.ActorOf<Foo>("#");
            WriteLine(await foo.Ask<string>("test"));
        }
    }

    public class Foo : Actor
    {
        public Foo()
        {}

        public Foo(string id, IActorRuntime runtime)
            : base(id, runtime)
        {}

        Task<string> On(string msg)
        {
            var bar = System.ActorOf<Bar>("#");

            var message = new Request
            {
                Text = msg,
                Date = DateTime.UtcNow
            };

            try
            {
                return bar.Ask<string>(message);
            }
            catch (ApplicationException)
            {
                return Task.FromResult("default");
            }
        }
    }

    [Serializable]
    public class Request
    {
        public string Text;
        public DateTime Date;
    }

    public class Bar : Actor
    {
        string On(Request msg) => $"{Self}: {msg.Text} - {msg.Date}, !";
    }
}
