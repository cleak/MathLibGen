using System;
using System.IO;

namespace MathLibGen
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] suffixes = new string[]{
                "l",
                "ul",
                "i",
                "ui",
                "s",
                "us",
                "b",
                "d",
                "f"
            };

            string[] numTypes = new string[]{
                "long",
                "ulong",
                "int",
                "uint",
                "short",
                "ushort",
                "byte",
                "double",
                "float"
            };

            String finalSrc = "using System;\nusing System.Runtime.CompilerServices;\nusing System.Runtime.InteropServices;\n\nnamespace LiteBox.LMath {\n";

            for (int j = 0; j < suffixes.Length; ++j)
            {
                string suffix = suffixes[j];
                string numType = numTypes[j];
                bool isIntType = (j < 7);

                for (int i = 2; i <= 4; ++i)
                {
                    ClassGen gen = new ClassGen("Vec", suffix, i, numType);
                    gen.PushIndent();
                    gen.StartStruct();
                    gen.AddVars();
                    gen.AddConstructors(suffixes);

                    gen.AddElementWiseOp("+");
                    gen.AddElementWiseOp("-");
                    gen.AddElementWiseOp("*");
                    gen.AddElementWiseOp("/");

                    // Scalar ops
                    gen.AddScalarOp("+");
                    gen.AddScalarOp("-");
                    gen.AddScalarOp("*");
                    gen.AddScalarOp("/");

                    if (isIntType)
                    {
                        // Int only mehtods
                        gen.AddElementWiseOp("%");
                        gen.AddScalarOp("%");
                    }
                    else
                    {
                        // Float only methods
                        gen.AddMagnitude();
                        gen.AddNormal();
                    }

                    gen.AddDotProduct();
                    if (i == 3)
                    {
                        gen.AddCrossProduct();
                    }

                    // Boolean ops
                    gen.AddBoolOp("<=", "&&");
                    gen.AddBoolOp("<", "&&");
                    gen.AddBoolOp(">=", "&&");
                    gen.AddBoolOp(">", "&&");

                    gen.AddEquals();

                    gen.AddIndexOp();

                    gen.AddHash();
                    gen.AddToStr();

                    gen.AddPairwiseMethod("Min", "System.Math.Min");
                    gen.AddPairwiseMethod("Max", "System.Math.Max");

                    gen.EndStruct();
                    finalSrc += gen.Source;
                }
            }

            finalSrc += "};\n";
            File.WriteAllText("Vec.cs", finalSrc);

            Console.WriteLine(finalSrc);
        }
    }
}
