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

        public void StartNamespace(string nsName)
        {
            AddLine("namespace " + nsName + " {");
            PushIndent();
        }

        public void EndNamespace()
        {
            PopIndent();
            AddLine("}\n");
        }

        public void StartStruct()
        {
            AddLine("[StructLayout(LayoutKind.Sequential)]");
            AddLine(String.Format(@"struct {0} : IEquatable<{0}> {{", ClassName()));
            PushIndent();
        }

        public void EndStruct()
        {
            PopIndent();
            AddLine("}\n");
        }

        public void StartMethod(string methodProto)
        {
            AddLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            AddLine(methodProto + " {");
            PushIndent();
        }

        public void EndMethod()
        {
            PopIndent();
            AddLine("}\n");
        }

        public void AddVars()
        {
            PerElement("public {1} {0};");
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

        public void AddBoolOp(string op, string joinOp)
        {
            StartMethod(
                String.Format(
                    "public static bool operator {1} ({0} l, {0} r)",
                    ClassName(),
                    op
                )
            );

            string expr = "return ";
            for (int i = 0; i < ElementCount; ++i)
            {
                if (i > 0)
                {
                    expr += " " + joinOp + " ";
                }

                expr += String.Format("l.{0} {1} r.{0}", ElementNames[0], op);
            }
            AddLine(expr + ";");

            EndMethod();
        }

        public void AddEquals()
        {
            StartMethod(
                String.Format(
                    "public bool Equals({0} other)",
                    ClassName()
                )
            );

            string expr = "return ";
            for (int i = 0; i < ElementCount; ++i)
            {
                if (i > 0)
                {
                    expr += " && ";
                }

                expr += String.Format("{0} == other.{0}", ElementNames[0]);
            }
            AddLine(expr + ";");

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

        public void AddScalarOp(string op)
        {
            StartMethod(
                String.Format(
                    "public static {0} operator {1} ({0} l, {2} r)",
                    ClassName(),
                    op,
                    NumType
                )
            );

            AddLine(ClassName() + " ret;");
            PerElement("ret.{0} = ({1})(l.{0} {2} r);", op);
            AddLine("return ret;");

            EndMethod();

            AddScalarOpRev(op);
        }

        private void AddScalarOpRev(string op)
        {
            StartMethod(
                String.Format(
                    "public static {0} operator {1} ({2} l, {0} r)",
                    ClassName(),
                    op,
                    NumType
                )
            );

            AddLine(ClassName() + " ret;");
            PerElement("ret.{0} = ({1})(l {2} r.{0});", op);
            AddLine("return ret;");

            EndMethod();
        }

        public void AddDotProduct()
        {
            StartMethod(
                String.Format(
                    "public static {1} Dot({0} l, {0} r)",
                    ClassName(),
                    NumType
                )
            );

            AddLine(NumType + " ret = 0;");
            PerElement("ret += ({1})(l.{0} * r.{0});");
            AddLine("return ret;");

            EndMethod();

            // Non static version
            StartMethod(
                String.Format(
                    "public {1} Dot({0} r)",
                    ClassName(),
                    NumType
                )
            );

            AddLine(NumType + " ret = 0;");
            PerElement("ret += ({1})({0} * r.{0});");
            AddLine("return ret;");

            EndMethod();
        }

        public void AddCrossProduct()
        {
            if (ElementCount != 3)
            {
                return;
            }

            StartMethod(
                String.Format(
                    "public static {0} Cross({0} u, {0} v)",
                    ClassName()
                )
            );

            AddLine(ClassName() + " ret;");
            AddLine(String.Format("ret.{3} = ({0})(u.{1} * v.{2} -u.{2} * v.{1} );",
                NumType, ElementNames[1], ElementNames[2], ElementNames[0]));
            AddLine(String.Format("ret.{3} = ({0})(u.{1} * v.{2} -u.{2} * v.{1} );",
                NumType, ElementNames[2], ElementNames[0], ElementNames[1]));
            AddLine(String.Format("ret.{3} = ({0})(u.{1} * v.{2} -u.{2} * v.{1} );",
                NumType, ElementNames[0], ElementNames[1], ElementNames[2]));
            AddLine("return ret;");
            EndMethod();

            // Non static version
            StartMethod(
                String.Format(
                    "public {0} Cross({0} v)",
                    ClassName()
                )
            );

            AddLine(ClassName() + " ret;");
            AddLine(String.Format("ret.{3} = ({0})({1} * v.{2} - {2} * v.{1} );",
                NumType, ElementNames[1], ElementNames[2], ElementNames[0]));
            AddLine(String.Format("ret.{3} = ({0})({1} * v.{2} - {2} * v.{1} );",
                NumType, ElementNames[2], ElementNames[0], ElementNames[1]));
            AddLine(String.Format("ret.{3} = ({0})({1} * v.{2} - {2} * v.{1} );",
                NumType, ElementNames[0], ElementNames[1], ElementNames[2]));
            AddLine("return ret;");
            EndMethod();
        }

        public void AddMagnitude()
        {
            // Length
            StartMethod(
                String.Format(
                    "public {0} Magnitude()",
                    NumType
                )
            );
            AddLine(String.Format("return ({0})Math.Sqrt(Dot(this, this));", NumType));
            EndMethod();

            // Length squared
            StartMethod(
                String.Format(
                    "public {0} MagnitudeSqrd()",
                    NumType
                )
            );
            AddLine("return Dot(this, this);");
            EndMethod();
        }

        public void AddNormal()
        {
            // Static version
            StartMethod(
                String.Format(
                    "public static {0} Normalize({0} vec)",
                    ClassName()
                )
            );

            AddLine(String.Format("{0} oneOverSqrt = 1.0{1} / vec.Magnitude();", NumType, Suffix));
            AddLine("return vec * oneOverSqrt;");
            EndMethod();

            // Non-static version
            StartMethod(
                String.Format(
                    "public {0} Normalized()",
                    ClassName()
                )
            );
            AddLine("return Normalize(this);");
            EndMethod();
        }

        public void AddIndexOp()
        {
            AddLine(String.Format("public {0} this[int elmentIndex] {{", NumType));
            PushIndent();

            // Getter 
            AddLine("get {");
            PushIndent();
            AddLine("switch (elmentIndex) {");

            for (int i = 0; i < ElementCount - 1; ++i)
            {
                AddLine(String.Format("case {0}: return {1};", i, ElementNames[i]));
            }
            AddLine(String.Format("default: return {0};", ElementNames[ElementCount - 1]));

            AddLine("}");
            PopIndent();
            AddLine("}");

            // Setter
            AddLine("set {");
            PushIndent();
            AddLine("switch (elmentIndex) {");

            for (int i = 0; i < ElementCount - 1; ++i)
            {
                AddLine(String.Format("case {0}: {1} = value; break;", i, ElementNames[i]));
            }
            AddLine(String.Format("default: {0} = value; break;", ElementNames[ElementCount - 1]));

            AddLine("}");
            PopIndent();
            AddLine("}");

            EndMethod();
        }
    }
}
