using Xunit;
using MergeSharp;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{ 
    public class LWWRegisterTests
    {
        private readonly ITestOutputHelper _output;

        public LWWRegisterTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestLWWRegisterSingle()
        {
            LWWRegister<string> register = new();
            register.Assign("A", 1);
            register.Assign("B", 2);

            Assert.Equal("B", register.GetValue());
        }

        [Fact]
        public void TestLWWRegisterMerge()
        {
            LWWRegister<string> register1 = new();
            register1.Assign("A", 1);

            LWWRegister<string> register2 = new();
            register2.Assign("B", 2);

            register1.ApplySynchronizedUpdate(register2.GetLastSynchronizedUpdate());
            Assert.Equal("B", register1.GetValue());
        }
        [Fact]
        public void TestLWWRegisterMergeConflictFreeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                LWWRegister<string> register1 = new();
                register1.Assign("A", 1);

                LWWRegister<string> register2 = new();
                register2.Assign("B", 2);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                register1.ApplySynchronizedUpdate(register2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestLWWRegisterMergeWithConflictsAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                LWWRegister<string> register1 = new();
                register1.Assign("A", 1);

                LWWRegister<string> register2 = new();
                register2.Assign("A", 2);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                register1.ApplySynchronizedUpdate(register2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}
