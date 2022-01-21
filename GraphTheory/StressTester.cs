using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTheory
{
    internal class StressTester
    {
        public volatile int currentExecutionCount = 0;
        public volatile int maxConcurrent = 0;

        public async Task Execute()
        {
            List<Task<long>> taskList = new List<Task<long>>();

            for (int i = 0; i < 10000; i++)
            {
                taskList.Add(DoMagic());
            }

            await Task.WhenAll(taskList.ToArray());


            Console.WriteLine("Done " + taskList.Sum(t => t.Result));
            Console.ReadLine();
            Console.WriteLine("Max: " + maxConcurrent);

        }


        private void Print(object state)
        {
            Console.WriteLine(currentExecutionCount);
        }

        private async Task<long> DoMagic()
        {
            return await Task.Factory.StartNew(() =>
            {
                Interlocked.Increment(ref currentExecutionCount);
                maxConcurrent = Math.Max(maxConcurrent, currentExecutionCount);
                Task.Delay(TimeSpan.FromMilliseconds(200));
                Interlocked.Decrement(ref currentExecutionCount);
                return 4;
            }
            , TaskCreationOptions.LongRunning
            );
        }
    }
}
