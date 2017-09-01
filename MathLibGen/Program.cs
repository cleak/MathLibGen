using System;

namespace MathLibGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ClassGen gen = new ClassGen("Vec", "i", 4, "int");
            gen.AddElementWiseOp("+");
            gen.AddConstructors(new string[]{
                "i",
                "s",
                "us",
                "d",
                "f"
            });
            Console.WriteLine(gen.Source);
        }
    }
}
