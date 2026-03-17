using System.Text;
using Art.Common.IO;
using Art.Common.Management;

namespace Art.Common.Tests.IO;

public class CommittableFileStreamTests
{
    [Fact]
    public void ShouldCommit_TrueWithNewFile_FileKeptWithContents()
    {
        string tempDir = Path.GetTempPath();
        Assert.True(Directory.Exists(tempDir));
        string temp = ArtIOUtility.CreateRandomPath(tempDir, ".tmp");
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                Assert.True(File.Exists(temp));
                cfs.Write(data);
                cfs.ShouldCommit = true;
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
        string tempDir = Path.GetTempPath();
        Assert.True(Directory.Exists(tempDir));
        string temp = ArtIOUtility.CreateRandomPath(tempDir, ".tmp");
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true))
            {
                Assert.False(File.Exists(temp));
                cfs.Write(data);
                cfs.ShouldCommit = true;
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
        string tempDir = Path.GetTempPath();
        Assert.True(Directory.Exists(tempDir));
        string temp = ArtIOUtility.CreateRandomPath(tempDir, ".tmp");
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                Assert.True(File.Exists(temp));
                cfs.Write(data);
                //cfs.ShouldCommit = false;
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
        string tempDir = Path.GetTempPath();
        Assert.True(Directory.Exists(tempDir));
        string temp = ArtIOUtility.CreateRandomPath(tempDir, ".tmp");
        try
        {
            Assert.Equal(Path.GetFileName(temp), Path.GetRelativePath(tempDir, temp));
            string mess = $"ya like jazz? {Random.Shared.Next()}";
            byte[] data = Encoding.UTF8.GetBytes(mess);
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true))
            {
                Assert.False(File.Exists(temp));
                cfs.Write(data);
                //cfs.ShouldCommit = false;
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
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                cfs.Write(data1);
                cfs.ShouldCommit = true;
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
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: true))
            {
                cfs.Write(data1);
                cfs.ShouldCommit = true;
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
            using (CommittableFileStream cfs = new(temp, FileMode.Create, preferTemporaryLocation: false))
            {
                cfs.Write(data1);
                //cfs.ShouldCommit = false;
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
                //cfs.ShouldCommit = false;
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
