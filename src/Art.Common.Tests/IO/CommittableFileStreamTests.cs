using System.Text;
using Art.Common.IO;
using Art.Common.Management;

namespace Art.Common.Tests.IO;

public class CommittableFileStreamTests
{
    public static string CreateTempFile(out string parentDirectory)
    {
        string tmpParentDirectory = Path.Join(Path.GetTempPath(), "collectioneering_art_test_tempfile");
        Directory.CreateDirectory(tmpParentDirectory);
        string tmpFile = ArtIOUtility.CreateRandomPath(tmpParentDirectory, ".tmp");
        Assert.EndsWith(".tmp", tmpFile);
        parentDirectory = tmpParentDirectory;
        return tmpFile;
    }

    [Fact]
    public void ShouldCommit_TrueWithNewFile_FileKeptWithContents()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false) { Committable = committable };
                committable._stream = cfs;
                Assert.True(File.Exists(temp));
                cfs.Write(data);
                committable.ShouldCommit = true;
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_TrueWithNewFile_WithPreferTemporaryLocation_FileKeptWithContents()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true) { Committable = committable };
                committable._stream = cfs;
                Assert.False(File.Exists(temp));
                cfs.Write(data);
                committable.ShouldCommit = true;
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithNewFile_FileNotExist()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false) { Committable = committable };
                committable._stream = cfs;
                Assert.True(File.Exists(temp));
                cfs.Write(data);
            }
            Assert.False(File.Exists(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithNewFile_NoCommittable_FileNotExist()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                Assert.True(File.Exists(temp));
                cfs.Write(data);
            }
            Assert.False(File.Exists(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithNewFile_WithPreferTemporaryLocation_FileNotExist()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true) { Committable = committable };
                Assert.False(File.Exists(temp));
                cfs.Write(data);
            }
            Assert.False(File.Exists(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithNewFile_WithPreferTemporaryLocation_NoCommittable_FileNotExist()
    {
        string temp = CreateTempFile(out string tempDir);
        Assert.True(Directory.Exists(tempDir));
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true))
            {
                Assert.False(File.Exists(temp));
                cfs.Write(data);
            }
            Assert.False(File.Exists(temp));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_TrueWithExisting_NewFileKeptWithContents()
    {
        string temp = Path.GetTempFileName();
        try
        {
            string mess0 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data0 = Encoding.UTF8.GetBytes(mess0);
            File.WriteAllBytes(temp, data0);
            Assert.True(File.Exists(temp));
            string mess1 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data1 = Encoding.UTF8.GetBytes(mess1);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false) { Committable = committable };
                committable._stream = cfs;
                cfs.Write(data1);
                committable.ShouldCommit = true;
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data1));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_TrueWithExisting_WithPreferTemporaryLocation_NewFileKeptWithContents()
    {
        string temp = Path.GetTempFileName();
        try
        {
            string mess0 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data0 = Encoding.UTF8.GetBytes(mess0);
            File.WriteAllBytes(temp, data0);
            Assert.True(File.Exists(temp));
            string mess1 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data1 = Encoding.UTF8.GetBytes(mess1);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true) { Committable = committable };
                committable._stream = cfs;
                cfs.Write(data1);
                committable.ShouldCommit = true;
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data1));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithExisting_OldFileKeptWithContents()
    {
        string temp = Path.GetTempFileName();
        try
        {
            string mess0 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data0 = Encoding.UTF8.GetBytes(mess0);
            File.WriteAllBytes(temp, data0);
            Assert.True(File.Exists(temp));
            string mess1 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data1 = Encoding.UTF8.GetBytes(mess1);
            using (var committable = new StreamCommitManager())
            {
                CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false) { Committable = committable };
                committable._stream = cfs;
                cfs.Write(data1);
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data0));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithExisting_NoCommittable_OldFileKeptWithContents()
    {
        string temp = Path.GetTempFileName();
        try
        {
            string mess0 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data0 = Encoding.UTF8.GetBytes(mess0);
            File.WriteAllBytes(temp, data0);
            Assert.True(File.Exists(temp));
            string mess1 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data1 = Encoding.UTF8.GetBytes(mess1);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                cfs.Write(data1);
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data0));
        }
        finally
        {
            File.Delete(temp);
        }
    }

    [Fact]
    public void ShouldCommit_FalseWithExisting_WithPreferTemporaryLocation_OldFileKeptWithContents()
    {
        string temp = Path.GetTempFileName();
        try
        {
            string mess0 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data0 = Encoding.UTF8.GetBytes(mess0);
            File.WriteAllBytes(temp, data0);
            Assert.True(File.Exists(temp));
            string mess1 = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data1 = Encoding.UTF8.GetBytes(mess1);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true))
            {
                cfs.Write(data1);
            }
            Assert.True(File.Exists(temp));
            Assert.True(File.ReadAllBytes(temp).AsSpan().SequenceEqual(data0));
        }
        finally
        {
            File.Delete(temp);
        }
    }
}
