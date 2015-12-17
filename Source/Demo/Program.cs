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
                .UseInMemoryPubSubStore()
                .Register(Assembly.GetExecutingAssembly())
                .Done();

            Run(system).Wait();
            ReadKey(true);

            system.Dispose();
        }

        static async Task Run(IActorSystem system)
        {
            var inventory = system.ActorOf<Inventory>("#");
            await inventory.Tell(new Subscribe {Item = "INV-1"});
            await inventory.Tell(new Subscribe {Item = "INV-2"});

            var item1 = system.StreamOf("sms", "INV-1");
            var item2 = system.StreamOf("sms", "INV-2");

            await item1.Push(new InventoryItemCreated());
            await item1.Push(new InventoryItemRenamed());

            await item2.Push(new InventoryItemCreated());
            await item2.Push(new InventoryItemDeactivated());

            WriteLine(await inventory.Ask<int>(new Total()));
        }
        
        [Serializable] class InventoryItemCreated     {}
        [Serializable] class InventoryItemRenamed     {}
        [Serializable] class InventoryItemDeactivated {}
        
        public class Inventory : Actor
        {
            void On(Subscribe x) => System
                .StreamOf("sms", x.Item)
                .Subscribe(this);

            int total;
            int On(Total x) => total;

            void On(InventoryItemCreated e)     => total++;
            void On(InventoryItemDeactivated e) => total--;
        }

        [Serializable] class Subscribe
        {
            public string Item;
        }

        [Serializable] class Total {}
    }    
}
