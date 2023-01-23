using System.Text;
using SharpYaml.Serialization;

namespace RendleLabs.OpenApi.Bundle.Tests;

public class YamlTest
{
    [Fact]
    public void Temp()
    {
        var root = new YamlMappingNode();
        root.Children.Add(new YamlScalarNode("foo"), new YamlMappingNode(new KeyValuePair<YamlNode, YamlNode>(new YamlScalarNode("bar"), new YamlScalarNode("42"))));
        var document = new YamlDocument(root);
        var stream = new YamlStream(document);
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);
        stream.Save(writer, true);
        var s = builder.ToString();
        Assert.NotNull(s);
    }
}