using System.Xml;
using System.IO;

string path = @"D:\a\THUAI8\";
Visit(new DirectoryInfo(path));

void Visit(DirectoryInfo root)
{
    foreach (var file in root.EnumerateFiles())
    {
        if (file.Name == "debug_interface.csproj")
        {
            ChangeFileA(file.FullName);
        }
        else if (file.Name == "installer.csproj")
        {
            ChangeFileM(file.FullName);
        }
    }
    foreach (var dir in root.EnumerateDirectories())
    {
        Visit(dir);
    }
}

void ChangeFileA(string path)
{
    var document = new XmlDocument();
    document.Load(path);
    var projectNode = document.DocumentElement;
    if (projectNode != null)
    {
        var propertyGroup = document.CreateElement("PropertyGroup", projectNode.NamespaceURI);
        
        var optimize = document.CreateElement("Optimize", projectNode.NamespaceURI);
        optimize.InnerText = "true";
        propertyGroup.AppendChild(optimize);
        
        var serverGc = document.CreateElement("ServerGarbageCollection", projectNode.NamespaceURI);
        serverGc.InnerText = "true";
        propertyGroup.AppendChild(serverGc);
        
        var concurrentGc = document.CreateElement("ConcurrentGarbageCollection", projectNode.NamespaceURI);
        concurrentGc.InnerText = "true";
        propertyGroup.AppendChild(concurrentGc);
        
        var tieredCompilation = document.CreateElement("TieredCompilation", projectNode.NamespaceURI);
        tieredCompilation.InnerText = "true";
        propertyGroup.AppendChild(tieredCompilation);
        
        projectNode.AppendChild(propertyGroup);
    }
    
    document.Save(path);
}

void ChangeFileM(string path)
{
    var document = new XmlDocument();
    document.Load(path);
    var es = document.GetElementsByTagName("TargetFrameworks");
    if (es.Count == 2)
    {
        var i0 = es[0];
        var i1 = es[1];
        var text = i1.InnerText;
        i0.InnerText = text.Split(';')[1];
        i0.ParentNode.RemoveChild(i1);
    }
    document.Save(path);
}