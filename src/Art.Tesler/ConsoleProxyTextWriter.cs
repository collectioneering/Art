﻿using System.CommandLine.IO;
using System.Text;

namespace Art.Tesler;

internal class ConsoleProxyTextWriter : TextWriter
{
    public IStandardStreamWriter StandardStreamWriter { get; }
    public override Encoding Encoding { get; }
    private readonly string _newLine;

    public ConsoleProxyTextWriter(IStandardStreamWriter standardStreamWriter, char[] newLine) : this(standardStreamWriter, newLine, Encoding.UTF8)
    {
    }

    public ConsoleProxyTextWriter(IStandardStreamWriter standardStreamWriter, string newLine) : this(standardStreamWriter, newLine, Encoding.UTF8)
    {
    }

    public ConsoleProxyTextWriter(IStandardStreamWriter standardStreamWriter, char[] newLine, Encoding encoding)
    {
        CoreNewLine = newLine ?? throw new ArgumentNullException(nameof(newLine));
        _newLine = new string(newLine);
        StandardStreamWriter = standardStreamWriter ?? throw new ArgumentNullException(nameof(standardStreamWriter));
        Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public ConsoleProxyTextWriter(IStandardStreamWriter standardStreamWriter, string newLine, Encoding encoding)
    {
        if (newLine == null)
        {
            throw new ArgumentNullException(nameof(newLine));
        }
        CoreNewLine = newLine.ToCharArray();
        _newLine = newLine;
        StandardStreamWriter = standardStreamWriter ?? throw new ArgumentNullException(nameof(standardStreamWriter));
        Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public override void WriteLine(string? value)
    {
        // this overload actually matters
        // used by StyledLogHandler
        StandardStreamWriter.Write(value);
        StandardStreamWriter.Write(_newLine);
    }

    public override void Write(char value)
    {
        // this overload actually matters
        // used as fallback
        StandardStreamWriter.Write(char.ToString(value));
    }

    public override void Write(string? value)
    {
        StandardStreamWriter.Write(value);
    }

    public override void Write(char[]? buffer)
    {
        if (buffer == null)
        {
            return;
        }
        Write(buffer, 0, buffer.Length);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        if (buffer.Length - index < count)
        {
            throw new ArgumentException("Invalid buffer bounds specified");
        }
        // surely this scales better than creating strings per char
        Write(new string(buffer.AsSpan(index, count)));
    }
}