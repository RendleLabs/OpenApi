// ReSharper disable once CheckNamespace
namespace System.CodeDom.Compiler;

public static class IndentedTextWriterExtensions
{
    public static IDisposable OpenIndent(this IndentedTextWriter writer) => new Indent(writer);
    public static IDisposable OpenBrace(this IndentedTextWriter writer) => new Brace(writer);

    private class Indent : IDisposable
    {
        private readonly IndentedTextWriter _writer;

        public Indent(IndentedTextWriter writer)
        {
            _writer = writer;
            _writer.Indent++;
        }

        public void Dispose() => _writer.Indent--;
    }
    
    private class Brace : IDisposable
    {
        private readonly IndentedTextWriter _writer;
        
        public Brace(IndentedTextWriter writer)
        {
            _writer = writer;
            writer.WriteLine("{");
            writer.Indent++;
        }

        public void Dispose()
        {
            _writer.Indent--;
            _writer.WriteLine("}");
        }
    }
}