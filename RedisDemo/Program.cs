using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;

namespace RedisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IRedisNativeClient client = new RedisClient())
            {
                client.Set("urn:messages:1", Encoding.UTF8.GetBytes("Hello C# World!"));
            }

            using (IRedisNativeClient client = new RedisClient())
            {
                var result = Encoding.UTF8.GetString(client.Get("urn:messages:1"));
                Console.WriteLine("Message: {0}", result);
            }

            using (IRedisClient client = new RedisClient())
            {
                var customerNames = client.Lists["urn:customernames"];
                customerNames.Clear();
                customerNames.Add("Joe");
                customerNames.Add("Mary");
                customerNames.Add("Bob");
            }

            using (IRedisClient client = new RedisClient())
            {
                var customerNames = client.Lists["urn:customernames"];
                foreach (var customerName in customerNames)
                {
                    Console.WriteLine("Customer: {0}", customerName);
                }
            }

            long lastId = 0;
            using (IRedisClient client = new RedisClient())
            {
                var customerClient = client.GetTypedClient<Customer>();
                var customer = new Customer()
                {
                    Id = customerClient.GetNextSequence(),
                    Address = "123 Main St",
                    Name = "Bob Green",
                    Orders =
                                           new List<Order>
                                               {
                                                   new Order {OrderNumber = "AB123"},
                                                   new Order {OrderNumber = "AB124"}
                                               }
                };
                var storedCustomer = customerClient.Store(customer);
                lastId = storedCustomer.Id;
            }

            using (IRedisClient client = new RedisClient())
            {
                var customerClient = client.GetTypedClient<Customer>();
                var customer = customerClient.GetById(lastId);
                Console.WriteLine("Got customer {0}, with name {1}", customer.Id, customer.Name);
            }

            using (IRedisClient client = new RedisClient())
            {
                var transaction = client.CreateTransaction();
                transaction.QueueCommand(c => c.Set("abc", 1));
                transaction.QueueCommand(c => c.Increment("abc", 1));
                transaction.Commit();
                var result = client.Get<int>("abc");
                Console.WriteLine(result);
            }

            using (IRedisClient client = new RedisClient())
            {
                //client.PublishMessage("debug", "Hello C#!");
                var sub = client.CreateSubscription();
                sub.OnMessage = (c, m) => Console.WriteLine("Got message: {0}, from channel {1}", m, c);
                sub.SubscribeToChannels("news");
            }

            Console.ReadLine();
        }
    }

    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<Order> Orders { get; set; } 
    }

    public class Order
    {
        public string OrderNumber { get; set; }
    }
}
