using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibGen
{
    public class ClassGen
    {
        public int SpacesPerIndent { get; private set; }
        public int ElementCount { get; private set; }
        public string Prefix { get; private set; }
        public string Suffix { get; private set; }
        public string Source {
            get { return src; }
        }
        public string NumType { get; private set; }

        public static string[] ElementNames =
        {
            "x",
            "y",
            "z",
            "w"
        };

        int indent = 0;
        string src = "";

        public ClassGen(string prefix, string suffix, int elemCount, string numType)
        {
            NumType = numType;
            Prefix = prefix;
            Suffix = suffix;
            ElementCount = elemCount;
            SpacesPerIndent = 4;
        }

        public string ClassName()
        {
            return Prefix + ElementCount + Suffix;
        }

        public void PerElement(string format, params object[] extraArgs)
        {
            for (int i = 0; i < ElementCount; ++i)
            {
                AddLine(FormatExtra(format, extraArgs, ElementNames[i], NumType));
            }
        }

        public static string FormatExtra(string format, object[] extraParams, params object[] baseParams)
        {
            object[] objs = new object[extraParams.Length + baseParams.Length];

            for (int i = 0; i < baseParams.Length; ++i)
            {
                objs[i] = baseParams[i];
            }

            for (int i = 0; i < extraParams.Length; ++i)
            {
                objs[baseParams.Length + i] = extraParams[i];
            }

            return String.Format(format, objs);
        }

        public void StartMethod(string methodProto)
        {
            AddLine(methodProto);
            PushIndent();
        }

        public void EndMethod()
        {
            PopIndent();
            AddLine("}\n");
        }

        public void AddLine(string line)
        {
            src += MakeIndent() + line + "\n";
        }

        public string MakeIndent()
        {
            return new string(' ', indent * SpacesPerIndent);
        }

        public void PushIndent()
        {
            ++indent;
        }

        public void PopIndent()
        {
            --indent;
        }

        public void AddElementWiseOp(string op)
        {
            StartMethod(
                String.Format(
                    "public static {0} operator {1} ({0} l, {0} r)",
                    ClassName(),
                    op
                )
            );

            AddLine(ClassName() + " ret;");
            PerElement("ret.{0} = ({1})(l.{0} {2} r.{0});", op);
            AddLine("return ret;");

            EndMethod();
        }

        public void AddConstructors(string[] otherSuffix)
        {
            foreach (string s in otherSuffix)
            {
                // Truncating and fill constructors
                for (int i = 2; i <= ElementCount; ++i)
                {
                    StartMethod(
                        String.Format(
                            "public {0} ({1} other)",
                            ClassName(),
                            Prefix + i + s
                        )
                    );

                    for (int j = 0; j < i; ++j)
                    {
                        AddLine(String.Format("{0} = ({1})other.{0};", ElementNames[j], NumType));
                    }

                    for (int j = i; j < ElementCount; ++j)
                    {
                        AddLine(ElementNames[j] + " = 0;");
                    }

                    EndMethod();
                    AddLine("");
                }

                // Optional padded constructors
                // Truncating and fill constructors
                for (int i = 2; i < ElementCount; ++i)
                {
                    string pad = "";
                    for (int j = i; j < ElementCount; ++j)
                    {
                        pad += String.Format(", {0} {1}", NumType, ElementNames[j]);
                    }

                    StartMethod(
                        String.Format(
                            "public {0} ({1} other{2})",
                            ClassName(),
                            Prefix + i + s,
                            pad
                        )
                    );

                    for (int j = 0; j < i; ++j)
                    {
                        AddLine(String.Format("{0} = ({1})other.{0};", ElementNames[j], NumType));
                    }

                    for (int j = i; j < ElementCount; ++j)
                    {
                        AddLine(String.Format("this.{0} = {0};", ElementNames[j]));
                    }

                    EndMethod();
                    AddLine("");
                }
            }

            // Specify each element in the constructor
            string fullParams = "";
            for (int j = 0; j < ElementCount; ++j)
            {
                if (j > 0)
                {
                    fullParams += ", ";
                }
                fullParams += String.Format("{0} {1}", NumType, ElementNames[j]);
            }

            StartMethod(
                String.Format(
                    "public {0} ({1})",
                    ClassName(),
                    fullParams
                )
            );

            for (int j = 0; j < ElementCount; ++j)
            {
                AddLine(String.Format("this.{0} = {0};", ElementNames[j]));
            }

            EndMethod();
            AddLine("");

            // Fill all elements with a scalar

            StartMethod(
                String.Format(
                    "public {0} ({1} fillValue)",
                    ClassName(),
                    NumType
                )
            );

            for (int j = 0; j < ElementCount; ++j)
            {
                AddLine(String.Format("this.{0} = fillValue;", ElementNames[j]));
            }

            EndMethod();
            AddLine("");
        }
    }
}
