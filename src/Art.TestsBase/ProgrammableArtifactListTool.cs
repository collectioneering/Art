using Art.Common;

namespace Art.TestsBase;

public class ProgrammableArtifactListTool : ArtifactTool, IArtifactListTool
{
    public delegate List<IArtifactData> SynchronousListDelegate(ProgrammableArtifactListTool tool);

    public readonly SynchronousListDelegate? SynchronousListFunc;

    public ProgrammableArtifactListTool(SynchronousListDelegate? synchronousListFunc)
    {
        SynchronousListFunc = synchronousListFunc;
    }

    public IAsyncEnumerable<IArtifactData> ListAsync(CancellationToken cancellationToken = default)
    {
        if (SynchronousListFunc == null)
        {
            throw new InvalidOperationException();
        }
        return SynchronousListFunc(this).ToAsyncEnumerable();
    }

    public static ArtifactToolRegistryEntry CreateRegistryEntry(SynchronousListDelegate synchronousListDelegate)
    {
        return CreateRegistryEntry(ArtifactToolIDUtil.CreateToolID<ProgrammableArtifactListTool>(), synchronousListDelegate);
    }

    public static ArtifactToolRegistryEntry CreateRegistryEntry(ArtifactToolID artifactToolId, SynchronousListDelegate synchronousListDelegate)
    {
        return new CustomArtifactToolRegistryEntry(artifactToolId, synchronousListDelegate);
    }

    private record CustomArtifactToolRegistryEntry(ArtifactToolID Id, SynchronousListDelegate Delegate) : ArtifactToolRegistryEntry(Id)
    {
        public override IArtifactTool CreateArtifactTool() => new ProgrammableArtifactListTool(Delegate);

        public override Type GetArtifactToolType() => typeof(ProgrammableArtifactListTool);
    }
}

public class AsyncProgrammableArtifactListTool : ArtifactTool, IArtifactListTool
{
    public delegate IAsyncEnumerable<IArtifactData> AsyncListDelegate(AsyncProgrammableArtifactListTool tool);

    public readonly AsyncListDelegate? AsyncListFunc;

    public AsyncProgrammableArtifactListTool(AsyncListDelegate? asyncListFunc)
    {
        AsyncListFunc = asyncListFunc;
    }

    public IAsyncEnumerable<IArtifactData> ListAsync(CancellationToken cancellationToken = default)
    {
        if (AsyncListFunc == null)
        {
            throw new InvalidOperationException();
        }
        return AsyncListFunc(this);
    }

    public static ArtifactToolRegistryEntry CreateRegistryEntry(AsyncListDelegate asyncListDelegate)
    {
        return CreateRegistryEntry(ArtifactToolIDUtil.CreateToolID<AsyncProgrammableArtifactListTool>(), asyncListDelegate);
    }

    public static ArtifactToolRegistryEntry CreateRegistryEntry(ArtifactToolID artifactToolId, AsyncListDelegate asyncListDelegate)
    {
        return new CustomArtifactToolRegistryEntry(artifactToolId, asyncListDelegate);
    }

    private record CustomArtifactToolRegistryEntry(ArtifactToolID Id, AsyncListDelegate Delegate) : ArtifactToolRegistryEntry(Id)
    {
        public override IArtifactTool CreateArtifactTool() => new AsyncProgrammableArtifactListTool(Delegate);

        public override Type GetArtifactToolType() => typeof(AsyncProgrammableArtifactListTool);
    }
}
