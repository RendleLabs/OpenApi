using System.CodeDom.Compiler;
using System.Text;

namespace RendleLabs.OpenApi.Generator.Tests;

internal class Writer
{
    public StringBuilder Builder { get; set; }
    public StringWriter TextWriter { get; set; }
    public IndentedTextWriter IndentedTextWriter { get; set; }

    public Writer()
    {
        Builder = new StringBuilder();
        TextWriter = new StringWriter(Builder);
        IndentedTextWriter = new IndentedTextWriter(TextWriter, "    ");
    }

    public override string ToString()
    {
        TextWriter.Flush();
        return Builder.ToString();
    }
}