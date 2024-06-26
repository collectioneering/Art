﻿using System.CommandLine;

namespace Art.Tesler.Database;

public abstract class DatabaseCommandBase : CommandBase
{
    protected ITeslerRegistrationProvider RegistrationProvider;

    protected Option<string> ToolOption;

    protected Option<string> GroupOption;

    protected Option<string> IdOption;

    protected Option<string> ToolLikeOption;

    protected Option<string> GroupLikeOption;

    protected Option<string> IdLikeOption;

    protected Option<string> NameLikeOption;

    protected Option<bool> ListResourceOption;

    protected Option<bool> DetailedOption;

    protected DatabaseCommandBase(
        IOutputControl toolOutput,
        ITeslerRegistrationProvider registrationProvider,
        string name,
        string? description = null)
        : base(toolOutput, name, description)
    {
        RegistrationProvider = registrationProvider;
        RegistrationProvider.Initialize(this);
        ToolOption = new Option<string>(new[] { "-t", "--tool" }, "Tool to filter by") { ArgumentHelpName = "value" };
        AddOption(ToolOption);
        GroupOption = new Option<string>(new[] { "-g", "--group" }, "Group to filter by") { ArgumentHelpName = "value" };
        AddOption(GroupOption);
        IdOption = new Option<string>(new[] { "-i", "--id" }, "Id to filter by") { ArgumentHelpName = "value" };
        AddOption(IdOption);
        ToolLikeOption = new Option<string>(new[] { "--tool-like" }, "Tool pattern to filter by") { ArgumentHelpName = "pattern" };
        AddOption(ToolLikeOption);
        GroupLikeOption = new Option<string>(new[] { "--group-like" }, "Group pattern to filter by") { ArgumentHelpName = "pattern" };
        AddOption(GroupLikeOption);
        IdLikeOption = new Option<string>(new[] { "--id-like" }, "Id pattern to filter by") { ArgumentHelpName = "pattern" };
        AddOption(IdLikeOption);
        NameLikeOption = new Option<string>(new[] { "--name-like" }, "Name pattern to filter by") { ArgumentHelpName = "pattern" };
        AddOption(NameLikeOption);
        ListResourceOption = new Option<bool>(new[] { "-l", "--list-resource" }, "List resource items");
        AddOption(ListResourceOption);
        DetailedOption = new Option<bool>(new[] { "--detailed" }, "Show detailed information on entries");
        AddOption(DetailedOption);
    }
}
