using System.CommandLine;
using System.CommandLine.Invocation;

namespace Art.Tesler;

public interface ITeslerDataProvider
{
    void Initialize(Command command);

    IArtifactDataManager CreateArtifactDataManager(ParseResult parseResult);
}
