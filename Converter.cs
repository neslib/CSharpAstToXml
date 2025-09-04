using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpAStToXml
{
    internal class Converter
    {
        public void Run(string SourceDirectory)
        {
            var Filenames = Directory.GetFiles(SourceDirectory, "*.cs");
            foreach (var Filename in Filenames)
                Convert(Filename);

            Console.WriteLine("Finished. Press Enter...");
            Console.ReadLine();
        }

        private void Convert(string Filename)
        {
            Console.WriteLine($"Converting {Filename}...");
            var Source = File.ReadAllText(Filename);
            var Tree = CSharpSyntaxTree.ParseText(Source);
            var CompilationUnit = Tree.GetCompilationUnitRoot();

            Filename = Path.ChangeExtension(Filename, ".astml");
            var Writer = new XmlTextWriter(Filename, Encoding.Default);
            Writer.Formatting = Formatting.Indented;
            WriteNode(CompilationUnit, Writer);
            Writer.Flush();
        }

        private void WriteNode(SyntaxNode node, XmlTextWriter writer)
        {
            var Type = node.GetType();
            writer.WriteStartElement(Type.Name);

            var Props = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var Prop in Props)
            {
                var Value = Prop.GetValue(node);
                if ((Value != null) && (Value is ValueType) && (Value is not IEnumerable))
                {
                    writer.WriteAttributeString(Prop.Name, Value.ToString());
                }
            }

            foreach (var Child in node.ChildNodes())
                WriteNode(Child, writer);

            writer.WriteEndElement();
        }
    }
}
