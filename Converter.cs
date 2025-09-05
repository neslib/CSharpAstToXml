using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CSharpAStToXml
{
    internal class Converter
    {
        public void Run(string ASourceDirectory)
        {
            var Filenames = Directory.GetFiles(ASourceDirectory, "*.cs");
            foreach (var Filename in Filenames)
                Convert(Filename);

            Console.WriteLine("Finished. Press Enter...");
            Console.ReadLine();
        }

        private void Convert(string AFilename)
        {
            Console.WriteLine($"Converting {AFilename}...");
            var Source = File.ReadAllText(AFilename);
            var Tree = CSharpSyntaxTree.ParseText(Source);
            var CompilationUnit = Tree.GetCompilationUnitRoot();

            AFilename = Path.ChangeExtension(AFilename, ".astml");
            var Writer = new XmlTextWriter(AFilename, Encoding.Default);
            Writer.Formatting = Formatting.Indented;
            WriteNode(CompilationUnit, Writer);
            Writer.Flush();
        }

        private void WriteNode(SyntaxNode ANode, XmlTextWriter AWriter)
        {
            var Type = ANode.GetType();
            var TypeName = Type.Name;
            if (TypeName.EndsWith("Syntax"))
                TypeName = TypeName.Remove(TypeName.Length - 6);
            AWriter.WriteStartElement(TypeName);

            var Props = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            WriteProperties(ANode, Props, AWriter);

            foreach (var ChildNodeOrToken in ANode.ChildNodesAndTokens())
            {
                if (ChildNodeOrToken.IsNode)
                    WriteNode(ChildNodeOrToken.AsNode()!, AWriter);
                else if (ChildNodeOrToken.IsToken)
                    WriteToken(ChildNodeOrToken.AsToken(), AWriter);

            }

            AWriter.WriteEndElement();
        }
        private void WriteToken(SyntaxToken AToken, XmlTextWriter AWriter)
        {
            AWriter.WriteStartElement("token");

            var Type = AToken.GetType();
            var Props = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            WriteProperties(AToken, Props, AWriter);

            AWriter.WriteEndElement();
        }
        private void WriteProperties(object AInstance, PropertyInfo[] AProps, XmlTextWriter AWriter)
        {
            foreach (var Prop in AProps)
            {
                var Value = Prop.GetValue(AInstance);
                if ((Value != null) && ((Value is String) || ((Value is ValueType) && (Value is not IEnumerable) && (Value is not SyntaxToken))))
                {
                    var Text = Value.ToString();
                    if (!string.IsNullOrEmpty(Text))
                        AWriter.WriteAttributeString(Prop.Name, Text);
                }
            }
        }
    }
}
